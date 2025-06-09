// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using ReferenceResolver;

namespace Exec.Services.Middleware;

public class NewDirectivesOptionsConfigureMiddleware : IOptionsConfigureMiddleware
{
    public Task InvokeAsync(ExecOptions context, Func<ExecOptions, Task> next)
    {
        var splits = context.Script.Split('\n');
        foreach (var line in splits)
        {
            if (line.StartsWith("#:", StringComparison.Ordinal))
            {
                // new directive
                var trimmedDirective = line[2..].Trim();
                if (trimmedDirective.StartsWith("package ", StringComparison.Ordinal))
                {
                    var packageReference = trimmedDirective["package ".Length..];
                    var packageSplits = packageReference.Split('@');
                    if (packageSplits.Length == 1)
                    {
                        context.References.Add(NuGetReference.Parse($"nuget: {packageSplits[0]}")
                            .ReferenceWithSchema());
                    }
                    else if (packageSplits.Length == 2)
                    {
                        context.References.Add(NuGetReference.Parse($"nuget: {packageSplits[0]},{packageSplits[1]}")
                            .ReferenceWithSchema());
                    }
                }
            }
        }
        return next(context);
    }
}
