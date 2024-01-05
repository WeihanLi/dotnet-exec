// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Versioning;
using ReferenceResolver;

namespace IntegrationTest;
public class NuGetHelperTest
{
    private static readonly NuGetHelper NugetHelper = new(NullLoggerFactory.Instance);

    [Theory]
    [InlineData("Microsoft.NETCore.App.Ref")]
    public async Task GetPackageVersions(string packageId)
    {
        var versions = await NugetHelper.GetPackageVersions(packageId).ToArrayAsync();
        Assert.NotNull(versions);
        Assert.DoesNotContain(versions, v => v.Version.OriginalVersion?.Contains("preview") == true);
    }

    [Theory]
    [InlineData("Microsoft.NETCore.App.Ref")]
    public async Task GetPackageVersionsIncludePreview(string packageId)
    {
        var versions = await NugetHelper.GetPackageVersions(packageId, true).ToArrayAsync();
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
        var references = await NugetHelper.ResolvePackageReferences(
            ExecOptions.DefaultTargetFramework, packageId, null, true
            );
        Assert.NotEmpty(references);
        Assert.True(references.Length > 3);
    }

    [Fact]
    public async Task GetPackages()
    {
        var prefix = "WeihanLi";
        var packages = await NugetHelper.GetPackages(prefix).ToArrayAsync();
        Assert.NotEmpty(packages);
        Assert.Contains("WeihanLi.Common", packages);
    }

    [Fact]
    public async Task GetLatestVersion()
    {
        var packageId = "WeihanLi.Common";
        var result = await NugetHelper.GetLatestPackageVersion(packageId);
        Assert.NotNull(result);
        Assert.True(result >= new NuGetVersion("1.0.60"));
    }

    [Fact]
    public async Task GetVersions()
    {
        var packageId = "WeihanLi.Common";
        var result = await NugetHelper.GetPackageVersions(packageId).ToArrayAsync();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, v => v.Version is { Major: 1, Minor: 0, Patch: 60 });
    }
}
