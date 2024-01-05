// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

[System.Diagnostics.DebuggerDisplay("framework: {Reference}")]
public sealed record FrameworkReference(string FrameworkName) : IReference
{
    public string Reference => FrameworkName;
    public ReferenceType ReferenceType => ReferenceType.FrameworkReference;
}

