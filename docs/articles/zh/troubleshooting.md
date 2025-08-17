# 故障排除

本指南帮助您诊断和解决使用 dotnet-exec 时可能遇到的常见问题。

## 安装和路径问题

### dotnet tool 未找到

**问题**：执行 `dotnet-exec` 时收到 "command not found" 错误

**解决方案**：

1. **验证安装**：
```sh
# 检查工具是否已安装
dotnet tool list -g

# 重新安装
dotnet tool install -g dotnet-execute

# 或更新到最新版本
dotnet tool update -g dotnet-execute
```

2. **检查 PATH 环境变量**：
```sh
# Windows
echo $env:PATH | findstr ".dotnet"

# Linux/macOS
echo $PATH | grep ".dotnet"
```

3. **手动添加到 PATH**：
```sh
# Windows (PowerShell)
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"

# Linux/macOS (bash)
export PATH="$PATH:$HOME/.dotnet/tools"
```

4. **重启终端或重新加载配置文件**

### 权限问题

**问题**：安装时出现权限错误

**解决方案**：

```sh
# 确保有足够权限安装全局工具
sudo dotnet tool install -g dotnet-execute  # Linux/macOS

# 或安装到用户目录
dotnet tool install --tool-path ~/.local/bin dotnet-execute
```

### .NET SDK 版本不兼容

**问题**：dotnet-exec 要求特定的 .NET SDK 版本

**解决方案**：

```sh
# 检查当前 .NET 版本
dotnet --version

# 列出已安装的版本
dotnet --list-sdks

# 安装所需版本
# 从 https://dotnet.microsoft.com/download 下载

# 使用特定版本运行
dotnet --fx-version 8.0.0 tool run dotnet-execute
```

## 编译错误

### 语法错误

**问题**：脚本包含 C# 语法错误

**示例错误**：
```
(3,15): error CS1002: ; expected
```

**解决方案**：

1. **启用详细错误信息**：
```sh
dotnet-exec script.cs --verbose
```

2. **检查常见语法问题**：
```csharp
// 错误：缺少分号
var x = 5

// 正确
var x = 5;

// 错误：大括号不匹配
if (condition)
{
    DoSomething();

// 正确
if (condition)
{
    DoSomething();
}
```

3. **使用 IDE 验证语法**：
将脚本代码复制到 Visual Studio 或 VS Code 中检查语法

### 命名空间和 using 语句

**问题**：类型或命名空间找不到

**示例错误**：
```
error CS0246: The type or namespace name 'JsonSerializer' could not be found
```

**解决方案**：

1. **添加必要的 using 语句**：
```sh
dotnet-exec script.cs --using "System.Text.Json"
```

2. **使用完全限定名称**：
```csharp
// 而不是
JsonSerializer.Serialize(data);

// 使用
System.Text.Json.JsonSerializer.Serialize(data);
```

3. **检查引用的包**：
```sh
dotnet-exec script.cs \
  --reference "nuget:System.Text.Json" \
  --using "System.Text.Json"
```

### 语言版本不兼容

**问题**：使用了当前语言版本不支持的功能

**示例错误**：
```
error CS8107: Feature 'top-level programs' is not available in C# 7.3
```

**解决方案**：

```sh
# 指定更高的语言版本
dotnet-exec script.cs --langversion latest

# 或指定特定版本
dotnet-exec script.cs --langversion 11
```

## 引用解析问题

### NuGet 包未找到

**问题**：无法解析 NuGet 包引用

**示例错误**：
```
error: Package 'NonExistentPackage' not found
```

**解决方案**：

1. **验证包名和版本**：
```sh
# 搜索包
dotnet package search PackageName

# 验证包是否存在
dotnet-exec script.cs --reference "nuget:Newtonsoft.Json" --dry-run
```

2. **清除 NuGet 缓存**：
```sh
dotnet nuget locals all --clear
```

3. **指定包源**：
```sh
dotnet-exec script.cs \
  --reference "nuget:PackageName" \
  --nuget-source https://api.nuget.org/v3/index.json
```

4. **检查网络连接**：
```sh
# 测试网络连接
curl -I https://api.nuget.org/v3/index.json

# 使用代理
HTTP_PROXY=http://proxy.company.com:8080 dotnet-exec script.cs
```

