// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace Exec;

public sealed class CompileResult
{
    public CompileResult(Compilation compilation, EmitResult emitResult, MemoryStream stream, string[] references)
    {
        Compilation = compilation;
        EmitResult = emitResult;
        Stream = stream;
        References = references;
    }

    public Compilation Compilation { get; }
    public EmitResult EmitResult { get; }

    public string[] References { get; }

    public MemoryStream Stream { get; }
}
