# dotnet-exec

Package | Latest | Latest Preview
---- | ---- | ----
dotnet-execute | [![dotnet-execute](https://img.shields.io/nuget/v/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/) | [![dotnet-execute Latest](https://img.shields.io/nuget/vpre/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/absoluteLatest)
ReferenceResolver | [![ReferenceResolver](https://img.shields.io/nuget/v/ReferenceResolver)](https://www.nuget.org/packages/ReferenceResolver/) | [![ReferenceResolver Latest](https://img.shields.io/nuget/vpre/ReferenceResolver)](https://www.nuget.org/packages/ReferenceResolver/absoluteLatest)

[![default](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnet.yml/badge.svg)](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnet.yml)

[![Docker Pulls](https://img.shields.io/docker/pulls/weihanli/dotnet-exec)](https://hub.docker.com/r/weihanli/dotnet-exec)

[![GitHub Commit Activity](https://img.shields.io/github/commit-activity/m/WeihanLi/dotnet-exec)](https://github.com/WeihanLi/dotnet-exec/commits/main)

[![GitHub Release](https://img.shields.io/github/v/release/WeihanLi/dotnet-exec)](https://github.com/WeihanLi/dotnet-exec/releases)

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/WeihanLi/dotnet-exec)

[![Ask OpenDeepWiki](https://img.shields.io/badge/Ask-OpenDeepWiki-blue)](https://opendeep.wiki/WeihanLi/dotnet-exec/)

[For English](./README.md)

## 简介

`dotnet-exec` 是一个强大的命令行工具，允许您在不创建项目文件的情况下执行 C# 程序。它支持自定义入口方法、REPL 交互模式、丰富的引用管理和测试功能。

### 主要特性

- ✨ **无项目执行**：直接运行 C# 脚本，无需 `.csproj` 文件
- 🚀 **灵活的入口点**：支持 `Main` 方法或任何自定义方法作为入口点
- 🔄 **交互式 REPL**：实时 C# 代码执行和实验
- 📦 **智能引用管理**：自动处理 NuGet 包、本地 DLL 和框架引用
- 🧪 **内置测试支持**：集成 xUnit 框架进行单元测试
- ⚙️ **配置文件**：保存和重用常用配置
- 🔧 **命令别名**：创建自定义命令快捷方式
- 🌐 **远程执行**：直接从 GitHub 或任何 URL 执行脚本
- 🐳 **容器就绪**：提供 Docker 镜像支持

## 安装

### .NET 工具安装

```sh
# 安装最新稳定版本
dotnet tool install -g dotnet-execute

# 更新到最新版本
dotnet tool update -g dotnet-execute

# 安装预览版本
dotnet tool install -g dotnet-execute --prerelease
```

### 故障排除

如果安装失败，尝试：

```sh
# 明确指定源
dotnet tool install -g dotnet-execute --add-source https://api.nuget.org/v3/index.json

# 清除缓存后重新安装
dotnet nuget locals all --clear
dotnet tool install -g dotnet-execute
```

### Docker 支持

```sh
# 运行简单表达式
docker run --rm weihanli/dotnet-exec:latest "1+1"

# 运行复杂代码
docker run --rm weihanli/dotnet-exec:latest "Guid.NewGuid()"

# 获取运行时信息
docker run --rm weihanli/dotnet-exec:latest "ApplicationHelper.RuntimeInfo"
```

完整镜像标签列表请参考：<https://hub.docker.com/r/weihanli/dotnet-exec/tags>

## 快速开始

### 基本用法

```sh
# 执行简单表达式
dotnet-exec "Console.WriteLine(\"Hello, World!\");"

# 数学计算
dotnet-exec "Console.WriteLine(Math.PI * 2);"

# 执行本地脚本文件
dotnet-exec script.cs

# 执行远程脚本
dotnet-exec https://raw.githubusercontent.com/user/repo/main/script.cs
```

### 自定义入口点

```csharp
// example.cs
public class Program 
{
    public static void Main() => Console.WriteLine("Main method");
    public static void Test() => Console.WriteLine("Test method");
    public static void Execute() => Console.WriteLine("Execute method");
}
```

```sh
# 使用默认 Main 方法
dotnet-exec example.cs

# 使用自定义入口点
dotnet-exec example.cs --entry Test
```

### REPL 交互模式

```sh
# 启动 REPL
dotnet-exec
```

在 REPL 中：

```csharp
> var name = "dotnet-exec";
> Console.WriteLine($"Hello from {name}!");
Hello from dotnet-exec!
> #r nuget:Newtonsoft.Json
> using Newtonsoft.Json;
> JsonConvert.SerializeObject(new { message = "Hello", timestamp = DateTime.Now })
"{\"message\":\"Hello\",\"timestamp\":\"2025-08-19T11:03:15.095747+08:00\"}"
```

## 引用管理

### NuGet 包引用

```sh
# 基本包引用
dotnet-exec script.cs --reference "nuget:Newtonsoft.Json"

# 指定版本
dotnet-exec script.cs --reference "nuget:Newtonsoft.Json,13.0.3"

# 多个包
dotnet-exec script.cs \
  --reference "nuget:Dapper" \
  --reference "nuget:Microsoft.EntityFrameworkCore"

# 预发布版本
dotnet-exec script.cs --reference "nuget:Package,1.0.0-preview"
```

### 本地引用

```sh
# 本地 DLL
dotnet-exec script.cs --reference "./lib/MyLibrary.dll"

# 文件夹中的所有 DLL
dotnet-exec script.cs --reference "folder:./lib"

# 项目引用
dotnet-exec script.cs --reference "project:../MyProject/MyProject.csproj"
```

### 框架引用

```sh
# ASP.NET Core Web 应用
dotnet-exec script.cs --web

# 等同于
dotnet-exec script.cs --framework Microsoft.AspNetCore.App

# Windows 桌面应用
dotnet-exec script.cs --framework Microsoft.WindowsDesktop.App
```

## 高级功能

### using 管理

```sh
# 添加 using 语句
dotnet-exec script.cs --using "System.Text.Json"

# 静态 using
dotnet-exec "WriteLine(\"Hello!\");" --using "static System.Console"

# 多个 using
dotnet-exec script.cs \
  --using "Microsoft.EntityFrameworkCore" \
  --using "Microsoft.Extensions.DependencyInjection"
```

### 测试支持

```csharp
// test.cs
using Xunit;

public class CalculatorTests
{
    [Fact]
    public void Add_ReturnsCorrectSum()
    {
        Assert.Equal(5, Add(2, 3));
    }
    
    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(5, 5, 10)]
    public void Add_MultipleInputs_ReturnsCorrectSums(int a, int b, int expected)
    {
        Assert.Equal(expected, Add(a, b));
    }
    
    private int Add(int a, int b) => a + b;
}
```

```sh
# 运行测试
dotnet-exec test test.cs
```

### 配置文件

```sh
# 创建配置文件
dotnet-exec config set-profile web-dev \
  --web \
  --reference "nuget:Microsoft.EntityFrameworkCore.SqlServer" \
  --reference "nuget:Serilog.AspNetCore" \
  --using "Microsoft.EntityFrameworkCore"

# 使用配置文件
dotnet-exec script.cs --profile web-dev

# 列出配置文件
dotnet-exec config list-profiles
```

### 命令别名

```sh
# 创建别名
dotnet-exec alias set guid "Guid.NewGuid()"
dotnet-exec alias set now "DateTime.Now"
dotnet-exec alias set sha256 "Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(args[0]))).Dump();"
dotnet-exec alias set base64 "Convert.ToBase64String(Encoding.UTF8.GetBytes(args[0])).Dump();"

# 列出所有别名
dotnet-exec alias ls

# 使用别名
dotnet-exec guid
dotnet-exec now
dotnet-exec sha256 -- "text to hash"
dotnet-exec base64 -- "text to encode"

# 移除别名
dotnet-exec alias unset guid
```

## 实用示例

### 数据处理

```csharp
// csv-processor.cs
#r "nuget:CsvHelper"
using CsvHelper;

var records = new List<dynamic>();
using var reader = new StringReader(File.ReadAllText(args[0]));
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

records.AddRange(csv.GetRecords<dynamic>());
Console.WriteLine($"处理了 {records.Count} 条记录");

// 数据转换和分析...
```

```sh
dotnet-exec csv-processor.cs data.csv --reference "nuget:CsvHelper"
```

### API 调用

```csharp
// api-client.cs
var client = new HttpClient();
var response = await client.GetStringAsync(args[0]);
var data = JsonSerializer.Deserialize<dynamic>(response);
Console.WriteLine(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
```

```sh
dotnet-exec api-client.cs "https://api.github.com/users/octocat"
```

### 系统管理

```csharp
// system-monitor.cs
Console.WriteLine($"系统: {Environment.OSVersion}");
Console.WriteLine($"CPU 核心: {Environment.ProcessorCount}");
Console.WriteLine($"内存使用: {GC.GetTotalMemory(false) / 1024 / 1024} MB");

foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
{
    var freeSpace = drive.TotalFreeSpace / 1024 / 1024 / 1024;
    var totalSpace = drive.TotalSize / 1024 / 1024 / 1024;
    Console.WriteLine($"磁盘 {drive.Name}: {freeSpace}GB 可用 / {totalSpace}GB 总计");
}
```

```sh
dotnet-exec system-monitor.cs
```

### DevOps 自动化

```csharp
// deploy-script.cs
#r "nuget:Docker.DotNet"
using Docker.DotNet;

var client = new DockerClientConfiguration().CreateClient();
var containers = await client.Containers.ListContainersAsync(new ContainersListParameters());

Console.WriteLine("运行中的容器:");
foreach (var container in containers)
{
    Console.WriteLine($"- {container.Names.First()}: {container.Status}");
}
```

```sh
dotnet-exec deploy-script.cs --reference "nuget:Docker.DotNet"
```

## 命令选项

### 核心选项

| 选项 | 简写 | 描述 | 示例 |
|------|------|------|------|
| `--reference` | `-r` | 添加程序集引用 | `-r "nuget:Newtonsoft.Json"` |
| `--using` | `-u` | 添加 using 语句 | `-u "System.Text.Json"` |
| `--entry` | `-e` | 指定入口方法 | `--entry MainTest` |
| `--web` | | 添加 Web 框架引用 | `--web` |
| `--profile` | | 使用配置文件 | `--profile web-dev` |

### 编译选项

| 选项 | 描述 | 示例 |
|------|------|------|
| `--configuration`/`-c` | 编译配置 | `--configuration Release` |
| `--framework` | 目标框架 | `--framework net10.0` |
| `--langversion` | C# 语言版本 | `--langversion 11` |

### 输出选项

| 选项 | 描述 | 示例 |
|------|------|------|
| `--debug` | 详细输出 | `--debug` |
| `--compile-output` | 保存编译结果 | `--compile-output ./output.dll` |
| `--dry-run` | 仅验证不执行 | `--dry-run` |

## 配置管理

### 环境特定配置

```sh
# 开发环境
dotnet-exec config set-profile development \
  --reference "nuget:Microsoft.Extensions.Logging.Debug" \
  --using "Microsoft.Extensions.Logging"

# 生产环境
dotnet-exec config set-profile production \
  --reference "nuget:Microsoft.Extensions.Logging.EventLog" \
  --configuration Release
```

## 集成场景

### CI/CD 流水线

```yaml
# GitHub Actions
- name: 运行构建脚本
  run: dotnet-exec scripts/build.cs --profile ci-build

# Azure DevOps
- script: dotnet-exec deploy/azure-deploy.cs --configuration Release
  displayName: '部署到 Azure'
```

### 开发工作流

```sh
# 代码生成
dotnet-exec codegen/generate-models.cs -- --input schema.json

# 数据库迁移
dotnet-exec migrations/migrate.cs -- --connection-string "$DB_CONN"

# 性能测试
dotnet-exec perf/benchmark.cs -- --iterations 1000
```

## 文档

📚 **完整文档**: [docs/articles/zh/](docs/articles/zh/)

- [快速开始](docs/articles/zh/getting-started.md) - 基础使用指南
- [高级使用指南](docs/articles/zh/advanced-usage.md) - 复杂场景和优化
- [引用管理指南](docs/articles/zh/references-guide.md) - 包和引用管理
- [配置文件和别名](docs/articles/zh/profiles-and-aliases.md) - 工作流自动化
- [测试指南](docs/articles/zh/testing-guide.md) - 测试最佳实践
- [REPL 和架构](docs/articles/zh/repl-and-architecture.md) - 交互模式和架构
- [示例和用例](docs/articles/zh/examples.md) - 50+ 实际示例
- [故障排除](docs/articles/zh/troubleshooting.md) - 问题解决方案

## 社区和支持

- 🐛 **问题反馈**: [GitHub Issues](https://github.com/WeihanLi/dotnet-exec/issues)
- 💬 **讨论**: [GitHub Issues](https://github.com/WeihanLi/dotnet-exec/issues)
- 📖 **Wiki**: [DeepWiki](https://deepwiki.com/WeihanLi/dotnet-exec)
- 🔄 **更新日志**: [Release Notes](docs/ReleaseNotes.md)

## 为什么选择 dotnet-exec？

✅ **快速原型制作** - 无需项目设置即可测试想法  
✅ **脚本自动化** - 强大的 DevOps 和管理脚本支持  
✅ **学习和实验** - 交互式 REPL 环境  
✅ **CI/CD 集成** - 无缝集成到构建流水线  
✅ **企业就绪** - 配置文件和团队协作功能  
✅ **跨平台** - Windows、Linux、macOS 和容器支持  

立即开始使用 dotnet-exec，体验 C# 脚本执行的强大和灵活性！

## 许可证

本项目采用 [Apache License 2.0](LICENSE) 许可证。

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

### Alias command

`alias` 命令允许你管理常用命令的别名。

#### 列出别名

要列出所有配置的别名，请使用 `list` 子命令：

```sh
dotnet-exec alias list
```

你也可以使用 `dotnet-exec alias ls` 来列出别名。

#### 设置别名

要设置新别名，请使用 `set` 子命令，后跟别名和值：

```sh
dotnet-exec alias set <aliasName> <aliasValue>
```

例如，要设置生成新 GUID 的别名：

```sh
dotnet-exec alias set guid "Guid.NewGuid()"
```

使用示例：

```sh
dotnet-exec guid
```

#### 取消别名

要删除现有别名，请使用 `unset` 子命令，后跟别名：

```sh
dotnet-exec alias unset <aliasName>
```

例如，要删除 `guid` 别名：

```sh
dotnet-exec alias unset guid
```

## Acknowledgements

- [Roslyn](https://github.com/dotnet/roslyn)
- [NuGet.Clients](https://github.com/NuGet/NuGet.Client)
- [System.CommandLine](https://github.com/dotnet/command-line-api)
- [感谢 JetBrains 提供的 Rider 开源 license](https://jb.gg/OpenSource?from=dotnet-exec)
- 感谢这个项目的贡献者和使用者
