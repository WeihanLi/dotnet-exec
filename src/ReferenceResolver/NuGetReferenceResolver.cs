// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

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

    private static readonly char[] Separator = new[] { ',', ':' };

    public async Task<IEnumerable<string>> Resolve(string reference, string targetFramework, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(targetFramework);
        NuGetVersion? version = null;
        var splits = reference.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        var packageId = splits[0];
        if (splits.Length == 2)
        {
            version = NuGetVersion.Parse(splits[1]);
        }

        var references = await _nugetHelper.ResolvePackageReferences(targetFramework, packageId, version, false, cancellationToken)
            .ConfigureAwait(false);
        return references;
    }

    public async Task<IEnumerable<string>> Resolve(string reference, string targetFramework, bool includePreview, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(targetFramework);
        NuGetVersion? version = null;
        var splits = reference.Split(Separator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var packageId = splits[0];
        if (splits.Length == 2)
        {
            version = NuGetVersion.Parse(splits[1]);
        }

        var references =
            await _nugetHelper.ResolvePackageReferences(targetFramework, packageId, version, includePreview, cancellationToken)
                .ConfigureAwait(false);
        return references;
    }

    public async Task<IEnumerable<string>> ResolveAnalyzers(string reference, string targetFramework,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(targetFramework);
        NuGetVersion? version = null;
        var splits = reference.Split(Separator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var packageId = splits[0];
        if (splits.Length == 2)
        {
            version = NuGetVersion.Parse(splits[1]);
        }

        var references =
            await _nugetHelper.ResolvePackageAnalyzerReferences(targetFramework, packageId, version, false, cancellationToken)
                .ConfigureAwait(false);
        return references;
    }
}
