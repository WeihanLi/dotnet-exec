# Getting Started

## Intro

`dotnet-exec` is a command-line tool for executing C# program without a project file, and you can have your custom entry point other than the `Main` method

We could use

```sh
dotnet tool update -g dotnet-execute
```

to install or update to latest version, if you want to experience the latest preview version, with a `--prerelease` option

```sh
dotnet tool update -g dotnet-execute --prerelease
```

You can also use `docker`/`podman` without dotnet sdk installed

```sh
docker/podman run --rm weihanli/dotnet-exec:latest "1+1"
```

## Commands

### Default command

`dotnet-exec` can execute C# script directly `dotnet-exec <script>` 

#### Script

script type：

- script path，support local file path and remote file path, for examples: `dotnet-exec 'Hello.cs'`/`dotnet-exec 'https://aka.ms/abc/Hello.cs'`
- Raw C# code, for exmaple: `dotnet-exec 'Console.WriteLine("Hello dotnet-exec")'`
- Raw C# Script Code, for example： `dotnet-exec 'Guid.NewGuid()'`

`script` support multiple scripts, for example: `dotnet-exec A.cs B.cs`

#### REPL

It would start a REPL(`Read Evaluate Print Loop`) if no arguments provided, it would work like C# Interactive in Visual Studio, and you can use `#r nuget:CsvHelper` or `#r nuget:WeihanLi.Npoi,2.4.2` to reference a nuget package.

For those API not so familar, you could end with a `?` to get code completions

#### Options


### Profile command

