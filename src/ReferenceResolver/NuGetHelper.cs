// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using NuGetLogLevel = NuGet.Common.LogLevel;

namespace ReferenceResolver;

public sealed class NuGetHelper : INuGetHelper, IDisposable
{
    private const string LoggerCategoryName = "NuGetClient";
    public const string NuGetOrgSourceUrl = "https://api.nuget.org/v3/index.json";
    public const string NuGetConfigEnvName = "REFERENCE_RESOLVER_NUGET_CONFIG_PATH";

    private readonly HashSet<SourceRepository> _nugetSources = new(new NuGetSourceRepositoryComparer());
    private readonly SourceCacheContext _sourceCacheContext = new()
    {
        IgnoreFailedSources = true
    };
    private readonly PackageSourceMapping _packageSourceMapping;
    private readonly string _globalPackagesFolder;
    private readonly FrameworkReducer _frameworkReducer = new();

    private readonly LoggerBase _nugetLogger;
    private readonly ILogger _logger;

    public NuGetHelper(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(LoggerCategoryName);
        _nugetLogger = new NuGetLoggerLoggingAdapter(_logger);

        var configFilePath = Environment.GetEnvironmentVariable(NuGetConfigEnvName);
        ISettings nugetSettings;
        var root = Environment.CurrentDirectory;
        if (!string.IsNullOrEmpty(configFilePath) && File.Exists(configFilePath))
        {
            var configFileFullPath = Path.GetFullPath(configFilePath);
            nugetSettings = Settings.LoadSpecificSettings(root, configFileFullPath);
            _logger.LogInformation(
                "NuGetHelper is using the specific nuget config file {NuGetConfigPath}, current working directory: {Root}",
                configFilePath, root);
        }
        else
        {
            nugetSettings = Settings.LoadDefaultSettings(root);
            _logger.LogInformation("NuGetHelper is using the default nuget config file, current working directory: {Root}", root);
        }

        _globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(nugetSettings);
        var resourceProviders = Repository.Provider.GetCoreV3().ToArray();
        foreach (var packageSource in SettingsUtility.GetEnabledSources(nugetSettings))
        {
            _nugetSources.Add(new SourceRepository(packageSource, resourceProviders));
        }
        // try to add nuget.org to ensure NuGet org source exists
        _nugetSources.Add(Repository.Factory.GetCoreV3(NuGetOrgSourceUrl));
        _packageSourceMapping = PackageSourceMapping.GetPackageSourceMapping(nugetSettings);
    }

    public IEnumerable<NuGetSourceInfo> GetSources(string? packageId = null)
    {
        return GetSourceRepositories(packageId).Select(NuGetSourceInfo.FromSourceRepository);
    }

