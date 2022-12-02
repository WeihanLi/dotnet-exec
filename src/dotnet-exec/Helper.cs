// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using NuGet.Versioning;
using ReferenceResolver;
using System.Reflection;
using System.Text;
using System.Text.Json;
using WeihanLi.Common.Models;

namespace Exec;

public static class Helper
{
    private static readonly HashSet<string> SpecialConsoleDiagnosticIds = new()
    {
        // Program does not contain a static 'Main' method suitable for an entry point
        // https://learn.microsoft.com/en-us/dotnet/csharp/misc/cs5001
        "CS5001", 
        // The method declaration for Main was invalid
        // https://learn.microsoft.com/en-us/dotnet/csharp/misc/CS0028
        "CS0028"
    };

    public const string ApplicationName = "dotnet-exec";

    public const string Default = "default";

    public const string Script = "script";

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
        services.AddSingleton<WorkspaceCodeCompiler>();
        services.AddSingleton<CSharpScriptCompilerExecutor>();
        services.AddSingleton<ICompilerFactory, CompilerFactory>();
        services.AddSingleton<DefaultCodeExecutor>();
        services.AddSingleton<IExecutorFactory, ExecutorFactory>();
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<ICommandHandler>(sp => sp.GetRequiredService<CommandHandler>());
        services.AddSingleton<IUriTransformer, UriTransformer>();
        services.AddSingleton<IScriptContentFetcher, ScriptContentFetcher>();
        services.AddSingleton<IAdditionalScriptContentFetcher, AdditionalScriptContentFetcher>();
        services.AddSingleton<HttpClient>();
        services.AddReferenceResolvers();
        services.AddSingleton<IRefResolver, RefResolver>();
        services.AddSingleton<IConfigProfileManager, ConfigProfileManager>();

