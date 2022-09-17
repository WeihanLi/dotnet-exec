// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Immutable;
using System.Runtime.Loader;
using WeihanLi.Common.Models;

namespace Exec;

public sealed class AdvancedCodeCompiler : ICodeCompiler
{
    private readonly ILogger _logger;
    public AdvancedCodeCompiler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null)
    {
        var msBuildInstance = RegisterMSBuild(options);
        // https://github.com/microsoft/MSBuildLocator/issues/86#issuecomment-640275377
        AssemblyLoadContext.Default.Resolving += (assemblyLoadContext, assemblyName) =>
        {
            var path = Path.Combine(msBuildInstance.MSBuildPath, assemblyName.Name + ".dll");
            if (File.Exists(path))
            {
                return assemblyLoadContext.LoadFromAssemblyPath(path);
            }

            return null;
        };
        var projectPath = GetProjectFile(options.ProjectPath);
        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (_, args) =>
        {
            _logger.LogError($"Workspace load failed, {args.Diagnostic.Kind}, {args.Diagnostic.Message}");
        };
        var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: options.CancellationToken);
        var documentIdsToRemove = project.Documents.Where(d =>
                d.FilePath.IsNotNullOrEmpty()
                && !d.FilePath.Equals(options.Script)
                && !(options.AdditionalScripts.HasValue() && options.AdditionalScripts.Any(x=>x == d.FilePath))
                && d.FilePath.EndsWith($"{Path.DirectorySeparatorChar}Program.cs"))
            .Select(d => d.Id)
            .ToImmutableArray();

        var compilationOptions = project.CompilationOptions?.WithOutputKind(OutputKind.ConsoleApplication)
                                 ?? new CSharpCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel: options.Configuration, nullableContextOptions: NullableContextOptions.Annotations);
        var compilation = await project.RemoveDocuments(documentIdsToRemove)
            .WithCompilationOptions(compilationOptions)
            .GetCompilationAsync(options.CancellationToken);
        return await Guard.NotNull(compilation)
            .GetCompilationAssemblyResult(options.CancellationToken);
    }

    private static string GetProjectFile(string projectFile)
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
            var projectFilePath = Directory.GetFiles(dir!, "*.csproj").FirstOrDefault();
            if (projectFilePath.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Please specify the project file");
            }

            project = projectFilePath;
        }

        return project;
    }

    // ReSharper disable InconsistentNaming
    private static VisualStudioInstance RegisterMSBuild(ExecOptions options)
    {
        var netVersion = Version.Parse(options.TargetFramework["net".Length..]);

        var msBuildInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
        var matchedInstance = msBuildInstances
            .Where(x => x.Version.Major == netVersion.Major && x.Version.Minor == netVersion.Minor)
            .MaxBy(x => x.Version);
        if (matchedInstance != null)
        {
            MSBuildLocator.RegisterInstance(matchedInstance);
        }
        else
        {
            matchedInstance = MSBuildLocator.RegisterDefaults();
        }

        return matchedInstance;
    }
}
