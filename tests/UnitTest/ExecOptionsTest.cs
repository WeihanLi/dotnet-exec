// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace UnitTest;

public class ExecOptionsTest
{
    [Fact]
    public void DefaultTargetFrameworkTest()
    {
        var options = new ExecOptions();
        Assert.Equal(ExecOptions.DefaultTargetFramework, options.TargetFramework);
    }
}
