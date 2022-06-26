// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using System.Runtime.Loader;

namespace Exec;

// https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
public sealed class CustomLoadContext : AssemblyLoadContext
{
    private readonly Dictionary<string, string> _assemblyPaths;

    public CustomLoadContext(IEnumerable<string> assemblyPaths) : base(Helper.ApplicationName)
    {
        _assemblyPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var assemblyPath in assemblyPaths)
        {
            // Add the first entry with the same simple assembly name if there are multiples
            // and ignore others
            _assemblyPaths.TryAdd(Path.GetFileNameWithoutExtension(assemblyPath), assemblyPath);
        }
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name != null
            && _assemblyPaths.TryGetValue(assemblyName.Name, out var assemblyPath)
            )
        {
            return LoadFromAssemblyPath(assemblyPath);
        }
        return null;
    }
}
