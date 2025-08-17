# Advanced Usage Guide

This guide covers advanced features and scenarios for using dotnet-exec effectively.

## Script Types and Execution Modes

### Local File Execution

Execute C# files with custom entry points:

```sh
# Default entry point (Main method)
dotnet-exec MyScript.cs

# Custom entry point
dotnet-exec MyScript.cs --entry MainTest

# Multiple entry methods (fallback)
dotnet-exec MyScript.cs --default-entry MainTest Execute Run
```

### Remote File Execution

Execute scripts directly from URLs:

```sh
# GitHub raw file
dotnet-exec https://raw.githubusercontent.com/user/repo/main/script.cs

# Any accessible URL
dotnet-exec https://example.com/scripts/sample.cs
```

### Multiple Script Files

Execute multiple scripts together:

```sh
# Multiple files as arguments
dotnet-exec Script1.cs Script2.cs Script3.cs

# Using additional scripts option
dotnet-exec MainScript.cs --addition Helper1.cs --addition Helper2.cs
```

## Advanced Reference Management

### Framework References

Target specific framework features:

```sh
# Web framework (ASP.NET Core)
dotnet-exec 'WebApplication.Create().Run();' --web

# Explicit framework reference
dotnet-exec 'WebApplication.Create().Run();' --reference 'framework:web'

# Multiple framework references
dotnet-exec MyScript.cs --reference 'framework:web' --reference 'framework:desktop'
```

### Project References

Use existing project dependencies:

```sh
# Reference a project file
dotnet-exec MyScript.cs --reference 'project:./MyLibrary.csproj'

# Extract all references from project
dotnet-exec MyScript.cs --project ./MyProject.csproj
```

### Complex NuGet References

```sh
# Specific version
dotnet-exec MyScript.cs -r 'nuget:Newtonsoft.Json,13.0.3'

# Prerelease versions
dotnet-exec MyScript.cs -r 'nuget:Microsoft.Extensions.Hosting,8.0.0-preview.1'

# Multiple packages
dotnet-exec MyScript.cs \
  -r 'nuget:Serilog' \
  -r 'nuget:Serilog.Sinks.Console' \
  -u 'Serilog'
```

## Advanced Using Statements

### Static Imports

```sh
# Static using for Console methods
dotnet-exec 'WriteLine("Hello World");' --using 'static System.Console'

# Multiple static imports
dotnet-exec MyScript.cs \
  --using 'static System.Console' \
  --using 'static System.Math'
```

### Using Aliases

```sh
# Static usings
dotnet-exec MyScript.cs \
  --using 'static System.Math' \
  -r 'System.Runtime'

# Multiple usings
dotnet-exec MyScript.cs \
  --using 'System.Collections.Generic' \
  --using 'System.Linq'
```

> **Note**: Namespace and type aliases are supported through script directives (`// using: MyConsole = System.Console`) or project files (`<Using Alias="..." Include="..." />`), but not directly via the `--using` command line option.

### Removing Default Usings

```sh
# Remove default System namespace
dotnet-exec 'System.Console.WriteLine("Hello");' --using '-System'

# Remove multiple namespaces
dotnet-exec MyScript.cs --using '-System' --using '-System.Collections.Generic'
```

## Compilation and Execution Options

### Preview Features

```sh
# Enable preview language features and APIs that require RequiresPreviewFeaturesAttribute
dotnet-exec PreviewScript.cs --preview

# With specific preview APIs
dotnet-exec 'SomePreviewAPI();' \
  --preview \
  -r 'nuget:Microsoft.Extensions.Hosting,8.0.0-preview.1'
```

### Debug and Development

```sh
# Debug mode for detailed diagnostics
dotnet-exec MyScript.cs --debug

# Dry run (compile but don't execute)
dotnet-exec MyScript.cs --dry-run

# Compile to specific output location
dotnet-exec MyScript.cs --compile-out ./output/compiled.dll
```

### Optimization Levels

```sh
# Debug configuration
dotnet-exec MyScript.cs --configuration Debug

# Release configuration (optimized)
dotnet-exec MyScript.cs --configuration Release
```

## Environment and Process Control

### Environment Variables

