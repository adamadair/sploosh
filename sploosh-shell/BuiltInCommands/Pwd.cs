using System.IO;

namespace AwaShell.BuiltInCommands;

public class Pwd : IBuiltInCommand
{
    public string Name => "pwd";
    
    public string HelpText => "pwd - Print the current working directory.";
    
    public bool Execute(ParsedCommand cmd)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        ShellIo.Out.WriteLine(currentDirectory);
        return true;
    }
}
