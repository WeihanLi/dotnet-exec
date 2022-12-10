// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace ReferenceResolver;

[System.Diagnostics.DebuggerDisplay("framework: {Reference}")]
public sealed record FrameworkReference(string FrameworkName) : IReference
{
    public string Reference => FrameworkName;
    public ReferenceType ReferenceType => ReferenceType.FrameworkReference;
}

