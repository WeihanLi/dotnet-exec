// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.AspNetCore.Http.Extensions;

Environment.SetEnvironmentVariable("ASPNETCORE_USEFORWARDEDHEADHERS_ENABLED", "true");
Environment.SetEnvironmentVariable("ASPNETCORE_HTTP_PORTS", "8500");

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
        options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
    });
var app = builder.Build();
app.Map("/", (HttpContext context) => new
{
    Url = context.Request.GetDisplayUrl()
});
await Task.WhenAny(app.RunAsync(), Task.Delay(300));
await app.StopAsync();