### 版本冲突

**问题**：多个包引用了同一依赖的不同版本

**示例错误**：
```
error: Version conflict for package 'System.Text.Json'
```

**解决方案**：

1. **显示依赖冲突**：
```sh
dotnet-exec script.cs \
  --reference "nuget:PackageA" \
  --reference "nuget:PackageB" \
  --show-dependency-conflicts
```

2. **强制指定版本**：
```sh
dotnet-exec script.cs \
  --reference "nuget:PackageA" \
  --reference "nuget:PackageB" \
  --force-version "System.Text.Json,7.0.0"
```

3. **使用兼容版本**：
```sh
dotnet-exec script.cs \
  --reference "nuget:PackageA,1.0.0" \
  --reference "nuget:PackageB,2.0.0"
```

### 本地引用问题

**问题**：无法找到本地 DLL 或项目文件

**解决方案**：

1. **使用绝对路径**：
```sh
dotnet-exec script.cs --reference "/absolute/path/to/library.dll"
```

2. **验证文件存在**：
```sh
# 检查文件是否存在
ls -la /path/to/library.dll

# 使用相对路径
dotnet-exec script.cs --reference "./lib/library.dll"
```

3. **项目引用调试**：
```sh
# 验证项目文件
dotnet build /path/to/project.csproj

# 使用项目引用
dotnet-exec script.cs --reference project:/path/to/project.csproj
```

## 运行时错误

### 内存不足

**问题**：脚本执行时出现 OutOfMemoryException

**解决方案**：

1. **增加内存限制**：
```sh
# 设置环境变量
export DOTNET_GCHeapHardLimit=1073741824  # 1GB

# 或在脚本中
dotnet-exec script.cs --max-memory 2GB
```

2. **优化内存使用**：
```csharp
// 使用 using 语句确保资源释放
using var fileStream = File.OpenRead("largefile.dat");

// 分批处理大数据
const int batchSize = 1000;
for (int i = 0; i < totalItems; i += batchSize)
{
    var batch = items.Skip(i).Take(batchSize);
    ProcessBatch(batch);
    
    // 强制垃圾回收
    GC.Collect();
    GC.WaitForPendingFinalizers();
}
```

3. **使用流处理**：
```csharp
// 而不是一次加载整个文件
var content = File.ReadAllText("hugefile.txt");

// 使用流
using var reader = new StreamReader("hugefile.txt");
while (!reader.EndOfStream)
{
    var line = await reader.ReadLineAsync();
    ProcessLine(line);
}
```

### 文件访问权限

**问题**：无法访问文件或目录

**解决方案**：

1. **检查文件权限**：
```sh
# Linux/macOS
ls -la /path/to/file

# Windows
icacls "C:\path\to\file"
```

2. **以管理员权限运行**：
```sh
# Windows
Start-Process -Verb RunAs dotnet-exec script.cs

# Linux/macOS
sudo dotnet-exec script.cs
```

3. **使用异常处理**：
```csharp
try
{
    var content = File.ReadAllText(filePath);
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"访问被拒绝: {ex.Message}");
    Console.WriteLine("请检查文件权限或以管理员身份运行");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"文件未找到: {ex.Message}");
}
```

### 网络连接问题

**问题**：脚本无法连接到远程服务

**解决方案**：

1. **检查网络连接**：
```csharp
// 添加超时和重试机制
var client = new HttpClient();
client.Timeout = TimeSpan.FromSeconds(30);

for (int retry = 0; retry < 3; retry++)
{
    try
    {
        var response = await client.GetAsync(url);
        break;
    }
    catch (HttpRequestException ex) when (retry < 2)
    {
        Console.WriteLine($"重试 {retry + 1}/3: {ex.Message}");
        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry)));
    }
}
```

2. **配置代理**：
```csharp
var handler = new HttpClientHandler()
{
    Proxy = new WebProxy("http://proxy.company.com:8080"),
    UseProxy = true
};

var client = new HttpClient(handler);
```

3. **SSL/TLS 问题**：
```csharp
// 仅用于开发环境 - 忽略 SSL 证书错误
ServicePointManager.ServerCertificateValidationCallback = 
    (sender, certificate, chain, sslPolicyErrors) => true;
```

