// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Contracts;
using Exec.Services;
using System.Diagnostics;
using System.Text.Json;
using WeihanLi.Common.Models;

namespace Exec;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class CommandHandler(ILogger logger,
        ICompilerFactory compilerFactory,
        IExecutorFactory executorFactory,
        IScriptContentFetcher scriptContentFetcher,
        IConfigProfileManager profileManager,
        IOptionsPreConfigurePipeline optionsPreConfigurePipeline,
        IOptionsConfigurePipeline optionsConfigurePipeline,
        IRepl repl
        )
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

        return await Execute(options);
    }

    public async Task<int> Execute(ExecOptions options)
    {
        logger.LogDebug("options: {options}", JsonSerializer.Serialize(options, JsonHelper.WriteIntendedUnsafeEncoderOptions));

        // try to read script content from stdin
        var inputText = string.Empty;
        if (ConsoleHelper.HasStandardInput())
        {
            logger.LogDebug("Try to read stdin");
            inputText = (await Console.In.ReadToEndAsync(options.CancellationToken)).Trim();
            logger.LogDebug("Content ({StdinContent}) read from stdin", inputText);
        }

        if (string.IsNullOrEmpty(inputText) && options.Script.IsNullOrEmpty())
        {
            // stdin is empty and no script provided
            // start REPL
            logger.LogDebug("No script provided and no redirected input found, start REPL");
            await repl.RunAsync(options);
            return 0;
        }
        
        // stdin is not empty
        if (!string.IsNullOrEmpty(inputText))
        {
            logger.LogDebug("Script( {Script} ) read from stdin.", inputText);
            if (!string.IsNullOrWhiteSpace(options.Script))
            {
                var script = options.Script;
                options.AdditionalScripts = [script, ..options.AdditionalScripts ?? []];
            }
            options.Script = inputText;
        }

        // pre-configure pipeline before fetch script content
        await optionsPreConfigurePipeline.Execute(options);
        logger.LogDebug("options after PreConfigure: {options}", JsonSerializer.Serialize(options, JsonHelper.WriteIntendedUnsafeEncoderOptions));

        // fetch the script
        var fetchResult = await scriptContentFetcher.FetchContent(options);
        if (!fetchResult.IsSuccess())
        {
            logger.LogError(fetchResult.Msg);
            return ExitCodes.FetchError;
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
        var compileElapsed = Stopwatch.GetElapsedTime(compileStartTime);
        logger.LogDebug("Compile elapsed: {elapsed}", compileElapsed);

        if (!compileResult.IsSuccess())
        {
            logger.LogError($"Compile error:{Environment.NewLine}{compileResult.Msg}");
            return ExitCodes.CompileError;
        }

        // output compiled assembly if needed
        if (!string.IsNullOrEmpty(options.CompileOutput))
        {
            var dir = Path.GetDirectoryName(options.CompileOutput);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var outputPath = options.CompileOutput;
            if (string.IsNullOrEmpty(Path.GetFileName(outputPath)))
            {
                outputPath = Path.Combine(dir ?? Environment.CurrentDirectory, $"{compileResult.Data!.Compilation.AssemblyName}.dll");
            }

            var originalPosition = compileResult.Data!.Stream.Position;
            compileResult.Data.Stream.Seek(0, SeekOrigin.Begin);

            await using var fs = File.Create(outputPath);
            await compileResult.Data.Stream.CopyToAsync(fs);

            compileResult.Data!.Stream.Position = originalPosition;
        }

        // return if dry-run
        if (options.DryRun) return ExitCodes.Success;

        Guard.NotNull(compileResult.Data);
        // execute
        var executor = executorFactory.GetExecutor(options.ExecutorType);
        try
        {
            var executeStartTime = Stopwatch.GetTimestamp();
            var executeTask = executor.Execute(compileResult.Data, options);
            var executeResult =
                    options.Timeout > 0
                        ? await executeTask.WaitAsync(TimeSpan.FromSeconds(options.Timeout.Value),
                            options.CancellationToken)
                        : await executeTask
                ;
            if (!executeResult.IsSuccess())
            {
                logger.LogError($"Execute error:{Environment.NewLine}{executeResult.Msg}");
                return ExitCodes.ExecuteError;
            }

            var elapsed = Stopwatch.GetElapsedTime(executeStartTime);
            logger.LogDebug("Execute elapsed: {elapsed}", elapsed);

            return Environment.ExitCode is not 0 ? Environment.ExitCode : executeResult.Data;
        }
        catch (TimeoutException timeoutException)
        {
            logger.LogError(timeoutException, "Timeout({TimeoutSeconds}) when executing script", options.Timeout);
            return ExitCodes.ExecuteTimeout;
        }
        catch (OperationCanceledException) when (options.CancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Execution cancelled...");
            return ExitCodes.OperationCancelled;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Execute code exception");
            return ExitCodes.ExecuteException;
        }
        finally
        {
            // wait for the console flush
            await Console.Out.FlushAsync();
        }
    }
}
