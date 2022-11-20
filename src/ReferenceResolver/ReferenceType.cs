// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace ReferenceResolver;

public enum ReferenceType
{
    [ReferenceSchema("file")]
    LocalFile = 0,
    [ReferenceSchema("folder")]
    LocalFolder = 1,
    [ReferenceSchema("nuget")]
    NuGetPackage = 2,
    [ReferenceSchema("framework")]
    FrameworkReference = 3,
    [ReferenceSchema("project")]
    ProjectReference = 4,
}
