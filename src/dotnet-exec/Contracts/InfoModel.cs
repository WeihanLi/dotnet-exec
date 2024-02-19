// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Exec.Contracts;

/// <summary>
/// Dump version info
/// https://github.com/dotnet/core/blob/main/samples/dotnet-runtimeinfo/Program.cs
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class InfoModel
{
    public string ToolVersion { get; init; }
        = ApplicationHelper.GetLibraryInfo(typeof(Helper)).LibraryVersion;

    public string CommonVersion { get; init; }
        = ApplicationHelper.GetLibraryInfo(typeof(Guard)).LibraryVersion;

    public string NuGetVersion { get; init; }
        = ApplicationHelper.GetLibraryInfo(typeof(NuGet.Common.NullLogger)).LibraryVersion;

    public string RoslynVersion { get; init; }
        = ApplicationHelper.GetLibraryInfo(typeof(Microsoft.CodeAnalysis.CSharp.CSharpCompilation)).LibraryVersion;

    public string EnvironmentVersion { get; init; } = Environment.Version.ToString();
    public string FrameworkDescription { get; init; } = RuntimeInformation.FrameworkDescription;
    public string RuntimeIdentifier { get; init; } = RuntimeInformation.RuntimeIdentifier;
    public int ProcessorCount { get; set; } = Environment.ProcessorCount;
    public string OSArchitecture { get; set; } = RuntimeInformation.OSArchitecture.ToString();
    public string OSDescription { get; set; }= RuntimeInformation.OSDescription;
    public string OSVersion { get; set; } = Environment.OSVersion.VersionString;
}
