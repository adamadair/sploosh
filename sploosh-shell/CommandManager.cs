using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AwaShell;

// This class is responsible for managing and executing commands in the shell.
public static class CommandManager
{
    // Dictionary of built-in commands and their handlers
    private static readonly Dictionary<string, Func<ParsedCommand, bool>> _builtins = new()
    {
        ["echo"] = Echo,
        ["exit"] = Exit,
        ["type"] = Type,
        ["pwd"] = Pwd,
        ["cd"] = Cd,
        ["clear"] = (cmd) => { ShellIo.Out.WriteLine("\x1b[H\x1b[2J"); return true; },
        ["help"] = (cmd) => { ShellIo.Out.WriteLine("Available commands: " + string.Join(", ", _builtins.Keys)); return true; },
        ["alias"] = (cmd) => { ShellIo.Out.WriteLine("Alias command not implemented yet."); return true; },
        ["unalias"] = (cmd) => { ShellIo.Out.WriteLine("Unalias command not implemented yet."); return true; },
        ["export"] = (cmd) => { ShellIo.Out.WriteLine("Export command not implemented yet."); return true; },
        ["unset"] = (cmd) => { ShellIo.Out.WriteLine("Unset command not implemented yet."); return true; },
        ["history"] = (cmd) => { ShellIo.Out.WriteLine("History command not implemented yet."); return true; },
        ["jobs"] = (cmd) => { ShellIo.Out.WriteLine("Jobs command not implemented yet."); return true; },
        ["fg"] = (cmd) => { ShellIo.Out.WriteLine("Foreground command not implemented yet."); return true; },
        ["bg"] = (cmd) => { ShellIo.Out.WriteLine("Background command not implemented yet."); return true; },
    };
    
    // Readonly property to get the list of built-in commands
    public static IEnumerable<string> BuiltinCommands => _builtins.Keys;

    /// <summary>
    /// Class to handle temporary redirection of output streams
    /// </summary>
    private class RedirectionScope : IDisposable
    {
        private readonly TextWriter _originalOut;
        private readonly TextWriter _originalErr;
        private readonly TextWriter _stdOutWriter;
        private readonly TextWriter _stdErrWriter;

        public RedirectionScope(RedirectionInfo redirects)
        {
            _originalOut = ShellIo.Out;
            _originalErr = ShellIo.Error;

            if (redirects.HasStdOut)
            {
                _stdOutWriter = redirects.AppendStdOut 
                    ? new StreamWriter(redirects.StdOutTarget, append: true) 
                    : new StreamWriter(redirects.StdOutTarget, append: false);
                ShellIo.SetOut(_stdOutWriter);
            }

            if (redirects.HasStdErr)
            {
                // If both stdout and stderr point to the same file, reuse the writer
                if (redirects.HasStdOut && redirects.StdErrTarget == redirects.StdOutTarget)
                {
                    ShellIo.SetError(ShellIo.Out);
                }
                else
                {
                    _stdErrWriter = redirects.AppendStdErr 
                        ? new StreamWriter(redirects.StdErrTarget, append: true) 
                        : new StreamWriter(redirects.StdErrTarget, append: false);
                    ShellIo.SetError(_stdErrWriter);
                }
            }
        }

        public void Dispose()
        {
            ShellIo.SetOut(_originalOut);
            ShellIo.SetError(_originalErr);
            
            _stdOutWriter?.Dispose();
            if (_stdErrWriter != null && _stdOutWriter != _stdErrWriter)
            {
                _stdErrWriter.Dispose();
            }
        }
    }

    /// <summary>
    /// Sets up the redirection for command output based on the RedirectionInfo
    /// </summary>
    private static RedirectionScope SetupRedirection(RedirectionInfo redirects)
    {
        return redirects.HasAny ? new RedirectionScope(redirects) : null;
    }

