// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

public sealed class FileReferenceResolver : IReferenceResolver
{
    public ReferenceType ReferenceType => ReferenceType.LocalFile;

    public Task<IEnumerable<string>> Resolve(string reference, string targetFramework, CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(reference);
        return fileInfo.Exists
            ? Task.FromResult<IEnumerable<string>>([fileInfo.FullName])
            : Task.FromResult(Enumerable.Empty<string>());
    }
}
