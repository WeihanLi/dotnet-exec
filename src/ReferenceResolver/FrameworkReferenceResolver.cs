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

    public ReferenceType ReferenceType => ReferenceType.FrameworkReference;

    public Task<IEnumerable<string>> Resolve(string reference, string targetFramework)
    {
        if (reference.IsNullOrEmpty())
            reference = FrameworkNames.Default;
        var references = ResolveFrameworkReferencesViaSdkPacks(reference, targetFramework);
        return Task.FromResult<IEnumerable<string>>(references);
    }

    public Task<IEnumerable<string>> ResolveForRuntime(string reference, string targetFramework)
    {
        if (reference.IsNullOrEmpty())
            reference = FrameworkNames.Default;
        var references = ResolveFrameworkReferencesViaRuntimeShared(reference, targetFramework);
        return Task.FromResult<IEnumerable<string>>(references);
    }

    internal static string GetDotnetPath()
    {
        var commandNameWithExtension =
            $"dotnet{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty)}";
        var searchPaths = Guard.NotNull(Environment.GetEnvironmentVariable("PATH"))
            .Split(new[] { Path.PathSeparator }, options: StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim('"'))
            .ToArray();
        var commandPath = searchPaths
            .Where(p => !Path.GetInvalidPathChars().Any(p.Contains))
            .Select(p => Path.Combine(p, commandNameWithExtension))
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

    private static string DotnetDirectory
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

    private static string[] ResolveFrameworkReferencesViaSdkPacks(string frameworkName, string targetFramework)
    {
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
        var sharedDir = Path.Combine(DotnetDirectory, "shared");
        var frameworkDir = Path.Combine(sharedDir, frameworkName);
        if (!Directory.Exists(frameworkDir))
        {
            frameworkDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        }

        Guard.NotNull(frameworkDir);
        var versions = Directory.GetDirectories(frameworkDir).AsEnumerable();
        var versionPrefix = targetFramework["net".Length..];
        versions = versions.Where(x => Path.GetFileName(x).GetNotEmptyValueOrDefault(x).StartsWith(versionPrefix));
        var targetVersionDir = versions.OrderByDescending(x => x).First();
        return Directory.GetFiles(targetVersionDir, "*.dll");
    }
}
