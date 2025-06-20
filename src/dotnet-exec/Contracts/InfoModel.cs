﻿// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics.CodeAnalysis;

namespace Exec.Contracts;

/// <summary>
/// Dump version info
/// https://github.com/dotnet/core/blob/main/samples/dotnet-runtimeinfo/Program.cs
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class InfoModel
{
    public string? DotnetRoot { get; init; } = ApplicationHelper.GetDotnetDirectory();
    public string DotnetPath { get; init; } = ApplicationHelper.GetDotnetPath() ?? string.Empty;

    public string ToolVersion { get; init; }
        = ApplicationHelper.GetLibraryInfo(typeof(InfoModel)).LibraryVersion;

    public string CommonVersion { get; init; }
        = ApplicationHelper.GetLibraryInfo(typeof(Guard)).LibraryVersion;

    public string NuGetVersion { get; init; }
        = ApplicationHelper.GetLibraryInfo(typeof(NuGet.Common.NullLogger)).LibraryVersion;

    public string RoslynVersion { get; init; }
        = ApplicationHelper.GetLibraryInfo(typeof(Microsoft.CodeAnalysis.CSharp.CSharpCompilation)).LibraryVersion;

    public RuntimeInfo RuntimeInfo { get; init; } = ApplicationHelper.RuntimeInfo;
}
