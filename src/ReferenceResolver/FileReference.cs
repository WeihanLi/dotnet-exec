// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace ReferenceResolver;

[System.Diagnostics.DebuggerDisplay("{Reference}")]
public sealed record FileReference(string FilePath) : IReference
{
    public string Reference => Path.GetFullPath(FilePath);
    public ReferenceType ReferenceType => ReferenceType.LocalFile;
}
