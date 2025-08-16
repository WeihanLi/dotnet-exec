# Profiles and Aliases Guide

This guide covers configuration profiles and command aliases - powerful features that help you save time and maintain consistency when using dotnet-exec.

## Configuration Profiles

Configuration profiles allow you to save common sets of options and reuse them across multiple script executions, eliminating the need to repeatedly specify the same references, usings, and other settings.

### Profile Management Commands

#### List Profiles

View all configured profiles:

```sh
# List all profiles
dotnet-exec profile ls

# Alternative command
dotnet-exec profile list
```

#### Create/Update Profiles

Set up new profiles or update existing ones:

```sh
# Basic profile creation
dotnet-exec profile set myprofile -r 'nuget:Serilog' -u 'Serilog'

# Comprehensive profile with multiple options
dotnet-exec profile set webdev \
  --web \
  -r 'nuget:Swashbuckle.AspNetCore' \
  -r 'nuget:AutoMapper' \
  -u 'AutoMapper' \
  --preview \
  --default-entry MainTest Execute
```

#### View Profile Details

Check what's configured in a specific profile:

```sh
# Get profile configuration
dotnet-exec profile get webdev
```

#### Remove Profiles

Delete profiles you no longer need:

```sh
# Remove a profile
dotnet-exec profile rm oldprofile
```

### Profile Options

Profiles can store the following configuration options:

- **References** (`-r`, `--reference`): NuGet packages, local files, projects
- **Usings** (`-u`, `--using`): Namespace imports
- **Web References** (`--web`): ASP.NET Core framework
- **Wide References** (`--wide`): Extended reference set
- **Entry Point** (`-e`, `--entry`): Custom entry method
- **Preview Features** (`--preview`): Language preview features
- **Default Entry Methods** (`--default-entry`): Fallback entry methods

### Common Profile Examples

#### Web Development Profile

```sh
dotnet-exec profile set web \
  --web \
  -r 'nuget:Swashbuckle.AspNetCore' \
  -r 'nuget:Microsoft.EntityFrameworkCore' \
  -r 'nuget:AutoMapper' \
  -u 'AutoMapper' \
  -u 'Microsoft.EntityFrameworkCore'
```

Usage:
```sh
dotnet-exec ApiScript.cs --profile web
```

#### Data Processing Profile

```sh
dotnet-exec profile set data \
  -r 'nuget:CsvHelper' \
  -r 'nuget:Newtonsoft.Json' \
  -r 'nuget:System.Data.SqlClient' \
  -u 'CsvHelper' \
  -u 'Newtonsoft.Json' \
  -u 'System.Data.SqlClient'
```

#### Testing Profile

```sh
dotnet-exec profile set testing \
  -r 'nuget:xunit' \
  -r 'nuget:Moq' \
  -r 'nuget:FluentAssertions' \
  -u 'Xunit' \
  -u 'Moq' \
  -u 'FluentAssertions' \
  --default-entry TestMain TestExecute
```

#### Desktop Development Profile

```sh
dotnet-exec profile set desktop \
  -r 'framework:desktop' \
  -r 'nuget:System.Management' \
  -r 'nuget:Microsoft.Win32.Registry' \
  -u 'System.Management' \
  --wide false
```

#### Machine Learning Profile

```sh
dotnet-exec profile set ml \
  -r 'nuget:Microsoft.ML' \
  -r 'nuget:Microsoft.ML.AutoML' \
  -r 'nuget:Microsoft.Data.Analysis' \
  -u 'Microsoft.ML' \
  -u 'Microsoft.ML.AutoML' \
  --preview
```

### Using Profiles

#### Basic Profile Usage

```sh
# Execute script with profile
dotnet-exec MyScript.cs --profile webdev

# REPL with profile
dotnet-exec --profile data
```

#### Combining Profiles with Additional Options

