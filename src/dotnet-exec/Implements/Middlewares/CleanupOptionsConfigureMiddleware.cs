// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Implements.Middlewares;

public sealed class CleanupOptionsConfigureMiddleware : IOptionsConfigureMiddleware
{
    public async Task Execute(ExecOptions options, Func<ExecOptions, Task> next)
    {
        await next(options);
        // Cleanup references
        var referenceToRemoved = options.References.Where(r => r.StartsWith('-')).ToArray();
        foreach (var reference in referenceToRemoved)
        {
            var referenceToRemove = reference[1..];
            options.References.Remove(referenceToRemove);
            options.References.Remove(reference);
        }
    }
}
