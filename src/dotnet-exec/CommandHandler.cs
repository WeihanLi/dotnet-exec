// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.CommandLine.Invocation;
using WeihanLi.Common.Models;

namespace Exec;

public sealed class CommandHandler : ICommandHandler
{
    private readonly ILogger _logger;
    private readonly ICodeCompiler _compiler;
    private readonly ICodeExecutor _executor;

    public CommandHandler(ILogger logger, ICodeCompiler compiler, ICodeExecutor executor)
    {
        _logger = logger;
        _compiler = compiler;
        _executor = executor;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, args) =>
        {
            cts.Cancel();
            Thread.Sleep(1000);
        };
        var parseResult = context.ParseResult;

        // 1. options binding
        var options = new ExecOptions();
        options.BindCommandLineArguments(parseResult);
        options.CancellationToken = cts.Token;

        // 2. construct project
        if (!File.Exists(options.ScriptFile))
        {
            _logger.LogError($"The file {options.ScriptFile} does not exists");
            return -1;
        }
        return await Execute(options);
    }

    public async Task<int> Execute(ExecOptions options)
    {
        var sourceText = await File.ReadAllTextAsync(options.ScriptFile).ConfigureAwait(false);
        // 3. compile and run
        var compileResult = await _compiler.Compile(options, sourceText);
        if (!compileResult.IsSuccess())
        {
            _logger.LogError($"Compile error:{Environment.NewLine}{compileResult.Msg}");
            return -2;
        }
        Guard.NotNull(compileResult.Data);
        var executeResult = await _executor.Execute(compileResult.Data, options);
        if (!executeResult.IsSuccess())
        {
            _logger.LogError($"Execute error:{Environment.NewLine}{executeResult.Msg}");
            return -3;
        }
        return 0;
    }
}