```sh
# Use profile and add extra references
dotnet-exec MyScript.cs \
  --profile web \
  -r 'nuget:Redis.StackExchange' \
  -u 'StackExchange.Redis'

# Override profile settings
dotnet-exec MyScript.cs \
  --profile web \
  --using '-AutoMapper'  # Remove AutoMapper from web profile
```

#### Profile Inheritance and Overrides

When you use a profile with additional options:
- Additional references and usings are added to the profile's settings
- Using statements starting with `-` remove items from the profile
- Other options override the profile's settings

```sh
# Example: Start with web profile, customize for specific use
dotnet-exec CustomWebScript.cs \
  --profile web \
  -r 'nuget:SignalR' \
  --using 'Microsoft.AspNetCore.SignalR' \
  --using '-AutoMapper'  # Remove from web profile
```

## Command Aliases

Aliases allow you to create shortcuts for frequently used commands, making your workflow more efficient.

### Alias Management Commands

#### List Aliases

View all configured aliases:

```sh
# List all aliases
dotnet-exec alias list

# Alternative command
dotnet-exec alias ls
```

#### Create Aliases

Set up new aliases:

```sh
# Simple expression alias
dotnet-exec alias set guid "Guid.NewGuid()"

# Complex script alias
dotnet-exec alias set timestamp "DateTime.Now.ToString(\"yyyy-MM-dd HH:mm:ss\")"

# Multi-line script alias
dotnet-exec alias set weather "
var client = new HttpClient();
var response = await client.GetStringAsync(\"https://api.weather.com/current\");
response.Dump();
"
```

#### Remove Aliases

Delete aliases you no longer need:

```sh
# Remove an alias
dotnet-exec alias unset guid

# Alternative command
dotnet-exec alias rm timestamp
```

### Common Alias Examples

#### Quick Utilities

```sh
# Generate GUID
dotnet-exec alias set guid "Guid.NewGuid()"

# Current timestamp
dotnet-exec alias set now "DateTime.Now"

# Random number
dotnet-exec alias set random "new Random().Next(1, 100)"

# Base64 encode
dotnet-exec alias set base64 "Convert.ToBase64String(Encoding.UTF8.GetBytes(args[0]))"
```

#### System Information

```sh
# Machine info
dotnet-exec alias set sysinfo "Environment.MachineName + \" - \" + Environment.OSVersion"

# Current directory
dotnet-exec alias set pwd "Directory.GetCurrentDirectory()"

# Environment variables
dotnet-exec alias set env "Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().OrderBy(x => x.Key)"
```

#### Development Helpers

```sh
# JSON formatting
dotnet-exec alias set jsonformat "System.Text.Json.JsonSerializer.Serialize(System.Text.Json.JsonSerializer.Deserialize<object>(args[0]), new System.Text.Json.JsonSerializerOptions { WriteIndented = true })"

# Hash calculation
dotnet-exec alias set md5 "Convert.ToHexString(System.Security.Cryptography.MD5.HashData(Encoding.UTF8.GetBytes(args[0])))"

# URL encoding
dotnet-exec alias set urlencode "System.Web.HttpUtility.UrlEncode(args[0])"
```

### Using Aliases

#### Basic Alias Usage

```sh
# Execute alias
dotnet-exec guid

# Execute with arguments
dotnet-exec base64 "Hello World"
```

#### Aliases with Complex Logic

```sh
# Create a file listing alias
dotnet-exec alias set listfiles "
var path = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
Directory.GetFiles(path).Select(f => new { Name = Path.GetFileName(f), Size = new FileInfo(f).Length }).Dump();
"

# Use the alias
dotnet-exec listfiles ./src
```

## Advanced Profile and Alias Patterns

### Environment-Specific Profiles

```sh
# Development environment
dotnet-exec profile set dev \
  --web \
  -r 'nuget:Microsoft.Extensions.Logging.Debug' \
  --preview

# Production environment  
dotnet-exec profile set prod \
  --web \
  -r 'nuget:Serilog.Sinks.ApplicationInsights' \
  --wide false
```

