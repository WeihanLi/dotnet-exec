// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WeihanLi.Common.Models;

namespace Exec.Services;

public sealed class WorkspaceCodeCompiler(
        IRefResolver referenceResolver,
        IAdditionalScriptContentFetcher scriptContentFetcher,
        IParseOptionsPipeline parseOptionsPipeline,
        ICompilationOptionsPipeline compilationOptionsPipeline
        )
    : ICodeCompiler
{
    public async Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        var projectName = $"{Helper.ApplicationName}_{Guid.NewGuid():N}";
        var assemblyName = projectName;
        var projectInfo = ProjectInfo.Create(
            ProjectId.CreateNewId(),
            VersionStamp.Default,
            projectName,
            assemblyName,
            LanguageNames.CSharp);

        var scriptDocument = DocumentInfo.Create(DocumentId.CreateNewId(projectInfo.Id, "__Script"), "__Script.cs");
        scriptDocument = string.IsNullOrEmpty(code)
            ? scriptDocument.WithFilePath(options.Script)
            : scriptDocument.WithTextLoader(new PlainTextLoader(code));
        if (scriptDocument.FilePath is null && File.Exists(options.Script))
        {
            var fullPath = Path.GetFullPath(options.Script);
            scriptDocument = scriptDocument.WithFilePath(fullPath);
        }

        var globalUsingCode = Helper.GetGlobalUsingsCodeText(options);
        var globalUsingDocument = DocumentInfo.Create(
            DocumentId.CreateNewId(projectInfo.Id, "__GlobalUsings"),
            "__GlobalUsings",
            loader: new PlainTextLoader(globalUsingCode),
            filePath: "__GlobalUsings.cs",
            isGenerated: true);
        var documents = new List<DocumentInfo>() { globalUsingDocument, scriptDocument };
        if (options.AdditionalScripts.HasValue())
        {
            foreach (var additionalScript in options.AdditionalScripts)
            {
                var scriptContent = await scriptContentFetcher.FetchContent(additionalScript, options.CancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(scriptContent.Data))
                    continue;

                var additionDoc = DocumentInfo.Create(DocumentId.CreateNewId(projectInfo.Id),
                    Path.GetFileName(additionalScript), loader: new PlainTextLoader(scriptContent.Data), filePath: additionalScript);
                documents.Add(additionDoc);
            }
        }

        var parseOptions = new CSharpParseOptions(options.GetLanguageVersion());
        parseOptions = parseOptionsPipeline.Configure(parseOptions, options);

        var metadataReferences = await referenceResolver.ResolveMetadataReferences(options, true)
            .ConfigureAwait(false);
        projectInfo = projectInfo
                .WithParseOptions(parseOptions)
                .WithDocuments(documents)
                .WithMetadataReferences(metadataReferences);
        if (options.EnableSourceGeneratorSupport)
        {
            var analyzerReferences = await referenceResolver.ResolveAnalyzerReferences(options)
                .ConfigureAwait(false);
            var generatorReferences = analyzerReferences.Select(x => new
            {
                Reference = x,
                Generators = x.GetGenerators(LanguageNames.CSharp)
            })
                .Where(x => x.Generators.HasValue())
                .Select(x => x.Reference)
                .ToArray();
            if (generatorReferences.HasValue())
            {
                projectInfo = projectInfo.WithAnalyzerReferences(generatorReferences);
            }
        }
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject(projectInfo);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication,
            optimizationLevel: options.Configuration, nullableContextOptions: NullableContextOptions.Annotations);
        compilationOptions.EnableReferencesSupersedeLowerVersions();
        compilationOptions = compilationOptionsPipeline.Configure(compilationOptions, options);

        var compilation = await project
            .WithCompilationOptions(compilationOptions)
            .GetCompilationAsync().ConfigureAwait(false);
        return await Guard.NotNull(compilation).GetCompilationAssemblyResult(options.CancellationToken).ConfigureAwait(false);
    }
}
