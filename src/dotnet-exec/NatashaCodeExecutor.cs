// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Exec;

public sealed class NatashaCodeExecutor: CodeExecutor
{
    private readonly ILogger _logger;

    public NatashaCodeExecutor(ILogger logger): base(logger)
    {
        _logger = logger;
    }

    protected override Task<Assembly> GetAssembly(CompileResult compileResult, ExecOptions options)
    {
        var domain = NatashaManagement.CreateDomain(InternalHelper.ApplicationName);
        domain.SetAssemblyLoadBehavior(LoadBehaviorEnum.UseHighVersion);
        foreach (var reference in compileResult.References)
        {
            try
            {
                domain.LoadAssemblyFromFile(reference);
                _logger.LogDebug("Reference {reference} loaded", reference);
            }
            catch (Exception)
            {
                // ignore
            }
        }
        var assembly = domain.LoadAssemblyFromStream(compileResult.Stream, null);
        return assembly.WrapTask();
    }
}
