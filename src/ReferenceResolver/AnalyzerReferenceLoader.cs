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
        return Assembly.Load(fullPath);
    }

    public void AddDependencyLocation(string fullPath)
    {
    }
}
