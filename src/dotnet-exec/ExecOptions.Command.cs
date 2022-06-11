// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Exec;

public partial class ExecOptions
{
    internal const string DefaultTargetFramework =
#if NET7_0
      "net7.0"
#else
           "net6.0"
#endif
       ;

    private static readonly Argument<string> FilePathArgument = new("script", "CSharp program to execute");

    private static readonly Option<string> TargetFrameworkOption = new(new[] { "-f", "--framework" },
        () => DefaultTargetFramework, "Target framework");

    private static readonly Option<string> StartupTypeOption = new("--startup-type", "Startup type");
    private static readonly Option<string> EntryPointOption = new("--entry", () => "MainTest", "Entry point");

    private static readonly Option<string> CompilerTypeOption =
        new("--compiler-type", () => "default", "The compiler to use");

    private static readonly Option<LanguageVersion> LanguageVersionOption =
        new("--lang-version", () => LanguageVersion.Default, "Language version");

    private static readonly Option<bool> PreviewOption =
        new("--preview", "Use preview language feature and enable preview features");

    private static readonly Option<OptimizationLevel> ConfigurationOption =
        new(new[] { "-c", "--configuration" }, "Compile configuration/OptimizationLevel");

    private static readonly Option<string> ArgumentsOption =
        new(new[] { "--args", "--arguments" }, "Input arguments");

    private static readonly Option<bool> DebugOption = new("--debug", "Enable debug logs for debugging purpose");
    private static readonly Option<string> ProjectOption = new("--project", "Project file path");
    private static readonly Option<bool> AdvancedOption = new(new[] { "-a", "--advanced" }, "Advanced mode");

    private static readonly Option<bool> WideReferencesOption =
        new(new[] { "-w" }, () => true, "Include Newtonsoft.Json/WeihanLi.Common references");

    private static readonly Option<string[]> AdditionalReferencesOption =
        new(new[] { "-r", "--reference" }, "Additional references") { Arity = ArgumentArity.ZeroOrMore };

    static ExecOptions()
    {
        CompilerTypeOption.AddCompletions("advanced", "workspace", "default");
    }

    public void BindCommandLineArguments(ParseResult parseResult)
    {
        Script = Guard.NotNull(parseResult.GetValueForArgument(FilePathArgument));
        StartupType = parseResult.GetValueForOption(StartupTypeOption);
        EntryPoint = Guard.NotNull(parseResult.GetValueForOption(EntryPointOption));
        TargetFramework = parseResult.GetValueForOption(TargetFrameworkOption)
            .GetValueOrDefault(DefaultTargetFramework);
        LanguageVersion = parseResult.GetValueForOption(LanguageVersionOption);
        Configuration = parseResult.GetValueForOption(ConfigurationOption);
        Arguments = CommandLineStringSplitter.Instance
            .Split(parseResult.GetValueForOption(ArgumentsOption) ?? string.Empty).ToArray();
        ProjectPath = parseResult.GetValueForOption(ProjectOption) ?? string.Empty;
        IncludeWideReferences = parseResult.HasOption(WideReferencesOption);
        CompilerType = parseResult.GetValueForOption(CompilerTypeOption) ?? "default";
        AdditionalReferences = parseResult.GetValueForOption(AdditionalReferencesOption);

        if (parseResult.HasOption(AdvancedOption))
        {
            CompilerType = "advanced";
        }
        if (parseResult.HasOption(PreviewOption))
        {
            LanguageVersion = LanguageVersion.Preview;
        }
    }

    public static Command GetCommand(string commandName)
    {
        var command = new Command(commandName);
        foreach (var item in GetArguments())
        {
            command.AddArgument(item);
        }

        foreach (var option in GetOptions())
        {
            command.AddOption(option);
        }

        return command;
    }

    private static IEnumerable<Argument> GetArguments()
    {
        yield return FilePathArgument;
    }

    private static IEnumerable<Option> GetOptions()
    {
        yield return DebugOption;
        yield return TargetFrameworkOption;
        yield return StartupTypeOption;
        yield return EntryPointOption;
        yield return LanguageVersionOption;
        yield return PreviewOption;
        yield return ConfigurationOption;
        yield return ArgumentsOption;
        yield return ProjectOption;
        yield return AdvancedOption;
        yield return WideReferencesOption;
        yield return CompilerTypeOption;
        yield return AdditionalReferencesOption;
    }
}
