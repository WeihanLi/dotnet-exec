// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace Exec;

public interface IUriTransformer
{
    string Transform(string originalUri);
}

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
                _ => originalUri
            };
            return scriptUrl;
        }
        return originalUri;
    }
}
