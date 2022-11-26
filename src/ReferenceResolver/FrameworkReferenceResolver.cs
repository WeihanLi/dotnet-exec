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

    public static string GetDotnetPath()
    {
        var executableName =
            $"dotnet{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty)}";
        var searchPaths = Guard.NotNull(Environment.GetEnvironmentVariable("PATH"))
            .Split(new[] { Path.PathSeparator }, options: StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim('"'))
            .ToArray();
        var commandPath = searchPaths
            .Where(p => !Path.GetInvalidPathChars().Any(p.Contains))
            .Select(p => Path.Combine(p, executableName))
            .First(File.Exists);
        return commandPath;
    }

    private static string GetDotnetDirectory()
    {
        var environmentOverride = Environment.GetEnvironmentVariable("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR");
        if (!string.IsNullOrEmpty(environmentOverride))
        {
            return environmentOverride;
        }

        var dotnetExe = GetDotnetPath();

        if (dotnetExe.IsNotNullOrEmpty() && !Interop.RunningOnWindows)
        {
            // e.g. on Linux the 'dotnet' command from PATH is a symlink so we need to
            // resolve it to get the actual path to the binary
            dotnetExe = Interop.Unix.RealPath(dotnetExe) ?? dotnetExe;
        }

        if (string.IsNullOrWhiteSpace(dotnetExe))
        {
            dotnetExe = Environment.ProcessPath;
        }

        return Guard.NotNull(Path.GetDirectoryName(dotnetExe));
    }

    private static string _dotnetDirectory = string.Empty;

    public static string DotnetDirectory
    {
        get
        {
            if (!string.IsNullOrEmpty(_dotnetDirectory))
            {
                return _dotnetDirectory;
            }

            _dotnetDirectory = GetDotnetDirectory();
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

[System.Diagnostics.DebuggerDisplay("framework: {Reference}")]
public sealed record FrameworkReference(string FrameworkName) : IReference
{
    public string Reference => FrameworkName;
    public ReferenceType ReferenceType => ReferenceType.FrameworkReference;
}
