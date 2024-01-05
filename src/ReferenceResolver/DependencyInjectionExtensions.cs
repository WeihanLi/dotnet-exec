// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ReferenceResolver;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddReferenceResolvers(this IServiceCollection serviceCollection)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        serviceCollection.AddLogging();
        serviceCollection.TryAddSingleton<INuGetHelper, NuGetHelper>();
        // reference resolver
        serviceCollection.TryAddSingleton<IReferenceResolverFactory, ReferenceResolverFactory>();
        serviceCollection
            .TryAddReferenceResolver<FileReferenceResolver>()
            .TryAddReferenceResolver<FolderReferenceResolver>()
            .TryAddReferenceResolver<FrameworkReferenceResolver>()
            .TryAddReferenceResolver<NuGetReferenceResolver>()
            .TryAddReferenceResolver<ProjectReferenceResolver>()
            ;
        return serviceCollection;
    }

    public static IServiceCollection TryAddReferenceResolver<TResolver>(this IServiceCollection serviceCollection)
        where TResolver : class, IReferenceResolver
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        serviceCollection.TryAddSingleton<TResolver>();
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IReferenceResolver, TResolver>());
        return serviceCollection;
    }
}
