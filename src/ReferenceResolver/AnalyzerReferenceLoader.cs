// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using System.Reflection;

namespace ReferenceResolver;

public sealed class AnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
{
    public static readonly IAnalyzerAssemblyLoader Instance = new AnalyzerAssemblyLoader();

    private AnalyzerAssemblyLoader()
    {
    }

    public Assembly LoadFromPath(string fullPath)
    {
#pragma warning disable IL2026
        return Assembly.LoadFrom(fullPath);
#pragma warning restore IL2026
    }

    public void AddDependencyLocation(string fullPath)
    {
    }
}
