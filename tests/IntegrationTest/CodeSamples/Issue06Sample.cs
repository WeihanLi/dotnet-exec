// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

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
