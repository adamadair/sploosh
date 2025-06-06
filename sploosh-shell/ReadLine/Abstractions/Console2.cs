using System;
using static System.Console;

namespace AwaShell.ReadLine.Abstractions;

internal class Console2(string prompt) : IConsole
{
    public string Prompt => prompt;
    public void WritePrompt() => Console.Write(prompt); 
    public int CursorLeft => Console.CursorLeft;

    public int CursorTop => Console.CursorTop;

    public int BufferWidth => Console.BufferWidth;

    public int BufferHeight => Console.BufferHeight;

    public bool PasswordMode { get; set; }

#pragma warning disable CA1416
    public void SetBufferSize(int width, int height) => Console.SetBufferSize(width, height);
#pragma warning restore CA1416

    public void SetCursorPosition(int left, int top)
    {
        if (!PasswordMode)
            Console.SetCursorPosition(left, top);
    }

    public void Write(string value)
    {
        if (PasswordMode)
            value = new string('\0', value.Length);

        Console.Write(value);
    }

    public void WriteLine(string value) => Console.WriteLine(value);

    public void Clear() => Console.Clear();
}