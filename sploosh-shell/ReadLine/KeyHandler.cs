using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwaShell.ReadLine.Abstractions;
using AwaShell.Utils;

namespace AwaShell.ReadLine;

internal class KeyHandler
{
    private int _cursorPos;
    private int _cursorLimit;
    private readonly StringBuilder _text;
    private readonly List<string> _history;
    private int _historyIndex;
    private ConsoleKeyInfo _keyInfo;
    private readonly Dictionary<string, Action> _keyActions;
    private readonly IConsole _console;
    private bool _isAutoCompleteMode;

    private bool IsStartOfLine() => _cursorPos == 0;

    private bool IsEndOfLine() => _cursorPos == _cursorLimit;
    private bool IsLastChar() => _cursorPos == _cursorLimit || _cursorPos == _cursorLimit - 1;
    private bool IsBlank() => _cursorPos >= _cursorLimit || _text[_cursorPos] == ' ';

    private bool IsStartOfBuffer() => _console.CursorLeft == 0;

    private bool IsEndOfBuffer() => _console.CursorLeft == _console.BufferWidth - 1;

    private void MoveCursorLeft()
    {
        if (IsStartOfLine())
            return;

        if (IsStartOfBuffer())
            _console.SetCursorPosition(_console.BufferWidth - 1, _console.CursorTop - 1);
        else
            _console.SetCursorPosition(_console.CursorLeft - 1, _console.CursorTop);

        _cursorPos--;
    }

    private void MoveCursorHome()
    {
        while (!IsStartOfLine())
            MoveCursorLeft();
    }

    private string BuildKeyInput() => _keyInfo.Modifiers == 0
        ? $"{_keyInfo.Key}"
        : $"{_keyInfo.Modifiers}{_keyInfo.Key}";

    private void MoveCursorRight()
    {
        if (IsEndOfLine())
            return;

        if (IsEndOfBuffer())
            _console.SetCursorPosition(0, _console.CursorTop + 1);
        else
            _console.SetCursorPosition(_console.CursorLeft + 1, _console.CursorTop);

        _cursorPos++;
    }

    private void MoveCursorEnd()
    {
        while (!IsEndOfLine())
            MoveCursorRight();
    }

    private void ClearScreen() {
        ClearLine();
        _console.Clear();
        _console.WritePrompt();
    }

    private void ClearLine()
    {
        if(!IsEndOfLine())
            MoveCursorEnd();
        for (var i = 0; i < _text.Length; i++)
        {
            Console.Write("\b \b");
        }
        _text.Clear();
        _cursorPos = 0;
        _cursorLimit = 0;
    }

    private void SkipBlanks(bool backwards = false) {
        Action moveCursor = backwards ? MoveCursorLeft : MoveCursorRight;
        moveCursor();
        while (!IsStartOfLine() && !IsEndOfLine() && IsBlank())
            moveCursor();
    }

    private void WriteNewString(string str)
    {
        ClearLine();
        foreach (char character in str)
            WriteChar(character);
    }

    private void WriteString(string str)
    {
        foreach (char character in str)
            WriteChar(character);
    }

    private void WriteChar() {
        // solves bug when typing things like ControlLeftArrow...
        // ONLY ACCEPTS PRINTABLE CHARACTERS
        if (_keyInfo.KeyChar >= ' ') 
            WriteChar(_keyInfo.KeyChar);
    }

    private void WriteChar(char c)
    {
        if (IsEndOfLine())
        {
            _text.Append(c);
            _console.Write(c.ToString());
            _cursorPos++;
        }
        else
        {
            int left = _console.CursorLeft;
            int top = _console.CursorTop;
            string str = _text.ToString().Substring(_cursorPos);
            _text.Insert(_cursorPos, c);
            _console.Write(c.ToString() + str);
            _console.SetCursorPosition(left, top);
            MoveCursorRight();
        }

        _cursorLimit++;
    }

