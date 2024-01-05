// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
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
using System.Runtime.CompilerServices;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using NuGetLogLevel = NuGet.Common.LogLevel;

namespace ReferenceResolver;

public sealed class NuGetHelper : INuGetHelper, IDisposable
{
    private const string LoggerCategoryName = "NuGetClient";
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
        _nugetLogger = new NuGetLoggingAdapter(_logger);

        var configProfilePath = Environment.GetEnvironmentVariable(NuGetConfigEnvName);
        ISettings nugetSettings;
        var root = Environment.CurrentDirectory;
        if (!string.IsNullOrEmpty(configProfilePath) && File.Exists(configProfilePath))
        {
            nugetSettings = Settings.LoadSpecificSettings(root, Path.GetFullPath(configProfilePath));
            _logger.LogInformation(
                "NuGetHelper is using the specific nuget config file {NuGetConfigPath}, current working directory: {Root}",
                configProfilePath, root);
        }
        else
        {
            nugetSettings = Settings.LoadDefaultSettings(root);
        }

        _globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(nugetSettings);
        var resourceProviders = Repository.Provider.GetCoreV3().ToArray();
        foreach (var packageSource in SettingsUtility.GetEnabledSources(nugetSettings))
        {
            _nugetSources.Add(new SourceRepository(packageSource, resourceProviders));
        }
        // try add nuget.org
        _nugetSources.Add(Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json"));
        _packageSourceMapping = PackageSourceMapping.GetPackageSourceMapping(nugetSettings);
    }

