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
    private readonly IAdditionalScriptContentFetcher _scriptContentFetcher;

    public DefaultCodeCompiler(IRefResolver referenceResolver, IAdditionalScriptContentFetcher scriptContentFetcher)
    {
        _referenceResolver = referenceResolver;
        _scriptContentFetcher = scriptContentFetcher;
    }

    public async Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null)
    {
        var assemblyName = $"{Helper.ApplicationName}_{Guid.NewGuid():N}.dll";
        var parseOptions = new CSharpParseOptions(options.GetLanguageVersion());
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
                var scriptContent = await _scriptContentFetcher.FetchContent(additionalScript, options.CancellationToken);
                if (string.IsNullOrEmpty(scriptContent.Data))
                    continue;
                var syntaxTree = CSharpSyntaxTree.ParseText(scriptContent.Data, parseOptions, additionalScript, null, options.CancellationToken);
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

