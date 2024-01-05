// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

await using var serviceProvider = new ServiceCollection()
    .RegisterApplicationServices(args)
    .BuildServiceProvider();
var command = ExecOptions.GetCommand();
command.Initialize(serviceProvider);
var index = Array.IndexOf(args, "--");
if (index > -1 && index < args.Length)
{
    var normalizedArgs = args[..index];
    Helper.CommandArguments = args[(index + 1)..];
    return await command.InvokeAsync(normalizedArgs);
}
return await command.InvokeAsync(args);
