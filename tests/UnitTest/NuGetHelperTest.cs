// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTest;
public class NuGetHelperTest
{
    private readonly INuGetHelper _nugetHelper = new NuGetHelper(NullLoggerFactory.Instance);

    [Theory]
    [InlineData(FrameworkNames.Default)]
    public async Task GetPackageVersions(string packageId)
    {
        var versions = await _nugetHelper.GetPackageVersions(packageId, false);
        Assert.NotNull(versions);
        Assert.DoesNotContain(versions, v => v.OriginalVersion.Contains("preview"));
    }

    [Theory]
    [InlineData(FrameworkReferencePackages.Default)]
    public async Task GetPackageVersionsIncludePreview(string packageId)
    {
        var versions = (await _nugetHelper.GetPackageVersions(packageId, true)).ToArray();
        Assert.NotNull(versions);
        Assert.Contains(versions, v => v.OriginalVersion.Contains("preview"));
        var maxVersion = versions.Max();
        Assert.NotNull(maxVersion);
    }

    [Theory]
    [InlineData("WeihanLi.Npoi")]
    public async Task ResolvePackageReference(string packageId)
    {
        var references = await _nugetHelper.ResolvePackageReferences(
            ExecOptions.DefaultTargetFramework, packageId, null, true
            );
        Assert.NotEmpty(references);
        Assert.True(references.Length > 3);
    }
}
