// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
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
        scriptDocument = string.IsNullOrEmpty(code) ? scriptDocument.WithFilePath(execOptions.ScriptFile) : scriptDocument.WithTextLoader(new PlainTextLoader(code));
        
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
        return await Guard.NotNull(compilation).GetCompilationAssemblyResult();
    }
}

public class AdvancedCodeCompiler : ICodeCompiler
{
    static AdvancedCodeCompiler()
    {
        MSBuildLocator.RegisterDefaults();
    }

    public async Task<Result<Assembly>> Compile(ExecOptions execOptions, string? code = null)
    {
        var projectPath = GetProjectFile(execOptions.ProjectPath);
        var dotnetPath = InternalHelper.GetDotnetPath();        
        var result = await CommandExecutor.ExecuteAndCaptureAsync(dotnetPath, $"restore {projectPath}", Path.GetDirectoryName(projectPath));
        if (result.ExitCode != 0)
        {
            return Result.Fail<Assembly>($"{result.StandardError}{Environment.NewLine}{result.StandardOut}".Trim(), ResultStatus.ProcessFail);
        }
        
        using var workspace = MSBuildWorkspace.Create();
        var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: execOptions.CancellationToken);
        var documentIds = project.Documents.Where(d =>
                d.FilePath.IsNotNullOrEmpty()
                && !d.FilePath.Equals(execOptions.ScriptFile)
                && !InternalHelper.GlobalUsingFileNames.Contains(Path.GetFileName(d.FilePath))
                && !d.FilePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Select(d => d.Id)
            .ToImmutableArray();
        
        var globalUsingCode = InternalHelper.GetGlobalUsingsCodeText(execOptions.IncludeWebReferences);
        var globalUsingDoc = project.AddDocument("__GlobalUsings", SourceText.From(globalUsingCode));
        project = globalUsingDoc.Project.RemoveDocuments(documentIds)
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel: execOptions.Configuration, nullableContextOptions: NullableContextOptions.Annotations))
                .AddMetadataReferences(InternalHelper.ResolveFrameworkReferences(FrameworkName.Default, execOptions.TargetFramework, true)
                    .SelectMany(_ => _)
                    .Select(x=> MetadataReference.CreateFromFile(x)))
            ;
        
        var compilation = await project.GetCompilationAsync(execOptions.CancellationToken);
        return await Guard.NotNull(compilation).GetCompilationAssemblyResult();
    }

    private string GetProjectFile(string projectFile)
    {
        var project = string.Empty;
        var dir = Directory.GetCurrentDirectory();
        if (projectFile.IsNotNullOrEmpty())
        {
            if (projectFile.EndsWith(".csproj"))
            {
                project = projectFile;
            }
            else
            {
                dir = Directory.Exists(projectFile)
                        ? projectFile
                        : Path.GetDirectoryName(projectFile)
                    ;
            }
        }

        if (project.IsNullOrEmpty())
        {
            project = Directory.GetFiles(dir!, "*.csproj").First();
        }

        return project;
    }
}
