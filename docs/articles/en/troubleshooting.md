# Troubleshooting Guide

This guide helps you diagnose and resolve common issues when using dotnet-exec.

## General Troubleshooting

### Enable Debug Mode

The first step in troubleshooting is to enable debug mode for detailed diagnostic information:

```sh
# Enable debug output
dotnet-exec MyScript.cs --debug

# Combine with dry-run to see compilation details without execution
dotnet-exec MyScript.cs --debug --dry-run
```

Debug mode provides:
- Assembly loading details
- Reference resolution information
- Compilation diagnostics
- Execution environment details

## Installation Issues

### Tool Installation Problems

#### Issue: `dotnet tool install` fails

**Symptoms:**
```
error: Failed to install tool 'dotnet-execute': unable to find package
```

**Solutions:**

1. **Update NuGet sources:**
   ```sh
   dotnet tool install -g dotnet-execute --add-source https://api.nuget.org/v3/index.json
   ```

2. **Clear NuGet cache:**
   ```sh
   dotnet nuget locals all --clear
   dotnet tool install -g dotnet-execute
   ```

3. **Use specific version:**
   ```sh
   dotnet tool install -g dotnet-execute --version 0.31.0
   ```

4. **Ignore failed sources:**
   ```sh
   dotnet tool install -g dotnet-execute --ignore-failed-sources
   ```

#### Issue: Tool update fails

**Solution:**
```sh
# Uninstall and reinstall
dotnet tool uninstall -g dotnet-execute
dotnet tool install -g dotnet-execute
```

### PATH Issues

#### Issue: `dotnet-exec` command not found

**Symptoms:**
```
bash: dotnet-exec: command not found
```

**Solutions:**

1. **Check if tools directory is in PATH:**
   ```sh
   echo $PATH | grep -o '[^:]*\.dotnet[^:]*'
   ```

2. **Add tools directory to PATH (Linux/macOS):**
   ```sh
   export PATH="$PATH:$HOME/.dotnet/tools"
   ```

3. **Add to profile for persistence:**
   ```sh
   echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.bashrc
   source ~/.bashrc
   ```

4. **Windows PowerShell:**
   ```powershell
   $env:PATH += ";$env:USERPROFILE\.dotnet\tools"
   ```

## Compilation Issues

### Reference Resolution Problems

#### Issue: Assembly or package not found

**Symptoms:**
```
error CS0246: The type or namespace name 'SomeType' could not be found
```

**Debug steps:**
```sh
# Check what references are being loaded
dotnet-exec MyScript.cs --debug --dry-run
```

**Solutions:**

1. **Add missing NuGet reference:**
   ```sh
   dotnet-exec MyScript.cs -r 'nuget:MissingPackage'
   ```

2. **Add namespace using:**
   ```sh
   dotnet-exec MyScript.cs -r 'nuget:SomePackage' -u 'SomePackage.Namespace'
   ```

3. **Check package name and version:**
   ```sh
   # Use exact package name from NuGet.org
   dotnet-exec MyScript.cs -r 'nuget:Newtonsoft.Json,13.0.3'
   ```

#### Issue: Version conflicts

**Symptoms:**
```
warning: Version conflict detected for assembly
```

**Solutions:**

1. **Specify exact versions:**
   ```sh
   dotnet-exec MyScript.cs \
     -r 'nuget:PackageA,1.0.0' \
     -r 'nuget:PackageB,2.0.0'
   ```

2. **Use compatible versions:**
   ```sh
   # Check compatibility on NuGet.org
   dotnet-exec MyScript.cs -r 'nuget:PackageA,[1.0.0,2.0.0)'
   ```

3. **Clear cache and retry:**
   ```sh
   dotnet-exec MyScript.cs --disable-cache
   ```

### Compilation Errors

#### Issue: Language feature not supported

**Symptoms:**
```
error CS8400: Feature 'xyz' is not available in C# 8.0
```

**Solutions:**

1. **Enable preview features:**
   ```sh
   dotnet-exec MyScript.cs --preview
   ```

2. **Check target framework:**
   ```sh
   dotnet-exec MyScript.cs --framework net8.0
   ```

#### Issue: Using directive errors

**Symptoms:**
```
error CS0246: The type or namespace name 'System' could not be found
```

**Solutions:**

1. **Check if usings were accidentally removed:**
   ```sh
   # Don't remove essential namespaces
   dotnet-exec MyScript.cs --using '-System'  # This might cause issues
   ```

