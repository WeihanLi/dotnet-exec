// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

public interface IReference
{
    string Reference { get; }

    string ReferenceWithSchema =>
        $"{ReferenceResolverFactory.ReferenceTypeSchemaCache.Value[ReferenceType]}: {Reference}";

    ReferenceType ReferenceType { get; }
}
