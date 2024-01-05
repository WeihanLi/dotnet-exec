// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

public sealed class FolderReferenceResolver : IReferenceResolver
{
    public ReferenceType ReferenceType => ReferenceType.LocalFolder;

    public Task<IEnumerable<string>> Resolve(string reference, string targetFramework, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);
        if (!Directory.Exists(reference))
        {
            throw new ArgumentException("Folder not exits");
        }

        var dllPath = Directory.GetFiles(Path.GetFullPath(reference), "*.dll");
        return Task.FromResult<IEnumerable<string>>(dllPath);
    }
}
