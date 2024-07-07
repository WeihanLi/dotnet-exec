// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

public interface IReference
{
    string Reference { get; }

    ReferenceType ReferenceType { get; }
}

public static class ReferenceExtensions
{
    public static string ReferenceWithSchema(this IReference reference)
    {
        return $"{ReferenceResolverFactory.ReferenceTypeSchemaCache.Value[reference.ReferenceType]}: {reference.Reference}";
    }
}
