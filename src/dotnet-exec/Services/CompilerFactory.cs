// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Globalization;

namespace Exec.Services;

public sealed class CompilerFactory(IServiceProvider serviceProvider) : ICompilerFactory
{
    public ICodeCompiler GetCompiler(string compilerType)
    {
        return compilerType.ToLower(CultureInfo.InvariantCulture) switch
        {
            "workspace" => serviceProvider.GetRequiredService<WorkspaceCodeCompiler>(),
            Helper.Script => serviceProvider.GetRequiredService<CSharpScriptCompilerExecutor>(),
            _ => serviceProvider.GetRequiredService<SimpleCodeCompiler>()
        };
    }
}
