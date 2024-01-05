// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReferenceResolver;

public interface IReferenceResolverFactory
{
    IReferenceResolver GetResolver(ReferenceType referenceType);

    Task<IEnumerable<string>> ResolveReferences(string referenceWithSchema, string targetFramework, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> ResolveAnalyzers(string referenceWithSchema, string targetFramework, CancellationToken cancellationToken = default);

    Task<IEnumerable<MetadataReference>> ResolveMetadataReferences(string referenceWithSchema, string targetFramework, CancellationToken cancellationToken = default);

    Task<IEnumerable<AnalyzerReference>> ResolveAnalyzerReferences(string referenceWithSchema, string targetFramework, IAnalyzerAssemblyLoader? analyzerAssemblyLoader = null, CancellationToken cancellationToken = default);
}

