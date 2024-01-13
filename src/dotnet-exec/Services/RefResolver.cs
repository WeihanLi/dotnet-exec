// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Frameworks;
using ReferenceResolver;
using System.Collections.Concurrent;
using System.Reflection;

namespace Exec.Services;

public sealed class RefResolver(INuGetHelper nugetHelper, IReferenceResolverFactory referenceResolverFactory)
    : IRefResolver
{
    // for unit test only
    internal static IRefResolver InstanceForTest { get; } =
        new RefResolver(new NuGetHelper(NullLoggerFactory.Instance), new ReferenceResolverFactory(null));
    // for unit test only
    public bool DisableCache { get; set; }

    private readonly FrameworkReferenceResolver _frameworkReferenceResolver = new();
    private readonly ConcurrentDictionary<string, object> _cache = new();

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
            return references.Flatten().ToArray();
        }
    }

    public async Task<MetadataReference[]> ResolveMetadataReferences(ExecOptions options, bool compilation)
    {
        var cacheKey = $"{nameof(ResolveMetadataReferences)}_{compilation}";
        return await GetOrSetCache(cacheKey, ResolveMetadataReferencesInternal, options.DisableCache);

        async Task<MetadataReference[]> ResolveMetadataReferencesInternal()
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
            }).WhereNotNull().ToArray();
        }
    }

    public async Task<IEnumerable<string>> ResolveAnalyzers(ExecOptions options)
    {
        var frameworkAnalyzersTask = ResolveFrameworkAnalyzers(options);
        var additionalAnalyzersTask = ResolveAdditionalAnalyzer(options.TargetFramework, options.References, options.CancellationToken);
        var references = await Task.WhenAll(frameworkAnalyzersTask, additionalAnalyzersTask);
        return references.Flatten();
    }

    public async Task<IEnumerable<AnalyzerReference>> ResolveAnalyzerReferences(ExecOptions options)
    {
        var analyzers = await ResolveAnalyzers(options);
        return analyzers.Select(x => new AnalyzerFileReference(x, CustomLoadContext.Current.Value ?? AnalyzerAssemblyLoader.Instance));
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
                        await FrameworkReferenceResolver.ResolveForCompile(framework, options.TargetFramework, options.CancellationToken)
                            .ContinueWith(r => r.Result.ToArray());
                    if (references.HasValue()) return references;

                    if (options.UseRefAssembliesForCompile)
                    {
                        var packageId = FrameworkReferenceResolver.GetReferencePackageName(framework);
                        var nugetFramework = NuGetFramework.Parse(options.TargetFramework);
                        var version = await nugetHelper.GetPackageVersions(packageId, true, x => x.Major == nugetFramework.Version.Major
                            && x.Minor == nugetFramework.Version.Minor, null, options.CancellationToken)
                            .OrderByDescending(x => x.Version)
                            .FirstOrDefaultAsync(options.CancellationToken);
                        return await nugetHelper.ResolvePackageReferences(options.TargetFramework, packageId, version.Version,
                            true,
                            options.CancellationToken);
                    }
                }

                var runtimeReferences = await _frameworkReferenceResolver.Resolve(framework, options.TargetFramework, options.CancellationToken)
                    .ContinueWith(r => r.Result.ToArray());
                if (runtimeReferences.IsNullOrEmpty())
                {
                    // fallback to nuget package
                    var packageId = FrameworkReferenceResolver.GetRuntimePackageName(framework);
                    var nugetFramework = NuGetFramework.Parse(options.TargetFramework);
                    var version = await nugetHelper.GetPackageVersions(packageId, true, x => x.Major == nugetFramework.Version.Major
                        && x.Minor == nugetFramework.Version.Minor, null, options.CancellationToken)
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefaultAsync(options.CancellationToken);
                    return await nugetHelper.ResolvePackageReferences(options.TargetFramework, packageId, version.Version,
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
                    typeof(Microsoft.Extensions.Configuration.IConfiguration).Assembly.Location,
                    typeof(Microsoft.Extensions.Configuration.ConfigurationManager).Assembly.Location,
                    typeof(ServiceCollection).Assembly.Location,
                    typeof(ServiceProvider).Assembly.Location,
                    typeof(ILoggerFactory).Assembly.Location,
                    typeof(LoggerFactory).Assembly.Location,
                    typeof(Microsoft.Extensions.Options.Options).Assembly.Location,
                    typeof(Microsoft.Extensions.Primitives.ChangeToken).Assembly.Location,
                    typeof(Newtonsoft.Json.JsonConvert).Assembly.Location,
                    typeof(DependencyResolver).Assembly.Location,
                })
                .SelectMany(x => x)
                .Distinct()
                .ToArray();
        }
        return frameworkReferences.Flatten().Distinct().ToArray();
    }

    private async Task<string[]> ResolveAdditionalReferences(string targetFramework, ICollection<string>? references,
        CancellationToken cancellationToken)
    {
        if (references.IsNullOrEmpty())
        {
            return Array.Empty<string>();
        }
        // non-framework references
        var result = await references
            .Where(x => !x.StartsWith("framework:", StringComparison.OrdinalIgnoreCase))
            .Select(reference => referenceResolverFactory.ResolveReferences(reference, targetFramework, cancellationToken))
            .WhenAll();
        return result.Flatten().Distinct().ToArray();
    }

    private async Task<IEnumerable<string>> ResolveFrameworkAnalyzers(ExecOptions options)
    {
        var frameworks = Helper.GetDependencyFrameworks(options);
        var referenceFrameworks = options.References
            .Select(r => r.StartsWith("framework:", StringComparison.OrdinalIgnoreCase) ? r["framework:".Length..].Trim() : null)
            .WhereNotNull();
        var frameworkReferences = await frameworks.Union(referenceFrameworks)
            .Select(async framework =>
            {
                var references =
                    await _frameworkReferenceResolver.ResolveAnalyzers(framework, options.TargetFramework, options.CancellationToken)
                        .ContinueWith(r => r.Result.ToArray());
                if (references.HasValue()) return references;

                var packageId = FrameworkReferenceResolver.GetReferencePackageName(framework);
                var nugetFramework = NuGetFramework.Parse(options.TargetFramework);
                var version = await nugetHelper.GetPackageVersions(packageId, true, x => x.Major == nugetFramework.Version.Major
                    && x.Minor == nugetFramework.Version.Minor, null, options.CancellationToken)
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefaultAsync(options.CancellationToken);
                return await nugetHelper.ResolvePackageAnalyzerReferences(options.TargetFramework, packageId, version.Version,
                    true,
                    options.CancellationToken);
            })
            .WhenAll();
        return frameworkReferences.Flatten();
    }

    private async Task<IEnumerable<string>> ResolveAdditionalAnalyzer(string targetFramework, ICollection<string>? references,
        CancellationToken cancellationToken)
    {
        if (references.IsNullOrEmpty())
        {
            return Enumerable.Empty<string>();
        }
        // non-framework references
        var result = await references
            .Where(x => !x.StartsWith("framework:", StringComparison.OrdinalIgnoreCase))
            .Select(reference => referenceResolverFactory.ResolveAnalyzers(reference, targetFramework, cancellationToken))
            .WhenAll();
        return result.Flatten().Distinct();
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
