// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Exec.Contracts;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WeihanLi.Common.Models;

namespace Exec;

public sealed class CommandHandler : ICommandHandler
{
    private readonly ILogger _logger;
    private readonly ICompilerFactory _compilerFactory;
    private readonly IExecutorFactory _executorFactory;
    private readonly IUriTransformer _uriTransformer;
    private readonly IScriptContentFetcher _scriptContentFetcher;
    private readonly IConfigProfileManager _profileManager;

    public CommandHandler(ILogger logger,
        ICompilerFactory compilerFactory,
        IExecutorFactory executorFactory,
        IUriTransformer uriTransformer,
        IScriptContentFetcher scriptContentFetcher,
        IConfigProfileManager profileManager)
    {
        _logger = logger;
        _compilerFactory = compilerFactory;
        _executorFactory = executorFactory;
        _uriTransformer = uriTransformer;
        _scriptContentFetcher = scriptContentFetcher;
        _profileManager = profileManager;
    }

    public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var parseResult = context.ParseResult;

        // 1. options binding
        var options = new ExecOptions();
        var profileName = parseResult.GetValueForOption(ExecOptions.ConfigProfileOption);
        ConfigProfile? profile = null;
        if (profileName.IsNotNullOrEmpty())
        {
            profile = await _profileManager.GetProfile(profileName);
            if (profile is null)
            {
                _logger.LogDebug("The config profile({profileName}) not found", profileName);
            }
        }
        options.BindCommandLineArguments(parseResult, profile);
        options.CancellationToken = context.GetCancellationToken();
        if (options.DebugEnabled)
        {
            _logger.LogDebug("options: {options}", JsonSerializer.Serialize(options, new JsonSerializerOptions()
            {
                Converters =
                {
                    new JsonStringEnumConverter()
                },
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
            }));
        }

