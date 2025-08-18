# Getting Started

Welcome to dotnet-exec! This guide will help you get started with executing C# scripts and code without the need for a full project setup.

## What is dotnet-exec?

`dotnet-exec` is a command-line tool that allows you to execute C# programs without creating a project file. It supports:

- **Raw C# code execution**: Run code directly from the command line
- **Script file execution**: Execute .cs files locally or from URLs
- **Custom entry points**: Use methods other than `Main` as entry points
- **REPL mode**: Interactive C# execution environment
- **Rich reference support**: NuGet packages, local DLLs, framework references
- **Testing capabilities**: Built-in xUnit test execution
- **Configuration profiles**: Save and reuse common configurations
- **Command aliases**: Create shortcuts for frequently used commands

## Installation

### Install as .NET Tool

Install the latest stable version:

```sh
dotnet tool install -g dotnet-execute
```

Install the latest preview version:

```sh
dotnet tool install -g dotnet-execute --prerelease
```

Update to the latest version:

```sh
dotnet tool update -g dotnet-execute
```

### Installation Troubleshooting

If installation fails, try:

```sh
# Add NuGet source explicitly
dotnet tool install -g dotnet-execute --add-source https://api.nuget.org/v3/index.json

# Clear cache and retry
dotnet nuget locals all --clear
dotnet tool install -g dotnet-execute

# Ignore failed sources
dotnet tool install -g dotnet-execute --ignore-failed-sources
```

### Container Support

You can also use dotnet-exec with Docker/Podman without installing .NET SDK:

```sh
# Docker
docker run --rm weihanli/dotnet-exec:latest "1+1"
docker run --rm weihanli/dotnet-exec:latest "Guid.NewGuid()"

# Podman
podman run --rm weihanli/dotnet-exec:latest "DateTime.Now"
```

For the full image tag list, see <https://hub.docker.com/r/weihanli/dotnet-exec/tags>

## Quick Start Examples

### Execute Simple Expressions

```sh
# Basic arithmetic
dotnet-exec "1 + 1"

# Generate a GUID
dotnet-exec "Guid.NewGuid()"

# Get current time
dotnet-exec "DateTime.Now"

# String manipulation
dotnet-exec "\"Hello World\".ToUpper()"
```

### Execute C# Statements

```sh
# Print to console
dotnet-exec 'Console.WriteLine("Hello, dotnet-exec!");'

# Loop and calculations
dotnet-exec 'for(int i = 1; i <= 5; i++) Console.WriteLine($"Square of {i} is {i*i}");'

# Work with collections
dotnet-exec 'var numbers = new[] {1,2,3,4,5}; Console.WriteLine($"Sum: {numbers.Sum()}");'
```

### Execute Script Files

Create a file `hello.cs`:

```csharp
Console.WriteLine("Hello from script file!");
Console.WriteLine($"Current time: {DateTime.Now}");
```

Execute it:

```sh
dotnet-exec hello.cs
```

### Execute Remote Scripts

```sh
# Execute from GitHub
dotnet-exec https://raw.githubusercontent.com/user/repo/main/script.cs

# Execute from any URL
dotnet-exec https://example.com/scripts/utility.cs
```

## Script Types and Entry Points

### Default Entry Points

dotnet-exec looks for these entry methods in order:
1. Custom entry point specified with `--entry`
2. Default entry methods: `MainTest`, `Execute`, `Run`
3. Standard `Main` method

### Custom Entry Points

Create a script with custom entry point:

```csharp
// custom-entry.cs
public class MyScript
{
    public static void MainTest()
    {
        Console.WriteLine("Custom entry point executed!");
    }
    
    public static void Execute()
    {
        Console.WriteLine("Alternative entry point");
    }
}
```

Execute with specific entry point:

```sh
dotnet-exec custom-entry.cs --entry MainTest
```

### Multiple Entry Methods

Configure fallback entry methods with profile:

```sh
dotnet-exec profile set my-entries --default-entry CustomMain --default-entry Execute --default-entry Run

dotnet-exec script.cs --profile=my-entries
```

## REPL Mode

Start interactive mode by running dotnet-exec without arguments:

```sh
dotnet-exec
```

In REPL mode, you can:

```csharp
// Execute expressions
> 1 + 1
2

// Reference NuGet packages
> #r "nuget:Newtonsoft.Json"
> using Newtonsoft.Json;

// Use code completion
> Console.? // Press ? for IntelliSense

// Multi-line expressions
> var data = new {
    Name = "Test",
    Value = 42
  };
> data
{ Name = Test, Value = 42 }
```

