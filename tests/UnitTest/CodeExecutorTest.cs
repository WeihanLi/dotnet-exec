// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Services;
using Microsoft.Extensions.Logging.Abstractions;
using WeihanLi.Common.Models;
using Xunit.Abstractions;

namespace UnitTest;

public sealed class CodeExecutorTest
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly SimpleCodeCompiler _compiler = SimpleCodeCompilerTest.GetSimpleCodeCompiler();

    public CodeExecutorTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Theory]
    [InlineData("Console.WriteLine(123);")]
    [InlineData("using WeihanLi.Extensions; Console.WriteLine(args.StringJoin(\", \"));")]
    public async Task ExecuteWithDefaultEntry(string code)
    {
        var execOptions = new ExecOptions()
        {
            Arguments = ["--hello", "world"]
        };
        var result = await _compiler.Compile(execOptions, code);
        _outputHelper.WriteLine($"{result.Msg}");
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Data);
        Guard.NotNull(result.Data);
        var executor = new DefaultCodeExecutor(RefResolver.InstanceForTest, NullLogger.Instance);
        var executeResult = await executor.Execute(result.Data, execOptions);
        _outputHelper.WriteLine($"{executeResult.Msg}");
        Assert.True(executeResult.IsSuccess());
    }

    [Theory]
    [InlineData(@"
public class SomeTest
{
  public static void MainTest() { Console.WriteLine(""MainTest""); }
}")]
    [InlineData(@"
public class SomeTest
{
  public static void MainTest(string[] args) {}
}")]
    [InlineData(@"
internal class SomeTest
{
  public static void MainTest(string[] args) {}
}")]
    [InlineData(@"
internal class SomeTest
{
  private static void MainTest(string[] args) {}
}")]
    public async Task ExecuteWithCustomEntry(string code)
    {
        var execOptions = new ExecOptions();
        var result = await _compiler.Compile(execOptions, code);
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Data);
        Guard.NotNull(result.Data);
        var executor = new DefaultCodeExecutor(RefResolver.InstanceForTest, NullLogger.Instance);
        var executeResult = await executor.Execute(result.Data, execOptions);
        _outputHelper.WriteLine($"{executeResult.Msg}");
        Assert.True(executeResult.IsSuccess());
    }

    [Theory]
    [InlineData(@"
namespace Test;
class A
{
  public static void MainTest() => Console.WriteLine(123);
}
class B
{
  public static void MainTest() => throw null;
}
")]
    public async Task StartupTypeTestSuccess(string code)
    {
        var options = new ExecOptions() { StartupType = "Test.A" };
        var result = await _compiler.Compile(options, code);
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Data);
        Guard.NotNull(result.Data);
        using var output = await ConsoleOutput.CaptureAsync();
        var executor = new DefaultCodeExecutor(RefResolver.InstanceForTest, NullLogger.Instance);
        var executeResult = await executor.Execute(result.Data, options);
        Assert.True(executeResult.IsSuccess());
    }

    [Theory]
    [InlineData(@"
namespace Test;
class A
{
  public static void MainTest() => Console.WriteLine(123);
}
class B
{
  public static void MainTest() => throw null;
}
")]
    public async Task StartupTypeTestException(string code)
    {
        var options = new ExecOptions { StartupType = "Test.B" };
        var result = await _compiler.Compile(options, code);
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Data);
        Guard.NotNull(result.Data);
        using var output = await ConsoleOutput.CaptureAsync();
        var executor = new DefaultCodeExecutor(RefResolver.InstanceForTest, NullLogger.Instance);
        var executeResult = await executor.Execute(result.Data, options);
        _outputHelper.WriteLine(executeResult.Msg);
        Assert.False(executeResult.IsSuccess());
    }
}
