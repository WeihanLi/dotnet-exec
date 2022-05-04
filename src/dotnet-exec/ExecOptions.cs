// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

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
    private static readonly Option<string> EntryPointOption = new("--entry", () => "MainTest", "Entry point");
    private static readonly Option<LanguageVersion> LanguageVersionOption =
        new("--lang-version", () => LanguageVersion.Default, "Language version");
    private static readonly Option<OptimizationLevel> ConfigurationOption =
        new(new[]{ "-c", "--configuration" }, () => OptimizationLevel.Debug, "Compile configuration/OptimizationLevel");
    private static readonly Option DebugOption = new("--debug", "Enable debug logs for debugging purpose");

    //
    private static readonly ImmutableHashSet<string> DefaultGlobalUsing = new HashSet<string>()
        {
            "System",
            "System.Collections.Generic",
            "System.IO",
            "System.Linq",
            "System.Net.Http",
            "System.Text",
            "System.Threading",
            "System.Threading.Tasks",
        }
        .ToImmutableHashSet();

    public string ScriptFile { get; set; } = "Program.cs";

    public string TargetFramework { get; set; } = DefaultTargetFramework;

    public string EntryPoint { get; set; } = "MainTest";

    public LanguageVersion LanguageVersion { get; set; }
    public OptimizationLevel Configuration { get; set; }

    public HashSet<string> GlobalUsing { get; } = new(DefaultGlobalUsing);

    public static IEnumerable<Argument> GetArguments()
    {
        yield return FilePathArgument;
    }

    public static IEnumerable<Option> GetOptions()
    {
        yield return DebugOption;
        yield return TargetFrameworkOption;
        yield return EntryPointOption;
        yield return LanguageVersionOption;
        yield return ConfigurationOption;
    }

    public void BindCommandLineArguments(ParseResult parseResult)
    {
        ScriptFile = Guard.NotNull(parseResult.GetValueForArgument(FilePathArgument));        
        EntryPoint = Guard.NotNull(parseResult.GetValueForOption(EntryPointOption));
        TargetFramework = parseResult.GetValueForOption(TargetFrameworkOption).GetValueOrDefault(DefaultTargetFramework);
        LanguageVersion = parseResult.GetValueForOption(LanguageVersionOption);
        Configuration = parseResult.GetValueForOption(ConfigurationOption);
    }
}
