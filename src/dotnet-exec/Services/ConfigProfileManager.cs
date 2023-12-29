// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Contracts;
using System.Text.Json;

namespace Exec.Services;

public sealed class ConfigProfileManager : IConfigProfileManager
{
    private static readonly string ProfileFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".dotnet",
        "tools",
        ".config",
        Helper.ApplicationName,
        "profiles"
    );

    static ConfigProfileManager()
    {
        try
        {
            EnsureFolderCreated(ProfileFolder);
        }
        catch (Exception e)
        {
            InvokeHelper.OnInvokeException?.Invoke(e);
        }
    }

    public async Task ConfigureProfile(string profileName, ConfigProfile profile)
    {
        if (!Directory.Exists(ProfileFolder))
            throw new InvalidOperationException($"Could not create profiles folder, please create the folder(`{ProfileFolder}`) manually");

        var profilePath = Path.Combine(ProfileFolder, $"{profileName}.json");
        await using var fs = File.OpenWrite(profilePath);
        await JsonSerializer.SerializeAsync(fs, profile, JsonSerializerOptionsHelper.WriteIndented);
    }

    public Task DeleteProfile(string profileName)
    {
        if (!Directory.Exists(ProfileFolder)) return Task.CompletedTask;
        
        var profilePath = Path.Combine(ProfileFolder, $"{profileName}.json");
        if (File.Exists(profilePath))
        {
            File.Delete(profilePath);
        }
        return Task.CompletedTask;
    }

    public async Task<ConfigProfile?> GetProfile(string profileName)
    {
        if (!Directory.Exists(ProfileFolder)) return null;
        
        var profilePath = Path.Combine(ProfileFolder, $"{profileName}.json");
        if (!File.Exists(profilePath)) return null;

        await using var fs = File.OpenRead(profilePath);
        return await JsonSerializer.DeserializeAsync<ConfigProfile>(fs);
    }

    public Task<string[]> ListProfiles()
    {
        if (!Directory.Exists(ProfileFolder)) return Task.FromResult(Array.Empty<string>());
        
        var profileNames = Directory.GetFiles(ProfileFolder, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .WhereNotNull()
            .ToArray();
        return profileNames.WrapTask();
    }

    private static void EnsureFolderCreated(string folderPath)
    {
        if (Directory.Exists(folderPath)) return;
        
        var parent = Directory.GetParent(folderPath);
        if (parent is null || parent.Exists) return;

        // ensure path created
        EnsureFolderCreated(parent.FullName);

        // create parent folder if necessary
        if (!Directory.Exists(parent.FullName))
            Directory.CreateDirectory(parent.FullName);

        // create folder
        Directory.CreateDirectory(folderPath);
    }
}
