// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using WeihanLi.Common.Models;

namespace Exec;

public interface ICodeCompiler
{
    Task<Result<Assembly>> Compile(ExecOptions execOptions, string code);
}

public class SimpleCodeCompiler : ICodeCompiler
{
    public async Task<Result<Assembly>> Compile(ExecOptions execOptions, string code)
    {
        var (_, compilationResult, assembly) = await GetCompilation(code, execOptions);
        await using var ms = new MemoryStream();
        if (compilationResult.Success)
        {
            return Result.Success(Guard.NotNull(assembly));
        }

        var error = new StringBuilder();
        foreach (var diag in compilationResult.Diagnostics)
        {
            var message = CSharpDiagnosticFormatter.Instance.Format(diag);
            error.AppendLine($"{diag.Id}-{diag.Severity}-{message}");
        }

        return Result.Fail<Assembly>(error.ToString(), ResultStatus.ProcessFail);
    }

    private static async Task<(CSharpCompilation, EmitResult, Assembly?)> GetCompilation(string code,
        ExecOptions execOptions)
    {
        var globalUsingCode = execOptions.GlobalUsing.Select(u => $"global using {u};").StringJoin(Environment.NewLine);
        var combinedCode = $"{globalUsingCode}{Environment.NewLine}{code}";
        var syntaxTree = CSharpSyntaxTree.ParseText(combinedCode, new CSharpParseOptions(execOptions.LanguageVersion));
        var references = new[]
            {
                typeof(Microsoft.Extensions.Configuration.IConfigurationBuilder).Assembly,
                typeof(Microsoft.Extensions.Configuration.ConfigurationBuilder).Assembly,
                typeof(Microsoft.Extensions.DependencyInjection.ServiceCollection).Assembly,
                typeof(Microsoft.Extensions.Logging.LoggerFactory).Assembly,
                typeof(Microsoft.Extensions.Options.IOptions<>).Assembly,
                typeof(Newtonsoft.Json.JsonConvert).Assembly,
                typeof(Result).Assembly,
            }
            .Select(assembly => assembly.Location)
            .Distinct()
            .Select(l => MetadataReference.CreateFromFile(l))
            .Cast<MetadataReference>()
            .ToArray();

        var assemblyName = $"dotnet-exec.dynamic.{GuidIdGenerator.Instance.NewId()}";
        var compilation = CSharpCompilation.Create(assemblyName)
            .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication,
                optimizationLevel: execOptions.Configuration, allowUnsafe: true))
            .AddReferences(Basic.Reference.Assemblies.Net60.All)
            .AddReferences(references)
            .AddSyntaxTrees(syntaxTree);

        await using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);
        if (emitResult.Success)
        {
            return (compilation, emitResult, Assembly.Load(ms.ToArray()));
        }

        if (emitResult.Diagnostics.Any(d => InternalHelper.SpecialConsoleDiagnosticIds.Contains(d.Id)))
        {
            ms.Seek(0, SeekOrigin.Begin);
            ms.SetLength(0);
            compilation = compilation.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            emitResult = compilation.Emit(ms);
            return (compilation, emitResult, emitResult.Success ? Assembly.Load(ms.ToArray()) : null);
        }

        return (compilation, emitResult, null);
    }
}

public class AdvancedCodeCompiler : ICodeCompiler
{
    static AdvancedCodeCompiler()
    {
        MSBuildLocator.RegisterDefaults();
    }

    public async Task<Result<Assembly>> Compile(ExecOptions execOptions, string code)
    {
        var projectPath = GetProjectFile(execOptions.ProjectPath);
        var workspace = MSBuildWorkspace.Create();
        var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: execOptions.CancellationToken);

        var compilation = await project.GetCompilationAsync(execOptions.CancellationToken);
        Guard.NotNull(compilation);
        var entryPoint = compilation.GetEntryPoint(execOptions.CancellationToken);
         if (!"<top-level-statements-entry-point>".Equals(entryPoint?.ToString()))
        {
            var id = Guid.NewGuid().ToString("N");
            project.AddDocument("DynamicMain.generated.cs", $@"
namespace Dynamic_{id}
{{
  internal class Program
  {{
    public static void Main(string[] args)
    {{
        {execOptions.StartupType}.{execOptions.EntryPoint}();
        System.Console.ReadLine();
    }}
  }}
}}
");
            project = project.WithCompilationOptions(Guard.NotNull(project.CompilationOptions)
                .WithMainTypeName($"Dynamic_{id}.Program"));
            compilation = await project.GetCompilationAsync(execOptions.CancellationToken);
        }
        else
        {
            var documentIds = project.Documents.Where(d =>
                    d.FilePath.IsNotNullOrEmpty() 
                    && !d.FilePath.Equals(execOptions.ScriptFile)
                    && !InternalHelper.GlobalUsingFileNames.Contains(Path.GetFileName(d.FilePath))
                    && !d.FilePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                .Select(d => d.Id)
                .ToImmutableArray(); 
            project = project.RemoveDocuments(documentIds);
            compilation = await project.GetCompilationAsync(execOptions.CancellationToken);
        }

        Guard.NotNull(compilation);
        var ms = new MemoryStream();
        try
        {
            var emitResult = compilation.Emit(ms);
            if (emitResult.Success)
            {
                return Result.Success(Assembly.Load(ms.ToArray()));
            }

            if (emitResult.Diagnostics.Any(x => InternalHelper.SpecialConsoleDiagnosticIds.Contains(x.Id)))
            {
                project = project.WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                compilation = await project.GetCompilationAsync(execOptions.CancellationToken);
                Guard.NotNull(compilation);
                emitResult = compilation.Emit(ms);
                if (emitResult.Success)
                {
                    return Result.Success(Assembly.Load(ms.ToArray()));
                }
            }

            var error = new StringBuilder();
            foreach (var diag in emitResult.Diagnostics)
            {
                var message = CSharpDiagnosticFormatter.Instance.Format(diag);
                error.AppendLine($"{diag.Id}-{diag.Severity}-{message}");
            }

            return Result.Fail<Assembly>(error.ToString(), ResultStatus.ProcessFail);
        }
        finally
        {
            await ms.DisposeAsync();
        }
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
            project = Directory.GetFiles(dir, "*.csproj").First();
        }

        return project;
    }
}
