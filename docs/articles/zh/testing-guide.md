# 测试指南

本指南介绍如何使用 dotnet-exec 进行各种类型的测试，包括单元测试、集成测试和自动化测试场景。

## 基础测试设置

### 启用测试模式

```sh
# 使用测试标志
dotnet-exec script.cs --test

# 等同于添加 xUnit 引用
dotnet-exec script.cs --reference "nuget:xunit" --reference "nuget:xunit.runner.visualstudio"

# 测试模式 REPL
dotnet-exec --test
```

### 简单单元测试

```csharp
// simple-test.cs
using Xunit;

public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(2, 3);
        
        // Assert
        Assert.Equal(5, result);
    }
    
    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 3, 5)]
    [InlineData(-1, 1, 0)]
    public void Add_VariousInputs_ReturnsExpectedResults(int a, int b, int expected)
    {
        var calculator = new Calculator();
        var result = calculator.Add(a, b);
        Assert.Equal(expected, result);
    }
}

public class Calculator
{
    public int Add(int a, int b) => a + b;
}
```

```sh
# 执行测试
dotnet-exec simple-test.cs --test
```

## 高级测试场景

### 使用 FluentAssertions

```sh
# 添加 FluentAssertions 进行更好的断言
dotnet-exec test.cs \
  --test \
  --reference "nuget:FluentAssertions"
```

```csharp
// fluent-test.cs
using Xunit;
using FluentAssertions;
using System.Collections.Generic;

public class CollectionTests
{
    [Fact]
    public void List_ShouldContainExpectedItems()
    {
        // Arrange
        var numbers = new List<int> { 1, 2, 3, 4, 5 };
        
        // Act & Assert
        numbers.Should().HaveCount(5);
        numbers.Should().Contain(3);
        numbers.Should().BeInAscendingOrder();
        numbers.Should().AllSatisfy(x => x.Should().BePositive());
    }
    
    [Fact]
    public void String_ShouldMatchPattern()
    {
        // Arrange
        var email = "user@example.com";
        
        // Act & Assert
        email.Should().MatchRegex(@"^[^@]+@[^@]+\.[^@]+$");
        email.Should().StartWith("user");
        email.Should().EndWith(".com");
    }
}
```

### 模拟和依赖注入

```sh
# 添加 Moq 进行模拟
dotnet-exec test.cs \
  --test \
  --reference "nuget:Moq" \
  --reference "nuget:Microsoft.Extensions.DependencyInjection"
```

```csharp
// mock-test.cs
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public interface IUserRepository
{
    Task<User> GetUserAsync(int id);
    Task SaveUserAsync(User user);
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}

public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;
    
    public UserService(IUserRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }
    
    public async Task SendWelcomeEmailAsync(int userId)
    {
        var user = await _repository.GetUserAsync(userId);
        await _emailService.SendEmailAsync(user.Email, "Welcome!", $"Hello {user.Name}!");
    }
}

public class UserServiceTests
{
    [Fact]
    public async Task SendWelcomeEmail_ValidUser_SendsEmail()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        
        var user = new User { Id = 1, Email = "test@example.com", Name = "Test User" };
        mockRepository.Setup(r => r.GetUserAsync(1)).ReturnsAsync(user);
        
        var service = new UserService(mockRepository.Object, mockEmailService.Object);
        
        // Act
        await service.SendWelcomeEmailAsync(1);
        
        // Assert
        mockRepository.Verify(r => r.GetUserAsync(1), Times.Once);
        mockEmailService.Verify(e => e.SendEmailAsync(
            "test@example.com", 
            "Welcome!", 
            "Hello Test User!"), Times.Once);
    }
}
```

## Web API 测试

### ASP.NET Core 集成测试

```sh
# Web API 测试设置
dotnet-exec api-test.cs \
  --test \
  --web \
  --reference "nuget:Microsoft.AspNetCore.Mvc.Testing"
```

