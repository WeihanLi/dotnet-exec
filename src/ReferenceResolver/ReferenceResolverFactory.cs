// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace ReferenceResolver;

public interface IReferenceResolverFactory
{
    IReferenceResolver GetResolver(ReferenceType referenceType);

    Task<IEnumerable<MetadataReference>> ResolveMetadataReference(string reference, string targetFramework);
}

public sealed class ReferenceResolverFactory : IReferenceResolverFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ReferenceResolverFactory(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider ?? DependencyResolver.Current;
    }

    public IReferenceResolver GetResolver(ReferenceType referenceType)
    {
        return referenceType switch
        {
            ReferenceType.LocalFile => _serviceProvider.GetServiceOrCreateInstance<FileReferenceResolver>(),
            ReferenceType.LocalFolder => _serviceProvider.GetServiceOrCreateInstance<FolderReferenceResolver>(),
            ReferenceType.NuGetPackage => _serviceProvider.GetServiceOrCreateInstance<NuGetReferenceResolver>(),
            ReferenceType.FrameworkReference =>
                _serviceProvider.GetServiceOrCreateInstance<FrameworkReferenceResolver>(),
            _ => throw new ArgumentOutOfRangeException(nameof(referenceType))
        };
    }

    public async Task<IEnumerable<MetadataReference>> ResolveMetadataReference(string reference, string targetFramework)
    {
        ArgumentNullException.ThrowIfNull(reference);
        var splits = reference.Split(new[] { ':' }, 2, StringSplitOptions.TrimEntries);
        var schema = "file";
        if (splits.Length == 2)
        {
            schema = splits[0];
        }

        if (!ReferenceTypeCache.Value.TryGetValue(schema, out var referenceType))
            throw new ArgumentException($"Unsupported reference type({reference})", nameof(reference));

        var resolver = GetResolver(referenceType);
        var referenceWithoutSchema = splits.Length > 1 ? splits[1] : splits[0];
        return await resolver.ResolveMetadata(referenceWithoutSchema, targetFramework);
    }

    private static readonly Lazy<Dictionary<string, ReferenceType>> ReferenceTypeCache = new(() =>
    {
        return Enum.GetNames<ReferenceType>().ToDictionary(x => x, Enum.Parse<ReferenceType>);
    });
}