```sh
# Set environment variables
dotnet-exec MyScript.cs \
  --env 'DATABASE_URL=localhost:5432' \
  --env 'LOG_LEVEL=Debug'

# Multiple environment settings
dotnet-exec MyScript.cs \
  --env 'ENV=Development' \
  --env 'API_KEY=secret123'
```

### Timeout Control

```sh
# Set execution timeout (in seconds)
dotnet-exec LongRunningScript.cs --timeout 300

# For infinite loops or long processes
dotnet-exec InteractiveScript.cs --timeout 0
```

### Command Arguments

Pass arguments to your script:

```sh
# Using -- separator
dotnet-exec MyScript.cs -- arg1 arg2 arg3

# With complex arguments
dotnet-exec ProcessFiles.cs -- --input "./data" --output "./results" --format json
```

## Advanced REPL Usage

### REPL with Custom Configuration

```sh
# Start REPL with wide references
dotnet-exec --wide

# REPL with web framework loaded
dotnet-exec --web

# REPL with custom profile
dotnet-exec --profile myprofile
```

### REPL Commands

Within the REPL session:

```csharp
// Reference NuGet packages
#r "nuget:Newtonsoft.Json"
#r "nuget:Serilog,3.1.1"

// Reference local files
#r "./libs/MyLibrary.dll"
#r "folder:./assemblies"

// Code completion
Console.? // Press ? for intellisense

// Multiple line expressions
var data = new {
    Name = "Test",
    Value = 42
};
```

## Source Generators and Compilation Features

### Source Generator Support

```sh
# Enable source generators
dotnet-exec MyScript.cs --generator

# With specific generator packages
dotnet-exec MyScript.cs \
  --generator \
  -r 'nuget:AutoMapper.Extensions.Microsoft.DependencyInjection'
```

### Preprocessor Symbols

```sh
# Define compilation symbols
dotnet-exec MyScript.cs --compile-symbol DEBUG --compile-symbol TRACE

# With conditional compilation
dotnet-exec ConditionalScript.cs --compile-symbol FEATURE_X
```

### Custom Features

```sh
# Enable specific compiler features
dotnet-exec MyScript.cs --compile-feature nullable --compile-feature records
```

## Performance and Caching

### Cache Management

```sh
# Disable compilation cache
dotnet-exec MyScript.cs --disable-cache

# Force recompilation
dotnet-exec MyScript.cs --disable-cache --debug
```

### Assembly Loading

```sh
# Use reference assemblies for compilation
dotnet-exec MyScript.cs --use-ref-assemblies
```

## Integration Scenarios

### CI/CD Integration

```sh
# Script execution in pipelines
dotnet-exec Scripts/DeploymentValidation.cs \
  --env "ENVIRONMENT=Production" \
  --timeout 600 \
  --debug

# Health check scripts
dotnet-exec HealthCheck.cs \
  --web \
  -r 'nuget:Microsoft.Extensions.Http' \
  --timeout 30
```

### Development Workflows

```sh
# Database migration scripts
dotnet-exec DatabaseMigration.cs \
  --project ./MyApp.csproj \
  --env "CONNECTION_STRING=Server=localhost;Database=MyApp"

# Build verification scripts
dotnet-exec VerifyBuild.cs \
  --addition ./TestHelpers.cs \
  --debug \
  --dry-run
```

## Error Handling and Debugging

### Verbose Output

```sh
# Detailed execution information
dotnet-exec MyScript.cs --debug --dry-run

# With specific compiler diagnostics
dotnet-exec MyScript.cs --debug --compile-symbol VERBOSE_LOGGING
```

### Common Issues and Solutions

1. **Assembly Resolution Errors**:
   ```sh
   # Use folder references for local assemblies
   dotnet-exec MyScript.cs -r 'folder:./libs' --debug
   ```

2. **NuGet Package Conflicts**:
   ```sh
   # Specify exact versions
   dotnet-exec MyScript.cs -r 'nuget:PackageA,1.0.0' -r 'nuget:PackageB,2.0.0'
   ```

3. **Memory Issues with Large Scripts**:
   ```sh
   # Use compilation output for reuse
   dotnet-exec LargeScript.cs --compile-out ./compiled.dll
   ```

This advanced usage guide covers the most sophisticated features of dotnet-exec. For basic usage, see the [Getting Started](getting-started.md) guide.