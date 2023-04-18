// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WeihanLi.Common.Models;

namespace Exec.Implements;

public sealed class SimpleCodeCompiler : ICodeCompiler
{
    private readonly IRefResolver _referenceResolver;
    private readonly IAdditionalScriptContentFetcher _scriptContentFetcher;

    public SimpleCodeCompiler(IRefResolver referenceResolver, IAdditionalScriptContentFetcher scriptContentFetcher)
    {
        _referenceResolver = referenceResolver;
        _scriptContentFetcher = scriptContentFetcher;
    }

    public async Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null)
    {
        var assemblyName = $"{Helper.ApplicationName}_{Guid.NewGuid():N}.dll";
        var parseOptions = new CSharpParseOptions(options.GetLanguageVersion());
        var globalUsingCode = Helper.GetGlobalUsingsCodeText(options);
        var globalUsingSyntaxTree = CSharpSyntaxTree.ParseText(globalUsingCode, parseOptions, "__GlobalUsings.cs",
            cancellationToken: options.CancellationToken);
        if (string.IsNullOrEmpty(code))
        {
            code = await File.ReadAllTextAsync(options.Script, options.CancellationToken);
        }

        var scriptSyntaxTree =
            CSharpSyntaxTree.ParseText(code, parseOptions, "__Script.cs", cancellationToken: options.CancellationToken);
        var syntaxTreeList = new List<SyntaxTree>() { globalUsingSyntaxTree, scriptSyntaxTree, };
        if (options.AdditionalScripts.HasValue())
        {
            foreach (var additionalScript in options.AdditionalScripts)
            {
                var scriptContent =
                    await _scriptContentFetcher.FetchContent(additionalScript, options.CancellationToken);
                if (string.IsNullOrWhiteSpace(scriptContent.Data))
                    continue;
                var syntaxTree = CSharpSyntaxTree.ParseText(scriptContent.Data, parseOptions, additionalScript,
                    cancellationToken: options.CancellationToken);
                syntaxTreeList.Add(syntaxTree);
            }
        }

        var metadataReferences = await _referenceResolver.ResolveMetadataReferences(options, true);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication,
            optimizationLevel: options.Configuration, nullableContextOptions: NullableContextOptions.Annotations,
            allowUnsafe: true);
        compilationOptions.EnableReferencesSupersedeLowerVersions();

        var compilation = CSharpCompilation.Create(assemblyName, syntaxTreeList, metadataReferences, compilationOptions);
        Guard.NotNull(compilation);

        ISourceGenerator[]? generators = null;
        if (options.EnableSourceGeneratorSupport)
        {
            var analyzerReferences = await _referenceResolver.ResolveAnalyzerReferences(options);
            generators = analyzerReferences
                .Select(_ => new
                {
                    Generators = _.GetGenerators(LanguageNames.CSharp)
                })
                .Where(_ => _.Generators.HasValue())
                .SelectMany(_ => _.Generators)
                .ToArray();
        }
        if (generators.IsNullOrEmpty())
        {
            return await compilation.GetCompilationAssemblyResult(options.CancellationToken);
        }
        var generatorDriver = CSharpGeneratorDriver.Create(generators);
        generatorDriver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _, options.CancellationToken);
        return await updatedCompilation.GetCompilationAssemblyResult(options.CancellationToken);
    }
}
