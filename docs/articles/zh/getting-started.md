# 快速开始

欢迎使用 dotnet-exec！本指南将帮助您开始执行 C# 脚本和代码，而无需创建完整的项目设置。

## 什么是 dotnet-exec？

`dotnet-exec` 是一个命令行工具，允许您在不创建项目文件的情况下执行 C# 程序。它支持：

- **原始 C# 代码执行**：直接从命令行运行代码
- **脚本文件执行**：执行本地或来自 URL 的 .cs 文件
- **自定义入口点**：使用除 `Main` 之外的方法作为入口点
- **REPL 模式**：交互式 C# 执行环境
- **丰富的引用支持**：NuGet 包、本地 DLL、框架引用
- **测试功能**：内置 xUnit 测试执行
- **配置文件**：保存和重用常用配置
- **命令别名**：为常用命令创建快捷方式

## 安装

### 安装为 .NET 工具

安装最新稳定版本：

```sh
dotnet tool install -g dotnet-execute
```

安装最新预览版本：

```sh
dotnet tool install -g dotnet-execute --prerelease
```

更新到最新版本：

```sh
dotnet tool update -g dotnet-execute
```

### 安装故障排除

如果安装失败，请尝试：

```sh
# 明确添加 NuGet 源
dotnet tool install -g dotnet-execute --add-source https://api.nuget.org/v3/index.json

# 清除缓存并重试
dotnet nuget locals all --clear
dotnet tool install -g dotnet-execute
```

### 验证安装

```sh
# 检查版本
dotnet-exec --version

# 显示帮助
dotnet-exec --help
```

### 使用 Docker

如果您没有本地 .NET SDK，也可以通过 Docker 体验：

```sh
# 运行简单表达式
docker run --rm weihanli/dotnet-exec:latest "1+1"

# 执行脚本文件
docker run --rm -v $(pwd):/app weihanli/dotnet-exec:latest /app/script.cs
```

## 快速开始示例

### 简单代码执行

```sh
# 简单表达式
dotnet-exec "Console.WriteLine(\"Hello, World!\");"

# 数学计算
dotnet-exec "Console.WriteLine(Math.Sqrt(16));"

# 日期时间
dotnet-exec "Console.WriteLine(DateTime.Now.ToString(\"yyyy-MM-dd HH:mm:ss\"));"
```

### 脚本文件执行

创建一个简单的脚本文件 `hello.cs`：

```csharp
// hello.cs
using System;

Console.WriteLine("Hello from script!");
Console.WriteLine($"Current time: {DateTime.Now}");

if (args.Length > 0)
{
    Console.WriteLine($"Arguments: {string.Join(", ", args)}");
}
```

执行脚本：

```sh
# 基本执行
dotnet-exec hello.cs

# 带参数执行
dotnet-exec hello.cs arg1 arg2 arg3
```

### 自定义入口点

```csharp
// custom-entry.cs
using System;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("这是 Main 方法");
    }
    
    public static void AlternativeEntry(string[] args)
    {
        Console.WriteLine("这是替代入口点");
    }
    
    public static void Execute()
    {
        Console.WriteLine("这是 Execute 方法");
    }
}
```

```sh
# 使用默认 Main 方法
dotnet-exec custom-entry.cs

# 使用自定义入口点
dotnet-exec custom-entry.cs --entry AlternativeEntry
```

## REPL 模式

REPL（读取-求值-打印循环）模式提供交互式 C# 环境：

```sh
# 启动 REPL
dotnet-exec
```

在 REPL 中，您可以：

- 执行 C# 表达式和语句
- 定义变量和方法
- 动态添加 NuGet 包引用
- 获取代码补全建议

### REPL 示例会话

```csharp
> var name = "World";
> Console.WriteLine($"Hello, {name}!");
Hello, World!

> var numbers = new[] { 1, 2, 3, 4, 5 };
> numbers.Where(x => x % 2 == 0).ToArray()
int[2] { 2, 4 }

> #r nuget:Newtonsoft.Json
引用已添加

> using Newtonsoft.Json;
> JsonConvert.SerializeObject(new { Name = "Test", Value = 42 })
"{"Name":"Test","Value":42}"
```

### REPL 带自定义配置

```sh
# 带 Web 引用的 REPL
dotnet-exec --web

# 带自定义包的 REPL
dotnet-exec --reference "nuget:Dapper" --reference "nuget:Newtonsoft.Json"
```

## 处理引用

### NuGet 包

```sh
# 添加 NuGet 包
dotnet-exec script.cs --reference "nuget:Newtonsoft.Json"

# 指定版本
dotnet-exec script.cs --reference "nuget:Newtonsoft.Json,13.0.3"

# 多个包
dotnet-exec script.cs \
  --reference "nuget:Dapper" \
  --reference "nuget:MySql.Data"
```

### 本地 DLL 文件

```sh
# 引用本地 DLL
dotnet-exec script.cs --reference "./lib/MyLibrary.dll"

# 引用多个 DLL
dotnet-exec script.cs --reference "./lib/*.dll"
```

### 框架引用

```sh
# ASP.NET Core 引用
dotnet-exec script.cs --web

# 等同于
dotnet-exec script.cs --framework Microsoft.AspNetCore.App

# Windows 桌面应用
dotnet-exec script.cs --framework Microsoft.WindowsDesktop.App
```

## 测试支持

dotnet-exec 内置支持 xUnit 测试：

```csharp
// test-example.cs
using Xunit;

public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        var result = Add(2, 3);
        Assert.Equal(5, result);
    }
    
    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 3, 5)]
    [InlineData(-1, 1, 0)]
    public void Add_VariousInputs_ReturnsExpectedResults(int a, int b, int expected)
    {
        var result = Add(a, b);
        Assert.Equal(expected, result);
    }
    
    private static int Add(int a, int b) => a + b;
}
```

