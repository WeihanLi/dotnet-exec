# dotnet-exec

[![dotnet-execute](https://img.shields.io/nuget/v/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/)

[![dotnet-execute Latest](https://img.shields.io/nuget/vpre/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/absoluteLatest)

[![default](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnetcore.yml)

[![Docker Pulls](https://img.shields.io/docker/pulls/weihanli/dotnet-exec)](https://hub.docker.com/r/weihanli/dotnet-exec)

## Intro

`dotnet-exec` is a command line tool for executing C# program without a project file, and you can have your custom entry point other than `Main` method

## Install/Update

Latest stable version:

```sh
dotnet tool update -g dotnet-execute
```

Latest preview version:

```sh
dotnet tool update -g dotnet-execute --prerelease
```

## Examples

Execute local file:

``` sh
dotnet-exec HttpPathJsonSample.cs
```

Execute local file with custom entry point:

``` sh
dotnet-exec HttpPathJsonSample.cs --entry MainTest
```

Execute remote file:

``` sh
dotnet-exec https://github.com/WeihanLi/SamplesInPractice/blob/master/net7Sample/Net7Sample/ArgumentExceptionSample.cs
```

Execute file with preview features:

``` sh
dotnet-exec RawStringLiteral.cs --preview
```

Execute raw code:

``` sh
dotnet-exec 'code:Console.WriteLine(1+1);'

dotnet-exec 'Console.WriteLine(1+1);'
```

Execute raw code with custom usings:

``` sh
dotnet-exec 'code:WriteLine(1+1);' --using "static System.Console"

dotnet-exec 'WriteLine(1+1);' --using "static System.Console"
```

Execute raw code with custom references:

NuGet package reference:

``` sh
dotnet-exec 'code:CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "nuget: WeihanLi.Npoi,2.3.0" -u "WeihanLi.Npoi"
```

Local dll reference:

``` sh
dotnet-exec 'code:CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "./out/WeihanLi.Npoi.dll" -u "WeihanLi.Npoi"
```

Local dll in a folder references:

``` sh
dotnet-exec 'code:CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "folder: ./out" -u "WeihanLi.Npoi"
```

Local project reference:

``` sh
dotnet-exec 'code:CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "project: ./WeihanLi.Npoi.csproj" -u "WeihanLi.Npoi"
```

Execute script:

```sh
dotnet-exec 'script:1+1'

dotnet-exec '1+1'
```

Execute script with custom reference:

```sh
dotnet-exec 'script:Console.WriteLine(CsvHelper.GetCsvText(new[]{1,2,3}))' -r "nuget:WeihanLi.Npoi,2.4.2" -u WeihanLi.Npoi
```

``` sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump()' -r "nuget:WeihanLi.Npoi,2.4.2" -u WeihanLi.Npoi
```

Execute with additional dependencies

``` sh
dotnet-exec 'typeof(LocalType).FullName.Dump();' --ad FileLocalType2.cs
```

``` sh
dotnet-exec 'typeof(LocalType).FullName.Dump();' --addition FileLocalType2.cs
```

Execute with exacting references and usings from project file

``` sh
dotnet-exec 'typeof(LocalType).FullName.Dump();' --project ./Sample.csproj
```

Execute with docker

``` sh
docker run --rm weihanli/dotnet-exec:latest dotnet-exec "1+1"
```

``` sh
docker run --rm weihanli/dotnet-exec:latest dotnet-exec "Guid.NewGuid()"
```

``` sh
docker run --rm weihanli/dotnet-exec:latest dotnet-exec "ApplicationHelper.RuntimeInfo.Dump()"
```

## More

### LanguageVersion

By default, it's using the latest language version, you can use the `Preview` version with `--preview`

### EntryPoint

By default, it would use `MainTest` as the entry point, you can customize with `--entry` option

### TargetFramework

By default, it would use `net7.0` if you've installed .NET 7 SDK, otherwise use .NET 6 instead, you can customize with the `-f`/`--framework` option
