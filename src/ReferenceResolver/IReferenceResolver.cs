// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace ReferenceResolver;

public interface IReferenceResolver
{
    ReferenceType ReferenceType { get; }
    Task<IEnumerable<string>> Resolve(string reference, string targetFramework);
    Task<IEnumerable<MetadataReference>> ResolveMetadata(string reference, string targetFramework)
        => Resolve(reference, targetFramework)
            .ContinueWith(r => r.Result.Select(f => (MetadataReference)MetadataReference.CreateFromFile(f)), TaskContinuationOptions.OnlyOnRanToCompletion);
}
