// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Http;
using WeihanLi.Common.Models;

namespace Exec;

public interface IScriptContentFetcher
{
    Task<Result<string>> FetchContent(ExecOptions options);
}

public interface IAdditionalScriptContentFetcher
{
    Task<Result<string>> FetchContent(string script, CancellationToken cancellationToken = default);
}

public class AdditionalScriptContentFetcher: IAdditionalScriptContentFetcher
{
    // for test only
    internal static IAdditionalScriptContentFetcher InstanceForTest { get; } 
        = new AdditionalScriptContentFetcher(new HttpClient(new NoProxyHttpClientHandler()), new UriTransformer(), Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);

    private readonly HttpClient _httpClient;
    private readonly IUriTransformer _uriTransformer;
    private readonly ILogger _logger;

    public AdditionalScriptContentFetcher(HttpClient httpClient, IUriTransformer uriTransformer, ILogger logger)
    {
        _httpClient = httpClient;
        _uriTransformer = uriTransformer;
        _logger = logger;
    }
    
    public async Task<Result<string>> FetchContent(string script, CancellationToken cancellationToken = default)
    {
        string sourceText;
        try
        {
            if (Uri.TryCreate(script, UriKind.Absolute, out var uri) && !uri.IsFile)
            {
                var scriptUrl = _uriTransformer.Transform(script);
                sourceText = await _httpClient.GetStringAsync(scriptUrl, cancellationToken);
            }
            else
            {
                if (File.Exists(script))
                {
                    sourceText = await File.ReadAllTextAsync(script, cancellationToken);
                }
                else
                {
                    _logger.LogInformation("The file {ScriptFile} does not exists", script);
                    sourceText = script;
                }
            }
        }
        catch (Exception e)
        {
            return Result.Fail<string>($"Fail to fetch script content, {e}", ResultStatus.ProcessFail);
        }

        return Result.Success<string>(sourceText);
    }
}

public sealed class ScriptContentFetcher : AdditionalScriptContentFetcher, IScriptContentFetcher
{
    public ScriptContentFetcher(HttpClient httpClient, IUriTransformer uriTransformer, ILogger logger)
         : base(httpClient, uriTransformer, logger)
    {
    }

    public async Task<Result<string>> FetchContent(ExecOptions options)
    {
        var scriptFile = options.Script;
        const string codePrefix = "code:";
        if (scriptFile.StartsWith(codePrefix))
        {
            var code = scriptFile[codePrefix.Length..];
            if (code.EndsWith(".Dump()"))
            {
                // auto fix for `Dump()`
                code = $"{code};";
            }
            return Result.Success<string>(code);
        }

        const string scriptPrefix = "script:";
        if (scriptFile.StartsWith(scriptPrefix))
        {
            var code = scriptFile[scriptPrefix.Length..];
            options.ExecutorType = options.CompilerType = Helper.Script;
            return Result.Success<string>(code);
        }

        var sourceTextResult = await FetchContent(options.Script);
        if (sourceTextResult.Status != ResultStatus.Success)
        {
            return sourceTextResult;
        }
        if (options.Script == sourceTextResult.Data && !sourceTextResult.Data.EndsWith(';'))
        {
            options.ExecutorType = options.CompilerType = Helper.Script;
        }
        var sourceText = sourceTextResult.Data;
        Guard.NotNull(sourceText);
        
        var scriptReferences = new HashSet<string>();
        var scriptUsings = new HashSet<string>();

        foreach (var line in sourceText.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (!line.StartsWith("//"))
            {
                break;
            }

            // exact reference from file
            if (line.StartsWith("//r:")
                || line.StartsWith("// r:")
                || line.StartsWith("//reference:")
                || line.StartsWith("// reference:")
                       )
            {
                var reference = line.Split(':', 2)[1].Trim();
                if (reference.IsNotNullOrEmpty())
                {
                    scriptReferences.Add(reference);
                }

                continue;
            }

            // exact using from file
            if (line.StartsWith("//u:")
                || line.StartsWith("// u:")
                || line.StartsWith("//using:")
                || line.StartsWith("// using:")
               )
            {
                var @using = line.Split(':', 2)[1].Trim();
                if (@using.IsNotNullOrEmpty())
                {
                    scriptUsings.Add(@using);
                }
            }
        }

        if (scriptReferences.Count > 0)
        {
            if (options.References.HasValue())
            {
                foreach (var reference in options.References)
                {
                    scriptReferences.Add(reference);
                }
            }
            options.References = scriptReferences;
        }
        if (scriptUsings.Count > 0)
        {
            if (options.Usings.HasValue())
            {
                foreach (var @using in options.Usings)
                {
                    scriptUsings.Add(@using);
                }
            }
            options.Usings = scriptUsings;
        }

        return Result.Success<string>(sourceText);
    }
}