2. **Restore essential usings:**
   ```sh
   dotnet-exec MyScript.cs --using 'System'
   ```

3. **Use wide references for comprehensive namespace coverage:**
   ```sh
   dotnet-exec MyScript.cs --wide
   ```

## Runtime Issues

### Execution Failures

#### Issue: Entry point not found

**Symptoms:**
```
error: No suitable entry point found
```

**Solutions:**

1. **Specify custom entry point:**

   ```sh
   dotnet-exec MyScript.cs --entry MyCustomMain
   ```

2. **Use default entry methods:**

   ```sh
   dotnet-exec MyScript.cs
   ```

3. **Check method signature:**

   ```csharp
   // Valid entry points
   public static void MainTest() { }
   public static async Task MainTest() { }
   public static int MainTest() { }
   public static async Task<int> MainTest() { }
   ```

#### Issue: File not found

**Symptoms:**

```
error: Could not find file 'Script.cs'
```

**Solutions:**

1. **Use absolute paths:**

   ```sh
   dotnet-exec /full/path/to/MyScript.cs
   ```

2. **Check working directory:**

   ```sh
   # Change to script directory
   cd /path/to/scripts
   dotnet-exec MyScript.cs
   ```

3. **Verify file exists:**

   ```sh
   ls -la MyScript.cs
   ```

### Performance Issues

#### Issue: Slow compilation

**Solutions:**

1. **Use compilation cache:**

   ```sh
   # Default behavior, but ensure cache isn't disabled
   dotnet-exec MyScript.cs  # Uses cache
   ```

2. **Pre-compile for reuse:**

   ```sh
   dotnet-exec MyScript.cs --compile-out ./compiled.dll
   # Later executions will be faster
   ```

3. **Optimize references:**

   ```sh
   # Use profiles to avoid repeated reference resolution
   dotnet-exec profile set myprofile -r 'nuget:CommonPackage'
   dotnet-exec MyScript.cs --profile myprofile
   ```

#### Issue: High memory usage

**Solutions:**

1. **Use reference assemblies:**

   ```sh
   dotnet-exec MyScript.cs --use-ref-assemblies
   ```

2. **Disable wide references if not needed:**

   ```sh
   dotnet-exec MyScript.cs --wide false
   ```

3. **Clear cache periodically:**

   ```sh
   # Clear compilation cache
   rm -rf ~/.dotnet-exec/cache  # Linux/macOS
   rmdir /s %USERPROFILE%\.dotnet-exec\cache  # Windows
   ```

## Network and Package Issues

### NuGet Package Problems

#### Issue: Package download fails

**Symptoms:**

```
error: Failed to download package 'PackageName'
```

**Solutions:**

1. **Check internet connectivity:**

   ```sh
   ping nuget.org
   ```

2. **Use alternative package source:**

   ```sh
   dotnet-exec MyScript.cs \
     --nuget-config ./custom-nuget.config \
     -r 'nuget:PackageName'
   ```

3. **Retry with timeout:**

   ```sh
   dotnet-exec MyScript.cs -r 'nuget:PackageName' --timeout 300
   ```

#### Issue: Package not found

**Solutions:**

1. **Verify package name on NuGet.org:**

   ```sh
   # Check exact name and available versions
   dotnet-exec MyScript.cs -r 'nuget:Correct.Package.Name'
   ```

2. **Try different version:**

   ```sh
   dotnet-exec MyScript.cs -r 'nuget:PackageName,1.0.0'
   ```

### Remote File Access

#### Issue: Cannot download remote script

**Symptoms:**

```
error: Failed to download script from URL
```

**Solutions:**

1. **Check URL accessibility:**

   ```sh
   curl -I "https://example.com/script.cs"
   ```

2. **Use direct GitHub raw URLs:**

   ```sh
   # Use raw.githubusercontent.com URLs
   dotnet-exec https://raw.githubusercontent.com/user/repo/main/script.cs
   ```

3. **Download and run locally:**

   ```sh
   curl -o temp-script.cs "https://example.com/script.cs"
   dotnet-exec temp-script.cs
   ```

## Platform-Specific Issues

### Windows Issues

#### Issue: PowerShell execution policy

**Symptoms:**

```
execution of scripts is disabled on this system
```

**Solutions:**

1. **Change execution policy:**

   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

2. **Run with bypass:**

   ```powershell
   PowerShell -ExecutionPolicy Bypass -Command "dotnet-exec MyScript.cs"
   ```

#### Issue: Long path names

**Solutions:**

