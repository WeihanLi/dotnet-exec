// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using ReferenceResolver;

namespace Exec.Services;

public sealed class RunFileTransformer : IScriptTransformer
{
    public HashSet<string> SupportedExtensions { get; } = ["*"];
    public Task InvokeAsync(ExecOptions context, string[] scriptLines)
    {
        foreach (var line in scriptLines)
        {
            if (string.IsNullOrEmpty(line))
                continue;
            
            if (line.StartsWith("#!", StringComparison.Ordinal))
                continue;
            
            if (line.StartsWith("#:", StringComparison.Ordinal))
            {
                // new directive
                var trimmedDirective = line[2..].Trim();
                if (trimmedDirective.StartsWith("package ", StringComparison.Ordinal))
                {
                    var packageReference = trimmedDirective["package ".Length..].Trim('"', ' ');
                    var packageSplits = packageReference.Split('@');
                    if (packageSplits.Length == 1)
                    {
                        var reference = new NuGetReference(packageSplits[0]);
                        context.References.Add(reference.ReferenceWithSchema());
                    }
                    else if (packageSplits.Length == 2)
                    {
                        var reference = new NuGetReference(packageSplits[0], packageSplits[1]);
                        context.References.Add(reference.ReferenceWithSchema());
                    }
                } else if (trimmedDirective.StartsWith("sdk", StringComparison.OrdinalIgnoreCase))
                {
                    var sdkName = trimmedDirective["sdk".Length..].Trim('"', ' ');
                    var frameworkReference = sdkName switch
                    {
                        "Microsoft.NET.Sdk" => null,
                        "Microsoft.NET.Sdk.Windows" => FrameworkReferenceResolver.FrameworkNames.WindowsDesktop,
                        _ => FrameworkReferenceResolver.FrameworkNames.Web
                    };
                    if (!string.IsNullOrEmpty(frameworkReference))
                    {
                        context.References.Add(new FrameworkReference(frameworkReference).ReferenceWithSchema());
                    }
                }
            }
            else
            {
                break;
            }
        }
        
        return Task.CompletedTask;
    }
}
