// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Reflection;
using WeihanLi.Common.Models;

namespace Exec.Services;

public abstract class CodeExecutor(ILogger logger) : ICodeExecutor
{
    protected ILogger Logger { get; } = logger;

    protected async Task<Result<int>> ExecuteAssembly(Assembly assembly, ExecOptions options)
    {
        if (options.DebugEnabled)
        {
            var assembliesString = assembly.GetReferencedAssemblies()
              .Select(x => x.FullName)
              .StringJoin("; ");
            Logger.LogDebug("ReferencedAssemblies: {assemblies}", assembliesString);
        }
        var entryMethod = assembly.EntryPoint;
        if (entryMethod is null && options.EntryPoint.IsNotNullOrEmpty())
        {
            var types = assembly.GetTypes();
            var staticMethods = types.Select(x =>
                    x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .SelectMany(x => x)
                .Where(x => x.Name.Equals(options.EntryPoint, StringComparison.Ordinal));
            if (options.StartupType.IsNotNullOrEmpty())
            {
                staticMethods = staticMethods.Where(x => x.DeclaringType?.FullName == options.StartupType);
            }
            entryMethod = staticMethods.MinBy(m => m.GetParameters().Length);
        }

        var returnExitCode = 0;
        var executed = false;
        if (entryMethod is not null)
        {
            var parameters = entryMethod.GetParameters();
            Logger.LogDebug("Entry is found, {entryName}, returnType: {returnType}",
                $"{entryMethod.DeclaringType!.FullName}.{entryMethod.Name}", entryMethod.ReturnType.FullName);
            try
            {
                object? returnValue = null;
                if (parameters.IsNullOrEmpty())
                {
                    returnValue = entryMethod.Invoke(null, []);
                    executed = true;
                }
                else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]))
                {
                    returnValue = entryMethod.Invoke(null, [options.Arguments]);
                    executed = true;
                }
                await TaskHelper.ToTask(returnValue).ConfigureAwait(false);
                returnExitCode = returnValue switch
                {
                    ValueTask<int> valueTaskValue => valueTaskValue.Result,
                    Task<int> taskValue => taskValue.Result,
                    int value => value,
                    _ => 0
                };
            }
            catch (Exception e)
            {
                return Result.Fail(e.ToString(), ResultStatus.ProcessFail, (int)ResultStatus.ProcessFail);
            }
        }

        return executed ? Result.Success(returnExitCode) : Result.Fail("No valid EntryPoint found", ResultStatus.RequestError, (int)ResultStatus.RequestError);
    }

    public abstract Task<Result<int>> Execute(CompileResult compileResult, ExecOptions options);
}

public sealed class DefaultCodeExecutor(IRefResolver referenceResolver, ILogger logger) : CodeExecutor(logger)
{
    public override async Task<Result<int>> Execute(CompileResult compileResult, ExecOptions options)
    {
        var references = await referenceResolver.ResolveReferences(options, false);
        using var context = new CustomLoadContext(references);
        using var scope = context.EnterContextualReflection();
        var assembly = context.LoadFromStream(compileResult.Stream);
        var result = await ExecuteAssembly(assembly, options);
        return result;
    }
}
