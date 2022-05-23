// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace IntegrationTest;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug));
        services.AddSingleton(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger("dotnet-exec"));
        services.AddSingleton<ICodeCompiler, SimpleCodeCompiler>();
        services.AddSingleton<ICodeExecutor, CodeExecutor>();
        services.AddSingleton<AdvancedCodeCompiler>();
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<HttpClient>();
    }

    public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor outputHelperAccessor)
    {
        loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(outputHelperAccessor, (_, _) => true));
    }
}
