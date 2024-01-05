// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.Configuration;

const string testKey = "test";

var configuration = new ConfigurationManager();
Console.WriteLine(configuration[testKey]);

configuration.AddInMemoryCollection(new Dictionary<string, string?>()
{
    { testKey, "test" }
});
Console.WriteLine(configuration[testKey]);
