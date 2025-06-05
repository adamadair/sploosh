using System;
using System.Linq;

namespace AwaShell.BuiltInCommands;

public class History : IBuiltInCommand
{
    public string Name => "history";
    
    public string HelpText => "history [n] - Display the command history. If a number n is specified, display only the last n commands.";
    
    public bool Execute(ParsedCommand cmd)
    {
        var history = ReadLine.ReadLine.GetHistory();
        if (cmd.Arguments.Count > 0)
        {
            var flag = cmd.Arguments[0];
            switch (flag)
            {
                case "-c":
                case "--clear":
                    return ClearHistory();
                case "-r":
                    return ReadHistoryFromFile(cmd.Arguments.ToArray());
                case "-w":
                    return WriteHistoryToFile(cmd.Arguments.ToArray());
                case "-a":
                    return AppendHistoryToFile(cmd.Arguments.ToArray());
            }
        }

        if (history.Count == 0)
        {
            ShellIo.Out.WriteLine("No history available.");
            return true;
        }
        var startIndex = 0;
        if (cmd.Arguments.Count > 0 && int.TryParse(cmd.Arguments[0], out var count))
        {
            if (count > 0)
            {
                startIndex = Math.Max(0, history.Count - count);
            }
        }
        
        //Write the history to the output
        for (var i = startIndex; i < history.Count; i++)
        {
            ShellIo.Out.WriteLine($"    {i+1}  {history[i]}");
        }
        return true;
    }

    private static bool ClearHistory()
    {
        ReadLine.ReadLine.ClearHistory();
        ShellIo.Out.WriteLine("History cleared.");
        return true;
    }
    private static bool ReadHistoryFromFile(string[] args)
    {
        if (args.Length < 2)
        {
            ShellIo.Out.WriteLine("Filename to read history from is required.");
            return true;
        }
        var filename = args[1];
        ReadLine.ReadLine.LoadHistoryFromFile(filename);
        return true;
    }
    
    private static bool WriteHistoryToFile(string[] args)
    {
        if (args.Length < 2)
        {
            ShellIo.Out.WriteLine("Filename to write history to is required.");
            return true;
        }
        var filename = args[1];
        ReadLine.ReadLine.WriteHistoryToFile(filename);
        return true;
    }
    
    private static bool AppendHistoryToFile(string[] args)
    {
        if (args.Length < 2)
        {
            ShellIo.Out.WriteLine("Filename to append history to is required.");
            return true;
        }
        var filename = args[1];
        ReadLine.ReadLine.WriteHistoryToFile(filename, append: true);
        return true;
    }
}
