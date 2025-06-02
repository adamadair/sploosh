using System.Collections.Generic;

namespace AwaShell.ReadLine;

public interface ICompletionProvider
{
    IEnumerable<string> GetCandidates(string token, CompletionContext ctx);
}

public record CompletionContext(string FullLine, int CompletionStart);