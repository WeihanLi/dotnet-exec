// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Models;
using Xunit.Abstractions;

namespace IntegrationTest;

public class IntegrationTests
{
    private readonly CommandHandler _handler;
    private readonly ICompilerFactory _compilerFactory;
    private readonly ICodeExecutor _executor;
    private readonly ITestOutputHelper _outputHelper;

    public IntegrationTests(CommandHandler handler,
        ICompilerFactory compilerFactory,
        ICodeExecutor executor,
        ITestOutputHelper outputHelper)
    {
        _handler = handler;
        _compilerFactory = compilerFactory;
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
    [InlineData("https://github.com/WeihanLi/SamplesInPractice/blob/master/net7Sample/Net7Sample/ArgumentExceptionSample.cs")]
    [InlineData("https://raw.githubusercontent.com/WeihanLi/SamplesInPractice/master/net7Sample/Net7Sample/ArgumentExceptionSample.cs")]
    [InlineData("https://github.com/WeihanLi/SamplesInPractice/blob/9e3b5074f4565660f4d45adcc3dca662a9d8be00/net7Sample/Net7Sample/HttpClientJsonSample.cs")]
    [InlineData("https://gist.github.com/WeihanLi/7b4032a32f1a25c5f2d84b6955fa83f3")]
    [InlineData("https://gist.githubusercontent.com/WeihanLi/7b4032a32f1a25c5f2d84b6955fa83f3/raw/3e10a606b4121e0b7babcdf68a8fb1203c93c81c/print-date.cs")]
    public async Task RemoteScriptExecute(string fileUrl)
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _handler.Execute(new ExecOptions() { ScriptFile = fileUrl });
        Assert.Equal(0, result);
        _outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory(Skip = "AssemblyLoadContext")]
    [InlineData("Console.WriteLine(\"Hello .NET\");")]
    public async Task AssemblyLoadContextTest(string code)
    {
        var options = new ExecOptions();
        var compiler = _compilerFactory.GetCompiler(options.CompilerType);
        var result = await compiler.Compile(options, code);
        if (result.Msg.IsNotNullOrEmpty())
            _outputHelper.WriteLine(result.Msg);
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Data);

        var assemblyLoadContext = new CustomLoadContext(Guard.NotNull(result.Data).References);
        result.Data.Stream.Seek(0, SeekOrigin.Begin);
        var assembly = assemblyLoadContext.LoadFromStream(result.Data.Stream);
        Assert.NotNull(assembly);

        var executeResult = await _executor.Execute(assembly, options);
        if (executeResult.Msg.IsNotNullOrEmpty())
            _outputHelper.WriteLine(executeResult.Msg);
        Assert.True(executeResult.IsSuccess());
    }
}
