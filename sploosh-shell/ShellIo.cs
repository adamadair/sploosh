using System;
using System.IO;

namespace AwaShell;

public class ShellIo
{
    public static TextWriter Out { get; private set; }
    private static TextWriter Log { get; set; }
    public static TextReader In { get; private set; }
    public static TextWriter Error { get; private set; }

    static ShellIo()
    {
        Out = Console.Out;
        Log = Console.Error;
        In = Console.In;
        Error = Console.Error;
    }
    
    public static void SetOut(TextWriter w)
    {
        Out = w;
    }

    public static void SetLog(TextWriter w)
    {
        Log = w;
    }

    public static void SetIn(TextReader r)
    {
        In = r;
    }

    public static void SetError(TextWriter w)
    {
        Error = w;
    }
    
    public static string Prompt(string p = "$")
    {
        Out.Write($"{p} ");
        Out.Flush();
        return In.ReadLine();
    }
}
