using System;
using System.Collections.Generic;
using System.Linq;

namespace AwaShell.BuiltInCommands;

// This class serves as a container for built-in commands.
internal static class BuiltIns
{
    public static readonly List<IBuiltInCommand> Commands =
    [
        new Echo(),
        new Exit(),
        new Type(),
        new Pwd(),
        new Cd(),
        new History(),
        new Help()
        // Add more commands here
    ];
    
    public static bool IsBuiltInCommand(string commandName) => Commands.Any(command => command.Name.Equals(commandName, StringComparison.Ordinal));
    
    public static IBuiltInCommand GetBuiltInCommand(string commandName)
    {
        return Commands.FirstOrDefault(command => command.Name.Equals(commandName, StringComparison.Ordinal));
    }
}