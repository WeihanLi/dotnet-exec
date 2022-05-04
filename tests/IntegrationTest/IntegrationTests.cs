// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Exec;
using WeihanLi.Common.Models;

namespace IntegrationTest;

public class IntegrationTests
{
    private readonly ICodeExecutor _executor;
    private readonly ICodeCompiler _compiler;

    public IntegrationTests(ICodeExecutor executor, ICodeCompiler compiler)
    {
        _executor = executor;
        _compiler = compiler;
    }
    [Theory]
    [InlineData(nameof(RandomSharedSample))]
    public async Task RandomSharedSample(string sampleFileName)
    {
        var filePath = $"{sampleFileName}.cs";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions();
        
        var codeText = await File.ReadAllTextAsync(fullPath);
        var compileResult = await _compiler.Compile(codeText, execOptions);
        Assert.NotNull(compileResult.Data);
        Guard.NotNull(compileResult.Data);

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _executor.Execute(compileResult.Data, execOptions);
        Assert.True(result.IsSuccess());
        Assert.NotNull(output.StandardOutput);
        Assert.NotEmpty(output.StandardOutput);
    }
}
