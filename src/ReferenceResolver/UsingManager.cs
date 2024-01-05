// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

public static class UsingManager
{
    public static HashSet<string> GetUsings(
        ICollection<string> usings, string? frameworkOfImplicitUsing = null
        )
    {
        var usingList = new HashSet<string>(
            string.IsNullOrEmpty(frameworkOfImplicitUsing)
            ? Array.Empty<string>()
            : FrameworkReferenceResolver.GetImplicitUsings(frameworkOfImplicitUsing),
            StringComparer.Ordinal
        );

        if (usings is { Count: > 0 })
        {
            foreach (var @using in usings.Where(u => !u.StartsWith('-')))
            {
                usingList.Add(@using);
            }
            foreach (var @using in usings.Where(u => u.StartsWith('-')))
            {
                var usingToRemove = @using[1..].Trim();
                usingList.Remove(usingToRemove);
                usingList.Remove(@using);
                if (!usingToRemove.StartsWith("global::", StringComparison.Ordinal))
                {
                    usingList.Remove($"global::{usingToRemove}");
                }
            }
        }

        return usingList;
    }

    public static string GetGlobalUsingsCodeText(ICollection<string> usings,
        string? frameworkOfImplicitUsing = null)
    {
        var usingList = GetUsings(usings, frameworkOfImplicitUsing);
        var usingText = usingList.Select(x => $"global using {x};").StringJoin(Environment.NewLine);
        return usingText;
    }
}
