// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace Exec;

public interface ICompilerFactory
{
    ICodeCompiler GetCompiler(string compilerType);
}

public sealed class CompilerFactory : ICompilerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CompilerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public ICodeCompiler GetCompiler(string compilerType)
    {
        return compilerType switch
        {
            "advanced" => _serviceProvider.GetServiceOrCreateInstance<AdvancedCodeCompiler>(),
            "adhoc" => _serviceProvider.GetServiceOrCreateInstance<AdhocWorkspaceCodeCompiler>(),
            _ => _serviceProvider.GetServiceOrCreateInstance<SimpleCodeCompiler>()
        };
    }
}
