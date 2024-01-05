// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace ReferenceResolver;

[System.Diagnostics.DebuggerDisplay("project: {Reference}")]
public sealed record ProjectReference : IReference
{
    public ProjectReference(string projectPath)
    {
        ProjectPath = ProjectReferenceResolver.GetProjectPath(projectPath, false);
    }

    public string ProjectPath { get; }
    public string Reference => Path.GetFullPath(ProjectPath);
    public ReferenceType ReferenceType => ReferenceType.ProjectReference;
}
