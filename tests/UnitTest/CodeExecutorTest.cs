// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging.Abstractions;
using WeihanLi.Common.Models;
using Xunit.Abstractions;

namespace UnitTest;

public class CodeExecutorTest
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly ICodeCompiler _compiler = new SimpleCodeCompiler();

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
            Arguments = new[] { "--hello", "world" }
        };
        var result = await _compiler.Compile(execOptions, code);
        _outputHelper.WriteLine($"{result.Msg}");
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Data);
        Guard.NotNull(result.Data);
        var executor = new CodeExecutor(NullLogger.Instance);
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
        var executor = new CodeExecutor(NullLogger.Instance);
        var executeResult = await executor.Execute(result.Data, execOptions);
        _outputHelper.WriteLine($"{executeResult.Msg}");
        Assert.True(executeResult.IsSuccess());
    }
}
