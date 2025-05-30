using System;
using System.Collections.Generic;

namespace AwaShell;
/*
 
 # Parsing Rules
   The shell parsing rules define how the shell interprets and processes commands. This includes how it handles special 
   characters, quoting, expansions, and control structures. Understanding these rules is crucial for writing effective 
   shell scripts and commands.
   
   ## Quoting
   
   Quoting is used to remove the special meaning of certain characters or words to the shell. Quoting can be used to 
   disable special treatment for special characters, to prevent reserved words from being recognized as such, and to 
   prevent parameter expansion.
   
   Each of the shell metacharacters (see Definitions) has special meaning to the shell and must be quoted if it is to 
   represent itself. When the command history expansion facilities are being used (see History Expansion), the history 
   expansion character, usually ‘!’, must be quoted to prevent history expansion. See Bash History Facilities, for more 
   details concerning history expansion.
   
   There are three quoting mechanisms: 
   
   1. the escape character 
   2. single quotes
   3. double quotes.
   
   
   ### The Escape Character
   
   A non-quoted backslash ‘\’ is the Bash escape character. It preserves the literal value of the next character that 
   follows, with the exception of newline. If a \newline pair appears, and the backslash itself is not quoted, the 
   \newline is treated as a line continuation (that is, it is removed from the input stream and effectively ignored).
   
   ### Single Quotes
   
   Enclosing characters in single quotes (‘'’) preserves the literal value of each character within the quotes. 
   A single quote may not occur between single quotes, even when preceded by a backslash.
   
   ### Double Quotes
   
   Enclosing characters in double quotes (‘"’) preserves the literal value of all characters within the quotes, with
   the exception of ‘$’, ‘`’, ‘\’, and, when history expansion is enabled, ‘!’. When the shell is in POSIX mode (see 
   Bash POSIX Mode), the ‘!’ has no special meaning within double quotes, even when history expansion is enabled. The 
   characters ‘$’ and ‘`’ retain their special meaning within double quotes (see Shell Expansions). The backslash 
   retains its special meaning only when followed by one of the following characters: ‘$’, ‘`’, ‘"’, ‘\’, or newline. 
   Within double quotes, backslashes that are followed by one of these characters are removed. Backslashes preceding 
   characters without a special meaning are left unmodified. A double quote may be quoted within double quotes by 
   preceding it with a backslash. If enabled, history expansion will be performed unless an ‘!’ appearing in double 
   quotes is escaped using a backslash. The backslash preceding the ‘!’ is not removed.
   
   The special parameters ‘*’ and ‘@’ have special meaning when in double quotes (see Shell Parameter Expansion). 
 
 */

public static class InputParser
{
    public static string[] Parse(string input)
    {
        var arguments = new List<string>();
        var currentArg = new System.Text.StringBuilder();
        var inDoubleQuotes = false;
        var inSingleQuotes = false;
        var escaped = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            // Handle escape character (only outside single quotes)
            if (c == '\\' && !inSingleQuotes)
            {
                if (!escaped)
                {
                    escaped = true;
                }
                else
                {
                    currentArg.Append(c);
                    escaped = false;
                }

                continue;
            }

            // Handle double quotes (only if not in single quotes)
            if (c == '"' && !escaped && !inSingleQuotes)
            {
                inDoubleQuotes = !inDoubleQuotes;
                continue;
            }

            // Handle single quotes (only if not in double quotes and not escaped)
            if (c == '\'' && !inDoubleQuotes && !escaped)
            {
                inSingleQuotes = !inSingleQuotes;
                continue;
            }

            // Handle spaces (only if not in quotes)
            if ((c == ' ' || c == '\t') && !inDoubleQuotes && !inSingleQuotes && !escaped)
            {
                if (currentArg.Length > 0)
                {
                    arguments.Add(currentArg.ToString());
                    currentArg.Clear();
                }

                continue;
            }

            // Inside single quotes, everything is literal (including backslashes)
            if (inSingleQuotes)
            {
                currentArg.Append(c);
                continue;
            }

            // Handle escaped characters within double quotes
            if (escaped)
            {
                if (inDoubleQuotes)
                {
                    if(c=='$' || c=='`' || c=='"' || c=='\\')
                    {
                        currentArg.Append(c);
                    }
                    else
                    {
                        // If the escape is not followed by a special character, treat it as a literal backslash
                        currentArg.Append('\\');
                        currentArg.Append(c);
                    }
                }
                else
                {
                    currentArg.Append(c);
                }
                escaped = false;
                continue;
            }

            // Default behavior: append character
            currentArg.Append(c);
        }

        // Add the last argument if not empty
        if (currentArg.Length > 0)
        {
            arguments.Add(currentArg.ToString());
        }

        // Check for unclosed quotes
        if (inDoubleQuotes)
        {
            throw new FormatException("Unclosed double quotation mark");
        }

        if (inSingleQuotes)
        {
            throw new FormatException("Unclosed single quotation mark");
        }

        // Check for trailing escape
        if (escaped)
        {
            throw new FormatException("Trailing escape character");
        }

        return arguments.ToArray();
    }
}

