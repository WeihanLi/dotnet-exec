// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics;
using System.Reflection;

namespace ReferenceResolver;

public interface IReferenceResolver
{
    ReferenceType ReferenceType { get; }
    Task<IEnumerable<string>> Resolve(string reference, string targetFramework, CancellationToken cancellationToken = default);
    Task<IEnumerable<MetadataReference>> ResolveMetadataReferences(string reference, string targetFramework, CancellationToken cancellationToken = default)
        => Resolve(reference, targetFramework, cancellationToken)
            .ContinueWith(r => r.Result.Select(f =>
                {
                    try
                    {
                        // load managed assembly only
                        _ = AssemblyName.GetAssemblyName(f);
                        return (MetadataReference)MetadataReference.CreateFromFile(f);
                    }
                    catch (System.Exception)
                    {
                        Debug.WriteLine($"Failed to load {f}");
                        return null;
                    }
                }).WhereNotNull(), TaskContinuationOptions.OnlyOnRanToCompletion);

    Task<IEnumerable<string>> ResolveAnalyzers(string reference, string targetFramework, CancellationToken cancellationToken = default)
        => Enumerable.Empty<string>().WrapTask();
    
    Task<IEnumerable<AnalyzerReference>> ResolveAnalyzerReferences(string reference, string targetFramework, CancellationToken cancellationToken = default)
        => ResolveAnalyzers(reference, targetFramework, cancellationToken)
            .ContinueWith(r=> r.Result.Select(_ => (AnalyzerReference)new AnalyzerFileReference(_, AnalyzerAssemblyLoader.Instance)), cancellationToken);
}