### Project-Specific Profiles

```sh
# Profile for specific project
dotnet-exec profile set myproject \
  --project ./MyProject.csproj \
  -r 'nuget:ProjectSpecificPackage' \
  --default-entry ProjectMain

# Use in project context
dotnet-exec Scripts/ProjectScript.cs --profile myproject
```

### Chained Aliases

Create aliases that build upon each other:

```sh
# Base web request alias
dotnet-exec alias set httprequest "
var client = new HttpClient();
var response = await client.GetStringAsync(args[0]);
response.Dump();
"

# Specific API alias using the base
dotnet-exec alias set apicheck "
var url = \"https://api.github.com/repos/\" + args[0];
var client = new HttpClient();
client.DefaultRequestHeaders.UserAgent.ParseAdd(\"dotnet-exec\");
var response = await client.GetStringAsync(url);
response.Dump();
"
```

### Conditional Aliases

```sh
# Platform-specific behavior
dotnet-exec alias set osinfo "
if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
    \"Windows: \" + Environment.OSVersion.VersionString;
} else {
    \"Unix-like: \" + Environment.OSVersion.VersionString;
}.Dump();
"
```

## Best Practices

### Profile Organization

1. **Use Descriptive Names**:
   ```sh
   dotnet-exec profile set aspnet-webapi-dev \
     --web \
     -r 'nuget:Swashbuckle.AspNetCore'
   ```

2. **Layer Profiles Logically**:
   ```sh
   # Base web profile
   dotnet-exec profile set web-base --web
   
   # Extended for specific scenarios
   dotnet-exec profile set web-api \
     --web \
     -r 'nuget:Swashbuckle.AspNetCore'
   ```

3. **Version Control Profile Definitions**:
   Create scripts to set up profiles consistently across environments:
   ```sh
   #!/bin/bash
   # setup-profiles.sh
   dotnet-exec profile set webdev --web -r 'nuget:Swashbuckle.AspNetCore'
   dotnet-exec profile set data -r 'nuget:CsvHelper' -u 'CsvHelper'
   ```

### Alias Organization

1. **Use Clear Naming Conventions**:
   ```sh
   dotnet-exec alias set util-guid "Guid.NewGuid()"
   dotnet-exec alias set dev-timestamp "DateTime.Now"
   dotnet-exec alias set net-ping "new System.Net.NetworkInformation.Ping().Send(args[0])"
   ```

2. **Document Complex Aliases**:
   ```sh
   # Create documentation alias
   dotnet-exec alias set help-aliases "
   Console.WriteLine(\"Available aliases:\");
   Console.WriteLine(\"  guid - Generate new GUID\");
   Console.WriteLine(\"  now - Current timestamp\");
   Console.WriteLine(\"  sysinfo - System information\");
   "
   ```

3. **Parameterize When Possible**:
   ```sh
   # Good: parameterized
   dotnet-exec alias set file-size "new FileInfo(args[0]).Length"
   
   # Less flexible: hardcoded
   dotnet-exec alias set myfile-size "new FileInfo(\"myfile.txt\").Length"
   ```

### Integration with Development Workflow

#### Team Sharing

Share profiles and aliases through version control:

```sh
# Export current configuration
dotnet-exec profile get webdev > profiles/webdev.json
dotnet-exec alias list > aliases/current-aliases.txt

# Setup script for new team members
dotnet-exec Scripts/setup-environment.cs --profile team-setup
```

#### CI/CD Integration

Use profiles in build and deployment scripts:

```sh
# Build script
dotnet-exec BuildScript.cs --profile build-tools

# Deployment verification
dotnet-exec VerifyDeployment.cs --profile production-check
```

For more information on creating and managing scripts, see the [Getting Started](getting-started.md) and [Advanced Usage](advanced-usage.md) guides.