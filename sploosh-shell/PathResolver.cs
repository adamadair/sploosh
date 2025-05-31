using System;
using System.Collections.Generic;
using System.IO;

namespace AwaShell;

public static class PathResolver
{
    private static readonly Dictionary<string, string> _executableCache = new();
    private static DateTime _lastPathUpdate = DateTime.MinValue;
    private static string _lastPathValue = string.Empty;

    public static string FindExecutable(string command)
    {
        // Check if PATH has changed
        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        if (currentPath != _lastPathValue)
        {
            _executableCache.Clear();
            _lastPathValue = currentPath;
            _lastPathUpdate = DateTime.Now;
        }

        // Return from cache if available
        if (_executableCache.TryGetValue(command, out string cachedPath))
            return cachedPath;

        // Search in PATH directories
        var pathDirs = currentPath.Split(Path.PathSeparator);
        foreach (var dir in pathDirs)
        {
            if (string.IsNullOrWhiteSpace(dir)) continue;

            var fullPath = Path.Combine(dir, command);
            
            // Check for file with or without extensions (Windows vs Unix)
            if (File.Exists(fullPath))
            {
                _executableCache[command] = fullPath;
                return fullPath;
            }
            
            if (OperatingSystem.IsWindows() && File.Exists(fullPath + ".exe"))
            {
                _executableCache[command] = fullPath + ".exe";
                return fullPath + ".exe";
            }
        }

        _executableCache[command] = string.Empty; // Cache that it wasn't found
        return null;
    }
}