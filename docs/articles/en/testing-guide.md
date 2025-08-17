# Testing Guide

This guide covers how to use dotnet-exec's integrated testing capabilities, including xUnit test execution and testing workflows.

## Overview

dotnet-exec provides built-in support for executing xUnit test cases without requiring a full project setup. The `test` command integrates with xUnit v3 to run tests directly from C# files.

## Basic Test Execution

### Simple Test File

Create a test file with xUnit test methods:

```csharp
// SimpleTest.cs
public class SimpleTest
{
    [Fact]
    public void AdditionTest()
    {
        var result = 2 + 2;
        Assert.Equal(4, result);
    }
    
    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 3, 5)]
    [InlineData(-1, 1, 0)]
    public void AdditionTheoryTest(int a, int b, int expected)
    {
        var result = a + b;
        Assert.Equal(expected, result);
    }
}
```

Execute the test:

```sh
# Run a single test file
dotnet-exec test SimpleTest.cs

# Run multiple test files
dotnet-exec test TestFile1.cs TestFile2.cs TestFile3.cs
```

## Test Command Options

### Basic Options

```sh
# Run tests with additional references
dotnet-exec test MyTest.cs -r 'nuget:Moq' -u 'Moq'

# Run tests with web framework
dotnet-exec test WebApiTest.cs --web

# Run tests with debug output
dotnet-exec test MyTest.cs --debug

# Run tests with preview features
dotnet-exec test PreviewTest.cs --preview
```

### Complete Example

```sh
# Comprehensive test execution
dotnet-exec test IntegrationTest.cs \
  --web \
  -r 'nuget:Microsoft.EntityFrameworkCore.InMemory' \
  -r 'nuget:Moq' \
  -u 'Microsoft.EntityFrameworkCore' \
  -u 'Moq' \
  --debug
```

## Test Patterns and Examples

### Unit Tests

```csharp
// UnitTest.cs
public class CalculatorTests
{
    [Fact]
    public void Multiply_TwoNumbers_ReturnsProduct()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Multiply(3, 4);
        
        // Assert
        Assert.Equal(12, result);
    }
    
    [Theory]
    [InlineData(0, 5, 0)]
    [InlineData(1, 5, 5)]
    [InlineData(-2, 3, -6)]
    public void Multiply_VariousInputs_ReturnsExpectedResults(int a, int b, int expected)
    {
        var calculator = new Calculator();
        var result = calculator.Multiply(a, b);
        Assert.Equal(expected, result);
    }
}

public class Calculator
{
    public int Multiply(int a, int b) => a * b;
}
```

### Integration Tests with Dependencies

```csharp
// IntegrationTest.cs
public class ApiIntegrationTests
{
    [Fact]
    public async Task GetWeather_ValidCity_ReturnsWeatherData()
    {
        // Arrange
        var httpClient = new HttpClient();
        var apiService = new WeatherApiService(httpClient);
        
        // Act
        var weather = await apiService.GetWeatherAsync("London");
        
        // Assert
        Assert.NotNull(weather);
        Assert.NotEmpty(weather.Description);
    }
}

public class WeatherApiService
{
    private readonly HttpClient _httpClient;
    
    public WeatherApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        // Mock implementation for testing
        await Task.Delay(10);
        return new WeatherData { City = city, Description = "Sunny", Temperature = 22 };
    }
}

public class WeatherData
{
    public string City { get; set; } = "";
    public string Description { get; set; } = "";
    public int Temperature { get; set; }
}
```

Run with HTTP client support:

```sh
dotnet-exec test IntegrationTest.cs \
  -r 'nuget:Microsoft.Extensions.Http' \
  -u 'Microsoft.Extensions.Http'
```

### Database Tests

```csharp
// DatabaseTest.cs
public class DatabaseTests : IDisposable
{
    private readonly DbContext _context;
    
    public DatabaseTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }
    
    [Fact]
    public async Task AddUser_ValidUser_SavesSuccessfully()
    {
        // Arrange
        var user = new User { Name = "John Doe", Email = "john@example.com" };
        
        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "john@example.com");
        Assert.NotNull(savedUser);
        Assert.Equal("John Doe", savedUser.Name);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}
```

Run with Entity Framework:

```sh
dotnet-exec test DatabaseTest.cs \
  -r 'nuget:Microsoft.EntityFrameworkCore' \
  -r 'nuget:Microsoft.EntityFrameworkCore.InMemory' \
  -u 'Microsoft.EntityFrameworkCore'
```

### Mocking Tests

