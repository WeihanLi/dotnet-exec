// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace ReferenceResolver;

public interface INuGetHelper
{
    IEnumerable<NuGetSourceInfo> GetSources(string? packageId = null);
    
    IAsyncEnumerable<(NuGetSourceInfo Source, IEnumerable<IPackageSearchMetadata> SearchResult)> SearchPackages(
        string keyword, bool includePreRelease = true, int take = 20, int skip = 0,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<(NuGetSourceInfo Source, IEnumerable<string> Packages)> GetPackages(string packagePrefix,
        bool includePreRelease = true, CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<(NuGetSourceInfo Source, NuGetVersion Version)> GetPackageVersions(string packageId,
        bool includePrerelease = false,
        Func<NuGetVersion, bool>? predict = null, CancellationToken cancellationToken = default);

    Task<NuGetVersion?> GetLatestPackageVersion(string packageId, bool includePrerelease = false,
        CancellationToken cancellationToken = default);

    Task<(NuGetSourceInfo Source, NuGetVersion Version)?> GetLatestPackageVersionWithSource(string packageId,
        bool includePrerelease = false,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, NuGetVersion>> GetPackageDependencies(string packageId, NuGetVersion packageVersion,
        string targetFramework, CancellationToken cancellationToken = default);

    Task<string?> DownloadPackage(string packageId, NuGetVersion version, string? packagesDirectory = null,
        CancellationToken cancellationToken = default);

    Task<bool> GetPackageStream(string packageId, NuGetVersion version, Stream stream,
        CancellationToken cancellationToken = default);
    
    Task<string[]> ResolvePackageReferences(string targetFramework, string packageId,
        NuGetVersion? version, bool includePrerelease, CancellationToken cancellationToken = default);

    Task<string[]> ResolvePackageReferences(NuGetReference nugetReference, string targetFramework,
        bool includePrerelease,
        CancellationToken cancellationToken = default)
        => ResolvePackageReferences(targetFramework, nugetReference.PackageId, nugetReference.PackageVersion,
            includePrerelease, cancellationToken);
    
    Task<string[]> ResolvePackageAnalyzerReferences(string targetFramework, string packageId,
        NuGetVersion? version, bool includePrerelease, CancellationToken cancellationToken = default);
}
