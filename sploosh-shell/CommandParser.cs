using System;
using System.Collections.Generic;

namespace AwaShell;

/// <summary>
/// CommandParser is responsible for parsing a string array of tokens into a ParsedCommand.
/// </summary>
public static class CommandParser
{
    public static ParsedCommand ParseTokens(string[] tokens)
    {
        if (tokens.Length == 0)
            throw new ArgumentException("Tokens cannot be empty");

        var executable = tokens[0];
        var arguments = new List<string>();
        string stdoutTarget = null;
        var appendStdout = false;
        string stderrTarget = null;
        var appendStderr = false;
        var runInBackground = false;
        ParsedCommand pipeTarget = null;

        for (var i = 1; i < tokens.Length; i++)
        {
            var token = tokens[i];

            switch (token)
            {
                case ">":
                case "1>":
                    if (i + 1 >= tokens.Length)
                        throw new FormatException("Missing target for stdout redirection");
                    stdoutTarget = tokens[++i];
                    appendStdout = false;
                    break;

                case ">>":
                case "1>>":
                    if (i + 1 >= tokens.Length)
                        throw new FormatException("Missing target for stdout append redirection");
                    stdoutTarget = tokens[++i];
                    appendStdout = true;
                    break;

                case "2>":
                    if (i + 1 >= tokens.Length)
                        throw new FormatException("Missing target for stderr redirection");
                    stderrTarget = tokens[++i];
                    appendStderr = false;
                    break;

                case "2>>":
                    if (i + 1 >= tokens.Length)
                        throw new FormatException("Missing target for stderr append redirection");
                    stderrTarget = tokens[++i];
                    appendStderr = true;
                    break;

                case "|":
                    if (i + 1 >= tokens.Length)
                        throw new FormatException("Missing target for pipeline");
                    pipeTarget = ParseTokens(tokens[(i + 1)..]);
                    i = tokens.Length; // Stop further parsing
                    break;

                case "&":
                    runInBackground = true;
                    break;

                default:
                    arguments.Add(token);
                    break;
            }
        }

        var redirectionInfo = new RedirectionInfo(stdoutTarget, appendStdout, stderrTarget, appendStderr);
        return new ParsedCommand(executable, arguments, redirectionInfo, runInBackground, pipeTarget);
    }
}