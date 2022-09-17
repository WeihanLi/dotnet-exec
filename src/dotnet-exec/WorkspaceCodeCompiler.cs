// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WeihanLi.Common.Models;

namespace Exec;

public sealed class WorkspaceCodeCompiler : ICodeCompiler
{
    private readonly IRefResolver _referenceResolver;

    public WorkspaceCodeCompiler(IRefResolver referenceResolver)
    {
        _referenceResolver = referenceResolver;
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
            isGenerated: true);

        var scriptDocument = DocumentInfo.Create(DocumentId.CreateNewId(projectInfo.Id), "Script");
        scriptDocument = string.IsNullOrEmpty(code)
            ? scriptDocument.WithFilePath(options.Script)
            : scriptDocument.WithTextLoader(new PlainTextLoader(code));

        var documents = new List<DocumentInfo>() { globalUsingDocument, scriptDocument, };
        if (options.AdditionalScripts.HasValue())
        {
            foreach (var additionalScript in options.AdditionalScripts)
            {
                var additionDoc = DocumentInfo.Create(DocumentId.CreateNewId(projectInfo.Id),
                    Path.GetFileNameWithoutExtension(additionalScript), loader:new FileTextLoader(additionalScript, null), filePath: additionalScript);
                documents.Add(additionDoc);
            }   
        }

        var references = await _referenceResolver.ResolveMetadataReferences(options, true);
        projectInfo = projectInfo
                .WithParseOptions(new CSharpParseOptions(options.LanguageVersion))
                .WithDocuments(documents)
                .WithMetadataReferences(references)
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
