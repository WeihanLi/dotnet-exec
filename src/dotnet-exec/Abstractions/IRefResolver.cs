// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Exec.Abstractions;

public interface IRefResolver
{
    // for unit test only
    bool DisableCache { get; set; }

    Task<string[]> ResolveReferences(ExecOptions options, bool compilation);

    Task<MetadataReference[]> ResolveMetadataReferences(ExecOptions options, bool compilation);

    Task<IEnumerable<string>> ResolveAnalyzers(ExecOptions options);
    Task<IEnumerable<AnalyzerReference>> ResolveAnalyzerReferences(ExecOptions options);
}
