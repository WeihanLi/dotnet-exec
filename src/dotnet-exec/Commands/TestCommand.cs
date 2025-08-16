// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Commands;

internal sealed class TestCommand : Command
{
    private const string XunitTestEntryCode = "await Xunit.Runner.InProc.SystemConsole.ConsoleRunner.Run(args);";

    private readonly Argument<string[]> _testFileArgument = new
    (
        "testFiles"
    )
    {
        Arity = ArgumentArity.OneOrMore,
        Description = "The xunit test files to test"
    };

    private const string XunitPackageReference = "nuget: xunit.v3,2.0.0";
    private const string XunitNamespace = "global::Xunit";
    private const string XunitV3Namespace = "global::Xunit.v3";

    public TestCommand() : base("test", "Execute xunit test cases")
    {
        Add(_testFileArgument);

        Add(ExecOptions.UsingsOption);
        Add(ExecOptions.ReferencesOption);
        Add(ExecOptions.WebReferencesOption);
        Add(ExecOptions.PreviewOption);
        Add(ExecOptions.DebugOption);
    }

    public async Task<int> InvokeAsync(ParseResult parseResult, CommandHandler commandHandler, CancellationToken cancellationToken)
    {
        var options = new ExecOptions
        {
            CancellationToken = cancellationToken
        };

        var references = parseResult.GetValue(ExecOptions.ReferencesOption);
        foreach (var reference in references ?? [])
        {
            options.References.Add(reference);
        }
        var usings = parseResult.GetValue(ExecOptions.UsingsOption);
        foreach (var @using in usings ?? [])
        {
            options.Usings.Add(@using);
        }

        options.IncludeWebReferences = parseResult.GetValue(ExecOptions.WebReferencesOption);
        options.EnablePreviewFeatures = parseResult.GetValue(ExecOptions.PreviewOption);
        options.IncludeWideReferences = false;

        var testFiles = parseResult.GetValue(_testFileArgument);

        return await ExecuteAsync(commandHandler, options, testFiles ?? []);
    }

    public static Task<int> ExecuteAsync(CommandHandler commandHandler, ExecOptions options, params IEnumerable<string> testFiles)
    {
        options.Script = XunitTestEntryCode;
        options.AdditionalScripts = [.. testFiles];
        options.References.Add(XunitPackageReference);
        options.Usings.Add(XunitNamespace);
        options.Usings.Add(XunitV3Namespace);
        options.ExecutorType = options.CompilerType = Helper.Project;
        return commandHandler.Execute(options);
    }
}