    public async IAsyncEnumerable<(NuGetSourceInfo Source, IEnumerable<IPackageSearchMetadata> SearchResult)> SearchPackages(
        string keyword, bool includePrerelease = true,
        int take = 20, int skip = 0, string[]? sources = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var repository in GetSourceRepositories(null, sources))
        {
            var resource = await repository.GetResourceAsync<PackageSearchResource>(cancellationToken)
                .ConfigureAwait(false);
            var result = await resource.SearchAsync(
                keyword, new SearchFilter(includePrerelease), skip, take, _nugetLogger, cancellationToken
                ).ConfigureAwait(false);
            yield return (NuGetSourceInfo.FromSourceRepository(repository), result);
        }
    }

    public async IAsyncEnumerable<(NuGetSourceInfo Source, IEnumerable<string> Packages)> GetPackages(
        string packagePrefix, bool includePrerelease = true, string[]? sources = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var repository in GetSourceRepositories(null, sources))
        {
            var resource = await repository.GetResourceAsync<AutoCompleteResource>(cancellationToken)
                .ConfigureAwait(false);
            var result = await resource.IdStartsWith(packagePrefix, includePrerelease, _nugetLogger, cancellationToken)
                .ConfigureAwait(false);
            yield return (NuGetSourceInfo.FromSourceRepository(repository), result);
        }
    }

    public async Task<string?> DownloadPackage(string packageId, NuGetVersion version, string? packagesDirectory = null, string[]? sources = null, CancellationToken cancellationToken = default)
    {
        var packageDir = GetPackageInstalledDir(packageId, version, packagesDirectory);
        if (Directory.Exists(packageDir))
        {
            return packageDir;
        }

        var packagerIdentity = new PackageIdentity(packageId, version);
        var pkgDownloadContext = new PackageDownloadContext(_sourceCacheContext);

        foreach (var sourceRepository in GetSourceRepositories(packageId, sources))
        {
            var downloadRes = await sourceRepository.GetResourceAsync<DownloadResource>(cancellationToken)
                .ConfigureAwait(false);
            using var downloadResult = await RetryHelper.TryInvokeAsync(async () =>
                await downloadRes.GetDownloadResourceResultAsync(
                    packagerIdentity,
                    pkgDownloadContext,
                    packagesDirectory ?? _globalPackagesFolder,
                    _nugetLogger,
                    cancellationToken)
                    .ConfigureAwait(false), r => r is { Status: DownloadResourceResultStatus.Available or DownloadResourceResultStatus.NotFound },
                    10, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (downloadResult?.Status != DownloadResourceResultStatus.Available)
                continue;

            _logger.LogInformation("Package({PackageIdentity}) downloaded to {PackageDirectory} from {PackageSource}",
                packagerIdentity, packageDir, downloadResult.PackageSource ?? sourceRepository.PackageSource.Name);
        }

        return Directory.Exists(packageDir) ? packageDir : null;
    }

    public async IAsyncEnumerable<(NuGetSourceInfo Source, NuGetVersion Version)> GetPackageVersions(
        string packageId, bool includePrerelease = false, Func<NuGetVersion, bool>? predict = null, string[]? sources = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var sourceRepository in GetSourceRepositories(packageId, sources))
        {
            var findPackageByIdResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken)
                .ConfigureAwait(false);
            var versions = await findPackageByIdResource.GetAllVersionsAsync(
                packageId,
                _sourceCacheContext,
                _nugetLogger, cancellationToken)
                .ConfigureAwait(false);
            foreach (var version in versions.Where(v => includePrerelease || !v.IsPrerelease))
            {
                if (predict is null || predict(version))
                    yield return (NuGetSourceInfo.FromSourceRepository(sourceRepository), version);
            }
        }
    }

    public Task<NuGetVersion?> GetLatestPackageVersion(string packageId, bool includePrerelease = false, string[]? sources = null,
        CancellationToken cancellationToken = default)
        => GetLatestPackageVersionWithSource(packageId, includePrerelease, sources, cancellationToken)
            .ContinueWith(r => r.Result?.Version, cancellationToken);

    public async Task<(NuGetSourceInfo Source, NuGetVersion Version)?> GetLatestPackageVersionWithSource(
        string packageId, bool includePrerelease = false, string[]? sources = null,
        CancellationToken cancellationToken = default)
    {
        var versions = await GetSourceRepositories(packageId, sources)
            .Select(async repo =>
            {
                var packageMetadataResource = await repo.GetResourceAsync<PackageMetadataResource>(cancellationToken)
                    .ConfigureAwait(false);
                var metaDataResult = await packageMetadataResource.GetMetadataAsync(packageId, includePrerelease,
                    false, _sourceCacheContext, _nugetLogger, cancellationToken)
                    .ConfigureAwait(false);
                var metaData = metaDataResult?.MaxBy(x => x.Identity.Version);
                var version = metaData?.Identity.Version;
                return (Source: NuGetSourceInfo.FromSourceRepository(repo), Version: version!);
            })
            .WhenAll();
        var maxVersion = versions
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            .Where(x => x.Version != null)
            .OrderByDescending(v => v.Version)
            .FirstOrDefault();
        return maxVersion;
    }

    public async Task<bool> GetPackageStream(string packageId, NuGetVersion version, Stream stream,
        string[]? sources = null, CancellationToken cancellationToken = default)
    {
        foreach (var sourceRepository in GetSourceRepositories(packageId, sources))
        {
            var findPackageByIdResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken)
                .ConfigureAwait(false);
            var result = await findPackageByIdResource.CopyNupkgToStreamAsync(
                packageId,
                version,
                stream,
                _sourceCacheContext,
                _nugetLogger, cancellationToken)
                .ConfigureAwait(false);
            if (result) return true;
        }

        return false;
    }

    public async Task<string[]> ResolvePackageReferences(string targetFramework, string packageId,
        NuGetVersion? version, bool includePrerelease, CancellationToken cancellationToken = default)
    {
        if (version is null)
        {
            version = await GetLatestPackageVersion(packageId, includePrerelease, null, cancellationToken)
                .ConfigureAwait(false);
            if (version is null)
            {
                throw new InvalidOperationException($"No package version found for package {packageId}");
            }
        }
        var dependencies = await GetPackageDependencies(packageId, version, targetFramework, cancellationToken)
            .ConfigureAwait(false);
        var packageReferences = await ResolvePackageInternal(targetFramework, packageId, version, cancellationToken)
            .ConfigureAwait(false);
        if (dependencies.Count <= 0)
        {
            return packageReferences;
        }

        var references = new ConcurrentBag<string>(packageReferences);
        await Parallel.ForEachAsync(dependencies, cancellationToken, async (dependency, ct) =>
        {
            var result = await ResolvePackageInternal(targetFramework, dependency.Key, dependency.Value, ct)
                .ConfigureAwait(false);
            foreach (var item in result)
            {
                references.Add(item);
            }
        }).ConfigureAwait(false);
        return references.Distinct().ToArray();
    }

    public async Task<string[]> ResolvePackageAnalyzerReferences(string targetFramework, string packageId,
        NuGetVersion? version, bool includePrerelease, CancellationToken cancellationToken = default)
    {
        if (version is null)
        {
            version = await GetLatestPackageVersion(packageId, includePrerelease, null, cancellationToken)
                .ConfigureAwait(false);
            if (version is null)
            {
                throw new InvalidOperationException($"No package version found for package {packageId}");
            }
        }
        var dependencies = await GetPackageDependencies(packageId, version, targetFramework, cancellationToken)
            .ConfigureAwait(false);
        var analyzerReferences = await ResolvePackageAnalyzerInternal(targetFramework, packageId, version, cancellationToken)
            .ConfigureAwait(false);
        if (dependencies.Count <= 0)
        {
            return analyzerReferences;
        }

        var references = new ConcurrentBag<string>(analyzerReferences);
        await Parallel.ForEachAsync(dependencies, cancellationToken, async (dependency, ct) =>
        {
            var result = await ResolvePackageAnalyzerInternal(targetFramework, dependency.Key, dependency.Value, ct)
                .ConfigureAwait(false);
            foreach (var item in result)
            {
                references.Add(item);
            }
        }).ConfigureAwait(false);
        return references.Distinct().ToArray();
    }

    private async Task<Dictionary<string, NuGetVersion>> GetPackageDependencies(string packageId, NuGetVersion packageVersion, string targetFramework, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packageId);
        ArgumentNullException.ThrowIfNull(packageVersion);
        ArgumentNullException.ThrowIfNull(targetFramework);
        var dependencyGroupInfo = await GetPackageDependencyGroups(packageId, packageVersion, cancellationToken)
            .ConfigureAwait(false);
        if (dependencyGroupInfo.Count <= 0)
        {
            return [];
        }

        var nugetFramework = NuGetFramework.Parse(targetFramework);
        var nearestFramework = _frameworkReducer.GetNearest(nugetFramework, dependencyGroupInfo.Select(x => x.TargetFramework));
        if (nearestFramework != null)
        {
            var bestDependency = dependencyGroupInfo.First(x => x.TargetFramework == nearestFramework);
            var list = new Dictionary<string, NuGetVersion>(StringComparer.OrdinalIgnoreCase);
            foreach (var package in bestDependency.Packages)
            {
                var packageMinVersion = GetMinVersion(package);
                if (list.TryGetValue(package.Id, out var versionValue))
                {
                    if (versionValue < packageMinVersion)
                    {
                        list[package.Id] = packageMinVersion;
                    }
                }
                else
                {
                    list.Add(package.Id, packageMinVersion);
                }

                var childrenDependencies =
                    await GetPackageDependencies(package.Id, packageMinVersion, targetFramework, cancellationToken)
                        .ConfigureAwait(false);
                if (childrenDependencies is { Count: > 0 })
                {
                    foreach (var childrenDependency in childrenDependencies)
                    {
                        if (list.TryGetValue(childrenDependency.Key, out var value))
                        {
                            if (value < childrenDependency.Value)
                            {
                                list[childrenDependency.Key] = childrenDependency.Value;
                            }
                        }
                        else
                        {
                            list.Add(childrenDependency.Key, childrenDependency.Value);
                        }
                    }
                }
            }

            return list;
        }

        throw new InvalidOperationException($"no supported target framework for package({packageId}:{packageVersion})");
    }

    private async Task<IReadOnlyList<PackageDependencyGroup>> GetPackageDependencyGroups(string packageId, NuGetVersion packageVersion, CancellationToken cancellationToken)
    {
        var packageDir = GetPackageInstalledDir(packageId, packageVersion);
        if (Directory.Exists(packageDir))
        {
            try
            {
                using var packageReader = new PackageFolderReader(packageDir);
                var dependencies = (await packageReader.GetPackageDependenciesAsync(cancellationToken)
                    .ConfigureAwait(false)).ToArray();
                return dependencies;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Exception when GetPackageDependenciesAsync from PackageFolderReader");
            }
        }

        foreach (var repository in GetSourceRepositories(packageId))
        {
            var findPkgByIdRes = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken)
                .ConfigureAwait(false);
            var dependencyInfo = await findPkgByIdRes.GetDependencyInfoAsync(
                packageId, new NuGetVersion(packageVersion), _sourceCacheContext, _nugetLogger, cancellationToken
            ).ConfigureAwait(false);
            if (dependencyInfo != null)
                return dependencyInfo.DependencyGroups;
        }

        return [];
    }

    private async Task<string[]> ResolvePackageInternal(string targetFramework, string packageId, NuGetVersion version, CancellationToken cancellationToken)
    {
        await DownloadPackage(packageId, version, null, null, cancellationToken).ConfigureAwait(false);
        var packageDir = GetPackageInstalledDir(packageId, version);
        if (!Directory.Exists(packageDir))
        {
            throw new InvalidOperationException("Package could not be downloaded");
        }

        var nugetFramework = NuGetFramework.Parse(targetFramework);
        using var packageReader = new PackageFolderReader(packageDir);

        var libItems = (await packageReader.GetLibItemsAsync(cancellationToken).ConfigureAwait(false)).ToArray();
        var nearestLib = _frameworkReducer.GetNearest(nugetFramework, libItems.Select(x => x.TargetFramework));
        if (nearestLib != null)
        {
            return libItems.First(x => x.TargetFramework == nearestLib)
                .Items
                .Where(x => ".dll".EqualsIgnoreCase(Path.GetExtension(x)))
                .Select(x => Path.Combine(packageDir, x))
                .ToArray();
        }

        var refItems = (await packageReader.GetItemsAsync(PackagingConstants.Folders.Ref, cancellationToken).ConfigureAwait(false)).ToArray();
        var nearestRef = _frameworkReducer.GetNearest(nugetFramework, refItems.Select(x => x.TargetFramework));
        if (nearestRef != null)
        {
            return refItems.First(x => x.TargetFramework == nearestRef)
                 .Items
                 .Where(x => ".dll".EqualsIgnoreCase(Path.GetExtension(x)))
                 .Select(x => Path.Combine(packageDir, x))
                 .ToArray();
        }

        var runtimeItems = (await packageReader.GetItemsAsync(PackagingConstants.Folders.Runtimes, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        return runtimeItems != null
            ? runtimeItems
                .Items
                .Where(x => ".dll".EqualsIgnoreCase(Path.GetExtension(x)))
                .Select(x => Path.Combine(packageDir, x))
                .ToArray()
            : [];
    }

    private async Task<string[]> ResolvePackageAnalyzerInternal(string targetFramework, string packageId, NuGetVersion version, CancellationToken cancellationToken)
    {
        await DownloadPackage(packageId, version, null, null, cancellationToken).ConfigureAwait(false);
        var packageDir = GetPackageInstalledDir(packageId, version);
        if (!Directory.Exists(packageDir))
        {
            throw new InvalidOperationException("Package could not be downloaded");
        }

        var nugetFramework = NuGetFramework.Parse(targetFramework);
        using var packageReader = new PackageFolderReader(packageDir);
        var analyzerItems = (await packageReader.GetItemsAsync(PackagingConstants.Folders.Analyzers, cancellationToken)
            .ConfigureAwait(false)).ToArray();
        var nearestRef = _frameworkReducer.GetNearest(nugetFramework, analyzerItems.Select(x => x.TargetFramework));
        return nearestRef != null
            ? analyzerItems.First(x => x.TargetFramework == nearestRef)
                 .Items
                 .Where(x => ".dll".EqualsIgnoreCase(Path.GetExtension(x)))
                 .Select(x => Path.Combine(packageDir, x))
                 .ToArray()
            : analyzerItems.Length > 0
              ? analyzerItems[0].Items.Select(x => Path.Combine(packageDir, x)).ToArray()
              : []
              ;
    }

    private string GetPackageInstalledDir(string packageId, NuGetVersion packageVersion, string? packagesDirectory = null)
    {
        var packageDir = Path.Combine(
            packagesDirectory ?? _globalPackagesFolder,
            packageId.ToLowerInvariant(),
            packageVersion.ToString()
            );
        return packageDir;
    }

    public IEnumerable<SourceRepository> GetSourceRepositories(string? packageId = null, string[]? sources = null)
    {
        IEnumerable<SourceRepository> filterSources;

        if (_packageSourceMapping.IsEnabled && !string.IsNullOrEmpty(packageId))
        {
            var packageSources = new HashSet<string>(_packageSourceMapping.GetConfiguredPackageSources(packageId));
            filterSources = _nugetSources.Where(x => packageSources.Contains(x.PackageSource.Source) || packageSources.Contains(x.PackageSource.Name));
        }
        else
        {
            filterSources = _nugetSources;
        }

        if (sources.HasValue())
        {
            filterSources = filterSources.Where(x =>
                sources.Any(s => x.PackageSource.Name == s || x.PackageSource.Source == s));
        }

        return filterSources;
    }

    private static NuGetVersion GetMinVersion(PackageDependency packageDependency)
    {
        // need to be optimized since the dependency may do not have a specified min version
        // and the version may be a floating one or exclude 
        return (packageDependency.VersionRange.MinVersion ?? packageDependency.VersionRange.MaxVersion)!;
    }

    public void Dispose() => _sourceCacheContext.Dispose();
}

[ExcludeFromCodeCoverage]
public sealed class NuGetSourceInfo(string name, string source)
{
    public string Name => name;
    public string Source => source;

    public static NuGetSourceInfo FromSourceRepository(SourceRepository sourceRepository)
        => new(sourceRepository.PackageSource.Name, sourceRepository.PackageSource.Source);
}

[ExcludeFromCodeCoverage]
public sealed class NuGetLoggerLoggingAdapter(ILogger logger) : LoggerBase
{
    public override void Log(ILogMessage message)
    {
        var logLevel = message.Level switch
        {
            NuGetLogLevel.Debug => LogLevel.Debug,
            NuGetLogLevel.Information => LogLevel.Information,
            NuGetLogLevel.Warning => LogLevel.Warning,
            NuGetLogLevel.Error => LogLevel.Error,
            NuGetLogLevel.Verbose => LogLevel.Trace,
            NuGetLogLevel.Minimal => LogLevel.Information,
            _ => LogLevel.None
        };
        logger.Log(logLevel, message.FormatWithCode());
    }
    public override Task LogAsync(ILogMessage message)
    {
        Log(message);
        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage]
file sealed class NuGetSourceRepositoryComparer : IEqualityComparer<SourceRepository>
{
    public bool Equals(SourceRepository? x, SourceRepository? y)
    {
        return x == null ? y is null : y != null && x.PackageSource.Source.Equals(y.PackageSource.Source, StringComparison.Ordinal);
    }

    public int GetHashCode(SourceRepository obj)
    {
        return obj.PackageSource.Source.GetHashCode();
    }
}
