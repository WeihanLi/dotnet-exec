// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using WeihanLi.Common.Models;
using Xunit.Abstractions;

namespace UnitTest;

public class AdvancedCodeCompilerTest
{
    private readonly ITestOutputHelper _outputHelper;

    public AdvancedCodeCompilerTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Theory(
         Skip = "localOnly"
        )]
    [InlineData(@"C:\projects\sources\SamplesInPractice\CSharp10Sample", @"C:\projects\sources\SamplesInPractice\CSharp10Sample\CallerInfo.cs")]
    [InlineData(@"C:\projects\sources\SamplesInPractice\MathProblems\MathProblems.csproj", @"C:\projects\sources\SamplesInPractice\MathProblems\CombinationAndPermutation.cs")]
    public async Task CompileTest(string projectPath, string scriptFile)
    {
        var codeCompiler = new AdvancedCodeCompiler(NullLogger.Instance);

        var result = await codeCompiler.Compile(new ExecOptions()
        {
            ProjectPath = projectPath,
            Script = scriptFile
        }, string.Empty);

        if (result.Msg.IsNotNullOrEmpty())
            _outputHelper.WriteLine(result.Msg);

        Assert.True(result.IsSuccess());

        var assembly = Assembly.Load(Guard.NotNull(result.Data).Stream.ToArray());
        var types = assembly.GetTypes();
        Assert.NotEmpty(types);
    }
}
