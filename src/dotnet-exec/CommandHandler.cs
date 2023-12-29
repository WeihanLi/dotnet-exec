// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Contracts;
using System.Diagnostics;
using System.Text.Json;
using WeihanLi.Common.Models;

namespace Exec;

public sealed class CommandHandler(ILogger logger,
        ICompilerFactory compilerFactory,
        IExecutorFactory executorFactory,
        IScriptContentFetcher scriptContentFetcher,
        IConfigProfileManager profileManager,
        IOptionsConfigurePipeline optionsConfigurePipeline)
    : ICommandHandler
{
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
            profile = await profileManager.GetProfile(profileName);
            if (profile is null)
            {
                logger.LogDebug("The config profile({profileName}) not found, ignore profile", profileName);
            }
        }
        options.BindCommandLineArguments(parseResult, profile);
        options.CancellationToken = context.GetCancellationToken();
        if (options.DebugEnabled)
        {
            logger.LogDebug("options: {options}", JsonSerializer.Serialize(options, JsonSerializerOptionsHelper.WriteIndented));
        }

        return await Execute(options);
    }

    public async Task<int> Execute(ExecOptions options)
    {
        if (options.Script.IsNullOrWhiteSpace())
        {
            logger.LogError("The script {ScriptFile} can not be empty", options.Script);
            return -1;
        }
        // fetch script
        var fetchResult = await scriptContentFetcher.FetchContent(options);
        if (!fetchResult.IsSuccess())
        {
            logger.LogError(fetchResult.Msg);
            return -1;
        }

        // execute options configure pipeline
        await optionsConfigurePipeline.Execute(options);

        logger.LogDebug("CompilerType: {CompilerType} \nExecutorType: {ExecutorType} \nReferences: {References} \nUsings: {Usings}",
            options.CompilerType,
            options.ExecutorType,
            options.References.StringJoin(";"),
            options.Usings.StringJoin(";"));

        // compile assembly
        var sourceText = fetchResult.Data;
        var compiler = compilerFactory.GetCompiler(options.CompilerType);
        var compileStartTime = Stopwatch.GetTimestamp();
        var compileResult = await compiler.Compile(options, sourceText);
        var compileElapsed = ProfilerHelper.GetElapsedTime(compileStartTime);
        logger.LogDebug("Compile elapsed: {elapsed}", compileElapsed);

        if (!compileResult.IsSuccess())
        {
            logger.LogError($"Compile error:{Environment.NewLine}{compileResult.Msg}");
            return -2;
        }

        Guard.NotNull(compileResult.Data);
        // execute
        var executor = executorFactory.GetExecutor(options.ExecutorType);
        try
        {
            var executeStartTime = Stopwatch.GetTimestamp();
            var executeResult = await executor.Execute(compileResult.Data, options);
            if (!executeResult.IsSuccess())
            {
                logger.LogError($"Execute error:{Environment.NewLine}{executeResult.Msg}");
                return -3;
            }
            var elapsed = ProfilerHelper.GetElapsedTime(executeStartTime);
            logger.LogDebug("Execute elapsed: {elapsed}", elapsed);

            // wait for console flush
            await Console.Out.FlushAsync();

            return executeResult.Data;
        }
        catch (OperationCanceledException) when (options.CancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Execution cancelled...");
            return -998;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Execute code exception");
            return -999;
        }
    }
}
