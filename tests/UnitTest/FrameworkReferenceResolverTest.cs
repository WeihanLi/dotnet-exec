// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using ReferenceResolver;

namespace UnitTest;

public class FrameworkReferenceResolverTest
{
    private readonly FrameworkReferenceResolver _frameworkReferenceResolver = new();

    [Theory]
    [InlineData("")]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Default)]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Web)]
    [InlineData("Default")]
    [InlineData("Web")]
    public async Task Resolve(string frameworkName)
    {
        var references = await _frameworkReferenceResolver.Resolve(frameworkName, ExecOptions.DefaultTargetFramework);
        Assert.NotNull(references);
        Assert.NotEmpty(references);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ResolveDefaultFrameworkReferences(bool forCompile)
    {
        var references = await FrameworkReferenceResolver.ResolveDefaultReferences(
            ExecOptions.DefaultTargetFramework, forCompile);
        Assert.NotNull(references);
        Assert.NotEmpty(references);
    }

    [Theory]
    [InlineData("")]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Default)]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Web)]
    [InlineData("Default")]
    [InlineData("Web")]
    public async Task ResolveForCompile(string frameworkName)
    {
        var references = await FrameworkReferenceResolver.ResolveForCompile(frameworkName, ExecOptions.DefaultTargetFramework);
        Assert.NotNull(references);
        Assert.NotEmpty(references);
    }

    [Theory]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Default)]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Web)]
    [InlineData("Default")]
    [InlineData("Web")]
    public void GetRuntimePackageName(string frameworkName)
    {
        var runtimePackageName = FrameworkReferenceResolver.GetRuntimePackageName(frameworkName);
        Assert.NotNull(runtimePackageName);
        Assert.NotEmpty(runtimePackageName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Default)]
    public async Task ResolveAnalyzers(string frameworkName)
    {
        var analyzers = await _frameworkReferenceResolver.ResolveAnalyzers(frameworkName, ExecOptions.DefaultTargetFramework);
        Assert.NotNull(analyzers);
        Assert.NotEmpty(analyzers);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Default")]
    [InlineData("Web")]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Default)]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Web)]
    public void GetReferencePackageName(string frameworkName)
    {
        var packageName = FrameworkReferenceResolver.GetReferencePackageName(frameworkName);
        Assert.NotNull(packageName);
        Assert.NotEmpty(packageName);
    }

    [Theory]
    [InlineData(nameof(FrameworkReferenceResolver.FrameworkNames.Default))]
    [InlineData(nameof(FrameworkReferenceResolver.FrameworkNames.Web))]
    [InlineData(nameof(FrameworkReferenceResolver.FrameworkNames.WindowsDesktop))]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Default)]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.Web)]
    [InlineData(FrameworkReferenceResolver.FrameworkNames.WindowsDesktop)]
    public void GetImplicitUsingsForKnownFramework(string frameworkName)
    {
        var implicitUsings = FrameworkReferenceResolver.GetImplicitUsings(frameworkName);
        Assert.NotNull(implicitUsings);
        Assert.NotEmpty(implicitUsings);
    }

    [Theory]
    [InlineData("")]
    [InlineData("wtf")]
    public void GetImplicitUsingsUnknownFramework(string frameworkName)
    {
        var implicitUsings = FrameworkReferenceResolver.GetImplicitUsings(frameworkName);
        Assert.NotNull(implicitUsings);
        Assert.Empty(implicitUsings);
    }
}
