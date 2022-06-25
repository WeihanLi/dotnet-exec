// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

await using var serviceProvider = new ServiceCollection()
    .RegisterApplicationServices(args)
    .BuildServiceProvider();
var command = ExecOptions.GetCommand();
command.Handler = serviceProvider.GetRequiredService<ICommandHandler>();
await command.InvokeAsync(args);
