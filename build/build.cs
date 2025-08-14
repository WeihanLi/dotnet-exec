// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

string[] srcProjects = [
    "./src/dotnet-exec/dotnet-exec.csproj",
    "./src/ReferenceResolver/ReferenceResolver.csproj"
];
string[] testProjects = [
    "./tests/UnitTest/UnitTest.csproj", 
    "./tests/IntegrationTest/IntegrationTest.csproj"
];

await DotNetPackageBuildProcess
    .Create(options => 
    {
        options.SolutionPath = "./dotnet-exec.slnx";
        options.SrcProjects = srcProjects;
        options.TestProjects = testProjects;
        options.AllowLocalPush = true;
    })
    .ExecuteAsync(args);
