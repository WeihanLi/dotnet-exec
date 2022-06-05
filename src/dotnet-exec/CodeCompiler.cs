// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WeihanLi.Common.Models;

namespace Exec;

public interface ICodeCompiler
{
    Task<Result<CompileResult>> Compile(ExecOptions execOptions, string? code = null);
}

public sealed class SimpleCodeCompiler : ICodeCompiler
{
    public async Task<Result<CompileResult>> Compile(ExecOptions execOptions, string? code = null)
    {
        var projectName = $"{InternalHelper.ApplicationName}_{Guid.NewGuid():N}";
        var assemblyName = $"{projectName}.dll";

        var parseOptions = new CSharpParseOptions(execOptions.LanguageVersion);
        var globalUsingCode = InternalHelper.GetGlobalUsingsCodeText(execOptions.IncludeWebReferences);
        var globalUsingSyntaxTree = CSharpSyntaxTree.ParseText(globalUsingCode, parseOptions, cancellationToken: execOptions.CancellationToken);
        if (string.IsNullOrEmpty(code))
        {
            code = await File.ReadAllTextAsync(execOptions.Script, execOptions.CancellationToken);
        }
        var scriptSyntaxTree = CSharpSyntaxTree.ParseText(code, parseOptions, cancellationToken: execOptions.CancellationToken);

        var assemblyLocations = InternalHelper.ResolveFrameworkReferences(
                execOptions.IncludeWebReferences
                    ? FrameworkName.Web
                    : FrameworkName.Default, execOptions.TargetFramework)
            .SelectMany(x => x)
            .Distinct()
            .ToArray();
        var references = assemblyLocations
            .Select(l => MetadataReference.CreateFromFile(l, MetadataReferenceProperties.Assembly));

        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication,
            optimizationLevel: execOptions.Configuration, nullableContextOptions: NullableContextOptions.Annotations);
        compilationOptions.EnableReferencesSupersedeLowerVersions();

        var compilation = CSharpCompilation.Create(assemblyName, new[]
        {
            globalUsingSyntaxTree,
            scriptSyntaxTree
        }, references, compilationOptions);
        return await Guard.NotNull(compilation).GetCompilationAssemblyResult(execOptions.CancellationToken);
    }
}

internal sealed class AdhocWorkspaceCodeCompiler : ICodeCompiler
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

        var globalUsingCode = InternalHelper.GetGlobalUsingsCodeText(execOptions.IncludeWebReferences);
        var globalUsingDocument = DocumentInfo.Create(
            DocumentId.CreateNewId(projectInfo.Id, "__GlobalUsings"),
            "__GlobalUsings",
            loader: new PlainTextLoader(globalUsingCode));

        var scriptDocument = DocumentInfo.Create(DocumentId.CreateNewId(projectInfo.Id),
            Path.GetFileNameWithoutExtension(execOptions.Script));
        scriptDocument = string.IsNullOrEmpty(code)
            ? scriptDocument.WithFilePath(execOptions.Script)
            : scriptDocument.WithTextLoader(new PlainTextLoader(code));

        var assemblyLocations = InternalHelper.ResolveFrameworkReferences(
                execOptions.IncludeWebReferences
                    ? FrameworkName.Web
                    : FrameworkName.Default, execOptions.TargetFramework)
            .SelectMany(x => x)
            .Distinct()
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
