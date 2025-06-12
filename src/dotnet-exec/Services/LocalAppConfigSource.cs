// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Contracts;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Exec.Services;

[ExcludeFromCodeCoverage]
internal sealed class LocalAppConfigSource : IAppConfigSource
{
    private static readonly string LegacyConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".dotnet",
        "tools",
        ".config",
        $"{Helper.ApplicationName}.json"
    );

    private static readonly string DefaultConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".dotnet",
        "tools",
        ".config",
        Helper.ApplicationName,
        "config.json"
    );

    public async Task<AppConfiguration> GetConfigAsync()
    {
        if (!File.Exists(DefaultConfigPath))
        {
            if (File.Exists(LegacyConfigPath))
            {
                return await GetAppConfigurationFromFile(LegacyConfigPath);
            }

            return AppConfiguration.Default;
        }

        return await GetAppConfigurationFromFile(DefaultConfigPath);
    }

    public async Task<bool> SaveConfigAsync(AppConfiguration appConfig)
    {
        var folder = Path.GetDirectoryName(DefaultConfigPath);
        ArgumentNullException.ThrowIfNull(folder);
        if (!Directory.Exists(folder))
        {
            Helper.EnsureFolderCreated(folder);
        }

        var bytes = JsonSerializer.SerializeToUtf8Bytes(appConfig, JsonHelper.UnsafeEncoderOptions);
        await File.WriteAllBytesAsync(DefaultConfigPath, bytes);

        return true;
    }

    private static async Task<AppConfiguration> GetAppConfigurationFromFile(string filePath)
    {
        using var fs = File.OpenRead(filePath);
        var appConfig = await JsonSerializer.DeserializeAsync<AppConfiguration>(fs, JsonHelper.UnsafeEncoderOptions);
        ArgumentNullException.ThrowIfNull(appConfig);
        return appConfig;
    }
}
