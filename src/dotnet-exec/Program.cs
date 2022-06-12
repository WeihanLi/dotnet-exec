// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.CommandLine.Builder;
using System.CommandLine.Hosting;

var command = ExecOptions.GetCommand();
command.SetHandler(invocationContext => invocationContext.GetHost()
    .Services.GetRequiredService<CommandHandler>()
    .InvokeAsync(invocationContext));
await new CommandLineBuilder(command)
    .UseHost(hostBuilder =>
    {
        hostBuilder.ConfigureServices((_, services) =>
        {
            services.RegisterApplicationServices(args);
        });
    })
    .Build()
    .InvokeAsync(args);
