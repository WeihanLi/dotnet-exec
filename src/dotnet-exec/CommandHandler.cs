// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.CommandLine.Invocation;
using System.Runtime.Loader;
using System.Text.Json;
using WeihanLi.Common.Models;

namespace Exec;

public sealed class CommandHandler : ICommandHandler
{
    private readonly ILogger _logger;
    private readonly ICompilerFactory _compilerFactory;
    private readonly ICodeExecutor _executor;
    private readonly HttpClient _httpClient;

    public CommandHandler(ILogger logger, ICompilerFactory compilerFactory, ICodeExecutor executor, HttpClient httpClient)
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
        string? sourceText;
        if (options.ScriptFile.IsNullOrWhiteSpace())
        {
            _logger.LogError("The file {ScriptFile} can not be empty", options.ScriptFile);
            return -1;
        }
        if (options.ScriptFile.StartsWith("http://") || options.ScriptFile.StartsWith("https://"))
        {
            sourceText = await _httpClient.GetStringAsync(options.ScriptFile, options.CancellationToken);
        }
        else
        {
            if (!File.Exists(options.ScriptFile))
            {
                _logger.LogError("The file {ScriptFile} does not exists", options.ScriptFile);
                return -1;
            }
            sourceText = await File.ReadAllTextAsync(options.ScriptFile, options.CancellationToken);
        }

        // 2. compile assembly
        var compiler = _compilerFactory.GetCompiler(options.CompilerType);
        var compileResult = await compiler.Compile(options, sourceText);
        if (!compileResult.IsSuccess())
        {
            _logger.LogError($"Compile error:{Environment.NewLine}{compileResult.Msg}");
            return -2;
        }
        Guard.NotNull(compileResult.Data);
        // 3. execute
        var assemblyLoadContext =
                // new CustomLoadContext(compileResult.Data.References)
                new AssemblyLoadContext(InternalHelper.ApplicationName)
            ;
        compileResult.Data.Stream.Seek(0, SeekOrigin.Begin);
        var assembly = assemblyLoadContext.LoadFromStream(compileResult.Data.Stream);

        try
        {
            using var scope = assemblyLoadContext.EnterContextualReflection();
            var executeResult = await _executor.Execute(assembly, options);
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
}
