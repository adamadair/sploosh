using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AwaShell.ReadLine;

public class ExternalCommandProvider : ICompletionProvider
{
    private readonly ConcurrentDictionary<string, byte> _exeCache = new();
    public ExternalCommandProvider()
        => BuildCache();

    private void BuildCache()
    {
        var pathExt = OperatingSystem.IsWindows()
            ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.BAT;.CMD")  // semi-colon on Windows
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            : Array.Empty<string>();

        foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "")
                 .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                 .Where(Directory.Exists))
        {
            foreach (var file in Directory.EnumerateFiles(dir))
            {
                var name = Path.GetFileName(file);
                if (OperatingSystem.IsWindows())
                {
                    if (pathExt.Contains(Path.GetExtension(name), StringComparer.OrdinalIgnoreCase))
                        _exeCache.TryAdd(name, 0);
                }
                else
                {
                    // .NET 6+ built-in Unix permission helper
                    if ((File.GetUnixFileMode(file) & UnixFileMode.UserExecute) != 0)
                    {
                        _exeCache.TryAdd(name, 0);
                    }
                }
            }
        }
    }

    public IEnumerable<string> GetCandidates(string token, CompletionContext ctx)
    {
        if (ctx.CompletionStart > -1)
        {
            return [];
        }
        var sc = OperatingSystem.IsWindows()?StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        
        return _exeCache.Keys.Where(k => k.StartsWith(token, sc))
            .Select(c => c.Replace(token, ""));
            
    }

    public bool ContainsCommand(string commandName) => _exeCache.ContainsKey(commandName);

}
