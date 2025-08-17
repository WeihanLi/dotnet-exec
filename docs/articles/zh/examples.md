# 示例和用例

本指南提供了使用 dotnet-exec 的实际示例，涵盖不同领域和用例的 50 多个实用示例。

## 字符串处理和文件操作工具

### 文本文件处理

```csharp
// text-processor.cs - 批量文本文件处理
using System.Text.RegularExpressions;

if (args.Length < 2)
{
    Console.WriteLine("用法: dotnet-exec text-processor.cs <目录> <模式>");
    return 1;
}

var directory = args[0];
var pattern = args[1];
var regex = new Regex(pattern, RegexOptions.IgnoreCase);

var files = Directory.GetFiles(directory, "*.txt", SearchOption.AllDirectories);
Console.WriteLine($"在 {files.Length} 个文件中搜索模式 '{pattern}':");

foreach (var file in files)
{
    var content = File.ReadAllText(file);
    var matches = regex.Matches(content);
    
    if (matches.Count > 0)
    {
        Console.WriteLine($"\n{file}: 找到 {matches.Count} 个匹配");
        foreach (Match match in matches.Take(3))
        {
            var lineNumber = content.Substring(0, match.Index).Count(c => c == '\n') + 1;
            Console.WriteLine($"  第 {lineNumber} 行: {match.Value}");
        }
    }
}
```

```sh
# 运行文本处理器
dotnet-exec text-processor.cs ./documents "error|warning"
```

### CSV 数据转换

```csharp
// csv-converter.cs - CSV 到 JSON 转换器
#r "nuget:CsvHelper"
#r "nuget:System.Text.Json"

using CsvHelper;
using System.Globalization;
using System.Text.Json;

if (args.Length < 2)
{
    Console.WriteLine("用法: dotnet-exec csv-converter.cs <输入.csv> <输出.json>");
    return 1;
}

var inputFile = args[0];
var outputFile = args[1];

using var reader = new StringReader(File.ReadAllText(inputFile));
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

var records = csv.GetRecords<dynamic>().ToList();
var json = JsonSerializer.Serialize(records, new JsonSerializerOptions 
{ 
    WriteIndented = true 
});

File.WriteAllText(outputFile, json);
Console.WriteLine($"转换完成: {records.Count} 条记录从 {inputFile} 转换到 {outputFile}");
```

```sh
# 运行 CSV 转换器
dotnet-exec csv-converter.cs data.csv output.json \
  --reference "nuget:CsvHelper" \
  --reference "nuget:System.Text.Json"
```

### 日志分析器

```csharp
// log-analyzer.cs - 日志文件分析
using System.Text.RegularExpressions;

if (args.Length < 1)
{
    Console.WriteLine("用法: dotnet-exec log-analyzer.cs <日志文件>");
    return 1;
}

var logFile = args[0];
var lines = File.ReadAllLines(logFile);

var errorPattern = new Regex(@"ERROR|FATAL", RegexOptions.IgnoreCase);
var warningPattern = new Regex(@"WARN", RegexOptions.IgnoreCase);
var timestampPattern = new Regex(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}");

var errors = new List<string>();
var warnings = new List<string>();
var hourlyStats = new Dictionary<int, int>();

foreach (var line in lines)
{
    // 提取时间戳
    var timestampMatch = timestampPattern.Match(line);
    if (timestampMatch.Success && DateTime.TryParse(timestampMatch.Value, out var timestamp))
    {
        hourlyStats[timestamp.Hour] = hourlyStats.GetValueOrDefault(timestamp.Hour, 0) + 1;
    }
    
    // 分类错误和警告
    if (errorPattern.IsMatch(line))
        errors.Add(line);
    else if (warningPattern.IsMatch(line))
        warnings.Add(line);
}

Console.WriteLine($"日志分析结果：");
Console.WriteLine($"总行数: {lines.Length}");
Console.WriteLine($"错误: {errors.Count}");
Console.WriteLine($"警告: {warnings.Count}");

Console.WriteLine("\n按小时统计:");
foreach (var kvp in hourlyStats.OrderBy(x => x.Key))
{
    Console.WriteLine($"{kvp.Key:D2}:00 - {kvp.Value} 条记录");
}

if (errors.Any())
{
    Console.WriteLine("\n最近的错误:");
    foreach (var error in errors.TakeLast(5))
    {
        Console.WriteLine($"  {error}");
    }
}
```

