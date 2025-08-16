// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace UnitTest;

public class HelperTest
{
    [Theory]
    [InlineData("new-guid")]
    [InlineData("new_guid")]
    [InlineData("now")]
    [InlineData("date")]
    [InlineData("test:123")]
    [InlineData("test.test123")]
    public void AliasNameValidTest(string alias)
    {
        Assert.True(Helper.IsValidAliasName(alias));
    }

    [Theory]
    [InlineData("")]
    [InlineData("https://google.com")]
    [InlineData("https://github.com/WeihanLi/dotnet-exec/blob/main/tests/IntegrationTest/CodeSamples/ConfigurationManagerSample.cs")]
    [InlineData("dotnet-exec_blob_main_tests_IntegrationTest_CodeSamples_ConfigurationManagerSample.cs")]
    public void AliasNameRegexInvalidTest(string alias)
    {
        Assert.False(Helper.IsValidAliasName(alias));
    }
}
