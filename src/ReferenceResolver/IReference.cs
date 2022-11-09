// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace ReferenceResolver;

public interface IReference
{
    string Reference { get; }

    string ReferenceWithSchema =>
        $"{ReferenceResolverFactory.ReferenceTypeSchemaCache.Value[ReferenceType]}: {Reference}";
    
    ReferenceType ReferenceType { get; }
}
