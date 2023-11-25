// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
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
        var versions = await NugetHelper.GetPackageVersions(packageId);
        Assert.NotNull(versions);
        Assert.DoesNotContain(versions, v => v.OriginalVersion?.Contains("preview") == true);
    }
    
    [Theory]
    [InlineData("Microsoft.NETCore.App.Ref")]
    public async Task GetPackageVersionsIncludePreview(string packageId)
    {
        var versions = (await NugetHelper.GetPackageVersions(packageId, true)).ToArray();
        Assert.NotNull(versions);
        Assert.Contains(versions, v => v.OriginalVersion?.Contains("preview") == true);
        var maxVersion = versions.Max();
        Assert.NotNull(maxVersion);
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
        var packages = (await NugetHelper.GetPackages(prefix)).ToArray();
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
        var result = (await NugetHelper.GetPackageVersions(packageId)).ToArray();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
