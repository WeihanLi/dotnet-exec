// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Xml;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using NuGetLogLevel = NuGet.Common.LogLevel;

namespace ReferenceResolver;

public interface INuGetHelper
{
    Task<string[]> ResolvePackageReferences(string targetFramework, string packageId,
        NuGetVersion? version, bool includePreview, CancellationToken cancellationToken = default);

    Task<string[]> ResolvePackageReferences(NuGetReference nugetReference, string targetFramework, bool includePreview,
        CancellationToken cancellationToken = default)
        => ResolvePackageReferences(targetFramework, nugetReference.PackageId, nugetReference.PackageVersion,
            includePreview, cancellationToken);

    Task<IEnumerable<NuGetVersion>> GetPackageVersions(string packageId, bool includePreview = false, CancellationToken cancellationToken = default);
    Task<NuGetVersion?> GetLatestPackageVersion(string packageId, bool includePreview = false,
        CancellationToken cancellationToken = default)
        => GetPackageVersions(packageId, includePreview, cancellationToken).ContinueWith(r => r.Result.OrderByDescending(_ => _).FirstOrDefault(), TaskContinuationOptions.OnlyOnRanToCompletion);
    Task<Dictionary<string, NuGetVersion>> GetPackageDependencies(string packageId, NuGetVersion packageVersion, string targetFramework, CancellationToken cancellationToken = default);
    Task<string?> DownloadPackage(string packageId, NuGetVersion version, string? packagesDirectory = null, CancellationToken cancellationToken = default);
    Task<bool> GetPackageStream(string packageId, NuGetVersion version, Stream stream, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetPackages(string packagePrefix, bool includePreRelease = true, CancellationToken cancellationToken = default);
}

public sealed class NuGetHelper : INuGetHelper
{
    private const string LoggerCategoryName = "NuGet";

    private readonly SourceCacheContext _cache = new();
    private readonly SourceRepository _repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
    private readonly FrameworkReducer _frameworkReducer = new();

    private readonly NugetLoggingAdapter _nugetLogger;
    private readonly ILogger _logger;

