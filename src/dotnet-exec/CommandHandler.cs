// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Contracts;
using System.Diagnostics;
using System.Text.Json;
using WeihanLi.Common.Models;

namespace Exec;

public sealed class CommandHandler : ICommandHandler
{
    private readonly ILogger _logger;
    private readonly ICompilerFactory _compilerFactory;
    private readonly IExecutorFactory _executorFactory;
    private readonly IScriptContentFetcher _scriptContentFetcher;
    private readonly IConfigProfileManager _profileManager;
    private readonly IOptionsConfigurePipeline _optionsConfigurePipeline;

    public CommandHandler(ILogger logger,
        ICompilerFactory compilerFactory,
        IExecutorFactory executorFactory,
        IScriptContentFetcher scriptContentFetcher,
        IConfigProfileManager profileManager,
        IOptionsConfigurePipeline optionsConfigurePipeline)
    {
        _logger = logger;
        _compilerFactory = compilerFactory;
        _executorFactory = executorFactory;
        _scriptContentFetcher = scriptContentFetcher;
        _profileManager = profileManager;
        _optionsConfigurePipeline = optionsConfigurePipeline;
    }

    public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var parseResult = context.ParseResult;

        // 1. options binding
        var options = new ExecOptions();
        var profileName = parseResult.GetValueForOption(ExecOptions.ConfigProfileOption);
        ConfigProfile? profile = null;
        if (profileName.IsNotNullOrEmpty())
        {
            profile = await _profileManager.GetProfile(profileName);
            if (profile is null)
            {
                _logger.LogDebug("The config profile({profileName}) not found, ignore profile", profileName);
            }
        }
        options.BindCommandLineArguments(parseResult, profile);
        options.CancellationToken = context.GetCancellationToken();
        if (options.DebugEnabled)
        {
            _logger.LogDebug("options: {options}", JsonSerializer.Serialize(options, JsonSerializerOptionsHelper.RelaxedJsonWriteIndentedWithEnumStringConverter));
        }

        return await Execute(options);
    }

    public async Task<int> Execute(ExecOptions options)
    {
        if (options.Script.IsNullOrWhiteSpace())
        {
            _logger.LogError("The script {ScriptFile} can not be empty", options.Script);
            return -1;
        }
        // fetch script
        var fetchResult = await _scriptContentFetcher.FetchContent(options);
        if (!fetchResult.IsSuccess())
        {
            _logger.LogError(fetchResult.Msg);
            return -1;
        }

        // execute options configure pipeline
        await _optionsConfigurePipeline.Execute(options);

        _logger.LogDebug("CompilerType: {CompilerType} \nExecutorType: {ExecutorType} \nReferences: {References} \nUsings: {Usings}",
            options.CompilerType,
            options.ExecutorType,
            options.References.StringJoin(";"),
            options.Usings.StringJoin(";"));

        // compile assembly
        var sourceText = fetchResult.Data;
        var compiler = _compilerFactory.GetCompiler(options.CompilerType);
        var compileStartTime = Stopwatch.GetTimestamp();
        var compileResult = await compiler.Compile(options, sourceText);
        var compileEndTime = Stopwatch.GetTimestamp();
        var compileElapsed = ProfilerHelper.GetElapsedTime(compileStartTime, compileEndTime);
        _logger.LogDebug("Compile elapsed: {elapsed}", compileElapsed);

        if (!compileResult.IsSuccess())
        {
            _logger.LogError($"Compile error:{Environment.NewLine}{compileResult.Msg}");
            return -2;
        }

        Guard.NotNull(compileResult.Data);
        // execute
        var executor = _executorFactory.GetExecutor(options.ExecutorType);
        try
        {
            var executeStartTime = Stopwatch.GetTimestamp();
            var executeResult = await executor.Execute(compileResult.Data, options);
            if (!executeResult.IsSuccess())
            {
                _logger.LogError($"Execute error:{Environment.NewLine}{executeResult.Msg}");
                return -3;
            }
            var elapsed = ProfilerHelper.GetElapsedTime(executeStartTime);
            _logger.LogDebug("Execute elapsed: {elapsed}", elapsed);

            // wait for console flush
            await Console.Out.FlushAsync();

            return executeResult.Data;
        }
        catch (OperationCanceledException) when (options.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Execution cancelled...");
            return -998;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Execute code exception");
            return -999;
        }
    }
}
