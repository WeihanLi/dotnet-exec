// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Models;
using Xunit.Abstractions;

namespace IntegrationTest;

public class IntegrationTests
{
    private readonly CommandHandler _handler;
    private readonly ICompilerFactory _compilerFactory;
    private readonly IExecutorFactory _executorFactory;
    private readonly ITestOutputHelper _outputHelper;

    public IntegrationTests(CommandHandler handler,
        ICompilerFactory compilerFactory,
        IExecutorFactory executorFactory,
        ITestOutputHelper outputHelper)
    {
        _handler = handler;
        _compilerFactory = compilerFactory;
        _executorFactory = executorFactory;
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
    [InlineData("WebApiSample")]
    [InlineData("EmbeddedReferenceSample")]
    [InlineData("UsingSample")]
    public async Task SamplesTest(string sampleFileName)
    {
        var filePath = $"{sampleFileName}.cs";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            Script = fullPath,
            Arguments = new[] { "--hello", "world" },
            IncludeWebReferences = true,
            IncludeWideReferences = true
        };

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _handler.Execute(execOptions);
        Assert.Equal(0, result);

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
        var result = await _handler.Execute(new ExecOptions() { Script = fileUrl });
        Assert.Equal(0, result);
        _outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("code:Console.Write(\"Hello .NET\");")]
    [InlineData("code:\"Hello .NET\".Dump();")]
    [InlineData("code:\"Hello .NET\".Dump()")]
    public async Task CodeTextExecute(string code)
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _handler.Execute(new ExecOptions() { Script = code });
        Assert.Equal(0, result);
        _outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("Guid.NewGuid()")]
    public async Task ImplicitScriptTextExecute(string code)
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _handler.Execute(new ExecOptions() { Script = code });
        Assert.Equal(0, result);
        _outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("script:Console.Write(\"Hello .NET\")")]
    [InlineData("script:\"Hello .NET\"")]
    [InlineData("script:\"Hello .NET\".Dump()")]
    public async Task ScriptTextExecute(string code)
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _handler.Execute(new ExecOptions() { Script = code });
        Assert.Equal(0, result);
        _outputHelper.WriteLine(output.StandardOutput);
    }

    [Fact]
    public async Task ScriptStaticUsingTest()
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _handler.Execute(new ExecOptions()
        {
            Script = "WriteLine(Math.PI)",
            Usings = new()
            {
                "static System.Console"
            }
        });
        Assert.Equal(0, result);
        _outputHelper.WriteLine(output.StandardOutput);
    }

    [Fact]
    public async Task ScriptUsingAliasTest()
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _handler.Execute(new ExecOptions()
        {
            Script = "MyConsole.WriteLine(Math.PI)",
            Usings = new()
            {
                "MyConsole = System.Console"
            }
        });
        Assert.Equal(0, result);
        _outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("Console.WriteLine(\"Hello .NET\");")]
    [InlineData("Console.WriteLine(typeof(object).Assembly.Location);")]
    public async Task AssemblyLoadContextExecutorTest(string code)
    {
        var options = new ExecOptions();
        var compiler = _compilerFactory.GetCompiler(options.CompilerType);
        var result = await compiler.Compile(options, code);
        if (result.Msg.IsNotNullOrEmpty())
            _outputHelper.WriteLine(result.Msg);
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Data);
        using var output = await ConsoleOutput.CaptureAsync();
        var executor = _executorFactory.GetExecutor(options.ExecutorType);
        var executeResult = await executor.Execute(Guard.NotNull(result.Data), options);
        if (executeResult.Msg.IsNotNullOrEmpty())
            _outputHelper.WriteLine(executeResult.Msg);
        Assert.True(executeResult.IsSuccess());
        _outputHelper.WriteLine(output.StandardOutput);
    }

    [Fact]
    public async Task NuGetPackageTest()
    {
        var options = new ExecOptions()
        {
            References = new()
            {
                "nuget:WeihanLi.Npoi,2.3.0"
            },
            Usings = new()
            {
                "WeihanLi.Npoi"
            },
            Script = "code:CsvHelper.GetCsvText(new[]{1,2,3}).Dump()"
        };
        var result = await _handler.Execute(options);
        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData("https://raw.githubusercontent.com/WeihanLi/SamplesInPractice/master/net6sample/ImplicitUsingsSample/ImplicitUsingsSample.csproj")]
    [InlineData("https://github.com/WeihanLi/SamplesInPractice/blob/master/net6sample/ImplicitUsingsSample/ImplicitUsingsSample.csproj")]
    public async Task ProjectFileTest(string projectPath)
    {
        var options = new ExecOptions()
        {
            ProjectPath = projectPath,
            Script = "Console.WriteLine(MyFile.Exists(\"appsettings.json\"));"
        };
        var result = await _handler.Execute(options);
        Assert.Equal(0, result);
    }
}
