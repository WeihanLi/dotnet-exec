// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

await using var serviceProvider = new ServiceCollection()
    .RegisterApplicationServices(args)
    .BuildServiceProvider();
var command = ExecOptions.GetCommand();
command.Initialize(serviceProvider);
await command.InvokeAsync(args);
