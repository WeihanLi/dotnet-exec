// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using WeihanLi.Common.Extensions;
using WeihanLi.Common.Models;

namespace Exec.Implements;

public sealed class CSharpScriptCompilerExecutor : ICodeCompiler, ICodeExecutor
{
    private readonly IRefResolver _referenceResolver;
    private readonly IAdditionalScriptContentFetcher _scriptContentFetcher;
    private readonly ILogger _logger;

    public CSharpScriptCompilerExecutor(IRefResolver referenceResolver, IAdditionalScriptContentFetcher scriptContentFetcher, ILogger logger)
    {
        _referenceResolver = referenceResolver;
        _scriptContentFetcher = scriptContentFetcher;
        _logger = logger;
    }

    public async Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null)
    {
        var references = await _referenceResolver.ResolveMetadataReferences(options, false);
        var scriptOptions = ScriptOptions.Default
                .WithReferences(references)
                .WithOptimizationLevel(options.Configuration)
                .WithAllowUnsafe(true)
                .WithLanguageVersion(options.GetLanguageVersion())
            ;
        var globalUsingText = Helper.GetGlobalUsingsCodeText(options);
        var state = await CSharpScript.RunAsync(globalUsingText, scriptOptions);
        var script = state.Script;
        if (options.AdditionalScripts.HasValue())
        {
            foreach (var additionalScript in options.AdditionalScripts)
            {
                var additionalScriptCode = await _scriptContentFetcher.FetchContent(additionalScript, options.CancellationToken);
                if (additionalScriptCode.IsSuccess())
                {
                    script = script.ContinueWith(additionalScriptCode.Data, scriptOptions);
                }
            }
        }
        script = script.ContinueWith(code, scriptOptions);
        var compileResult = new CompileResult(null!, null!, null!);
        compileResult.SetProperty(nameof(Script), script);
        return Result.Success(compileResult);
    }

    public async Task<Result<int>> Execute(CompileResult compileResult, ExecOptions options)
    {
        var script = compileResult.GetProperty<Script>(nameof(Script));
        Guard.NotNull(script);
        var state = await script.RunAsync(cancellationToken: options.CancellationToken);
        if (state.ReturnValue != null)
        {
            Console.WriteLine(CSharpObjectFormatter.Instance.FormatObject(state.ReturnValue));
        }
        if (state.Exception != null)
        {
            _logger.LogError(state.Exception, "Execute script exception");
        }
        return new Result<int>()
        {
            Data = state.Exception is null ? 0 : (int)ResultStatus.ProcessFail,
            Status = state.Exception is null ? ResultStatus.Success : ResultStatus.ProcessFail,
            Msg = state.Exception?.ToString()
        };
    }
}
