using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Exec;

public sealed class ExecOptions
{
    private const string DefaultTargetFramework = 
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

    public string TargetFramework { get; set; } = "net6.0";

    public string EntryPoint { get; set; } = "MainTest";

    public LanguageVersion LanguageVersion { get; set; }

    public HashSet<string> GlobalUsing { get; } = new(DefaultGlobalUsing);

    public static IEnumerable<Argument> GetArguments()
    {
        yield return FilePathArgument;
    }

    public static IEnumerable<Option> GetOptions()
    {
        yield return TargetFrameworkOption;
        yield return EntryPointOption;
        yield return LanguageVersionOption;
    }

    public void BindCommandLineArguments(ParseResult parseResult)
    {
        ScriptFile = Guard.NotNull(parseResult.GetValueForArgument(FilePathArgument));        
        EntryPoint = Guard.NotNull(parseResult.GetValueForOption(EntryPointOption));
        TargetFramework = parseResult.GetValueForOption(TargetFrameworkOption).GetValueOrDefault(DefaultTargetFramework);
    }
}
