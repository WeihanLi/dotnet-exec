﻿// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

var app = WebApplication.Create();
app.Map("/", () => "Hello world");
await Task.WhenAny(app.RunAsync(), Task.Delay(500));
await app.StopAsync();
Console.WriteLine("WebServer Stopped");