    private readonly string _globalPackagesFolder;
    private static string GetGlobalPackagesFolder()
    {
        var dotnetPath = FrameworkReferenceResolver.GetDotnetPath();
        var result = CommandExecutor.ExecuteAndCapture(dotnetPath, "nuget locals global-packages -l");
        var folder = string.Empty;
        if (result.StandardOut.StartsWith("global-packages:"))
        {
            folder = result.StandardOut["global-packages:".Length..].Trim();
        }
        if (folder.IsNullOrEmpty())
        {
            var packagesFolder = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

            if (string.IsNullOrEmpty(packagesFolder))
            {
                // Nuget globalPackagesFolder resolve
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var defaultConfigFilePath =
                        $@"{Environment.GetEnvironmentVariable("APPDATA")}\NuGet\NuGet.Config";
                    if (File.Exists(defaultConfigFilePath))
                    {
                        var doc = new XmlDocument();
                        doc.Load(defaultConfigFilePath);
                        var node = doc.SelectSingleNode("/configuration/config/add[@key='globalPackagesFolder']");
                        if (node != null)
                        {
                            packagesFolder = node.Attributes?["value"]?.Value;
                        }
                    }

                    if (string.IsNullOrEmpty(packagesFolder))
                    {
                        packagesFolder = $@"{Environment.GetEnvironmentVariable("USERPROFILE")}\.nuget\packages";
                    }
                }
                else
                {
                    var defaultConfigFilePath =
                        $@"{Environment.GetEnvironmentVariable("HOME")}/.config/NuGet/NuGet.Config";
                    if (File.Exists(defaultConfigFilePath))
                    {
                        var doc = new XmlDocument();
                        doc.Load(defaultConfigFilePath);
                        var node = doc.SelectSingleNode("/configuration/config/add[@key='globalPackagesFolder']");
                        if (node != null)
                        {
                            packagesFolder = node.Attributes?["value"]?.Value;
                        }
                    }

                    if (string.IsNullOrEmpty(packagesFolder))
                    {
                        defaultConfigFilePath = $@"{Environment.GetEnvironmentVariable("HOME")}/.nuget/NuGet/NuGet.Config";
                        if (File.Exists(defaultConfigFilePath))
                        {
                            var doc = new XmlDocument();
                            doc.Load(defaultConfigFilePath);
                            var node = doc.SelectSingleNode("/configuration/config/add[@key='globalPackagesFolder']");
                            if (node != null)
                            {
                                packagesFolder = node.Value;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(packagesFolder))
                    {
                        packagesFolder = $@"{Environment.GetEnvironmentVariable("HOME")}/.nuget/packages";
                    }
                }
            }

            folder = packagesFolder;
        }
        return folder;
    }

    public NuGetHelper(ILoggerFactory loggerFactory)
    {
        _nugetLogger = new NugetLoggingAdapter(loggerFactory);
        _logger = loggerFactory.CreateLogger(LoggerCategoryName);

        _globalPackagesFolder = GetGlobalPackagesFolder();
    }

    public async Task<IEnumerable<string>> GetPackages(string packagePrefix, bool includePreRelease = true, CancellationToken cancellationToken = default)
    {
        var resource = await _repository.GetResourceAsync<AutoCompleteResource>(cancellationToken);
        var result = await resource.IdStartsWith(packagePrefix, includePreRelease, _nugetLogger, cancellationToken);
        return result;
    }

    public async Task<string?> DownloadPackage(string packageId, NuGetVersion version, string? packagesDirectory = null, CancellationToken cancellationToken = default)
    {
        var packageDir = GetPackageInstalledDir(packageId, version, packagesDirectory);
        if (Directory.Exists(packageDir))
        {
            return packageDir;
        }
        var packagerIdentity = new PackageIdentity(packageId, version);
        var pkgDownloadContext = new PackageDownloadContext(_cache);
        var downloadRes = await _repository.GetResourceAsync<DownloadResource>(cancellationToken);
        using var downloadResult = await RetryHelper.TryInvokeAsync(async () =>
            await downloadRes.GetDownloadResourceResultAsync(
                packagerIdentity,
                pkgDownloadContext,
                packagesDirectory ?? _globalPackagesFolder,
                _nugetLogger,
                cancellationToken), _ => true, 5);
        _logger.LogInformation("Package({packageIdentity}) downloaded to {packageDirectory} from {packageSource}", packagerIdentity, packageDir, downloadResult!.PackageSource ?? "NuGet.org");
        return Directory.Exists(packageDir) ? packageDir : null;
    }

    public async Task<IEnumerable<NuGetVersion>> GetPackageVersions(string packageId, bool includePreview = false, CancellationToken cancellationToken = default)
    {
        var findPackageByIdResource = await _repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
        var versions = await findPackageByIdResource.GetAllVersionsAsync(
            packageId,
            _cache,
            _nugetLogger, cancellationToken);
        return versions.Where(_ => includePreview || !_.IsPrerelease);
    }

    public async Task<bool> GetPackageStream(string packageId, NuGetVersion version, Stream stream, CancellationToken cancellationToken = default)
    {
        var findPackageByIdResource = await _repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
        return await findPackageByIdResource.CopyNupkgToStreamAsync(
            packageId,
            version,
            stream,
            _cache,
            _nugetLogger, cancellationToken);
    }

    public async Task<Dictionary<string, NuGetVersion>> GetPackageDependencies(string packageName, NuGetVersion packageVersion, string targetFramework, CancellationToken cancellationToken = default)
    {
        var dependencyGroupInfo = await GetPackageDependencyGroups(packageName, packageVersion, cancellationToken);
        if (dependencyGroupInfo.Count <= 0)
        {
            return new Dictionary<string, NuGetVersion>();
        }

        var nugetFramework = NuGetFramework.Parse(targetFramework);
        var nearestFramework = _frameworkReducer.GetNearest(nugetFramework, dependencyGroupInfo.Select(x => x.TargetFramework));
        if (nearestFramework != null)
        {
            var bestDependency = dependencyGroupInfo.First(x => x.TargetFramework == nearestFramework);
            var list = new Dictionary<string, NuGetVersion>(StringComparer.OrdinalIgnoreCase);
            foreach (var package in bestDependency.Packages)
            {
                if (list.ContainsKey(package.Id))
                {
                    if (list[package.Id] < package.VersionRange.MinVersion)
                    {
                        list[package.Id] = package.VersionRange.MinVersion;
                    }
                }
                else
                {
                    list.Add(package.Id, package.VersionRange.MinVersion);
                }

                var childrenDependencies =
                    await GetPackageDependencies(package.Id, package.VersionRange.MinVersion, targetFramework, cancellationToken);
                if (childrenDependencies is { Count: > 0 })
                {
                    foreach (var childrenDependency in childrenDependencies)
                    {
                        if (list.ContainsKey(childrenDependency.Key))
                        {
                            if (list[childrenDependency.Key] < childrenDependency.Value)
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

        throw new InvalidOperationException($"no supported target framework for package({packageName}:{packageVersion})");
    }

    public async Task<string[]> ResolvePackageReferences(string targetFramework, string packageId,
        NuGetVersion? version, bool includePreview, CancellationToken cancellationToken = default)
    {
        if (version is null)
        {
            var versions = await GetPackageVersions(packageId, includePreview, cancellationToken);
            // ReSharper disable once SimplifyLinqExpressionUseMinByAndMaxBy
            version = versions.OrderByDescending(_ => _).FirstOrDefault();
            if (version is null)
            {
                throw new InvalidOperationException($"No package versions found for package {packageId}");
            }
        }
        var dependencies = await GetPackageDependencies(packageId, version, targetFramework, cancellationToken);
        var packageReferences = await ResolvePackageInternal(targetFramework, packageId, version, cancellationToken);
        if (dependencies.Count <= 0)
        {
            return packageReferences;
        }

        var references = new ConcurrentBag<string>(packageReferences);
        await Parallel.ForEachAsync(dependencies, cancellationToken, async (dependency, ct) =>
        {
            var result = await ResolvePackageInternal(targetFramework, dependency.Key, dependency.Value, ct);
            foreach (var item in result)
            {
                references.Add(item);
            }
        });
        return references.Distinct().ToArray();
    }

    private async Task<IReadOnlyList<PackageDependencyGroup>> GetPackageDependencyGroups(string packageName, NuGetVersion packageVersion, CancellationToken cancellationToken)
    {
        var packageDir = GetPackageInstalledDir(packageName, packageVersion);
        if (Directory.Exists(packageDir))
        {
            using var packageReader = new PackageFolderReader(packageDir);
            var dependencies = (await packageReader.GetPackageDependenciesAsync(cancellationToken)).ToArray();
            return dependencies;
        }

        var findPkgByIdRes = await _repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
        var dependencyInfo = await findPkgByIdRes.GetDependencyInfoAsync(packageName, new NuGetVersion(packageVersion), _cache, _nugetLogger, cancellationToken);
        return dependencyInfo.DependencyGroups;
    }

    private async Task<string[]> ResolvePackageInternal(string targetFramework, string packageId, NuGetVersion version, CancellationToken cancellationToken)
    {
        await DownloadPackage(packageId, version, null, cancellationToken);
        var packageDir = GetPackageInstalledDir(packageId, version);
        if (!Directory.Exists(packageDir))
        {
            throw new InvalidOperationException("Package could not be downloaded");
        }
        //
        var nugetFramework = NuGetFramework.Parse(targetFramework);
        using var packageReader = new PackageFolderReader(packageDir);

        var libItems = (await packageReader.GetLibItemsAsync(cancellationToken)).ToArray();
        var nearestLib = _frameworkReducer.GetNearest(nugetFramework, libItems.Select(x => x.TargetFramework));
        if (nearestLib != null)
        {
            return libItems.First(x => x.TargetFramework == nearestLib)
                .Items
                .Where(x => ".dll".EqualsIgnoreCase(Path.GetExtension(x)))
                .Select(x => Path.Combine(packageDir, x))
                .ToArray();
        }
        var refItems = (await packageReader.GetItemsAsync(PackagingConstants.Folders.Ref, cancellationToken)).ToArray();
        var nearestRef = _frameworkReducer.GetNearest(nugetFramework, refItems.Select(x => x.TargetFramework));
        if (nearestRef != null)
        {
            return refItems.First(x => x.TargetFramework == nearestRef)
                 .Items
                 .Where(x => ".dll".EqualsIgnoreCase(Path.GetExtension(x)))
                 .Select(x => Path.Combine(packageDir, x))
                 .ToArray();
        }
        return Array.Empty<string>();
    }

    private string GetPackageInstalledDir(string packageId, NuGetVersion packageVersion, string? packagesDirectory = null)
    {
        var packageDir = Path.Combine(packagesDirectory ?? _globalPackagesFolder, packageId.ToLowerInvariant(),
            packageVersion.ToString());
        return packageDir;
    }

    private sealed class NugetLoggingAdapter : LoggerBase
    {
        private readonly ILogger _logger;
        public NugetLoggingAdapter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("NuGetClient");
        }
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
            _logger.Log(logLevel, message.FormatWithCode());
        }
        public override Task LogAsync(ILogMessage message)
        {
            Log(message);
            return Task.CompletedTask;
        }
    }
}