    private void ReplaceChar(char c) {
        if (IsEndOfLine()) return;
        _text[_cursorPos++] = c;
        Console.Write($"{c}");
    }

    private void Backspace()
    {
        if (IsStartOfLine())
            return;

        MoveCursorLeft();
        int index = _cursorPos;
        _text.Remove(index, 1);
        string replacement = _text.ToString().Substring(index);
        int left = _console.CursorLeft;
        int top = _console.CursorTop;
        _console.Write(string.Format("{0} ", replacement));
        _console.SetCursorPosition(left, top);
        _cursorLimit--;
    }

    private void Delete()
    {
        if (IsEndOfLine())
            return;

        int index = _cursorPos;
        _text.Remove(index, 1);
        string replacement = _text.ToString().Substring(index);
        int left = _console.CursorLeft;
        int top = _console.CursorTop;
        _console.Write(string.Format("{0} ", replacement));
        _console.SetCursorPosition(left, top);
        _cursorLimit--;
    }

    private void TransposeChars()
    {
        // local helper functions
        bool AlmostEndOfLine() => (_cursorLimit - _cursorPos) == 1;
        int IncrementIf(Func<bool> expression, int index) =>  expression() ? index + 1 : index;
        int DecrementIf(Func<bool> expression, int index) => expression() ? index - 1 : index;

        if (IsStartOfLine()) { return; }

        var firstIdx = DecrementIf(IsEndOfLine, _cursorPos - 1);
        var secondIdx = DecrementIf(IsEndOfLine, _cursorPos);

        (_text[firstIdx], _text[secondIdx]) = (_text[secondIdx], _text[firstIdx]);
        var left = IncrementIf(AlmostEndOfLine, _console.CursorLeft);
        var cursorPosition = IncrementIf(AlmostEndOfLine, _cursorPos);

        WriteNewString(_text.ToString());

        _console.SetCursorPosition(left, _console.CursorTop);
        _cursorPos = cursorPosition;

        MoveCursorRight();
    }

    private void PrevHistory()
    {
        if (_historyIndex > 0)
        {
            _historyIndex--;
            WriteNewString(_history[_historyIndex]);
        }
    }

    private void NextHistory()
    {
        if (_historyIndex < _history.Count)
        {
            _historyIndex++;
            if (_historyIndex == _history.Count)
                ClearLine();
            else
                WriteNewString(_history[_historyIndex]);
        }
    }
    private void OneWordBackward() {
        SkipBlanks(backwards: true);
        while (!IsStartOfLine() && _text[_cursorPos - 1] != ' ')
            MoveCursorLeft();
    }
    private void OneWordForward() {
        while (!IsLastChar() && _text[_cursorPos + 1] != ' ')
            MoveCursorRight();
        SkipBlanks();
    }
    private void DeletePreviousWord() {
        if (IsStartOfLine())
            return;
        // deletes contiguous blanks or the previous word
        Func<bool> stop = _text[_cursorPos - 1] == ' '
            ? () => _text[_cursorPos - 1] != ' '
            : () => _text[_cursorPos - 1] == ' ';
        while (!IsStartOfLine() && !stop())
            Backspace();
    }

    private void EndOfWord() {
        while (!IsEndOfLine() && !IsBlank())
            MoveCursorRight();
    }

    public string Text
    {
        get
        {
            return _text.ToString();
        }
    }

