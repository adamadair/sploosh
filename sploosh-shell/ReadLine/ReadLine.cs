using System;
using System.Collections.Generic;
using AwaShell.ReadLine.Abstractions;

namespace AwaShell.ReadLine;

public static class ReadLine
{
    private static List<string> _history;

    static ReadLine()
    {
        _history = [];
        AutoCompletionHandler = new AutoCompleteHandler();
    }

    public static void AddHistory(params string[] text) => _history.AddRange(text);
    public static List<string> GetHistory() => _history;
    public static void ClearHistory() => _history = [];
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
}