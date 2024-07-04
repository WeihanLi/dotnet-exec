// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Exec.Services;

public interface IRepl
{
    Task RunAsync(ExecOptions options);
}

[ExcludeFromCodeCoverage]
internal sealed class Repl
    (
        IRefResolver referenceResolver,
        IScriptCompletionService scriptCompletionService
    ) : IRepl
{
    public async Task RunAsync(ExecOptions options)
    {
        options.DisableCache = true;

        var references = await referenceResolver.ResolveMetadataReferences(options, false);
        var globalUsings = Helper.GetGlobalUsingList(options);
        var scriptOptions = ScriptOptions.Default
                .WithOptimizationLevel(options.Configuration)
                .WithAllowUnsafe(true)
                .WithLanguageVersion(options.GetLanguageVersion())
                .WithReferences(references)
                .AddImports(globalUsings.Select(g => g.TrimStart("global::")))
            ;

        Console.WriteLine("REPL started, Enter #q or #exit to exit");
        ScriptState state = await CSharpScript.RunAsync("", scriptOptions);
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if ("#q".EqualsIgnoreCase(input) || "#exit".EqualsIgnoreCase(input))
                break;

            if ("#help".EqualsIgnoreCase(input))
            {
                // print detailed help text
                continue;
            }

            if (input.StartsWith("#r ", StringComparison.Ordinal))
            {
                try
                {
                    var reference = input[3..];
                    options.References.Add(Helper.ReferenceNormalize(reference));
                    options.DisableCache = true;
                    references = await referenceResolver.ResolveMetadataReferences(options, false);
                    scriptOptions = scriptOptions.WithReferences(references);
                    state = await CSharpScript.RunAsync(state.Script.Code, scriptOptions);
                    ConsoleHelper.WriteLineWithColor("Reference added", ConsoleColor.DarkGreen);
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLineWithColor($"Exception when add reference", ConsoleColor.DarkRed);
                    ConsoleHelper.WriteLineWithColor(CSharpObjectFormatter.Instance.FormatException(ex), ConsoleColor.DarkRed);
                }
                continue;
            }

            if (input.EndsWith('.') && input.Length > 1)
            {
                var completions = await scriptCompletionService.GetCompletions(scriptOptions, input);
                if (completions is { Count: > 0 })
                {
                    foreach (var completion in completions)
                    {
                        Console.WriteLine(completion.DisplayText);
                    }
                }
                else
                {
                    completions = await scriptCompletionService.GetCompletions(scriptOptions, input[..^1]);
                    foreach (var completion in completions)
                    {
                        Console.WriteLine(completion.DisplayText);
                    }
                }
                continue;
            }

            try
            {
                var anotherScriptState = await state.ContinueWithAsync(input, scriptOptions);
                var diagnostics = anotherScriptState.Script.Compile();
                if (diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error))
                {
                    // error
                    foreach (var diagnostic in diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Error))
                    {
                        ConsoleHelper.WriteLineWithColor(CSharpDiagnosticFormatter.Instance.Format(diagnostic, CultureInfo.CurrentCulture), ConsoleColor.DarkRed);
                    }
                    continue;
                }

                try
                {
                    var anotherState = await anotherScriptState.Script.RunFromAsync(state);
                    if (anotherState.ReturnValue is not null)
                    {
                        Console.WriteLine(CSharpObjectFormatter.Instance.FormatObject(anotherState.ReturnValue));
                    }
                    state = anotherState;
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLineWithColor($"Exception when execute script", ConsoleColor.DarkRed);
                    ConsoleHelper.WriteLineWithColor(CSharpObjectFormatter.Instance.FormatException(ex), ConsoleColor.DarkRed);
                }
            }
            catch (CompilationErrorException e)
            {
                ConsoleHelper.WriteLineWithColor($"Exception when compile script", ConsoleColor.DarkRed);
                ConsoleHelper.WriteLineWithColor(CSharpObjectFormatter.Instance.FormatException(e), ConsoleColor.DarkRed);
                foreach (var diagnostic in e.Diagnostics)
                {
                    ConsoleHelper.WriteLineWithColor(CSharpDiagnosticFormatter.Instance.Format(diagnostic, CultureInfo.CurrentCulture), ConsoleColor.DarkRed);
                }
            }
        }
    }
}