```csharp
// MockTest.cs
public class ServiceTests
{
    [Fact]
    public async Task ProcessData_ValidInput_CallsRepository()
    {
        // Arrange
        var mockRepository = new Mock<IDataRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Data>()))
                     .Returns(Task.CompletedTask);
        
        var service = new DataService(mockRepository.Object);
        var data = new Data { Value = "test" };
        
        // Act
        await service.ProcessAsync(data);
        
        // Assert
        mockRepository.Verify(r => r.SaveAsync(data), Times.Once);
    }
}

public interface IDataRepository
{
    Task SaveAsync(Data data);
}

public class DataService
{
    private readonly IDataRepository _repository;
    
    public DataService(IDataRepository repository)
    {
        _repository = repository;
    }
    
    public async Task ProcessAsync(Data data)
    {
        // Process data
        data.Value = data.Value.ToUpper();
        await _repository.SaveAsync(data);
    }
}

public class Data
{
    public string Value { get; set; } = "";
}
```

Run with Moq:

```sh
dotnet-exec test MockTest.cs \
  -r 'nuget:Moq' \
  -u 'Moq'
```

## Advanced Testing Scenarios

### Parameterized Tests with Complex Data

```csharp
// ParameterizedTest.cs
public class AdvancedParameterizedTests
{
    public static IEnumerable<object[]> GetTestData()
    {
        yield return new object[] { new[] { 1, 2, 3 }, 6 };
        yield return new object[] { new[] { -1, 0, 1 }, 0 };
        yield return new object[] { new int[] { }, 0 };
    }
    
    [Theory]
    [MemberData(nameof(GetTestData))]
    public void Sum_Array_ReturnsCorrectTotal(int[] numbers, int expected)
    {
        var result = numbers.Sum();
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [ClassData(typeof(CalculationTestData))]
    public void Calculate_ComplexData_ReturnsExpected(CalculationInput input, int expected)
    {
        var result = input.A + input.B * input.Multiplier;
        Assert.Equal(expected, result);
    }
}

public class CalculationInput
{
    public int A { get; set; }
    public int B { get; set; }
    public int Multiplier { get; set; }
}

public class CalculationTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new CalculationInput { A = 1, B = 2, Multiplier = 3 }, 7 };
        yield return new object[] { new CalculationInput { A = 0, B = 5, Multiplier = 2 }, 10 };
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

### Async Testing

```csharp
// AsyncTest.cs
public class AsyncTests
{
    [Fact]
    public async Task ProcessAsync_LongRunningOperation_CompletesSuccessfully()
    {
        // Arrange
        var processor = new AsyncProcessor();
        
        // Act
        var result = await processor.ProcessAsync("test data");
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("PROCESSED", result);
    }
    
    [Fact]
    public async Task ConcurrentOperations_MultipleRequests_AllComplete()
    {
        // Arrange
        var processor = new AsyncProcessor();
        var tasks = new List<Task<string>>();
        
        // Act
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(processor.ProcessAsync($"data-{i}"));
        }
        
        var results = await Task.WhenAll(tasks);
        
        // Assert
        Assert.Equal(5, results.Length);
        Assert.All(results, r => Assert.Contains("PROCESSED", r));
    }
}

public class AsyncProcessor
{
    public async Task<string> ProcessAsync(string input)
    {
        await Task.Delay(100); // Simulate async work
        return $"PROCESSED: {input}";
    }
}
```

### Exception Testing

```csharp
// ExceptionTest.cs
public class ExceptionTests
{
    [Fact]
    public void DivideByZero_ThrowsDivideByZeroException()
    {
        var calculator = new Calculator();
        
        Assert.Throws<DivideByZeroException>(() => calculator.Divide(10, 0));
    }
    
    [Fact]
    public async Task ProcessInvalidData_ThrowsArgumentException()
    {
        var processor = new DataProcessor();
        
        await Assert.ThrowsAsync<ArgumentException>(() => processor.ProcessAsync(null));
    }
    
    [Fact]
    public void ValidateInput_InvalidInput_ThrowsWithCorrectMessage()
    {
        var validator = new InputValidator();
        
        var exception = Assert.Throws<ArgumentException>(() => validator.Validate(""));
        Assert.Contains("Input cannot be empty", exception.Message);
    }
}

public class Calculator
{
    public double Divide(double a, double b)
    {
        if (b == 0) throw new DivideByZeroException();
        return a / b;
    }
}

public class DataProcessor
{
    public async Task<string> ProcessAsync(string data)
    {
        if (data == null) throw new ArgumentException("Data cannot be null");
        await Task.Delay(10);
        return data.ToUpper();
    }
}

