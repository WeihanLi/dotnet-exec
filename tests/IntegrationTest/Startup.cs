// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Exec;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTest;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICodeCompiler, SimpleCodeCompiler>();
        services.AddSingleton<ICodeExecutor, CodeExecutor>();
    }
}
