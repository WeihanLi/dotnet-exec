// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Models;

namespace UnitTest;

public class AdvancedCodeCompilerTest
{
    [Fact]
    public async Task CompileTest()
    {
        var codeCompiler = new AdvancedCodeCompiler();
        
        var result = await codeCompiler.Compile(new ExecOptions()
        {
            ProjectPath = @"C:\projects\sources\SamplesInPractice\CSharp10Sample",
            ScriptFile = @"C:\projects\sources\SamplesInPractice\CSharp10Sample\CallerInfo.cs"
        }, string.Empty);
        
        Assert.True(result.IsSuccess());
    }

}
