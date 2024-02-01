// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

var target = Guard.NotNull(CommandLineParser.GetArgumentValue(args, "target", "Default"));
var apiKey = CommandLineParser.GetArgumentValue(args, "apiKey");
var stable = CommandLineParser.GetBoolArgumentValue(args, "stable");
var noPush = CommandLineParser.GetBoolArgumentValue(args, "noPush");

Console.WriteLine($$"""
Arguments:

target: {{target}}
stable: {{stable}}
noPush: {{noPush}}
args:   
{{args.StringJoin("\n")}}

""");

var solutionPath = "./dotnet-exec.sln";
string[] srcProjects = ["./src/dotnet-exec/dotnet-exec.csproj", "./src/ReferenceResolver/ReferenceResolver.csproj"];
string[] testProjects = ["./tests/UnitTest/UnitTest.csproj", "./tests/IntegrationTest/IntegrationTest.csproj"];

await BuildProcess.CreateBuilder()
    .WithSetup(() =>
    {
        // cleanup artifacts
        if (Directory.Exists("./artifacts/packages"))
            Directory.Delete("./artifacts/packages", true);

        // dump runtime info
        Console.WriteLine("RuntimeInfo:");
#pragma warning disable CA1869
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(ApplicationHelper.RuntimeInfo, new System.Text.Json.JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        }));
#pragma warning restore CA1869
    })
    .WithTask("hello", b => b.WithExecution(() => Console.WriteLine("Hello dotnet-exec build")))
    .WithTask("build", b =>
    {
        b.WithDescription("dotnet build")
            .WithExecution(cancellationToken => ExecuteCommandAsync($"dotnet build {solutionPath}", cancellationToken))
            ;
    })
    .WithTask("test", b =>
    {
        b.WithDescription("dotnet test")
            .WithDependency("build")
            .WithExecution(async cancellationToken =>
            {
                foreach (var project in testProjects)
                {
                    await ExecuteCommandAsync($"dotnet test --collect:\"XPlat Code Coverage;Format=cobertura,opencover;ExcludeByAttribute=ExcludeFromCodeCoverage,Obsolete,GeneratedCode,CompilerGenerated\" {project}", cancellationToken);
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
                await ExecuteCommandAsync($"dotnet nuget push {file} -k {apiKey} --skip-duplicate", cancellationToken);
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

file sealed class BuildProcess
{
    public IReadOnlyCollection<BuildTask> Tasks { get; init; } = [];
    public Func<Task>? Setup { private get; init; }
    public Func<Task>? Cleanup { private get; init; }

    public async Task ExecuteAsync(string target, CancellationToken cancellationToken = default)
    {
        var task = Tasks.FirstOrDefault(x => x.Name == target);
        if (task is null)
            throw new InvalidOperationException("Invalid target to execute");

        try
        {
            if (Setup != null)
                await Setup.Invoke();

            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteTask(task, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Operation canceled");
        }
        finally
        {
            if (Cleanup != null)
                await Cleanup.Invoke();
        }
    }

    private static async Task ExecuteTask(BuildTask task, CancellationToken cancellationToken)
    {
        Console.WriteLine($"===== Task {task.Name} {task.Description} executing ======");
        // execute dependency tasks
        foreach (var dependencyTask in task.Dependencies)
        {
            await ExecuteTask(dependencyTask, cancellationToken);
        }
        // execute task
        await task.ExecuteAsync(cancellationToken);
        Console.WriteLine($"===== Task {task.Name} {task.Description} executed ======");
    }

    public static BuildProcessBuilder CreateBuilder()
    {
        return new BuildProcessBuilder();
    }
}

file sealed class BuildProcessBuilder
{
    private readonly List<BuildTask> _tasks = [];
    private Func<Task>? _setup, _cleanup;

    public BuildProcessBuilder WithTask(string name, Action<BuildTaskBuilder> buildTaskConfigure)
    {
        var buildTaskBuilder = new BuildTaskBuilder(name);
        buildTaskBuilder.WithTaskFinder(s =>
            _tasks.Find(t => t.Name == s) ?? throw new InvalidOperationException($"No task found with name {s}"));
        buildTaskConfigure.Invoke(buildTaskBuilder);
        var task = buildTaskBuilder.Build();
        _tasks.Add(task);
        return this;
    }

    public BuildProcessBuilder WithSetup(Action setupFunc)
    {
        _setup = setupFunc.WrapTask();
        return this;
    }

    public BuildProcessBuilder WithSetup(Func<Task> setupFunc)
    {
        _setup = setupFunc;
        return this;
    }

    public BuildProcessBuilder WithCleanup(Action cleanupFunc)
    {
        _cleanup = cleanupFunc.WrapTask();
        return this;
    }

    public BuildProcessBuilder WithCleanup(Func<Task> cleanupFunc)
    {
        _cleanup = cleanupFunc;
        return this;
    }

    internal BuildProcess Build()
    {
        return new BuildProcess() { Tasks = _tasks, Setup = _setup, Cleanup = _cleanup };
    }
}

file sealed class BuildTask(string name, string? description, Func<CancellationToken, Task>? execution = null)
{
    public string Name => name;
    public string Description => description ?? name;

    public IReadOnlyCollection<BuildTask> Dependencies { get; init; } = [];

    public Task ExecuteAsync(CancellationToken cancellationToken = default) =>
        execution?.Invoke(cancellationToken) ?? Task.CompletedTask;
}

file sealed class BuildTaskBuilder(string name)
{
    private readonly string _name = name;

    private string? _description;
    private Func<CancellationToken, Task>? _execution;
    private readonly List<BuildTask> _dependencies = [];

    public BuildTaskBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public BuildTaskBuilder WithExecution(Action execution)
    {
        _execution = execution.WrapTask().WrapCancellation();
        return this;
    }

    public BuildTaskBuilder WithExecution(Func<Task> execution)
    {
        _execution = execution.WrapCancellation();
        return this;
    }

    public BuildTaskBuilder WithExecution(Func<CancellationToken, Task> execution)
    {
        _execution = execution;
        return this;
    }

    public BuildTaskBuilder WithDependency(string dependencyTaskName)
    {
        if (_taskFinder is null) throw new InvalidOperationException("Dependency task name is not supported");

        _dependencies.Add(_taskFinder.Invoke(dependencyTaskName));
        return this;
    }

    private Func<string, BuildTask>? _taskFinder;

    internal BuildTaskBuilder WithTaskFinder(Func<string, BuildTask> taskFinder)
    {
        _taskFinder = taskFinder;
        return this;
    }

    public BuildTask Build()
    {
        var buildTask = new BuildTask(_name, _description, _execution) { Dependencies = _dependencies };
        return buildTask;
    }
}
