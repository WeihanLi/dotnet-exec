// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;

const string testKey = "test";

var configuration = new ConfigurationManager();
Console.WriteLine(configuration[testKey]);

configuration.AddInMemoryCollection(new Dictionary<string, string>()
{
    { testKey, "test" }
});
Console.WriteLine(configuration[testKey]);