1. **Enable long path support in Windows:**
   - Group Policy: Computer Configuration → Administrative Templates → System → Filesystem → Enable Win32 long paths

2. **Use shorter paths:**

   ```sh
   # Move closer to root
   copy MyScript.cs C:\temp\
   dotnet-exec C:\temp\MyScript.cs
   ```

### Linux/macOS Issues

#### Issue: Permission denied

**Solutions:**

1. **Check file permissions:**

   ```sh
   chmod +x ~/.dotnet/tools/dotnet-exec
   ```

2. **Run with sudo if needed:**

   ```sh
   sudo dotnet-exec MyScript.cs
   ```

#### Issue: Library loading errors

**Solutions:**

1. **Install required native dependencies:**

   ```sh
   # Ubuntu/Debian
   sudo apt-get update
   sudo apt-get install libc6-dev

   # CentOS/RHEL
   sudo yum install glibc-devel
   ```

2. **Check .NET installation:**

   ```sh
   dotnet --info
   ```

## Configuration Issues

### Profile Problems

#### Issue: Profile not found

**Symptoms:**

```
error: Profile 'myprofile' not found
```

**Solutions:**

1. **List available profiles:**

   ```sh
   dotnet-exec profile ls
   ```

2. **Create the profile:**

   ```sh
   dotnet-exec profile set myprofile -r 'nuget:SomePackage'
   ```

3. **Check profile name spelling:**

   ```sh
   # Profile names are case-sensitive
   dotnet-exec profile get MyProfile  # Different from 'myprofile'
   ```

### Alias Issues

#### Issue: Alias not working

**Solutions:**

1. **List aliases to verify:**

   ```sh
   dotnet-exec alias ls
   ```

2. **Check alias definition:**

   ```sh
   # Aliases should contain valid C# code
   dotnet-exec alias set test "Console.WriteLine(\"Hello\");"
   ```

3. **Test alias syntax:**

   ```sh
   # Test the code separately first
   dotnet-exec 'Console.WriteLine("Hello");'
   ```

## Advanced Debugging

### Environment Information

#### Get system information

```sh
# Built-in info command
dotnet-exec --info

# Or create diagnostic script
dotnet-exec '
Console.WriteLine($"OS: {Environment.OSVersion}");
Console.WriteLine($".NET Version: {Environment.Version}");
Console.WriteLine($"Working Directory: {Environment.CurrentDirectory}");
Console.WriteLine($"Machine Name: {Environment.MachineName}");
Environment.GetEnvironmentVariables()
    .Cast<DictionaryEntry>()
    .Where(e => e.Key.ToString().Contains("DOTNET"))
    .OrderBy(e => e.Key)
    .ToList()
    .ForEach(e => Console.WriteLine($"{e.Key}: {e.Value}"));
'
```

#### Trace compilation process

```sh
# Verbose compilation diagnostics
dotnet-exec MyScript.cs \
  --debug \
  --dry-run \
  --compile-symbol TRACE \
  --compile-symbol DEBUG
```

### Log Analysis

#### Enable detailed logging

```sh
# Set environment variables for detailed logging
export DOTNET_EXEC_LOG_LEVEL=Debug
dotnet-exec MyScript.cs --debug
```

#### Save debug output

```sh
# Capture debug information
dotnet-exec MyScript.cs --debug --dry-run > debug-output.txt 2>&1
```

## Getting Help

### Command Line Help

```sh
# General help
dotnet-exec --help

# Command-specific help
dotnet-exec test --help
dotnet-exec profile --help
dotnet-exec alias --help
```

### Community Resources

1. **GitHub Issues**: Report bugs and ask questions at https://github.com/WeihanLi/dotnet-exec/issues
2. **Documentation**: Check the latest documentation
3. **Examples**: Review sample scripts and use cases

### Creating Minimal Reproduction

When reporting issues:

1. **Create minimal script:**

   ```csharp
   // Minimal.cs - simplest code that reproduces the issue
   Console.WriteLine("Hello World");
   ```

2. **Include command used:**

   ```sh
   dotnet-exec Minimal.cs --debug
   ```

3. **Provide environment info:**

   ```sh
   dotnet --info
   dotnet-exec --info
   ```

4. **Include error output:**

   ```sh
   dotnet-exec Minimal.cs --debug > output.txt 2>&1
   ```

This troubleshooting guide covers the most common issues. For additional help, see the [Getting Started](getting-started.md) guide or check the project's GitHub repository for known issues and solutions.
