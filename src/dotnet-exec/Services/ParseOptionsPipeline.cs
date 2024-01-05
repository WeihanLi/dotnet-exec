// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis.CSharp;

namespace Exec.Services;

public sealed class ParseOptionsPipeline(IEnumerable<IParseOptionsMiddleware> middlewares) : IParseOptionsPipeline
{
    public CSharpParseOptions Configure(CSharpParseOptions parseOptions, ExecOptions options)
    {
        return middlewares.Aggregate(parseOptions, (current, middleware) => middleware.Configure(current, options));
    }
}
