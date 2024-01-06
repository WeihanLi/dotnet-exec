// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Runtime.InteropServices;

namespace ReferenceResolver;

public sealed class FrameworkReferenceResolver : IReferenceResolver
{
    public static class FrameworkNames
    {
        public const string Default = "Microsoft.NETCore.App";
        public const string Web = "Microsoft.AspNetCore.App";
        public const string WindowsDesktop = "Microsoft.WindowsDesktop.App";
    }

    private static readonly Dictionary<string, string> FrameworkAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { nameof(FrameworkNames.Default), FrameworkNames.Default },
        { nameof(FrameworkNames.Web), FrameworkNames.Web },
        { "aspnet", FrameworkNames.Web },
        { "aspnetcore", FrameworkNames.Web },
        { nameof(FrameworkNames.WindowsDesktop), FrameworkNames.WindowsDesktop },
    };

    public ReferenceType ReferenceType => ReferenceType.FrameworkReference;

    public Task<IEnumerable<string>> Resolve(string reference, string targetFramework,
        CancellationToken cancellationToken = default)
    {
        var references = ResolveFrameworkReferencesViaRuntimeShared(reference, targetFramework);
        return Task.FromResult<IEnumerable<string>>(references);
    }

    public Task<IEnumerable<string>> ResolveAnalyzers(string reference, string targetFramework,
        CancellationToken cancellationToken = default)
    {
        return ResolveFrameworkAnalyzerReferencesViaSdkPacks(reference, targetFramework)
            .WrapTask<IEnumerable<string>>();
    }

    public static Task<IEnumerable<string>> ResolveForCompile(string reference, string targetFramework,
        CancellationToken cancellationToken = default)
    {
        var references = ResolveFrameworkReferencesViaSdkPacks(reference, targetFramework);
        return Task.FromResult<IEnumerable<string>>(references);
    }

    public static Task<IEnumerable<string>> ResolveDefaultReferences(string targetFramework, bool forCompile = false,
        CancellationToken cancellationToken = default)
    {
        if (forCompile)
            return Task.FromResult<IEnumerable<string>>(
                ResolveFrameworkReferencesViaSdkPacks(FrameworkNames.Default, targetFramework));
        return Task.FromResult<IEnumerable<string>>(
            ResolveFrameworkReferencesViaRuntimeShared(FrameworkNames.Default, targetFramework));
    }

    private static volatile string _dotnetDirectory = string.Empty;

    public static string DotnetDirectory
    {
        get
        {
            if (!string.IsNullOrEmpty(_dotnetDirectory))
            {
                return _dotnetDirectory;
            }
            _dotnetDirectory = ApplicationHelper.GetDotnetDirectory();
            return _dotnetDirectory;
        }
    }

    public static string GetReferencePackageName(string frameworkName)
    {
        FrameworkAliases.TryGetValue(frameworkName, out var framework);
        framework ??= frameworkName;
        return $"{framework}.Ref";
    }

    public static string GetRuntimePackageName(string frameworkName)
    {
        FrameworkAliases.TryGetValue(frameworkName, out var framework);
        framework ??= frameworkName;
        var platform = OperatingSystem.IsWindows() ? "win"
            : OperatingSystem.IsLinux() ? "linux"
            : OperatingSystem.IsMacOS() ? "osx"
            : null;
        if (platform is null)
        {
            throw new ArgumentException("Unknown OS-platform");
        }
        var fullPackageName = $"{framework}.Runtime.{platform}-{RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()}";
        return fullPackageName;
    }

    public static string[] GetImplicitUsings(string frameworkName)
    {
        if (string.IsNullOrEmpty(frameworkName)) return [];

        FrameworkAliases.TryGetValue(frameworkName, out var framework);
        framework ??= frameworkName;
        // https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview#implicit-using-directives
        return framework switch
        {
            FrameworkNames.Default =>
            [
                "global::System",
                "global::System.Collections.Generic",
                "global::System.IO",
                "global::System.Linq",
                "global::System.Net.Http",
                "global::System.Text",
                "global::System.Threading",
                "global::System.Threading.Tasks"
            ],
            FrameworkNames.Web =>
            [
                "global::System.Net.Http.Json",
                "global::Microsoft.AspNetCore.Builder",
                "global::Microsoft.AspNetCore.Hosting",
                "global::Microsoft.AspNetCore.Http",
                "global::Microsoft.AspNetCore.Routing",
                "global::Microsoft.Extensions.Configuration",
                "global::Microsoft.Extensions.DependencyInjection",
                "global::Microsoft.Extensions.Hosting",
                "global::Microsoft.Extensions.Logging"
            ],
            FrameworkNames.WindowsDesktop =>
            [
                "global::System.Drawing",
                "global::System.Windows.Forms"
            ],
            _ => []
        };
    }

    private static string[] ResolveFrameworkReferencesViaSdkPacks(string frameworkName, string targetFramework)
    {
        return GetFrameworkReferenceDllViaSdkPacks(frameworkName, targetFramework, "ref", targetFramework);
    }

    private static string[] ResolveFrameworkAnalyzerReferencesViaSdkPacks(string frameworkName, string targetFramework)
    {
        return GetFrameworkReferenceDllViaSdkPacks(frameworkName, targetFramework, "analyzers");
    }

    private static string[] GetFrameworkReferenceDllViaSdkPacks(string frameworkName, string targetFramework,
        params string[] folder)
    {
        if (frameworkName.IsNullOrEmpty())
            frameworkName = FrameworkNames.Default;
        
        if (FrameworkAliases.TryGetValue(frameworkName, out var fName)) 
            frameworkName = fName;
        
        var packsDir = Path.Combine(DotnetDirectory, "packs");
        var referencePackageName = $"{frameworkName}.Ref";
        var frameworkDir = Path.Combine(packsDir, referencePackageName);
        if (!Directory.Exists(frameworkDir)) return [];
        
        IEnumerable<string> versions = Directory.GetDirectories(frameworkDir);
        var versionPrefix = targetFramework["net".Length..];
        versions = versions.Where(x => Path.GetFileName(x).GetNotEmptyValueOrDefault(x)
            .StartsWith(versionPrefix, StringComparison.OrdinalIgnoreCase));
        var targetVersionDir = versions.OrderByDescending(x => x).First();
        var targetDir = Path.Combine([targetVersionDir, ..folder]);
        if (!Directory.Exists(targetDir)) return [];
        
        var dllFiles = Directory.GetFiles(targetDir, "*.dll", SearchOption.AllDirectories);
        return dllFiles;
    }

    private static string[] ResolveFrameworkReferencesViaRuntimeShared(string frameworkName, string targetFramework)
    {
        if (frameworkName.IsNullOrEmpty())
            frameworkName = FrameworkNames.Default;
        
        if (FrameworkAliases.TryGetValue(frameworkName, out var fName)) 
            frameworkName = fName;
        
        var sharedDir = Path.Combine(DotnetDirectory, "shared");
        var frameworkDir = Path.Combine(sharedDir, frameworkName);
        if (!Directory.Exists(frameworkDir))
            return [];

        Guard.NotNull(frameworkDir);
        IEnumerable<string> versions = Directory.GetDirectories(frameworkDir);
        var versionPrefix = targetFramework["net".Length..];
        // filter versions to match target framework like 8.0
        versions = versions.Where(x => Path.GetFileName(x).GetNotEmptyValueOrDefault(x)
            .StartsWith(versionPrefix, StringComparison.OrdinalIgnoreCase));
        var targetVersionDir = versions.OrderByDescending(x => x).First();
        return Directory.GetFiles(targetVersionDir, "*.dll");
    }
}
