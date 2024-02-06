// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Runtime.InteropServices;

namespace Exec.Contracts;

/// <summary>
/// Dump version info
/// https://github.com/dotnet/core/blob/main/samples/dotnet-runtimeinfo/Program.cs
/// </summary>
public sealed class InfoModel
{
    public string ToolVersion { get; init; }
        = typeof(Helper).Assembly.ImageRuntimeVersion;

    public string CommonVersion { get; init; }
        = typeof(Guard).Assembly.ImageRuntimeVersion;

    public string NuGetVersion { get; init; }
        = typeof(NuGet.Common.NullLogger).Assembly.ImageRuntimeVersion;

    public string RoslynVersion { get; init; }
        = typeof(Microsoft.CodeAnalysis.CSharp.CSharpCompilation).Assembly.ImageRuntimeVersion;

    public string EnvironmentVersion { get; init; } = Environment.Version.ToString();
    public string FrameworkDescription { get; init; } = RuntimeInformation.FrameworkDescription;
    public int ProcessorCount { get; set; } = Environment.ProcessorCount;
    public string OSArchitecture { get; set; } = RuntimeInformation.OSArchitecture.ToString();
    public string OSDescription { get; set; }= RuntimeInformation.OSDescription;
    public string OSVersion { get; set; } = Environment.OSVersion.VersionString;
}
