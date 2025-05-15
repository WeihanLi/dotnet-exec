// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Commands;
using WeihanLi.Common.Models;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace IntegrationTest;

public class IntegrationTests(
    CommandHandler handler,
    ICompilerFactory compilerFactory,
    IExecutorFactory executorFactory,
    ITestOutputHelper outputHelper
    )
{
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
    [InlineData("ForwardedHeadersSample")]
    [InlineData("EntryMethodExecuteSample")]
    [InlineData("EntryMethodRunSample")]
    [InlineData("EntryMethodRunAsyncSample")]
    [InlineData("FieldKeywordSample")]
    public async Task SamplesTestWithSimpleCompiler(string sampleFileName)
    {
        var filePath = $"{sampleFileName}.cs";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            Script = fullPath,
            Arguments = ["--hello", "world"],
            IncludeWebReferences = true,
            IncludeWideReferences = true,
            CompilerType = "simple",
            EnableSourceGeneratorSupport = sampleFileName.Contains("SourceGenerator"),
            EnablePreviewFeatures = true
        };

        var result = await handler.Execute(execOptions);
        Assert.Equal(0, result);
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
    [InlineData("ForwardedHeadersSample")]
    [InlineData("EntryMethodExecuteSample")]
    [InlineData("EntryMethodRunSample")]
    [InlineData("EntryMethodRunAsyncSample")]
    [InlineData("FieldKeywordSample")]
    public async Task SamplesTestWithWorkspaceCompiler(string sampleFileName)
    {
        var filePath = $"{sampleFileName}.cs";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            Script = fullPath,
            Arguments = ["--hello", "world"],
            IncludeWebReferences = true,
            IncludeWideReferences = true,
            CompilerType = "workspace",
            EnableSourceGeneratorSupport = sampleFileName.Contains("SourceGenerator"),
            EnablePreviewFeatures = true
        };

        var result = await handler.Execute(execOptions);
        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData("ConfigurationManagerSample")]
    [InlineData("JsonNodeSample")]
    [InlineData("LinqSample")]
    [InlineData("RandomSharedSample")]
    [InlineData("TopLevelSample")]
    [InlineData("HostApplicationBuilderSample")]
    [InlineData("DumpAssemblyInfoSample")]
    [InlineData("WebApiSample")]
    // [InlineData("EmbeddedReferenceSample")]
    [InlineData("UsingSample")]
    [InlineData("FileLocalTypeSample")]
    [InlineData("SourceGeneratorSample")]
    [InlineData("ForwardedHeadersSample")]
    [InlineData("FieldKeywordSample")]
    public async Task SamplesTestWithProjectCompiler(string sampleFileName)
    {
        var filePath = $"{sampleFileName}.cs";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            Script = fullPath,
            Arguments = ["--hello", "world"],
            IncludeWebReferences = true,
            IncludeWideReferences = true,
            CompilerType = "project",
            ExecutorType = "project",
            EnablePreviewFeatures = true
        };

        var result = await handler.Execute(execOptions);
        Assert.Equal(0, result);
    }

    [Theory(Skip = "NeedWork")]
    [InlineData(
        "https://github.com/WeihanLi/SamplesInPractice/blob/56dda58920fa9921dad50fde4a8333581541cbd2/BalabalaSample/CorrelationIdSample.cs", 
        "https://github.com/WeihanLi/SamplesInPractice/blob/56dda58920fa9921dad50fde4a8333581541cbd2/BalabalaSample/BalabalaSample.csproj"
        )]
    public async Task Issue06SampleTest(string sampleFileName, string sampleProjectFile)
    {
        var execOptions = new ExecOptions()
        {
            Script = "await CorrelationIdSample.MainTest();",
            ProjectPath = sampleProjectFile,
            AdditionalScripts = [sampleFileName],
            IncludeWebReferences = false,
            IncludeWideReferences = false,
            CompilerType = Helper.Project,
            ExecutorType = Helper.Project
        };

        var result = await handler.Execute(execOptions);
        Assert.Equal(0, result);
    }
    
    [Theory]
    [InlineData("https://github.com/WeihanLi/SamplesInPractice/blob/master/net7Sample/Net7Sample/ArgumentExceptionSample.cs")]
    [InlineData("https://raw.githubusercontent.com/WeihanLi/SamplesInPractice/master/net7Sample/Net7Sample/ArgumentExceptionSample.cs")]
    [InlineData("https://github.com/WeihanLi/SamplesInPractice/blob/9e3b5074f4565660f4d45adcc3dca662a9d8be00/net7Sample/Net7Sample/HttpClientJsonSample.cs")]
    [InlineData("https://gist.github.com/WeihanLi/7b4032a32f1a25c5f2d84b6955fa83f3")]
    [InlineData("https://gist.githubusercontent.com/WeihanLi/7b4032a32f1a25c5f2d84b6955fa83f3/raw/3e10a606b4121e0b7babcdf68a8fb1203c93c81c/print-date.cs")]
    public async Task RemoteScriptExecute(string fileUrl)
    {
        var result = await handler.Execute(new ExecOptions { Script = fileUrl });
        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData("HelloScriptSample")]
    [InlineData("ScriptReferenceSample")]
    public async Task ScriptSamplesTest(string sampleFileName)
    {
        var filePath = $"{sampleFileName}.csx";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            Script = fullPath,
            Arguments = ["--hello", "world"],
            IncludeWebReferences = true,
            IncludeWideReferences = true,
            CompilerType = Helper.Script,
            ExecutorType = Helper.Script
        };

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(execOptions);
        Assert.Equal(0, result);

        outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("code:Console.Write(\"Hello .NET\");")]
    [InlineData("code:\"Hello .NET\".Dump();")]
    [InlineData("code:\"Hello .NET\".Dump()")]
    public async Task CodeTextExecute(string code)
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(new ExecOptions() { Script = code });
        Assert.Equal(0, result);
        outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("Guid.NewGuid()")]
    public async Task ImplicitScriptTextExecute(string code)
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(new ExecOptions() { Script = code });
        Assert.Equal(0, result);
        outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("script:Console.Write(\"Hello .NET\")")]
    [InlineData("script:\"Hello .NET\"")]
    [InlineData("script:\"Hello .NET\".Dump()")]
    public async Task ScriptTextExecute(string code)
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(new ExecOptions() { Script = code });
        Assert.Equal(0, result);
        outputHelper.WriteLine(output.StandardOutput);
    }

    [Fact]
    public async Task ScriptStaticUsingTest()
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(new ExecOptions()
        {
            Script = "WriteLine(Math.PI)",
            Usings = ["static System.Console"]
        });
        Assert.Equal(0, result);
        outputHelper.WriteLine(output.StandardOutput);
    }

    [Fact]
    public async Task ScriptUsingAliasTest()
    {
        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(new ExecOptions()
        {
            Script = "MyConsole.WriteLine(Math.PI)",
            Usings = ["MyConsole = System.Console"]
        });
        Assert.Equal(0, result);
        outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("Console.WriteLine(\"Hello .NET\");")]
    [InlineData("Console.WriteLine(typeof(object).Assembly.Location);")]
    public async Task AssemblyLoadContextExecutorTest(string code)
    {
        var options = new ExecOptions();
        var compiler = compilerFactory.GetCompiler(options.CompilerType);
        var result = await compiler.Compile(options, code);
        if (result.Msg.IsNotNullOrEmpty())
            outputHelper.WriteLine(result.Msg);
        Assert.True(result.IsSuccess());
        Assert.NotNull(result.Data);
        using var output = await ConsoleOutput.CaptureAsync();
        var executor = executorFactory.GetExecutor(options.ExecutorType);
        var executeResult = await executor.Execute(Guard.NotNull(result.Data), options);
        if (executeResult.Msg.IsNotNullOrEmpty())
            outputHelper.WriteLine(executeResult.Msg);
        Assert.True(executeResult.IsSuccess());
        outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("workspace")]
    [InlineData(Helper.Project)]
    public async Task NuGetPackageTest(string compilerType)
    {
        var options = new ExecOptions()
        {
            References = ["nuget:WeihanLi.Npoi,3.0.0"],
            Usings = ["WeihanLi.Npoi"],
            Script = "CsvHelper.GetCsvText(new[]{1,2,3}).Dump();",
            CompilerType = compilerType
        };
        var result = await handler.Execute(options);
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
            References = [dllPath],
            Usings = ["WeihanLi.Npoi"],
            Script = "CsvHelper.GetCsvText(new[]{1,2,3}).Dump()"
        };
        var result = await handler.Execute(options);
        Assert.Equal(0, result);
        Assert.NotNull(output.StandardOutput);
        Assert.NotEmpty(output.StandardOutput);
        outputHelper.WriteLine(output.StandardOutput);
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
            References = [$"folder:{folder}"],
            Usings = ["WeihanLi.Npoi"],
            Script = "CsvHelper.GetCsvText(new[]{1,2,3}).Dump()"
        };
        var result = await handler.Execute(options);
        Assert.Equal(0, result);
        Assert.NotNull(output.StandardOutput);
        Assert.NotEmpty(output.StandardOutput);
        outputHelper.WriteLine(output.StandardOutput);
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
            References = [$"project:{path}"],
            Usings = ["WeihanLi.Npoi"],
            Script = "CsvHelper.GetCsvText(new[]{1,2,3}).Dump()"
        };
        var result = await handler.Execute(options);
        Assert.Equal(0, result);
        Assert.NotNull(output.StandardOutput);
        Assert.NotEmpty(output.StandardOutput);
        outputHelper.WriteLine(output.StandardOutput);
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
        var result = await handler.Execute(options);
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
            Script = fullPath,
            CompilerType = "simple"
        };
        var result = await handler.Execute(options);
        Assert.Equal(0, result);
    }

    [Theory]
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
        var result = await handler.Execute(options);
        outputHelper.WriteLine(output.StandardOutput);
        Assert.Equal(0, result);
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
        var result = await handler.Execute(options);
        Assert.Equal(expectedExitCode, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task EnvironmentExitCode(int exitCode)
    {
        var exePath = typeof(CommandHandler).Assembly.Location
            .Replace(".dll", OperatingSystem.IsWindows() ? ".exe" : "");
        var result = await CommandExecutor.ExecuteAndCaptureAsync(
            exePath, $"\"Environment.ExitCode = {exitCode};\"", cancellationToken: TestContext.Current.CancellationToken
            );
        outputHelper.WriteLine(result.StandardOut);
        outputHelper.WriteLine(result.StandardError);
        Assert.Equal(exitCode, result.ExitCode);
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
            ParserFeatures =
            [
                new("InterceptorsPreviewNamespaces", "CSharp12Sample.Generated")
            ]
        };

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(execOptions);
        Assert.Equal(0, result);

        outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory(
        Skip = "localOnly"
    )]
    [InlineData("workspace")]
    [InlineData("simple")]
    public async Task InterceptorRelativeCodePathSample(string compilerType)
    {
        var filePath = "InterceptorSample.cs";
        var relativePath = Path.Combine("CodeSamples", filePath);
        Assert.True(File.Exists(relativePath));

        var execOptions = new ExecOptions()
        {
            Script = relativePath,
            IncludeWebReferences = false,
            IncludeWideReferences = false,
            CompilerType = compilerType,
            EnableSourceGeneratorSupport = true,
            ParserFeatures =
            [
                new("InterceptorsPreviewNamespaces", "CSharp12Sample.Generated")
            ]
        };

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(execOptions);
        Assert.Equal(0, result);

        outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("workspace")]
    [InlineData("simple")]
    public async Task PreprocessorSymbolNameSample(string compilerType)
    {
        var symbolName = "SYMBOL_TEST";
        var filePath = "SymbolSample.cs";
        var relativePath = Path.Combine("CodeSamples", filePath);
        Assert.True(File.Exists(relativePath));

        var execOptions = new ExecOptions()
        {
            Script = relativePath,
            IncludeWebReferences = false,
            IncludeWideReferences = false,
            CompilerType = compilerType,
            ParserPreprocessorSymbolNames = [symbolName]
        };

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(execOptions);
        Assert.Equal(0, result);

        outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("workspace")]
    [InlineData("simple")]
    public async Task PreprocessorSymbolNameNotDefinedSample(string compilerType)
    {
        var filePath = "SymbolSample.cs";
        var relativePath = Path.Combine("CodeSamples", filePath);
        Assert.True(File.Exists(relativePath));

        var execOptions = new ExecOptions()
        {
            Script = relativePath,
            IncludeWebReferences = false,
            IncludeWideReferences = false,
            CompilerType = compilerType
        };

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(execOptions);
        Assert.NotEqual(0, result);

        outputHelper.WriteLine(output.StandardOutput);
    }

    [Fact]
    public async Task ReferenceDuplicateTest()
    {
        var code = """
        // r: nuget:WeihanLi.Common,1.0.61
        // r: nuget: WeihanLi.Common,1.0.61
        // r: nuget: WeihanLi.Common, 1.0.61
        // r: "nuget: WeihanLi.Common, 1.0.61"
        Console.WriteLine("Hello World!");
        """;
        var options = new ExecOptions()
        {
            Script = code,
            DryRun = true,
        };
        await handler.Execute(options);
        Assert.Single(options.References);
    }

    [Fact]
    public async Task ReferenceDuplicateRemoveTest()
    {
        var code = """
                   // r: nuget:WeihanLi.Common,1.0.64
                   // r: "nuget: WeihanLi.Common, 1.0.64"
                   Console.WriteLine("Hello World!");
                   """;
        var options = new ExecOptions()
        {
            Script = code,
            DryRun = true,
            References = ["-nuget: WeihanLi.Common, 1.0.64"]
        };
        await handler.Execute(options);
        Assert.Empty(options.References);
    }

    [Fact]
    public async Task ReferenceDuplicateRemoveTest2()
    {
        var code = """
                   // r: nuget:WeihanLi.Common
                   // r: "nuget: WeihanLi.Common"
                   Console.WriteLine("Hello World!");
                   """;
        var options = new ExecOptions()
        {
            Script = code,
            DryRun = true,
            References = ["- nuget: WeihanLi.Common"]
        };
        await handler.Execute(options);
        Assert.Empty(options.References);
    }

    [Theory]
    [InlineData("LinqpadExecSample")]
    [InlineData("LinqpadExecExpressionSample")]
    [InlineData("LinqpadExecProgramSample")]
    public async Task LinqpadExecTest(string sampleName)
    {
        var filePath = $"{sampleName}.linq";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            Script = fullPath,
            Arguments = ["--hello", "world"],
            CompilerType = Helper.Default
        };

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(execOptions);
        Assert.Equal(0, result);

        outputHelper.WriteLine(output.StandardOutput);
    }

    [Theory]
    [InlineData("NetpadExecSample")]
    public async Task NetPadExecTest(string sampleName)
    {
        var filePath = $"{sampleName}.netpad";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            Script = fullPath,
            Arguments = ["--hello", "world"],
            CompilerType = Helper.Default
        };

        using var output = await ConsoleOutput.CaptureAsync();
        var result = await handler.Execute(execOptions);
        Assert.Equal(0, result);

        outputHelper.WriteLine(output.StandardOutput);
    }
    
    [Theory]
    [InlineData("XunitSample")]
    public async Task TestCommandExecuteTest(string sampleName)
    {
        var filePath = $"{sampleName}.cs";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "CodeSamples", filePath);
        Assert.True(File.Exists(fullPath));

        var execOptions = new ExecOptions()
        {
            IncludeWebReferences = false,
            IncludeWideReferences = false,
        };

        var result = await TestCommand.ExecuteAsync(handler, execOptions, fullPath);
        Assert.Equal(0, result);
    }

    public static IEnumerable<TheoryDataRow<int, string>> EntryMethodWithExitCodeTestData()
    {
        yield return new TheoryDataRow<int, string>(0, """Console.WriteLine("Amazing dotnet");""");

        yield return new TheoryDataRow<int, string>(0, "return 0;");
        yield return new TheoryDataRow<int, string>(1, "return 1;");

        yield return new TheoryDataRow<int, string>(0, "return await Task.FromResult(0);");
        yield return new TheoryDataRow<int, string>(1, "return await Task.FromResult(1);");

        yield return new TheoryDataRow<int, string>(0, "return await ValueTask.FromResult(0);");
        yield return new TheoryDataRow<int, string>(1, "return await ValueTask.FromResult(1);");
    }
}
