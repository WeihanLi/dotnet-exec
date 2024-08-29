// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Contracts;

public sealed class ConfigProfile
{
    private readonly HashSet<string> _usings = new(StringComparer.Ordinal);

    public HashSet<string> Usings
    {
        get => _usings;
        init => _usings = Guard.NotNull(value);
    }

    private readonly HashSet<string> _references = new(StringComparer.Ordinal);

    public HashSet<string> References
    {
        get => _references;
        init => _references = Guard.NotNull(value);
    }

    public bool IncludeWebReferences { get; set; }

    public bool IncludeWideReferences { get; set; }

    public string? EntryPoint { get; set; }

    public string[]? DefaultEntryMethods { get; set; }

    public bool EnablePreviewFeatures { get; set; }
}
