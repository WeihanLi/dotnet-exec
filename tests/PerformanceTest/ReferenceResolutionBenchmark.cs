// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using BenchmarkDotNet.Jobs;
using Microsoft.CodeAnalysis;

namespace PerformanceTest;

/// <summary>
/// Benchmarks reference resolution performance.
/// Each benchmark pair shows "cold" (no cache, baseline) vs "warm" (Lazy&lt;Task&lt;T&gt;&gt; cache hit)
/// to demonstrate the speedup delivered by the caching optimisation.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class ReferenceResolutionBenchmark
{
    // Shared resolver with Lazy<Task<T>> caching ("after" optimisation state)
    private IRefResolver _cachedResolver = null!;
    // Per-call resolver with DisableCache=true ("before" baseline – full pipeline every time)
    private RefResolver _uncachedResolver = null!;
    private ExecOptions _optionsNoRefs = null!;
    private ExecOptions _optionsWithNuGet = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _optionsNoRefs = new ExecOptions { IncludeWideReferences = false };
        _optionsWithNuGet = new ExecOptions
        {
            IncludeWideReferences = false,
            References = new HashSet<string>(StringComparer.Ordinal) { "nuget:WeihanLi.Common" }
        };

        // Uncached resolver simulates the pre-optimisation path (full pipeline on every call)
        _uncachedResolver = (RefResolver)RefResolver.InstanceForTest;
        _uncachedResolver.DisableCache = true;

        // Cached resolver: pre-warm so benchmark iterations measure pure cache-hit cost
        _cachedResolver = RefResolver.InstanceForTest;
        await _cachedResolver.ResolveReferences(_optionsNoRefs, compilation: true);
        await _cachedResolver.ResolveReferences(_optionsNoRefs, compilation: false);
        await _cachedResolver.ResolveReferences(_optionsWithNuGet, compilation: true);
        await _cachedResolver.ResolveReferences(_optionsWithNuGet, compilation: false);
        await _cachedResolver.ResolveMetadataReferences(_optionsNoRefs, compilation: true);
    }

    // ── Framework-only resolution ────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "Framework refs (compile) — cold [baseline]")]
    public Task<string[]> FrameworkRefs_Compile_Cold()
        => _uncachedResolver.ResolveReferences(_optionsNoRefs, compilation: true);

    [Benchmark(Description = "Framework refs (compile) — warm [cached]")]
    public Task<string[]> FrameworkRefs_Compile_Warm()
        => _cachedResolver.ResolveReferences(_optionsNoRefs, compilation: true);

    [Benchmark(Description = "Framework refs (runtime) — cold")]
    public Task<string[]> FrameworkRefs_Runtime_Cold()
        => _uncachedResolver.ResolveReferences(_optionsNoRefs, compilation: false);

    [Benchmark(Description = "Framework refs (runtime) — warm [cached]")]
    public Task<string[]> FrameworkRefs_Runtime_Warm()
        => _cachedResolver.ResolveReferences(_optionsNoRefs, compilation: false);

    // ── NuGet + framework resolution ─────────────────────────────────────────────────

    [Benchmark(Description = "NuGet + framework refs (compile) — cold")]
    public Task<string[]> NuGetAndFrameworkRefs_Compile_Cold()
        => _uncachedResolver.ResolveReferences(_optionsWithNuGet, compilation: true);

    [Benchmark(Description = "NuGet + framework refs (compile) — warm [cached]")]
    public Task<string[]> NuGetAndFrameworkRefs_Compile_Warm()
        => _cachedResolver.ResolveReferences(_optionsWithNuGet, compilation: true);

    [Benchmark(Description = "NuGet + framework refs (runtime) — cold")]
    public Task<string[]> NuGetAndFrameworkRefs_Runtime_Cold()
        => _uncachedResolver.ResolveReferences(_optionsWithNuGet, compilation: false);

    [Benchmark(Description = "NuGet + framework refs (runtime) — warm [cached]")]
    public Task<string[]> NuGetAndFrameworkRefs_Runtime_Warm()
        => _cachedResolver.ResolveReferences(_optionsWithNuGet, compilation: false);

    // ── Metadata reference creation ──────────────────────────────────────────────────

    [Benchmark(Description = "Metadata refs (compile) — cold")]
    public Task<MetadataReference[]> MetadataRefs_Compile_Cold()
        => _uncachedResolver.ResolveMetadataReferences(_optionsNoRefs, compilation: true);

    [Benchmark(Description = "Metadata refs (compile) — warm [cached]")]
    public Task<MetadataReference[]> MetadataRefs_Compile_Warm()
        => _cachedResolver.ResolveMetadataReferences(_optionsNoRefs, compilation: true);
}
