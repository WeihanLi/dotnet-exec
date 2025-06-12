// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.DependencyInjection;
using ReferenceResolver;

namespace IntegrationTest;

public class ReferenceResolverExtensionsTests(IServiceProvider serviceProvider)
{
    [Theory]
    [InlineData(ReferenceType.LocalFile)]
    [InlineData(ReferenceType.LocalFolder)]
    [InlineData(ReferenceType.ProjectReference)]
    [InlineData(ReferenceType.FrameworkReference)]
    [InlineData(ReferenceType.NuGetPackage)]
    public void ResolversResolveTest(ReferenceType referenceType)
    {
        var resolver = serviceProvider.GetKeyedService<IReferenceResolver>(referenceType.ToString());
        Assert.NotNull(resolver);
    }

    [Fact]
    public void NuGetHelperRegisterTest()
    {
        Assert.NotNull(serviceProvider.GetService<INuGetHelper>());
    }

    [Fact]
    public void ResolverFactoryRegisterTest()
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        Assert.NotNull(serviceProvider.GetService<IReferenceResolverFactory>());
    }

    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddReferenceResolvers();
        }
    }
}
