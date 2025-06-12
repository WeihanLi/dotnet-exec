// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace IntegrationTest.CodeSamples;

internal class FieldKeywordSample
{
    public string? Name { get; set => field = value?.Trim(); }

    public static void Main()
    {
        var sample = new FieldKeywordSample();
        sample.Name = "  Test  ";
        Console.WriteLine(sample.Name);
    }
}
