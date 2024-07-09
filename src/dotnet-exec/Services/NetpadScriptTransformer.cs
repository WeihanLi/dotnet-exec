// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using ReferenceResolver;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Exec.Services;

[ExcludeFromCodeCoverage]
internal sealed class NetpadScriptTransformer : IScriptTransformer
{
    public HashSet<string> SupportedExtensions { get; } =
    [
        ".netpad"
    ];

    public Task InvokeAsync(ExecOptions context, string[] scriptLines)
    {
        // https://github.com/tareqimbasher/NetPad/blob/b48529cab236d8607bb67ee211154239e72de757/src/Apps/NetPad.Apps.Common/Scripts/ScriptSerializer.cs#L49

        var codeIndex = Array.FindIndex(scriptLines, l => l.Trim() == "#Code");
        if (codeIndex < 0)
            throw new InvalidOperationException("The script is missing #Code identifier.");

        var scriptOptions = string.Join("", scriptLines[1..codeIndex]);
        var scriptData = JsonSerializer.Deserialize<ScriptData>(scriptOptions, JsonHelper.WebOptions);
        ArgumentNullException.ThrowIfNull(scriptData?.Config);

        context.IncludeWebReferences = scriptData.Config.UseAspNet;
        foreach (var ns in scriptData.Config.Namespaces ?? [])
        {
            context.Usings.Add(ns);
        }
        foreach (var reference in scriptData.Config.References ?? [])
        {
            context.References.Add(new NuGetReference(reference.PackageId, reference.Version).ReferenceWithSchema());
        }
        if (Enum.TryParse<OptimizationLevel>(scriptData.Config.OptimizationLevel, out var configuration))
        {
            context.Configuration = configuration;
        }
        if ("Expression".EqualsIgnoreCase(scriptData.Config.Kind))
        {
            context.CompilerType = context.ExecutorType = Helper.Script;
        }

        var code = string.Join("\n", scriptLines[(codeIndex + 1)..]);
        context.Script = code;

        return Task.CompletedTask;
    }
}

file sealed class ScriptData(ScriptConfigData config)
{
    public ScriptConfigData Config { get; } = config;
}

file sealed class ScriptConfigData
{
    public string? Kind { get; set; }
    public string? TargetFrameworkVersion { get; set; }
    public string? OptimizationLevel { get; set; }
    public bool UseAspNet { get; set; }
    public List<string>? Namespaces { get; set; }
    public List<PackageReference>? References { get; set; }
}

file sealed class PackageReference(string packageId, string version)
{
    public string PackageId { get; } = packageId;
    public string Version { get; } = version;
}
