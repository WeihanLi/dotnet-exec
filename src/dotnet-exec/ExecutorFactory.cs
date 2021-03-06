// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace Exec;

public interface IExecutorFactory
{
    ICodeExecutor GetExecutor(string executorType);
}

public sealed class ExecutorFactory : IExecutorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ExecutorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ICodeExecutor GetExecutor(string executorType)
    {
        return executorType.ToLower() switch
        {
            Helper.Script => _serviceProvider.GetRequiredService<CSharpScriptCompilerExecutor>(),
            _ => _serviceProvider.GetRequiredService<DefaultCodeExecutor>()
        };
    }
}
