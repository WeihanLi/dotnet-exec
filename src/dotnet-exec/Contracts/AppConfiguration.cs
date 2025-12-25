// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Contracts;

public sealed class AppConfiguration
{
    public static readonly AppConfiguration Default = new();

    // theme settings ...
    public Dictionary<string, string> Aliases { get; set; } = new()
    {
        { "guid", "System.Guid.NewGuid()" },
        { "now", "System.DateTimeOffset.Now" },
        { "date", "System.DateTimeOffset.Now" },
        { "base64", "System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(args[0])).Dump();" },
        { "base64-decode", "System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(args[0])).Dump();" },
        { "md5", "System.Convert.ToHexString(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(args[0]))).Dump();" },
        { "sha256", "System.Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(args[0]))).Dump();" }
    };
}
