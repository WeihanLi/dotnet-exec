// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
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
        var versions = await NuGetHelper.GetPackageVersions(packageId).ToArrayAsync();
        Assert.NotNull(versions);
        Assert.DoesNotContain(versions, v => v.Version.OriginalVersion?.Contains("preview") == true);
    }

    [Theory]
    [InlineData("Microsoft.NETCore.App.Ref")]
    public async Task GetPackageVersionsIncludePreview(string packageId)
    {
        var versions = await NuGetHelper.GetPackageVersions(packageId, true).ToArrayAsync();
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
            ExecOptions.DefaultTargetFramework, packageId, null, true
            );
        Assert.NotEmpty(references);
        Assert.True(references.Length > 3);
    }

    [Fact]
    public async Task GetPackages()
    {
        var prefix = "WeihanLi";
        var packages = await NuGetHelper.GetPackages(prefix).ToArrayAsync();
        Assert.NotEmpty(packages);
        Assert.Contains("WeihanLi.Common", packages.SelectMany(p => p.Packages));
    }

    [Fact]
    public async Task SearchPackages()
    {
        var prefix = "WeihanLi";
        var result = await NuGetHelper.SearchPackages(prefix).ToArrayAsync();
        Assert.NotEmpty(result);
        var packages = result.SelectMany(x => x.SearchResult).ToArray();
        Assert.Contains("WeihanLi.Common", packages.Select(x => x.Identity.Id), StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetLatestVersion()
    {
        var packageId = "WeihanLi.Common";
        var result = await NuGetHelper.GetLatestPackageVersion(packageId);
        Assert.NotNull(result);
        Assert.True(result >= new NuGetVersion("1.0.61"));
    }

    [Fact]
    public async Task GetVersions()
    {
        var packageId = "WeihanLi.Common";
        var result = await NuGetHelper.GetPackageVersions(packageId).ToArrayAsync();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, v => v.Version is { Major: 1, Minor: 0, Patch: 61 });
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
}
