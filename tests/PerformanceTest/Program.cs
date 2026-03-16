// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Reflection;

// Run all benchmarks when invoked from CLI, otherwise just run with Job.Dry for quick validation
var summaries = BenchmarkRunner.Run(Assembly.GetExecutingAssembly());
return summaries.Any(s => s.HasCriticalValidationErrors) ? 1 : 0;
