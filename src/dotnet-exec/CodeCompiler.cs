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

public sealed class DefaultCodeCompiler : ICodeCompiler
{
    private readonly IReferenceResolver _referenceResolver;

    public DefaultCodeCompiler(IReferenceResolver referenceResolver)
    {
        _referenceResolver = referenceResolver;
    }

    public async Task<Result<CompileResult>> Compile(ExecOptions execOptions, string? code = null)
    {
        var assemblyName = $"{Helper.ApplicationName}_{Guid.NewGuid():N}.dll";
        var parseOptions = new CSharpParseOptions(execOptions.LanguageVersion);
        var globalUsingCode = Helper.GetGlobalUsingsCodeText(execOptions);
        var globalUsingSyntaxTree = CSharpSyntaxTree.ParseText(globalUsingCode, parseOptions, cancellationToken: execOptions.CancellationToken);
        if (string.IsNullOrEmpty(code))
        {
            code = await File.ReadAllTextAsync(execOptions.Script, execOptions.CancellationToken);
        }
        var scriptSyntaxTree = CSharpSyntaxTree.ParseText(code, parseOptions, cancellationToken: execOptions.CancellationToken);
        var assemblyLocations = await _referenceResolver.ResolveReferences(execOptions, true);
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

