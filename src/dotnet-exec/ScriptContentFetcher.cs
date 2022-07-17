// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Models;

namespace Exec;

public interface IScriptContentFetcher
{
    Task<Result<string>> FetchContent(ExecOptions options);
}

public sealed class ScriptContentFetcher : IScriptContentFetcher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public ScriptContentFetcher(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
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

        string sourceText;
        try
        {
            if (Uri.TryCreate(scriptFile, UriKind.Absolute, out var uri) && !uri.IsFile)
            {
                var httpClient = _httpClientFactory.CreateClient(nameof(ScriptContentFetcher));
                var scriptUrl = uri.Host switch
                {
                    "github.com" => scriptFile
                        .Replace($"://{uri.Host}/", $"://raw.githubusercontent.com/")
                        .Replace("/blob/", "/")
                        .Replace("/tree/", "/"),
                    "gist.github.com" => scriptFile
                                             .Replace($"://{uri.Host}/", $"://gist.githubusercontent.com/")
                                         + "/raw",
                    _ => scriptFile
                };
                sourceText = await httpClient.GetStringAsync(scriptUrl, options.CancellationToken);
            }
            else
            {
                if (!File.Exists(scriptFile))
                {
                    _logger.LogError("The file {ScriptFile} does not exists", scriptFile);
                    return Result.Fail<string>("File path not exits");
                }

                sourceText = await File.ReadAllTextAsync(scriptFile, options.CancellationToken);
            }
        }
        catch (Exception e)
        {
            return Result.Fail<string>($"Fail to fetch script content, {e}", ResultStatus.ProcessFail);
        }

        var scriptReferences = new HashSet<string>();
        var scriptUsings = new HashSet<string>();

        foreach (var line in sourceText.Split('\n'))
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
