// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WeihanLi.Common.Models;

namespace Exec;

public sealed class AdhocWorkspaceCodeCompiler : ICodeCompiler
{
    public async Task<Result<CompileResult>> Compile(ExecOptions execOptions, string? code = null)
    {
        var projectName = $"{InternalHelper.ApplicationName}_{Guid.NewGuid():N}";
        var assemblyName = $"{projectName}.dll";
        var projectInfo = ProjectInfo.Create(
            ProjectId.CreateNewId(),
            VersionStamp.Default,
            projectName,
            assemblyName,
            LanguageNames.CSharp);

        var globalUsingCode = InternalHelper.GetGlobalUsingsCodeText(execOptions);
        var globalUsingDocument = DocumentInfo.Create(
            DocumentId.CreateNewId(projectInfo.Id, "__GlobalUsings"),
            "__GlobalUsings",
            loader: new PlainTextLoader(globalUsingCode));

        var scriptDocument = DocumentInfo.Create(DocumentId.CreateNewId(projectInfo.Id),
            Path.GetFileNameWithoutExtension(execOptions.Script));
        scriptDocument = string.IsNullOrEmpty(code)
            ? scriptDocument.WithFilePath(execOptions.Script)
            : scriptDocument.WithTextLoader(new PlainTextLoader(code));

        var assemblyLocations = InternalHelper.ResolveReferences(execOptions)
            .ToArray();

        var references = assemblyLocations.Select(l => MetadataReference.CreateFromFile(l));
        projectInfo = projectInfo
                .WithParseOptions(new CSharpParseOptions(execOptions.LanguageVersion))
                .WithDocuments(new[] { globalUsingDocument, scriptDocument })
                .WithMetadataReferences(references)
            ;
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject(projectInfo);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication,
            optimizationLevel: execOptions.Configuration, nullableContextOptions: NullableContextOptions.Annotations);
        compilationOptions.EnableReferencesSupersedeLowerVersions();

        var compilation = await project
            .WithCompilationOptions(compilationOptions)
            .GetCompilationAsync();
        return await Guard.NotNull(compilation).GetCompilationAssemblyResult(execOptions.CancellationToken);
    }
}