## Web 开发和 API 测试

### HTTP API 测试器

```csharp
// api-tester.cs - REST API 测试工具
#r "nuget:System.Net.Http.Json"

using System.Net.Http.Json;
using System.Text.Json;

if (args.Length < 1)
{
    Console.WriteLine("用法: dotnet-exec api-tester.cs <API基础URL>");
    return 1;
}

var baseUrl = args[0];
var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

// 定义测试用例
var testCases = new[]
{
    new { Method = "GET", Path = "/api/health", ExpectedStatus = 200 },
    new { Method = "GET", Path = "/api/users", ExpectedStatus = 200 },
    new { Method = "GET", Path = "/api/users/1", ExpectedStatus = 200 },
    new { Method = "GET", Path = "/api/nonexistent", ExpectedStatus = 404 }
};

Console.WriteLine($"正在测试 API: {baseUrl}");
Console.WriteLine(new string('-', 50));

var results = new List<object>();

foreach (var test in testCases)
{
    try
    {
        var response = test.Method switch
        {
            "GET" => await client.GetAsync(test.Path),
            "POST" => await client.PostAsync(test.Path, null),
            _ => throw new NotSupportedException($"方法 {test.Method} 不支持")
        };
        
        var success = (int)response.StatusCode == test.ExpectedStatus;
        var result = new
        {
            test.Method,
            test.Path,
            Expected = test.ExpectedStatus,
            Actual = (int)response.StatusCode,
            Success = success,
            ResponseTime = DateTimeOffset.Now // 简化版本
        };
        
        results.Add(result);
        
        var status = success ? "✅ 通过" : "❌ 失败";
        Console.WriteLine($"{status} {test.Method} {test.Path} - 期望: {test.ExpectedStatus}, 实际: {(int)response.StatusCode}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ 错误 {test.Method} {test.Path} - {ex.Message}");
        results.Add(new { test.Method, test.Path, Error = ex.Message });
    }
}

var passedTests = results.Count(r => r.GetType().GetProperty("Success")?.GetValue(r) as bool? == true);
Console.WriteLine(new string('-', 50));
Console.WriteLine($"测试结果: {passedTests}/{testCases.Length} 通过");
```

### 网站可用性检查器

```csharp
// site-checker.cs - 网站可用性监控
using System.Diagnostics;

var websites = new[]
{
    "https://github.com",
    "https://stackoverflow.com",
    "https://docs.microsoft.com",
    "https://nuget.org"
};

var client = new HttpClient();
var results = new List<object>();

Console.WriteLine("网站可用性检查:");
Console.WriteLine(new string('-', 60));

foreach (var site in websites)
{
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        var response = await client.GetAsync(site);
        stopwatch.Stop();
        
        var result = new
        {
            Site = site,
            StatusCode = (int)response.StatusCode,
            ResponseTime = stopwatch.ElapsedMilliseconds,
            IsHealthy = response.IsSuccessStatusCode
        };
        
        results.Add(result);
        
        var status = response.IsSuccessStatusCode ? "🟢" : "🔴";
        Console.WriteLine($"{status} {site} - {response.StatusCode} ({stopwatch.ElapsedMilliseconds}ms)");
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        Console.WriteLine($"🔴 {site} - 错误: {ex.Message}");
        results.Add(new { Site = site, Error = ex.Message, IsHealthy = false });
    }
}

Console.WriteLine(new string('-', 60));
var healthySites = results.Count(r => r.GetType().GetProperty("IsHealthy")?.GetValue(r) as bool? == true);
Console.WriteLine($"健康网站: {healthySites}/{websites.Length}");

var avgResponseTime = results
    .Where(r => r.GetType().GetProperty("ResponseTime")?.GetValue(r) is long)
    .Average(r => (long)r.GetType().GetProperty("ResponseTime").GetValue(r));

if (!double.IsNaN(avgResponseTime))
    Console.WriteLine($"平均响应时间: {avgResponseTime:F0}ms");
```

## 数据库操作和数据处理

### 数据库连接测试器

