// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace IntegrationTest.CodeSamples;

public class XunitSample
{
    [Fact]
    public void AddTest()
    {
        Assert.Equal(3, Add(1, 2));
    }
    
    [Theory]
    [InlineData(1, 2, 3)]
    public void AddTestByTheory(int a, int b, int expected)
    {
        Assert.Equal(expected, Add(a, b));
    }
    
    private static int Add(int a, int b) => a + b;
}
