// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

var app = WebApplication.Create();
app.Map("/", () => "Hello world");
await Task.WhenAny(app.RunAsync(), Task.Delay(5000));
await app.StopAsync();
Console.WriteLine("WebServer Stopped");
