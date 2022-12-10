// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

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
