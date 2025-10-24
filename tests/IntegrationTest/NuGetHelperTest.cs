// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Versioning;
using ReferenceResolver;

namespace IntegrationTest;

public class NuGetHelperTest
{
    private static readonly NuGetHelper NuGetHelper = new(NullLoggerFactory.Instance);

    [Theory]
    [InlineData("Microsoft.NETCore.App.Ref")]
    public async Task GetPackageVersions(string packageId)
    {
        var versions = await NuGetHelper.GetPackageVersions(packageId, cancellationToken: TestContext.Current.CancellationToken).ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(versions);
        Assert.DoesNotContain(versions, v => v.Version.OriginalVersion?.Contains("preview") == true);
    }

    [Theory]
    [InlineData("Microsoft.NETCore.App.Ref")]
    public async Task GetPackageVersionsIncludePreview(string packageId)
    {
        var versions = await NuGetHelper.GetPackageVersions(packageId, true, cancellationToken: TestContext.Current.CancellationToken).ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(versions);
        Assert.NotEmpty(versions);
        Assert.Contains(versions, v => v.Version.OriginalVersion?.Contains("preview") == true);
        var maxVersion = versions.MaxBy(x => x.Version);
        Assert.NotNull(maxVersion.Source);
    }

    [Theory]
    [InlineData("WeihanLi.Npoi")]
    public async Task ResolvePackageReference(string packageId)
    {
        var references = await NuGetHelper.ResolvePackageReferences(
            ExecOptions.DefaultTargetFramework, packageId, null, true, cancellationToken: TestContext.Current.CancellationToken
            );
        Assert.NotEmpty(references);
        Assert.True(references.Length > 3);
    }

    [Fact]
    public async Task GetPackages()
    {
        var prefix = "WeihanLi";
        var packages = await NuGetHelper.GetPackages(prefix, cancellationToken: TestContext.Current.CancellationToken)
            .ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.NotEmpty(packages);
        Assert.Contains("WeihanLi.Common", packages.SelectMany(p => p.Packages));
    }

    [Fact]
    public async Task SearchPackages()
    {
        var prefix = "WeihanLi";
        var result = await NuGetHelper.SearchPackages(prefix, cancellationToken: TestContext.Current.CancellationToken)
           .ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.NotEmpty(result);
        var packages = result.SelectMany(x => x.SearchResult).ToArray();
        Assert.Contains("WeihanLi.Common", packages.Select(x => x.Identity.Id), StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetLatestVersion()
    {
        var packageId = "WeihanLi.Common";
        var result = await NuGetHelper.GetLatestPackageVersion(packageId, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.True(result >= new NuGetVersion("1.0.72"));
    }

    [Fact]
    public async Task GetVersions()
    {
        var packageId = "WeihanLi.Common";
        var result = await NuGetHelper.GetPackageVersions(packageId, cancellationToken: TestContext.Current.CancellationToken)
            .ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, v => v.Version is { Major: 1, Minor: 0, Patch: 72 });
    }

    [Fact]
    public void GetSources()
    {
        var sources = NuGetHelper.GetSources();
        Assert.NotNull(sources);
        Assert.NotEmpty(sources);
    }

    [Theory]
    [InlineData("WeihanLi.Common")]
    public void GetSourcesWithPackageId(string packageId)
    {
        var sources = NuGetHelper.GetSources(packageId);
        Assert.NotNull(sources);
        Assert.NotEmpty(sources);
    }

    [Fact]
    public void SpecificNuGetConfig()
    {
        var defaultNuGetConfigPath = "./Assets/nuget.config";
        Environment.SetEnvironmentVariable(NuGetHelper.NuGetConfigEnvName, defaultNuGetConfigPath);
        var helper = new NuGetHelper(NullLoggerFactory.Instance);
        Assert.NotEmpty(helper.GetSources());
    }

    [Fact]
    public async Task ResolveMultiplePackageReferences()
    {
        var references = new[]
        {
            new NuGetReference("WeihanLi.Common", "1.0.72"),
            new NuGetReference("Newtonsoft.Json", "13.0.3")
        };
        var result = await NuGetHelper.ResolvePackageReferences(
            ExecOptions.DefaultTargetFramework, references, cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotEmpty(result);
        // Should contain assemblies from both packages
        Assert.Contains(result, r => r.Contains("WeihanLi.Common", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result, r => r.Contains("Newtonsoft.Json", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ResolveMultiplePackageReferencesWithoutVersions()
    {
        var references = new[]
        {
            new NuGetReference("WeihanLi.Common"),
            new NuGetReference("Newtonsoft.Json")
        };
        var result = await NuGetHelper.ResolvePackageReferences(
            ExecOptions.DefaultTargetFramework, references, cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotEmpty(result);
        // Should contain assemblies from both packages
        Assert.Contains(result, r => r.Contains("WeihanLi.Common", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result, r => r.Contains("Newtonsoft.Json", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ResolveMultiplePackageReferencesWithVersionConflict()
    {
        // Test that when same package is specified with different versions, highest version wins
        var references = new[]
        {
            new NuGetReference("Newtonsoft.Json", "12.0.1"),
            new NuGetReference("Newtonsoft.Json", "13.0.3")
        };
        var result = await NuGetHelper.ResolvePackageReferences(
            ExecOptions.DefaultTargetFramework, references, cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotEmpty(result);
        // Should resolve to version 13.0.3 (higher version)
        Assert.Contains(result, r => r.Contains("Newtonsoft.Json", StringComparison.OrdinalIgnoreCase) && r.Contains("13.0.3"));
        // Should not contain 12.0.1
        Assert.DoesNotContain(result, r => r.Contains("Newtonsoft.Json", StringComparison.OrdinalIgnoreCase) && r.Contains("12.0.1"));
    }

    [Fact]
    public async Task ResolveEmptyPackageReferences()
    {
        var references = Array.Empty<NuGetReference>();
        var result = await NuGetHelper.ResolvePackageReferences(
            ExecOptions.DefaultTargetFramework, references, cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Empty(result);
    }
}
