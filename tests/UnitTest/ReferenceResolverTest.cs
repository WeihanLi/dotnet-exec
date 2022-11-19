// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging.Abstractions;
using ReferenceResolver;

namespace UnitTest;
public class ReferenceResolverTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ResolveFrameworkReferencesOnly(bool includeWide)
    {
        var references = await RefResolver.InstanceForTest.ResolveReferences(new ExecOptions()
        {
            IncludeWideReferences = includeWide
        }, true);
        Assert.NotEmpty(references);
        Assert.Contains(references, x => x.Contains("System.Text.Json.dll"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ResolveFrameworkRuntimeReferencesOnly(bool includeWide)
    {
        var references = await RefResolver.InstanceForTest.ResolveReferences(new ExecOptions()
        {
            IncludeWideReferences = includeWide
        }, false);
        Assert.NotEmpty(references);
        Assert.Contains(references, x => x.Contains("System.Text.Json.dll"));
    }

    [Theory]
    [InlineData("nuget:WeihanLi.Common", true)]
    [InlineData("nuget:WeihanLi.Common", false)]
    public async Task ResolveFrameworkReferencesWithAdditional(string reference, bool includeWide)
    {
        var references = await RefResolver.InstanceForTest.ResolveReferences(new ExecOptions()
        {
            IncludeWideReferences = includeWide,
            References = new(reference.Split(';', StringSplitOptions.RemoveEmptyEntries))
        }, true);
        Assert.NotEmpty(references);
        Assert.Contains(references, x => x.Contains("WeihanLi.Common.dll"));
    }

    [Theory]
    [InlineData("nuget:WeihanLi.Common", true)]
    [InlineData("nuget:WeihanLi.Common", false)]
    public async Task ResolveFrameworkRuntimeReferencesWithAdditional(string reference, bool includeWide)
    {
        var references = await RefResolver.InstanceForTest.ResolveReferences(new ExecOptions()
        {
            IncludeWideReferences = includeWide,
            References = new(reference.Split(';', StringSplitOptions.RemoveEmptyEntries))
        }, false);
        Assert.NotEmpty(references);
        Assert.Contains(references, x => x.Contains("WeihanLi.Common.dll"));
    }
}
