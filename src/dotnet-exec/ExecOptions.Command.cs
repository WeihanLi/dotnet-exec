// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Commands;
using Exec.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Exec;

public sealed partial class ExecOptions
{
    internal const string DefaultTargetFramework =
#if NET8_0_OR_GREATER
      "net8.0"
#elif NET7_0
      "net7.0"
#else
            "net6.0"
#endif
        ;

    private static readonly Argument<string> ScriptArgument = new("script", "CSharp script to execute");

    private static readonly Option<string> TargetFrameworkOption = new(new[] { "-f", "--framework" },
        () => DefaultTargetFramework, "Target framework");

    private static readonly Option<string> StartupTypeOption = new("--startup-type", "The startup type to use for finding the correct entry");
    internal static readonly Option<string> EntryPointOption = new(new[] { "-e", "--entry" }, "Custom entry point('MainTest' by default)");

    private static readonly Option<string> CompilerTypeOption =
        new(new[] { "--compiler-type", "--compiler" }, () => "workspace", "The compiler to use");

    private static readonly Option<string> ExecutorTypeOption =
        new(new[] { "--executor-type", "--executor" }, () => Helper.Default, "The executor to use");

    internal static readonly Option<bool> PreviewOption =
        new("--preview", "Use preview language feature and enable preview features");

    private static readonly Option<OptimizationLevel> ConfigurationOption =
        new(new[] { "-c", "--configuration" }, "Compile configuration/OptimizationLevel");

    private static readonly Option<string> ArgumentsOption =
        new(new[] { "--args", "--arguments" }, "Input arguments");

    private static readonly Option<bool> DebugOption = new("--debug", "Enable debug logs for debug");

    private static readonly Option<bool> UseRefAssembliesForCompileOption = new("--ref-compile",
        "Use Ref assemblies for compile, when not found from local download from nuget");

    private static readonly Option<string> ProjectOption =
        new("--project", "The project file path to exact references and usings");

    internal static readonly Option<bool> WideReferencesOption =
        new(new[] { "--wide" }, () => true, "Includes widely-used references(Microsoft.Extensions.Configuration/DependencyInjection/Logging,Newtonsoft.Json,WeihanLi.Common)");

    internal static readonly Option<bool> WebReferencesOption =
        new(new[] { "-w", "--web" }, "Includes Web SDK references");

    internal static readonly Option<string[]> ReferencesOption =
        new(new[] { "-r", "--reference" }, "Additional references") { Arity = ArgumentArity.ZeroOrMore };

    internal static readonly Option<string[]> UsingsOption =
        new(new[] { "-u", "--using" }, "Namespace usings") { Arity = ArgumentArity.ZeroOrMore };

    private static readonly Option<string[]> AdditionalScriptsOption =
        new(new[] { "--ad", "--addition" }, "Additional script path");

    private static readonly Option<bool> EnableSourceGeneratorOption =
        new(new[] { "--generator" }, "Enable the source generator support");

    internal static readonly Option<string> ConfigProfileOption =
        new(new[] { "--profile" }, "The config profile to use");

    private static readonly Option<string[]> ParserSymbolNamesOption =
        new(new[] { "--compile-symbol" }, "Preprocessor symbol names for parsing and compiling");

    private static readonly Option<string[]> ParserFeaturesOption =
        new(new[] { "--compile-feature" }, "Features for parsing and compiling");

    static ExecOptions()
    {
        CompilerTypeOption.FromAmong("simple", "workspace");
        ExecutorTypeOption.FromAmong(Helper.Default);
        TargetFrameworkOption.FromAmong(Helper.SupportedFrameworks.ToArray());
    }

    public void BindCommandLineArguments(ParseResult parseResult, ConfigProfile? configProfile)
    {
        Script = Guard.NotNull(parseResult.GetValueForArgument(ScriptArgument));
        StartupType = parseResult.GetValueForOption(StartupTypeOption);
        EntryPoint = parseResult.GetValueForOption(EntryPointOption).GetValueOrDefault("MainTest");
        TargetFramework = parseResult.GetValueForOption(TargetFrameworkOption)
            .GetValueOrDefault(DefaultTargetFramework);
        Configuration = parseResult.GetValueForOption(ConfigurationOption);
        Arguments = CommandLineStringSplitter.Instance
            .Split(parseResult.GetValueForOption(ArgumentsOption) ?? string.Empty).ToArray();
        ProjectPath = parseResult.GetValueForOption(ProjectOption) ?? string.Empty;
        IncludeWideReferences = parseResult.GetValueForOption(WideReferencesOption);
        IncludeWebReferences = parseResult.GetValueForOption(WebReferencesOption);
        CompilerType = parseResult.GetValueForOption(CompilerTypeOption) ?? Helper.Default;
        ExecutorType = parseResult.GetValueForOption(ExecutorTypeOption) ?? Helper.Default;
        References = new(parseResult.GetValueForOption(ReferencesOption) ?? Array.Empty<string>());
        Usings = new(parseResult.GetValueForOption(UsingsOption) ?? Array.Empty<string>());
        AdditionalScripts = new(parseResult.GetValueForOption(AdditionalScriptsOption) ?? Array.Empty<string>());
        DebugEnabled = parseResult.HasOption(DebugOption);
        UseRefAssembliesForCompile = parseResult.GetValueForOption(UseRefAssembliesForCompileOption);
        ConfigProfile = parseResult.GetValueForOption(ConfigProfileOption);
        EnablePreviewFeatures = parseResult.HasOption(PreviewOption);
        EnableSourceGeneratorSupport = parseResult.HasOption(EnableSourceGeneratorOption);
        ParserPreprocessorSymbolNames = new(parseResult.GetValueForOption(ParserSymbolNamesOption) ?? Array.Empty<string>());
        ParserFeatures = parseResult.GetValueForOption(ParserFeaturesOption)?
            .Select(x => x.Split('='))
            .Select(x => new KeyValuePair<string, string>(x[0], x.Length > 1 ? x[1] : string.Empty))
            .ToArray();

        if (configProfile != null)
        {
            if (!parseResult.HasOption(EntryPointOption) && !string.IsNullOrEmpty(configProfile.EntryPoint))
            {
                EntryPoint = configProfile.EntryPoint;
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
    }

    public static Command GetCommand()
    {
        var command = new Command(Helper.ApplicationName);
        command.AddCommand(new ConfigProfileCommand());
        foreach (var argument in GetArguments())
        {
            command.AddArgument(argument);
        }
        foreach (var option in GetOptions())
        {
            command.AddOption(option);
        }
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
