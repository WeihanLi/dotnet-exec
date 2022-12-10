// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace ReferenceResolver;

[System.Diagnostics.DebuggerDisplay("folder: {Reference}")]
public sealed record FolderReference(string FolderPath) : IReference
{
    public string Reference => Path.GetFullPath(FolderPath);
    public ReferenceType ReferenceType => ReferenceType.LocalFolder;
}
