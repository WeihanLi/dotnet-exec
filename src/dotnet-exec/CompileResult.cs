// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using WeihanLi.Common.Abstractions;

namespace Exec;

public sealed class CompileResult : IProperties
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
    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    internal static readonly CompileResult Empty = new(null!, null!, null!);
}
