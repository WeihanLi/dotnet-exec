# Examples and Use Cases

This guide provides real-world examples and practical use cases for dotnet-exec across different scenarios and domains.

## Quick Utilities

### String and Text Processing

```sh
# Base64 encoding
dotnet-exec 'Convert.ToBase64String(Encoding.UTF8.GetBytes(args[0]))' -- "Hello World"

# URL encoding
dotnet-exec 'System.Web.HttpUtility.UrlEncode(args[0])' -r 'nuget:System.Web.HttpUtility' -- "hello world"

# JSON formatting
dotnet-exec '
var json = args[0];
var obj = JsonSerializer.Deserialize<object>(json);
Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
' -- '{"name":"John","age":30}'

# Hash calculation
dotnet-exec '
using System.Security.Cryptography;
var input = args.Length > 0 ? args[0] : "default";
var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
Console.WriteLine(Convert.ToHexString(hash));
' -- "text to hash"
```

### File Operations

```sh
# Count lines in files
dotnet-exec '
var files = args.Length > 0 ? args : Directory.GetFiles(".", "*.cs");
foreach (var file in files)
{
    var lines = File.ReadAllLines(file).Length;
    Console.WriteLine($"{Path.GetFileName(file)}: {lines} lines");
}
' -- Program.cs Helper.cs

# Find and replace in files
dotnet-exec '
var pattern = args[0];
var replacement = args[1];
var files = Directory.GetFiles(".", "*.txt");
foreach (var file in files)
{
    var content = File.ReadAllText(file);
    var newContent = content.Replace(pattern, replacement);
    if (content != newContent)
    {
        File.WriteAllText(file, newContent);
        Console.WriteLine($"Updated: {file}");
    }
}
' -- "oldtext" "newtext"

# Directory size calculator
dotnet-exec '
var path = args.Length > 0 ? args[0] : ".";
var totalSize = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
    .Sum(file => new FileInfo(file).Length);
Console.WriteLine($"Total size: {totalSize:N0} bytes ({totalSize / 1024.0 / 1024.0:F2} MB)");
' -- ./src
```

### Network Utilities

```sh
# HTTP health check
dotnet-exec '
var url = args.Length > 0 ? args[0] : "https://httpbin.org/status/200";
var client = new HttpClient();
try
{
    var response = await client.GetAsync(url);
    Console.WriteLine($"Status: {response.StatusCode}");
    Console.WriteLine($"Response time: {response.Headers.Date}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
' -- "https://github.com"

# Port scanner
dotnet-exec '
using System.Net.Sockets;
var host = args.Length > 0 ? args[0] : "localhost";
var ports = new[] { 80, 443, 22, 3389, 5432, 3306 };
Console.WriteLine($"Scanning {host}...");
foreach (var port in ports)
{
    try
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port);
        Console.WriteLine($"Port {port}: Open");
    }
    catch
    {
        Console.WriteLine($"Port {port}: Closed");
    }
}
' -- "google.com"

# DNS lookup
dotnet-exec '
using System.Net;
var hostname = args.Length > 0 ? args[0] : "github.com";
try
{
    var addresses = await Dns.GetHostAddressesAsync(hostname);
    Console.WriteLine($"IP addresses for {hostname}:");
    foreach (var addr in addresses)
    {
        Console.WriteLine($"  {addr}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
' -- "microsoft.com"
```

## Data Processing

### CSV Processing

```sh
# CSV to JSON conversion
dotnet-exec '
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
var csvFile = args.Length > 0 ? args[0] : "data.csv";
using var reader = new StringReader(File.ReadAllText(csvFile));
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var records = csv.GetRecords<dynamic>().ToList();
var json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine(json);
' -r 'nuget:CsvHelper' -- data.csv

# CSV analysis
dotnet-exec '
using CsvHelper;
using System.Globalization;
var csvFile = args[0];
using var reader = new StringReader(File.ReadAllText(csvFile));
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
csv.Read();
csv.ReadHeader();
var headers = csv.HeaderRecord;
Console.WriteLine($"Columns: {headers?.Length}");
Console.WriteLine($"Headers: {string.Join(", ", headers ?? Array.Empty<string>())}");
var recordCount = 0;
while (csv.Read()) recordCount++;
Console.WriteLine($"Records: {recordCount}");
' -r 'nuget:CsvHelper' -- data.csv
```

### JSON Processing

