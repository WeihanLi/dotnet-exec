// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Services;

public interface IRepl
{
    Task Run();
}

public sealed class Repl : IRepl
{
    public async Task Run()
    {
        var script = CSharpScript.Create<object>(string.Empty);
        var state = script.RunAsync().Result;

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                continue;

            if (input.ToLower() == "#exit")
                break;
            
            if (input.StartsWith("#r "))
            {
                await AddReference(input.Substring(3));
                continue;
            }

            try
            {
                if (input.EndsWith("."))
                {
                    // Code completion
                    await ShowCompletions(script, input);
                }
                else
                {
                    // Execute the input
                    state = await state.ContinueWithAsync(input);
                    if (state.ReturnValue != null)
                        Console.WriteLine(state.ReturnValue);
                }
            }
            catch (CompilationErrorException e)
            {
                Console.WriteLine(string.Join(Environment.NewLine, e.Diagnostics));
            }
        }
    }
    
    private static async Task ShowCompletions(Script<object> script, string input)
    {
        var options = ScriptOptions.Default.AddReferences(typeof(Console).Assembly);
        var compilation = script.GetCompilation().AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(input));
        var completionService = CompletionService.GetService(compilation.SyntaxTrees.First().GetRoot().SyntaxTree);

        var completions = await completionService.GetCompletionsAsync(compilation.SyntaxTrees.First(), input.Length);
        
        foreach (var completion in completions.Items)
        {
            Console.WriteLine(completion.DisplayText);
        }
    }

    private static async Task AddReference(string reference)
    {
    }
}
