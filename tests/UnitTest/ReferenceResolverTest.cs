// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTest;
public class ReferenceResolverTest
{
    private readonly IReferenceResolver _referenceResolver =
        new ReferenceResolver(new NuGetHelper(NullLoggerFactory.Instance));

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ResolveFrameworkReferencesOnly(bool includeWide)
    {
        var references = await _referenceResolver.ResolveReferences(new ExecOptions()
        {
            IncludeWideReferences = includeWide
        }, true);
        Assert.NotEmpty(references);
        Assert.Contains(references, x => x.Contains("Microsoft.Extensions.DependencyInjection.dll"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ResolveFrameworkRuntimeReferencesOnly(bool includeWide)
    {
        var references = await _referenceResolver.ResolveReferences(new ExecOptions()
        {
            IncludeWideReferences = includeWide
        }, false);
        Assert.NotEmpty(references);
        Assert.Contains(references, x => x.Contains("Microsoft.Extensions.DependencyInjection.dll"));
    }

    [Theory]
    [InlineData("nuget:WeihanLi.Common", true)]
    [InlineData("nuget:WeihanLi.Common", false)]
    public async Task ResolveFrameworkReferencesWithAdditional(string reference, bool includeWide)
    {
        var references = await _referenceResolver.ResolveReferences(new ExecOptions()
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
        var references = await _referenceResolver.ResolveReferences(new ExecOptions()
        {
            IncludeWideReferences = includeWide,
            References = new(reference.Split(';', StringSplitOptions.RemoveEmptyEntries))
        }, false);
        Assert.NotEmpty(references);
        Assert.Contains(references, x => x.Contains("WeihanLi.Common.dll"));
    }
}
