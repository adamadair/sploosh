using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AwaShell.BuiltInCommands;

namespace AwaShell;

// This class is responsible for managing and executing commands in the shell.
public static class CommandManager
{
    
    // Readonly property to get the list of built-in commands
    public static HashSet<string> BuiltinCommands =>
    [
        ..BuiltIns.Commands
            .Select(cmd => cmd.Name)
    ];

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
    /// Walks a chain of piped ParsedCommand objects and returns them in execution order.
    /// </summary>
    private static List<ParsedCommand> CollectPipeline(ParsedCommand start)
    {
        var list = new List<ParsedCommand>();
        var current = start;
        while (current != null)
        {
            list.Add(current);
            current = current.PipeTarget; // or PipelineTarget, depending on your field name
        }
        return list;
    }
    
    private static bool ExecutePipeline(ParsedCommand command)
    {
        if (command == null)
            return true;

        // If the pipeline has no builtins, we can use a simpler approach
        if (!command.IsPipelineStart || !CollectPipeline(command).Any(c => BuiltinCommands.Contains(c.Executable)))
        {
            return ExecutePipelineWithNoBuiltins(command);
        }

        // Otherwise, we need to handle builtins and external commands
        return ExecutePipelineWithBuiltins(command);
    }

    /// <summary>
    /// Executes a pipeline of commands with no builtins -> Passes #BR6 tests
    /// </summary>
    private static bool ExecutePipelineWithNoBuiltins(ParsedCommand command)
    {
        var commands = new List<ParsedCommand>();
        var current = command;
        while (current != null)
        {
            commands.Add(current);
            current = current.PipeTarget;
        }

        int count = commands.Count;
        var processes = new List<Process>(count);

        // Create the pipe connections ahead of time
        // Removed unused 'streams' variable declaration to simplify the code.

        for (int i = 0; i < count; i++)
        {
            var cmd = commands[i];
            var isFirst = i == 0;
            var isLast = i == count - 1;

            var psi = new ProcessStartInfo
            {
                FileName = cmd.Executable,
                Arguments = string.Join(" ", cmd.Arguments),
                UseShellExecute = false,
                RedirectStandardInput = !isFirst,
                RedirectStandardOutput = !isLast,
                RedirectStandardError = true
            };

            var proc = new Process { StartInfo = psi };
            processes.Add(proc);
        }

        // Start processes
        foreach (var proc in processes)
            proc.Start();

        // Set up piping: from stdout of i to stdin of i+1
        var copyTasks = new List<Task>();
        for (int i = 0; i < count - 1; i++)
        {
            var source = processes[i].StandardOutput.BaseStream;
            var target = processes[i + 1].StandardInput.BaseStream;

            // Stream the data and close stdin of the next process when done
            var copyTask = Task.Run(async () =>
            {
                await source.CopyToAsync(target);
                target.Close();
            });

            copyTasks.Add(copyTask);
        }

        // Wait for output to finish copying
        Task.WaitAll(copyTasks.ToArray());

        // Wait for processes in reverse order (important)
        for (int i = count - 1; i >= 0; i--)
            processes[i].WaitForExit();

        return true;
    }

    
    /// <summary>
    /// Executes a pipeline of commands with at lease one builtin -> Passes #NY9 tests
    /// Without a proper fork we need to handle stream redirection manually.
    /// </summary>
    private static bool ExecutePipelineWithBuiltins(ParsedCommand start)
    {
        var pipeline = CollectPipeline(start);
        int stages = pipeline.Count;

        var buffers = new List<StringWriter>();
        for (int i = 0; i < stages - 1; i++)
            buffers.Add(new StringWriter());

        TextReader originalIn = ShellIo.In;
        TextWriter originalOut = ShellIo.Out;
        TextWriter originalErr = ShellIo.Error;

        try
        {
            for (int i = 0; i < stages; i++)
            {
                var cmd = pipeline[i];
                bool isBuiltin = BuiltinCommands.Contains(cmd.Executable);
                bool isFirst = i == 0;
                bool isLast = i == stages - 1;

                // Input: first = original input, otherwise use previous buffer
                if (isFirst)
                    ShellIo.SetIn(originalIn);
                else
                    ShellIo.SetIn(new StringReader(buffers[i - 1].ToString()));

                // Output: last = original output, otherwise write to buffer
                if (isLast)
                    ShellIo.SetOut(originalOut);
                else
                    ShellIo.SetOut(buffers[i]);

                ShellIo.SetError(originalErr); // always use original error for now

                if (isBuiltin)
                {
                    BuiltIns.GetBuiltInCommand(cmd.Executable).Execute(cmd);
                }
                else
                {
                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = cmd.Executable,
                            UseShellExecute = false,
                            RedirectStandardInput = !isFirst,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        }
                    };

                    foreach (var arg in cmd.Arguments)
                        proc.StartInfo.ArgumentList.Add(arg);

                    proc.Start();

                    // Write previous buffer (if any) to process stdin
                    if (!isFirst)
                    {
                        using var writer = proc.StandardInput;
                        writer.Write(ShellIo.In.ReadToEnd()); // safely read redirected input
                    }

                    // Read output and write it forward (or to terminal if last)
                    if (isLast)
                    {
                        using var reader = proc.StandardOutput;
                        var buffer = new char[4096];
                        int bytesRead;
                        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ShellIo.Out.Write(buffer, 0, bytesRead);
                        }
                    }
                    else
                    {
                        using var reader = proc.StandardOutput;
                        var buffer = new char[4096];
                        int bytesRead;
                        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            buffers[i].Write(buffer, 0, bytesRead);
                        }
                    }

                    // Read and emit errors
                    string error = proc.StandardError.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(error))
                        ShellIo.Error.Write(error);

                    proc.WaitForExit();
                }
            }
        }
        finally
        {
            ShellIo.SetIn(originalIn);
            ShellIo.SetOut(originalOut);
            ShellIo.SetError(originalErr);
        }

        return true;
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
        if (BuiltIns.IsBuiltInCommand(parsedCommand.Executable))
        {
            return BuiltIns.GetBuiltInCommand(parsedCommand.Executable).Execute(parsedCommand);
        }
        
        var executablePath = PathResolver.FindExecutable(command);
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
    
}
