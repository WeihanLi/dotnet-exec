// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Commands;
internal sealed class AliasCommand : Command
{
    internal static readonly Argument<string> AliasNameArg = new("aliasName")
    {
        Description = "Alias Name"
    };
    internal static readonly Argument<string> AliasValueArg = new("aliasValue")
    {
        Description = "Alias Value"
    };

    public AliasCommand() : base("alias", "Alias management")
    {
        Add(new AliasListCommand());
        Add(new AliasSetCommand());
        Add(new AliasUnsetCommand());
    }
}

file sealed class AliasListCommand : Command
{
    public AliasListCommand() : base("list", "List all alias config")
    {
        Aliases.Add("ls");
    }
}

file sealed class AliasSetCommand : Command
{
    public AliasSetCommand() : base("set", "Set alias config")
    {
        Add(AliasCommand.AliasNameArg);
        Add(AliasCommand.AliasValueArg);
    }
}

file sealed class AliasUnsetCommand : Command
{
    public AliasUnsetCommand() : base("unset", "Unset alias config")
    {
        Aliases.Add("rm");
        Add(AliasCommand.AliasNameArg);
    }
}
