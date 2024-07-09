// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0


using System.Diagnostics.CodeAnalysis;

namespace Exec.Services.Middlewares;

[ExcludeFromCodeCoverage]
internal sealed class ThirdPartyScriptOptionsPreConfigureMiddleware
    (IEnumerable<IThirdPartyScriptTransformer> transformers)
    : IOptionsPreConfigureMiddleware
{
    public async Task InvokeAsync(ExecOptions context, Func<ExecOptions, Task> next)
    {
        var script = context.Script;
        if (File.Exists(script)
            && context.AdditionalScripts is null or { Count: 0 }
            )
        {
            var ext = Path.GetExtension(script);
            var lines = await File.ReadAllLinesAsync(script);
            foreach (var transformer in transformers)
            {
                if (transformer.SupportedExtensions.Contains(ext))
                {
                    await transformer.InvokeAsync(context, lines);
                    break;
                }
            }
        }
        await next(context);
    }
}
