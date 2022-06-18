// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

    public string[]? References { get; set; }
    public string[]? Usings { get; set; }

    public LanguageVersion LanguageVersion { get; set; }
    public OptimizationLevel Configuration { get; set; }

    public string CompilerType { get; set; } = "default";

    public string ExecutorType { get; set; } = "default";

    [JsonIgnore] public CancellationToken CancellationToken { get; set; }
}
