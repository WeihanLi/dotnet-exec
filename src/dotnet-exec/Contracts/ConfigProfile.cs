// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Contracts;

public sealed class ConfigProfile
{
    private HashSet<string> _usings = new();

    public HashSet<string> Usings
    {
        get => _usings;
        set => _usings = value ?? new();
    }

    private HashSet<string> _references = new();

    public HashSet<string> References
    {
        get => _references;
        set => _references = value ?? new();
    }

    public bool IncludeWebReferences { get; set; }

    public bool IncludeWideReferences { get; set; }

    public string? EntryPoint { get; set; }

    public bool EnablePreviewFeatures { get; set; }
}