```csharp
// api-test.cs
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

// 简单的 API 控制器
[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var weather = new { Temperature = 22, Summary = "Sunny" };
        return Ok(weather);
    }
}

// 测试启动类
public class TestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}

// 自定义 WebApplicationFactory
public class CustomWebApplicationFactory : WebApplicationFactory<TestStartup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseStartup<TestStartup>();
    }
}

public class WeatherApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    
    public WeatherApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Get_Weather_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/weather");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", 
            response.Content.Headers.ContentType.ToString());
    }
    
    [Fact]
    public async Task Get_Weather_ReturnsExpectedData()
    {
        // Act
        var response = await _client.GetAsync("/api/weather");
        var content = await response.Content.ReadAsStringAsync();
        var weather = JsonSerializer.Deserialize<dynamic>(content);
        
        // Assert
        Assert.NotNull(weather);
    }
}
```

## 数据库测试

### Entity Framework 测试

```sh
# EF Core 测试设置
dotnet-exec ef-test.cs \
  --test \
  --reference "nuget:Microsoft.EntityFrameworkCore.InMemory" \
  --reference "nuget:Microsoft.EntityFrameworkCore"
```

```csharp
// ef-test.cs
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class ProductContext : DbContext
{
    public ProductContext(DbContextOptions<ProductContext> options) : base(options) { }
    public DbSet<Product> Products { get; set; }
}

public class ProductService
{
    private readonly ProductContext _context;
    
    public ProductService(ProductContext context)
    {
        _context = context;
    }
    
    public async Task<Product> CreateProductAsync(string name, decimal price)
    {
        var product = new Product { Name = name, Price = price };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }
    
    public async Task<Product[]> GetExpensiveProductsAsync(decimal minPrice)
    {
        return await _context.Products
            .Where(p => p.Price >= minPrice)
            .ToArrayAsync();
    }
}

public class ProductServiceTests
{
    private ProductContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ProductContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ProductContext(options);
    }
    
    [Fact]
    public async Task CreateProduct_ValidData_ReturnsProductWithId()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ProductService(context);
        
        // Act
        var product = await service.CreateProductAsync("Test Product", 99.99m);
        
        // Assert
        Assert.True(product.Id > 0);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(99.99m, product.Price);
    }
    
    [Fact]
    public async Task GetExpensiveProducts_WithData_ReturnsFilteredResults()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ProductService(context);
        
        await service.CreateProductAsync("Cheap Product", 10.00m);
        await service.CreateProductAsync("Expensive Product", 100.00m);
        await service.CreateProductAsync("Very Expensive Product", 500.00m);
        
        // Act
        var expensiveProducts = await service.GetExpensiveProductsAsync(50.00m);
        
        // Assert
        Assert.Equal(2, expensiveProducts.Length);
        Assert.All(expensiveProducts, p => Assert.True(p.Price >= 50.00m));
    }
}
```

## 异步测试

### 异步操作测试

```csharp
// async-test.cs
using Xunit;
using System.Threading.Tasks;
using System.Net.Http;

public class AsyncHttpService
{
    private readonly HttpClient _httpClient;
    
    public AsyncHttpService()
    {
        _httpClient = new HttpClient();
    }
    
    public async Task<string> FetchDataAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
    
    public async Task<T> FetchJsonAsync<T>(string url)
    {
        var json = await FetchDataAsync(url);
        return JsonSerializer.Deserialize<T>(json);
    }
}

public class AsyncHttpServiceTests
{
    [Fact]
    public async Task FetchData_ValidUrl_ReturnsContent()
    {
        // Arrange
        var service = new AsyncHttpService();
        
        // Act
        var content = await service.FetchDataAsync("https://httpbin.org/uuid");
        
        // Assert
        Assert.NotEmpty(content);
    }
    
    [Fact]
    public async Task FetchJson_ValidUrl_ReturnsDeserializedObject()
    {
        // Arrange
        var service = new AsyncHttpService();
        
        // Act
        var result = await service.FetchJsonAsync<dynamic>("https://httpbin.org/uuid");
        
        // Assert
        Assert.NotNull(result);
    }
    
    [Fact]
    public async Task Multiple_Concurrent_Requests_Complete_Successfully()
    {
        // Arrange
        var service = new AsyncHttpService();
        var urls = Enumerable.Range(1, 5)
            .Select(i => $"https://httpbin.org/delay/{i}")
            .ToArray();
        
        // Act
        var tasks = urls.Select(url => service.FetchDataAsync(url));
        var results = await Task.WhenAll(tasks);
        
        // Assert
        Assert.Equal(5, results.Length);
        Assert.All(results, result => Assert.NotEmpty(result));
    }
}
```

