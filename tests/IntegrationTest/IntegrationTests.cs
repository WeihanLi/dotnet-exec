// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Models;
using Xunit.Abstractions;

namespace IntegrationTest;

public class IntegrationTests
{
    private readonly CommandHandler _handler;
    private readonly ICodeCompiler _compiler;
    private readonly ICodeExecutor _executor;
    private readonly ITestOutputHelper _outputHelper;

    public IntegrationTests(CommandHandler handler, 
        ICodeCompiler compiler, 
        ICodeExecutor executor,
        ITestOutputHelper outputHelper)
    {
        _handler = handler;
        _compiler = compiler;
        _executor = executor;
        _outputHelper = outputHelper;
    }

    [Theory]
    [InlineData("ConfigurationManagerSample")]
    [InlineData("JsonNodeSample")]
    [InlineData("LinqSample")]
    [InlineData("MainMethodSample")]
    [InlineData("RandomSharedSample")]
    [InlineData("TopLevelSample")]
    [InlineData("HostApplicationBuilderSample")]
    [InlineData("DumpAssemblyInfoSample")]
    public async Task SamplesTest(string sampleFileName)
    {
        var filePath = $"{sampleFileName}.cs";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            ScriptFile = fullPath,
            Arguments = new[] { "--hello", "world" },
            IncludeWebReferences = true
        };

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _handler.Execute(execOptions);
        Assert.Equal(0, result);
        Assert.NotNull(output.StandardOutput);
        Assert.NotEmpty(output.StandardOutput);

        _outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("Console.WriteLine(\"Hello .NET\");")]
    public async Task AssemblyLoadContextTest(string code)
    {
        var options = new ExecOptions();
        var result = await _compiler.Compile(new ExecOptions(), code);
        if (result.Msg.IsNotNullOrEmpty())
            _outputHelper.WriteLine(result.Msg);
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Data);

        var assemblyLoadContext = new CustomLoadContext(Guard.NotNull(result.Data).References)
            ;
        result.Data.Stream.Seek(0, SeekOrigin.Begin);
        var assembly = assemblyLoadContext.LoadFromStream(result.Data.Stream);
        Assert.NotNull(assembly);

        var executeResult = await _executor.Execute(assembly, options);
        if (executeResult.Msg.IsNotNullOrEmpty())
            _outputHelper.WriteLine(executeResult.Msg);
        Assert.True(executeResult.IsSuccess());
    }
}
