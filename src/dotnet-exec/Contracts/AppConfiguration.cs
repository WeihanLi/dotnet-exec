// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Contracts;
public sealed class AppConfiguration
{
    public static readonly AppConfiguration Default = new();

    // theme settings ...
    public Dictionary<string, string> Aliases { get; set; } = new()
    {
        { "new-guid", "System.Guid.NewGuid()" },
#if NET8_0_OR_GREATER
        { "now", "System.TimeProvider.System.GetLocalNow()" },
        { "date", "System.TimeProvider.System.GetLocalNow()" },
#endif
    };
}
