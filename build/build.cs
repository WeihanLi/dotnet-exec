// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

// r: nuget:CliWrap

using CliWrap;
using Newtonsoft.Json;

//
var target = Guard.NotNull(Argument("target", "Default"));
var stable = Argument("stable", false);
var noPush = Argument("noPush", false);
var apiKey = Argument("apiKey", "");

var solutionPath = "./dotnet-exec.sln";
string[] srcProjects = ["./src/dotnet-exec/dotnet-exec.csproj", "./src/ReferenceResolver/ReferenceResolver.csproj"];
string[] testProjects = [ "./tests/UnitTest/UnitTest.csproj", "./tests/IntegrationTest/IntegrationTest.csproj" ];

await BuildProcess.CreateBuilder()
    .WithSetup(() =>
    {
        if (Directory.Exists("./artifacts/packages"))
            Directory.Delete("./artifacts/packages", true);

        // dump runtime info
        Console.WriteLine("RuntimeInfo:");
        Console.WriteLine(ApplicationHelper.RuntimeInfo.ToJson(new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented
        }));
    })
    .WithTask("hello", _ =>
    {
        Console.WriteLine("Hello dotnet-exec build");
    })
    .WithTask("build", b =>
    {
        b.WithDescription("dotnet build")
            .WithExecution(() => ExecuteCommandAsync($"dotnet build {solutionPath}"))
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
                    await ExecuteCommandAsync($"dotnet test {project}");
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
                    await ExecuteCommandAsync($"dotnet pack {project} -o ./artifacts/packages");
                }
                else
                {
                    var suffix = $"preview-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
                    await ExecuteCommandAsync($"dotnet pack {project} -o ./artifacts/packages --version-suffix {suffix}");   
                }
            }

            if (noPush)
            {
                Console.WriteLine("Skip push there's noPush");
                return;
            }
            if (!OperatingSystem.IsWindows())
            {
                Console.WriteLine("Skip push since we're not on Windows");
                return;
            }
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("NuGet__ApiKey");
            }
            if (!string.IsNullOrEmpty(apiKey))
            {
                // work around for local push
                // should be removed when push package using CI
                if (Environment.GetEnvironmentVariable("CI") is null) 
                    Environment.SetEnvironmentVariable("CI", "true");
                
                foreach (var file in Directory.GetFiles("./artifacts/packages/", "*.nupkg"))
                {
                    await ExecuteCommandAsync($"dotnet nuget push {file} -k {apiKey} --skip-duplicate");
                }
            }
        }))
    .WithTask("Default", b => b.WithDependency("pack"))
    .Build()
    .ExecuteAsync(target);


T? Argument<T>(string argumentName, T? defaultValue = default)
{
    for (var i = 0; i < args.Length-1; i++)
    {
        if (args[i] == $"--{argumentName}" || args[i] == $"-{argumentName}")
        {
            if (typeof(T) == typeof(bool) && args[i + 1].StartsWith('-'))
                return (T)(object)true;
            
            return args[i + 1].To<T>();
        }
    }

    return defaultValue;
}

async Task ExecuteCommandAsync(string commandText)
{
    Console.WriteLine($"Executing command: \n\t  {commandText}");
    Console.WriteLine();
    var splits = commandText.Split([' '], 2);
    var result = await Cli.Wrap(splits[0])
        .WithArguments(splits.Length > 1 ? splits[1] : string.Empty)
        .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
        .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
        .ExecuteAsync();
    Console.WriteLine();
    Console.WriteLine($"ExitCode: {result.ExitCode} ElapsedTime: {result.RunTime}");
}

file sealed class BuildProcess
{
    public IReadOnlyCollection<BuildTask> Tasks { get; init; } = [];
    public Func<Task>? Setup { private get; init; }
    public Func<Task>? CleanUp { private get; init; }

    public async Task ExecuteAsync(string target)
    {
        var task = Tasks.FirstOrDefault(x => x.Name == target);
        if (task is null)
            throw new InvalidOperationException("Invalid target to execute");
        
        try
        {
            if (Setup != null)
                await Setup.Invoke();
            
            await ExecuteTask(task);
        }
        finally
        {
            if (CleanUp != null)
                await CleanUp.Invoke();
        }                
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
    private Func<Task>? _setup, _cleanUp;

    public BuildProcessBuilder WithTask(string name, Action<BuildTaskBuilder> buildTaskConfigure)
    {
        var buildTaskBuilder = new BuildTaskBuilder(name);
        buildTaskBuilder.WithTaskFinder(s => _tasks.Find(t => t.Name == s) ?? throw new InvalidOperationException($"No task found with name {s}"));
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
    
    public BuildProcessBuilder WithCleanUp(Action cleanUpFunc)
    {
        _cleanUp = cleanUpFunc.WrapTask();
        return this;
    }

    public BuildProcessBuilder WithCleanUp(Func<Task> cleanUpFunc)
    {
        _cleanUp = cleanUpFunc;
        return this;
    }

    internal BuildProcess Build()
    {
        return new BuildProcess()
        {
            Tasks = _tasks,
            Setup = _setup,
            CleanUp = _cleanUp
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
