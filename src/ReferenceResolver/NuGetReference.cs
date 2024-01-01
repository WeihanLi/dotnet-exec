// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using NuGet.Versioning;

namespace ReferenceResolver;

[System.Diagnostics.DebuggerDisplay("nuget: {Reference}")]
public sealed record NuGetReference(string PackageId, NuGetVersion? PackageVersion) : IReference
{
    public NuGetReference(string packageId, string? packageVersion)
        : this(packageId, string.IsNullOrEmpty(packageVersion) ? null : NuGetVersion.Parse(packageVersion))
    {
    }

    public string Reference => PackageVersion is null
        ? $"{PackageId}"
        : $"{PackageId}, {PackageVersion}";

    public ReferenceType ReferenceType => ReferenceType.NuGetPackage;

    public void Deconstruct(out string packageId, out string? packageVersion, out bool hasVersion)
    {
        packageId = PackageId;
        packageVersion = PackageVersion?.ToString();
        hasVersion = PackageVersion != null;
    }
    
    private static readonly char[] Separator = [',', ':'];
    public static NuGetReference Parse(string reference)
    {
        var splits = reference.Split(Separator, 2, 
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries);
        var packageId = splits[0];
        if (splits.Length != 2 || string.IsNullOrEmpty(splits[1])) 
            return new(packageId, (NuGetVersion?)null);
        
        var packageVersion = NuGetVersion.Parse(splits[1]);
        return new(packageId, packageVersion);
    }
}
