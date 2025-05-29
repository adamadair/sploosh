namespace AwaShell;
using System;
using System.Collections.Generic;
using System.IO;
//using System.Runtime.InteropServices;
//#if !WINDOWS
//using Mono.Unix.Native;
//#endif

// This class checks if a file is likely to be an executable based on its extension, magic bytes, or shebang line.
public static class ExecutableChecker
{
    private static readonly HashSet<string> Extensions;

    static ExecutableChecker()
    {
        
        if (OperatingSystem.IsWindows())
        {
            Extensions = [".exe", ".bat", ".cmd", ".com", ".msi"];
        }
        else
        {
            Extensions = ["", ".sh", ".run", ".bin"];
        }
    }
    
    public static bool IsLikelyExecutable(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        return HasExecutableExtension(filePath) 
               || HasExecutableMagicBytes(filePath) 
               || HasShebang(filePath);
               //|| HasUnixExecutePermission(filePath);
    }

    private static bool HasExecutableExtension(string filePath)
    {


        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return Extensions.Contains(ext);
    }

    private static bool HasExecutableMagicBytes(string filePath)
    {
        byte[] header = new byte[4];

        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            if (fs.Length < 4)
                return false;

            fs.ReadExactly(header, 0, 4);
        }

        // Windows PE
        if (header[0] == 'M' && header[1] == 'Z')
            return true;

        // Linux ELF
        if (header[0] == 0x7F && header[1] == (byte)'E' &&
            header[2] == (byte)'L' && header[3] == (byte)'F')
            return true;

        // macOS Mach-O
        uint magic = BitConverter.ToUInt32(header, 0);
        if (magic == 0xFEEDFACE || magic == 0xCEFAEDFE || // 32-bit
            magic == 0xFEEDFACF || magic == 0xCFFAEDFE)   // 64-bit
            return true;

        return false;
    }
    
    private static bool HasShebang(string filePath)
    {
        try
        {
            using var reader = new StreamReader(filePath);
            var firstLine = reader.ReadLine();
            return firstLine != null && firstLine.StartsWith("#!");
        }
        catch
        {
            return false; // Not a text-readable file, probably not a script
        }
    }

    /*
    private static bool HasUnixExecutePermission(string filePath)
    {
#if WINDOWS
        return false; // Windows doesn't use POSIX execute permissions
#else
        // Check if the file is executable by user, group, or others
        var stat = new Stat();
        if (Syscall.stat(filePath, out stat) == 0)
        {
            var mode = stat.st_mode;
            var uid = Syscall.geteuid();
            var gid = Syscall.getegid();

            bool isOwner = uid == stat.st_uid;
            bool isGroup = gid == stat.st_gid;

            return (isOwner && (mode & FilePermissions.S_IXUSR) != 0) ||
                   (isGroup && (mode & FilePermissions.S_IXGRP) != 0) ||
                   ((mode & FilePermissions.S_IXOTH) != 0);
        }

        return false;
#endif
    }
    */
}
