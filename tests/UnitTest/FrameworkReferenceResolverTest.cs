// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using ReferenceResolver;

namespace UnitTest;

public class FrameworkReferenceResolverTest
{
    private readonly FrameworkReferenceResolver _frameworkReferenceResolver = new();
    
    [Theory]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Default)]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Web)]
    public async Task Resolve(string frameworkName)
    {
        var references = await _frameworkReferenceResolver.Resolve(frameworkName, ExecOptions.DefaultTargetFramework);
        Assert.NotNull(references);
        Assert.NotEmpty(references);
    }

    [Theory]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Default)]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Web)]
    public async Task ResolveForCompile(string frameworkName)
    {
        var references = await _frameworkReferenceResolver.ResolveForCompile(frameworkName, ExecOptions.DefaultTargetFramework);
        Assert.NotNull(references);
        Assert.NotEmpty(references);
    }
}
