// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ExecMcp;

public class ToolExecOptions
{
    public required string Script { get; set; }

    [Description("The package references for the execution")]
    public string[]? References { get; set; }
    
    [Description("The namespace using for the execution")]
    public string[]? Usings { get; set; }
}

[McpServerToolType]
public class ExecTool(CommandHandler commandHandler)
{
    public async Task<int> Exec(ToolExecOptions options)
    {
        var execOptions = new ExecOptions
        {
            Script = options.Script
        };
        return await commandHandler.Execute(execOptions);
    }
}
