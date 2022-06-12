// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;

var command = ExecOptions.GetCommand();
await new CommandLineBuilder(command)
    .UseDefaults()
    .UseHost(hostBuilder =>
    {
        hostBuilder.ConfigureServices((_, services) =>
        {
            services.RegisterApplicationServices(args);
        });
    })
    .AddMiddleware(invocationContext =>
    {
        var serviceProvider = invocationContext.BindingContext.GetService<IHost>()?.Services
                              ?? invocationContext.BindingContext;
        var commandHandler = serviceProvider.GetService<ICommandHandler>();
        if (command.Handler is null && commandHandler != null)
        {
            command.Handler = commandHandler;
        }
    })
    .Build()
    .InvokeAsync(args);
