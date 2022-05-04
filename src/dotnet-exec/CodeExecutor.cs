// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using WeihanLi.Common.Models;

namespace Exec;

public interface ICodeExecutor
{
    Task<Result> Execute(Assembly assembly, string[] args, ExecOptions options);
}

public class CodeExecutor: ICodeExecutor
{
    public async Task<Result> Execute(Assembly assembly, string[] args, ExecOptions options)
    {
        await Task.CompletedTask;
        
        var entryMethod = assembly.EntryPoint;
        if (entryMethod is null && options.EntryPoint.IsNotNullOrEmpty())
        {
            var types = assembly.GetTypes();
            var staticMethods = types.Select(x => x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .SelectMany(x => x);
            entryMethod = staticMethods.OrderBy(x=> x.Name).ThenBy(m=> m.GetParameters().Length).FirstOrDefault(x => x.Name.Equals(options.EntryPoint));
        }
        
        var executed = false;
        if (entryMethod is not null)
        {
            var parameters = entryMethod.GetParameters();
            try
            {
                if (parameters.IsNullOrEmpty())
                {
                    entryMethod.Invoke(null, Array.Empty<object?>());
                    executed = true;
                }
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]))
                {
                    entryMethod.Invoke(null, new object?[]{ args });
                    executed = true;
                }
            }
            catch (Exception e)
            {
                return Result.Fail(e.ToString(), ResultStatus.ProcessFail);
            }
        }
        return executed ? Result.Success() : Result.Fail("No valid EntryPoint found");
    }
}
