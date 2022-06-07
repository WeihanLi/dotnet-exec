# dotnet-exec

[![dotnet-execute](https://img.shields.io/nuget/v/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/)

[![dotnet-execute Latest](https://img.shields.io/nuget/vpre/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/absoluteLatest)

[![default](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnetcore.yml)

## Intro

`dotnet-exec` is a command line tool for executing C# program without a project file, and you can have custom entry point other than `Main` method

## Install

```sh
dotnet tool update -g dotnet-execute
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

Execute raw code:

``` sh
dotnet-exec 'code:Console.WriteLine(1+1);'
```

## More

### LanguageVersion

By default, it's using the latest language version, you can use the `Preview` version with `--lang-version=Preview`

### EntryPoint

By default, it would use `MainTest` as the entry point, you can customize with `--entry` option

### TargetFramework

By default, it would use `net7.0` if you've installed .NET 7 SDK, otherwise use .NET 6 instead, you can customize with the `-f`/`--framework` option

### CompilerType

By default, it would use the `SimpleCodeCompiler` to compile the code, you can customize with the `--compiler-type` option, and you can use `-a`/`--advanced` for `--compiler-type=advanced`
