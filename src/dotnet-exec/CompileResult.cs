// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using WeihanLi.Common.Abstractions;

namespace Exec;

public sealed class CompileResult(Compilation compilation, EmitResult emitResult, MemoryStream stream)
    : IProperties
{
    public Compilation Compilation { get; } = compilation;
    public EmitResult EmitResult { get; } = emitResult;

    public MemoryStream Stream { get; } = stream;
    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();
}
