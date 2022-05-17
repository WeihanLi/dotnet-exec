// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace Exec;

internal static class InternalHelper
{
    public static readonly HashSet<string> SpecialConsoleDiagnosticIds = new() { "CS5001", "CS0028" };
    public static readonly HashSet<string> GlobalUsingFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "GlobalUsing.cs", 
        "GlobalUsings.cs",
        "Imports.cs",
        "_Imports.cs"
    };
}
