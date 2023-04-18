// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
internal sealed class ReferenceSchemaAttribute : Attribute
{
    public string Schema { get; }

    public ReferenceSchemaAttribute(string schema)
    {
        Schema = schema;
    }
}
