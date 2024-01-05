// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

[System.Diagnostics.DebuggerDisplay("{Reference}")]
public sealed record FileReference(string FilePath) : IReference
{
    public string Reference => Path.GetFullPath(FilePath);
    public ReferenceType ReferenceType => ReferenceType.LocalFile;
}
