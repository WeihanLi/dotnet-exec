// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Text;
using WeihanLi.Common.Models;

namespace Exec;

public interface ICodeCompiler
{
    Task<Result<Assembly>> Compile(string code, ExecOptions execOptions);
}

public class SimpleCodeCompiler : ICodeCompiler
{
    private static readonly HashSet<string> SpecialConsoleDiagnosticIds = new() { "CS5001", "CS0028" };
    public async Task<Result<Assembly>> Compile(string code, ExecOptions execOptions)
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

    private static async Task<(CSharpCompilation, EmitResult, Assembly?)> GetCompilation(string code, ExecOptions execOptions)
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
            .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel: execOptions.Configuration, allowUnsafe: true))            
            .AddReferences(Basic.Reference.Assemblies.Net60.All)
            .AddReferences(references)
            .AddSyntaxTrees(syntaxTree);

        await using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);
        if (emitResult.Success)
        {
            return (compilation, emitResult, Assembly.Load(ms.ToArray()));
        }
        if (emitResult.Diagnostics.Any(d => SpecialConsoleDiagnosticIds.Contains(d.Id)))
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
