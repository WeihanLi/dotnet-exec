# Architecture

dotnet-exec is built with a modular, extensible architecture that supports various execution scenarios and compilation strategies.

## Core Components

### 1. Command Handler (`CommandHandler.cs`)

The main orchestrator that:
- Processes command-line arguments
- Determines execution mode (REPL vs script execution)
- Coordinates the compilation and execution pipeline

### 2. Reference Resolution (`ReferenceResolver`)

Handles all types of references:
- **NuGet Package Resolution**: Downloads and resolves NuGet packages
- **Framework References**: .NET framework assemblies
- **Local File References**: DLL files and project references
- **Dependency Resolution**: Transitive dependency handling

### 3. Compilation System

**Compiler Factory Pattern**:
- `ICompilerFactory`: Creates appropriate compiler instances
- Multiple compiler implementations for different scenarios
- Support for different .NET versions and compilation options

**Script Options Configuration**:
- Language version selection
- Optimization levels
- Unsafe code support
- Global using statements

### 4. Execution System

**Executor Factory Pattern**:
- `IExecutorFactory`: Creates appropriate executor instances
- Support for different execution environments
- Entry point resolution (Main, custom methods)

### 5. REPL System (`Repl.cs`)

**Interactive Execution Engine**:
- Built on Microsoft.CodeAnalysis.CSharp.Scripting
- State persistence between statements
- Dynamic reference loading
- Completion service integration

**Script State Management**:
- Maintains execution context
- Variable and type definitions
- Exception handling and recovery

## Architecture Patterns

### 1. Pipeline Pattern

**Options Configuration Pipeline**:
```
Raw Options → Pre-Configure → Script Fetch → Configure → Compiled Options
```

**Middleware Components**:
- `AliasOptionsPreConfigureMiddleware`: Processes command aliases
- `ProjectFileOptionsConfigureMiddleware`: Handles project file references
- Extensible middleware system for custom processing

### 2. Factory Pattern

**Compiler Factory**:
- Abstracts compilation strategy selection
- Supports multiple compilation backends
- Enables easy extension for new compilation scenarios

**Executor Factory**:
- Abstracts execution strategy selection
- Supports different runtime environments
- Handles entry point resolution

### 3. Service Layer Architecture

**Dependency Injection**:
- All services registered in DI container
- Interface-based design for testability
- Scoped service lifetimes

**Core Services**:
- `IScriptContentFetcher`: Retrieves script content from various sources
- `IConfigProfileManager`: Manages configuration profiles
- `IScriptCompletionService`: Provides IntelliSense-like completion
- `IUriTransformer`: Handles URL transformations and shortcuts

## Data Flow

### Script Execution Flow

```
1. Command Line Parsing
   ↓
2. Options Binding & Profile Loading
   ↓
3. Pre-Configuration Pipeline
   ↓
4. Script Content Fetching
   ↓
5. Configuration Pipeline
   ↓
6. Reference Resolution
   ↓
7. Compilation
   ↓
8. Assembly Execution
```

### REPL Flow

```
1. REPL Initialization
   ↓
2. Script Options Setup
   ↓
3. Interactive Loop:
   - Read Input
   - Parse Commands
   - Handle Special Commands
   - Compile & Execute
   - Display Results
   - Update State
```

## Extension Points

### 1. Custom Middleware

Implement `IOptionsPreConfigureMiddleware` or `IOptionsConfigureMiddleware`:

```csharp
public class CustomMiddleware : IOptionsConfigureMiddleware
{
    public Task Execute(ExecOptions options)
    {
        // Custom option processing logic
        return Task.CompletedTask;
    }
}
```

### 2. Custom Compilers

Implement `ICompiler` interface:

```csharp
public class CustomCompiler : ICompiler
{
    public async Task<Result<CompileResult>> Compile(ExecOptions options, string sourceText)
    {
        // Custom compilation logic
    }
}
```

### 3. Custom Executors

Implement `IExecutor` interface:

```csharp
public class CustomExecutor : IExecutor
{
    public async Task<int> Execute(ExecOptions options, CompileResult compileResult)
    {
        // Custom execution logic
    }
}
```

## Performance Considerations

### 1. Compilation Caching

- Assembly caching based on source content hash
- Reference resolution caching
- Metadata reference reuse

### 2. Reference Resolution Optimization

- Parallel package downloads
- Local cache utilization
- Incremental resolution

### 3. REPL Optimization

- Script state reuse
- Incremental compilation
- Memory management for long-running sessions

## Security Model

### 1. Code Execution

- No sandboxing by default
- Full .NET runtime capabilities
- User responsibility for code safety

### 2. Reference Resolution

- NuGet package verification
- Local file access controls
- Network access for remote packages

### 3. REPL Security

- Same security model as script execution
- Dynamic reference loading capabilities
- Session isolation

## Integration Scenarios

### 1. Build Systems

- MSBuild integration
- GitHub Actions workflows
- Docker container execution

### 2. Development Tools

- IDE integration patterns
- Notebook-style development
- Debugging capabilities

### 3. Automation Scripts

- CI/CD pipeline integration
- System administration tasks
- Data processing workflows

This architecture provides a flexible, extensible foundation for C# script execution while maintaining performance and ease of use across different scenarios.