// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.Loader;
using WeihanLi.Common.Models;

namespace Exec;

public interface ICodeExecutor
{
    Task<Result> Execute(CompileResult compileResult, ExecOptions options);
}

public class CodeExecutor : ICodeExecutor
{
    private readonly ILogger _logger;

    public CodeExecutor(ILogger logger)
    {
        _logger = logger;
    }

    protected virtual Task<Assembly> GetAssembly(CompileResult compileResult, ExecOptions options)
    {
        var assembly = Assembly.Load(compileResult.Stream.ToArray());
        return Task.FromResult(assembly);   
    }

    private async Task<Result> ExecuteAssembly(Assembly assembly, ExecOptions options)
    {
        var entryMethod = assembly.EntryPoint;
        if (entryMethod is null && options.EntryPoint.IsNotNullOrEmpty())
        {
            var types = assembly.GetTypes();
            var staticMethods = types.Select(x =>
                    x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .SelectMany(x => x);
            entryMethod = staticMethods.OrderBy(x => x.Name).ThenBy(m => m.GetParameters().Length)
                .FirstOrDefault(x => x.Name.Equals(options.EntryPoint));
        }

        var executed = false;
        if (entryMethod is not null)
        {
            var parameters = entryMethod.GetParameters();
            _logger.LogDebug("Entry is found, {entryName}, returnType: {returnType}",
                $"{entryMethod.DeclaringType!.FullName}.{entryMethod.Name}", entryMethod.ReturnType.FullName);
            try
            {
                object? returnValue = null;
                if (parameters.IsNullOrEmpty())
                {
                    returnValue = entryMethod.Invoke(null, Array.Empty<object?>());
                    executed = true;
                }
                else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]))
                {
                    returnValue = entryMethod.Invoke(null, new object?[] { options.Arguments });
                    executed = true;
                }

                switch (returnValue)
                {
                    case Task task:
                        await task.ConfigureAwait(false);
                        break;
                    case ValueTask valueTask:
                        await valueTask.ConfigureAwait(false);
                        break;
                }
            }
            catch (Exception e)
            {
                return Result.Fail(e.ToString(), ResultStatus.ProcessFail);
            }
        }

        return executed ? Result.Success() : Result.Fail("No valid EntryPoint found");
    
    }
    
    public async Task<Result> Execute(CompileResult compileResult, ExecOptions options)
    {
        var assembly = await GetAssembly(compileResult, options);
        return await ExecuteAssembly(assembly, options);
    }
}


public sealed class AssemblyLoadContextExecutor : CodeExecutor
{
    public AssemblyLoadContextExecutor(ILogger logger) : base(logger)
    {
    }

    protected override Task<Assembly> GetAssembly(CompileResult compileResult, ExecOptions options)
    {
        var context = new CustomLoadContext(compileResult.References);
        var assembly = context.LoadFromStream(compileResult.Stream);
        return Task.FromResult(assembly);
    }
}
