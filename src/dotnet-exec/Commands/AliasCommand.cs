// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Commands;
internal sealed class AliasCommand : Command
{
    internal static readonly Argument<string> AliasNameArg = new("aliasName", "Alias Name");
    internal static readonly Argument<string> AliasValueArg = new("aliasValue", "Alias Value");

    public AliasCommand() : base("alias", "Alias management")
    {
        AddCommand(new AliasListCommand());
        AddCommand(new AliasSetCommand());
        AddCommand(new AliasUnsetCommand());
    }
}

file sealed class AliasListCommand : Command
{
    public AliasListCommand() : base("list", "List all alias config")
    {
        AddAlias("ls");
    }
}

file sealed class AliasSetCommand : Command
{
    public AliasSetCommand() : base("set", "Set alias config")
    {
        AddArgument(AliasCommand.AliasNameArg);
        AddArgument(AliasCommand.AliasValueArg);
    }
}

file sealed class AliasUnsetCommand : Command
{
    public AliasUnsetCommand() : base("unset", "Unset alias config")
    {
        AddAlias("rm");
        AddArgument(AliasCommand.AliasNameArg);
    }
}
