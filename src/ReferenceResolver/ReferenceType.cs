// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

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
