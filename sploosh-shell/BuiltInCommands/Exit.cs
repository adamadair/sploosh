using System;

namespace AwaShell.BuiltInCommands;

public class Exit : IBuiltInCommand
{
    public string Name => "exit";
    
    public string HelpText => "exit [code] - Terminate the shell with an optional exit code (0-255).";
    
    public bool Execute(ParsedCommand cmd)
    {
        var exitCode = 0;
        ReadLine.ReadLine.SaveHistory();
        if (cmd.Arguments.Count > 0)
        {
            var tryParse = int.TryParse(cmd.Arguments[0], out exitCode);
            if (exitCode is < 0 or > 255) exitCode = 0;
            if (!tryParse)
            {
                exitCode = 0;
            }
        }
        Environment.Exit(exitCode);
        return false; // This will never actually be reached due to Environment.Exit()
    }
}
