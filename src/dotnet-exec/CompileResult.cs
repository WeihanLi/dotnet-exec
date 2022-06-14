// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace Exec;

public sealed class CompileResult
{
    public CompileResult(Compilation compilation, EmitResult emitResult, MemoryStream stream)
    {
        Compilation = compilation;
        EmitResult = emitResult;
        Stream = stream;
    }

    public Compilation Compilation { get; }
    public EmitResult EmitResult { get; }

    public MemoryStream Stream { get; }
}
