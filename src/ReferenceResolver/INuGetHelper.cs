// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using NuGet.Versioning;

namespace ReferenceResolver;

public interface INuGetHelper
{
    Task<string[]> ResolvePackageReferences(string targetFramework, string packageId,
        NuGetVersion? version, bool includePrerelease, CancellationToken cancellationToken = default);

    Task<string[]> ResolvePackageReferences(NuGetReference nugetReference, string targetFramework, bool includePrerelease,
        CancellationToken cancellationToken = default)
        => ResolvePackageReferences(targetFramework, nugetReference.PackageId, nugetReference.PackageVersion,
            includePrerelease, cancellationToken);

    IAsyncEnumerable<(NuGetSourceInfo Source, NuGetVersion Version)> GetPackageVersions(string packageId, bool includePrerelease = false,
        Func<NuGetVersion, bool>? predict = null, CancellationToken cancellationToken = default);

    Task<NuGetVersion?> GetLatestPackageVersion(string packageId, bool includePrerelease = false,
        CancellationToken cancellationToken = default);
    Task<(NuGetSourceInfo Source, NuGetVersion Version)?> GetLatestPackageVersionWithSource(string packageId, bool includePrerelease = false,
        CancellationToken cancellationToken = default);
    Task<Dictionary<string, NuGetVersion>> GetPackageDependencies(string packageId, NuGetVersion packageVersion, string targetFramework, CancellationToken cancellationToken = default);
    Task<string?> DownloadPackage(string packageId, NuGetVersion version, string? packagesDirectory = null, CancellationToken cancellationToken = default);
    Task<bool> GetPackageStream(string packageId, NuGetVersion version, Stream stream, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> GetPackages(string packagePrefix, bool includePreRelease = true, CancellationToken cancellationToken = default);

    Task<string[]> ResolvePackageAnalyzerReferences(string targetFramework, string packageId,
        NuGetVersion? version, bool includePrerelease, CancellationToken cancellationToken = default);
}
