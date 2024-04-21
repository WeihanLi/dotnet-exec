// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Commands;
using Exec.Contracts;
using Exec.Services;
using Exec.Services.Middlewares;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NuGet.Versioning;
using ReferenceResolver;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using WeihanLi.Common.Models;

namespace Exec;

public static class Helper
{
    private static readonly ImmutableHashSet<string> SpecialConsoleDiagnosticIds = new[]
    {
        // Program does not contain a static 'Main' method suitable for an entry point
        // https://learn.microsoft.com/en-us/dotnet/csharp/misc/cs5001
        "CS5001", 
        // The method declaration for Main was invalid
        // https://learn.microsoft.com/en-us/dotnet/csharp/misc/CS0028
        "CS0028"
    }.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

    public const string ApplicationName = "dotnet-exec";
    public const string Default = "default";
    public const string Script = "script";

    private const string EnableDebugEnvName = "DOTNET_EXEC_DEBUG_ENABLED";
    public const string EnableWebReferenceEnvName = "DOTNET_EXEC_WEB_REF_ENABLED";
    public static bool DebugModelEnabled(string[] args)
    {
        if (args.Contains("--debug"))
            return true;

        return EnvHelper.Val(EnableDebugEnvName).ToBoolean();
    }

    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services, string[] args)
    {
        var isDebugMode = DebugModelEnabled(args);
        if (isDebugMode && !Debugger.IsAttached && args.Contains("--attach")) Debugger.Launch();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(isDebugMode ? LogLevel.Debug : LogLevel.Error);
        });
        services.AddSingleton(sp => sp.GetRequiredService<ILoggerFactory>()
            .CreateLogger(ApplicationName));
        services.AddSingleton<SimpleCodeCompiler>();
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

        services.RegisterOptionsConfigureMiddleware<ProjectFileOptionsConfigureMiddleware>()
            .RegisterOptionsConfigureMiddleware<CleanupOptionsConfigureMiddleware>()
            ;

        services.RegisterParseOptionsMiddleware<PreprocessorSymbolNamesParserOptionsMiddleware>()
            .RegisterParseOptionsMiddleware<FeaturesParserOptionsMiddleware>()
            ;
        // register options configure pipeline
        services.AddSingleton<IOptionsConfigurePipeline, OptionsConfigurePipeline>();
        // register parse options configure pipeline
        services.AddSingleton<IParseOptionsPipeline, ParseOptionsPipeline>();
        // register compilation options configure pipeline
        services.AddSingleton<ICompilationOptionsPipeline, CompilationOptionsPipeline>();
        return services;
    }

    public static string[] CommandArguments { get; set; } = [];

    private static IServiceCollection RegisterOptionsConfigureMiddleware<TMiddleware>
        (this IServiceCollection services) where TMiddleware : class, IOptionsConfigureMiddleware
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsConfigureMiddleware, TMiddleware>());
        return services;
    }

    private static IServiceCollection RegisterParseOptionsMiddleware<TMiddleware>
        (this IServiceCollection services) where TMiddleware : class, IParseOptionsMiddleware
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IParseOptionsMiddleware, TMiddleware>());
        return services;
    }

    public static void Initialize(this Command command, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(command);
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
                            await profileManager.ConfigureProfile(profileName, profile).ConfigureAwait(false);
                        }
                        ,
                        "rm" => async context =>
                        {
                            var profileName = context.ParseResult.GetValueForArgument(ConfigProfileCommand.ProfileNameArgument);
                            if (string.IsNullOrEmpty(profileName))
                            {
                                return;
                            }
                            await profileManager.DeleteProfile(profileName).ConfigureAwait(false);
                        }
                        ,
                        "ls" => async context =>
                        {
                            var profiles = await profileManager.ListProfiles().ConfigureAwait(false);
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

                            var profile = await profileManager.GetProfile(profileName).ConfigureAwait(false);
                            if (profile is null)
                            {
                                context.Console.WriteLine($"The profile [{profileName}] does not exists");
                                return;
                            }

                            var output = JsonSerializer.Serialize(profile, JsonHelper.WriteIntendedUnsafeEncoderOptions);
                            context.Console.WriteLine(output);
                        }
                    };
                    configSubcommand.SetHandler(commandHandler);
                }
            }
        }
    }

    internal static async Task<Result<CompileResult>> GetCompilationAssemblyResult(this Compilation compilation,
        CancellationToken cancellationToken = default)
    {
        var result = await GetCompilationResult(compilation, cancellationToken).ConfigureAwait(false);
        if (result.EmitResult.Success)
        {
            Guard.NotNull(result.Assembly).Seek(0, SeekOrigin.Begin);
            return Result.Success(new CompileResult(result.Compilation, result.EmitResult,
                result.Assembly));
        }

        var error = new StringBuilder();
        foreach (var diagnostic in result.EmitResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error))
        {
#pragma warning disable CA1305
            var message = CSharpDiagnosticFormatter.Instance.Format(diagnostic);
#pragma warning restore CA1305
            error.AppendLine(message);
        }

        return Result.Fail<CompileResult>(error.ToString(), ResultStatus.ProcessFail);
    }

    private static async Task<(Compilation Compilation, EmitResult EmitResult, MemoryStream? Assembly)>
        GetCompilationResult(Compilation compilation, CancellationToken cancellationToken = default)
    {
        var consoleStream = new MemoryStream();
        var emitResult = compilation.Emit(consoleStream, cancellationToken: cancellationToken);
        if (emitResult.Success)
        {
            return (compilation, emitResult, consoleStream);
        }
        await consoleStream.DisposeAsync().ConfigureAwait(false);
        if (!emitResult.Diagnostics.Any(d => SpecialConsoleDiagnosticIds.Contains(d.Id)))
            return (compilation, emitResult, null);

        var dllStream = new MemoryStream();
        var options = compilation.Options.WithOutputKind(OutputKind.DynamicallyLinkedLibrary);
        emitResult = compilation.WithOptions(options)
            .Emit(dllStream, cancellationToken: cancellationToken);
        return (compilation, emitResult, emitResult.Success ? dllStream : null);
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
            yield return
            [
                "global::Microsoft.Extensions.Configuration",
                "global::Microsoft.Extensions.DependencyInjection",
                "global::Microsoft.Extensions.Logging",

                "global::WeihanLi.Common",
                "global::WeihanLi.Common.Helpers",
                "global::WeihanLi.Extensions",
                "global::WeihanLi.Extensions.Dump"
            ];
        }

        const string frameworkPrefix = "framework:";
        foreach (var reference in options.References.Where(x => x.StartsWith(frameworkPrefix, StringComparison.Ordinal)))
        {
            var frameworkName = reference[frameworkPrefix.Length..].Trim();
            yield return FrameworkReferenceResolver.GetImplicitUsings(frameworkName);
        }
    }

    private static HashSet<string> GetGlobalUsings(ExecOptions options)
    {
        var usings = new HashSet<string>(GetGlobalUsingsInternal(options).Flatten());
        if (options.Usings.IsNullOrEmpty()) return usings;

        foreach (var @using in options.Usings.Where(u => !u.StartsWith('-')))
        {
            usings.Add(@using);
        }
        foreach (var @using in options.Usings.Where(u => u.StartsWith('-')))
        {
            var usingToRemove = @using[1..].Trim();
            usings.Remove(usingToRemove);
            usings.Remove(@using);
            if (!usingToRemove.StartsWith("global::", StringComparison.OrdinalIgnoreCase))
            {
                usings.Remove($"global::{usingToRemove}");
            }
        }
        return usings;
    }

    public static string GetGlobalUsingsCodeText(ExecOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var usings = GetGlobalUsings(options);
        var usingText = UsingManager.GetGlobalUsingsCodeText(usings);

        // global usings have to be placed before anything else
        // Generate System.Runtime.Versioning.RequiresPreviewFeatures attribute on assembly level if needed
        if (options.EnablePreviewFeatures)
            usingText = $"{usingText}{Environment.NewLine}[assembly:System.Runtime.Versioning.RequiresPreviewFeatures]";

        return usingText;
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
    private static readonly HashSet<string> _supportedFrameworks = [];
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
            .Invoke(compilationOptions, [true]);
    }

    public static string ReferenceNormalize(string reference)
    {
        IReference typedReference;
        if (reference.StartsWith('-'))
        {
            typedReference = ReferenceResolverFactory.ParseReference(reference[1..]);
            return $"- {typedReference.ReferenceWithSchema}";
        }

        typedReference = ReferenceResolverFactory.ParseReference(reference);
        return typedReference.ReferenceWithSchema;
    }
}

