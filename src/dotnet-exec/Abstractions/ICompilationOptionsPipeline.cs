// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis.CSharp;

namespace Exec.Abstractions;

public interface ICompilationOptionsPipeline
{
    CSharpCompilationOptions Configure(CSharpCompilationOptions compilationOptions, ExecOptions options);
}

public interface ICompilationOptionsMiddleware
{
    CSharpCompilationOptions Configure(CSharpCompilationOptions compilationOptions, ExecOptions options);
}
