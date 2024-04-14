// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Services;
using WeihanLi.Common.Models;
using Xunit.Abstractions;

namespace UnitTest;

public sealed class SimpleCodeCompilerTest(ITestOutputHelper outputHelper)
{
    private readonly ITestOutputHelper _outputHelper = outputHelper;

    [Theory]
    [InlineData(@"public static void Main(int num){}")]
    [InlineData(@"public class Program{ public static int Main(){} }")]
    public async Task CompileFailed(string code)
    {
        var compiler = GetSimpleCodeCompiler();
        var result = await compiler.Compile(new ExecOptions(), code);
        Assert.Equal(ResultStatus.ProcessFail, result.Status);
        _outputHelper.WriteLine(result.Msg);
    }

    [Theory]
    [InlineData(@"
public class SomeTest
{
  public static void MainTest() { Console.WriteLine(""MainTest""); }
}")]
    [InlineData(@"Console.WriteLine(""Top-level statements"");")]
    [InlineData(@"using WeihanLi.Extensions;
Console.WriteLine(args.StringJoin(Environment.NewLine));
")]
    public async Task CompileWithCustomEntryPoint(string code)
    {
        var compiler = GetSimpleCodeCompiler();
        var result = await compiler.Compile(new ExecOptions(), code);
        _outputHelper.WriteLine($"{result.Msg}");
        Assert.Equal(ResultStatus.Success, result.Status);
    }

    [Theory]
    [InlineData(""""
public class SomeTest
{
  public static void MainTest() 
  {
      Console.WriteLine("""
      {
          "Name": "test"
      }
      """);
  }
}
"""")]
    [InlineData(""""
Console.WriteLine("""
{
    "Name": "test"
}
""");
"""")]
    public async Task CompileWithPreviewLanguageFeature(string code)
    {
        var compiler = GetSimpleCodeCompiler();
        var result = await compiler.Compile(new ExecOptions()
        {
            EnablePreviewFeatures = true
        }, code);
        _outputHelper.WriteLine($"{result.Msg}");
        Assert.Equal(ResultStatus.Success, result.Status);
    }

    public static SimpleCodeCompiler GetSimpleCodeCompiler() =>
        new(RefResolver.InstanceForTest, AdditionalScriptContentFetcher.InstanceForTest,
            new ParseOptionsPipeline([]),
            new CompilationOptionsPipeline([]));
}