```csharp
// db-tester.cs - 数据库连接测试
#r "nuget:Microsoft.Data.SqlClient"
#r "nuget:MySqlConnector"
#r "nuget:Npgsql"

using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;

var connectionStrings = new Dictionary<string, string>
{
    ["SqlServer"] = "Server=localhost;Database=TestDB;Integrated Security=true;",
    ["MySQL"] = "Server=localhost;Database=testdb;Uid=root;Pwd=password;",
    ["PostgreSQL"] = "Host=localhost;Database=testdb;Username=postgres;Password=password"
};

Console.WriteLine("数据库连接测试:");
Console.WriteLine(new string('-', 50));

foreach (var kvp in connectionStrings)
{
    var dbType = kvp.Key;
    var connectionString = kvp.Value;
    
    try
    {
        var success = dbType switch
        {
            "SqlServer" => await TestSqlServerConnection(connectionString),
            "MySQL" => await TestMySqlConnection(connectionString),
            "PostgreSQL" => await TestPostgreSqlConnection(connectionString),
            _ => false
        };
        
        var status = success ? "🟢 连接成功" : "🔴 连接失败";
        Console.WriteLine($"{status} {dbType}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"🔴 {dbType} - 错误: {ex.Message}");
    }
}

static async Task<bool> TestSqlServerConnection(string connectionString)
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    return connection.State == System.Data.ConnectionState.Open;
}

static async Task<bool> TestMySqlConnection(string connectionString)
{
    using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();
    return connection.State == System.Data.ConnectionState.Open;
}

static async Task<bool> TestPostgreSqlConnection(string connectionString)
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    return connection.State == System.Data.ConnectionState.Open;
}
```

### 数据迁移脚本

```csharp
// data-migrator.cs - 数据迁移工具
#r "nuget:Dapper"
#r "nuget:Microsoft.Data.SqlClient"

using Dapper;
using Microsoft.Data.SqlClient;

if (args.Length < 3)
{
    Console.WriteLine("用法: dotnet-exec data-migrator.cs <源连接字符串> <目标连接字符串> <表名>");
    return 1;
}

var sourceConnectionString = args[0];
var targetConnectionString = args[1];
var tableName = args[2];

using var sourceConnection = new SqlConnection(sourceConnectionString);
using var targetConnection = new SqlConnection(targetConnectionString);

await sourceConnection.OpenAsync();
await targetConnection.OpenAsync();

Console.WriteLine($"开始迁移表: {tableName}");

// 获取源表结构
var columns = await sourceConnection.QueryAsync<string>(
    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName",
    new { tableName });

var columnList = string.Join(", ", columns);

// 获取总记录数
var totalRecords = await sourceConnection.QuerySingleAsync<int>(
    $"SELECT COUNT(*) FROM {tableName}");

Console.WriteLine($"总记录数: {totalRecords}");

// 分批迁移数据
var batchSize = 1000;
var migratedCount = 0;

for (var offset = 0; offset < totalRecords; offset += batchSize)
{
    var data = await sourceConnection.QueryAsync(
        $"SELECT {columnList} FROM {tableName} ORDER BY (SELECT NULL) OFFSET {offset} ROWS FETCH NEXT {batchSize} ROWS ONLY");
    
    if (data.Any())
    {
        // 这里简化了插入逻辑，实际应用中需要处理列类型匹配
        var insertSql = GenerateInsertSql(tableName, columns.ToList());
        
        foreach (var row in data)
        {
            await targetConnection.ExecuteAsync(insertSql, row);
            migratedCount++;
        }
        
        Console.WriteLine($"已迁移: {migratedCount}/{totalRecords} ({(double)migratedCount/totalRecords:P})");
    }
}

Console.WriteLine($"迁移完成: {migratedCount} 条记录");

static string GenerateInsertSql(string tableName, List<string> columns)
{
    var columnList = string.Join(", ", columns);
    var parameterList = string.Join(", ", columns.Select(c => $"@{c}"));
    return $"INSERT INTO {tableName} ({columnList}) VALUES ({parameterList})";
}
```

## DevOps 自动化和系统监控

### 系统信息收集器

