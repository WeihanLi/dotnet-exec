// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace Exec;

public sealed class CompileResult
{
    public CompileResult(string assemblyName, string[] references, MemoryStream stream)
    {
        AssemblyName = assemblyName;
        References = references;
        Stream = stream;
    }

    public string AssemblyName { get; }

    public string[] References { get; }

    public MemoryStream Stream { get; }
}
