// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Services.Middlewares;

internal sealed class CleanupOptionsConfigureMiddleware : IOptionsConfigureMiddleware
{
    public async Task InvokeAsync(ExecOptions options, Func<ExecOptions, Task> next)
    {
        await next(options);
        // Cleanup references
        var referenceToRemoved = options.References.Where(r => r.StartsWith('-')).ToArray();
        foreach (var reference in referenceToRemoved)
        {
            var referenceToRemove = reference[1..].Trim();
            options.References.Remove(referenceToRemove);
            options.References.Remove(reference);
        }
    }
}
