﻿// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis.CSharp;

namespace Exec.Services.Middleware;

public sealed class FeaturesParserOptionsMiddleware : IParseOptionsMiddleware
{
    public CSharpParseOptions Configure(CSharpParseOptions parseOptions, ExecOptions options)
    {
        return options.ParserFeatures.IsNullOrEmpty()
                ? parseOptions
                : parseOptions.WithFeatures(options.ParserFeatures)
            ;
    }
}
