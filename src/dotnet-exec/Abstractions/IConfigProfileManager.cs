// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Contracts;

namespace Exec.Abstractions;

public interface IConfigProfileManager
{
    Task ConfigureProfile(string profileName, ConfigProfile profile);

    Task DeleteProfile(string profileName);

    Task<ConfigProfile?> GetProfile(string profileName);

    Task<string[]> ListProfiles();
}
