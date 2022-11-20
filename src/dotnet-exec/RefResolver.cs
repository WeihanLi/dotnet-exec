// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Frameworks;
using ReferenceResolver;
using System.Collections.Concurrent;
using System.Reflection;

namespace Exec;

public interface IRefResolver
{
    // for unit test only
    bool DisableCache { get; set; }

    Task<string[]> ResolveReferences(ExecOptions options, bool compilation);

    Task<IEnumerable<MetadataReference>> ResolveMetadataReferences(ExecOptions options, bool compilation);
}

public sealed class RefResolver : IRefResolver
{
    // for unit test only
    internal static IRefResolver InstanceForTest { get; } =
        new RefResolver(new NuGetHelper(NullLoggerFactory.Instance), new ReferenceResolverFactory(null));
    // for unit test only
    public bool DisableCache { get; set; }

    private readonly INuGetHelper _nugetHelper;
    private readonly IReferenceResolverFactory _referenceResolverFactory;
    private readonly FrameworkReferenceResolver _frameworkReferenceResolver = new();
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public RefResolver(INuGetHelper nugetHelper, IReferenceResolverFactory referenceResolverFactory)
    {
        _nugetHelper = nugetHelper;
        _referenceResolverFactory = referenceResolverFactory;
    }

    private async Task<string[]> ResolveFrameworkReferences(ExecOptions options, bool compilation)
    {
        var frameworks = Helper.GetDependencyFrameworks(options);
        var referenceFrameworks = options.References
            .Select(r => r.StartsWith("framework:", StringComparison.OrdinalIgnoreCase) ? r["framework:".Length..].Trim() : null)
            .WhereNotNull();
        var frameworkReferences = await frameworks.Union(referenceFrameworks)
            .Select(async framework =>
            {
                if (compilation)
                {
                    var references =
                        await _frameworkReferenceResolver.ResolveForCompile(framework, options.TargetFramework, options.CancellationToken)
                            .ContinueWith(r => r.Result.ToArray());
                    if (references.HasValue()) return references;

                    if (options.UseRefAssembliesForCompile)
                    {
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
                }

                var runtimeReferences = await _frameworkReferenceResolver.Resolve(framework, options.TargetFramework, options.CancellationToken)
                    .ContinueWith(r=> r.Result.ToArray());
                if (runtimeReferences.IsNullOrEmpty())
                {
                    // fallback to nugetFramework
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
                return runtimeReferences;
            })
            .WhenAll();
        if (options.IncludeWideReferences)
        {
            return frameworkReferences.Append(new[]
                {
                    typeof(Microsoft.Extensions.Configuration.ConfigurationBuilder).Assembly.Location,
                    typeof(ServiceCollection).Assembly.Location,
                    typeof(LoggerFactory).Assembly.Location,
                    typeof(Newtonsoft.Json.JsonConvert).Assembly.Location,
                    typeof(DependencyResolver).Assembly.Location,
                })
                .SelectMany(x => x)
                .Distinct()
                .ToArray();
        }
        return frameworkReferences.SelectMany(x => x).Distinct().ToArray();
    }

    private async Task<string[]> ResolveAdditionalReferences(string targetFramework, HashSet<string>? references,
        CancellationToken cancellationToken)
    {
        if (references.IsNullOrEmpty())
        {
            return Array.Empty<string>();
        }
        // non-framework references
        var result = await references
            .Where(x => !x.StartsWith("framework:", StringComparison.OrdinalIgnoreCase))
            .Select(reference => _referenceResolverFactory.ResolveReference(reference, targetFramework, cancellationToken))
            .WhenAll();
        return result.SelectMany(_ => _).Distinct().ToArray();
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
