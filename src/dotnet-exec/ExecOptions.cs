// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Exec;

public sealed partial class ExecOptions
{
    public string Script { get; set; } = "Program.cs";

    public string TargetFramework { get; set; } = DefaultTargetFramework;

    public string? StartupType { get; set; } = string.Empty;
    public string EntryPoint { get; set; } = "MainTest";

    public string[] Arguments { get; set; } = Array.Empty<string>();

    public string ProjectPath { get; set; } = string.Empty;

    public bool IncludeWideReferences { get; set; } = true;

    public bool IncludeWebReferences { get; set; }

    public HashSet<string> References { get; set; } = new(StringComparer.Ordinal);
    public HashSet<string> Usings { get; set; } = new(StringComparer.Ordinal);

    public HashSet<string>? AdditionalScripts { get; set; }

    public bool EnablePreviewFeatures { get; set; }
    public OptimizationLevel Configuration { get; set; }

    public string CompilerType { get; set; } = "workspace";

    public string ExecutorType { get; set; } = "default";

    public string? ConfigProfile { get; set; }

    public bool DebugEnabled { get; set; }

    public bool DisableCache { get; set; }

    public bool UseRefAssembliesForCompile { get; set; }

    public bool EnableSourceGeneratorSupport { get; set; }

    public HashSet<string>? ParserPreprocessorSymbolNames { get; set; }

    public KeyValuePair<string, string>[]? ParserFeatures { get; set; }

    [JsonIgnore] public CancellationToken CancellationToken { get; set; }
}
