// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Immutable;
using WeihanLi.Common.Models;

namespace Exec;

public sealed class AdvancedCodeCompiler : ICodeCompiler
{
    private readonly ILogger _logger;

    static AdvancedCodeCompiler()
    {
        MSBuildLocator.RegisterDefaults();
    }

    public AdvancedCodeCompiler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<Result<CompileResult>> Compile(ExecOptions execOptions, string? code = null)
    {
        var projectPath = GetProjectFile(execOptions.ProjectPath);
        var dotnetPath = Helper.GetDotnetPath();
        var result = await CommandExecutor.ExecuteAndCaptureAsync(dotnetPath, $"restore {projectPath}", Path.GetDirectoryName(projectPath));
        if (result.ExitCode != 0)
        {
            return Result.Fail<CompileResult>($"{result.StandardError}{Environment.NewLine}{result.StandardOut}".Trim(), ResultStatus.ProcessFail);
        }

        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (_, args) =>
        {
            _logger.LogError($"Workspace failed, {args.Diagnostic.Kind}, {args.Diagnostic.Message}");
        };

        var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: execOptions.CancellationToken);
        var documentIds = project.Documents.Where(d =>
                d.FilePath.IsNotNullOrEmpty()
                && !d.FilePath.Equals(execOptions.Script)
                && d.FilePath.EndsWith($"{Path.DirectorySeparatorChar}Program.cs"))
            .Select(d => d.Id)
            .ToImmutableArray();

        project = project.RemoveDocuments(documentIds);

        var compilationOptions = project.CompilationOptions?.WithOutputKind(OutputKind.ConsoleApplication)
                                 ?? new CSharpCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel: execOptions.Configuration, nullableContextOptions: NullableContextOptions.Annotations);
        compilationOptions.EnableReferencesSupersedeLowerVersions();

        var compilation = await project.WithCompilationOptions(compilationOptions).GetCompilationAsync(execOptions.CancellationToken);
        return await Guard.NotNull(compilation).GetCompilationAssemblyResult(execOptions.CancellationToken);
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
