# dotnet-exec

Package | Latest | Latest Preview
---- | ---- | ----
dotnet-execute | [![dotnet-execute](https://img.shields.io/nuget/v/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/) | [![dotnet-execute Latest](https://img.shields.io/nuget/vpre/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/absoluteLatest)
ReferenceResolver | [![ReferenceResolver](https://img.shields.io/nuget/v/ReferenceResolver)](https://www.nuget.org/packages/ReferenceResolver/) | [![ReferenceResolver Latest](https://img.shields.io/nuget/vpre/ReferenceResolver)](https://www.nuget.org/packages/ReferenceResolver/absoluteLatest)

[![default](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnet.yml/badge.svg)](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnet.yml)

[![Docker Pulls](https://img.shields.io/docker/pulls/weihanli/dotnet-exec)](https://hub.docker.com/r/weihanli/dotnet-exec)

[For English](./README.md)

## Intro

`dotnet-exec` 是一个可以执行 C# 程序而不需要项目文件的命令行工具，并且你可以指定自定义的入口方法不仅仅是 `Main` 方法

## Install/Update

### dotnet tool

最新的稳定版本:

```sh
dotnet tool update -g dotnet-execute
```

最新的预览版本:

```sh
dotnet tool update -g dotnet-execute --prerelease
```

### 容器支持

使用 docker

``` sh
docker run --rm weihanli/dotnet-exec:latest "1+1"
```

``` sh
docker run --rm weihanli/dotnet-exec:latest "Guid.NewGuid()"
```

``` sh
docker run --rm --pull=always weihanli/dotnet-exec:latest "ApplicationHelper.RuntimeInfo"
```

使用 podman

``` sh
podman run --rm weihanli/dotnet-exec:latest "1+1"
```

``` sh
podman run --rm weihanli/dotnet-exec:latest "Guid.NewGuid()"
```

``` sh
podman run --rm --pull=always weihanli/dotnet-exec:latest "ApplicationHelper.RuntimeInfo"
```

完整的 tag 列表请参考 <https://hub.docker.com/r/weihanli/dotnet-exec/tags>

## Examples

### Get started

执行本地文件:

``` sh
dotnet-exec HttpPathJsonSample.cs
```

执行本地文件并且自定义入口方法:

``` sh
dotnet-exec 'HttpPathJsonSample.cs' --entry MainTest
```

执行远程文件:

``` sh
dotnet-exec 'https://github.com/WeihanLi/SamplesInPractice/blob/master/net7Sample/Net7Sample/ArgumentExceptionSample.cs'
```

执行原始代码:

``` sh
dotnet-exec 'Console.WriteLine(1+1);'
```

执行原始脚本:

```sh
dotnet-exec 'script:1+1'
```

``` sh
dotnet-exec 'Guid.NewGuid()'
```

### References

执行原始代码并自定义程序集引用:

NuGet 包引用:

``` sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "nuget: WeihanLi.Npoi,2.5.0" -u "WeihanLi.Npoi"
```

本地 dll 引用:

``` sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "./out/WeihanLi.Npoi.dll" -u "WeihanLi.Npoi"
```

本地目录下的 dll 引用:

``` sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "folder: ./out" -u "WeihanLi.Npoi"
```

本地项目引用:

``` sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "project: ./WeihanLi.Npoi.csproj" -u "WeihanLi.Npoi"
```

框架引用:

``` sh
dotnet-exec 'WebApplication.Create().Run();' --reference 'framework:web'
```

使用 `--web` 一个选项来添加 web 框架引用:

``` sh
dotnet-exec 'WebApplication.Create().Run();' --web
```

### Usings

执行原始代码并且自定义命名空间引用:

``` sh
dotnet-exec 'WriteLine(1+1);' --using "static System.Console"
```

执行原始脚本并且自定义命名空间引用:

``` sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump()' -r "nuget:WeihanLi.Npoi,2.5.0" -u WeihanLi.Npoi
```

### More

执行原始代码并且指定更多依赖：

``` sh
dotnet-exec 'typeof(LocalType).FullName.Dump();' --ad FileLocalType2.cs
```

``` sh
dotnet-exec 'typeof(LocalType).FullName.Dump();' --addition FileLocalType2.cs
```

执行原始代码并且指定从项目文件中提取 using 信息和 reference 信息：

``` sh
dotnet-exec 'typeof(LocalType).FullName.Dump();' --project ./Sample.csproj
```

执行本地文件并指定启用预览特性:

``` sh
dotnet-exec RawStringLiteral.cs --preview
```

### Config Profile

你可以自定义常用的配置到一个 profile 配置里以方便重复使用。

列出所有可用的 profile 配置:

``` sh
dotnet-exec profile ls
```

配置一个 profile:

``` sh
dotnet-exec profile set web -r "nuget:WeihanLi.Web.Extensions" -u 'WeihanLi.Web.Extensions' --web --wide false
```

获取一个 profile 配置详情:

``` sh
dotnet-exec profile get web
```

移除不需要的 profile 配置:

``` sh
dotnet-exec profile rm web
```

执行代码时指定某一个 profile 配置:

``` sh
dotnet-exec 'WebApplication.Create().Chain(_=>_.MapRuntimeInfo()).Run();' --profile web --using 'WeihanLi.Extensions'
```

![image](https://user-images.githubusercontent.com/7604648/205428791-48f0863b-ca9a-4a55-93cd-bb5514845c5d.png)

执行代码时指定某一个 profile 配置并且移除配置中的某一个 using:

``` sh
dotnet-exec 'WebApplication.Create().Run();' --profile web --using '-WeihanLi.Extensions'
```

## Acknowledgements

- [Roslyn](https://github.com/dotnet/roslyn)
- [NuGet.Clients](https://github.com/NuGet/NuGet.Client)
- [System.CommandLine](https://github.com/dotnet/command-line-api)
- [感谢 JetBrains 提供的 Rider 开源 license](https://jb.gg/OpenSource?from=dotnet-exec)
- 感谢这个项目的贡献者和使用者
