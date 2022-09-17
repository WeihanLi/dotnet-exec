// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace ReferenceResolver;

public sealed class NuGetReferenceResolver : IReferenceResolver
{
    private readonly INuGetHelper _nugetHelper;

    public NuGetReferenceResolver(ILoggerFactory loggerFactory)
    {
        _nugetHelper = new NuGetHelper(loggerFactory);
    }

    public ReferenceType ReferenceType => ReferenceType.NuGetPackage;

    public async Task<IEnumerable<string>> Resolve(string reference, string targetFramework)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(targetFramework);
        NuGetVersion? version = null;
        var splits = reference.Split(new[] { ',', ':' }, StringSplitOptions.RemoveEmptyEntries);
        var packageId = splits[0];
        if (splits.Length == 2)
        {
            version = NuGetVersion.Parse(splits[1]);
        }
        var references = await _nugetHelper.ResolvePackageReferences(targetFramework, packageId, version, false);
        return references;
    }

    public async Task<IEnumerable<string>> Resolve(string reference, string targetFramework, bool includePreview)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(targetFramework);
        NuGetVersion? version = null;
        var splits = reference.Split(new[] { ',', ':' }, StringSplitOptions.RemoveEmptyEntries);
        var packageId = splits[0];
        if (splits.Length == 2)
        {
            version = NuGetVersion.Parse(splits[1]);
        }
        var references = await _nugetHelper.ResolvePackageReferences(targetFramework, packageId, version, includePreview);
        return references;
    }
}
