﻿// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
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
#if NET9_0_OR_GREATER
      "net9.0"
#elif NET8_0
      "net8.0"
#elif NET7_0
      "net7.0"
#else
      "net6.0"
#endif
        ;

    private static readonly Argument<string[]> ScriptArgument = new("script", "CSharp script to execute, start repl when no script provided")
    {
        Arity = ArgumentArity.ZeroOrMore
    };

    private static readonly Option<string> TargetFrameworkOption = new(["-f", "--framework"],
        () => DefaultTargetFramework, "Target framework");

    private static readonly Option<string> StartupTypeOption = new("--startup-type", "The startup type to use for finding the correct entry");
    internal static readonly Option<string> EntryPointOption = new(["-e", "--entry"], "Custom entry point('MainTest' by default)");

    internal static readonly Option<string[]> DefaultEntryMethodsOption = new("--default-entry", "Default entry methods")
    {
        Arity = ArgumentArity.ZeroOrMore
    };

    private static readonly Option<string> CompilerTypeOption =
        new(["--compiler-type", "--compiler"], () => "workspace", "The compiler to use");

    private static readonly Option<string> ExecutorTypeOption =
        new(["--executor-type", "--executor"], () => Helper.Default, "The executor to use");

    internal static readonly Option<bool> PreviewOption =
        new("--preview", "Use preview language feature and enable preview features");

    private static readonly Option<OptimizationLevel> ConfigurationOption =
        new(["-c", "--configuration"], "Compile configuration/OptimizationLevel");

    private static readonly Option<string?> ArgumentsOption =
        new(["--args", "--arguments"], "Input arguments, please use `-- <args[0]> <args[1]>` instead")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

    private static readonly Option<bool> UseRefAssembliesForCompileOption = new("--ref-compile",
        "Use Ref assemblies for compile, when not found from local download from nuget");

    private static readonly Option<string> ProjectOption =
        new("--project", "The project file path to exact references and usings");

    internal static readonly Option<bool> WideReferencesOption =
        new(["--wide"], () => true, "Includes widely-used references(Microsoft.Extensions.Configuration/DependencyInjection/Logging,Newtonsoft.Json,WeihanLi.Common)");

    internal static readonly Option<bool> WebReferencesOption =
        new(["-w", "--web"], "Includes Web SDK references");

    internal static readonly Option<string[]> ReferencesOption =
        new(["-r", "--reference"], "Additional references") { Arity = ArgumentArity.ZeroOrMore };

    internal static readonly Option<string[]> UsingsOption =
        new(["-u", "--using"], "Namespace usings") { Arity = ArgumentArity.ZeroOrMore };

    private static readonly Option<string[]> AdditionalScriptsOption =
        new(["--ad", "--addition"], "Additional script path");

    private static readonly Option<bool> EnableSourceGeneratorOption =
        new(["--generator"], "Enable the source generator support");

    internal static readonly Option<string> ConfigProfileOption =
        new(["--profile"], "The config profile to use");

    private static readonly Option<string[]> ParserSymbolNamesOption =
        new(["--compile-symbol"], "Preprocessor symbol names for parsing and compiling");

    private static readonly Option<string[]> ParserFeaturesOption =
        new(["--compile-feature"], "Features for parsing and compiling");

    private static readonly Option<bool> DryRunOption = new(["--dry-run"], "Dry-run, would not execute script and output debug info");
    private static readonly Option<string> NuGetConfigFileOption = new(["--nuget-config"], "NuGet config file path to use");
    private static readonly Option<string[]> EnvOption =
        new(["--env"], "Set environment variable for process, usage example: --env name=test --env value=123");

    private static readonly Option<string?> CompileOutputOption = new(["--compile-out"], "Compiled dll output path");

#pragma warning disable IDE0052
    private static readonly Option<bool> DebugOption = new("--debug", "Enable debug logs for debug");
    private static readonly Option<bool> InfoOption = new(["--info"], "Tool version and runtime info");
