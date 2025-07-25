// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

var target = CommandLineParser.Val(args, "target", "Default");
var apiKey = CommandLineParser.Val(args, "apiKey");
var stable = CommandLineParser.BooleanVal(args, "stable");
var noPush = CommandLineParser.BooleanVal(args, "noPush");
var runningOnGithubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

Console.WriteLine($$"""
Arguments:

target: {{target}}
stable: {{stable}}
noPush: {{noPush}}
args:   
{{args.StringJoin("\n")}}

""");

var solutionPath = "./dotnet-exec.slnx";
string[] srcProjects = [
    "./src/dotnet-exec/dotnet-exec.csproj",
    "./src/ReferenceResolver/ReferenceResolver.csproj"
];
string[] testProjects = [
    "./tests/UnitTest/UnitTest.csproj", 
    "./tests/IntegrationTest/IntegrationTest.csproj"
];

await new BuildProcessBuilder()
    .WithSetup(() =>
    {
        // cleanup previous artifacts
        if (Directory.Exists("./artifacts/packages"))
            Directory.Delete("./artifacts/packages", true);
    })
    .WithTaskExecuting(task => Console.WriteLine($@"===== Task [{task.Name}] {task.Description} executing ======"))
    .WithTaskExecuted(task => Console.WriteLine($@"===== Task [{task.Name}] {task.Description} executed ======"))
    .WithTask("hello", b => b.WithExecution(() => Console.WriteLine("Hello dotnet-exec build")))
    .WithTask("build", b =>
    {
        b.WithDescription("dotnet build")
            .WithDependency("hello")
            .WithExecution(cancellationToken => ExecuteCommandAsync($"dotnet build {solutionPath}", cancellationToken))
            ;
    })
    .WithTask("test", b =>
    {
        b.WithDescription("dotnet test")
            .WithDependency("build")
            .WithExecution(async cancellationToken =>
            {
                var loggerOptions = runningOnGithubActions
                        ? "--logger \"GitHubActions;summary.includePassedTests=true;summary.includeSkippedTests=true\" "
                        : "";
                foreach (var project in testProjects)
                {
                    var command = $"dotnet test {loggerOptions} --collect:\"XPlat Code Coverage;Format=cobertura,opencover;ExcludeByAttribute=ExcludeFromCodeCoverage,Obsolete,GeneratedCode,CompilerGenerated\" {project}";
                    await ExecuteCommandAsync(command, cancellationToken);
                }
            })
            ;
    })
    .WithTask("pack", b => b.WithDescription("dotnet pack")
        .WithDependency("test")
        .WithExecution(async cancellationToken =>
        {
            if (stable)
            {
                foreach (var project in srcProjects)
                {
                    await ExecuteCommandAsync($"dotnet pack {project} -o ./artifacts/packages", cancellationToken);
                }
            }
            else
            {
                var suffix = $"preview-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
                foreach (var project in srcProjects)
                {
                    await ExecuteCommandAsync(
                        $"dotnet pack {project} -o ./artifacts/packages --version-suffix {suffix}", cancellationToken);
                }
            }

            if (noPush)
            {
                Console.WriteLine("Skip push there's noPush specified");
                return;
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                // try to get apiKey from environment variable
                apiKey = Environment.GetEnvironmentVariable("NuGet__ApiKey");

                if (string.IsNullOrEmpty(apiKey))
                {
                    Console.WriteLine("Skip push since there's no apiKey found");
                    return;
                }
            }

            // push nuget packages
            foreach (var file in Directory.GetFiles("./artifacts/packages/", "*.nupkg"))
            {
                await RetryHelper.TryInvokeAsync(() => ExecuteCommandAsync($"dotnet nuget push {file} -k {apiKey} --skip-duplicate", cancellationToken), cancellationToken: cancellationToken);
            }
        }))
    .WithTask("Default", b => b.WithDependency("hello").WithDependency("pack"))
    .Build()
    .ExecuteAsync(target, ApplicationHelper.ExitToken);

async Task ExecuteCommandAsync(string commandText, CancellationToken cancellationToken = default)
{
    Console.WriteLine($"Executing command: \n    {commandText}");
    Console.WriteLine();

    var result = await CommandExecutor.ExecuteCommandAndOutputAsync(commandText, cancellationToken: cancellationToken);
    result.EnsureSuccessExitCode();
    Console.WriteLine();
}
