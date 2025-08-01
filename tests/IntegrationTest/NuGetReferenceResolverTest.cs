﻿// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.Logging.Abstractions;
using ReferenceResolver;

namespace IntegrationTest;

public class NuGetReferenceResolverTest
{
    private readonly NuGetReferenceResolver _resolver = new(new NuGetHelper(NullLoggerFactory.Instance));

    [Fact]
    public async Task Resolve()
    {
        var references = await _resolver.Resolve("WeihanLi.Common,1.0.72", ExecOptions.DefaultTargetFramework, TestContext.Current.CancellationToken);
        Assert.NotNull(references);
        Assert.NotEmpty(references);
    }

    [Fact]
    public async Task ResolveReference()
    {
        var reference = new NuGetReference("WeihanLi.Common", "1.0.72");
        Assert.Equal($"nuget: {reference.Reference}", reference.ReferenceWithSchema());

        var references = await _resolver.Resolve(reference.Reference, ExecOptions.DefaultTargetFramework, TestContext.Current.CancellationToken);
        Assert.NotNull(references);
        Assert.NotEmpty(references);
    }
}
