// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Commands;
using Exec.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReferenceResolver;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Exec;

[ExcludeFromCodeCoverage]
public sealed partial class ExecOptions
{
    public const string DefaultTargetFramework =
#if NET10_0_OR_GREATER
      "net10.0"
#elif NET9_0
      "net9.0"
#elif NET8_0
      "net8.0"
#endif
        ;

    private static readonly Argument<string[]> ScriptArgument = new("script")
    {
        Description = "CSharp script to execute, start repl when no script provided",
        Arity = ArgumentArity.ZeroOrMore
    };
    private static readonly Option<string> TargetFrameworkOption = new("-f", "--framework")
    {
        Description = "Target framework",
        DefaultValueFactory = _ => DefaultTargetFramework
    };

    private static readonly Option<string> StartupTypeOption = new("--startup-type")
    {
        Description = "The startup type to use for finding the correct entry"
    };
    internal static readonly Option<string> EntryPointOption = new("-e", "--entry")
    {
        Description = "Custom entry point('MainTest' by default)"
    };

    internal static readonly Option<string[]> DefaultEntryMethodsOption = new("--default-entry")
    {
        Description = "Default entry method names",
        Arity = ArgumentArity.ZeroOrMore
    };

    private static readonly Option<string> CompilerTypeOption =
        new("--compiler-type", "--compiler")
        {
            Description = "The compiler to use",
            DefaultValueFactory = _ => "workspace"
        };

    private static readonly Option<string> ExecutorTypeOption =
        new("--executor-type", "--executor")
        {
            DefaultValueFactory = _ => Helper.Default, 
            Description = "The executor to use"
        };

    internal static readonly Option<bool> PreviewOption =
        new("--preview")
        {
            Description = "Use preview language feature and enable preview features"
        };

    private static readonly Option<OptimizationLevel> ConfigurationOption =
        new("-c", "--configuration")
        {
            Description = "Compile Configuration/OptimizationLevel"
        };

    private static readonly Option<string?> ArgumentsOption =
        new("--args", "--arguments")
        {
            Description = "Input arguments, this is obsolete, please use ` -- <args[0]> <args[1]>` instead",
            Arity = ArgumentArity.ZeroOrOne
        };

    private static readonly Option<bool> UseRefAssembliesForCompileOption = new("--ref-compile")
    {
        Description = "Use Ref assemblies for compile, when not found from local download from nuget"
    };

    private static readonly Option<string> ProjectOption =
        new("--project")
        {
            Description = "The project file path to exact references and usings"
        };

    internal static readonly Option<bool> WideReferencesOption =
        new("--wide")
        {
            Description = "Includes widely-used references(Microsoft.Extensions.Configuration/DependencyInjection/Logging,Newtonsoft.Json,WeihanLi.Common)",
            DefaultValueFactory = _ => true
        };

    internal static readonly Option<bool> WebReferencesOption =
        new("-w", "--web")
        {
            Description = "Includes the ASP.NET Core Web SDK references"
        };

    internal static readonly Option<string[]> ReferencesOption =
        new("-r", "--reference")
        {
            Arity = ArgumentArity.ZeroOrMore,
            Description = "Additional references"
        };

    internal static readonly Option<string[]> UsingsOption =
        new("-u", "--using")
        {
            Description = "Configure the global namespace usings",
            Arity = ArgumentArity.ZeroOrMore
        };

    private static readonly Option<string[]> AdditionalScriptsOption =
        new("--ad", "--addition")
        { 
            Description = "Additional script path"
        };

    private static readonly Option<bool> EnableSourceGeneratorOption =
        new("--generator")
        {
            Description = "Enable the source generator support"
        };

    internal static readonly Option<string> ConfigProfileOption =
        new("--profile")
        {
            Description = "The config profile to use"
        };

    private static readonly Option<string[]> ParserSymbolNamesOption =
        new("--compile-symbol")
        {
            Description = "Preprocessor symbol names for parsing and compiling"
        };

    private static readonly Option<string[]> ParserFeaturesOption =
        new("--compile-feature")
        {
            Description = "Features for parsing and compiling"
        };

    private static readonly Option<bool> DryRunOption = new("--dry-run")
    {
        Description = "Dry-run, would not execute script and output debug info"
    };
    private static readonly Option<string> NuGetConfigFileOption = new("--nuget-config")
    {
        Description = "NuGet config file path to use"
    };
    private static readonly Option<string[]> EnvOption =
        new("--env")
        {
            Description = "Set environment variable for process, usage example: --env name=test --env value=123"
        };

    private static readonly Option<string?> CompileOutputOption = new("--compile-out")
    {
        Description = "Compiled dll output path"
    };

    internal static readonly Option<bool> DebugOption = new("--debug")
    {
        Description = "Enable debug logs for debug"
    };
#pragma warning disable IDE0052
    private static readonly Option<bool> InfoOption = new("--info")
    {
        Description = "Inspect tool version and runtime info"
    };
#pragma warning restore IDE0052
    private static readonly Option<double?> TimeoutOption = new("--timeout")
    {
        Description = "Timeout in seconds for the execution"
    };

    static ExecOptions()
    {
        CompilerTypeOption.CompletionSources.Add("simple", "workspace", Helper.Project);
        ExecutorTypeOption.CompletionSources.Add(Helper.Default);
        TargetFrameworkOption.CompletionSources.Add([.. Helper.SupportedFrameworks]);
    }

