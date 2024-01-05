// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.DependencyInjection;
using ReferenceResolver;

namespace IntegrationTest;

public class ReferenceResolverExtensionsTests(IServiceProvider serviceProvider)
{
    [Fact]
    public void ResolversRegisterTest()
    {
        var resolvers = serviceProvider.GetServices<IReferenceResolver>().ToArray();
        Assert.Equal(5, resolvers.Length);
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
