namespace AwaShell.BuiltInCommands;

using System.Collections.Generic;

public class Echo : IBuiltInCommand
{
    public string Name => "echo";

    public string HelpText => "echo [-n] [args...] - Display the specified arguments. If -n is provided, no newline is output at the end.";

    public bool Execute(ParsedCommand cmd)
    {
        var suppressNewline = false;
        var arguments = new List<string>(cmd.Arguments);

        if (arguments.Count > 0 && arguments[0] == "-n")
        {
            suppressNewline = true;
            arguments.RemoveAt(0);
        }

        var output = string.Join(" ", arguments);
        if (suppressNewline)
        {
            ShellIo.Out.Write(output);
        }
        else
        {
            ShellIo.Out.WriteLine(output);
        }
        return true;
    }
}
