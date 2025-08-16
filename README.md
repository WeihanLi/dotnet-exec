# dotnet-exec

A powerful command-line tool for executing C# programs without project files, featuring custom entry points, REPL mode, comprehensive reference management, and integrated testing capabilities.

## 🎯 Overview

`dotnet-exec` simplifies C# development by allowing you to:

- **Execute C# code directly** from command line or files
- **Use custom entry points** beyond the traditional `Main` method
- **Access REPL mode** for interactive C# development
- **Reference NuGet packages, local DLLs, and frameworks** seamlessly
- **Run xUnit tests** without project setup
- **Save configurations** as reusable profiles
- **Create aliases** for frequently used commands
- **Work with remote scripts** via URLs

## 📦 Package Information

Package | Latest | Latest Preview
---- | ---- | ----
dotnet-execute | [![dotnet-execute](https://img.shields.io/nuget/v/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/) | [![dotnet-execute Latest](https://img.shields.io/nuget/vpre/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/absoluteLatest)
ReferenceResolver | [![ReferenceResolver](https://img.shields.io/nuget/v/ReferenceResolver)](https://www.nuget.org/packages/ReferenceResolver/) | [![ReferenceResolver Latest](https://img.shields.io/nuget/vpre/ReferenceResolver)](https://www.nuget.org/packages/ReferenceResolver/absoluteLatest)

[![default](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnet.yml/badge.svg)](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnet.yml)
[![Docker Pulls](https://img.shields.io/docker/pulls/weihanli/dotnet-exec)](https://hub.docker.com/r/weihanli/dotnet-exec)
[![GitHub Commit Activity](https://img.shields.io/github/commit-activity/m/WeihanLi/dotnet-exec)](https://github.com/WeihanLi/dotnet-exec/commits/main)
[![GitHub Release](https://img.shields.io/github/v/release/WeihanLi/dotnet-exec)](https://github.com/WeihanLi/dotnet-exec/releases)
[![BuiltWithDot.Net shield](https://builtwithdot.net/project/5741/dotnet-exec/badge)](https://builtwithdot.net/project/5741/dotnet-exec)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/WeihanLi/dotnet-exec)

**📖 Documentation**: [English](./docs/index.md) | [中文介绍](./README.zh-CN.md)

## 🚀 Quick Start

### Installation

```sh
# Install latest stable version
dotnet tool install -g dotnet-execute

# Install latest preview version
dotnet tool install -g dotnet-execute --prerelease

# Update to latest version
dotnet tool update -g dotnet-execute
```

### Basic Usage

```sh
# Execute simple expressions
dotnet-exec "1 + 1"
dotnet-exec "Guid.NewGuid()"
dotnet-exec "DateTime.Now"

# Execute C# statements
dotnet-exec 'Console.WriteLine("Hello, dotnet-exec!");'

# Execute script files
dotnet-exec MyScript.cs

# Execute remote scripts
dotnet-exec https://raw.githubusercontent.com/user/repo/main/script.cs

# Start REPL mode
dotnet-exec
```

## ✨ Key Features

### 🎯 Multiple Execution Modes
- **Raw Code**: Execute C# expressions and statements directly
- **Script Files**: Run local .cs files with custom entry points
- **Remote Scripts**: Execute scripts from URLs
- **REPL Mode**: Interactive C# development environment

### 📚 Rich Reference Support
- **NuGet Packages**: Latest or specific versions
- **Local Assemblies**: DLL files and folder references
- **Project References**: Inherit dependencies from .csproj files
- **Framework References**: Web, desktop, and custom frameworks

### 🧪 Integrated Testing
- **xUnit Integration**: Run tests without project setup
- **Test Discovery**: Automatic test method detection
- **Custom References**: Add packages for testing scenarios

### ⚙️ Configuration Management
- **Profiles**: Save and reuse common configurations
- **Aliases**: Create shortcuts for frequent commands
- **Environment Variables**: Set execution context

### 🛠️ Developer-Friendly
- **Custom Entry Points**: Use methods beyond `Main`
- **Preview Features**: Access latest C# language features
- **Debug Mode**: Detailed compilation and execution information
- **Container Support**: Docker/Podman execution without .NET SDK

## 📖 Usage Examples

### Basic Execution

```sh
# Simple calculations
dotnet-exec "Math.Sqrt(16)"
dotnet-exec "string.Join(\", \", new[] {\"a\", \"b\", \"c\"})"

# Working with dates
dotnet-exec "DateTime.Now.AddDays(7).ToString(\"yyyy-MM-dd\")"

# File operations
dotnet-exec "Directory.GetFiles(\".\", \"*.cs\").Length"
```

### Script Files with Custom Entry Points

Create `example.cs`:
```csharp
public class Example
{
    public static void MainTest()
    {
        Console.WriteLine("Custom entry point executed!");
    }
    
    public static void Execute()
    {
        Console.WriteLine("Alternative entry method");
    }
}
```

```sh
# Use custom entry point
dotnet-exec example.cs --entry MainTest

# Use default entry method fallbacks
dotnet-exec example.cs --default-entry MainTest Execute Run
```

### References and Using Statements

```sh
# NuGet package references
dotnet-exec 'JsonConvert.SerializeObject(new {name="test"})' \
  -r 'nuget:Newtonsoft.Json' \
  -u 'Newtonsoft.Json'

# Multiple references
dotnet-exec MyScript.cs \
  -r 'nuget:Serilog' \
  -r 'nuget:AutoMapper' \
  -u 'Serilog' \
  -u 'AutoMapper'

# Local DLL references
dotnet-exec MyScript.cs -r './libs/MyLibrary.dll'

# Framework references
dotnet-exec 'WebApplication.Create().Run();' --web
```

### REPL Mode

```sh
# Start interactive mode
dotnet-exec

# In REPL:
> #r "nuget:Newtonsoft.Json"
> using Newtonsoft.Json;
> var obj = new { Name = "Test", Value = 42 };
> JsonConvert.SerializeObject(obj)
"{"Name":"Test","Value":42}"
```

### Testing

Execute xUnit tests without project setup:

```sh
# Run test file
dotnet-exec test MyTests.cs

# Run multiple test files
dotnet-exec test Test1.cs Test2.cs Test3.cs

# Run tests with additional references
dotnet-exec test MyTests.cs \
  -r 'nuget:Moq' \
  -r 'nuget:FluentAssertions' \
  -u 'Moq' \
  -u 'FluentAssertions'
```

Example test file:
```csharp
public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        var result = 2 + 3;
        Assert.Equal(5, result);
    }
    
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, 0)]
    public void Add_VariousInputs_ReturnsExpected(int a, int b, int expected)
    {
        var result = a + b;
        Assert.Equal(expected, result);
    }
}
```

### Configuration Profiles

Save common configurations for reuse:

```sh
# Create a web development profile
dotnet-exec profile set webdev \
  --web \
  -r 'nuget:Swashbuckle.AspNetCore' \
  -r 'nuget:AutoMapper' \
  -u 'AutoMapper'

# List profiles
dotnet-exec profile ls

# Use profile
dotnet-exec MyWebScript.cs --profile webdev

# Get profile details
dotnet-exec profile get webdev

# Remove profile
dotnet-exec profile rm webdev
```

### Command Aliases

Create shortcuts for frequently used commands:

```sh
# Create aliases
dotnet-exec alias set guid "Guid.NewGuid()"
dotnet-exec alias set now "DateTime.Now"
dotnet-exec alias set hash "Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(args[0])))"

# List aliases
dotnet-exec alias ls

# Use aliases
dotnet-exec guid
dotnet-exec now
dotnet-exec hash "text to hash"

# Remove alias
dotnet-exec alias unset guid
```

### Container Support

Execute with Docker/Podman without .NET SDK:

```sh
# Docker
docker run --rm weihanli/dotnet-exec:latest "1+1"
docker run --rm weihanli/dotnet-exec:latest "Guid.NewGuid()"
docker run --rm weihanli/dotnet-exec:latest "DateTime.Now"

# Podman
podman run --rm weihanli/dotnet-exec:latest "1+1"

# Mount local files
docker run --rm -v $(pwd):/workspace weihanli/dotnet-exec:latest MyScript.cs
```

For the full image tag list, see <https://hub.docker.com/r/weihanli/dotnet-exec/tags>

## 📚 Documentation

### Comprehensive Guides

- **[Getting Started](docs/articles/en/getting-started.md)**: Installation, basic usage, and core concepts
- **[Advanced Usage](docs/articles/en/advanced-usage.md)**: Complex scenarios and advanced features
- **[References Guide](docs/articles/en/references-guide.md)**: Managing assemblies, packages, and dependencies
- **[Profiles and Aliases](docs/articles/en/profiles-and-aliases.md)**: Configuration management and shortcuts
- **[Testing Guide](docs/articles/en/testing-guide.md)**: xUnit integration and testing workflows
- **[Examples and Use Cases](docs/articles/en/examples.md)**: Real-world examples across different domains
- **[Troubleshooting](docs/articles/en/troubleshooting.md)**: Common issues and solutions

### Quick Reference

```sh
# Get help
dotnet-exec --help
dotnet-exec profile --help
dotnet-exec alias --help
dotnet-exec test --help

# System information
dotnet-exec --info
```

## 🎤 Presentations

- [Makes C# more simple -- .NET Conf China 2022](https://github.com/WeihanLi/dotnet-exec/blob/main/docs/slides/dotnet-conf-china-2022-dotnet-exec_makes_csharp_more_simple.pdf)
- [dotnet-exec simpler C# -- .NET Conf China 2023 Watch Party Shanghai](https://github.com/WeihanLi/dotnet-exec/blob/main/docs/slides/dotnet-exec-simpler-csharp.pdf)

## 🔗 GitHub Actions Integration

Execute C# code in CI/CD without .NET SDK setup:

- **Repository**: <https://github.com/WeihanLi/dotnet-exec-action>
- **Marketplace**: <https://github.com/marketplace/actions/dotnet-exec>

Example usage:
```yaml
- name: Execute C# Script
  uses: WeihanLi/dotnet-exec-action@main
  with:
    script: 'Console.WriteLine("Hello from GitHub Actions!");'
```

## 🙏 Acknowledgements

- [Roslyn](https://github.com/dotnet/roslyn) - C# compiler and analysis APIs
- [NuGet.Clients](https://github.com/NuGet/NuGet.Client) - Package management
- [System.CommandLine](https://github.com/dotnet/command-line-api) - Command-line interface
- [Thanks JetBrains for the open source Rider license](https://jb.gg/OpenSource?from=dotnet-exec)
- Many thanks to all contributors and users of this project

## 📄 License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 🐛 Issues & Support

- **Report bugs**: [GitHub Issues](https://github.com/WeihanLi/dotnet-exec/issues)
- **Ask questions**: [GitHub Discussions](https://github.com/WeihanLi/dotnet-exec/discussions)
- **Documentation**: [Comprehensive Guides](docs/articles/en/)

---

⭐ **If you find this project helpful, please consider giving it a star!**
