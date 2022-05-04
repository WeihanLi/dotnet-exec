# dotnet-exec

[![dotnet-execute](https://img.shields.io/nuget/v/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/)

[![dotnet-execute Latest](https://img.shields.io/nuget/vpre/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/absoluteLatest)

[![default](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnetcore.yml)

## Intro

`dotnet-exec` is a command line tool for custom C# program entry point

``` sh
dotnet-exec HttpPathJsonSample.cs

dotnet-exec HttpPathJsonSample.cs --entry MainTest
```

## Install

```sh
dotnet tool update -g dotnet-execute
```

## More

### LanguageVersion

By default, it's using the latest language version, you can use the `Preview` version with `--lang-version=Preview`

### EntryPoint

By default, it would use `MainTest` as the entry point, you can customize with `--entry` option
