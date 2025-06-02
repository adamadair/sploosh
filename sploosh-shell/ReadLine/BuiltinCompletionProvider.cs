using System.Collections.Generic;
using System.Linq;

namespace AwaShell.ReadLine;

/// <summary>
/// Provides completion candidates for built-in commands in the shell.
/// </summary>
public class BuiltinCompletionProvider : ICompletionProvider
{
    private readonly List<string> _builtInCommands = CommandManager.BuiltinCommands.ToList();
    public IEnumerable<string> GetCandidates(string token, CompletionContext ctx)
    {
        if (ctx.CompletionStart > -1)
        {
            return [];
        }
        return string.IsNullOrWhiteSpace(token) ? [] : 
            _builtInCommands.Where(s => s.StartsWith(token))
                .Select(c => c.Replace(token, ""))
                .ToArray();
    }
}