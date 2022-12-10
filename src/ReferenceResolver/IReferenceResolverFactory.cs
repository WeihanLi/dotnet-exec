// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace ReferenceResolver;

public interface IReferenceResolverFactory
{
    IReferenceResolver GetResolver(ReferenceType referenceType);

    Task<IEnumerable<string>> ResolveReference(string referenceWithSchema, string targetFramework, CancellationToken cancellationToken = default);

    Task<IEnumerable<MetadataReference>> ResolveMetadataReference(string referenceWithSchema, string targetFramework, CancellationToken cancellationToken = default);
}

