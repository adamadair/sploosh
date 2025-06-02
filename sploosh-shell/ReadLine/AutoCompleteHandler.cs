using System.Collections.Generic;
using System.Linq;

namespace AwaShell.ReadLine;

internal class AutoCompleteHandler : IAutoCompleteHandler
{
    private readonly BuiltinCompletionProvider _builtinCompletionProvider = new BuiltinCompletionProvider();
    private readonly ExternalCommandProvider _externalCommandProvider = new ExternalCommandProvider();
    public char[] Separators { get; set; } = [' ', '.', '/'];

    public string[] GetSuggestions(string text, int index)
    {
        var token = GetToken(text, index);
        var ctx = new CompletionContext(text, index);
        var suggestions = _builtinCompletionProvider.GetCandidates(token, ctx).ToList();
        if (suggestions.Count > 0)
        {
            return suggestions.ToArray();
        }

        suggestions.AddRange(_externalCommandProvider.GetCandidates(token, ctx));
        return suggestions.ToArray();
    }

    private string GetToken(string text, int index)
    {
        if (index < 0)
        {
            return text;
        }
        
        // Extract and return the token
        return text.Substring(index);
    }
}

