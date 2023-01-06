// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

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
        if (reference.IsNullOrEmpty())
            reference = FrameworkNames.Default;
        var references = ResolveFrameworkReferencesViaRuntimeShared(reference, targetFramework);
        return Task.FromResult<IEnumerable<string>>(references);
    }

    public Task<IEnumerable<string>> ResolveForCompile(string reference, string targetFramework,
        CancellationToken cancellationToken = default)
    {
        if (reference.IsNullOrEmpty())
            reference = FrameworkNames.Default;
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
        FrameworkAliases.TryGetValue(frameworkName, out var framework);
        framework ??= frameworkName;
        // https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview#implicit-using-directives
        return framework switch
        {
            FrameworkNames.Default => new[]
            {
                "global::System", 
                "global::System.Collections.Generic", 
                "global::System.IO", 
                "global::System.Linq", 
                "global::System.Net.Http",
                "global::System.Text", 
                "global::System.Threading", 
                "global::System.Threading.Tasks"
            },
            FrameworkNames.Web => new[]
            {
                "global::System.Net.Http.Json", 
                "global::Microsoft.AspNetCore.Builder", 
                "global::Microsoft.AspNetCore.Hosting",
                "global::Microsoft.AspNetCore.Http", 
                "global::Microsoft.AspNetCore.Routing", 
                "global::Microsoft.Extensions.Configuration",
                "global::Microsoft.Extensions.DependencyInjection", 
                "global::Microsoft.Extensions.Hosting",
                "global::Microsoft.Extensions.Logging"
            },
            FrameworkNames.WindowsDesktop => new[]
            {
                "global::System.Drawing",
                "global::System.Windows.Forms"
            },
            _ => Array.Empty<string>()
        };
    }

    private static string[] ResolveFrameworkReferencesViaSdkPacks(string frameworkName, string targetFramework)
    {
        if (FrameworkAliases.TryGetValue(frameworkName, out var fName))
        {
            frameworkName = fName;
        }
        var packsDir = Path.Combine(DotnetDirectory, "packs");
        var referencePackageName = $"{frameworkName}.Ref";
        var frameworkDir = Path.Combine(packsDir, referencePackageName);
        if (Directory.Exists(frameworkDir))
        {
            var versions = Directory.GetDirectories(frameworkDir).AsEnumerable();
            var versionPrefix = targetFramework["net".Length..];
            versions = versions.Where(x => Path.GetFileName(x).GetNotEmptyValueOrDefault(x).StartsWith(versionPrefix));
            var targetVersionDir = versions.OrderByDescending(x => x).First();
            var targetReferenceDir = Path.Combine(targetVersionDir, "ref", targetFramework);
            return Directory.GetFiles(targetReferenceDir, "*.dll");
        }

        return Array.Empty<string>();
    }

    private static string[] ResolveFrameworkReferencesViaRuntimeShared(string frameworkName, string targetFramework)
    {
        if (FrameworkAliases.TryGetValue(frameworkName, out var fName))
        {
            frameworkName = fName;
        }
        var sharedDir = Path.Combine(DotnetDirectory, "shared");
        var frameworkDir = Path.Combine(sharedDir, frameworkName);
        if (!Directory.Exists(frameworkDir))
        {
            return Array.Empty<string>();
        }

        Guard.NotNull(frameworkDir);
        var versions = Directory.GetDirectories(frameworkDir).AsEnumerable();
        var versionPrefix = targetFramework["net".Length..];
        versions = versions.Where(x => Path.GetFileName(x).GetNotEmptyValueOrDefault(x).StartsWith(versionPrefix));
        var targetVersionDir = versions.OrderByDescending(x => x).First();
        return Directory.GetFiles(targetVersionDir, "*.dll");
    }
}
