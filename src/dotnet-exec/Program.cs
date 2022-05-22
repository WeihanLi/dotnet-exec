// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var command = new Command("dotnet-exec");
foreach (var item in ExecOptions.GetArguments())
{
    command.AddArgument(item);
}
foreach (var option in ExecOptions.GetOptions())
{
    command.AddOption(option);
}

var debugEnabled = args.Contains("--debug", StringComparer.OrdinalIgnoreCase);
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(debugEnabled ? LogLevel.Debug : LogLevel.Warning);
});
services.AddSingleton(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger("dotnet-exec"));
if (args.Contains("--advanced") || args.Contains("-a"))
{
    services.AddSingleton<ICodeCompiler, AdvancedCodeCompiler>();    
}
else
{
    services.AddSingleton<ICodeCompiler, SimpleCodeCompiler>();    
}
services.AddSingleton<ICodeExecutor, CodeExecutor>();
services.AddSingleton<CommandHandler>();
services.AddSingleton<HttpClient>();

await using var provider = services.BuildServiceProvider();
command.Handler = provider.GetRequiredService<CommandHandler>();
await command.InvokeAsync(args);
