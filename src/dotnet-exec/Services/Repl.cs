// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using ReferenceResolver;
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

        Console.WriteLine("REPL started, Enter #q or #exit to exit, #r to reference dll or package, #cls or #clear to clear screen");
        ScriptState? state = null;
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if ("#q".EqualsIgnoreCase(input) || "#exit".EqualsIgnoreCase(input))
                break;

            if ("#cls".EqualsIgnoreCase(input) || "#clear".EqualsIgnoreCase(input))
            {
                Console.Clear();
                continue;
            }

            if ("#help".EqualsIgnoreCase(input))
            {
                // print detailed help text
                Console.WriteLine("Enter #q or #exit to exit, #cls or #clear to clear screen");
                continue;
            }

            if (input.StartsWith("#r ", StringComparison.Ordinal))
            {
                try
                {
                    var reference = Helper.ReferenceNormalize(input[3..]);
                    options.References.Add(reference);
                    options.DisableCache = true;
                    if (ReferenceResolverFactory.ParseReference(reference) is FrameworkReference frameworkReference)
                    {
                        var frameworkImplicitUsings = FrameworkReferenceResolver.GetImplicitUsings(frameworkReference.Reference);
                        foreach (var u in frameworkImplicitUsings)
                        {
                            if (options.Usings.Add(u))
                            {
                                scriptOptions = scriptOptions.AddImports(u.TrimStart("global::").Trim());
                            }
                        }
                    }
                    references = await referenceResolver.ResolveMetadataReferences(options, false);
                    scriptOptions = scriptOptions.WithReferences(references);
                    ConsoleHelper.WriteLineWithColor("Reference added", ConsoleColor.DarkGreen);
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLineWithColor($"Exception when add reference", ConsoleColor.DarkRed);
                    ConsoleHelper.WriteLineWithColor(CSharpObjectFormatter.Instance.FormatException(ex), ConsoleColor.DarkRed);
                }
                continue;
            }

            if (input.Length > 1 && input[^1] is '?' or '.')
            {
                var endWithDot = input[^1] is '.';
                var inputScript = endWithDot ? input : input[..^1];
                var completions = await scriptCompletionService.GetCompletions(scriptOptions, inputScript);
                foreach (var completion in completions)
                {
                    Console.WriteLine(completion.DisplayText);
                }
                continue;
            }

            try
            {
                if (state is null)
                {
                    state = await CSharpScript.RunAsync(input, scriptOptions);
                }
                else
                {
                    var anotherScriptState = await state.ContinueWithAsync(input, scriptOptions);
                    state = anotherScriptState;
                }
                if (state.ReturnValue is not null)
                {
                    Console.WriteLine(CSharpObjectFormatter.Instance.FormatObject(state.ReturnValue));
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
            catch (Exception ex)
            {
                ConsoleHelper.WriteLineWithColor($"Exception when execute script", ConsoleColor.DarkRed);
                ConsoleHelper.WriteLineWithColor(CSharpObjectFormatter.Instance.FormatException(ex), ConsoleColor.DarkRed);
            }
        }
    }
}
