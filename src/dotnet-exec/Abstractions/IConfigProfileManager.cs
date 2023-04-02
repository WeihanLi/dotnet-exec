// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace Exec.Abstractions;

public interface IConfigProfileManager
{
    Task ConfigureProfile(string profileName, ConfigProfile profile);

    Task DeleteProfile(string profileName);

    Task<ConfigProfile?> GetProfile(string profileName);

    Task<string[]> ListProfiles();
}
