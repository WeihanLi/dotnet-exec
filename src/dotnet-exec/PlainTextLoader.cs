// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Exec;

public sealed class PlainTextLoader(string text) : TextLoader
{
    private readonly TextAndVersion _textAndVersion = TextAndVersion.Create(SourceText.From(text), VersionStamp.Default);

    public override Task<TextAndVersion> LoadTextAndVersionAsync(LoadTextOptions options, CancellationToken cancellationToken)
    {
        return Task.FromResult(_textAndVersion);
    }
}
