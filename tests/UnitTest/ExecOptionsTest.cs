// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Exec;

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
