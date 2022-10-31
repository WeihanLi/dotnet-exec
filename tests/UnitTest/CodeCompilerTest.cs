// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;
using WeihanLi.Common.Models;
using Xunit.Abstractions;

namespace UnitTest;

public class CodeCompilerTest
{
    private readonly ITestOutputHelper _outputHelper;

    public CodeCompilerTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Theory]
    [InlineData(@"public static void Main(int num){}")]
    [InlineData(@"public class Program{ public static int Main(){} }")]
    public async Task CompileFailed(string code)
    {
        var compiler = new DefaultCodeCompiler(RefResolver.InstanceForTest, AdditionalScriptContentFetcher.InstanceForTest);
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
        var compiler = new DefaultCodeCompiler(RefResolver.InstanceForTest, AdditionalScriptContentFetcher.InstanceForTest);
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
        var compiler = new DefaultCodeCompiler(RefResolver.InstanceForTest, AdditionalScriptContentFetcher.InstanceForTest);
        var result = await compiler.Compile(new ExecOptions()
        {
            LanguageVersion = LanguageVersion.Preview
        }, code);
        _outputHelper.WriteLine($"{result.Msg}");
        Assert.Equal(ResultStatus.Success, result.Status);
    }
}
