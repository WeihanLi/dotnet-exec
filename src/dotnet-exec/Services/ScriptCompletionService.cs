﻿// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Scripting;

namespace Exec.Services;

public interface IScriptCompletionService
{
    Task<IReadOnlyList<CompletionItem>> GetCompletions(ScriptOptions scriptOptions, string code);
}

/// <summary>
/// completion service for script
/// https://www.strathweb.com/2018/12/using-roslyn-c-completion-service-programmatically/
/// https://github.com/filipw/Strathweb.Samples.Roslyn.Completion
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class ScriptCompletionService : IScriptCompletionService
{
    public async Task<IReadOnlyList<CompletionItem>> GetCompletions(ScriptOptions scriptOptions, string code)
    {
        using var workspace = new AdhocWorkspace(MefHostServices.DefaultHost);
        var compilationOptions = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    nullableContextOptions: NullableContextOptions.Annotations
                )
                .WithUsings(scriptOptions.Imports)
            ;
        var projectInfo = ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "dotnet-exec-repl",
                    "dotnet-exec-repl",
                    LanguageNames.CSharp,
                    isSubmission: true
                )
                .WithMetadataReferences(scriptOptions.MetadataReferences)
                .WithCompilationOptions(compilationOptions)
            ;
        var project = workspace.AddProject(projectInfo);
        var documentInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(project.Id), "Script.cs",
            sourceCodeKind: SourceCodeKind.Script,
            loader: new PlainTextLoader(code)
        );
        var len = code.Length;
        var filter = string.Empty;
        var lastDotIndex = code.LastIndexOf('.') + 1;
        if (lastDotIndex < 1)
        {
            filter = code;
        }
        else if (lastDotIndex < code.Length)
        {
            filter = code[lastDotIndex..];
        }

        var document = workspace.AddDocument(documentInfo);
        var completionService = CompletionService.GetService(document);
        ArgumentNullException.ThrowIfNull(completionService);
        var completionList = await completionService.GetCompletionsAsync(document, code.Length);
        var list = completionList.ItemsList ?? [];

        if (string.IsNullOrEmpty(filter))
            return list;

        return list.Where(c => c.DisplayText.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToArray();
    }
}
