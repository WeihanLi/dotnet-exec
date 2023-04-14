// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Exec.Contracts;

namespace IntegrationTest;

public class ConfigProfileManagerTests
{
    private readonly IConfigProfileManager _configProfileManager;

    public ConfigProfileManagerTests(IConfigProfileManager configProfileManager)
    {
        _configProfileManager = configProfileManager;
    }

    [Fact]
    public async Task ProfileOperation()
    {
        var profile = new ConfigProfile()
        {
            IncludeWideReferences = false,
            EntryPoint = "Execute"
        };
        var name = Guid.NewGuid().ToString();
        var getProfile = await _configProfileManager.GetProfile(name);
        Assert.Null(getProfile);
        try
        {
            await _configProfileManager.ConfigureProfile(name, profile);
            getProfile = await _configProfileManager.GetProfile(name);
            Assert.NotNull(getProfile);
            Assert.Equal(profile.IncludeWideReferences, getProfile.IncludeWideReferences);
            Assert.Equal(profile.EntryPoint, getProfile.EntryPoint);

            var profiles = await _configProfileManager.ListProfiles();
            Assert.NotEmpty(profiles);
        }
        finally
        {
            await _configProfileManager.DeleteProfile(name);
        }
    }
}
