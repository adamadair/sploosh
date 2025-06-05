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

/*
    History Lesson: Why Echo?
    In early UNIX (circa the 1970s), shell scripting was designed to be composed of small, focused tools. echo was created as a:
   
       🗣️ Lightweight utility to print arguments to standard output.
   
   That’s it. It wasn't meant to be complex — just a convenient way to output text without relying on a more verbose language like printf or cat.
   For example:
   
   echo Hello, world
   
   This is much cleaner than:
   
   printf "Hello, world\n"
   
   or
   
   cat <<EOF
   Hello, world
   EOF
   
   So echo became a shell-friendly, low-overhead way to:
       - Print messages   
       - Show variable values   
       - Output script progress markers   
       - Embed text into pipes
       
   The behavior of echo is not consistent across systems. Some quirks:
       - Does echo -n suppress the newline? Maybe. (In Sploosh it does)
       - Does echo -e interpret \n and \t? Sometimes.
       - What about strings that start with a dash? Chaos.       
       
   Even POSIX says:
       "Implementations are encouraged to make echo behave consistently, but its behavior is inherently ambiguous."
   
   This is why serious shell scripts often use printf instead of echo.
 */