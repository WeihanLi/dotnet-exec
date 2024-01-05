// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace ReferenceResolver;

public sealed class ReferenceResolverFactory : IReferenceResolverFactory
{
    private static readonly char[] ReferenceSchemaSeparator = [':'];

    private readonly IServiceProvider _serviceProvider;

    public ReferenceResolverFactory(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider ?? DependencyResolver.Current;
    }

    public static IReference ParseReference(string referenceWithSchema)
    {
        var (referenceType, reference) = GetReferenceType(referenceWithSchema);
        return referenceType switch
        {
            ReferenceType.LocalFile => new FileReference(reference),
            ReferenceType.LocalFolder => new FolderReference(reference),
            ReferenceType.NuGetPackage => NuGetReference.Parse(reference),
            ReferenceType.FrameworkReference => new FrameworkReference(reference),
            ReferenceType.ProjectReference => new ProjectReference(reference),
            _ => throw new InvalidOperationException($"Not supported reference {referenceWithSchema}")
        };
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

    public async Task<IEnumerable<string>> ResolveReferences(string referenceWithSchema, string targetFramework, CancellationToken cancellationToken = default)
    {
        var (referenceWithoutSchema, resolver) = GetReferenceAndResolver(referenceWithSchema);
        return await resolver.Resolve(referenceWithoutSchema, targetFramework, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<string>> ResolveAnalyzers(string referenceWithSchema, string targetFramework, CancellationToken cancellationToken = default)
    {
        var (referenceWithoutSchema, resolver) = GetReferenceAndResolver(referenceWithSchema);
        return await resolver.ResolveAnalyzers(referenceWithoutSchema, targetFramework, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<MetadataReference>> ResolveMetadataReferences(string referenceWithSchema, string targetFramework, CancellationToken cancellationToken = default)
    {
        var (referenceWithoutSchema, resolver) = GetReferenceAndResolver(referenceWithSchema);
        return await resolver.ResolveMetadataReferences(referenceWithoutSchema, targetFramework, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<AnalyzerReference>> ResolveAnalyzerReferences(string referenceWithSchema, string targetFramework,
        IAnalyzerAssemblyLoader? analyzerAssemblyLoader = null, CancellationToken cancellationToken = default)
    {
        var (referenceWithoutSchema, resolver) = GetReferenceAndResolver(referenceWithSchema);
        return await resolver.ResolveAnalyzerReferences(referenceWithoutSchema, targetFramework, analyzerAssemblyLoader, cancellationToken)
            .ConfigureAwait(false);
    }

    private (string reference, IReferenceResolver referenceResolver) GetReferenceAndResolver(string fullReference)
    {
        ArgumentNullException.ThrowIfNull(fullReference);
        var (referenceType, reference) = GetReferenceType(fullReference);
        return (reference, GetResolver(referenceType));
    }

    private static (ReferenceType ReferenceType, string Reference) GetReferenceType(string fullReference)
    {
        var splits = fullReference.Split(ReferenceSchemaSeparator, 2, StringSplitOptions.TrimEntries);
        var schema = "file";
        if (splits.Length == 2 && ReferenceTypeCache.Value.ContainsKey(splits[0]))
        {
            schema = splits[0];
        }
        var referenceWithoutSchema = splits.Length > 1 ? splits[1] : splits[0];

        if (ReferenceTypeCache.Value.TryGetValue(schema, out var referenceType))
            return (referenceType, referenceWithoutSchema);

        if (File.Exists(fullReference))
            return (ReferenceType.LocalFile, fullReference);

        throw new ArgumentException($"Unsupported reference type({fullReference})", nameof(fullReference));
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
