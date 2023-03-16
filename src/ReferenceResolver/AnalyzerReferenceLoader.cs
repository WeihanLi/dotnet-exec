// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

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
        return Assembly.LoadFrom(fullPath);
    }

    public void AddDependencyLocation(string fullPath)
    {
    }
}
