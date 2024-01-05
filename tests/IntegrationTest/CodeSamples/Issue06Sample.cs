// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using WeihanLi.Common.Logging.Serilog;

namespace BalabalaSample;

public class Issue06Sample
{
    public static async Task Main()
    {
        SerilogHelper.LogInit(configuration =>
        {
            configuration.WriteTo.Console();
        });
        
        var serviceCollection = new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog())
            ;
        await using var provider = serviceCollection.BuildServiceProvider();
        provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("test")
            .LogInformation("Hello 1234");
    }
}
