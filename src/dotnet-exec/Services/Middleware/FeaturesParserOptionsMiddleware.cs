// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis.CSharp;

namespace Exec.Services.Middleware;

public sealed class FeaturesParserOptionsMiddleware : IParseOptionsMiddleware
{
    private const string FileProgramFeatureName = "FileBasedProgram";
    public CSharpParseOptions Configure(CSharpParseOptions parseOptions, ExecOptions options)
    {
        if (options.ParserFeatures.IsNullOrEmpty())
            return parseOptions.WithFeatures([new KeyValuePair<string, string>(FileProgramFeatureName, string.Empty)]);

        if (options.ParserFeatures.Any(x =>
                FileProgramFeatureName.Equals(FileProgramFeatureName, StringComparison.Ordinal)))
        {
            return parseOptions.WithFeatures(options.ParserFeatures);
        }

        return parseOptions.WithFeatures([
            ..options.ParserFeatures, new KeyValuePair<string, string>(FileProgramFeatureName, string.Empty)
        ]);
    }
}
