// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WeihanLi.Common.Models;

namespace Exec.Services;

public sealed class SimpleCodeCompiler(
        IRefResolver referenceResolver,
        IAdditionalScriptContentFetcher scriptContentFetcher,
        IParseOptionsPipeline parseOptionsPipeline,
        ICompilationOptionsPipeline compilationOptionsPipeline
        )
    : ICodeCompiler
{
    public async Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        var assemblyName = $"{Helper.ApplicationName}_{Guid.NewGuid():N}.dll";
        var globalUsingCode = Helper.GetGlobalUsingsCodeText(options);
        var parseOptions = new CSharpParseOptions(options.GetLanguageVersion());
        parseOptions = parseOptionsPipeline.Configure(parseOptions, options);
        var globalUsingSyntaxTree = CSharpSyntaxTree.ParseText(globalUsingCode, parseOptions, "__GlobalUsings.cs",
            cancellationToken: options.CancellationToken);
        var path = "__Script.cs";

        if (File.Exists(options.Script))
        {
            path = Path.GetFullPath(options.Script);
            if (string.IsNullOrEmpty(code))
            {
                code = await File.ReadAllTextAsync(options.Script, options.CancellationToken).ConfigureAwait(false);
            }
        }

        if (string.IsNullOrEmpty(code))
        {
            throw new InvalidOperationException("Code to compile can not be empty");
        }
        
        var scriptSyntaxTree =
            CSharpSyntaxTree.ParseText(code, parseOptions, path, cancellationToken: options.CancellationToken);
        var syntaxTreeList = new List<SyntaxTree>() { globalUsingSyntaxTree, scriptSyntaxTree, };
        if (options.AdditionalScripts.HasValue())
        {
            foreach (var additionalScript in options.AdditionalScripts)
            {
                var scriptContent =
                    await scriptContentFetcher.FetchContent(additionalScript, options.CancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(scriptContent.Data))
                    continue;
                var syntaxTree = CSharpSyntaxTree.ParseText(scriptContent.Data, parseOptions, additionalScript,
                    cancellationToken: options.CancellationToken);
                syntaxTreeList.Add(syntaxTree);
            }
        }

        var metadataReferences = await referenceResolver.ResolveMetadataReferences(options, true)
            .ConfigureAwait(false);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication,
            optimizationLevel: options.Configuration, nullableContextOptions: NullableContextOptions.Annotations,
            allowUnsafe: true);
        compilationOptions.EnableReferencesSupersedeLowerVersions();
        compilationOptions = compilationOptionsPipeline.Configure(compilationOptions, options);
        
        var compilation = CSharpCompilation.Create(assemblyName, syntaxTreeList, metadataReferences, compilationOptions);
        Guard.NotNull(compilation);

        ISourceGenerator[]? generators = null;
        if (options.EnableSourceGeneratorSupport)
        {
            var analyzerReferences = await referenceResolver.ResolveAnalyzerReferences(options)
                .ConfigureAwait(false);
            generators = analyzerReferences
                .Select(r => new
                {
                    Generators = r.GetGenerators(LanguageNames.CSharp),
                })
                .Where(x => x.Generators.HasValue())
                .SelectMany(x => x.Generators)
                .ToArray();
        }
        if (generators.IsNullOrEmpty())
        {
            return await compilation.GetCompilationAssemblyResult(options.CancellationToken).ConfigureAwait(false);
        }
        var generatorDriver = CSharpGeneratorDriver.Create(generators);
        generatorDriver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _, options.CancellationToken);
        return await updatedCompilation.GetCompilationAssemblyResult(options.CancellationToken).ConfigureAwait(false);
    }
}
