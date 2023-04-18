// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using ReferenceResolver;

namespace UnitTest;

public class ReferenceResolverFactoryTest
{
    [Theory]
    [InlineData(ReferenceType.FrameworkReference)]
    [InlineData(ReferenceType.LocalFile)]
    [InlineData(ReferenceType.LocalFolder)]
    [InlineData(ReferenceType.NuGetPackage)]
    public void GetResolverTest(ReferenceType referenceType)
    {
        var resolver = new ReferenceResolverFactory(null)
            .GetResolver(referenceType);
        Assert.NotNull(resolver);
    }
}