```csharp
// system-info.cs - 系统信息收集
using System.Diagnostics;
using System.Management;

Console.WriteLine("系统信息报告");
Console.WriteLine(new string('=', 50));

// 基本系统信息
Console.WriteLine($"操作系统: {Environment.OSVersion}");
Console.WriteLine($"机器名: {Environment.MachineName}");
Console.WriteLine($"用户名: {Environment.UserName}");
Console.WriteLine($"处理器数: {Environment.ProcessorCount}");
Console.WriteLine($"工作目录: {Environment.CurrentDirectory}");
Console.WriteLine($".NET 版本: {Environment.Version}");

// 内存信息
var totalMemory = GC.GetTotalMemory(false);
Console.WriteLine($"当前内存使用: {totalMemory / 1024 / 1024:F2} MB");

// 磁盘空间
Console.WriteLine("\n磁盘空间:");
Console.WriteLine(new string('-', 30));

foreach (var drive in DriveInfo.GetDrives())
{
    if (drive.IsReady)
    {
        var totalSize = drive.TotalSize / 1024 / 1024 / 1024;
        var freeSpace = drive.TotalFreeSpace / 1024 / 1024 / 1024;
        var usedSpace = totalSize - freeSpace;
        var usagePercent = (double)usedSpace / totalSize * 100;
        
        Console.WriteLine($"{drive.Name} {usedSpace}GB/{totalSize}GB ({usagePercent:F1}% 已使用)");
    }
}

// 网络接口
Console.WriteLine("\n网络接口:");
Console.WriteLine(new string('-', 30));

foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
{
    if (ni.OperationalStatus == OperationalStatus.Up)
    {
        Console.WriteLine($"{ni.Name}: {ni.NetworkInterfaceType}");
        
        foreach (var addr in ni.GetIPProperties().UnicastAddresses)
        {
            if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                Console.WriteLine($"  IP: {addr.Address}");
            }
        }
    }
}

// 进程信息
Console.WriteLine("\n运行中的关键进程:");
Console.WriteLine(new string('-', 30));

var processes = Process.GetProcesses()
    .Where(p => !string.IsNullOrEmpty(p.ProcessName))
    .OrderByDescending(p => p.WorkingSet64)
    .Take(10);

foreach (var process in processes)
{
    try
    {
        var memoryMB = process.WorkingSet64 / 1024 / 1024;
        Console.WriteLine($"{process.ProcessName}: {memoryMB} MB");
    }
    catch
    {
        // 某些进程可能无法访问
    }
}
```

### Docker 容器管理器

```csharp
// docker-manager.cs - Docker 容器管理
using System.Diagnostics;
using System.Text.Json;

if (args.Length < 1)
{
    Console.WriteLine("用法: dotnet-exec docker-manager.cs <命令>");
    Console.WriteLine("命令: list, status, cleanup, stats");
    return 1;
}

var command = args[0].ToLowerInvariant();

switch (command)
{
    case "list":
        await ListContainers();
        break;
    case "status":
        await ShowContainerStatus();
        break;
    case "cleanup":
        await CleanupContainers();
        break;
    case "stats":
        await ShowContainerStats();
        break;
    default:
        Console.WriteLine($"未知命令: {command}");
        break;
}

static async Task ListContainers()
{
    var result = await RunDockerCommand("ps -a --format \"table {{.Names}}\\t{{.Status}}\\t{{.Image}}\"");
    Console.WriteLine("Docker 容器列表:");
    Console.WriteLine(result);
}

static async Task ShowContainerStatus()
{
    var result = await RunDockerCommand("ps --format \"{{.Names}},{{.Status}},{{.Ports}}\"");
    var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    
    Console.WriteLine("容器状态:");
    Console.WriteLine(new string('-', 60));
    
    foreach (var line in lines)
    {
        var parts = line.Split(',');
        if (parts.Length >= 2)
        {
            var status = parts[1].Contains("Up") ? "🟢 运行中" : "🔴 已停止";
            Console.WriteLine($"{status} {parts[0]}");
            if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2]))
            {
                Console.WriteLine($"    端口: {parts[2]}");
            }
        }
    }
}

static async Task CleanupContainers()
{
    Console.WriteLine("清理未使用的 Docker 资源...");
    
    // 删除停止的容器
    await RunDockerCommand("container prune -f");
    Console.WriteLine("✅ 已清理停止的容器");
    
    // 删除未使用的镜像
    await RunDockerCommand("image prune -f");
    Console.WriteLine("✅ 已清理未使用的镜像");
    
    // 删除未使用的网络
    await RunDockerCommand("network prune -f");
    Console.WriteLine("✅ 已清理未使用的网络");
    
    // 删除未使用的卷
    await RunDockerCommand("volume prune -f");
    Console.WriteLine("✅ 已清理未使用的卷");
}

static async Task ShowContainerStats()
{
    var result = await RunDockerCommand("stats --no-stream --format \"{{.Name}},{{.CPUPerc}},{{.MemUsage}}\"");
    var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    
    Console.WriteLine("容器资源使用情况:");
    Console.WriteLine(new string('-', 50));
    Console.WriteLine("名称\t\tCPU\t内存使用");
    Console.WriteLine(new string('-', 50));
    
    foreach (var line in lines)
    {
        var parts = line.Split(',');
        if (parts.Length >= 3)
        {
            Console.WriteLine($"{parts[0]}\t{parts[1]}\t{parts[2]}");
        }
    }
}

static async Task<string> RunDockerCommand(string arguments)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "docker",
        Arguments = arguments,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    
    using var process = Process.Start(startInfo);
    var output = await process.StandardOutput.ReadToEndAsync();
    var error = await process.StandardError.ReadToEndAsync();
    
    await process.WaitForExitAsync();
    
    if (process.ExitCode != 0)
    {
        throw new Exception($"Docker 命令失败: {error}");
    }
    
    return output;
}
```

