// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Models;

namespace Exec.Abstractions;

public interface ICodeCompiler
{
    Task<Result<CompileResult>> Compile(ExecOptions options, string? code = null);
}
