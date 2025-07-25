﻿// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Contracts;
using System.Text.Json;

if (args is ["--info"] or ["info"])
{
    var info = new InfoModel();
    Console.WriteLine(JsonSerializer.Serialize(info, JsonHelper.WriteIntendedUnsafeEncoderOptions));
    return 0;
}

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
    return await command.Parse(normalizedArgs).InvokeAsync(ApplicationHelper.ExitToken);
}

return await command.Parse(args).InvokeAsync(ApplicationHelper.ExitToken);

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal static partial class Program { }
