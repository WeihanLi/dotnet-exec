// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.Diagnostics;
using NuGet.Versioning;

namespace ReferenceResolver;

public sealed class NuGetReferenceResolver : IReferenceResolver
{
    private readonly INuGetHelper _nugetHelper;

    public NuGetReferenceResolver(INuGetHelper nugetHelper)
    {
        _nugetHelper = nugetHelper;
    }

    public ReferenceType ReferenceType => ReferenceType.NuGetPackage;

    public async Task<IEnumerable<string>> Resolve(string reference, string targetFramework, CancellationToken cancellationToken = default)
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

        var references = await _nugetHelper.ResolvePackageReferences(targetFramework, packageId, version, false, cancellationToken);
        return references;
    }

    public async Task<IEnumerable<string>> Resolve(string reference, string targetFramework, bool includePreview, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(targetFramework);
        NuGetVersion? version = null;
        var splits = reference.Split(new[] { ',', ':' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var packageId = splits[0];
        if (splits.Length == 2)
        {
            version = NuGetVersion.Parse(splits[1]);
        }

        var references =
            await _nugetHelper.ResolvePackageReferences(targetFramework, packageId, version, includePreview, cancellationToken);
        return references;
    }

    public async Task<IEnumerable<string>> ResolveAnalyzers(string reference, string targetFramework,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(targetFramework);
        NuGetVersion? version = null;
        var splits = reference.Split(new[] { ',', ':' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var packageId = splits[0];
        if (splits.Length == 2)
        {
            version = NuGetVersion.Parse(splits[1]);
        }
        
        var references =
            await _nugetHelper.ResolvePackageAnalyzerReferences(targetFramework, packageId, version, false, cancellationToken);
        return references;
    }
}
