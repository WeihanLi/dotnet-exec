// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using WeihanLi.Common.Models;

namespace Exec;

public interface ICodeCompiler
{
    Task<Result<Assembly>> Compile(ExecOptions execOptions, string? code = null);
}

public class SimpleCodeCompiler : ICodeCompiler
{
    public async Task<Result<Assembly>> Compile(ExecOptions execOptions, string? code = null)
    {
        var projectName = $"dotnet-exec_{Guid.NewGuid():N}";
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
            Path.GetFileNameWithoutExtension(execOptions.ScriptFile));
        scriptDocument = string.IsNullOrEmpty(code) 
            ? scriptDocument.WithFilePath(execOptions.ScriptFile) 
            : scriptDocument.WithTextLoader(new PlainTextLoader(code));
        
        var references = InternalHelper.ResolveFrameworkReferences(
                execOptions.IncludeWebReferences
                    ? FrameworkName.Web
                    : FrameworkName.Default, execOptions.TargetFramework, true)
            .SelectMany(x=> x)
            .Distinct()
            .Select(l => MetadataReference.CreateFromFile(l));
        
        projectInfo = projectInfo
                .WithParseOptions(new CSharpParseOptions(execOptions.LanguageVersion))
                .WithDocuments(new[] { globalUsingDocument, scriptDocument })
                .WithMetadataReferences(references)
            ;
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject(projectInfo);
        var compilation = await project
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel: execOptions.Configuration, nullableContextOptions: NullableContextOptions.Annotations))
            .GetCompilationAsync();
        return await Guard.NotNull(compilation).GetCompilationAssemblyResult(execOptions.CancellationToken);
    }
}
