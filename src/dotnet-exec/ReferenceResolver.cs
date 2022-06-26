// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Versioning;

namespace Exec;

public interface IReferenceResolver
{
    Task<string[]> ResolveReferences(ExecOptions options, bool compilation);
}

public sealed class ReferenceResolver : IReferenceResolver
{
    // for unit test only
    internal static IReferenceResolver InstanceForTest { get; } = new ReferenceResolver(new NuGetHelper(NullLoggerFactory.Instance));

    private readonly INuGetHelper _nugetHelper;

    public ReferenceResolver(INuGetHelper nugetHelper)
    {
        _nugetHelper = nugetHelper;
    }

    private async Task<string[]> ResolveFrameworkReferences(ExecOptions options, bool compilation)
    {
        var frameworks = Helper.GetDependencyFrameworks();
        var frameworkReferences = await frameworks.Select(async framework =>
        {
            if (compilation)
            {
                var references =
                    Helper.ResolveFrameworkReferencesViaSdkPacks(framework, options.TargetFramework);
                if (references.IsNullOrEmpty())
                {
                    var packageId = Helper.GetReferencePackageName(framework);
                    return await _nugetHelper.ResolvePackageReferences(options.TargetFramework, packageId, null, true,
                        options.CancellationToken);
                }
                return references;
            }
            return Helper.ResolveFrameworkReferencesViaRuntimeShared(framework, options.TargetFramework);
        }).WhenAll();
        if (options.IncludeWideReferences)
        {
            return frameworkReferences.Append(new[]
            {
                typeof(DependencyResolver).Assembly.Location, typeof(Newtonsoft.Json.JsonConvert).Assembly.Location
            })
                .SelectMany(x => x)
                .ToArray();
        }
        return frameworkReferences.SelectMany(x => x).ToArray();
    }

    private async Task<string[]> ResolveAdditionalReferences(string targetFramework, string[]? references,
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
                var splits = reference["nuget:".Length..].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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
        var frameworkReferencesTask = ResolveFrameworkReferences(options, compilation);
        var additionalReferencesTask =
             ResolveAdditionalReferences(options.TargetFramework, options.References, options.CancellationToken);
        var references = await Task.WhenAll(frameworkReferencesTask, additionalReferencesTask);
        return references.SelectMany(_ => _).ToArray();
    }
}

