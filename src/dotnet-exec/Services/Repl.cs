// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using WeihanLi.Common.Models;

namespace Exec.Services;

public interface IRepl
{
    Task RunAsync(ExecOptions options);
}

[ExcludeFromCodeCoverage]
internal sealed class Repl
    (
        IRefResolver referenceResolver,
        IAdditionalScriptContentFetcher scriptContentFetcher
    ) : IRepl
{
    public async Task RunAsync(ExecOptions options)
    {
        var references = await referenceResolver.ResolveMetadataReferences(options, false);
        var scriptOptions = ScriptOptions.Default
                .WithReferences(references)
                .WithOptimizationLevel(options.Configuration)
                .WithAllowUnsafe(true)
                .WithLanguageVersion(options.GetLanguageVersion())
            ;
        var globalUsingText = Helper.GetGlobalUsingsCodeText(options);
        var state = await CSharpScript.RunAsync(globalUsingText, scriptOptions);
        var script = state.Script;
        if (options.AdditionalScripts.HasValue())
        {
            foreach (var additionalScript in options.AdditionalScripts)
            {
                var additionalScriptCode = await scriptContentFetcher.FetchContent(additionalScript, options.CancellationToken);
                if (additionalScriptCode.IsSuccess())
                {
                    script = script.ContinueWith(additionalScriptCode.Data, scriptOptions);
                }
            }
        }
        Console.WriteLine("REPL started, Enter #exit to exit, #help for help text");
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                continue;

            if ("#exit".EqualsIgnoreCase(input))
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
                    state = await CSharpScript.RunAsync(script.Code, scriptOptions);
                    script = state.Script;
                    ConsoleHelper.WriteLineWithColor("Reference added", ConsoleColor.DarkGreen);
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLineWithColor($"Exception when add reference", ConsoleColor.DarkRed);
                    ConsoleHelper.WriteLineWithColor(CSharpObjectFormatter.Instance.FormatException(ex), ConsoleColor.DarkRed);
                }
                continue;
            }

            if (input.EndsWith('.'))
            {
                var completions = await GetCompletions(state, scriptOptions, input);
            }

            try
            {
                var anotherScript = script.ContinueWith(input, scriptOptions);
                var diagnostics = anotherScript.Compile();
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
                    var anotherState = await anotherScript.RunFromAsync(state);
                    if (anotherState.ReturnValue is not null)
                    {
                        Console.WriteLine(CSharpObjectFormatter.Instance.FormatObject(anotherState.ReturnValue));
                    }
                    state = anotherState;
                    script = anotherState.Script;
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLineWithColor($"Exception when execute script", ConsoleColor.DarkRed);
                    ConsoleHelper.WriteLineWithColor(CSharpObjectFormatter.Instance.FormatException(ex), ConsoleColor.DarkRed);
                    continue;
                }

                //
                script = anotherScript;
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

    private static async Task<IReadOnlyList<CompletionItem>> GetCompletions(
        ScriptState scriptState, ScriptOptions scriptOptions, string input)
    {
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("Script", LanguageNames.CSharp)
            .WithMetadataReferences(scriptOptions.MetadataReferences);

        var combinedCode = scriptState.Script.Code + input;
        var document = project.AddDocument("Script.csx", combinedCode);
        var completionService = CompletionService.GetService(document);
        ArgumentNullException.ThrowIfNull(completionService);

        var completionList = await completionService.GetCompletionsAsync(document, combinedCode.Length - 1);
        return completionList?.ItemsList ?? [];
    }
}
