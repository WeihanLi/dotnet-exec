// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using System.Globalization;
using WeihanLi.Common.Extensions;
using WeihanLi.Common.Models;

namespace Exec.Services;

public sealed class CSharpScriptCompilerExecutor(IRefResolver referenceResolver,
        IAdditionalScriptContentFetcher scriptContentFetcher, ILogger logger)
    : ICodeCompiler, ICodeExecutor
{
    public async Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null)
    {
        var references = await referenceResolver.ResolveMetadataReferences(options, false);
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
                var additionalScriptCode = await scriptContentFetcher.FetchContent(additionalScript, options.CancellationToken);
                if (additionalScriptCode.IsSuccess())
                {
                    script = script.ContinueWith(additionalScriptCode.Data, scriptOptions);
                }
            }
        }
        script = script.ContinueWith(code, scriptOptions);
        var diagnostics = script.Compile(options.CancellationToken);
        if (diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error))
        {
            return Result.Fail<CompileResult>(diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage(CultureInfo.InvariantCulture))
                .StringJoin(Environment.NewLine));
        }
        var compileResult = new CompileResult(null!, null!, null!);
        compileResult.SetProperty(nameof(Script), script);
        compileResult.SetProperty(nameof(Diagnostic), diagnostics);
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
            logger.LogError(state.Exception, "Execute script exception");
        }
        return new Result<int>()
        {
            Data = state.Exception is null ? 0 : (int)ResultStatus.ProcessFail,
            Status = state.Exception is null ? ResultStatus.Success : ResultStatus.ProcessFail,
            Msg = state.Exception?.ToString()
        };
    }
}
