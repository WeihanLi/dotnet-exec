// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

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
    [InlineData("FileLocalTypeSample")]
    [InlineData("SourceGeneratorSample")]
    public async Task SamplesTestWithSimpleCompiler(string sampleFileName)
    {
        var filePath = $"{sampleFileName}.cs";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            Script = fullPath,
            Arguments = new[] { "--hello", "world" },
            IncludeWebReferences = true,
            IncludeWideReferences = true,
            CompilerType = Helper.Default,
            EnableSourceGeneratorSupport = sampleFileName.Contains("SourceGenerator")
        };

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _handler.Execute(execOptions);
        Assert.Equal(0, result);

        _outputHelper.WriteLine(output.StandardOutput);
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
    [InlineData("FileLocalTypeSample")]
    [InlineData("SourceGeneratorSample")]
    public async Task SamplesTestWithWorkspaceCompiler(string sampleFileName)
    {
        var filePath = $"{sampleFileName}.cs";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            Script = fullPath,
            Arguments = new[] { "--hello", "world" },
            IncludeWebReferences = true,
            IncludeWideReferences = true,
            CompilerType = "workspace",
            EnableSourceGeneratorSupport = sampleFileName.Contains("SourceGenerator")
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
                "nuget:WeihanLi.Npoi,2.4.2"
            },
            Usings = new()
            {
                "WeihanLi.Npoi"
            },
            Script = "CsvHelper.GetCsvText(new[]{1,2,3}).Dump();"
        };
        var result = await _handler.Execute(options);
        Assert.Equal(0, result);
    }

    [Theory(
        Skip = "localOnly"
        )]
    [InlineData(@"C:\projects\sources\WeihanLi.Npoi\src\WeihanLi.Npoi\bin\Release\net6.0\WeihanLi.Npoi.dll")]
    [InlineData(@".\out\WeihanLi.Npoi.dll")]
    public async Task LocalDllReferenceTest(string dllPath)
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var options = new ExecOptions()
        {
            References = new()
            {
                dllPath
            },
            Usings = new()
            {
                "WeihanLi.Npoi"
            },
            Script = "CsvHelper.GetCsvText(new[]{1,2,3}).Dump()"
        };
        var result = await _handler.Execute(options);
        Assert.Equal(0, result);
        Assert.NotNull(output.StandardOutput);
        Assert.NotEmpty(output.StandardOutput);
        _outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory(
        Skip = "localOnly"
    )]
    [InlineData(@"C:\projects\sources\WeihanLi.Npoi\src\WeihanLi.Npoi\bin\Release\net6.0")]
    [InlineData(@".\out")]
    public async Task LocalFolderReferenceTest(string folder)
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var options = new ExecOptions()
        {
            References = new()
            {
                $"folder:{folder}"
            },
            Usings = new()
            {
                "WeihanLi.Npoi"
            },
            Script = "CsvHelper.GetCsvText(new[]{1,2,3}).Dump()"
        };
        var result = await _handler.Execute(options);
        Assert.Equal(0, result);
        Assert.NotNull(output.StandardOutput);
        Assert.NotEmpty(output.StandardOutput);
        _outputHelper.WriteLine(output.StandardOutput);
    }


    [Theory(
        Skip = "localOnly"
    )]
    [InlineData(@"C:\projects\sources\WeihanLi.Npoi\src\WeihanLi.Npoi")]
    [InlineData(@"C:\projects\sources\WeihanLi.Npoi\src\WeihanLi.Npoi\WeihanLi.Npoi.csproj")]
    public async Task LocalProjectReferenceTest(string path)
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var options = new ExecOptions()
        {
            References = new()
            {
                $"project:{path}"
            },
            Usings = new()
            {
                "WeihanLi.Npoi"
            },
            Script = "CsvHelper.GetCsvText(new[]{1,2,3}).Dump()"
        };
        var result = await _handler.Execute(options);
        Assert.Equal(0, result);
        Assert.NotNull(output.StandardOutput);
        Assert.NotEmpty(output.StandardOutput);
        _outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("https://raw.githubusercontent.com/WeihanLi/SamplesInPractice/master/net6sample/ImplicitUsingsSample/ImplicitUsingsSample.csproj")]
    [InlineData("https://github.com/WeihanLi/SamplesInPractice/blob/master/net6sample/ImplicitUsingsSample/ImplicitUsingsSample.csproj")]
    public async Task ProjectFileTest(string projectPath)
    {
        var options = new ExecOptions()
        {
            ProjectPath = projectPath,
            Script = "System.Console.WriteLine(MyFile.Exists(\"appsettings.json\"));"
        };
        var result = await _handler.Execute(options);
        Assert.Equal(0, result);
    }

    [Theory]
    // [InlineData("https://github.com/WeihanLi/SamplesInPractice/blob/56dda58920fa9921dad50fde4a8333581541cbd2/BalabalaSample/BalabalaSample.csproj")]
    [InlineData("https://github.com/WeihanLi/SamplesInPractice/blob/22dc739b74ea6e58ae06986d518544c9ef4d8d8e/BalabalaSample/BalabalaSample.csproj")]
    [InlineData("Issue06Sample.csproj")]
    public async Task ProjectFileWithPropertyTest(string projectPath)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", "Issue06Sample.cs");
        Assert.True(File.Exists(fullPath));

        var fullProjectPath =
            projectPath.StartsWith("https://", StringComparison.Ordinal)
            ? projectPath
            : Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", projectPath)
            ;

        var options = new ExecOptions()
        {
            ProjectPath = fullProjectPath,
            IncludeWideReferences = false,
            // Script = "https://github.com/WeihanLi/SamplesInPractice/blob/56dda58920fa9921dad50fde4a8333581541cbd2/BalabalaSample/CorrelationIdSample.cs"
            Script = fullPath,
            CompilerType = "simple"
        };
        var result = await _handler.Execute(options);
        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData("6.0")]
    [InlineData("7.0")]
    [InlineData("8.0")]
    public async Task TargetFrameworkTest(string version)
    {
        var targetFramework = $"net{version}";
        var options = new ExecOptions()
        {
            TargetFramework = targetFramework,
            IncludeWideReferences = false,
            Script = "Console.Write(typeof(System.Text.Json.JsonSerializer).Assembly.GetName().Version.ToString(2));"
        };
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _handler.Execute(options);
        Assert.Equal(0, result);
        _outputHelper.WriteLine(output.StandardOutput);
        // Assert.Equal(version, output.StandardOutput);
    }

    [Theory]
    [MemberData(nameof(EntryMethodWithExitCodeTestData))]
    public async Task EntryMethodWithExitCode(int expectedExitCode, string code)
    {
        var options = new ExecOptions()
        {
            Script = code
        };
        var result = await _handler.Execute(options);
        Assert.Equal(expectedExitCode, result);
    }

    [Theory(
        Skip = "localOnly"
        )]
    [InlineData("workspace")]
    [InlineData("simple")]
    public async Task InterceptorSample(string compilerType)
    {
        var filePath = "InterceptorSample.cs";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            Script = fullPath,
            IncludeWebReferences = false,
            IncludeWideReferences = false,
            CompilerType = compilerType,
            EnableSourceGeneratorSupport = true,
            ParserFeatures = new KeyValuePair<string, string>[]
            {
                new("InterceptorsPreviewNamespaces", "CSharp12Sample.Generated")
            }
        };

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await _handler.Execute(execOptions);
        Assert.Equal(0, result);

        _outputHelper.WriteLine(output.StandardOutput);
    }

    public static IEnumerable<object[]> EntryMethodWithExitCodeTestData()
    {
        yield return new object[] { 0, @"Console.WriteLine(""Amazing dotnet"");" };

        yield return new object[] { 0, "return 0;" };
        yield return new object[] { 1, "return 1;" };

        yield return new object[] { 0, "return await Task.FromResult(0);" };
        yield return new object[] { 1, "return await Task.FromResult(1);" };

        yield return new object[] { 0, "return await ValueTask.FromResult(0);" };
        yield return new object[] { 1, "return await ValueTask.FromResult(1);" };
    }
}
