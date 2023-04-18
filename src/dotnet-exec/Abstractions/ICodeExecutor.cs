// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using WeihanLi.Common.Models;

namespace Exec.Abstractions;

public interface ICodeExecutor
{
    Task<Result<int>> Execute(CompileResult compileResult, ExecOptions options);
}
