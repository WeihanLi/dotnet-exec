// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.CommandLine.Invocation;
using System.Text.Json;
using WeihanLi.Common.Models;

namespace Exec;

public sealed class CommandHandler : ICommandHandler
{
    private readonly ILogger _logger;
    private readonly ICompilerFactory _compilerFactory;
    private readonly ICodeExecutor _executor;
    private readonly HttpClient _httpClient;

    public CommandHandler(ILogger logger, ICompilerFactory compilerFactory, ICodeExecutor executor,
        HttpClient httpClient)
    {
        _logger = logger;
        _compilerFactory = compilerFactory;
        _executor = executor;
        _httpClient = httpClient;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) =>
        {
            // ReSharper disable once AccessToDisposedClosure
            cts.Cancel();
            Thread.Sleep(1000);
        };
        var parseResult = context.ParseResult;

        // 1. options binding
        var options = new ExecOptions();
        options.BindCommandLineArguments(parseResult);
        options.CancellationToken = cts.Token;

        _logger.LogDebug("options: {options}", JsonSerializer.Serialize(options));

        return await Execute(options);
    }

    public async Task<int> Execute(ExecOptions options)
    {
        if (options.ScriptFile.IsNullOrWhiteSpace())
        {
            _logger.LogError("The file {ScriptFile} can not be empty", options.ScriptFile);
            return -1;
        }

        // fetch script
        var fetchResult = await FetchScriptContent(options.ScriptFile, options.CancellationToken);
        if (!fetchResult.IsSuccess())
        {
            _logger.LogError(fetchResult.Msg);
            return -1;
        }

        // compile assembly
        var sourceText = fetchResult.Data;
        var compiler = _compilerFactory.GetCompiler(options.CompilerType);
        var compileResult = await compiler.Compile(options, sourceText);
        if (!compileResult.IsSuccess())
        {
            _logger.LogError($"Compile error:{Environment.NewLine}{compileResult.Msg}");
            return -2;
        }

        Guard.NotNull(compileResult.Data);
        // execute
        try
        {
            var executeResult = await _executor.Execute(compileResult.Data, options);
            if (!executeResult.IsSuccess())
            {
                _logger.LogError($"Execute error:{Environment.NewLine}{executeResult.Msg}");
                return -3;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Execute code error");
            return -999;
        }
    }

    private async Task<Result<string>> FetchScriptContent(string scriptFile, CancellationToken cancellationToken)
    {
        string sourceText;
        if (scriptFile.StartsWith("code:") || scriptFile.StartsWith("text:"))
        {
            return Result.Success<string>(scriptFile[5..]);
        }
        try
        {
            if (Uri.TryCreate(scriptFile, UriKind.Absolute, out var uri) && !uri.IsFile)
            {
                var scriptUrl = uri.Host switch
                {
                    "github.com" => scriptFile
                        .Replace($"://{uri.Host}/", $"://raw.githubusercontent.com/")
                        .Replace("/blob/", "/")
                        .Replace("/tree/", "/"),
                    "gist.github.com" => scriptFile
                                             .Replace($"://{uri.Host}/", $"://gist.githubusercontent.com/")
                                         + "/raw",
                    _ => scriptFile
                };
                sourceText = await _httpClient.GetStringAsync(scriptUrl, cancellationToken);
            }
            else
            {
                if (!File.Exists(scriptFile))
                {
                    _logger.LogError("The file {ScriptFile} does not exists", scriptFile);
                    return Result.Fail<string>("File path not exits");
                }

                sourceText = await File.ReadAllTextAsync(scriptFile, cancellationToken);
            }
        }
        catch (Exception e)
        {
            return Result.Fail<string>($"Fail to fetch script content, {e}", ResultStatus.ProcessFail);
        }

        return Result.Success<string>(sourceText);
    }
}
