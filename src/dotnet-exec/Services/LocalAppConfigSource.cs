// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Contracts;
using System.Text.Json;

namespace Exec.Services;
internal sealed class LocalAppConfigSource : IAppConfigSource
{
    private static readonly string DefaultConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".dotnet",
        "tools",
        ".config",
        $"{Helper.ApplicationName}.json"
    );

    public async Task<AppConfiguration> GetConfigAsync()
    {
        if (!File.Exists(DefaultConfigPath))
        {
            return AppConfiguration.Default;
        }

        using var fs = File.OpenRead(DefaultConfigPath);
        var appConfig = await JsonSerializer.DeserializeAsync<AppConfiguration>(fs, JsonHelper.UnsafeEncoderOptions);
        ArgumentNullException.ThrowIfNull(appConfig);
        return appConfig;
    }

    public async Task<bool> SaveConfigAsync(AppConfiguration appConfig)
    {
        var folder = Path.GetDirectoryName(DefaultConfigPath);
        ArgumentNullException.ThrowIfNull(folder);
        if (!Directory.Exists(folder))
        {
            Helper.EnsureFolderCreated(folder);
        }

        using var fs = File.OpenWrite(DefaultConfigPath);
        await JsonSerializer.SerializeAsync(fs, appConfig, JsonHelper.UnsafeEncoderOptions);
        return true;
    }
}
