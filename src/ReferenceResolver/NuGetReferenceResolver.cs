// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

public sealed class NuGetReferenceResolver : IReferenceResolver
{
    private readonly INuGetHelper _nugetHelper;

    public NuGetReferenceResolver(INuGetHelper nugetHelper)
    {
        _nugetHelper = nugetHelper;
    }

    public ReferenceType ReferenceType => ReferenceType.NuGetPackage;

    public async Task<IEnumerable<string>> Resolve(string reference, string targetFramework,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(targetFramework);

        var nugetReference = NuGetReference.Parse(reference);
        var references = await _nugetHelper.ResolvePackageReferences(targetFramework, nugetReference.PackageId,
                nugetReference.PackageVersion, false, cancellationToken)
            .ConfigureAwait(false);
        return references;
    }

    public async Task<IEnumerable<string>> Resolve(string reference, string targetFramework, bool includePreview,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(targetFramework);

        var nugetReference = NuGetReference.Parse(reference);
        var references =
            await _nugetHelper.ResolvePackageReferences(targetFramework, nugetReference.PackageId,
                    nugetReference.PackageVersion, includePreview, cancellationToken)
                .ConfigureAwait(false);
        return references;
    }

    public async Task<IEnumerable<string>> ResolveAnalyzers(string reference, string targetFramework,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(targetFramework);

        var nugetReference = NuGetReference.Parse(reference);
        var references =
            await _nugetHelper.ResolvePackageAnalyzerReferences(targetFramework, nugetReference.PackageId,
                    nugetReference.PackageVersion, false, cancellationToken)
                .ConfigureAwait(false);
        return references;
    }
}
