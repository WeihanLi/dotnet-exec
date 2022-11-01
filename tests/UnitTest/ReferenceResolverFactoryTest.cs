// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

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
