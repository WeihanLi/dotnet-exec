// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Text;
using WeihanLi.Common.Extensions;
using WeihanLi.Common.Models;

namespace Exec.Services;

internal sealed class ProjectCodeCompilerExecutor(IAdditionalScriptContentFetcher contentFetcher, ILogger logger) : ICodeCompiler, ICodeExecutor
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
        string projectFileContent;
        if (options.ProjectPath.IsNullOrEmpty())
        {
            projectFileContent = await GetImplicitProjectFile(options, tempFolderPath);
        }
        else
        {
            if (File.Exists(options.ProjectPath))
            {
                projectFileContent = await File.ReadAllTextAsync(options.ProjectPath, options.CancellationToken);
            }
            else
            {
                var projectContent = (await contentFetcher.FetchContent(options.ProjectPath)).Data;
                if (projectContent.IsNullOrEmpty())
                {
                    return Result.Fail<CompileResult>("Invalid project file provided");
                }
                
                projectFileContent = projectContent;
            }

            await foreach (var script in GetScriptPathList(options, tempFolderPath))
            {
                logger.LogDebug("Script info: {ScriptPath}", script);
            }
        }

        await File.WriteAllTextAsync(projectFilePath, projectFileContent);
        logger.LogDebug("Project file created. Path: {ProjectPath}, {Content}",
            projectFilePath, projectFileContent);
        
        var outputDir = Path.Combine(tempFolderPath, "output");
        // dotnet build
        var dotnetPath = ApplicationHelper.GetDotnetPath();
        Guard.NotNull(dotnetPath);
        var buildResult = await CommandExecutor.ExecuteAndCaptureAsync(dotnetPath,
            $"build {projectFilePath} -c {options.Configuration} -o {outputDir}");
        if (buildResult.ExitCode != 0)
        {
            Directory.Delete(tempFolderPath, true);
            return Result.Fail<CompileResult>($"Build failed with exit code {buildResult.ExitCode},{buildResult.StandardOut}\n{buildResult.StandardError}", ResultStatus.InternalError);
        }
        
        var result = new CompileResult(null!, null!, null!);
        result.SetProperty(nameof(execId), execId);
        result.SetProperty(nameof(outputDir), outputDir);
        logger.LogDebug("Project code compile succeeded. ExecId: {ExecId}, output: {OutputDir}", execId, outputDir);

        if (options.DryRun)
        {
            Directory.Delete(tempFolderPath, true);
        }
        return Result.Success(result);
    }

    public async Task<Result<int>> Execute(CompileResult compileResult, ExecOptions options)
    {
        var outputDir = compileResult.GetProperty<string>("outputDir");
        Guard.NotNull(outputDir);
        var outputDllPath = Path.Combine(outputDir, "exec.dll");
        var dotnetPath = ApplicationHelper.GetDotnetPath();
        Guard.NotNull(dotnetPath);
        var exitCode = await CommandExecutor.ExecuteAndOutputAsync(dotnetPath, $"{outputDllPath}", workingDirectory: outputDir);
        Directory.GetParent(outputDir)?.Delete(true);
        if (exitCode != 0)
        {
            return Result.Fail(
                $"Execute failed with exit code {exitCode} ",
                ResultStatus.InternalError, exitCode);
        }
        
        return Result.Success(0);
    }

    private async Task<string> GetImplicitProjectFile(ExecOptions options, string tempFolderPath)
    {
        var sdk = options.IncludeWebReferences ? "Microsoft.NET.Sdk.Web" : "Microsoft.NET.Sdk";
        var targetFramework = options.TargetFramework;

        var projectFileBuilder = new StringBuilder($"""
                                                      <Project Sdk="{sdk}">
                                                        <PropertyGroup>
                                                          <TargetFramework>{targetFramework}</TargetFramework>
                                                          <Nullable>annotations</Nullable>
                                                          <EnableDefaultItems>false</EnableDefaultItems>
                                                          <OutputType>Exe</OutputType>
                                                      
                                                      """);
        // build properties
        if (options.EnablePreviewFeatures)
        {
            projectFileBuilder.Append("    <LangVersion>preview</LangVersion>");
        }

        // build PropertyGroup end
        projectFileBuilder.AppendLine("""
                                        </PropertyGroup>
                                        <ItemGroup>
                                      """
            );
        
        // build items
        await foreach (var scriptPath in GetScriptPathList(options, tempFolderPath))
        {
            var item = $"    <Compile Include=\"{scriptPath}\" />";
            projectFileBuilder.AppendLine(item);
        }

        if (options.IncludeWideReferences)
        {
            projectFileBuilder.AppendLine("    <PackageReference Include=\"WeihanLi.Common\" Version=\"*-*\" />");
        }
        
        // build ItemGroup end
        projectFileBuilder.Append("""
                                    </ItemGroup>
                                  </Project>
                                  """);
        return projectFileBuilder.ToString();
    }

    private async IAsyncEnumerable<string> GetScriptPathList(ExecOptions options, string tempFolderPath)
    {
        yield return GetPath(tempFolderPath, options.Script);

        var usingText = Helper.GetGlobalUsingsCodeText(options);
        yield return GetPath(tempFolderPath, usingText, "_GlobalUsings");
        
        if (options.AdditionalScripts is not { Count: > 0 }) yield break;
        
        foreach (var script in options.AdditionalScripts)
        {
            yield return GetPath(tempFolderPath, (await contentFetcher.FetchContent(script, options.CancellationToken)).Data ?? script);
        }
    }

    private static string GetPath(string tempFolderPath, string script, string? fileName = null)
    {
        if (File.Exists(script))
        {
            return Path.GetRelativePath(tempFolderPath,script);
        }

        var scriptFileName = fileName ?? Path.GetRandomFileName();
        var scriptPath = Path.Combine(tempFolderPath, $"{scriptFileName}.cs");
        File.WriteAllText(scriptPath, script);
        return Path.GetRelativePath(tempFolderPath, scriptPath);
    }
}
