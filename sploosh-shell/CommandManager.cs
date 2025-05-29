using System.Diagnostics;

namespace AwaShell;

// This class is responsible for managing and executing commands in the shell.
public class CommandManager
{
    
    private static readonly Dictionary<string, Func<string[], bool>> _builtins = new()
    {
        ["echo"] = Echo,
        ["exit"] = Exit,
        ["type"] = Type,
        ["pwd"] = Pwd,
        ["cd"] = Cd,
        ["clear"] = (args) => { ShellIo.Out.WriteLine("\x1b[H\x1b[2J"); return true; },
        ["help"] = (args) => { ShellIo.Out.WriteLine("Available commands: " + string.Join(", ", _builtins.Keys)); return true; },
        ["alias"] = (args) => { ShellIo.Out.WriteLine("Alias command not implemented yet."); return true; },
        ["unalias"] = (args) => { ShellIo.Out.WriteLine("Unalias command not implemented yet."); return true; },
        ["export"] = (args) => { ShellIo.Out.WriteLine("Export command not implemented yet."); return true; },
        ["unset"] = (args) => { ShellIo.Out.WriteLine("Unset command not implemented yet."); return true; },
        ["history"] = (args) => { ShellIo.Out.WriteLine("History command not implemented yet."); return true; },
        ["jobs"] = (args) => { ShellIo.Out.WriteLine("Jobs command not implemented yet."); return true; },
        ["fg"] = (args) => { ShellIo.Out.WriteLine("Foreground command not implemented yet."); return true; },
        ["bg"] = (args) => { ShellIo.Out.WriteLine("Background command not implemented yet."); return true; },
    };
    
    public static bool Execute(string[] args)
    {
        if (args.Length == 0)
            return true;

        var command = args[0]; // stop trying to make the command lowercase
        // Check if it's a builtin command
        if (_builtins.TryGetValue(command, out var builtinHandler))
        {
            return builtinHandler(args);
        }
        string? executablePath = PathResolver.FindExecutable(command);
        if (executablePath != null)
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    //Arguments = string.Join(" ", args.Skip(1).Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg)),
                    
                    UseShellExecute = false,
                    
                };
                foreach (var arg in args.Skip(1))
                {
                    process.StartInfo.ArgumentList.Add(arg);
                }
                process.Start();
                process.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing '{command}': {ex.Message}");
                return true;
            }
        }
        ShellIo.Out.WriteLine($"{args[0]}: command not found");
        return true;
    }

    private static bool Echo(string[] args)
    {
        // Skip the command name "echo" and join the remaining arguments
        string output = string.Join(" ", args.Skip(1));
        ShellIo.Out.WriteLine(output);
        return true;
    }

    private static bool Exit(string[] args)
    {
        var exitCode = 0;
        if (args.Length > 1)
        {
            var tryParse = int.TryParse(args[1], out exitCode);
            if (exitCode is < 0 or > 255) exitCode = 0;
            if (!tryParse)
            {
                exitCode = 0;
            }
        }
        Environment.Exit(exitCode);
        return false;
    }

    private static bool Type(string[] args)
    {
        if (args.Length < 2)
        {
            return true;
        }
        
        var cmd = args[1];

        if (_builtins.ContainsKey(cmd))
        {
            ShellIo.Out.WriteLine($"{cmd} is a shell builtin");
        }
        else
        {
            var path = PathResolver.FindExecutable(cmd);
            if (path != null)
            {
                ShellIo.Out.WriteLine($"{cmd} is {path}");
            }
            else
            {
                ShellIo.Out.WriteLine($"{cmd} not found");
            }
        }

        return true;
    }

    private static bool Pwd(string[] args)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        ShellIo.Out.WriteLine(currentDirectory);
        return true;
    }

    private static bool Cd(string[] args)
    {
        if (args.Length < 2)
        {
            // go to home directory
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            Directory.SetCurrentDirectory(homeDir);
            return true;
        }
        
        string path = args[1];
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
        catch (DirectoryNotFoundException ex)
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
