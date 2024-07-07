// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics.CodeAnalysis;

namespace Exec.Services;

[ExcludeFromCodeCoverage]
public sealed class OptionsConfigurePipeline : IOptionsConfigurePipeline
{
    private readonly Func<ExecOptions, Task> _pipeline;
    public OptionsConfigurePipeline(IEnumerable<IOptionsConfigureMiddleware> middlewares)
    {
        var pipelineBuilder = PipelineBuilder.CreateAsync<ExecOptions>();
        foreach (var middleware in middlewares)
        {
            pipelineBuilder.UseMiddleware(middleware);
        }
        _pipeline = pipelineBuilder.Build();
    }

    public Task Execute(ExecOptions options)
    {
        return _pipeline.Invoke(options);
    }
}

[ExcludeFromCodeCoverage]
public sealed class OptionsPreConfigurePipeline : IOptionsPreConfigurePipeline
{
    private readonly Func<ExecOptions, Task> _pipeline;
    public OptionsPreConfigurePipeline(IEnumerable<IOptionsPreConfigureMiddleware> middlewares)
    {
        var pipelineBuilder = PipelineBuilder.CreateAsync<ExecOptions>();
        foreach (var middleware in middlewares)
        {
            pipelineBuilder.UseMiddleware(middleware);
        }
        _pipeline = pipelineBuilder.Build();
    }

    public Task Execute(ExecOptions options)
    {
        return _pipeline.Invoke(options);
    }
}
