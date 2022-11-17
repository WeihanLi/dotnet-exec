﻿// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using NuGet.Versioning;

namespace ReferenceResolver;

[System.Diagnostics.DebuggerDisplay("nuget: {Reference}")]
public sealed record NuGetReference(string PackageId, NuGetVersion? PackageVersion) : IReference
{
    public NuGetReference(string packageId, string packageVersion)
        : this(packageId, string.IsNullOrEmpty(packageVersion) ? null : NuGetVersion.Parse(packageVersion))
    {
    }

    public string Reference => PackageVersion is null
        ? $"{PackageId}"
        : $"{PackageId}, {PackageVersion}";

    public ReferenceType ReferenceType => ReferenceType.NuGetPackage;

    public void Deconstruct(out string packageId, out string packageVersion, out bool hasVersion)
    {
        packageId = PackageId;
        packageVersion = PackageVersion?.ToString() ?? string.Empty;
        hasVersion = PackageVersion != null;
    }
}