// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection;

namespace IntegrationTest;

public static class Startup
{
    private static readonly string[] DebugArgs = ["--debug"];

    public static void ConfigureServices(IServiceCollection services)
    {
        services.RegisterApplicationServices(DebugArgs);
    }

    public static void Configure(ITestOutputHelperAccessor outputHelperAccessor, IRefResolver refResolver)
    {
        refResolver.DisableCache = true;
        InvokeHelper.OnInvokeException = ex => outputHelperAccessor.Output?.WriteLine(ex.ToString());
    }
}