    public KeyHandler(IConsole console, List<string> history, IAutoCompleteHandler autoCompleteHandler)
    {
        this._console = console;

        _history = history ?? [];
        _historyIndex = _history.Count;
        _text = new StringBuilder();
        _keyActions = new Dictionary<string, Action> {
            ["LeftArrow"] = MoveCursorLeft,
            ["Home"] = MoveCursorHome,
            ["End"] = MoveCursorEnd,
            ["ControlA"] = MoveCursorHome,
            ["ControlB"] = MoveCursorLeft,
            ["RightArrow"] = MoveCursorRight,
            ["ControlF"] = MoveCursorRight,
            ["ControlE"] = MoveCursorEnd,
            ["Backspace"] = Backspace,
            ["Delete"] = Delete,
            ["ControlD"] = Delete,
            ["ControlH"] = Backspace,
            ["ControlL"] = ClearScreen,
            ["Escape"] = ClearLine,
            ["UpArrow"] = PrevHistory,
            ["ControlP"] = PrevHistory,
            ["DownArrow"] = NextHistory,
            ["ControlN"] = NextHistory,
            ["ControlU"] = () => {
                while (!IsStartOfLine())
                    Backspace();
            },
            ["ControlK"] = () => {
                int pos = _cursorPos;
                MoveCursorEnd();
                while (_cursorPos > pos)
                    Backspace();
            },
            ["ControlW"] = () => {
                while (!IsStartOfLine() && _text[_cursorPos - 1] != ' ')
                    Backspace();
            },
            ["ControlT"] = TransposeChars,
            ["ControlLeftArrow"] = OneWordBackward,
            ["AltB"] = OneWordBackward,
            ["ControlRightArrow"] = OneWordForward,
            ["AltF"] = OneWordForward,
            ["ControlBackspace"] = DeletePreviousWord,
            ["AltC"] = () => {
                // Capitalizes the current char and moves to the end of the word
                if (IsBlank()) return;
                ReplaceChar(char.ToUpperInvariant(_text[_cursorPos]));
                EndOfWord();
            },
            ["AltU"] = () => {
                // Capitalizes every char from the cursor to the end of the word
                while(!(IsBlank() || IsEndOfLine())) 
                    ReplaceChar(char.ToUpperInvariant(_text[_cursorPos]));    
            },
            ["AltL"] = () => {
                // Lowers the case of every char from the cursor to the end of the word
                while (!(IsBlank() || IsEndOfLine()))
                    ReplaceChar(char.ToLowerInvariant(_text[_cursorPos]));
            },
            ["Tab"] = () => {
                string text = _text.ToString();
                if(autoCompleteHandler == null || !IsEndOfLine() || string.IsNullOrEmpty(text))
                    return; 
                var completionStart = text.LastIndexOfAny(autoCompleteHandler.Separators);
                var completions1 = autoCompleteHandler.GetSuggestions(text, completionStart);
                if (completions1.Length == 1)
                {
                    // If there's only one completion, write it directly
                    WriteString($"{completions1[0]} ");
                    _isAutoCompleteMode = false;
                }
                else if (completions1.Length > 1)
                {
                    //  #WT6 Partial completions - look for common prefix
                    var prefix = StringUtils.FindCommonPrefix(completions1);
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        // If there's a common prefix, write it
                        WriteString(prefix);
                        return;
                    }
                    
                    if (!_isAutoCompleteMode)
                    {
                        Console.Write("\x07"); // Bell character to indicate multiple completions. Alert the user when entering auto-complete mode.
                        _isAutoCompleteMode = true;
                    }
                    else
                    {
                        // #WH6 Multiple completions
                        // user has pressed Tab again, so we print the list of candidate completions
                        ShellIo.Out.WriteLine();
                        var completions = completions1.Select(s => text + s).Order();
                        var line = string.Join("  ", completions);
                        ShellIo.Out.WriteLine(line);
                        ShellIo.Out.Write($"{ShellSettings.Instance.Prompt}{text}");
                    }
                }
                else
                {
                    Console.Write("\x07");
                }            
            },

            ["ShiftTab"] = () => {
                Console.Write("\x07");
            }
        };
    }

    public void Handle(ConsoleKeyInfo keyInfo)
    {
        _keyInfo = keyInfo;

        // If in auto complete mode and Tab wasn't pressed
        if (_isAutoCompleteMode && _keyInfo.Key != ConsoleKey.Tab)
            _isAutoCompleteMode = false;

        _keyActions.TryGetValue(BuildKeyInput(), out Action action);

        (action ?? WriteChar).Invoke();
    }
}