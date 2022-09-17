// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WeihanLi.Common.Models;

namespace Exec;

public interface ICodeCompiler
{
    Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null);
}

public sealed class DefaultCodeCompiler : ICodeCompiler
{
    private readonly IRefResolver _referenceResolver;

    public DefaultCodeCompiler(IRefResolver referenceResolver)
    {
        _referenceResolver = referenceResolver;
    }

    public async Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null)
    {
        var assemblyName = $"{Helper.ApplicationName}_{Guid.NewGuid():N}.dll";
        var parseOptions = new CSharpParseOptions(options.LanguageVersion);
        var globalUsingCode = Helper.GetGlobalUsingsCodeText(options);
        var globalUsingSyntaxTree = CSharpSyntaxTree.ParseText(globalUsingCode, parseOptions, cancellationToken: options.CancellationToken);
        if (string.IsNullOrEmpty(code))
        {
            code = await File.ReadAllTextAsync(options.Script, options.CancellationToken);
        }
        var scriptSyntaxTree = CSharpSyntaxTree.ParseText(code, parseOptions, cancellationToken: options.CancellationToken);
        var syntaxTreeList = new List<SyntaxTree>()
        {
            globalUsingSyntaxTree,
            scriptSyntaxTree,
        };
        if (options.AdditionalScripts.HasValue())
        {
            foreach (var additionalScript in options.AdditionalScripts)
            {
                var scriptText = await File.ReadAllTextAsync(additionalScript, options.CancellationToken);
                var syntaxTree = CSharpSyntaxTree.ParseText(scriptText, parseOptions, additionalScript, null, options.CancellationToken);
                syntaxTreeList.Add(syntaxTree);
            }
        }
        var references = await _referenceResolver.ResolveMetadataReferences(options, true);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication,
            optimizationLevel: options.Configuration, nullableContextOptions: NullableContextOptions.Annotations);
        compilationOptions.EnableReferencesSupersedeLowerVersions();

        var compilation = CSharpCompilation.Create(assemblyName, syntaxTreeList, references, compilationOptions);
        return await Guard.NotNull(compilation).GetCompilationAssemblyResult(options.CancellationToken);
    }
}

