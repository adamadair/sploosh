namespace AwaShell.BuiltInCommands;

public class Clear : IBuiltInCommand
{
    public string Name => "clear";
    public bool Execute(ParsedCommand cmd)
    {
        ShellIo.Out.WriteLine("\x1b[H\x1b[2J"); 
        return true;
    }

    public string HelpText => "Clears the terminal screen.";
}