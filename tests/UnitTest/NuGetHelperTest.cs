// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.Logging.Abstractions;
using ReferenceResolver;

namespace UnitTest;
public class NuGetHelperTest
{
    private static readonly NuGetHelper NugetHelper = new NuGetHelper(NullLoggerFactory.Instance);

    [Theory]
    [InlineData("Microsoft.NETCore.App.Ref")]
    public async Task GetPackageVersions(string packageId)
    {
        var versions = await NugetHelper.GetPackageVersions(packageId);
        Assert.NotNull(versions);
        Assert.DoesNotContain(versions, v => v.OriginalVersion?.Contains("preview") == true);
    }

    // framework reference test
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
}
