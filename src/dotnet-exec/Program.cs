// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

var command = ExecOptions.GetCommand(InternalHelper.ApplicationName);
var services = new ServiceCollection();
services.RegisterApplicationServices(args);
await using var provider = services.BuildServiceProvider();
command.Handler = provider.GetRequiredService<CommandHandler>();
await command.InvokeAsync(args);
