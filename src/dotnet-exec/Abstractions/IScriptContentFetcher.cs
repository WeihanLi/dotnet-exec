// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

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