## 性能测试

### 基准测试

```sh
# 添加 BenchmarkDotNet 进行性能测试
dotnet-exec benchmark-test.cs \
  --test \
  --reference "nuget:BenchmarkDotNet"
```

```csharp
// benchmark-test.cs
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Linq;

[MemoryDiagnoser]
public class StringProcessingBenchmark
{
    private readonly string[] _testData;
    
    public StringProcessingBenchmark()
    {
        _testData = Enumerable.Range(1, 1000)
            .Select(i => $"test string {i}")
            .ToArray();
    }
    
    [Benchmark]
    public string StringConcatenation()
    {
        var result = "";
        foreach (var item in _testData)
        {
            result += item + " ";
        }
        return result;
    }
    
    [Benchmark]
    public string StringBuilderApproach()
    {
        var sb = new StringBuilder();
        foreach (var item in _testData)
        {
            sb.Append(item).Append(" ");
        }
        return sb.ToString();
    }
    
    [Benchmark]
    public string StringJoinApproach()
    {
        return string.Join(" ", _testData) + " ";
    }
}

// 运行基准测试
public class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<StringProcessingBenchmark>();
    }
}
```

## 测试配置和最佳实践

### 测试配置文件

```sh
# 创建测试专用配置
dotnet-exec config set-profile unit-tests \
  --test \
  --reference "nuget:FluentAssertions" \
  --reference "nuget:Moq" \
  --using "Xunit" \
  --using "FluentAssertions" \
  --using "Moq"

# 集成测试配置
dotnet-exec config set-profile integration-tests \
  --test \
  --web \
  --reference "nuget:Microsoft.AspNetCore.Mvc.Testing" \
  --reference "nuget:Microsoft.EntityFrameworkCore.InMemory" \
  --reference "nuget:Testcontainers"

# 使用配置运行测试
dotnet-exec test.cs --profile unit-tests
```

### 测试别名

```sh
# 创建测试别名
dotnet-exec alias set unit-test \
  --test \
  --reference "nuget:FluentAssertions" \
  --using "FluentAssertions"

dotnet-exec alias set api-test \
  --test \
  --web \
  --reference "nuget:Microsoft.AspNetCore.Mvc.Testing"

# 使用别名
dotnet-exec unit-test my-tests.cs
dotnet-exec api-test api-integration-tests.cs
```

## CI/CD 集成

### GitHub Actions 测试流水线

```yaml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Install dotnet-exec
      run: dotnet tool install -g dotnet-execute
    
    - name: Run Unit Tests
      run: dotnet-exec tests/unit-tests.cs --test --reference "nuget:FluentAssertions"
    
    - name: Run Integration Tests
      run: dotnet-exec tests/integration-tests.cs --profile integration-tests
    
    - name: Generate Test Report
      run: dotnet-exec tests/test-reporter.cs --output ./test-results.xml
```

### 测试报告生成

```csharp
// test-reporter.cs
using Xunit;
using Xunit.Abstractions;

public class TestReporter
{
    public static void GenerateReport(string outputPath)
    {
        // 生成测试报告逻辑
        var report = new
        {
            TestSuite = "dotnet-exec Tests",
            ExecutionTime = DateTime.UtcNow,
            TotalTests = 25,
            PassedTests = 23,
            FailedTests = 2,
            SkippedTests = 0
        };
        
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        File.WriteAllText(outputPath, json);
    }
}

// 在测试完成后调用
TestReporter.GenerateReport("./test-results.json");
```

## 故障排除

### 常见测试问题

```sh
# 测试发现问题
dotnet-exec test.cs --test --verbose

# 并行测试问题
dotnet-exec test.cs --test --disable-parallelization

# 依赖冲突
dotnet-exec test.cs --test --show-dependency-conflicts
```

### 调试测试

```sh
# 启用详细输出
dotnet-exec test.cs --test --verbose

# 单独运行特定测试
dotnet-exec test.cs --test --filter "TestClassName.TestMethodName"

# 调试模式
dotnet-exec test.cs --test --configuration Debug --wait-for-debugger
```

这个全面的测试指南展示了如何使用 dotnet-exec 进行各种测试场景，从简单的单元测试到复杂的集成测试和性能基准测试。