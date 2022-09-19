// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

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
