// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Commands;

internal sealed class TestCommand : Command
{
    private const string XunitTestEntryCode = "await Xunit.Runner.InProc.SystemConsole.ConsoleRunner.Run(args);";
    
    private readonly Argument<string[]> _testFileArgument = new
    (
        "testFiles",
        "The xunit test files to test"
    )
    {
        Arity = ArgumentArity.OneOrMore
    };
    
    public TestCommand() : base("test", "Execute xunit test cases")
    {
        AddArgument(_testFileArgument);
        
        AddOption(ExecOptions.UsingsOption);
        AddOption(ExecOptions.ReferencesOption);
        AddOption(ExecOptions.WebReferencesOption);
        AddOption(ExecOptions.WideReferencesOption);
        AddOption(ExecOptions.PreviewOption);
    }

    public async Task<int> InvokeAsync(InvocationContext context, CommandHandler commandHandler)
    {
        var testFiles = context.ParseResult.GetValueForArgument(_testFileArgument);
        
        var options = new ExecOptions
        {
            Script = XunitTestEntryCode,
            AdditionalScripts = [..testFiles],
            References = ["nuget: xunit.v3,2.0.0"],
            Usings = ["global::Xunit", "global::Xunit.v3"],
            CompilerType = Helper.Project,
            ExecutorType = Helper.Project,
            CancellationToken = context.GetCancellationToken()
        };
        
        var references = context.ParseResult.GetValueForOption(ExecOptions.ReferencesOption);
        foreach (var reference in references ?? [])
        {
            options.References.Add(reference);
        }
        var usings = context.ParseResult.GetValueForOption(ExecOptions.UsingsOption);
        foreach (var @using in usings ?? [])
        {
            options.Usings.Add(@using);
        }
        
        options.IncludeWebReferences = context.ParseResult.GetValueForOption(ExecOptions.WebReferencesOption);
        options.EnablePreviewFeatures = context.ParseResult.GetValueForOption(ExecOptions.PreviewOption);
        options.IncludeWideReferences = false;

        return await commandHandler.Execute(options);
    }
}