```sh
# JSON path query
dotnet-exec '
using Newtonsoft.Json.Linq;
var jsonFile = args[0];
var path = args[1];
var json = File.ReadAllText(jsonFile);
var obj = JObject.Parse(json);
var result = obj.SelectToken(path);
Console.WriteLine(result?.ToString());
' -r 'nuget:Newtonsoft.Json' -- data.json "$.users[0].name"

# JSON flattening
dotnet-exec '
using Newtonsoft.Json.Linq;
var jsonText = args[0];
var obj = JObject.Parse(jsonText);
var flattened = new Dictionary<string, object>();
FlattenJson(obj, flattened, "");
foreach (var kvp in flattened)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

void FlattenJson(JToken token, Dictionary<string, object> result, string prefix)
{
    switch (token.Type)
    {
        case JTokenType.Object:
            foreach (var prop in token.Children<JProperty>())
            {
                FlattenJson(prop.Value, result, prefix + prop.Name + ".");
            }
            break;
        case JTokenType.Array:
            var array = token as JArray;
            for (int i = 0; i < array?.Count; i++)
            {
                FlattenJson(array[i], result, prefix + $"[{i}].");
            }
            break;
        default:
            result[prefix.TrimEnd('.')] = ((JValue)token).Value ?? "";
            break;
    }
}
' -r 'nuget:Newtonsoft.Json' -- '{"user":{"name":"John","addresses":[{"city":"NYC"},{"city":"LA"}]}}'
```

### XML Processing

```sh
# XML to JSON conversion
dotnet-exec '
using System.Xml;
using Newtonsoft.Json;
var xmlFile = args[0];
var xml = File.ReadAllText(xmlFile);
var doc = new XmlDocument();
doc.LoadXml(xml);
var json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
Console.WriteLine(json);
' -r 'nuget:Newtonsoft.Json' -- data.xml

# XML validation
dotnet-exec '
using System.Xml;
using System.Xml.Schema;
var xmlFile = args[0];
var xsdFile = args.Length > 1 ? args[1] : null;
var settings = new XmlReaderSettings();
if (xsdFile != null)
{
    settings.Schemas.Add(null, xsdFile);
    settings.ValidationType = ValidationType.Schema;
}
settings.ValidationEventHandler += (s, e) => Console.WriteLine($"Validation Error: {e.Message}");
try
{
    using var reader = XmlReader.Create(xmlFile, settings);
    while (reader.Read()) { }
    Console.WriteLine("XML is valid");
}
catch (Exception ex)
{
    Console.WriteLine($"XML Error: {ex.Message}");
}
' -- document.xml schema.xsd
```

## Web Development

### API Testing

```sh
# REST API client
dotnet-exec '
var baseUrl = args.Length > 0 ? args[0] : "https://jsonplaceholder.typicode.com";
var client = new HttpClient();

// GET request
var users = await client.GetStringAsync($"{baseUrl}/users");
Console.WriteLine("Users:");
Console.WriteLine(users);

// POST request
var newPost = new
{
    title = "Test Post",
    body = "This is a test post",
    userId = 1
};
var json = JsonSerializer.Serialize(newPost);
var content = new StringContent(json, Encoding.UTF8, "application/json");
var response = await client.PostAsync($"{baseUrl}/posts", content);
Console.WriteLine($"POST Status: {response.StatusCode}");
Console.WriteLine(await response.Content.ReadAsStringAsync());
' -- "https://jsonplaceholder.typicode.com"

# API performance test
dotnet-exec '
var url = args[0];
var requests = int.Parse(args.Length > 1 ? args[1] : "10");
var client = new HttpClient();
var stopwatch = Stopwatch.StartNew();
var tasks = Enumerable.Range(0, requests)
    .Select(_ => client.GetAsync(url))
    .ToArray();
var responses = await Task.WhenAll(tasks);
stopwatch.Stop();
Console.WriteLine($"Completed {requests} requests in {stopwatch.ElapsedMilliseconds}ms");
Console.WriteLine($"Average: {stopwatch.ElapsedMilliseconds / (double)requests:F2}ms per request");
var statusCodes = responses.GroupBy(r => r.StatusCode)
    .ToDictionary(g => g.Key, g => g.Count());
foreach (var kvp in statusCodes)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value} responses");
}
' -- "https://httpbin.org/delay/1" "5"
```

### Web Scraping

