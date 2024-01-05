// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis.CSharp;

namespace Exec.Services;

public sealed class CompilationOptionsPipeline(IEnumerable<ICompilationOptionsMiddleware> middlewares) : ICompilationOptionsPipeline
{
    public CSharpCompilationOptions Configure(CSharpCompilationOptions compilationOptions, ExecOptions options)
    {
        return middlewares.Aggregate(compilationOptions, (current, middleware) => middleware.Configure(current, options));
    }
}
