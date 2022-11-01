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
        var references =  await _resolver.Resolve("WeihanLi.Common,1.0.54", ExecOptions.DefaultTargetFramework);
        Assert.NotNull(references);
        Assert.NotEmpty(references);
    }
}
