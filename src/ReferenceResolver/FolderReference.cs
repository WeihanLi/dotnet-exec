// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

[System.Diagnostics.DebuggerDisplay("folder: {Reference}")]
public sealed record FolderReference(string FolderPath) : IReference
{
    public string Reference => Path.GetFullPath(FolderPath);
    public ReferenceType ReferenceType => ReferenceType.LocalFolder;
}
