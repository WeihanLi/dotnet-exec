﻿// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Abstractions;

public interface IOptionsConfigurePipeline
{
    Task Execute(ExecOptions options);
}

public interface IOptionsPreConfigurePipeline
{
    Task Execute(ExecOptions options);
}

public interface IOptionsConfigureMiddleware : IAsyncPipelineMiddleware<ExecOptions>
{
}

public interface IOptionsPreConfigureMiddleware : IAsyncPipelineMiddleware<ExecOptions>
{
}
