// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common.Models;

var command = new Command("dotnet-exec");

foreach (var item in ExecOptions.GetArguments())
{
    command.AddArgument(item);
}
foreach (var option in ExecOptions.GetOptions())
{
    command.AddOption(option);
}

var services = new ServiceCollection();
services.AddSingleton<ICodeCompiler, SimpleCodeCompiler>();
services.AddSingleton<ICodeExecutor, CodeExecutor>();
var provider = services.BuildServiceProvider();

command.SetHandler(async (ParseResult parseResult, IConsole console) =>
{
    // 1. options binding
    var options = new ExecOptions();
    options.BindCommandLineArguments(parseResult);
    // 2. construct project
    if (!File.Exists(options.ScriptFile))
    {
        console.Error.Write($"The file {options.ScriptFile} does not exists");
        return;
    }
    var sourceText = await File.ReadAllTextAsync(options.ScriptFile).ConfigureAwait(false);
    // 3. compile and run
    var compiler = provider.GetRequiredService<ICodeCompiler>();
    var compileResult = await compiler.Compile(sourceText, options);
    if (compileResult.Status != ResultStatus.Success)
    {
        throw new ArgumentException($"Compile error:{Environment.NewLine}{compileResult.Msg}");
    }
    Guard.NotNull(compileResult.Data);
    var executor = provider.GetRequiredService<ICodeExecutor>();    
    var executeResult = await executor.Execute(compileResult.Data, args, options);
    if (executeResult.Status != ResultStatus.Success)
    {
        throw new InvalidOperationException($"Execute error:{Environment.NewLine}{executeResult.Msg}");
    }
});

await command.InvokeAsync(args);
