using System;
using System.Collections.Generic;
using System.Linq;
using AwaShell.ReadLine.Abstractions;

namespace AwaShell.ReadLine;

public static class ReadLine
{
    private static List<string> _history;
    private static int _lastAppendedIndex = 0; // Track the last appended index for history
    
    
    static ReadLine()
    {
        _history = [];
        AutoCompletionHandler = new AutoCompleteHandler();
    }

    public static void AddHistory(params string[] text) => _history.AddRange(text);
    public static List<string> GetHistory() => _history;
    public static void ClearHistory()
    { 
        _history = [];
        _lastAppendedIndex = 0;
    } 
    public static bool HistoryEnabled { get; set; }
    private static IAutoCompleteHandler AutoCompletionHandler { get; set; }

    public static string Read(string prompt = "", string @default = "")
    {
        var console = new Console2(prompt);
        console.WritePrompt();
        var keyHandler = new KeyHandler(console, _history, AutoCompletionHandler);
        var text = GetText(keyHandler);

        if (string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(@default))
        {
            text = @default;
        }
        else
        {
            if (HistoryEnabled)
                AddHistory(text);
        }

        return text;
    }

    private static void AddHistory(string commandText)
    {
        var lastCommand = _history.Count > 0 ? _history[^1] : string.Empty;
        if (lastCommand != commandText)
        {
            _history.Add(commandText);
        }
    }

    public static string ReadPassword(string prompt = "")
    {
        var console = new Console2(prompt) { PasswordMode = true };
        console.WritePrompt();
        var keyHandler = new KeyHandler(console, null, null);
        return GetText(keyHandler);
    }

    private static string GetText(KeyHandler keyHandler)
    {
        var keyInfo = Console.ReadKey(true);
        while (keyInfo.Key != ConsoleKey.Enter)
        {
            keyHandler.Handle(keyInfo);
            keyInfo = Console.ReadKey(true);
        }

        Console.WriteLine();
        return keyHandler.Text;
    }
    
    public static void WriteHistoryToFile(string filePath, bool append = false)
    {
        if (_history.Count == 0)
            return;

        try
        {
            if (append)
            {
                var historyToAppend = _history.Skip(_lastAppendedIndex).ToList();
                System.IO.File.AppendAllLines(filePath, historyToAppend);
                _lastAppendedIndex = _history.Count; // Update the last appended index
                return;
            }
            System.IO.File.WriteAllLines(filePath, _history);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing history to file: {ex.Message}");
        }
    }
    
    public static void LoadHistoryFromFile(string filePath, bool append = false)
    {
        try
        {
            if (!System.IO.File.Exists(filePath)) return;
            var loadedHistory = new List<string>(System.IO.File.ReadAllLines(filePath));
            if (append)
            {
                
                _history.AddRange(loadedHistory);
            }
            else
            {
                var oldHistory = _history; 
                _history = loadedHistory;
                if (oldHistory.Count > 0)
                {
                    _history.Insert(0,oldHistory.Last());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading history from file: {ex.Message}");
        }
    }

    public static void InitializeHistory()
    {
        var fileName = Environment.GetEnvironmentVariable("HISTFILE");
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = ShellSettings.SettingsFile;
        }
        LoadHistoryFromFile(fileName);
    }
    
}