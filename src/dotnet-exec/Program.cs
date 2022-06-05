// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

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
services.RegisterApplicationServices(args);
await using var provider = services.BuildServiceProvider();
command.Handler = provider.GetRequiredService<CommandHandler>();
await command.InvokeAsync(args);
