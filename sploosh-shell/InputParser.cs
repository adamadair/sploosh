namespace AwaShell;

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

            // Handle escape character (only outside of single quotes)
            if (c == '\\' && !escaped && !inSingleQuotes)
            {
                escaped = true;
                // When inside double quotes, we need to preserve the backslash
                if (inDoubleQuotes)
                {
                    currentArg.Append('\\');
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
                currentArg.Append(c);
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