### REPL with Custom Configuration

```sh
# Start REPL with web references
dotnet-exec --web

# Start with custom profile
dotnet-exec --profile myprofile

# Start with additional references
dotnet-exec -r 'nuget:Serilog' -u 'Serilog'
```

## Basic References and Using Statements

### NuGet Package References

```sh
# Latest stable version
dotnet-exec 'JsonConvert.SerializeObject(new {name="test"})' \
  -r 'nuget:Newtonsoft.Json' \
  -u 'Newtonsoft.Json'

# Specific version
dotnet-exec 'JsonConvert.SerializeObject(new {name="test"})' \
  -r 'nuget:Newtonsoft.Json,13.0.3' \
  -u 'Newtonsoft.Json'
```

### Local File References

```sh
# Reference local DLL
dotnet-exec MyScript.cs -r './libs/MyLibrary.dll'

# Reference all DLLs in folder
dotnet-exec MyScript.cs -r 'folder:./libs'
```

### Framework References

```sh
# Web framework (ASP.NET Core)
dotnet-exec 'WebApplication.Create().Run();' --web

# Explicit framework reference
dotnet-exec 'WebApplication.Create().Run();' -r 'framework:web'
```

### Using Statements

```sh
# Static using
dotnet-exec 'WriteLine("Hello World");' -u 'static System.Console'

# Using alias
dotnet-exec 'Json.SerializeObject(data)' \
  -u 'Json = Newtonsoft.Json.JsonConvert' \
  -r 'nuget:Newtonsoft.Json'

# Remove default using
dotnet-exec 'System.Console.WriteLine("Hello");' -u '-System'
```

## Commands Overview

### Default Command

The default command executes C# scripts and code:

```sh
# Execute script file
dotnet-exec script.cs

# Execute raw code
dotnet-exec 'Console.WriteLine("Hello");'

# Start REPL
dotnet-exec
```

### Profile Command

Manage configuration profiles:

```sh
# List profiles
dotnet-exec profile ls

# Create profile
dotnet-exec profile set myprofile -r 'nuget:Serilog' -u 'Serilog'

# Use profile
dotnet-exec script.cs --profile myprofile

# Get profile details
dotnet-exec profile get myprofile

# Remove profile
dotnet-exec profile rm myprofile
```

### Alias Command

Manage command aliases:

```sh
# List aliases
dotnet-exec alias ls

# Create alias
dotnet-exec alias set guid "Guid.NewGuid()"

# Use alias
dotnet-exec guid

# Remove alias
dotnet-exec alias unset guid
```

### Test Command

Execute xUnit tests:

```sh
# Run test file
dotnet-exec test MyTests.cs

# Run multiple test files
dotnet-exec test Test1.cs Test2.cs Test3.cs

# Run tests with additional references
dotnet-exec test MyTests.cs -r 'nuget:Moq' -u 'Moq'
```

## Common Options

### Debug and Development

```sh
# Enable debug output
dotnet-exec script.cs --debug

# Dry run (compile but don't execute)
dotnet-exec script.cs --dry-run

# Use preview language features
dotnet-exec script.cs --preview
```

### Reference Management

```sh
# Wide references (includes common packages)
dotnet-exec script.cs --wide

# Disable wide references
dotnet-exec script.cs --wide false

# Web references
dotnet-exec script.cs --web
```

### Environment and Configuration

```sh
# Set environment variables
dotnet-exec script.cs --env 'VAR1=value1' --env 'VAR2=value2'

# Set execution timeout
dotnet-exec script.cs --timeout 300

# Use specific .NET framework
dotnet-exec script.cs --framework net8.0
```

## Next Steps

Now that you understand the basics, explore these guides for more advanced usage:

- **[Advanced Usage Guide](advanced-usage.md)**: Complex scenarios, multiple scripts, and advanced options
- **[References Guide](references-guide.md)**: Comprehensive reference management
- **[Profiles and Aliases](profiles-and-aliases.md)**: Configuration management and shortcuts
- **[Testing Guide](testing-guide.md)**: xUnit integration and testing workflows
- **[Examples and Use Cases](examples.md)**: Real-world examples across different domains
- **[Troubleshooting](troubleshooting.md)**: Common issues and solutions

## Getting Help

```sh
# General help
dotnet-exec --help

# Command-specific help
dotnet-exec profile --help
dotnet-exec alias --help
dotnet-exec test --help

# System information
dotnet-exec --info
```

For additional support, visit the [GitHub repository](https://github.com/WeihanLi/dotnet-exec) or check the [troubleshooting guide](troubleshooting.md).