## 机器学习和数据科学

### 数据分析器

```csharp
// data-analyzer.cs - 简单数据分析
#r "nuget:CsvHelper"

using CsvHelper;
using System.Globalization;

if (args.Length < 1)
{
    Console.WriteLine("用法: dotnet-exec data-analyzer.cs <CSV文件>");
    return 1;
}

var csvFile = args[0];
using var reader = new StringReader(File.ReadAllText(csvFile));
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

var records = csv.GetRecords<dynamic>().ToList();

if (!records.Any())
{
    Console.WriteLine("CSV 文件为空");
    return 1;
}

Console.WriteLine($"数据分析报告: {csvFile}");
Console.WriteLine(new string('=', 50));
Console.WriteLine($"总记录数: {records.Count}");

// 获取列名
var firstRecord = records.First() as IDictionary<string, object>;
var columns = firstRecord.Keys.ToList();

Console.WriteLine($"列数: {columns.Count}");
Console.WriteLine($"列名: {string.Join(", ", columns)}");

Console.WriteLine("\n列统计:");
Console.WriteLine(new string('-', 30));

foreach (var column in columns)
{
    var values = records.Select(r => ((IDictionary<string, object>)r)[column]?.ToString()).ToList();
    var nonEmptyValues = values.Where(v => !string.IsNullOrEmpty(v)).ToList();
    
    Console.WriteLine($"\n{column}:");
    Console.WriteLine($"  非空值: {nonEmptyValues.Count}/{records.Count}");
    Console.WriteLine($"  唯一值: {nonEmptyValues.Distinct().Count()}");
    
    // 尝试解析为数字进行统计分析
    var numericValues = new List<double>();
    foreach (var value in nonEmptyValues)
    {
        if (double.TryParse(value, out var number))
        {
            numericValues.Add(number);
        }
    }
    
    if (numericValues.Any())
    {
        Console.WriteLine($"  数值统计:");
        Console.WriteLine($"    最小值: {numericValues.Min():F2}");
        Console.WriteLine($"    最大值: {numericValues.Max():F2}");
        Console.WriteLine($"    平均值: {numericValues.Average():F2}");
        Console.WriteLine($"    中位数: {CalculateMedian(numericValues):F2}");
    }
    else
    {
        // 显示最常见的值
        var topValues = nonEmptyValues
            .GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .ToList();
        
        if (topValues.Any())
        {
            Console.WriteLine($"  最常见的值:");
            foreach (var group in topValues)
            {
                Console.WriteLine($"    '{group.Key}': {group.Count()} 次");
            }
        }
    }
}

static double CalculateMedian(List<double> values)
{
    values.Sort();
    var mid = values.Count / 2;
    return values.Count % 2 == 0 
        ? (values[mid - 1] + values[mid]) / 2.0
        : values[mid];
}
```

这些示例展示了 dotnet-exec 在各种实际场景中的强大功能，从简单的文件处理到复杂的系统管理和数据分析任务。