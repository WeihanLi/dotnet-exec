// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using WeihanLi.Common.Models;

namespace Exec;

internal static class InternalHelper
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
        services.AddSingleton<SimpleCodeCompiler>();
        services.AddSingleton<AdhocWorkspaceCodeCompiler>();
        services.AddSingleton<AdvancedCodeCompiler>();
        services.AddSingleton<ICompilerFactory, CompilerFactory>();
        services.AddSingleton<DefaultCodeExecutor>();
        services.AddSingleton<IExecutorFactory, ExecutorFactory>();
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
        foreach (var diag in result.EmitResult.Diagnostics)
        {
            var message = CSharpDiagnosticFormatter.Instance.Format(diag);
            error.AppendLine($"{diag.Id}-{diag.Severity}-{message}");
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

    public static string GetGlobalUsingsCodeText(ExecOptions execOptions)
    {
        var usings = new HashSet<string>(GetGlobalUsings(execOptions.IncludeWideReferences));
        if (execOptions.Usings.HasValue())
        {
            foreach (var @using in execOptions.Usings)
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

        var usingText = usings.Select(x => $"global using {x};").StringJoin(Environment.NewLine);
        if (execOptions.LanguageVersion != LanguageVersion.Preview)
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

    private static string GetReferenceDirName(string frameworkName)
    {
        return frameworkName switch
        {
            FrameworkNames.Web => "Microsoft.AspNetCore.App.Ref",
            FrameworkNames.WindowsDesktop => "Microsoft.WindowsDesktop.App.Ref",
            _ => "Microsoft.NETCore.App.Ref"
        };
    }

    private static IEnumerable<string> GetDependencyFrameworks()
    {
        yield return FrameworkNames.Default;
        yield return FrameworkNames.Web;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            yield return FrameworkNames.WindowsDesktop;
        }
    }

    public static IEnumerable<string> ResolveReferences(ExecOptions options, bool compilation = true)
    {
        // TODO: handle execOptions.AdditionalReferences

        return (compilation ? ResolveCompileFrameworkReferences(options.TargetFramework, options.IncludeWideReferences)
                : ResolveRuntimeFrameworkReferences(options.TargetFramework, options.IncludeWideReferences))
            .SelectMany(_ => _)
            .Distinct()
            ;
    }

    private static IEnumerable<string[]> ResolveCompileFrameworkReferences(string targetFramework,
        bool includeAdditionalReferences = true)
    {
        foreach (var frameworkName in GetDependencyFrameworks())
        {
            yield return ResolveFrameworkReferencesViaPacksInternal(frameworkName, targetFramework);
        }

        if (includeAdditionalReferences)
        {
            yield return new[] { typeof(Guard).Assembly.Location, typeof(JsonConvert).Assembly.Location };
        }
    }

    private static IEnumerable<string[]> ResolveRuntimeFrameworkReferences(string targetFramework,
        bool includeAdditionalReferences = true)
    {
        foreach (var frameworkName in GetDependencyFrameworks())
        {
            yield return ResolveFrameworkReferencesViaSharedInternal(frameworkName, targetFramework);
        }

        if (includeAdditionalReferences)
        {
            yield return new[] { typeof(Guard).Assembly.Location, typeof(JsonConvert).Assembly.Location };
        }
    }

    private static string[] ResolveFrameworkReferencesViaPacksInternal(string frameworkName, string targetFramework)
    {
        var packsDir = Path.Combine(DotnetDirectory, "packs");
        var referencePackDirName = GetReferenceDirName(frameworkName);
        var frameworkDir = Path.Combine(packsDir, referencePackDirName);

        var versions = Directory.GetDirectories(frameworkDir).AsEnumerable();
        var versionPrefix = targetFramework["net".Length..];
        versions = versions.Where(x => Path.GetFileName(x).GetNotEmptyValueOrDefault(x).StartsWith(versionPrefix));
        var targetVersionDir = versions.OrderByDescending(x => x).First();
        var targetReferenceDir = Path.Combine(targetVersionDir, "ref", targetFramework);
        return Directory.GetFiles(targetReferenceDir, "*.dll");
    }

    private static string[] ResolveFrameworkReferencesViaSharedInternal(string frameworkName, string targetFramework)
    {
        var sharedDir = Path.Combine(DotnetDirectory, "shared");
        var frameworkDir = Path.Combine(sharedDir, frameworkName);

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