```sh
# Simple web scraper
dotnet-exec '
using HtmlAgilityPack;
var url = args[0];
var web = new HtmlWeb();
var doc = await web.LoadFromWebAsync(url);
var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim();
Console.WriteLine($"Title: {title}");
var links = doc.DocumentNode.SelectNodes("//a[@href]");
Console.WriteLine($"Found {links?.Count ?? 0} links:");
foreach (var link in links?.Take(10) ?? Enumerable.Empty<HtmlNode>())
{
    var href = link.GetAttributeValue("href", "");
    var text = link.InnerText?.Trim();
    Console.WriteLine($"  {text}: {href}");
}
' -r 'nuget:HtmlAgilityPack' -- "https://github.com"

# Extract all images
dotnet-exec '
using HtmlAgilityPack;
var url = args[0];
var web = new HtmlWeb();
var doc = await web.LoadFromWebAsync(url);
var images = doc.DocumentNode.SelectNodes("//img[@src]");
Console.WriteLine($"Found {images?.Count ?? 0} images:");
foreach (var img in images ?? Enumerable.Empty<HtmlNode>())
{
    var src = img.GetAttributeValue("src", "");
    var alt = img.GetAttributeValue("alt", "");
    Console.WriteLine($"  {alt}: {src}");
}
' -r 'nuget:HtmlAgilityPack' -- "https://github.com"
```

### Server Management

```sh
# Simple HTTP server
dotnet-exec '
var app = WebApplication.Create();
app.MapGet("/", () => "Hello from dotnet-exec server!");
app.MapGet("/time", () => DateTime.Now.ToString());
app.MapGet("/env", (string? name) => 
    name != null ? Environment.GetEnvironmentVariable(name) : 
    Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
        .ToDictionary(e => e.Key.ToString(), e => e.Value?.ToString()));
Console.WriteLine("Server starting at http://localhost:5000");
app.Run("http://localhost:5000");
' --web

# Health check endpoint
dotnet-exec '
var app = WebApplication.Create();
app.MapGet("/health", async () =>
{
    var checks = new Dictionary<string, object>
    {
        ["timestamp"] = DateTime.UtcNow,
        ["uptime"] = Environment.TickCount64,
        ["memory"] = GC.GetTotalMemory(false),
        ["threads"] = ThreadPool.ThreadCount
    };
    return Results.Ok(checks);
});
app.Run();
' --web
```

## Database Operations

### SQL Server

```sh
# Database query
dotnet-exec '
using Microsoft.Data.SqlClient;
var connectionString = args[0];
var query = args.Length > 1 ? args[1] : "SELECT @@VERSION";
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();
using var command = new SqlCommand(query, connection);
using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
{
    for (int i = 0; i < reader.FieldCount; i++)
    {
        Console.Write($"{reader.GetName(i)}: {reader[i]}  ");
    }
    Console.WriteLine();
}
' -r 'nuget:Microsoft.Data.SqlClient' -- "Server=localhost;Database=TestDB;Trusted_Connection=true;" "SELECT * FROM Users"

# Database backup script
dotnet-exec '
using Microsoft.Data.SqlClient;
var connectionString = args[0];
var backupPath = args[1];
var databaseName = args[2];
var sql = $"BACKUP DATABASE [{databaseName}] TO DISK = '\''{backupPath}'\''";
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();
using var command = new SqlCommand(sql, connection);
Console.WriteLine($"Starting backup of {databaseName}...");
await command.ExecuteNonQueryAsync();
Console.WriteLine("Backup completed successfully");
' -r 'nuget:Microsoft.Data.SqlClient' -- "Server=localhost;Trusted_Connection=true;" "C:\\Backups\\MyDB.bak" "MyDatabase"
```

### SQLite

```sh
# SQLite operations
dotnet-exec '
using Microsoft.Data.Sqlite;
var dbPath = args.Length > 0 ? args[0] : "test.db";
var connectionString = $"Data Source={dbPath}";
using var connection = new SqliteConnection(connectionString);
await connection.OpenAsync();

// Create table
var createTable = @"
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Email TEXT UNIQUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
)";
using var createCmd = new SqliteCommand(createTable, connection);
await createCmd.ExecuteNonQueryAsync();

// Insert data
var insertSql = "INSERT INTO Users (Name, Email) VALUES (@name, @email)";
using var insertCmd = new SqliteCommand(insertSql, connection);
insertCmd.Parameters.AddWithValue("@name", "John Doe");
insertCmd.Parameters.AddWithValue("@email", "john@example.com");
try
{
    await insertCmd.ExecuteNonQueryAsync();
    Console.WriteLine("User inserted successfully");
}
catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // UNIQUE constraint
{
    Console.WriteLine("User already exists");
}

// Query data
var selectSql = "SELECT * FROM Users";
using var selectCmd = new SqliteCommand(selectSql, connection);
using var reader = await selectCmd.ExecuteReaderAsync();
Console.WriteLine("Users:");
while (await reader.ReadAsync())
{
    Console.WriteLine($"  {reader["Id"]}: {reader["Name"]} ({reader["Email"]}) - {reader["CreatedAt"]}");
}
' -r 'nuget:Microsoft.Data.Sqlite' -- "users.db"
```

