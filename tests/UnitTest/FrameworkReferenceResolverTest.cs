// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

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
        var references = await FrameworkReferenceResolver.ResolveForCompile(frameworkName, ExecOptions.DefaultTargetFramework);
        Assert.NotNull(references);
        Assert.NotEmpty(references);
    }
}
