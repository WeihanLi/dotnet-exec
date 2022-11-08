// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using NuGet.Versioning;

namespace ReferenceResolver;

public sealed record NuGetReference(string PackageId, NuGetVersion? PackageVersion) : IReference
{
    public string Reference { get; } = PackageVersion is null
        ? $"nuget: {PackageId}"
        : $"nuget: {PackageId}, {PackageVersion}";

    public ReferenceType ReferenceType => ReferenceType.NuGetPackage;
}
