// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Commands;

public sealed class ConfigProfileCommand : Command
{
    public ConfigProfileCommand() : base("profile", "Configure user config profile")
    {
        AddCommand(new SetCommand());
        AddCommand(new GetCommand());
        AddCommand(new RemoveCommand());
        AddCommand(new ListCommand());
    }

    public static readonly Argument<string> ProfileNameArgument = new("profile-name", "The config profile name to operate");

    private sealed class SetCommand : Command
    {
        public SetCommand() : base("set", "Configure config profile")
        {
            AddArgument(ProfileNameArgument);
            AddOption(ExecOptions.UsingsOption);
            AddOption(ExecOptions.ReferencesOption);
            AddOption(ExecOptions.WebReferencesOption);
            AddOption(ExecOptions.WideReferencesOption);
            AddOption(ExecOptions.EntryPointOption);
            AddOption(ExecOptions.PreviewOption);
        }
    }

    private sealed class GetCommand : Command
    {
        public GetCommand() : base("get", "Get config profile")
        {
            AddArgument(ProfileNameArgument);
        }
    }

    private sealed class RemoveCommand : Command
    {
        public RemoveCommand() : base("rm", "Remove config profile")
        {
            AddArgument(ProfileNameArgument);
        }
    }

    private sealed class ListCommand : Command
    {
        public ListCommand() : base("ls", "List the config profiles configured")
        {
        }
    }
}