        return await Execute(options);
    }

    public async Task<int> Execute(ExecOptions options)
    {
        if (options.Script.IsNullOrWhiteSpace())
        {
            _logger.LogError("The file {ScriptFile} can not be empty", options.Script);
            return -1;
        }

        // exact reference and usings from project file
        if (options.ProjectPath.IsNotNullOrEmpty())
        {
            var startTime = Stopwatch.GetTimestamp();
            // https://learn.microsoft.com/en-us/dotnet/standard/linq/linq-xml-overview
            var projectPath = _uriTransformer.Transform(options.ProjectPath);
            var element = XElement.Load(projectPath);
            var itemGroups = element.Descendants("ItemGroup").ToArray();
            if (itemGroups.HasValue())
            {
                var propertyRegex = new Regex(@"\$\((?<propertyName>\w+)\)", RegexOptions.Compiled);
                var usingElements = itemGroups.SelectMany(x => x.Descendants("Using"));
                foreach (var usingElement in usingElements)
                {
                    var usingText = usingElement.Attribute("Include")?.Value;
                    if (usingText.IsNotNullOrEmpty())
                    {
                        if (usingText.Contains("$("))
                        {
                            var propertyMatch = false;
                            var match = propertyRegex.Match(usingText);
                            if (match.Success)
                            {
                                var propertyValue = element.Descendants("PropertyGroup")
                                    .Descendants(match.Groups["propertyName"].Value)
                                    .FirstOrDefault()?.Value;
                                if (propertyValue != null)
                                {
                                    usingText = usingText.Replace(match.Value, propertyValue);
                                    propertyMatch = !usingText.Contains("$(");
                                }
                                else
                                {
                                    propertyMatch = false;
                                }
                            }

                            if (!propertyMatch) continue;
                        }
                        if (usingElement.Attribute("Static")?.Value == "true")
                        {
                            usingText = $"static {usingText}";
                        }

                        var alias = usingElement.Attribute("Alias")?.Value;
                        if (alias.IsNotNullOrEmpty())
                        {
                            usingText = $"{alias} = {usingText}";
                        }
                    }
                    else
                    {
                        usingText = usingElement.Attribute("Remove")?.Value;
                        if (usingText.IsNotNullOrEmpty())
                        {
                            usingText = $"- {usingText}";
                        }
                    }

                    if (usingText.IsNotNullOrEmpty())
                    {
                        options.Usings.Add(usingText);
                    }
                }

                var packageReferenceElements = itemGroups.SelectMany(x => x.Descendants("PackageReference"));
                foreach (var packageReferenceElement in packageReferenceElements)
                {
                    var packageIdAttribute = packageReferenceElement.Attribute("Include") ?? packageReferenceElement.Attribute("Update");
                    if (packageIdAttribute is null) continue;
                    var packageId = packageIdAttribute.Value;
                    var packageVersion = packageReferenceElement.Attribute("Version")?.Value ?? string.Empty;
                    if (packageVersion.Contains("$("))
                    {
                        var newPackageVersion = string.Empty;
                        var match = propertyRegex.Match(packageVersion);
                        if (match.Success)
                        {
                            var propertyValue = element.Descendants("PropertyGroup")
                                .Descendants(match.Groups["propertyName"].Value)
                                .FirstOrDefault()?.Value;
                            if (propertyValue != null)
                            {
                                var packageVersionUpdated = packageVersion.Replace(match.Value, propertyValue);
                                if (!packageVersionUpdated.Contains("$("))
                                {
                                    newPackageVersion = packageVersionUpdated;
                                }
                            }
                        }
                        if (newPackageVersion.IsNullOrWhiteSpace())
                        {
                            packageVersion = newPackageVersion;
                        }
                    }

                    var reference =
                        $"nuget: {packageId}{(string.IsNullOrEmpty(packageVersion) ? "" : $", {packageVersion}")}";
                    options.References.Add(reference);
                }

                if (File.Exists(projectPath))
                {
                    var projectDirectory = Path.GetFullPath(Guard.NotNullOrEmpty(Path.GetDirectoryName(projectPath)));
                    var projectReferenceElements = itemGroups.SelectMany(x => x.Descendants("ProjectReference"));
                    foreach (var projectReferenceElement in projectReferenceElements)
                    {
                        var includeAttribute = projectReferenceElement.Attribute("Include");
                        if (includeAttribute?.Value is null) continue;

                        var referenceProjectPath = includeAttribute.Value;
                        var referenceProjectFullPath = Path.GetFullPath(referenceProjectPath, projectDirectory);
                        if (!File.Exists(referenceProjectPath))
                            continue;

                        var projectReference = $"project: {referenceProjectFullPath}";
                        options.References.Add(projectReference);
                    }

                }
            }
            var endTime = Stopwatch.GetTimestamp();
            var duration = ProfilerHelper.GetElapsedTime(startTime, endTime);
            _logger.LogDebug("Exact info from project file elapsed time: {duration}", duration);
        }

        // fetch script
        var fetchResult = await _scriptContentFetcher.FetchContent(options);
        if (!fetchResult.IsSuccess())
        {
            _logger.LogError(fetchResult.Msg);
            return -1;
        }

        // Cleanup references
        var referenceToRemoved = options.References.Where(r => r.StartsWith('-')).ToArray();
        foreach (var reference in referenceToRemoved)
        {
            var referenceToRemove = reference[1..];
            options.References.Remove(referenceToRemove);
            options.References.Remove(reference);
        }

        _logger.LogDebug("CompilerType: {CompilerType} \nExecutorType: {ExecutorType} \nReferences: {References} \nUsings: {Usings}",
            options.CompilerType,
            options.ExecutorType,
            options.References.StringJoin(";"),
            options.Usings.StringJoin(";"));

        // compile assembly
        var sourceText = fetchResult.Data;
        var compiler = _compilerFactory.GetCompiler(options.CompilerType);
        var compileStartTime = Stopwatch.GetTimestamp();
        var compileResult = await compiler.Compile(options, sourceText);
        var compileEndTime = Stopwatch.GetTimestamp();
        var compileElapsed = ProfilerHelper.GetElapsedTime(compileStartTime, compileEndTime);
        _logger.LogDebug("Compile elapsed: {elapsed}", compileElapsed);

        if (!compileResult.IsSuccess())
        {
            _logger.LogError($"Compile error:{Environment.NewLine}{compileResult.Msg}");
            return -2;
        }

        Guard.NotNull(compileResult.Data);
        // execute
        var executor = _executorFactory.GetExecutor(options.ExecutorType);
        try
        {
            var executeStartTime = Stopwatch.GetTimestamp();
            var executeResult = await executor.Execute(compileResult.Data, options);
            if (!executeResult.IsSuccess())
            {
                _logger.LogError($"Execute error:{Environment.NewLine}{executeResult.Msg}");
                return -3;
            }
            var elapsed = ProfilerHelper.GetElapsedTime(executeStartTime);
            _logger.LogDebug("Execute elapsed: {elapsed}", elapsed);

            // wait for console flush
            await Console.Out.FlushAsync();

            return executeResult.Data;
        }
        catch (OperationCanceledException) when (options.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Execution cancelled...");
            return -998;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Execute code exception");
            return -999;
        }
    }
}
