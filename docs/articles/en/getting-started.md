# Getting Started

## Intro

`dotnet-exec` is a command-line tool for executing C# program without a project file, and you can have your custom entry point other than the `Main` method

## Install

We could use

```sh
dotnet tool update -g dotnet-execute
```

to install or update to latest version, if you want to experience the latest preview version, with a `--prerelease` option

```sh
dotnet tool update -g dotnet-execute --prerelease
```

If install failed, it may relate to the nuget source config, you could try the following script to install

```sh
dotnet tool update -g dotnet-execute --prerelease --add-source https://api.nuget.org/v3/index.json --ignore-failed-sources
```

You can also use `docker`/`podman` without dotnet sdk installed

```sh
docker/podman run --rm weihanli/dotnet-exec:latest "1+1"
```

## Commands

### Default command

`dotnet-exec` can execute C# script directly via `dotnet-exec <script>` 

#### Script

Script type：

- script path，support local file path and remote file path, for examples: `dotnet-exec 'Hello.cs'`/`dotnet-exec 'https://aka.ms/abc/Hello.cs'`
- Raw C# code, for exmaple: `dotnet-exec 'Console.WriteLine("Hello dotnet-exec")'`
- Raw C# Script Code, for example： `dotnet-exec 'Guid.NewGuid()'`

`script` support multiple scripts, for example: `dotnet-exec A.cs B.cs`

#### REPL

It would start a REPL(`Read Evaluate Print Loop`) if no arguments provided, it would work like C# Interactive in Visual Studio or dotnet-script, and you can use `#r nuget:CsvHelper` or `#r nuget:WeihanLi.Npoi,2.5.0` to reference a nuget package, and you can pass some options to configure the default references and options etc

For those API not so familar, you could end with a `?` to get code completions

#### Options

**Using**

`dotnet-exec` would include the default implicit usings

`-u`/`--using` to add a `namespace` using, it could support common namespace using, static using and using alias, and you could start with `-` to remove namespace using

examples:

> Default implicit using

```sh
dotnet-exec 'Console.WriteLine("Hello World");'
```

> static using

```sh
dotnet-exec 'WriteLine("Hello World");' -u 'static System.Console'
```

> using alias

```sh
dotnet-exec 'MyConsole.WriteLine("Hello World");' -u 'MyConsole = System.Console'
```

> remove specific using

```sh
dotnet-exec 'System.Console.WriteLine("Hello World");' -u '-System'
```

**References**

`dotnet-exec` would include the default framework references, including the `System.Private.CoreLib`/`System.Console`/`System.Text.Json` etc...

Besides, you could also add other references like following:

**NuGet Package Reference**

> nuget package reference without version:

```sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "nuget: WeihanLi.Npoi" -u "WeihanLi.Npoi"
```

By default, it you specific the nuget package only, if would try to use the latest stable package version, you can also specific the package version want to use

> nuget package reference with specific version:

```sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "nuget: WeihanLi.Npoi,2.5.0" -u "WeihanLi.Npoi"
```

**local file reference**

Reference a local dll file

```sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "./out/WeihanLi.Npoi.dll" -u "WeihanLi.Npoi"
```

**local folder reference**

Reference local dll files inside a folder

```sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "folder: ./out" -u "WeihanLi.Npoi"
```

**SDK framework reference**

Reference SDK Web framework reference:

```sh
dotnet-exec 'WebApplication.Create().Run();' --reference 'framework:web'
```

`web` is an alias for `Microsoft.AspNetCore.App`, and you could use `--web` option to enable the AspNetCore framework reference

**preview option**

By default, we use the `latest` language version, if we want to try out some preview features, we could use `--preview` option to use the `preview` language version,
and some preview features annotated with `RequiresPreviewFeaturesAttribute` could also be tried with this option

### Profile command

Profile commands could configure custom config profile to simplify options configuration when executing
