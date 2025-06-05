namespace AwaShell.BuiltInCommands;

public class Type : IBuiltInCommand
{
    public string Name => "type";
    
    public string HelpText => "type [command] - Display information about command type (builtin or executable).";

    public bool Execute(ParsedCommand cmd)
    {
        if (cmd.Arguments.Count < 1)
        {
            return true;
        }

        var command = BuiltIns.GetBuiltInCommand(cmd.Arguments[0]);
        if (command != null)
        {
            ShellIo.Out.WriteLine($"{cmd.Arguments[0]} is a shell builtin");
            return true;
        }

        var path = PathResolver.FindExecutable(cmd.Arguments[0]);
        ShellIo.Out.WriteLine(path != null ? $"{cmd.Arguments[0]} is {path}" : $"{cmd.Arguments[0]} not found");
        
        return true;
    }
}
