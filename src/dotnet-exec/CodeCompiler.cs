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
        var assemblyName = $"{InternalHelper.ApplicationName}_{Guid.NewGuid():N}.dll";
        var parseOptions = new CSharpParseOptions(execOptions.LanguageVersion);
        var globalUsingCode = InternalHelper.GetGlobalUsingsCodeText(execOptions);
        var globalUsingSyntaxTree = CSharpSyntaxTree.ParseText(globalUsingCode, parseOptions, cancellationToken: execOptions.CancellationToken);
        if (string.IsNullOrEmpty(code))
        {
            code = await File.ReadAllTextAsync(execOptions.Script, execOptions.CancellationToken);
        }
        var scriptSyntaxTree = CSharpSyntaxTree.ParseText(code, parseOptions, cancellationToken: execOptions.CancellationToken);
        var references = InternalHelper.ResolveReferences(execOptions)
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

