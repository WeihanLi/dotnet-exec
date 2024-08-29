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

不提供 script 参数的时候，默认会开启一个 REPL(`Read Evaluate Print Loop`)，大体和在 Visual Studio 中的 C# Interative 一样，并且额外支持 nuget reference，可以通过 `#r nuget:CsvHelper` 或者 `#r nuget:WeihanLi.Npoi,2.5.0` 等

对于某些 API 不太熟悉，忘记了应该怎么输入，也可以通过在最后输入一个 `?` 来获取代码提示

#### Options

**Using**

`dotnet-exec` 会包含默认的隐式命名空间引用

使用 `-u`/`--using` 来新增一个 `namespace` 引用, 支持普通的命名空间引用，静态引用以及引用别名，并且可以通过以 `-` 开头来移除命名空间引用

示例如下:

> 默认隐式命名空间引用

```sh
dotnet-exec 'Console.WriteLine("Hello World");'
```

> 静态引用

```sh
dotnet-exec 'WriteLine("Hello World");' -u 'static System.Console'
```

> 引用别名

```sh
dotnet-exec 'MyConsole.WriteLine("Hello World");' -u 'MyConsole = System.Console'
```

> 移除某一个命名空间引用

```sh
dotnet-exec 'System.Console.WriteLine("Hello World");' -u '-System'
```

**References**

`dotnet-exec` 会包含默认的框架引用, 包括 `System.Private.CoreLib`/`System.Console`/`System.Text.Json` 等等...

除此之外，你也可以像下面的例子一样添加其他的引用：

**NuGet Package Reference**

> 不指定版本的 nuget 包引用:

```sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "nuget: WeihanLi.Npoi" -u "WeihanLi.Npoi"
```

如果你只指定了 nuget 包，会自动使用最新的稳定版版本，你也可以指定要使用的版本

> 指定某个版本的 nuget 包引用:

```sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "nuget: WeihanLi.Npoi,2.5.0" -u "WeihanLi.Npoi"
```

**local file reference**

引用本地 dll

```sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "./out/WeihanLi.Npoi.dll" -u "WeihanLi.Npoi"
```

**local folder reference**

引用本地某个目录下的所有 dll

```sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "folder: ./out" -u "WeihanLi.Npoi"
```

**SDK framework reference**

添加 Web SDK 框架引用

```sh
dotnet-exec 'WebApplication.Create().Run();' --reference 'framework:web'
```

`web` 是 `Microsoft.AspNetCore.App` 的一个别名, 你也可以使用 `--web` 来添加 ASP.NET Core web 框架引用

**preview option**

默认会使用 `latest` 语言版本，如果需要使用到一些 `preview` 才支持的特性，可以指定 `--preview` 选项，如果有一些功能声明了 `RequiresPreviewFeaturesAttribute` 也可以使用这一选项来尝试

### Profile command

Profile 命令可以用来配置一些自定义的 profile 来简化执行时要指定的选项
