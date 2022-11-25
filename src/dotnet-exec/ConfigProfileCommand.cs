// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace Exec;

public sealed class ConfigProfileCommand: Command
{
    public ConfigProfileCommand() : base("profile", "Configure user config profile")
    {
        AddCommand(new SetCommand());
        AddCommand(new GetCommand());
        AddCommand(new RemoveCommand());
    }

    public static readonly Argument<string> ProfileNameArgument = new("profile-name"); 

    private sealed class SetCommand: Command
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
    
    private sealed class GetCommand: Command
    {
        public GetCommand() : base("get", "Get config profile")
        {
            AddArgument(ProfileNameArgument);
        }
    }
    
    private sealed class RemoveCommand: Command
    {
        public RemoveCommand() : base("rm", "Remove config profile")
        {
            AddArgument(ProfileNameArgument);
        }
    }
} 