        return services;
    }

    public static void Initialize(this Command command, IServiceProvider serviceProvider)
    {
        command.Handler = serviceProvider.GetRequiredService<ICommandHandler>();
        var profileManager = serviceProvider.GetRequiredService<IConfigProfileManager>();
        foreach (var subcommand in command.Subcommands)
        {
            if (subcommand is ConfigProfileCommand configProfileCommand)
            {
                foreach (var configSubcommand in configProfileCommand.Subcommands)
                {
                    Func<InvocationContext, Task> commandHandler = configSubcommand.Name switch
                    {
                        "set" => async context =>
                        {
                            var profileName = context.ParseResult.GetValueForArgument(ConfigProfileCommand.ProfileNameArgument);
                            if (string.IsNullOrEmpty(profileName))
                                return;

                            var profile = new ConfigProfile()
                            {
                                Usings = new HashSet<string>(context.ParseResult.GetValueForOption(ExecOptions.UsingsOption) ?? Enumerable.Empty<string>()),
                                References = new HashSet<string>(context.ParseResult.GetValueForOption(ExecOptions.ReferencesOption) ?? Enumerable.Empty<string>()),
                                IncludeWideReferences = context.ParseResult.GetValueForOption(ExecOptions.WideReferencesOption),
                                IncludeWebReferences = context.ParseResult.HasOption(ExecOptions.WebReferencesOption),
                                EntryPoint = context.ParseResult.GetValueForOption(ExecOptions.EntryPointOption),
                                EnablePreviewFeatures = context.ParseResult.HasOption(ExecOptions.PreviewOption)
                            };
                            await profileManager.ConfigureProfile(profileName, profile);
                        }
                        ,
                        "rm" => async context =>
                        {
                            var profileName = context.ParseResult.GetValueForArgument(ConfigProfileCommand.ProfileNameArgument);
                            if (string.IsNullOrEmpty(profileName))
                            {
                                return;
                            }
                            await profileManager.DeleteProfile(profileName);
                        }
                        ,
                        "ls" => async context =>
                        {
                            var profiles = await profileManager.ListProfiles();
                            if (profiles.IsNullOrEmpty())
                            {
                                context.Console.WriteLine("No profiles found");
                                return;
                            }
                            context.Console.WriteLine("Profiles:");
                            foreach (var profile in profiles)
                            {
                                context.Console.WriteLine($"- {profile}");
                            }
                        }
                        ,
                        _ => async context =>
                        {
                            var profileName = context.ParseResult.GetValueForArgument(ConfigProfileCommand.ProfileNameArgument);
                            if (string.IsNullOrEmpty(profileName))
                            {
                                return;
                            }

                            var profile = await profileManager.GetProfile(profileName);
                            if (profile is null)
                            {
                                context.Console.WriteLine($"The profile [{profileName}] does not exists");
                                return;
                            }

                            var output = JsonSerializer.Serialize(profile, new JsonSerializerOptions() { WriteIndented = true });
                            context.Console.WriteLine(output);
                        }
                    };
                    configSubcommand.SetHandler(commandHandler);
                }
            }
        }
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
    private static IEnumerable<string[]> GetGlobalUsingsInternal(ExecOptions options)
    {
        // Default SDK
        yield return FrameworkReferenceResolver.GetImplicitUsings(FrameworkReferenceResolver.FrameworkNames.Default);
        if (options.IncludeWebReferences)
        {
            // Web
            yield return FrameworkReferenceResolver.GetImplicitUsings(FrameworkReferenceResolver.FrameworkNames.Web);
        }

        if (options.IncludeWideReferences)
        {
            yield return new[]
            {
                "Microsoft.Extensions.Configuration",
                "Microsoft.Extensions.DependencyInjection",
                "Microsoft.Extensions.Logging",

                "WeihanLi.Common",
                "WeihanLi.Common.Helpers",
                "WeihanLi.Extensions",
                "WeihanLi.Extensions.Dump"
            };
        }

        const string frameworkPrefix = "framework:";
        foreach (var reference in options.References.Where(x => x.StartsWith(frameworkPrefix)))
        {
            var frameworkName = reference[frameworkPrefix.Length..].Trim();
            yield return FrameworkReferenceResolver.GetImplicitUsings(frameworkName);
        }
    }

    private static ICollection<string> GetGlobalUsings(ExecOptions options)
    {
        var usings = new HashSet<string>(GetGlobalUsingsInternal(options).SelectMany(_ => _));
        if (!options.Usings.HasValue()) return usings;

        foreach (var @using in options.Usings.Where(_ => !_.StartsWith('-')))
        {
            usings.Add(@using);
        }
        foreach (var @using in options.Usings.Where(_ => _.StartsWith('-')))
        {
            usings.Remove(@using);
        }
        return usings;
    }

    public static string GetGlobalUsingsCodeText(ExecOptions options)
    {
        var usings = GetGlobalUsings(options);
        if (usings.IsNullOrEmpty()) return string.Empty;

        var usingText = usings.Select(x => $"global using {x};").StringJoin(Environment.NewLine);
        return options.EnablePreviewFeatures ?
            // Generate System.Runtime.Versioning.RequiresPreviewFeatures attribute on assembly level
            $"{usingText}{Environment.NewLine}[assembly:System.Runtime.Versioning.RequiresPreviewFeatures]"
            : usingText
            ;
    }

    private static void LoadSupportedFrameworks()
    {
        var frameworkDir = Path.Combine(FrameworkReferenceResolver.DotnetDirectory, "shared", FrameworkReferenceResolver.FrameworkNames.Default);
        foreach (var framework in Directory
                     .GetDirectories(frameworkDir)
                     .Select(Path.GetFileName)
                     .WhereNotNull()
                     .Where(x => x.Length > 0 && char.IsDigit(x[0]))
                 )
        {
            if (NuGetVersion.TryParse(framework, out var frameworkVersion)
                && frameworkVersion.Major >= 6)
            {
                _supportedFrameworks.Add($"net{frameworkVersion.Major}.{frameworkVersion.Minor}");
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    private static readonly HashSet<string> _supportedFrameworks = new();
    public static HashSet<string> SupportedFrameworks
    {
        get
        {
            if (_supportedFrameworks.Count == 0)
            {
                LoadSupportedFrameworks();
            }
            return _supportedFrameworks;
        }
    }

    public static IEnumerable<string> GetDependencyFrameworks(ExecOptions options)
    {
        yield return FrameworkReferenceResolver.FrameworkNames.Default;
        if (options.IncludeWebReferences)
        {
            yield return FrameworkReferenceResolver.FrameworkNames.Web;
        }
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

