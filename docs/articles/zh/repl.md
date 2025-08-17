# REPL（读取-求值-打印循环）

本指南介绍 dotnet-exec 的 REPL 功能，它提供了交互式 C# 执行环境。

## 交互模式

REPL 提供了一个交互式 C# 执行环境，允许您逐行执行 C# 代码，类似于其他交互式 shell。

### 启动 REPL

当未提供脚本时，REPL 模式会自动启动：

```sh
# 启动 REPL 模式
dotnet-exec

# 带特定引用启动 REPL
dotnet-exec --reference "nuget:Newtonsoft.Json"

# 带 Web 框架启动 REPL
dotnet-exec --web

# 使用配置文件启动 REPL
dotnet-exec --profile myprofile
```

### REPL 命令

进入 REPL 模式后，您可以使用几个特殊命令：

| 命令 | 描述 | 示例 |
|------|------|------|
| `#q` 或 `#exit` | 退出 REPL 会话 | `#q` |
| `#cls` 或 `#clear` | 清除屏幕 | `#cls` |
| `#help` | 显示帮助信息 | `#help` |
| `#r <引用>` | 添加引用 | `#r nuget:CsvHelper` |
| `<表达式>?` | 获取补全 | `Console.?` |
| `<表达式>.` | 显示成员 | `DateTime.` |
| `\`（行尾） | 在下一行继续 | `var x = 1 \` |

### 交互功能

#### 基本代码执行

```csharp
> var message = "Hello, World!";
> Console.WriteLine(message);
Hello, World!

> DateTime.Now
[2024/1/15 10:30:45]

> Math.Sqrt(16)
4
```

#### 多行输入

在行尾使用 `\` 在下一行继续：

```csharp
> var numbers = new[] { 1, 2, 3, 4, 5 } \
* .Where(x => x % 2 == 0) \
* .ToArray();
> numbers
int[2] { 2, 4 }
```

#### 自动补全

表达式以 `?` 或 `.` 结尾来获取补全：

```csharp
> Console.?
WriteLine
Write
ReadLine
ReadKey
...

> DateTime.
Now
Today
UtcNow
DaysInMonth
...
```

#### 动态引用加载

在 REPL 会话期间添加引用：

```csharp
> #r nuget:Newtonsoft.Json
引用已添加

> using Newtonsoft.Json;
> JsonConvert.SerializeObject(new { Name = "John", Age = 30 })
"{"Name":"John","Age":30}"

> #r nuget:CsvHelper,30.0.0
引用已添加

> #r /path/to/local/library.dll
引用已添加
```

### REPL 配置

#### 使用预定义配置启动

```sh
# 带 Web 引用的 REPL
dotnet-exec --web

# 带特定框架的 REPL
dotnet-exec --reference "framework:Microsoft.AspNetCore.App"

# 带多个引用的 REPL
dotnet-exec --reference "nuget:Dapper" --reference "nuget:MySql.Data"
```

#### 使用配置文件

为常见 REPL 场景创建配置文件：

```sh
# 创建 Web 开发 REPL 配置文件
dotnet-exec profile set web-repl \
  --web \
  --reference "nuget:Dapper" \
  --reference "nuget:Microsoft.EntityFrameworkCore.SqlServer" \
  --using "Microsoft.AspNetCore.Mvc" \
  --using "Microsoft.EntityFrameworkCore"

# 使用配置文件启动 REPL
dotnet-exec --profile web-repl
```

### 高级 REPL 用法

#### 状态持久化

在之前语句中定义的变量和类型在后续语句中可用：

```csharp
> class Person { public string Name { get; set; } public int Age { get; set; } }
> var john = new Person { Name = "John", Age = 30 };
> john.Name
"John"

> void PrintPerson(Person p) => Console.WriteLine($"{p.Name} is {p.Age} years old");
> PrintPerson(john);
John is 30 years old
```

#### 异常处理

REPL 优雅地处理编译和运行时错误：

```csharp
> int x = "invalid";
(1,9): error CS0029: Cannot implicitly convert type 'string' to 'int'

> throw new Exception("Test");
System.Exception: Test
   at Submission#1.<<Initialize>>d__0.MoveNext()
```

#### 与外部服务集成

```csharp
> #r nuget:System.Net.Http.Json
引用已添加

> var client = new HttpClient();
> var response = await client.GetFromJsonAsync<dynamic>("https://api.github.com/users/octocat");
> response.name
"The Octocat"
```

REPL 提供了强大的交互式环境，用于快速 C# 开发、测试和探索。有关 dotnet-exec 内部架构的信息，请参阅[架构指南](architecture.md)。

