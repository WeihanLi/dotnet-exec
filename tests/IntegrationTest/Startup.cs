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
        services.RegisterApplicationServices(new[] { "--debug" })
            .AddLogging(builder => builder.ClearProviders());
    }

    public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor outputHelperAccessor)
    {
        loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(outputHelperAccessor, (_, _) => true));
    }
}
