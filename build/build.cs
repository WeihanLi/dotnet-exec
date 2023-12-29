// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Newtonsoft.Json;

// dump runtime info
Console.WriteLine("RuntimeInfo:");
Console.WriteLine(ApplicationHelper.RuntimeInfo.ToJson(new JsonSerializerSettings()
{
    Formatting = Formatting.Indented
}));

//
var target = Guard.NotNull(Argument("target", "Default"));
var stable = Argument("stable", false);
var apiKey = Argument( "apiKey", "");

var solutionPath = "./dotnet-exec.sln";
string[] srcProjects = ["./src/dotnet-exec/dotnet-exec.csproj", "./src/ReferenceResolver/ReferenceResolver.csproj"];
string[] testProjects = [ "./tests/UnitTest/UnitTest.csproj", "./tests/IntegrationTest/IntegrationTest.csproj" ];

await BuildProcess.CreateBuilder()
    .WithTask("hello", b => {})
    .WithTask("build", b =>
    {
        b.WithDescription("dotnet build")
            .WithExecution(() => CommandExecutor.ExecuteAndCapture("dotnet", $"build {solutionPath}").EnsureSuccessExitCode())
            ;
    })
    .WithTask("test", b =>
    {
        b.WithDescription("dotnet test")
            .WithDependency("build")
            .WithExecution(async () =>
            {
                foreach (var project in testProjects)
                {
                    await CommandExecutor.ExecuteCommandAsync($"dotnet test {project}");
                }
            })
            ;
    })
    .WithTask("pack", b => b.WithDescription("dotnet pack")
        .WithDependency("test")
        .WithExecution(async () =>
        {
            foreach (var project in srcProjects)
            {
                if (stable)
                {
                    await CommandExecutor.ExecuteCommandAsync($"dotnet pack {project} -o ./artifacts/packages");
                }
                else
                {
                    var suffix = $"preview-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
                    await CommandExecutor.ExecuteCommandAsync($"dotnet pack {project} -o ./artifacts/packages --version-suffix {suffix}");   
                }
            }
            //
            if (!string.IsNullOrEmpty(apiKey))
            {
                foreach (var file in Directory.GetFiles("./artifacts/packages/", "*.nupkg"))
                {
                    await CommandExecutor.ExecuteCommandAsync($"dotnet nuget push {file} -k {apiKey}");
                }
            }
        }))
    .WithTask("Default", b => b.WithDependency("test"))
    .Build()
    .ExecuteAsync(target);


T? Argument<T>(string argumentName, T? defaultValue = default)
{
    for (var i = 0; i < args.Length-1; i++)
    {
        if (args[i] == $"--{argumentName}" || args[i] == $"-{argumentName}")
        {
            return args[i + 1].To<T>();
        }
    }

    return defaultValue;
}

file sealed class BuildProcess
{
    public IReadOnlyCollection<BuildTask> Tasks { get; init; } = [];

    public async Task ExecuteAsync(string target)
    {
        var task = Tasks.FirstOrDefault(x => x.Name == target);
        if (task is null) throw new InvalidOperationException("Invalid target to execute");

        await ExecuteTask(task);
    }

    private static async Task ExecuteTask(BuildTask task)
    {
        foreach (var dependencyTask in task.Dependencies)
        {
            await ExecuteTask(dependencyTask);
        }

        Console.WriteLine($"===== Task {task.Name} {task.Description} executing ======");
        await task.ExecuteAsync();
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

    public BuildProcessBuilder WithTask(string name, Action<BuildTaskBuilder> buildTaskConfigure)
    {
        var buildTaskBuilder = new BuildTaskBuilder(name);
        buildTaskBuilder.WithTaskFinder(s => _tasks.Find(t => t.Name == s) ?? throw new InvalidOperationException($"No task found with name {s}"));
        buildTaskConfigure.Invoke(buildTaskBuilder);
        var task = buildTaskBuilder.Build();
        _tasks.Add(task);
        return this;
    }
    
    internal BuildProcess Build()
    {
        return new BuildProcess()
        {
            Tasks = _tasks
        };
    }
}

file sealed class BuildTask(string name, string? description, Func<Task>? execution = null)
{
    public string Name => name;
    public string Description => description ?? name;

    public IReadOnlyCollection<BuildTask> Dependencies { get; init; } = [];

    public Task ExecuteAsync() => execution?.Invoke() ?? Task.CompletedTask;
}

file sealed class BuildTaskBuilder(string name)
{
    private readonly string _name = name;

    private string? _description;
    private Func<Task>? _execution;
    private readonly List<BuildTask> _dependencies = [];

    public BuildTaskBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }
    
    public BuildTaskBuilder WithExecution(Action execution)
    {
        _execution = execution.WrapTask();
        return this;
    }
    public BuildTaskBuilder WithExecution(Func<Task> execution)
    {
        _execution = execution;
        return this;
    }

    public BuildTaskBuilder WithDependency(BuildTask dependencyTask)
    {
        _dependencies.Add(dependencyTask);
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
        var buildTask = new BuildTask(_name, _description, _execution)
        {
            Dependencies = _dependencies
        };
        return buildTask;
    }
}
