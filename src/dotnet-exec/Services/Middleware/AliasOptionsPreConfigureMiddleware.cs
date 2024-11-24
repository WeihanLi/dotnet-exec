// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Contracts;

namespace Exec.Services.Middleware;
internal sealed class AliasOptionsPreConfigureMiddleware
        (AppConfiguration appConfiguration, ILogger logger)
    : IOptionsPreConfigureMiddleware
{
    public Task InvokeAsync(ExecOptions context, Func<ExecOptions, Task> next)
    {
        if (appConfiguration.Aliases.TryGetValue(context.Script, out var aliasValue))
        {
            context.Script = aliasValue;
            logger.LogDebug("Script replaced to alias value : {AliasValue}", aliasValue);
        }
        return Task.CompletedTask;
    }
}
