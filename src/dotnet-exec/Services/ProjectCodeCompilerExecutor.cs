// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Text;
using WeihanLi.Common.Extensions;
using WeihanLi.Common.Models;

namespace Exec.Services;

internal sealed class ProjectCodeCompilerExecutor : ICodeCompiler, ICodeExecutor
{
    public async Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null)
    {
        var execId = Guid.NewGuid().ToString();
        var tempFolderPath = Path.Combine(Path.GetTempPath(), "dotnet-exec", execId);
        if (!Directory.Exists(tempFolderPath))
        {
            Directory.CreateDirectory(tempFolderPath);
        }
        var projectFilePath = Path.Combine(tempFolderPath, "exec.csproj");
        var projectFileContent = GetImplicitProjectFile(options, tempFolderPath);
        await File.WriteAllTextAsync(projectFilePath, projectFileContent);
        var outputDir = Path.Combine(tempFolderPath, "output");
        // dotnet build
        var dotnetPath = ApplicationHelper.GetDotnetPath();
        Guard.NotNull(dotnetPath);
        var buildResult = await CommandExecutor.ExecuteAndCaptureAsync(dotnetPath,
            $"build {projectFilePath} -c {options.Configuration} -o {outputDir}");
        var result = new CompileResult(null!, null!, null!);
        if (buildResult.ExitCode != 0)
        {
            return Result.Fail<CompileResult>($"Build failed with exit code {buildResult.ExitCode},{buildResult.StandardOut}\n{buildResult.StandardError}", ResultStatus.InternalError);
        }
        
        result.SetProperty(nameof(execId), execId);
        result.SetProperty(nameof(outputDir), outputDir);
        return Result.Success(result);
    }

    public async Task<Result<int>> Execute(CompileResult compileResult, ExecOptions options)
    {
        var outputDir = compileResult.GetProperty<string>("outputDir");
        Guard.NotNull(outputDir);
        var outputDllPath = Path.Combine(outputDir, "exec.dll");
        var dotnetPath = ApplicationHelper.GetDotnetPath();
        Guard.NotNull(dotnetPath);
        var result = await CommandExecutor.ExecuteAndCaptureAsync(dotnetPath, $"{outputDllPath}", outputDir);
        if (result.ExitCode != 0)
        {
            return Result.Fail(
                $"Execute failed with exit code {result.ExitCode} \n{result.StandardOut} \n{result.StandardError} ",
                ResultStatus.InternalError, result.ExitCode);
        }
        
        return Result.Success(result.ExitCode);
    }

    private static string GetImplicitProjectFile(ExecOptions options, string tempFolderPath)
    {
        var sdk = options.IncludeWebReferences ? "Microsoft.NET.Sdk.Web" : "Microsoft.NET.Sdk";
        var targetFramework = options.TargetFramework;

        var scriptPathList = GetScriptPathList(options, tempFolderPath).ToArray();

        var projectFileBuilder = new StringBuilder($"""
                                                      <Project Sdk="{sdk}">
                                                        <PropertyGroup>
                                                          <TargetFramework>{targetFramework}</TargetFramework>
                                                          <Nullable>annotations</Nullable>
                                                          <EnabledDefaultItems>false</EnabledDefaultItems>
                                                        </PropertyGroup>
                                                        <ItemGroup>
                                                        
                                                      """);
        foreach (var scriptPath in scriptPathList)
        {
            var item = $"    <Compile Include=\"{scriptPath}\" />";
            projectFileBuilder.AppendLine(item);
        }
        projectFileBuilder.Append("""
                                    </ItemGroup>
                                  </Project>
                                  """);
        return projectFileBuilder.ToString();
    }

    private static IEnumerable<string> GetScriptPathList(ExecOptions options, string tempFolderPath)
    {
        yield return GetPath(tempFolderPath, options.Script);
        if (options.AdditionalScripts is not { Count: > 0 }) yield break;
        
        foreach (var script in options.AdditionalScripts)
        {
            yield return GetPath(tempFolderPath, script);
        }
    }

    private static string GetPath(string tempFolderPath, string script)
    {
        if (File.Exists(script))
        {
            return Path.GetRelativePath(tempFolderPath,script);
        }

        var scriptPath = $"{Path.GetRandomFileName()}.cs";
        File.WriteAllText(scriptPath, script);
        return scriptPath;
    }
}
