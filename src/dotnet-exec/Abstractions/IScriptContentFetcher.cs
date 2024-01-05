// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using WeihanLi.Common.Models;

namespace Exec.Abstractions;

public interface IScriptContentFetcher
{
    Task<Result<string>> FetchContent(ExecOptions options);
}

public interface IAdditionalScriptContentFetcher
{
    Task<Result<string>> FetchContent(string script, CancellationToken cancellationToken = default);
}
