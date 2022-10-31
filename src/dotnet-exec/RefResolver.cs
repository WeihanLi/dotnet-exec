// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Frameworks;
using NuGet.Versioning;
using ReferenceResolver;
using System.Collections.Concurrent;
using System.Reflection;

namespace Exec;

public interface IRefResolver
{
    bool DisableCache { get; set; }

    Task<string[]> ResolveReferences(ExecOptions options, bool compilation);

    Task<IEnumerable<MetadataReference>> ResolveMetadataReferences(ExecOptions options, bool compilation);
}

public sealed class RefResolver : IRefResolver
{
    // for unit test only
    internal static IRefResolver InstanceForTest { get; } =
        new RefResolver(new NuGetHelper(NullLoggerFactory.Instance));
    // for unit test only
    public bool DisableCache { get; set; }

    private readonly INuGetHelper _nugetHelper;
    private readonly FrameworkReferenceResolver _frameworkReferenceResolver = new();
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public RefResolver(INuGetHelper nugetHelper)
    {
        _nugetHelper = nugetHelper;
    }

    private async Task<string[]> ResolveFrameworkReferences(ExecOptions options, bool compilation)
    {
        var frameworks = Helper.GetDependencyFrameworks(options);
        var frameworkReferences = await frameworks.Select(async framework =>
        {
            if (compilation)
            {
                var references =
                    await _frameworkReferenceResolver.ResolveForCompile(framework, options.TargetFramework, options.CancellationToken)
                        .ContinueWith(r=> r.Result.ToArray());
                if (references.HasValue()) return references;
                
                var packageId = Helper.GetReferencePackageName(framework);
                var versions = await _nugetHelper.GetPackageVersions(packageId, true, options.CancellationToken);
                var nugetFramework = NuGetFramework.Parse(options.TargetFramework);
                var version = versions
                    .Where(x => x.Major == nugetFramework.Version.Major
                                && x.Minor == nugetFramework.Version.Minor)
                    .Max();
                return await _nugetHelper.ResolvePackageReferences(options.TargetFramework, packageId, version,
                    true,
                    options.CancellationToken);
            }

            return await _frameworkReferenceResolver.Resolve(framework, options.TargetFramework, options.CancellationToken);
        }).WhenAll();
        if (options.IncludeWideReferences)
        {
            return frameworkReferences.Append(new[]
                {
                    typeof(DependencyResolver).Assembly.Location,
                    typeof(Newtonsoft.Json.JsonConvert).Assembly.Location
                })
                .SelectMany(x => x)
                .ToArray();
        }

        return frameworkReferences.SelectMany(x => x).ToArray();
    }

    private async Task<string[]> ResolveAdditionalReferences(string targetFramework, HashSet<string>? references,
        CancellationToken cancellationToken)
    {
        if (references.IsNullOrEmpty())
        {
            return Array.Empty<string>();
        }

        var result = await references.Select(async reference =>
        {
            if (reference.IsNullOrWhiteSpace())
                return Array.Empty<string>();

            if (reference.StartsWith("nuget:", StringComparison.OrdinalIgnoreCase))
            {
                // nuget
                var splits = reference["nuget:".Length..].Split(',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (splits.Length <= 0)
                    return Array.Empty<string>();

                NuGetVersion? version = null;
                var packageId = splits[0];
                if (splits.Length == 2)
                {
                    version = NuGetVersion.Parse(splits[1]);
                }

                return await _nugetHelper.ResolvePackageReferences(targetFramework, packageId, version, false,
                    cancellationToken);
            }

            return File.Exists(reference) ? new[] { reference } : Array.Empty<string>();
        }).WhenAll();
        return result.SelectMany(_ => _).ToArray();
    }

    public async Task<string[]> ResolveReferences(ExecOptions options, bool compilation)
    {
        var cacheKey = $"{nameof(ResolveReferences)}_{compilation}";
        return await GetOrSetCache(cacheKey, ResolveReferencesInternal, options.DisableCache);

        async Task<string[]> ResolveReferencesInternal()
        {
            var frameworkReferencesTask = ResolveFrameworkReferences(options, compilation);
            var additionalReferencesTask =
                ResolveAdditionalReferences(options.TargetFramework, options.References, options.CancellationToken);
            var references = await Task.WhenAll(frameworkReferencesTask, additionalReferencesTask);
            return references.SelectMany(_ => _).ToArray();
        }
    }

    public async Task<IEnumerable<MetadataReference>> ResolveMetadataReferences(ExecOptions options, bool compilation)
    {
        var cacheKey = $"{nameof(ResolveMetadataReferences)}_{compilation}";
        return await GetOrSetCache(cacheKey, ResolveMetadataReferencesInternal, options.DisableCache);

        async Task<IEnumerable<MetadataReference>> ResolveMetadataReferencesInternal()
        {
            var references = await ResolveReferences(options, compilation);
            return references.Select(l =>
            {
                try
                {
                    // load managed assembly only
                    _ = AssemblyName.GetAssemblyName(l);
                    return (MetadataReference)MetadataReference.CreateFromFile(l, MetadataReferenceProperties.Assembly);
                }
                catch
                {
                    return null;
                }
            }).WhereNotNull();
        }
    }

    private async Task<T> GetOrSetCache<T>(string cacheKey, Func<Task<T>> factory, bool disableCache)
    {
        if (disableCache || DisableCache)
        {
            return await factory();
        }

        if (_cache.TryGetValue(cacheKey, out var referencesCache))
        {
            return (T)referencesCache;
        }

        var refs = await factory();
        ArgumentNullException.ThrowIfNull(refs);
        _cache[cacheKey] = refs;
        return refs;
    }
}
