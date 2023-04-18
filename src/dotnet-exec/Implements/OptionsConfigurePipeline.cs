// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace Exec.Implements;

public sealed class OptionsConfigurePipeline : IOptionsConfigurePipeline
{
    private readonly Func<ExecOptions, Task> _pipeline;
    public OptionsConfigurePipeline(IEnumerable<IOptionsConfigureMiddleware> middlewares)
    {
        var pipelineBuilder = PipelineBuilder.CreateAsync<ExecOptions>();
        foreach (var middleware in middlewares)
        {
            pipelineBuilder.Use(middleware.Execute);
        }
        _pipeline = pipelineBuilder.Build();
    }

    public Task Execute(ExecOptions options)
    {
        return _pipeline.Invoke(options);
    }
}