```sh
# 运行测试
dotnet-exec test test-example.cs
```

## 配置文件和别名

### 创建配置文件

保存常用的配置组合：

```sh
# 创建 Web 开发配置文件
dotnet-exec config set-profile web-dev \
  --web \
  --reference "nuget:Microsoft.EntityFrameworkCore.SqlServer" \
  --reference "nuget:Serilog.AspNetCore" \
  --using "Microsoft.EntityFrameworkCore" \
  --using "Serilog"

# 使用配置文件
dotnet-exec script.cs --profile web-dev
```

### 创建别名

创建命令快捷方式：

```sh
# 创建 JSON 处理别名
dotnet-exec alias set json \
  --reference "nuget:Newtonsoft.Json" \
  --using "Newtonsoft.Json"

# 使用别名
dotnet-exec json my-json-script.cs
```

## 常用工作流程

### 数据处理脚本

```csharp
// data-processor.cs
#r "nuget:CsvHelper"
using CsvHelper;
using System.Globalization;

if (args.Length < 2)
{
    Console.WriteLine("用法: script.cs <输入文件> <输出文件>");
    return 1;
}

var inputFile = args[0];
var outputFile = args[1];

using var reader = new StringReader(File.ReadAllText(inputFile));
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var records = csv.GetRecords<dynamic>().ToList();

Console.WriteLine($"处理了 {records.Count} 条记录");

// 处理数据逻辑...
var processedData = records.Where(r => /* 某些条件 */ true).ToList();

// 输出结果
File.WriteAllText(outputFile, JsonSerializer.Serialize(processedData));
Console.WriteLine($"结果已保存到 {outputFile}");
```

```sh
dotnet-exec data-processor.cs input.csv output.json \
  --reference "nuget:CsvHelper" \
  --using "CsvHelper" \
  --using "System.Globalization"
```

### API 调用脚本

```csharp
// api-client.cs
if (args.Length < 1)
{
    Console.WriteLine("用法: script.cs <API_URL>");
    return 1;
}

var apiUrl = args[0];
var client = new HttpClient();

try
{
    var response = await client.GetStringAsync(apiUrl);
    Console.WriteLine("API 响应:");
    Console.WriteLine(response);
}
catch (Exception ex)
{
    Console.WriteLine($"错误: {ex.Message}");
    return 1;
}
```

```sh
dotnet-exec api-client.cs "https://api.github.com/users/octocat"
```

### 系统管理脚本

```csharp
// system-info.cs
Console.WriteLine("系统信息:");
Console.WriteLine($"操作系统: {Environment.OSVersion}");
Console.WriteLine($"机器名: {Environment.MachineName}");
Console.WriteLine($"处理器数: {Environment.ProcessorCount}");
Console.WriteLine($"工作目录: {Environment.CurrentDirectory}");

Console.WriteLine("\n环境变量:");
foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
{
    Console.WriteLine($"{env.Key}={env.Value}");
}
```

```sh
dotnet-exec system-info.cs
```

## 远程脚本执行

dotnet-exec 可以直接从 URL 执行脚本：

```sh
# 从 GitHub 执行脚本
dotnet-exec https://raw.githubusercontent.com/WeihanLi/dotnet-exec/main/samples/hello.cs

# 使用短链接
dotnet-exec gh:WeihanLi/dotnet-exec/samples/hello.cs
```

## 下一步

现在您已经了解了基础知识，可以探索更高级的功能：

- [高级使用指南](advanced-usage.md) - 复杂场景和优化
- [引用管理指南](references-guide.md) - 深入了解包和引用管理
- [配置文件和别名](profiles-and-aliases.md) - 工作流程自动化
- [测试指南](testing-guide.md) - 全面的测试支持
- [REPL 和架构](repl-and-architecture.md) - 交互模式和内部架构
- [示例和用例](examples.md) - 50+ 实际示例
- [故障排除](troubleshooting.md) - 常见问题解决方案

开始使用 dotnet-exec 快速执行 C# 代码，无需项目文件的复杂性！

#### Script

script 支持三种：

- script 路径，支持本地的 local path 和远程的 path比如 github 等，比如 `dotnet-exec 'Hello.cs'`/`dotnet-exec 'https://aka.ms/abc/Hello.cs'`
- 原始代码，比如： `dotnet-exec 'Console.WriteLine("Hello dotnet-exec")'`
- 原始 C# Script 代码, 比如： `dotnet-exec 'Guid.NewGuid()'`

`script` 也支持指定多个 script，如：`dotnet-exec A.cs B.cs`

#### REPL

不提供 script 参数的时候，默认会开启一个 REPL(`Read Evaluate Print Loop`)，大体和在 Visual Studio 中的 C# Interative 一样，并且额外支持 nuget reference，可以通过 `#r nuget:CsvHelper` 或者 `#r nuget:WeihanLi.Npoi,3.0.0` 等

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
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "nuget: WeihanLi.Npoi,3.0.0" -u "WeihanLi.Npoi"
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

### Test command

`test` 命令允许你执行 xunit 测试用例。

```sh
dotnet-exec test XxTest.cs
```

此命令集成了 xunit v3 以执行指定的 xunit 测试文件。

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

#### 取消别名

要删除现有别名，请使用 `unset` 子命令，后跟别名：

```sh
dotnet-exec alias unset <aliasName>
```

例如，要删除 `guid` 别名：

```sh
dotnet-exec alias unset guid
```
