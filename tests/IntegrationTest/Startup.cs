// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection.Logging;

namespace IntegrationTest;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger("dotnet-exec"));
        services.AddSingleton<ICodeCompiler, SimpleCodeCompiler>();
        services.AddSingleton<ICodeExecutor, CodeExecutor>();
        services.AddSingleton<CommandHandler>();
    }

    public void Configure(IServiceProvider provider)
    {
        XunitTestOutputLoggerProvider.Register(provider);
    }
}
