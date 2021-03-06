// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using WeihanLi.Common.Models;

namespace Exec;

public interface ICodeExecutor
{
    Task<Result> Execute(CompileResult compileResult, ExecOptions options);
}

public abstract class CodeExecutor : ICodeExecutor
{
    protected ILogger Logger { get; }

    protected CodeExecutor(ILogger logger)
    {
        Logger = logger;
    }

    protected async Task<Result> ExecuteAssembly(Assembly assembly, ExecOptions options)
    {
        var assembliesString = assembly.GetReferencedAssemblies()
            .Select(x => x.FullName)
            .StringJoin(";");
        Logger.LogDebug("ReferencedAssemblies: {assemblies}", assembliesString);
        var entryMethod = assembly.EntryPoint;
        if (entryMethod is null && options.EntryPoint.IsNotNullOrEmpty())
        {
            var types = assembly.GetTypes();
            var staticMethods = types.Select(x =>
                    x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .SelectMany(x => x)
                .Where(x => x.Name.Equals(options.EntryPoint));
            if (options.StartupType.IsNotNullOrEmpty())
            {
                staticMethods = staticMethods.Where(x => x.DeclaringType?.FullName == options.StartupType);
            }
            entryMethod = staticMethods.MinBy(m => m.GetParameters().Length);
        }

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

    public abstract Task<Result> Execute(CompileResult compileResult, ExecOptions options);
}

public sealed class DefaultCodeExecutor : CodeExecutor
{
    private readonly IReferenceResolver _referenceResolver;

    public DefaultCodeExecutor(IReferenceResolver referenceResolver, ILogger logger) : base(logger)
    {
        _referenceResolver = referenceResolver;
    }

    public override async Task<Result> Execute(CompileResult compileResult, ExecOptions options)
    {
        var references = await _referenceResolver.ResolveReferences(options, false);
        var context = new CustomLoadContext(references);
        using var scope = context.EnterContextualReflection();
        var assembly = context.LoadFromStream(compileResult.Stream);
        return await ExecuteAssembly(assembly, options);
    }
}