    public void BindCommandLineArguments(ParseResult parseResult, ConfigProfile? configProfile)
    {
        var scripts = parseResult.GetValue(ScriptArgument) ?? [];
        Script = scripts.FirstOrDefault() ?? string.Empty;
        StartupType = parseResult.GetValue(StartupTypeOption);
        EntryPoint = parseResult.GetValue(EntryPointOption);
        TargetFramework = parseResult.GetValue(TargetFrameworkOption)
            .GetValueOrDefault(DefaultTargetFramework);
        Configuration = parseResult.GetValue(ConfigurationOption);

        Arguments = Helper.CommandArguments.HasValue()
            ? Helper.CommandArguments
            : [.. WeihanLi.Common.Helpers.CommandLineParser.ParseLine(parseResult.GetValue(ArgumentsOption) ?? string.Empty)];

        ProjectPath = parseResult.GetValue(ProjectOption) ?? string.Empty;
        IncludeWideReferences = parseResult.GetValue(WideReferencesOption);
        IncludeWebReferences = parseResult.GetValue(WebReferencesOption) || EnvHelper.Val(Helper.EnableWebReferenceEnvName).ToBoolean();
        var executorTypeValue = parseResult.GetValue(ExecutorTypeOption);
        if (!string.IsNullOrEmpty(executorTypeValue))
        {
            ExecutorType = executorTypeValue.ToLowerInvariant();
        }
        CompilerType = parseResult.GetValue(CompilerTypeOption)?.ToLowerInvariant() ?? Helper.Default;
        foreach (var reference in parseResult.GetValue(ReferencesOption) ?? [])
        {
            References.Add(Helper.ReferenceNormalize(reference));
        }
        Usings = [.. parseResult.GetValue(UsingsOption) ?? []];
        AdditionalScripts = new(scripts.Skip(1).Union(parseResult.GetValue(AdditionalScriptsOption) ?? []), StringComparer.Ordinal);
        UseRefAssembliesForCompile = parseResult.GetValue(UseRefAssembliesForCompileOption);
        ConfigProfile = parseResult.GetValue(ConfigProfileOption);
        EnablePreviewFeatures = parseResult.HasOption(PreviewOption);
        EnableSourceGeneratorSupport = parseResult.HasOption(EnableSourceGeneratorOption);
        ParserPreprocessorSymbolNames = new(parseResult.GetValue(ParserSymbolNamesOption) ?? [], StringComparer.Ordinal);
        ParserFeatures = parseResult.GetValue(ParserFeaturesOption)?
            .Select(x => x.Split('='))
            .Select(x => new KeyValuePair<string, string>(x[0], x.Length > 1 ? x[1] : string.Empty))
            .ToArray();
        EnvVariables = parseResult.GetValue(EnvOption)?
            .Select(x => x.Split('='))
            .Select(x => new KeyValuePair<string, string>(x[0], x.Length > 1 ? x[1] : string.Empty))
            .ToArray();
        CompileOutput = parseResult.GetValue(CompileOutputOption);
        DryRun = parseResult.HasOption(DryRunOption);
        DebugEnabled = Helper.DebugModelEnabled(Environment.GetCommandLineArgs());
        Timeout = parseResult.GetValue(TimeoutOption);
        var nugetConfigFile = parseResult.GetValue(NuGetConfigFileOption);
        if (!string.IsNullOrEmpty(nugetConfigFile))
        {
            Environment.SetEnvironmentVariable(NuGetHelper.NuGetConfigEnvName, nugetConfigFile);
        }

        if (configProfile != null)
        {
            if (!parseResult.HasOption(EntryPointOption) && !string.IsNullOrEmpty(configProfile.EntryPoint))
            {
                EntryPoint = configProfile.EntryPoint;
            }
            if (configProfile.DefaultEntryMethods is { Length: > 0 })
            {
                DefaultEntryMethods = configProfile.DefaultEntryMethods;
            }
            if (!parseResult.HasOption(PreviewOption))
            {
                EnablePreviewFeatures = configProfile.EnablePreviewFeatures;
            }
            if (!parseResult.HasOption(WebReferencesOption))
            {
                IncludeWebReferences = configProfile.IncludeWebReferences;
            }
            if (parseResult.GetResult(WideReferencesOption)?.Implicit == true)
            {
                IncludeWideReferences = configProfile.IncludeWideReferences;
            }

            foreach (var profileReference in configProfile.References)
            {
                References.Add(profileReference);
            }
            foreach (var profileUsing in configProfile.Usings)
            {
                Usings.Add(profileUsing);
            }
        }

        if (EnvVariables is { Length: > 0 })
        {
            foreach (var envVariable in EnvVariables)
            {
                Environment.SetEnvironmentVariable(envVariable.Key, envVariable.Value);
            }
        }
    }


    public static Command GetCommand()
    {
        var command = new RootCommand("dotnet-exec, execute raw C# script/program from the command line");

        // arguments
        foreach (var argument in GetArguments())
        {
            command.Add(argument);
        }
        // options
        foreach (var option in GetOptions())
        {
            command.Add(option);
        }

        // add sub commands
        command.Add(new ConfigProfileCommand());
        command.Add(new AliasCommand());
        command.Add(new TestCommand());

        return command;
    }

    private static IEnumerable<Argument> GetArguments()
    {
        yield return ScriptArgument;
    }

    private static IEnumerable<Option> GetOptions()
    {
        return typeof(ExecOptions)
            .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(Option<>))
            .Select(f => f.GetValue(null))
            .Cast<Option>();
    }

    public LanguageVersion GetLanguageVersion() =>
        EnablePreviewFeatures ? LanguageVersion.Preview : LanguageVersion.Latest;
}
