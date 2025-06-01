using System.Collections.Generic;
using System.Linq;

namespace AwaShell.ReadLine;

public class AutoCompleteHandler : IAutoCompleteHandler
{
    private readonly List<string> _builtInCommands = CommandManager.BuiltinCommands.ToList();
    public char[] Separators { get; set; } = [' ', '.', '/'];

    public string[] GetSuggestions(string text, int index)
    {
        return string.IsNullOrWhiteSpace(text) ? [] : _builtInCommands.Where(s => s.StartsWith(text)).ToArray();
    }
}