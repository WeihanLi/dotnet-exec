// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using WeihanLi.Common.Models;

namespace Exec.Services;

public class AdditionalScriptContentFetcher(HttpClient httpClient, IUriTransformer uriTransformer)
    : IAdditionalScriptContentFetcher
{
    // for test only
    internal static IAdditionalScriptContentFetcher InstanceForTest { get; }
        = new AdditionalScriptContentFetcher(new HttpClient(), new UriTransformer());

    public async Task<Result<string>> FetchContent(string script, CancellationToken cancellationToken = default)
    {
        string sourceText;
        try
        {
            if (Uri.TryCreate(script, UriKind.Absolute, out var uri) && !uri.IsFile)
            {
                var scriptUrl = uriTransformer.Transform(script);
                sourceText = await httpClient.GetStringAsync(scriptUrl, cancellationToken);
            }
            else
            {
                var isValidFilePath = !Path.GetInvalidPathChars().Any(script.Contains);
                sourceText = isValidFilePath && File.Exists(script) ? await File.ReadAllTextAsync(script, cancellationToken) : script;
            }
        }
        catch (Exception e)
        {
            return Result.Fail<string>($"Fail to fetch script content, {e}", ResultStatus.InternalError);
        }

        return Result.Success<string>(sourceText);
    }
}

public sealed class ScriptContentFetcher(HttpClient httpClient, IUriTransformer uriTransformer)
    : AdditionalScriptContentFetcher(httpClient, uriTransformer), IScriptContentFetcher
{
    public async Task<Result<string>> FetchContent(ExecOptions options)
    {
        var scriptFile = options.Script;
        const string codePrefix = "code:";
        if (scriptFile.StartsWith(codePrefix, StringComparison.Ordinal))
        {
            var code = scriptFile[codePrefix.Length..];
            if (code.EndsWith(".Dump()", StringComparison.Ordinal))
            {
                // auto fix for `Dump()`
                code = $"{code};";
            }
            return Result.Success<string>(code);
        }

        const string scriptPrefix = "script:";
        if (scriptFile.StartsWith(scriptPrefix, StringComparison.Ordinal))
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

        var replacements = new List<KeyValuePair<string, string>>();
        foreach (var line in sourceText.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var lineText = line;

            if (!line.StartsWith("//", StringComparison.Ordinal))
            {
                if (line.Length > 3 &&
                    line[0] is '#'
                    && line[1] is 'r' or 'u'
                    && line[2] is ' '
                    )
                {
                    lineText = $"//{line[1]}: {line[2..]}";
                    replacements.Add(new(line, lineText));
                }
                else
                {
                    break;
                }
            }

            // exact reference from file
            if (lineText.StartsWith("//r:", StringComparison.Ordinal)
                || lineText.StartsWith("// r:", StringComparison.Ordinal)
                || lineText.StartsWith("//reference:", StringComparison.Ordinal)
                || lineText.StartsWith("// reference:", StringComparison.Ordinal)
                       )
            {
                var reference = lineText.Split(':', 2)[1].Trim().TrimEnd(';').Trim('"');
                if (reference.IsNotNullOrEmpty())
                {
                    scriptReferences.Add(Helper.ReferenceNormalize(reference));
                }

                continue;
            }

            // exact using from file
            if (lineText.StartsWith("//u:", StringComparison.Ordinal)
                || lineText.StartsWith("// u:", StringComparison.Ordinal)
                || lineText.StartsWith("//using:", StringComparison.Ordinal)
                || lineText.StartsWith("// using:", StringComparison.Ordinal)
               )
            {
                var @using = lineText.Split(':', 2)[1].Trim().TrimEnd(';').Trim('"');
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

        foreach (var replacement in replacements)
        {
            sourceText = sourceText.Replace(replacement.Key, replacement.Value);
        }

        return Result.Success<string>(sourceText);
    }
}
