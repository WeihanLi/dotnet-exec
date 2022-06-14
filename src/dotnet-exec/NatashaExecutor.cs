// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using WeihanLi.Common.Models;

namespace Exec;

public sealed class NatashaExecutor : CodeExecutor
{
    static NatashaExecutor()
    {
        NatashaInitializer.Preheating();
    }

    public NatashaExecutor(ILogger logger) : base(logger)
    {
    }

    public override Task<Result> Execute(CompileResult compileResult, ExecOptions options)
    {
        var references = InternalHelper.ResolveReferences(options, false);
        using var domain = NatashaManagement.CreateDomain(InternalHelper.ApplicationName);
        foreach (var reference in references)
        {
            try
            {
                domain.LoadAssemblyFromFile(reference);
            }
            catch (Exception)
            {
                // ignore
            }
        }
        var assembly = domain.LoadAssemblyFromStream(compileResult.Stream, null);
        return ExecuteAssembly(assembly, options);
    }
}