public class InputValidator
{
    public void Validate(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be empty");
    }
}
```

## Testing Profiles and Aliases

### Create Testing Profile

Set up a common testing configuration:

```sh
# Create a comprehensive testing profile
dotnet-exec profile set testing \
  -r 'nuget:Moq' \
  -r 'nuget:FluentAssertions' \
  -r 'nuget:Microsoft.EntityFrameworkCore.InMemory' \
  -u 'Moq' \
  -u 'FluentAssertions' \
  -u 'Microsoft.EntityFrameworkCore' \
  --preview
```

Use the profile:

```sh
dotnet-exec test MyTests.cs --profile testing
```

### Testing Aliases

Create aliases for common testing tasks:

```sh
# Quick test runner
dotnet-exec alias set quicktest "dotnet-exec test"

# Test with common setup
dotnet-exec alias set webtest "dotnet-exec test --profile testing --web"

# Database test runner
dotnet-exec alias set dbtest "dotnet-exec test --profile testing -r 'nuget:Microsoft.EntityFrameworkCore.SqlServer'"
```

## Best Practices

### Test Organization

1. **One Test Class Per File**:
   ```csharp
   // UserServiceTests.cs - focused on UserService
   public class UserServiceTests
   {
       // All tests for UserService
   }
   ```

2. **Descriptive Test Names**:
   ```csharp
   [Fact]
   public void CreateUser_ValidData_ReturnsUserWithId()
   [Fact]
   public void CreateUser_DuplicateEmail_ThrowsException()
   [Fact]
   public void GetUser_NonExistentId_ReturnsNull()
   ```

3. **Arrange-Act-Assert Pattern**:
   ```csharp
   [Fact]
   public void TestMethod_Scenario_ExpectedResult()
   {
       // Arrange
       var input = "test";
       var expected = "TEST";
       
       // Act
       var result = input.ToUpper();
       
       // Assert
       Assert.Equal(expected, result);
   }
   ```

### Test Dependencies

1. **Minimize External Dependencies**:
   ```sh
   # Use in-memory alternatives
   dotnet-exec test DatabaseTests.cs \
     -r 'nuget:Microsoft.EntityFrameworkCore.InMemory'
   ```

2. **Mock External Services**:
   ```sh
   # Use mocking frameworks
   dotnet-exec test ServiceTests.cs \
     -r 'nuget:Moq' \
     -u 'Moq'
   ```

### Performance Considerations

1. **Parallel Test Execution**:
   xUnit runs tests in parallel by default, but be aware of shared resources.

2. **Resource Cleanup**:
   ```csharp
   public class TestClass : IDisposable
   {
       public void Dispose()
       {
           // Clean up resources
       }
   }
   ```

## Integration with CI/CD

### Running Tests in Pipelines

```sh
# Basic test execution
dotnet-exec test Tests/*.cs --profile testing

# With detailed output for CI
dotnet-exec test Tests/*.cs --profile testing --debug

# Generate test results (if needed, process output)
dotnet-exec test Tests/*.cs --profile testing > test-results.txt
```

### Example CI Script

```bash
#!/bin/bash
# ci-test.sh

echo "Setting up test environment..."
dotnet-exec profile set ci-testing \
  -r 'nuget:Moq' \
  -r 'nuget:FluentAssertions' \
  --debug

echo "Running unit tests..."
dotnet-exec test UnitTests/*.cs --profile ci-testing

echo "Running integration tests..."
dotnet-exec test IntegrationTests/*.cs --profile ci-testing --web

echo "Tests completed."
```

## Troubleshooting Tests

### Common Issues

1. **Assembly Resolution**:
   ```sh
   # Debug assembly loading
   dotnet-exec test MyTest.cs --debug
   ```

2. **Missing Dependencies**:
   ```sh
   # Add missing references explicitly
   dotnet-exec test MyTest.cs \
     -r 'nuget:MissingPackage' \
     -u 'MissingNamespace'
   ```

3. **Test Discovery Issues**:
   Ensure test classes and methods are public and properly attributed with `[Fact]` or `[Theory]`.

### Debug Output

Use the `--debug` flag to see detailed information about test execution:

```sh
dotnet-exec test MyTest.cs --debug
```

This shows:
- Assembly loading details
- Reference resolution
- Compilation information
- Test discovery process

For more information on script execution and configuration, see the [Getting Started](getting-started.md) and [Advanced Usage](advanced-usage.md) guides.