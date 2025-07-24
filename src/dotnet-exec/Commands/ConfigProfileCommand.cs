// Copyright (c) 2022-2025 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Commands;

internal sealed class ConfigProfileCommand : Command
{
    public ConfigProfileCommand() : base("profile", "Configure user config profile")
    {
        Add(new SetCommand());
        Add(new GetCommand());
        Add(new RemoveCommand());
        Add(new ListCommand());
    }

    public static readonly Argument<string> ProfileNameArgument = new("profile-name")
    {
        Description = "The config profile name to operate"
    };

    private sealed class SetCommand : Command
    {
        public SetCommand() : base("set", "Configure config profile")
        {
            Add(ProfileNameArgument);
            Add(ExecOptions.UsingsOption);
            Add(ExecOptions.ReferencesOption);
            Add(ExecOptions.WebReferencesOption);
            Add(ExecOptions.WideReferencesOption);
            Add(ExecOptions.EntryPointOption);
            Add(ExecOptions.PreviewOption);
            Add(ExecOptions.DefaultEntryMethodsOption);
        }
    }

    private sealed class GetCommand : Command
    {
        public GetCommand() : base("get", "Get config profile")
        {
            Add(ProfileNameArgument);
        }
    }

    private sealed class RemoveCommand : Command
    {
        public RemoveCommand() : base("rm", "Remove config profile")
        {
            Add(ProfileNameArgument);
        }
    }

    private sealed class ListCommand() : Command("ls", "List the config profiles configured");
}
