// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Globalization;

namespace Exec.Services;

public sealed class ExecutorFactory(IServiceProvider serviceProvider) : IExecutorFactory
{
    public ICodeExecutor GetExecutor(string executorType)
    {
        return executorType.ToLower(CultureInfo.InvariantCulture) switch
        {
            Helper.Script => serviceProvider.GetRequiredService<CSharpScriptCompilerExecutor>(),
            _ => serviceProvider.GetRequiredService<DefaultCodeExecutor>()
        };
    }
}
