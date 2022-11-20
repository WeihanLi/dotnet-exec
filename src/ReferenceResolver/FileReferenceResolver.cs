// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace ReferenceResolver;

public sealed class FileReferenceResolver : IReferenceResolver
{
    public ReferenceType ReferenceType => ReferenceType.LocalFile;

    public Task<IEnumerable<string>> Resolve(string reference, string targetFramework, CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(reference);
        return fileInfo.Exists
            ? Task.FromResult<IEnumerable<string>>(new[] { fileInfo.FullName })
            : Task.FromResult(Enumerable.Empty<string>());
    }
}

[System.Diagnostics.DebuggerDisplay("{Reference}")]
public sealed record FileReference(string FilePath) : IReference
{
    public string Reference => Path.GetFullPath(FilePath);
    public ReferenceType ReferenceType => ReferenceType.LocalFile;
}
