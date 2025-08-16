# References Guide

This guide provides comprehensive information about managing references in dotnet-exec, including assemblies, NuGet packages, and framework dependencies.

## Overview

dotnet-exec supports various types of references to extend your scripts with additional functionality:

- **NuGet Package References**: Reference packages from NuGet.org or custom feeds
- **Local File References**: Reference local DLL files
- **Folder References**: Reference all DLLs in a directory
- **Project References**: Reference .csproj projects and inherit their dependencies
- **Framework References**: Reference specific .NET framework components
- **Default References**: Built-in framework assemblies included automatically

## NuGet Package References

### Basic NuGet References

Use the `nuget:` prefix to reference NuGet packages:

```sh
# Latest stable version
dotnet-exec MyScript.cs -r 'nuget:Newtonsoft.Json'

# Specific version
dotnet-exec MyScript.cs -r 'nuget:Newtonsoft.Json,13.0.3'

# Prerelease version
dotnet-exec MyScript.cs -r 'nuget:Microsoft.Extensions.Hosting,8.0.0-preview.1'
```

### Multiple NuGet Packages

```sh
# Multiple package references
dotnet-exec MyScript.cs \
  -r 'nuget:Serilog' \
  -r 'nuget:Serilog.Sinks.Console' \
  -r 'nuget:Serilog.Sinks.File'

# With corresponding using statements
dotnet-exec MyScript.cs \
  -r 'nuget:AutoMapper' \
  -r 'nuget:FluentValidation' \
  -u 'AutoMapper' \
  -u 'FluentValidation'
```

### Version Constraints

```sh
# Exact version
-r 'nuget:PackageName,1.2.3'

# Version ranges (using standard NuGet syntax)
-r 'nuget:PackageName,[1.0.0,2.0.0)'  # >= 1.0.0 and < 2.0.0
-r 'nuget:PackageName,[1.0.0,]'       # >= 1.0.0
-r 'nuget:PackageName,(,2.0.0)'       # < 2.0.0
```

### Private NuGet Feeds

```sh
# Using custom NuGet config
dotnet-exec MyScript.cs \
  --nuget-config ./custom-nuget.config \
  -r 'nuget:PrivatePackage,1.0.0'
```

## Local File References

### Single DLL Reference

```sh
# Reference a specific DLL
dotnet-exec MyScript.cs -r './libs/MyLibrary.dll'

# Absolute path
dotnet-exec MyScript.cs -r '/path/to/assembly.dll'

# Relative to script location
dotnet-exec MyScript.cs -r '../shared/Common.dll'
```

### Examples with Local References

```sh
# Reference custom business logic
dotnet-exec DataProcessor.cs \
  -r './BusinessLogic.dll' \
  -r './DataAccess.dll' \
  -u 'MyCompany.BusinessLogic' \
  -u 'MyCompany.DataAccess'

# Reference third-party DLLs
dotnet-exec ReportGenerator.cs \
  -r './ThirdParty/ReportingEngine.dll' \
  -u 'ReportingEngine'
```

## Folder References

### Reference All DLLs in a Directory

Use the `folder:` prefix to reference all DLL files in a directory:

```sh
# Reference all DLLs in a folder
dotnet-exec MyScript.cs -r 'folder:./libs'

# Multiple folders
dotnet-exec MyScript.cs \
  -r 'folder:./libs' \
  -r 'folder:./plugins' \
  -r 'folder:../shared/assemblies'
```

### Folder Reference Examples

```sh
# Reference build output directory
dotnet-exec TestScript.cs -r 'folder:./bin/Debug/net8.0'

# Reference plugin directory
dotnet-exec PluginHost.cs \
  -r 'folder:./plugins' \
  -u 'MyApp.Plugins'

# Reference distributed libraries
dotnet-exec ProcessingScript.cs \
  -r 'folder:/opt/myapp/libs' \
  -r 'folder:./additional-libs'
```

## Project References

### Reference a Project File

Use the `project:` prefix to reference a .csproj file and inherit its dependencies:

```sh
# Reference a project
dotnet-exec MyScript.cs -r 'project:./MyLibrary.csproj'

# Reference multiple projects
dotnet-exec MyScript.cs \
  -r 'project:./Core.csproj' \
  -r 'project:./Utilities.csproj'
```

### Extract All References from Project

Use the `--project` option to inherit all references and usings from a project:

```sh
# Inherit everything from project
dotnet-exec MyScript.cs --project ./MyApp.csproj

# Override specific settings while inheriting from project
dotnet-exec MyScript.cs \
  --project ./MyApp.csproj \
  --using 'MyCustomNamespace'
```

### Project Reference Examples

```sh
# Use existing web application dependencies
dotnet-exec AdminScript.cs --project ./MyWebApp.csproj

# Reference class library for utilities
dotnet-exec DataMigration.cs \
  -r 'project:./DataLayer.csproj' \
  --using 'MyApp.Data'

# Multiple related projects
dotnet-exec IntegrationTest.cs \
  -r 'project:./Core.csproj' \
  -r 'project:./Services.csproj' \
  -r 'project:./Models.csproj'
```