## 性能问题

### 编译速度慢

**问题**：脚本编译时间过长

**解决方案**：

1. **启用编译缓存**：
```sh
# 默认启用，确保未禁用
dotnet-exec script.cs  # 缓存已启用

# 清除并重建缓存
dotnet-exec script.cs --clear-cache
```

2. **减少引用数量**：
```sh
# 只引用必要的包
dotnet-exec script.cs --reference "nuget:SpecificPackage"

# 而不是
dotnet-exec script.cs --web  # 引用很多包
```

3. **使用预编译配置文件**：
```sh
# 创建轻量级配置文件
dotnet-exec config set-profile minimal \
  --reference "nuget:System.Text.Json"

dotnet-exec script.cs --profile minimal
```

### 执行速度慢

**问题**：脚本运行时性能差

**解决方案**：

1. **使用 Release 配置**：
```sh
dotnet-exec script.cs --configuration Release
```

2. **性能分析**：
```csharp
// 使用 Stopwatch 测量性能
var stopwatch = Stopwatch.StartNew();
PerformOperation();
stopwatch.Stop();
Console.WriteLine($"操作耗时: {stopwatch.ElapsedMilliseconds}ms");
```

3. **异步操作优化**：
```csharp
// 并行处理
var tasks = urls.Select(async url => await ProcessUrlAsync(url));
var results = await Task.WhenAll(tasks);

// 使用 Parallel.ForEach 处理 CPU 密集型操作
Parallel.ForEach(items, item => ProcessItem(item));
```

## 平台特定问题

### Windows 特定问题

1. **PowerShell 执行策略**：
```powershell
# 检查执行策略
Get-ExecutionPolicy

# 设置执行策略
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

2. **长路径支持**：
```sh
# 启用长路径支持
dotnet-exec script.cs --enable-long-paths
```

### Linux/macOS 特定问题

1. **权限问题**：
```sh
# 设置执行权限
chmod +x script.cs

# 使用 sudo
sudo dotnet-exec script.cs
```

2. **环境变量**：
```sh
# 设置环境变量
export DOTNET_ROOT=/usr/local/dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
```

## 调试技巧

### 启用详细日志

```sh
# 详细输出
dotnet-exec script.cs --verbose

# 调试级别日志
dotnet-exec script.cs --log-level Debug

# 输出到文件
dotnet-exec script.cs --verbose 2>&1 | tee execution.log
```

### 检查编译输出

```sh
# 保存编译的程序集
dotnet-exec script.cs --compile-output ./output/compiled.dll

# 保留临时文件
dotnet-exec script.cs --keep-temp-files
```

### 性能诊断

```sh
# 显示时间信息
dotnet-exec script.cs --timing

# 内存使用分析
dotnet-exec script.cs --memory-profiling

# 编译性能分析
dotnet-exec script.cs --compilation-timing
```

### 依赖分析

```sh
# 显示所有引用
dotnet-exec script.cs --show-references

# 依赖树
dotnet-exec script.cs --show-dependency-tree

# 冲突检测
dotnet-exec script.cs --show-conflicts
```

## 常见错误代码

| 错误代码 | 含义 | 解决方案 |
|---------|------|----------|
| 1 | 一般错误 | 检查脚本语法和参数 |
| 2 | 编译错误 | 检查 C# 语法和引用 |
| 3 | 运行时错误 | 检查脚本逻辑和异常处理 |
| 4 | 引用解析错误 | 检查包名、版本和网络连接 |
| 5 | 文件访问错误 | 检查文件路径和权限 |

## 获取帮助

1. **内置帮助**：
```sh
dotnet-exec --help
dotnet-exec config --help
dotnet-exec alias --help
```

2. **社区资源**：
- GitHub Issues: https://github.com/WeihanLi/dotnet-exec/issues
- 文档: https://github.com/WeihanLi/dotnet-exec/tree/main/docs

3. **诊断信息收集**：
```sh
# 收集诊断信息
dotnet-exec --version
dotnet --info
dotnet tool list -g
echo $PATH
```

通过遵循本故障排除指南，您应该能够解决使用 dotnet-exec 时遇到的大多数常见问题。