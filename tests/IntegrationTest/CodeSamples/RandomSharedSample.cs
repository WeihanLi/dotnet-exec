﻿// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace IntegrationTest.CodeSamples;

public class RandomSharedSample
{
    public static void Main()
    {
        Console.WriteLine(Random.Shared.Next(10, 100));
    }
}
