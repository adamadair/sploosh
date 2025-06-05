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
/*
    History Lesson:
    The original Bourne shell (1977) had only a small set of built-ins — mostly for things that required direct 
    manipulation of shell internals: cd, set, trap, etc.
    
    Later shells like KornShell (ksh) and C Shell (csh) expanded on these, introducing ideas like alias, 
    command history, arithmetic, and more advanced scripting constructs.
    
    BASH, beginning in 1989, incorporated many of these and added non-standard but convenient ones like:
        - shopt (toggle BASH options)
        - bind (keyboard shortcuts)
        - help (BASH help system)
        - declare (type-aware variable declarations)
        - complete (programmable tab completion)
    
    POSIX does specify a set of built-in commands that conforming shells must support. Some of these are:
      
    | POSIX-Specified Built-ins | Purpose                       |
    | ------------------------- | ----------------------------- |
    | `cd`                      | Change directory              |
    | `echo`                    | Print to stdout               |
    | `eval`                    | Evaluate input as command     |
    | `exec`                    | Replace shell with command    |
    | `exit`                    | Terminate shell               |
    | `export`                  | Set environment variables     |
    | `readonly`                | Mark variable as immutable    |
    | `read`                    | Read from stdin into variable |
    | `test` / `[ ]`            | Conditional evaluation        |
    | `ulimit`                  | Set resource limits           |
    | `umask`                   | Set default file permissions  |   
    
    There are more built-ins in POSIX, but these are the most commonly used.
    
    Under the POSIX.1-2017 standard, pwd is specified as a standard utility, which means:

        - It must be available in the shell environment.
        - It can be a shell built-in or an external command (/bin/pwd), and shells like bash or dash often provide both.
    
    Here's where things get tricky.
   
    POSIX requires pwd to support:
   
       - Logical (-L): Follows symbolic links. Returns the value of $PWD.   
       - Physical (-P): Resolves symlinks and shows the actual filesystem path.
    
    Sploosh Shell implements pwd as a built-in command but currently does not support either options.
*/