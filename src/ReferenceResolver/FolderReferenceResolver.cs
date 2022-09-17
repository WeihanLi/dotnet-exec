// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace ReferenceResolver;

public sealed class FolderReferenceResolver : IReferenceResolver
{
    public ReferenceType ReferenceType => ReferenceType.LocalFolder;

    public Task<IEnumerable<string>> Resolve(string reference, string targetFramework)
    {
        ArgumentNullException.ThrowIfNull(reference);
        if (!Directory.Exists(reference))
        {
            throw new ArgumentException("Folder not exits");
        }

        var dllPath = Directory.GetFiles(reference, "*.dll");
        return Task.FromResult<IEnumerable<string>>(dllPath);
    }
}
