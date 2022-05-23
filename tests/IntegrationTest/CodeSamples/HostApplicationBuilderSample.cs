// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace IntegrationTest.CodeSamples;

public class HostApplicationBuilderSample
{
    public static async Task MainTest()
    {
        var cts = new CancellationTokenSource(5000);
        
        var builder = Host.CreateApplicationBuilder();

        builder.Logging.AddJsonConsole(config =>
        {
            config.UseUtcTimestamp = true;
            config.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
            config.JsonWriterOptions = new System.Text.Json.JsonWriterOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = true
            };
        });

        builder.Configuration.AddJsonFile("env.json", true);

        builder.Services.AddHostedService<TestHostedService>();

        var host = builder.Build();
        await host.StartAsync(cts.Token);
    }

    private sealed class TestHostedService : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                Console.WriteLine($"{DateTime.Now}");
            }
        }
    }
}
