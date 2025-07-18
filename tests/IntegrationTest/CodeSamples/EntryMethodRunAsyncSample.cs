﻿// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace IntegrationTest.CodeSamples;

public class EntryMethodRunAsyncSample
{
    public static async Task RunAsync()
    {
        await Task.CompletedTask;
        Console.WriteLine("EntryMethodRunAsyncSample");
    }
}
