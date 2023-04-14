// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WeihanLi.Common.Models;

namespace Exec.Implements;

public sealed class WorkspaceCodeCompiler : ICodeCompiler
{
    private readonly IRefResolver _referenceResolver;
    private readonly IAdditionalScriptContentFetcher _scriptContentFetcher;

    public WorkspaceCodeCompiler(IRefResolver referenceResolver, IAdditionalScriptContentFetcher scriptContentFetcher)
    {
        _referenceResolver = referenceResolver;
        _scriptContentFetcher = scriptContentFetcher;
    }

    public async Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null)
    {
        var projectName = $"{Helper.ApplicationName}_{Guid.NewGuid():N}";
        var assemblyName = $"{projectName}.dll";
        var projectInfo = ProjectInfo.Create(
            ProjectId.CreateNewId(),
            VersionStamp.Default,
            projectName,
            assemblyName,
            LanguageNames.CSharp);

        var globalUsingCode = Helper.GetGlobalUsingsCodeText(options);
        var globalUsingDocument = DocumentInfo.Create(
            DocumentId.CreateNewId(projectInfo.Id, "__GlobalUsings"),
            "__GlobalUsings",
            loader: new PlainTextLoader(globalUsingCode),
            filePath: "__GlobalUsings.cs",
            isGenerated: true);

        var scriptDocument = DocumentInfo.Create(DocumentId.CreateNewId(projectInfo.Id, "__Script"), "__Script.cs");
        scriptDocument = string.IsNullOrEmpty(code)
            ? scriptDocument.WithFilePath(options.Script)
            : scriptDocument.WithTextLoader(new PlainTextLoader(code));

        var documents = new List<DocumentInfo>() { globalUsingDocument, scriptDocument };
        if (options.AdditionalScripts.HasValue())
        {
            foreach (var additionalScript in options.AdditionalScripts)
            {
                var scriptContent = await _scriptContentFetcher.FetchContent(additionalScript, options.CancellationToken);
                if (string.IsNullOrWhiteSpace(scriptContent.Data))
                    continue;

                var additionDoc = DocumentInfo.Create(DocumentId.CreateNewId(projectInfo.Id),
                    Path.GetFileName(additionalScript), loader: new PlainTextLoader(scriptContent.Data), filePath: additionalScript);
                documents.Add(additionDoc);
            }
        }

        var metadataReferences = await _referenceResolver.ResolveMetadataReferences(options, true);
        var analyzerReferences = await _referenceResolver.ResolveAnalyzerReferences(options);
        projectInfo = projectInfo
                .WithParseOptions(new CSharpParseOptions(options.GetLanguageVersion()))
                .WithDocuments(documents)
                .WithMetadataReferences(metadataReferences)
                .WithAnalyzerReferences(analyzerReferences)
            ;
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject(projectInfo);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication,
            optimizationLevel: options.Configuration, nullableContextOptions: NullableContextOptions.Annotations);
        compilationOptions.EnableReferencesSupersedeLowerVersions();

        var compilation = await project
            .WithCompilationOptions(compilationOptions)
            .GetCompilationAsync();
        return await Guard.NotNull(compilation).GetCompilationAssemblyResult(options.CancellationToken);
    }
}
