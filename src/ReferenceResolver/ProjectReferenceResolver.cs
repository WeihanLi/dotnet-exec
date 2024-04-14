﻿// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

public sealed class ProjectReferenceResolver : IReferenceResolver
{
    public ReferenceType ReferenceType => ReferenceType.ProjectReference;

    public async Task<IEnumerable<string>> Resolve(string reference, string targetFramework,
        CancellationToken cancellationToken = default)
    {
        var dotnetPath = Guard.NotNull(ApplicationHelper.GetDotnetPath());
        var projectPath = GetProjectPath(reference, true);
        var outputDir = Path.Combine(Path.GetDirectoryName(projectPath)!, "bin/_exec_build/out");
        var result = await RetryHelper.TryInvokeAsync(async () =>
        {
            return await CommandExecutor.ExecuteAndCaptureAsync(dotnetPath, $"build {reference} -o {outputDir}", cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }, r => r?.ExitCode is 0, 5, cancellationToken: cancellationToken).ConfigureAwait(false);
        return result?.ExitCode != 0
            ? throw new InvalidOperationException(
                $"Failed to build project {reference}, {result?.StandardOut}, {(string.IsNullOrEmpty(result?.StandardError) ? "" : $"Error: {result.StandardError}")}")
            : (IEnumerable<string>)Directory.GetFiles(outputDir, "*.dll");
    }

    public static string GetProjectPath(string projectPath, bool ensureFullPath)
    {
        string path;
        if (File.Exists(projectPath))
        {
            path = projectPath;
        }
        else
        {
            if (Directory.Exists(projectPath))
            {
                var csprojFiles = Directory.GetFiles(projectPath, "*.csproj");
                if (csprojFiles.IsNullOrEmpty())
                {
                    throw new ArgumentException($"Project file not found in {projectPath}");
                }

                if (csprojFiles.Length > 1)
                {
                    throw new ArgumentException($"Multiple project file found, please specific a project file");
                }

                path = csprojFiles[0];
            }
            else
            {
                throw new ArgumentException($"Project file not found {projectPath}");
            }
        }

        return ensureFullPath ? Path.GetFullPath(path) : path;
    }
}
