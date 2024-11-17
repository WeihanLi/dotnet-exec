// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Contracts;

public sealed class ConfigProfile
{
    public HashSet<string> Usings
    {
        get;
        init => field = Guard.NotNull(value);
    } = new(StringComparer.Ordinal);

    public HashSet<string> References
    {
        get;
        init => field = Guard.NotNull(value);
    } = new(StringComparer.Ordinal);

    public bool IncludeWebReferences { get; set; }

    public bool IncludeWideReferences { get; set; }

    public string? EntryPoint { get; set; }

    public string[]? DefaultEntryMethods { get; set; }

    public bool EnablePreviewFeatures { get; set; }
}
