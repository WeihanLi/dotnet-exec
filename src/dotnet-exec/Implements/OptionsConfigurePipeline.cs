// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

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
