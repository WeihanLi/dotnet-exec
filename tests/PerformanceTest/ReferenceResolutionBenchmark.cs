// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using BenchmarkDotNet.Jobs;
using Microsoft.CodeAnalysis;

namespace PerformanceTest;

/// <summary>
/// Benchmarks reference resolution performance.
/// Tests framework-only resolution and NuGet package resolution,
/// covering both the compile-time and runtime reference paths.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class ReferenceResolutionBenchmark
{
    private IRefResolver _refResolver = null!;
    private ExecOptions _optionsNoRefs = null!;
    private ExecOptions _optionsWithNuGet = null!;

    [GlobalSetup]
    public void Setup()
    {
        _refResolver = RefResolver.InstanceForTest;

        _optionsNoRefs = new ExecOptions
        {
            IncludeWideReferences = false
        };

        _optionsWithNuGet = new ExecOptions
        {
            IncludeWideReferences = false,
            References = new HashSet<string>(StringComparer.Ordinal) { "nuget:WeihanLi.Common" }
        };
    }

    [Benchmark(Description = "Framework refs (compile)")]
    public Task<string[]> FrameworkReferences_Compile()
        => _refResolver.ResolveReferences(_optionsNoRefs, compilation: true);

    [Benchmark(Description = "Framework refs (runtime)")]
    public Task<string[]> FrameworkReferences_Runtime()
        => _refResolver.ResolveReferences(_optionsNoRefs, compilation: false);

    [Benchmark(Description = "NuGet + framework refs (compile)")]
    public Task<string[]> NuGetAndFrameworkReferences_Compile()
        => _refResolver.ResolveReferences(_optionsWithNuGet, compilation: true);

    [Benchmark(Description = "NuGet + framework refs (runtime)")]
    public Task<string[]> NuGetAndFrameworkReferences_Runtime()
        => _refResolver.ResolveReferences(_optionsWithNuGet, compilation: false);

    [Benchmark(Description = "Metadata refs (compile)")]
    public Task<MetadataReference[]> MetadataReferences_Compile()
        => _refResolver.ResolveMetadataReferences(_optionsNoRefs, compilation: true);
}
