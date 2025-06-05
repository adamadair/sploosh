using System.Linq;

namespace AwaShell.BuiltInCommands;

public class Help : IBuiltInCommand
{
    public string Name => "help";
    
    public string HelpText => "help [command] - Display information about built-in commands. If no command is specified, list all available commands.";
    
    public bool Execute(ParsedCommand cmd)
    {
        if (cmd.Arguments.Count == 0)
        {
            // Display all available built-in commands in alphabetical order
            ShellIo.Out.WriteLine("Available built-in commands:");
            var sortedCommands = BuiltIns.Commands
                .Select(c => c.Name)
                .OrderBy(name => name);
                
            foreach (var commandName in sortedCommands)
            {
                ShellIo.Out.WriteLine($"  {commandName}");
            }
            
            ShellIo.Out.WriteLine("\nType 'help <command>' for more information on a specific command.");
        }
        else
        {
            // Display help for a specific command
            string commandName = cmd.Arguments[0];
            var command = BuiltIns.GetBuiltInCommand(commandName);
            
            if (command != null)
            {
                ShellIo.Out.WriteLine(command.HelpText);
            }
            else
            {
                ShellIo.Out.WriteLine($"help: no help found for '{commandName}'");
            }
        }
        
        return true;
    }
}
