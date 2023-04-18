// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Implements;

public sealed class UriTransformer : IUriTransformer
{
    public string Transform(string originalUri)
    {
        if (Uri.TryCreate(originalUri, UriKind.Absolute, out var uri) && !uri.IsFile)
        {
            var scriptUrl = uri.Host switch
            {
                "github.com" => originalUri
                    .Replace($"://{uri.Host}/", $"://raw.githubusercontent.com/")
                    .Replace("/blob/", "/")
                    .Replace("/tree/", "/"),
                "gist.github.com" => originalUri
                                         .Replace($"://{uri.Host}/", $"://gist.githubusercontent.com/")
                                     + "/raw",
                "gitee.com" => originalUri
                    .Replace("/blob/", "/raw/")
                    .Replace("/tree/", "/raw/"),
                _ => originalUri
            };
            return scriptUrl;
        }
        return originalUri;
    }
}
