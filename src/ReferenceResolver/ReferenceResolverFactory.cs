// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace ReferenceResolver;

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
            ReferenceType.NuGetPackage => _serviceProvider.GetService<NuGetReferenceResolver>() ?? new NuGetReferenceResolver(new NuGetHelper(NullLoggerFactory.Instance)),
            ReferenceType.FrameworkReference =>
                _serviceProvider.GetServiceOrCreateInstance<FrameworkReferenceResolver>(),
            ReferenceType.ProjectReference => _serviceProvider.GetServiceOrCreateInstance<ProjectReferenceResolver>(),
            _ => throw new ArgumentOutOfRangeException(nameof(referenceType))
        };
    }

    public async Task<IEnumerable<string>> ResolveReference(string referenceWithSchema, string targetFramework, CancellationToken cancellationToken = default)
    {
        var (referenceWithoutSchema, resolver) = GetReferenceAndResolver(referenceWithSchema);
        return await resolver.Resolve(referenceWithoutSchema, targetFramework, cancellationToken);
    }

    public async Task<IEnumerable<MetadataReference>> ResolveMetadataReference(string referenceWithSchema, string targetFramework, CancellationToken cancellationToken = default)
    {
        var (referenceWithoutSchema, resolver) = GetReferenceAndResolver(referenceWithSchema);
        return await resolver.ResolveMetadata(referenceWithoutSchema, targetFramework, cancellationToken);
    }

    private (string reference, IReferenceResolver referenceResolver) GetReferenceAndResolver(string fullReference)
    {
        ArgumentNullException.ThrowIfNull(fullReference);
        var splits = fullReference.Split(new[] { ':' }, 2, StringSplitOptions.TrimEntries);
        var schema = "file";
        if (splits.Length == 2 && ReferenceTypeCache.Value.ContainsKey(splits[0]))
        {
            schema = splits[0];
        }
        var referenceWithoutSchema = splits.Length > 1 ? splits[1] : splits[0];

        if (!ReferenceTypeCache.Value.TryGetValue(schema, out var referenceType))
            throw new ArgumentException($"Unsupported reference type({fullReference})", nameof(fullReference));

        return (referenceWithoutSchema, GetResolver(referenceType));
    }

    private static readonly Lazy<Dictionary<string, ReferenceType>> ReferenceTypeCache = new(() =>
    {
        return Enum.GetNames<ReferenceType>()
          .ToDictionary(x => typeof(ReferenceType).GetField(x.ToString())!.GetCustomAttribute<ReferenceSchemaAttribute>()?.Schema ?? x.ToString(), Enum.Parse<ReferenceType>);
    });

    internal static readonly Lazy<Dictionary<ReferenceType, string>> ReferenceTypeSchemaCache = new(() =>
    {
        return Enum.GetNames<ReferenceType>()
            .ToDictionary(Enum.Parse<ReferenceType>, x => typeof(ReferenceType).GetField(x.ToString())!.GetCustomAttribute<ReferenceSchemaAttribute>()?.Schema ?? x.ToString());
    });
}