## Framework References

### Web Framework Reference

```sh
# ASP.NET Core framework
dotnet-exec WebScript.cs --web

# Equivalent explicit reference
dotnet-exec WebScript.cs -r 'framework:web'

# With additional web packages
dotnet-exec WebApiScript.cs \
  --web \
  -r 'nuget:Swashbuckle.AspNetCore'
```

### Other Framework References

```sh
# Windows Desktop frameworks
dotnet-exec WinFormsScript.cs -r 'framework:desktop'

# WPF applications
dotnet-exec WpfScript.cs -r 'framework:wpf'

# Custom framework references
dotnet-exec AdvancedScript.cs -r 'framework:custom-name'
```

### Framework Reference Examples

```sh
# Web API development script
dotnet-exec ApiTester.cs \
  --web \
  -r 'nuget:RestSharp' \
  -u 'RestSharp'

# Desktop automation script
dotnet-exec AutomationScript.cs \
  -r 'framework:desktop' \
  -r 'nuget:System.Management'
```

## Default References

### Built-in Framework References

dotnet-exec automatically includes these framework references:

- `System.Private.CoreLib`
- `System.Console`
- `System.Runtime`
- `System.Collections`
- `System.Linq`
- `System.Text.Json`
- `Microsoft.CSharp`

### Wide References Mode

Enable additional common references with `--wide`:

```sh
# Include extensive reference set
dotnet-exec MyScript.cs --wide
```

Wide mode includes:
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging
- WeihanLi.Common
- WeihanLi.Extensions

### Disable Wide References

```sh
# Use minimal reference set
dotnet-exec MyScript.cs --wide false
```

## Reference Resolution

### Resolution Order

References are resolved in this order:

1. Framework references (built-in)
2. NuGet package references
3. Local file references
4. Folder references
5. Project references

### Conflict Resolution

```sh
# Force specific versions to avoid conflicts
dotnet-exec MyScript.cs \
  -r 'nuget:PackageA,1.0.0' \
  -r 'nuget:PackageB,2.0.0' \
  --debug  # Shows resolution details
```

### Troubleshooting References

```sh
# Debug reference resolution
dotnet-exec MyScript.cs --debug --dry-run

# Check what references are loaded
dotnet-exec MyScript.cs \
  -r 'nuget:SomePackage' \
  --dry-run \
  --debug
```

## Best Practices

### Organizing References

1. **Group by Type**:
   ```sh
   dotnet-exec MyScript.cs \
     -r 'nuget:Serilog' \
     -r 'nuget:AutoMapper' \
     -r 'project:./Core.csproj' \
     -r './libs/Custom.dll'
   ```

2. **Use Profiles for Common Sets**:
   ```sh
   # Create a profile for web development
   dotnet-exec profile set webdev \
     --web \
     -r 'nuget:Swashbuckle.AspNetCore' \
     -r 'nuget:EntityFrameworkCore'
   
   # Use the profile
   dotnet-exec WebScript.cs --profile webdev
   ```

### Version Management

1. **Pin Critical Versions**:
   ```sh
   dotnet-exec MyScript.cs -r 'nuget:CriticalPackage,1.2.3'
   ```

2. **Use Latest for Development**:
   ```sh
   dotnet-exec TestScript.cs -r 'nuget:DevelopmentPackage'
   ```

### Performance Considerations

1. **Use Folder References for Multiple DLLs**:
   ```sh
   # Better than multiple file references
   dotnet-exec MyScript.cs -r 'folder:./libs'
   ```

2. **Leverage Project References**:
   ```sh
   # Inherits transitive dependencies efficiently
   dotnet-exec MyScript.cs --project ./MyApp.csproj
   ```

## Common Reference Patterns

### Data Access Pattern

```sh
dotnet-exec DataScript.cs \
  -r 'nuget:Microsoft.EntityFrameworkCore' \
  -r 'nuget:Microsoft.EntityFrameworkCore.SqlServer' \
  -u 'Microsoft.EntityFrameworkCore'
```

### Web API Client Pattern

```sh
dotnet-exec ApiClient.cs \
  --web \
  -r 'nuget:RestSharp' \
  -r 'nuget:Polly' \
  -u 'RestSharp' \
  -u 'Polly'
```

### Logging Pattern

```sh
dotnet-exec LoggingScript.cs \
  -r 'nuget:Serilog' \
  -r 'nuget:Serilog.Sinks.Console' \
  -r 'nuget:Serilog.Sinks.File' \
  -u 'Serilog'
```

### Testing Pattern

```sh
dotnet-exec TestRunner.cs \
  -r 'nuget:xUnit' \
  -r 'nuget:Moq' \
  -r 'project:./TestProject.csproj' \
  -u 'Xunit' \
  -u 'Moq'
```

For more information on using these references in your scripts, see the [Getting Started](getting-started.md) and [Advanced Usage](advanced-usage.md) guides.