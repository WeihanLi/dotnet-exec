// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging.Abstractions;
using ReferenceResolver;

namespace UnitTest;

public class NuGetReferenceResolverTest
{
    private readonly NuGetReferenceResolver _resolver =
        new(new NuGetHelper(NullLoggerFactory.Instance));

    [Fact]
    public async Task Resolve()
    {
        var references =  await _resolver.Resolve("WeihanLi.Common,1.0.55", ExecOptions.DefaultTargetFramework);
        Assert.NotNull(references);
        Assert.NotEmpty(references);
    }

    [Fact]
    public async Task ResolveReference()
    {
        IReference reference = new NuGetReference("WeihanLi.Common", "1.0.55");
        Assert.Equal($"nuget: {reference.Reference}", reference.ReferenceWithSchema);
        
        var references =  await _resolver.Resolve(reference.Reference, ExecOptions.DefaultTargetFramework);
        Assert.NotNull(references);
        Assert.NotEmpty(references);
    }
}
