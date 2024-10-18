﻿// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace Exec;

// https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
public sealed class CustomLoadContext : AssemblyLoadContext, IAnalyzerAssemblyLoader, IDisposable
{
    public static readonly AsyncLocal<CustomLoadContext?> Current = new();

    private readonly Dictionary<string, string> _assemblyPaths;

    public CustomLoadContext(IEnumerable<string> assemblyPaths) : base(Helper.ApplicationName)
    {
        _assemblyPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        AddAssemblyPath(assemblyPaths.ToArray());
        Current.Value = this;
    }

    public void AddAssemblyPath(params string[] paths)
    {
        if (paths.IsNullOrEmpty()) return;
        foreach (var path in paths)
        {
            if (string.IsNullOrEmpty(path)) continue;
            _assemblyPaths.TryAdd(Path.GetFileNameWithoutExtension(path), path);
        }
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name != null
            && _assemblyPaths.TryGetValue(assemblyName.Name, out var assemblyPath)
            )
        {
            return LoadFromPath(assemblyPath);
        }
        return null;
    }

    public Assembly LoadFromPath(string fullPath)
    {
#pragma warning disable IL2026
        return LoadFromAssemblyPath(fullPath);
#pragma warning restore IL2026
    }

    public void AddDependencyLocation(string fullPath)
    {
        _assemblyPaths.TryAdd(Path.GetFileNameWithoutExtension(fullPath), fullPath);
    }

    public void Dispose()
    {
        _assemblyPaths.Clear();
        Current.Value = null;
    }
}
