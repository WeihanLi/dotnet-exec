// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Globalization;

namespace Exec.Services;

public sealed class ExecutorFactory : IExecutorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ExecutorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ICodeExecutor GetExecutor(string executorType)
    {
        return executorType.ToLower(CultureInfo.InvariantCulture) switch
        {
            Helper.Script => _serviceProvider.GetRequiredService<CSharpScriptCompilerExecutor>(),
            _ => _serviceProvider.GetRequiredService<DefaultCodeExecutor>()
        };
    }
}