#pragma warning restore IDE0052

    static ExecOptions()
    {
        CompilerTypeOption.FromAmong("simple", "workspace");
        ExecutorTypeOption.FromAmong(Helper.Default);
        TargetFrameworkOption.FromAmong([.. Helper.SupportedFrameworks]);
    }

    public void BindCommandLineArguments(ParseResult parseResult, ConfigProfile? configProfile)
    {
        var scripts = parseResult.GetValueForArgument(ScriptArgument);
        Script = scripts.FirstOrDefault() ?? string.Empty;
        StartupType = parseResult.GetValueForOption(StartupTypeOption);
        EntryPoint = parseResult.GetValueForOption(EntryPointOption);
        TargetFramework = parseResult.GetValueForOption(TargetFrameworkOption)
            .GetValueOrDefault(DefaultTargetFramework);
        Configuration = parseResult.GetValueForOption(ConfigurationOption);

        Arguments = Helper.CommandArguments.HasValue()
            ? Helper.CommandArguments
            : CommandLineStringSplitter.Instance
                .Split(parseResult.GetValueForOption(ArgumentsOption) ?? string.Empty).ToArray();

        ProjectPath = parseResult.GetValueForOption(ProjectOption) ?? string.Empty;
        IncludeWideReferences = parseResult.GetValueForOption(WideReferencesOption);
        IncludeWebReferences = parseResult.GetValueForOption(WebReferencesOption) || EnvHelper.Val(Helper.EnableWebReferenceEnvName).ToBoolean();
        CompilerType = parseResult.GetValueForOption(CompilerTypeOption) ?? Helper.Default;
        var executorTypeValue = parseResult.GetValueForOption(ExecutorTypeOption);
        ExecutorType = string.IsNullOrEmpty(executorTypeValue)
            ? Helper.Script.EqualsIgnoreCase(CompilerType)
                ? Helper.Script
                : Helper.Default
            : executorTypeValue;
        foreach (var reference in parseResult.GetValueForOption(ReferencesOption) ?? [])
        {
            References.Add(Helper.ReferenceNormalize(reference));
        }
        Usings = [.. parseResult.GetValueForOption(UsingsOption) ?? []];
        AdditionalScripts = new(scripts.Skip(1).Union(parseResult.GetValueForOption(AdditionalScriptsOption) ?? []), StringComparer.Ordinal);
        UseRefAssembliesForCompile = parseResult.GetValueForOption(UseRefAssembliesForCompileOption);
        ConfigProfile = parseResult.GetValueForOption(ConfigProfileOption);
        EnablePreviewFeatures = parseResult.HasOption(PreviewOption);
        EnableSourceGeneratorSupport = parseResult.HasOption(EnableSourceGeneratorOption);
        ParserPreprocessorSymbolNames = new(parseResult.GetValueForOption(ParserSymbolNamesOption) ?? [], StringComparer.Ordinal);
        ParserFeatures = parseResult.GetValueForOption(ParserFeaturesOption)?
            .Select(x => x.Split('='))
            .Select(x => new KeyValuePair<string, string>(x[0], x.Length > 1 ? x[1] : string.Empty))
            .ToArray();
        EnvVariables = parseResult.GetValueForOption(EnvOption)?
            .Select(x => x.Split('='))
            .Select(x => new KeyValuePair<string, string>(x[0], x.Length > 1 ? x[1] : string.Empty))
            .ToArray();
        CompileOutput = parseResult.GetValueForOption(CompileOutputOption);
        DryRun = parseResult.HasOption(DryRunOption);
        DebugEnabled = Helper.DebugModelEnabled(Environment.GetCommandLineArgs());
        var nugetConfigFile = parseResult.GetValueForOption(NuGetConfigFileOption);
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
            if (parseResult.FindResultFor(WideReferencesOption)?.IsImplicit == true)
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
        var command = new Command(Helper.ApplicationName, "dotnet-exec, execute C# script/program from command line");
        // arguments
        foreach (var argument in GetArguments())
        {
            command.AddArgument(argument);
        }
        // options
        foreach (var option in GetOptions())
        {
            command.AddOption(option);
        }
        // add sub commands
        command.AddCommand(new ConfigProfileCommand());
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
