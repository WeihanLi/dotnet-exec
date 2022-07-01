// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using WeihanLi.Common.Models;

namespace Exec;

public static class Helper
{
    private static readonly HashSet<string> SpecialConsoleDiagnosticIds = new() { "CS5001", "CS0028" };

    public const string ApplicationName = "dotnet-exec";

    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services, string[] args)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(args.Contains("--debug") ? LogLevel.Debug : LogLevel.Error);
        });
        services.AddSingleton(sp => sp.GetRequiredService<ILoggerFactory>()
            .CreateLogger(ApplicationName));
        services.AddSingleton<DefaultCodeCompiler>();
        services.AddSingleton<AdhocWorkspaceCodeCompiler>();
        services.AddSingleton<AdvancedCodeCompiler>();
        services.AddSingleton<CSharpScriptCompilerExecutor>();
        services.AddSingleton<ICompilerFactory, CompilerFactory>();
        services.AddSingleton<DefaultCodeExecutor>();
        services.AddSingleton<IExecutorFactory, ExecutorFactory>();
        services.AddSingleton<INuGetHelper, NuGetHelper>();
        services.AddSingleton<IReferenceResolver, ReferenceResolver>();
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<ICommandHandler>(sp => sp.GetRequiredService<CommandHandler>());
        services.AddSingleton<IScriptContentFetcher, ScriptContentFetcher>();
        services.AddHttpClient(nameof(ScriptContentFetcher));

        return services;
    }

    public static async Task<Result<CompileResult>> GetCompilationAssemblyResult(this Compilation compilation,
        CancellationToken cancellationToken = default)
    {
        var result = await GetCompilationResult(compilation, cancellationToken);
        if (result.EmitResult.Success)
        {
            Guard.NotNull(result.Assembly).Seek(0, SeekOrigin.Begin);
            return Result.Success(new CompileResult(result.Compilation, result.EmitResult,
                result.Assembly));
        }

        var error = new StringBuilder();
        foreach (var diagnostic in result.EmitResult.Diagnostics)
        {
            var message = CSharpDiagnosticFormatter.Instance.Format(diagnostic);
            error.AppendLine($"{diagnostic.Id}-{diagnostic.Severity}-{message}");
        }

        return Result.Fail<CompileResult>(error.ToString(), ResultStatus.ProcessFail);
    }

    private static async Task<(Compilation Compilation, EmitResult EmitResult, MemoryStream? Assembly)>
        GetCompilationResult(Compilation compilation, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms, cancellationToken: cancellationToken);
        if (emitResult.Success)
        {
            return (compilation, emitResult, ms);
        }

        if (emitResult.Diagnostics.Any(d => SpecialConsoleDiagnosticIds.Contains(d.Id)))
        {
            ms.Seek(0, SeekOrigin.Begin);
            ms.SetLength(0);

            var options = compilation.Options.WithOutputKind(OutputKind.DynamicallyLinkedLibrary);
            emitResult = compilation.WithOptions(options)
                .Emit(ms, cancellationToken: cancellationToken);
            return (compilation, emitResult, emitResult.Success ? ms : null);
        }

        return (compilation, emitResult, null);
    }

    // https://docs.microsoft.com/en-us/dotnet/core/project-sdk/overview#implicit-using-directives
    private static IEnumerable<string> GetGlobalUsings(bool includeAdditional)
    {
        // Default SDK
        yield return "System";
        yield return "System.Collections.Generic";
        yield return "System.IO";
        yield return "System.Linq";
        yield return "System.Net.Http";
        yield return "System.Text";
        yield return "System.Threading";
        yield return "System.Threading.Tasks";

        // Web
        yield return "System.Net.Http.Json";
        yield return "Microsoft.AspNetCore.Builder";
        yield return "Microsoft.AspNetCore.Hosting";
        yield return "Microsoft.AspNetCore.Http";
        yield return "Microsoft.AspNetCore.Routing";
        yield return "Microsoft.Extensions.Configuration";
        yield return "Microsoft.Extensions.DependencyInjection";
        yield return "Microsoft.Extensions.Hosting";
        yield return "Microsoft.Extensions.Logging";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Include Windows Desktop SDK for Windows only
            yield return "System.Windows.Forms";
        }

        if (includeAdditional)
        {
            yield return "WeihanLi.Common";
            yield return "WeihanLi.Common.Logging";
            yield return "WeihanLi.Common.Helpers";
            yield return "WeihanLi.Extensions";
            yield return "WeihanLi.Extensions.Dump";
        }
    }

    public static HashSet<string> GetGlobalUsings(ExecOptions options)
    {
        var usings = new HashSet<string>(GetGlobalUsings(options.IncludeWideReferences));
        if (options.Usings.HasValue())
        {
            foreach (var @using in options.Usings)
            {
                if (@using.StartsWith('-'))
                {
                    usings.Remove(@using[1..]);
                }
                else
                {
                    usings.Add(@using);
                }
            }
        }
        return usings;
    }

    public static string GetGlobalUsingsCodeText(ExecOptions options)
    {
        var usings = GetGlobalUsings(options);

        var usingText = usings.Select(x => $"global using {x};").StringJoin(Environment.NewLine);
        if (options.LanguageVersion != LanguageVersion.Preview)
            return usingText;
        // Generate System.Runtime.Versioning.RequiresPreviewFeatures attribute on assembly level
        return $"{usingText}{Environment.NewLine}[assembly:System.Runtime.Versioning.RequiresPreviewFeatures]";
    }

    public static string GetDotnetPath()
    {
        var commandNameWithExtension =
            $"dotnet{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty)}";
        var searchPaths = Guard.NotNull(Environment.GetEnvironmentVariable("PATH"))
            .Split(new[] { Path.PathSeparator }, options: StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim('"'))
            .ToArray();
        var commandPath = searchPaths
            .Where(p => !Path.GetInvalidPathChars().Any(p.Contains))
            .Select(p => Path.Combine(p, commandNameWithExtension))
            .First(File.Exists);
        return commandPath;
    }

    private static string GetDotnetDirectory()
    {
        var environmentOverride = Environment.GetEnvironmentVariable("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR");
        if (!string.IsNullOrEmpty(environmentOverride))
        {
            return environmentOverride;
        }

        var dotnetExe = GetDotnetPath();

        if (dotnetExe.IsNotNullOrEmpty() && !Interop.RunningOnWindows)
        {
            // e.g. on Linux the 'dotnet' command from PATH is a symlink so we need to
            // resolve it to get the actual path to the binary
            dotnetExe = Interop.Unix.RealPath(dotnetExe) ?? dotnetExe;
        }

        if (string.IsNullOrWhiteSpace(dotnetExe))
        {
            dotnetExe = Environment.ProcessPath;
        }

        return Guard.NotNull(Path.GetDirectoryName(dotnetExe));
    }

    private static string _dotnetDirectory = string.Empty;

    private static string DotnetDirectory
    {
        get
        {
            if (!string.IsNullOrEmpty(_dotnetDirectory))
            {
                return _dotnetDirectory;
            }

            _dotnetDirectory = GetDotnetDirectory();
            return _dotnetDirectory;
        }
    }

    public static string GetReferencePackageName(string frameworkName)
    {
        return frameworkName switch
        {
            FrameworkNames.Web => FrameworkReferencePackages.Web,
            FrameworkNames.WindowsDesktop => FrameworkReferencePackages.WindowsDesktop,
            _ => FrameworkReferencePackages.Default
        };
    }

    public static IEnumerable<string> GetDependencyFrameworks(ExecOptions options)
    {
        yield return FrameworkNames.Default;
        yield return FrameworkNames.Web;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            yield return FrameworkNames.WindowsDesktop;
        }
    }

    public static string[] ResolveFrameworkReferencesViaSdkPacks(string frameworkName, string targetFramework)
    {
        var packsDir = Path.Combine(DotnetDirectory, "packs");
        var referencePackageName = GetReferencePackageName(frameworkName);
        var frameworkDir = Path.Combine(packsDir, referencePackageName);
        if (Directory.Exists(frameworkDir))
        {
            var versions = Directory.GetDirectories(frameworkDir).AsEnumerable();
            var versionPrefix = targetFramework["net".Length..];
            versions = versions.Where(x => Path.GetFileName(x).GetNotEmptyValueOrDefault(x).StartsWith(versionPrefix));
            var targetVersionDir = versions.OrderByDescending(x => x).First();
            var targetReferenceDir = Path.Combine(targetVersionDir, "ref", targetFramework);
            return Directory.GetFiles(targetReferenceDir, "*.dll");
        }
        return Array.Empty<string>();
    }

    public static string[] ResolveFrameworkReferencesViaRuntimeShared(string frameworkName, string targetFramework)
    {
        var sharedDir = Path.Combine(DotnetDirectory, "shared");
        var frameworkDir = Path.Combine(sharedDir, frameworkName);
        if (!Directory.Exists(frameworkDir))
        {
            frameworkDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        }
        Guard.NotNull(frameworkDir);
        var versions = Directory.GetDirectories(frameworkDir).AsEnumerable();
        var versionPrefix = targetFramework["net".Length..];
        versions = versions.Where(x => Path.GetFileName(x).GetNotEmptyValueOrDefault(x).StartsWith(versionPrefix));
        var targetVersionDir = versions.OrderByDescending(x => x).First();
        return Directory.GetFiles(targetVersionDir, "*.dll");
    }

    public static void EnableReferencesSupersedeLowerVersions(this CompilationOptions compilationOptions)
    {
        // https://github.com/dotnet/roslyn/blob/a51b65c86bb0f42a79c47798c10ad75d5c343f92/src/Compilers/Core/Portable/Compilation/CompilationOptions.cs#L183
        typeof(CompilationOptions)
            .GetProperty("ReferencesSupersedeLowerVersions", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetMethod!
            .Invoke(compilationOptions, new object[] { true });
    }
}

internal static class FrameworkNames
{
    public const string Default = "Microsoft.NETCore.App";

    public const string Web = "Microsoft.AspNetCore.App";

    public const string WindowsDesktop = "Microsoft.WindowsDesktop.App";
}

internal static class FrameworkReferencePackages
{
    public const string Default = "Microsoft.NETCore.App.Ref";
    public const string Web = "Microsoft.AspNetCore.App.Ref";
    public const string WindowsDesktop = "Microsoft.WindowsDesktop.App.Ref";
}
