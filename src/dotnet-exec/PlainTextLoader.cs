// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Exec;

public sealed class PlainTextLoader : TextLoader
{
    private readonly TextAndVersion _textAndVersion;

    public PlainTextLoader(string text)
    {
        _textAndVersion = TextAndVersion.Create(SourceText.From(text), VersionStamp.Default);
    }

    public override Task<TextAndVersion> LoadTextAndVersionAsync(Workspace workspace, DocumentId documentId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_textAndVersion);
    }
}
