// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using WeihanLi.Common.Extensions;
using WeihanLi.Common.Models;

namespace Exec;

public sealed class CSharpScriptCompilerExecutor : ICodeCompiler, ICodeExecutor
{
    private readonly IRefResolver _referenceResolver;
    private readonly ILogger _logger;

    public CSharpScriptCompilerExecutor(IRefResolver referenceResolver, ILogger logger)
    {
        _referenceResolver = referenceResolver;
        _logger = logger;
    }

    public async Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null)
    {
        var assemblyLocations = await _referenceResolver.ResolveReferences(options, false);
        var references = assemblyLocations
            .Select(l =>
            {
                try
                {
                    // load managed assembly only
                    _ = AssemblyName.GetAssemblyName(l);

                    return MetadataReference.CreateFromFile(l, MetadataReferenceProperties.Assembly);
                }
                catch
                {
                    return null;
                }
            })
            .WhereNotNull();
        var scriptOptions = ScriptOptions.Default
                .WithReferences(references)
                .WithOptimizationLevel(options.Configuration)
                .WithAllowUnsafe(true)
                .WithLanguageVersion(options.LanguageVersion)
            ;
        var globalUsingText = Helper.GetGlobalUsingsCodeText(options);
        var state = await CSharpScript.RunAsync(globalUsingText, scriptOptions);
        var script = state.Script.ContinueWith(code, scriptOptions);
        var result = Result.Success(new CompileResult(null!, null!, null!));
        result.Data?.SetProperty<Script<object>>(nameof(Script), script);
        return result;
    }

    public async Task<Result> Execute(CompileResult compileResult, ExecOptions options)
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
        return new Result()
        {
            Status = state.Exception is null ? ResultStatus.Success : ResultStatus.ProcessFail,
            Msg = state.Exception?.ToString()
        };
    }
}
