namespace AwaShell.Utils;

public static class StringUtils
{
    public static string FindCommonPrefix(string[] strings)
    {
        if (strings == null || strings.Length == 0)
            return string.Empty;

        var prefix = strings[0];

        foreach (var str in strings)
        {
            while (!str.StartsWith(prefix))
            {
                prefix = prefix[..^1];
                if (prefix == string.Empty)
                    return string.Empty;
            }
        }

        return prefix;
    }
}

