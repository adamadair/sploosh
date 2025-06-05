using System;
using System.IO;

namespace AwaShell.BuiltInCommands;

public class Cd : IBuiltInCommand
{
    public string Name => "cd";
    
    public string HelpText => "cd [directory] - Change the current directory. If no directory is specified, change to the user's home directory.";
    
    public bool Execute(ParsedCommand cmd)
    {
        if (cmd.Arguments.Count < 1)
        {
            // go to the home directory
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            Directory.SetCurrentDirectory(homeDir);
            return true;
        }
        
        string path = cmd.Arguments[0];
        if (path.StartsWith("~"))
        {
            var newPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (path.Length > 1)
            {
                newPath += path.Substring(1);
                
            }
            path = newPath;
        }
        try
        {
            Directory.SetCurrentDirectory(path);
        }
        catch (DirectoryNotFoundException)
        {
            ShellIo.Out.WriteLine($"cd: {path}: No such file or directory");
        }
        catch (Exception ex)
        {
            ShellIo.Out.WriteLine($"cd: {ex.Message}");
        }

        return true;
    }
}
