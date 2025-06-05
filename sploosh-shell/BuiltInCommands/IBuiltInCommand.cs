namespace AwaShell.BuiltInCommands;

/// <summary>
/// This is the interface for shell built-in commands.
/// </summary>
public interface IBuiltInCommand
{
    /// <summary>
    /// Name of the built-in command (ie. "pwd", "cd", "exit").
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Implement this method to execute the command.
    /// </summary>
    /// <param name="cmd">The ParsedCommand to execute. This will contain any arguments.</param>
    /// <returns>true if the shell should continue to loop</returns>
    bool Execute(ParsedCommand cmd);
    /// <summary>
    /// Text that describes the command and its usage.
    /// </summary>
    string HelpText { get; }
}