    public async IAsyncEnumerable<string> GetPackages(string packagePrefix, bool includePreRelease = true,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var repository in GetPackageSourceRepositories())
        {
            var resource = await repository.GetResourceAsync<AutoCompleteResource>(cancellationToken).ConfigureAwait(false);
            var result = await resource.IdStartsWith(packagePrefix, includePreRelease, _nugetLogger, cancellationToken)
                .ConfigureAwait(false);
            foreach (var item in result)
            {
                yield return item;
            }
        }
    }

    public async Task<string?> DownloadPackage(string packageId, NuGetVersion version, string? packagesDirectory = null, CancellationToken cancellationToken = default)
    {
        var packageDir = GetPackageInstalledDir(packageId, version, packagesDirectory);
        if (Directory.Exists(packageDir))
        {
            return packageDir;
        }

        var packagerIdentity = new PackageIdentity(packageId, version);
        var pkgDownloadContext = new PackageDownloadContext(_sourceCacheContext);

        foreach (var sourceRepository in GetPackageSourceRepositories(packageId))
        {
            var downloadRes = await sourceRepository.GetResourceAsync<DownloadResource>(cancellationToken).ConfigureAwait(false);
            using var downloadResult = await RetryHelper.TryInvokeAsync(async () =>
                await downloadRes.GetDownloadResourceResultAsync(
                    packagerIdentity,
                    pkgDownloadContext,
                    packagesDirectory ?? _globalPackagesFolder,
                    _nugetLogger,
                    cancellationToken).ConfigureAwait(false), _ => true, 5).ConfigureAwait(false);
            if (downloadResult?.Status != DownloadResourceResultStatus.Available)
                continue;

            _logger.LogInformation("Package({PackageIdentity}) downloaded to {PackageDirectory} from {PackageSource}",
                packagerIdentity, packageDir, downloadResult.PackageSource ?? sourceRepository.PackageSource.Name);
        }

        return Directory.Exists(packageDir) ? packageDir : null;
    }

    public async IAsyncEnumerable<(NuGetSourceInfo Source, NuGetVersion Version)> GetPackageVersions(string packageId, bool includePrerelease = false, Func<NuGetVersion, bool>? predict = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var sourceRepository in GetPackageSourceRepositories(packageId))
        {
            var sourceInfo = NuGetSourceInfo.FromSourceRepository(sourceRepository);
            var findPackageByIdResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken)
                .ConfigureAwait(false);
            var versions = await findPackageByIdResource.GetAllVersionsAsync(
                packageId,
                _sourceCacheContext,
                _nugetLogger, cancellationToken).ConfigureAwait(false);
            foreach (var version in versions.Where(v => includePrerelease || !v.IsPrerelease))
            {
                if (predict is null || predict(version))
                    yield return (sourceInfo, version);
            }
        }
    }

    public Task<NuGetVersion?> GetLatestPackageVersion(string packageId, bool includePrerelease = false,
        CancellationToken cancellationToken = default)
        => GetLatestPackageVersionWithSource(packageId, includePrerelease, cancellationToken)
            .ContinueWith(r => r.Result?.Version, cancellationToken);

    public async Task<(NuGetSourceInfo Source, NuGetVersion Version)?> GetLatestPackageVersionWithSource(string packageId, bool includePrerelease = false,
        CancellationToken cancellationToken = default)
    {
        var versions = await GetPackageSourceRepositories(packageId)
            .Select(async repo =>
            {
                var packageMetadataResource = await repo.GetResourceAsync<PackageMetadataResource>(cancellationToken)
                    .ConfigureAwait(false);
                var metaDataResult = await packageMetadataResource.GetMetadataAsync(packageId, includePrerelease,
                    false, _sourceCacheContext, _nugetLogger, cancellationToken).ConfigureAwait(false);
                var metaData = metaDataResult?.MaxBy(x => x.Identity.Version);
                var version = metaData?.Identity.Version;
                return (Source: NuGetSourceInfo.FromSourceRepository(repo), Version: version!);
            })
            .WhenAll();
        // ReSharper disable once SimplifyLinqExpressionUseMinByAndMaxBy
        var maxVersion = versions
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            .Where(x => x.Version != null)
            .OrderByDescending(v => v.Version)
            .FirstOrDefault();
        return maxVersion;
    }

    public async Task<bool> GetPackageStream(string packageId, NuGetVersion version, Stream stream, CancellationToken cancellationToken = default)
    {
        foreach (var sourceRepository in GetPackageSourceRepositories(packageId))
        {
            var findPackageByIdResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken)
                .ConfigureAwait(false);
            var result = await findPackageByIdResource.CopyNupkgToStreamAsync(
                packageId,
                version,
                stream,
                _sourceCacheContext,
                _nugetLogger, cancellationToken).ConfigureAwait(false);
            if (result) return true;
        }

        return false;
    }

    public async Task<Dictionary<string, NuGetVersion>> GetPackageDependencies(string packageId, NuGetVersion packageVersion, string targetFramework, CancellationToken cancellationToken = default)
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

    public async Task<string[]> ResolvePackageReferences(string targetFramework, string packageId,
        NuGetVersion? version, bool includePrerelease, CancellationToken cancellationToken = default)
    {
        if (version is null)
        {
            version = await GetLatestPackageVersion(packageId, includePrerelease, cancellationToken)
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
            version = await GetLatestPackageVersion(packageId, includePrerelease, cancellationToken);
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

    private async Task<IReadOnlyList<PackageDependencyGroup>> GetPackageDependencyGroups(string packageId, NuGetVersion packageVersion, CancellationToken cancellationToken)
    {
        var packageDir = GetPackageInstalledDir(packageId, packageVersion);
        if (Directory.Exists(packageDir))
        {
            using var packageReader = new PackageFolderReader(packageDir);
            var dependencies = (await packageReader.GetPackageDependenciesAsync(cancellationToken)
                    .ConfigureAwait(false)).ToArray();
            return dependencies;
        }

        foreach (var repository in GetPackageSourceRepositories(packageId))
        {
            var findPkgByIdRes = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken)
                .ConfigureAwait(false);
            var dependencyInfo = await findPkgByIdRes.GetDependencyInfoAsync(packageId,
                new NuGetVersion(packageVersion), _sourceCacheContext, _nugetLogger, cancellationToken).ConfigureAwait(false);
            if (dependencyInfo != null)
                return dependencyInfo.DependencyGroups;
        }

        return [];
    }

    private async Task<string[]> ResolvePackageInternal(string targetFramework, string packageId, NuGetVersion version, CancellationToken cancellationToken)
    {
        await DownloadPackage(packageId, version, null, cancellationToken).ConfigureAwait(false);
        var packageDir = GetPackageInstalledDir(packageId, version);
        if (!Directory.Exists(packageDir))
        {
            throw new InvalidOperationException("Package could not be downloaded");
        }
        //
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
        if (runtimeItems != null)
        {
            return runtimeItems
                .Items
                .Where(x => ".dll".EqualsIgnoreCase(Path.GetExtension(x)))
                .Select(x => Path.Combine(packageDir, x))
                .ToArray();
        }
        return [];
    }

    private async Task<string[]> ResolvePackageAnalyzerInternal(string targetFramework, string packageId, NuGetVersion version, CancellationToken cancellationToken)
    {
        await DownloadPackage(packageId, version, null, cancellationToken).ConfigureAwait(false);
        var packageDir = GetPackageInstalledDir(packageId, version);
        if (!Directory.Exists(packageDir))
        {
            throw new InvalidOperationException("Package could not be downloaded");
        }
        //
        var nugetFramework = NuGetFramework.Parse(targetFramework);
        using var packageReader = new PackageFolderReader(packageDir);
        var analyzerItems = (await packageReader.GetItemsAsync(PackagingConstants.Folders.Analyzers, cancellationToken)
            .ConfigureAwait(false)).ToArray();
        var nearestRef = _frameworkReducer.GetNearest(nugetFramework, analyzerItems.Select(x => x.TargetFramework));
        if (nearestRef != null)
        {
            return analyzerItems.First(x => x.TargetFramework == nearestRef)
                 .Items
                 .Where(x => ".dll".EqualsIgnoreCase(Path.GetExtension(x)))
                 .Select(x => Path.Combine(packageDir, x))
                 .ToArray();
        }

        return analyzerItems.Length > 0
            ? analyzerItems[0].Items.Select(x => Path.Combine(packageDir, x)).ToArray()
            : [];
    }

    private string GetPackageInstalledDir(string packageId, NuGetVersion packageVersion, string? packagesDirectory = null)
    {
        var packageDir = Path.Combine(packagesDirectory ?? _globalPackagesFolder, packageId.ToLowerInvariant(),
            packageVersion.ToString());
        return packageDir;
    }

    private IEnumerable<SourceRepository> GetPackageSourceRepositories(string? packageId = null)
    {
        if (!_packageSourceMapping.IsEnabled || string.IsNullOrEmpty(packageId))
            return _nugetSources;

        var packageSources = new HashSet<string>(_packageSourceMapping.GetConfiguredPackageSources(packageId));
        return _nugetSources.Where(x => packageSources.Contains(x.PackageSource.Source) || packageSources.Contains(x.PackageSource.Name));
    }

    private static NuGetVersion GetMinVersion(PackageDependency packageDependency)
    {
        // need to be optimized since the dependency may do not has a specified min version
        // and the version may be a floating one or exclude 
        return (packageDependency.VersionRange.MinVersion ?? packageDependency.VersionRange.MaxVersion)!;
    }

    public void Dispose() => _sourceCacheContext.Dispose();
}

public sealed class NuGetSourceInfo(string name, string source)
{
    public string Name => name;
    public string Source => source;

    public static NuGetSourceInfo FromSourceRepository(SourceRepository sourceRepository)
        => new(sourceRepository.PackageSource.Name, sourceRepository.PackageSource.Source);
}

file sealed class NuGetLoggingAdapter(ILogger logger) : LoggerBase
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
            NuGetLogLevel.Minimal => LogLevel.Warning,
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

file sealed class NuGetSourceRepositoryComparer : IEqualityComparer<SourceRepository>
{
    public bool Equals(SourceRepository? x, SourceRepository? y)
    {
        if (x == null)
            return y is null;

        return y != null && x.PackageSource.Source.Equals(y.PackageSource.Source, StringComparison.Ordinal);
    }

    public int GetHashCode(SourceRepository obj)
    {
        return obj.PackageSource.Source.GetHashCode();
    }
}
