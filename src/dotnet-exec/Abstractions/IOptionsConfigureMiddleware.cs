// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace Exec.Abstractions;

public interface IOptionsConfigureMiddleware
{
    Task Execute(ExecOptions options, Func<ExecOptions, Task> next);
}
