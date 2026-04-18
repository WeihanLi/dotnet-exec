// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using BenchmarkDotNet.Jobs;
using WeihanLi.Common.Models;

namespace PerformanceTest;

/// <summary>
/// Benchmarks the compilation pipeline performance.
/// Compares the simple and workspace compilers on typical C# scripts.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class CompilationBenchmark
{
    private SimpleCodeCompiler _simpleCompiler = null!;
    private WorkspaceCodeCompiler _workspaceCompiler = null!;
    private ExecOptions _options = null!;

    private const string TopLevelCode = """
        Console.WriteLine("Hello, World!");
        """;

    private const string ClassCode = """
        public class Program
        {
            public static void MainTest()
            {
                Console.WriteLine("Hello, World!");
            }
        }
        """;

    [GlobalSetup]
    public void Setup()
    {
        var refResolver = RefResolver.InstanceForTest;
        var scriptFetcher = AdditionalScriptContentFetcher.InstanceForTest;
        var parseOptionsPipeline = new ParseOptionsPipeline([]);
        var compilationOptionsPipeline = new CompilationOptionsPipeline([]);
        _simpleCompiler = new SimpleCodeCompiler(refResolver, scriptFetcher, parseOptionsPipeline, compilationOptionsPipeline);
        _workspaceCompiler = new WorkspaceCodeCompiler(
            refResolver, scriptFetcher, parseOptionsPipeline, compilationOptionsPipeline);
        _options = new ExecOptions { IncludeWideReferences = false };
    }

    [Benchmark(Description = "Simple compiler - top-level")]
    public Task<Result<CompileResult>> SimpleCompiler_TopLevel()
        => _simpleCompiler.Compile(_options, TopLevelCode);

    [Benchmark(Description = "Simple compiler - class")]
    public Task<Result<CompileResult>> SimpleCompiler_Class()
        => _simpleCompiler.Compile(_options, ClassCode);

    [Benchmark(Description = "Workspace compiler - top-level")]
    public Task<Result<CompileResult>> WorkspaceCompiler_TopLevel()
        => _workspaceCompiler.Compile(_options, TopLevelCode);

    [Benchmark(Description = "Workspace compiler - class")]
    public Task<Result<CompileResult>> WorkspaceCompiler_Class()
        => _workspaceCompiler.Compile(_options, ClassCode);
}
