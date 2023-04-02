// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WeihanLi.Common.Models;

namespace Exec;

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
                var syntaxTree = CSharpSyntaxTree.ParseText(scriptContent.Data, parseOptions, additionalScript, null,
                    options.CancellationToken);
                syntaxTreeList.Add(syntaxTree);
            }
        }

        var metadataReferences = await _referenceResolver.ResolveMetadataReferences(options, true);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication,
            optimizationLevel: options.Configuration, nullableContextOptions: NullableContextOptions.Annotations,
            allowUnsafe: true);
        compilationOptions.EnableReferencesSupersedeLowerVersions();

        var compilation = CSharpCompilation.Create(assemblyName, syntaxTreeList, metadataReferences, compilationOptions);
        return await Guard.NotNull(compilation).GetCompilationAssemblyResult(options.CancellationToken);
    }
}
