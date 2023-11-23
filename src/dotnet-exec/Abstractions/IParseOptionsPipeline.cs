// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis.CSharp;

namespace Exec.Abstractions;

public interface IParseOptionsPipeline
{
    CSharpParseOptions Configure(CSharpParseOptions parseOptions, ExecOptions options);
}

public interface IParseOptionsMiddleware
{
    CSharpParseOptions Configure(CSharpParseOptions parseOptions, ExecOptions options);
}