    /// <summary>
    /// Executes a pipeline of commands
    /// </summary>
    private static void ExecutePipeline(ParsedCommand pipeline)
    {
        if (!pipeline.IsPipelineStart)
        {
            return;
        }

        var processes = new List<Process>();
        ParsedCommand current = pipeline;

        try
        {
            Process previousProcess = null;

            // Create all processes in the pipeline
            while (current != null)
            {
                var process = new Process();
                string executable = current.Executable;
                string executablePath = PathResolver.FindExecutable(executable);
                
                if (executablePath == null)
                {
                    ShellIo.Error.WriteLine($"{executable}: command not found");
                    break;
                }

                process.StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    UseShellExecute = false,
                    RedirectStandardInput = previousProcess != null,
                    RedirectStandardOutput = current.PipeTarget != null || current.Redirects.HasStdOut,
                    RedirectStandardError = current.Redirects.HasStdErr
                };

                foreach (var arg in current.Arguments)
                {
                    process.StartInfo.ArgumentList.Add(arg);
                }

                // If this is the last command in the pipeline and has redirection
                if (current.PipeTarget == null && current.Redirects.HasAny)
                {
                    using var redirectionScope = SetupRedirection(current.Redirects);
                }

                processes.Add(process);
                previousProcess = process;
                current = current.PipeTarget;
            }

            // Set up the pipeline connections
            for (int i = 0; i < processes.Count; i++)
            {
                processes[i].Start();
                
                // Connect stdout of current process to stdin of next process
                if (i < processes.Count - 1)
                {
                    processes[i].StandardOutput.BaseStream.CopyToAsync(processes[i + 1].StandardInput.BaseStream);
                }
            }

            // Handle output of last process
            var lastProcess = processes[processes.Count - 1];
            if (lastProcess.StartInfo.RedirectStandardOutput)
            {
                string output;
                while ((output = lastProcess.StandardOutput.ReadLine()) != null)
                {
                    ShellIo.Out.WriteLine(output);
                }
            }

            // Wait for all processes to complete (in reverse order)
            for (int i = processes.Count - 1; i >= 0; i--)
            {
                if (i < processes.Count - 1)
                {
                    processes[i + 1].StandardInput.Close();
                }
                processes[i].WaitForExit();
            }
        }
        catch (Exception ex)
        {
            ShellIo.Error.WriteLine($"Pipeline execution error: {ex.Message}");
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
    }
    
    /// <summary>
    /// executes a command based on the provided ParsedCommand.
    /// </summary>
    /// <param name="parsedCommand">The parsed command to execute</param>
    /// <returns>A boolean indicating whether execution should continue</returns>
    public static bool Execute(ParsedCommand parsedCommand)
    {
        if (parsedCommand == null)
            return true;

        var command = parsedCommand.Executable;
        
        // Handle pipeline if present
        if (parsedCommand.IsPipelineStart)
        {
            ExecutePipeline(parsedCommand);
            return true;
        }

        // Set up redirection for the command execution
        using var outputRedirection = SetupRedirection(parsedCommand.Redirects);

        // Check if it's a builtin command
        if (_builtins.TryGetValue(command, out var builtinHandler))
        {
            return builtinHandler(parsedCommand);
        }
        
        string executablePath = PathResolver.FindExecutable(command);
        if (executablePath != null)
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = parsedCommand.Redirects.HasStdOut || ShellIo.Out != Console.Out,
                    RedirectStandardError = parsedCommand.Redirects.HasStdErr || ShellIo.Error != Console.Error
                };

                foreach (var arg in parsedCommand.Arguments)
                {
                    process.StartInfo.ArgumentList.Add(arg);
                }
                
                // Set up process output handling if redirection is needed
                if (process.StartInfo.RedirectStandardOutput)
                {
                    process.OutputDataReceived += (sender, e) => 
                    {
                        if (e.Data != null)
                            ShellIo.Out.WriteLine(e.Data);
                    };
                }

                if (process.StartInfo.RedirectStandardError)
                {
                    process.ErrorDataReceived += (sender, e) => 
                    {
                        if (e.Data != null)
                            ShellIo.Error.WriteLine(e.Data);
                    };
                }
                
                process.Start();

                if (process.StartInfo.RedirectStandardOutput)
                    process.BeginOutputReadLine();
                
                if (process.StartInfo.RedirectStandardError)
                    process.BeginErrorReadLine();

                if (!parsedCommand.RunInBackground)
                {
                    process.WaitForExit();
                }
                return true;
            }
            catch (Exception ex)
            {
                ShellIo.Error.WriteLine($"Error executing '{command}': {ex.Message}");
                return true;
            }
        }
        ShellIo.Out.WriteLine($"{command}: command not found");
        return true;
    }

    // This method handles the "echo" command, which prints its arguments to the output.
    private static bool Echo(ParsedCommand cmd)
    {
        // Skip the command name "echo" and join the remaining arguments
        string output = string.Join(" ", cmd.Arguments);
        ShellIo.Out.WriteLine(output);
        return true;
    }

    // This method handles the "exit" command, which terminates the shell with an optional exit code.
    private static bool Exit(ParsedCommand cmd)
    {
        var exitCode = 0;
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
        return false;
    }

    // This method handles the "type" command, which checks if a command is a shell builtin or an executable.
    private static bool Type(ParsedCommand cmd)
    {
        if (cmd.Arguments.Count < 1)
        {
            return true;
        }
        
        var command = cmd.Arguments[0];

        if (_builtins.ContainsKey(command))
        {
            ShellIo.Out.WriteLine($"{command} is a shell builtin");
        }
        else
        {
            var path = PathResolver.FindExecutable(command);
            if (path != null)
            {
                ShellIo.Out.WriteLine($"{command} is {path}");
            }
            else
            {
                ShellIo.Out.WriteLine($"{command} not found");
            }
        }

        return true;
    }

    // This method handles the "pwd" command, which prints the current working directory.
    private static bool Pwd(ParsedCommand cmd)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        ShellIo.Out.WriteLine(currentDirectory);
        return true;
    }

    // This method handles the "cd" command, which changes the current working directory.
    private static bool Cd(ParsedCommand cmd)
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
