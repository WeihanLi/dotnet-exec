// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ExecMcp;

[McpServerToolType]
public class ExecTool(CommandHandler commandHandler)
{
    [McpServerTool(Name = "execute-C#-script", Title = "Execute C# script, either script text or script file path")]
    public async Task<int> Exec(
        [Description("The C# script text or file path to execute")] string script,
        [Description("The package references for the execution, reference format: `WeihanLi.Npoi@2.4.2`")] string[]? references,
        [Description("The namespace using for the execution, format: `WeihanLi.Npoi` or `static System.Console`")] string[]? usings
        )
    {
        var execOptions = new ExecOptions
        {
            Script = script
        };

        foreach (var reference in references ?? [])
        {
            execOptions.References.Add("nuget: " + reference);
        }
        foreach (var @using in usings ?? [])
        {
            execOptions.Usings.Add(@using);
        }

        return await commandHandler.Execute(execOptions);
    }
}
