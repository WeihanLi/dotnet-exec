# Getting Started

## Intro

`dotnet-exec` dotnet-exec 是一个可以执行 C# 程序而不需要项目文件的命令行工具，并且你可以指定自定义的入口方法不仅仅是默认的 Main 方法

，我们可以使用

```sh
dotnet tool update -g dotnet-execute
```

来安装或更新到最新的版本，如果想要体验最新的预览版，可以使用 `--prerelease` 

```sh
dotnet tool update -g dotnet-execute --prerelease
```

如果本地没有 dotnet sdk 也可以通过 docker/podman 等来体验

```sh
docker/podman run --rm weihanli/dotnet-exec:latest "1+1"
```

## Commands

### Default command

`dotnet-exec` 可以直接执行一个脚本 `dotnet-exec <script>` 

#### Script

script 支持三种：

- script 路径，支持本地的 local path 和远程的 path比如 github 等，比如 `dotnet-exec 'Hello.cs'`/`dotnet-exec 'https://aka.ms/abc/Hello.cs'`
- 原始代码，比如： `dotnet-exec 'Console.WriteLine("Hello dotnet-exec")'`
- 原始 C# Script 代码, 比如： `dotnet-exec 'Guid.NewGuid()'`

`script` 也支持指定多个 script，如：`dotnet-exec A.cs B.cs`

#### REPL

不提供 script 参数的时候，默认会开启一个 REPL(`Read Evaluate Print Loop`)，大体和在 Visual Studio 中的 C# Interative 一样，并且额外支持 nuget reference，可以通过 `#r nuget:CsvHelper` 或者 `#r nuget:WeihanLi.Npoi,2.4.2` 等

对于某些 API 不太熟悉，忘记了应该怎么输入，也可以通过在最后输入一个 `?` 来获取代码提示

#### Options


### Profile command