## DevOps and Automation

### Git Operations

```sh
# Git log analysis
dotnet-exec '
using System.Diagnostics;
var repoPath = args.Length > 0 ? args[0] : ".";
var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "git",
        Arguments = "log --pretty=format:\"%h|%an|%ad|%s\" --date=short",
        WorkingDirectory = repoPath,
        RedirectStandardOutput = true,
        UseShellExecute = false
    }
};
process.Start();
var output = await process.StandardOutput.ReadToEndAsync();
await process.WaitForExitAsync();
var commits = output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
    .Select(line => line.Split('|'))
    .Where(parts => parts.Length == 4)
    .Select(parts => new { Hash = parts[0], Author = parts[1], Date = parts[2], Message = parts[3] })
    .ToList();
Console.WriteLine($"Total commits: {commits.Count}");
var authorStats = commits.GroupBy(c => c.Author)
    .OrderByDescending(g => g.Count())
    .Take(5);
Console.WriteLine("Top contributors:");
foreach (var author in authorStats)
{
    Console.WriteLine($"  {author.Key}: {author.Count()} commits");
}
' -- "."

# Repository health check
dotnet-exec '
using System.Diagnostics;
var repoPath = args.Length > 0 ? args[0] : ".";
async Task<string> RunGit(string args)
{
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = args,
            WorkingDirectory = repoPath,
            RedirectStandardOutput = true,
            UseShellExecute = false
        }
    };
    process.Start();
    var output = await process.StandardOutput.ReadToEndAsync();
    await process.WaitForExitAsync();
    return output.Trim();
}

Console.WriteLine("Repository Health Check");
Console.WriteLine("=======================");
Console.WriteLine($"Branch: {await RunGit("branch --show-current")}");
Console.WriteLine($"Status: {await RunGit("status --porcelain")}");
Console.WriteLine($"Last commit: {await RunGit("log -1 --pretty=format:\"%h %s\"")}");
Console.WriteLine($"Remote: {await RunGit("remote get-url origin")}");
' -- "."
```

### System Monitoring

```sh
# Process monitor
dotnet-exec '
var processName = args.Length > 0 ? args[0] : "dotnet";
Console.WriteLine($"Monitoring processes matching: {processName}");
while (true)
{
    var processes = Process.GetProcesses()
        .Where(p => p.ProcessName.Contains(processName, StringComparison.OrdinalIgnoreCase))
        .ToList();
    Console.Clear();
    Console.WriteLine($"Found {processes.Count} matching processes at {DateTime.Now}:");
    Console.WriteLine("PID\tName\t\tMemory (MB)\tCPU Time");
    Console.WriteLine("".PadRight(60, '-'));
    foreach (var p in processes.Take(10))
    {
        try
        {
            var memory = p.WorkingSet64 / 1024 / 1024;
            var cpuTime = p.TotalProcessorTime;
            Console.WriteLine($"{p.Id}\t{p.ProcessName}\t\t{memory}\t\t{cpuTime}");
        }
        catch { }
    }
    await Task.Delay(5000);
}
' -- "code"

# Disk space monitor
dotnet-exec '
var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
Console.WriteLine("Disk Space Report");
Console.WriteLine("================");
foreach (var drive in drives)
{
    var totalGB = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
    var freeGB = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
    var usedGB = totalGB - freeGB;
    var usedPercent = (usedGB / totalGB) * 100;
    Console.WriteLine($"Drive {drive.Name}:");
    Console.WriteLine($"  Total: {totalGB:F2} GB");
    Console.WriteLine($"  Used:  {usedGB:F2} GB ({usedPercent:F1}%)");
    Console.WriteLine($"  Free:  {freeGB:F2} GB");
    if (usedPercent > 90)
    {
        Console.WriteLine($"  WARNING: Drive {drive.Name} is {usedPercent:F1}% full!");
    }
    Console.WriteLine();
}
'
```

### Log Analysis

