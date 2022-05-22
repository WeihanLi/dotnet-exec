// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Exec;

public sealed class ExecOptions
{
    internal const string DefaultTargetFramework =
#if NET7_0
      "net7.0"
#else
      "net6.0"
#endif
    ;

    private static readonly Argument<string> FilePathArgument = new("file", "CSharp program to execute");
    private static readonly Option<string> TargetFrameworkOption = new(new[] { "-f", "--framework" }, () => DefaultTargetFramework, "Project target framework");
    private static readonly Option<string> StartupTypeOption = new("--startup-type", "Startup type");
    private static readonly Option<string> EntryPointOption = new("--entry", () => "MainTest", "Entry point");

    private static readonly Option<LanguageVersion> LanguageVersionOption =
        new("--lang-version", () => LanguageVersion.Default, "Language version");

    private static readonly Option<OptimizationLevel> ConfigurationOption =
        new(new[] { "-c", "--configuration" }, () => OptimizationLevel.Debug, "Compile configuration/OptimizationLevel");

    private static readonly Option<string> ArgumentsOption =
        new(new[] { "--args", "--arguments" }, "Input arguments");

    private static readonly Option DebugOption = new("--debug", "Enable debug logs for debugging purpose");
    private static readonly Option<string> ProjectOption = new("--project", "Project file path");
    private static readonly Option AdvancedOption = new(new[] { "-a", "--advanced" }, "Advanced mode");
    private static readonly Option WebReferencesOption = new(new[] { "-w", "--web" }, "Reference web mode");


    public string ScriptFile { get; set; } = "Program.cs";

    public string TargetFramework { get; set; } = DefaultTargetFramework;

    public string StartupType { get; set; } = string.Empty;
    public string EntryPoint { get; set; } = "MainTest";

    public string[] Arguments { get; set; } = Array.Empty<string>();

    public string ProjectPath { get; set; } = string.Empty;

    public bool IncludeWebReferences { get; set; }

    public LanguageVersion LanguageVersion { get; set; }
    public OptimizationLevel Configuration { get; set; }

    [JsonIgnore]
    public CancellationToken CancellationToken { get; set; }

    public static IEnumerable<Argument> GetArguments()
    {
        yield return FilePathArgument;
    }

    public static IEnumerable<Option> GetOptions()
    {
        yield return DebugOption;
        yield return TargetFrameworkOption;
        yield return StartupTypeOption;
        yield return EntryPointOption;
        yield return LanguageVersionOption;
        yield return ConfigurationOption;
        yield return ArgumentsOption;
        yield return ProjectOption;
        yield return AdvancedOption;
        yield return WebReferencesOption;
    }

    public void BindCommandLineArguments(ParseResult parseResult)
    {
        ScriptFile = Guard.NotNull(parseResult.GetValueForArgument(FilePathArgument));
        var dir = Path.GetDirectoryName(ScriptFile);
        if (dir.IsNullOrEmpty())
        {
            ScriptFile = Path.Combine(Directory.GetCurrentDirectory(), ScriptFile);
        }

        StartupType = parseResult.GetValueForOption(StartupTypeOption) ?? string.Empty;
        EntryPoint = Guard.NotNull(parseResult.GetValueForOption(EntryPointOption));
        TargetFramework = parseResult.GetValueForOption(TargetFrameworkOption).GetValueOrDefault(DefaultTargetFramework);
        LanguageVersion = parseResult.GetValueForOption(LanguageVersionOption);
        Configuration = parseResult.GetValueForOption(ConfigurationOption);
        Arguments = CommandLineStringSplitter.Instance
            .Split(parseResult.GetValueForOption(ArgumentsOption) ?? string.Empty).ToArray();
        ProjectPath = parseResult.GetValueForOption(ProjectOption) ?? string.Empty;
        IncludeWebReferences = parseResult.HasOption(WebReferencesOption);
    }
}
