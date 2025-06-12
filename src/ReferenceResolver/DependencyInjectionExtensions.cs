// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace ReferenceResolver;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddReferenceResolvers(this IServiceCollection serviceCollection)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        serviceCollection.AddLogging();
        serviceCollection.TryAddSingleton<INuGetHelper, NuGetHelper>();
        // reference resolvers
        serviceCollection.TryAddSingleton<IReferenceResolverFactory, ReferenceResolverFactory>();
        serviceCollection.TryAddReferenceResolver<FileReferenceResolver>(ReferenceType.LocalFile);
        serviceCollection.TryAddReferenceResolver<FolderReferenceResolver>(ReferenceType.LocalFolder);
        serviceCollection.TryAddReferenceResolver<FrameworkReferenceResolver>(ReferenceType.FrameworkReference);
        serviceCollection.TryAddReferenceResolver<ProjectReferenceResolver>(ReferenceType.ProjectReference);
        serviceCollection.TryAddReferenceResolver<NuGetReferenceResolver>(ReferenceType.NuGetPackage);
        return serviceCollection;
    }

    private static IServiceCollection TryAddReferenceResolver
        <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResolver>
        (this IServiceCollection serviceCollection, ReferenceType referenceType)
        where TResolver : class, IReferenceResolver
    {
        serviceCollection.TryAddSingleton<TResolver>();
        serviceCollection.AddKeyedSingleton<IReferenceResolver, TResolver>(referenceType.ToString());
        return serviceCollection;
    }
}