```sh
# Log file analyzer
dotnet-exec '
var logFile = args[0];
var pattern = args.Length > 1 ? args[1] : "ERROR";
var lines = File.ReadAllLines(logFile);
var matches = lines.Where(line => line.Contains(pattern, StringComparison.OrdinalIgnoreCase)).ToList();
Console.WriteLine($"Found {matches.Count} lines matching '\''{pattern}'\'' in {logFile}");
Console.WriteLine("Recent matches:");
foreach (var match in matches.TakeLast(10))
{
    Console.WriteLine($"  {match}");
}
if (matches.Count > 0)
{
    var hourlyStats = matches
        .Where(line => DateTime.TryParse(line.Substring(0, Math.Min(19, line.Length)), out _))
        .GroupBy(line => DateTime.Parse(line.Substring(0, 19)).Hour)
        .OrderBy(g => g.Key);
    Console.WriteLine("\nHourly distribution:");
    foreach (var hour in hourlyStats)
    {
        Console.WriteLine($"  Hour {hour.Key:D2}: {hour.Count()} occurrences");
    }
}
' -- "/var/log/application.log" "ERROR"

# Real-time log monitor
dotnet-exec '
var logFile = args[0];
var keywords = args.Skip(1).ToArray();
Console.WriteLine($"Monitoring {logFile} for: {string.Join(", ", keywords)}");
using var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
fs.Seek(0, SeekOrigin.End);
using var reader = new StreamReader(fs);
while (true)
{
    var line = await reader.ReadLineAsync();
    if (line != null)
    {
        if (keywords.Length == 0 || keywords.Any(k => line.Contains(k, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {line}");
        }
    }
    else
    {
        await Task.Delay(1000);
    }
}
' -- "app.log" "ERROR" "WARNING"
```

## Machine Learning and Data Science

### Data Analysis

```sh
# Statistical analysis
dotnet-exec '
var numbers = args.Select(double.Parse).ToArray();
if (numbers.Length == 0)
{
    Console.WriteLine("Please provide numbers as arguments");
    return;
}
var mean = numbers.Average();
var variance = numbers.Select(x => Math.Pow(x - mean, 2)).Average();
var stdDev = Math.Sqrt(variance);
var median = numbers.OrderBy(x => x).Skip(numbers.Length / 2).First();
Console.WriteLine($"Count: {numbers.Length}");
Console.WriteLine($"Mean: {mean:F2}");
Console.WriteLine($"Median: {median:F2}");
Console.WriteLine($"Std Dev: {stdDev:F2}");
Console.WriteLine($"Min: {numbers.Min():F2}");
Console.WriteLine($"Max: {numbers.Max():F2}");
Console.WriteLine($"Range: {numbers.Max() - numbers.Min():F2}");
' -- 1.5 2.3 3.7 4.1 5.9 2.8 3.2 4.6 5.1 2.9

# Data correlation
dotnet-exec '
using MathNet.Numerics.Statistics;
var file1 = args[0];
var file2 = args[1];
var data1 = File.ReadAllLines(file1).Select(double.Parse).ToArray();
var data2 = File.ReadAllLines(file2).Select(double.Parse).ToArray();
if (data1.Length != data2.Length)
{
    Console.WriteLine("Data sets must have the same length");
    return;
}
var correlation = Correlation.Pearson(data1, data2);
Console.WriteLine($"Pearson correlation: {correlation:F4}");
var covariance = data1.Zip(data2).Select(pair => (pair.First - data1.Average()) * (pair.Second - data2.Average())).Average();
Console.WriteLine($"Covariance: {covariance:F4}");
' -r 'nuget:MathNet.Numerics' -- data1.txt data2.txt
```

### ML.NET Examples

```sh
# Simple sentiment analysis
dotnet-exec '
using Microsoft.ML;
using Microsoft.ML.Data;
var context = new MLContext();
var data = new[]
{
    new SentimentData { Text = "This is great!", Label = true },
    new SentimentData { Text = "This is terrible", Label = false },
    new SentimentData { Text = "I love this", Label = true },
    new SentimentData { Text = "I hate this", Label = false }
};
var dataView = context.Data.LoadFromEnumerable(data);
var pipeline = context.Transforms.Text.FeaturizeText("Features", "Text")
    .Append(context.BinaryClassification.Trainers.SdcaLogisticRegression());
var model = pipeline.Fit(dataView);
var engine = context.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
var testText = args.Length > 0 ? args[0] : "This is amazing!";
var prediction = engine.Predict(new SentimentData { Text = testText });
Console.WriteLine($"Text: {testText}");
Console.WriteLine($"Sentiment: {(prediction.Prediction ? "Positive" : "Negative")}");
Console.WriteLine($"Probability: {prediction.Probability:F2}");

public class SentimentData
{
    public string Text { get; set; } = "";
    public bool Label { get; set; }
}

public class SentimentPrediction
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }
    public float Probability { get; set; }
}
' -r 'nuget:Microsoft.ML' -- "I really enjoy using this tool"
```

These examples demonstrate the versatility of dotnet-exec across different domains. For more specific use cases, see the [Getting Started](getting-started.md), [Advanced Usage](advanced-usage.md), and other specialized guides.