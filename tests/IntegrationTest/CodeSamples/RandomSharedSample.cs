// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace IntegrationTest.CodeSamples;

public class RandomSharedSample
{
    public static void MainTest()
    {
        Console.WriteLine(Random.Shared.Next(10, 100));
    }
}
