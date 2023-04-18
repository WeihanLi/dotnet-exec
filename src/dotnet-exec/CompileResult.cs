// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

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
}
