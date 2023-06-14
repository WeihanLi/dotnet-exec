// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace IntegrationTest;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.RegisterApplicationServices(new[] { "--debug" });
        services.AddLogging(lb => lb.AddXunitOutput(_ => { }));
    }

    public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor outputHelperAccessor, IRefResolver refResolver)
    {
        refResolver.DisableCache = true;
    }
}
