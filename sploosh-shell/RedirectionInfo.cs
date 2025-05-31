namespace AwaShell;

/// <summary>
/// Captures POSIX-style redirection targets.
/// Currently supports stdout and stderr (overwrite vs append),
/// but can be extended later to cover stdin , here-docs, etc.
/// </summary>
public readonly record struct RedirectionInfo(
    string StdOutTarget,
    bool    AppendStdOut,
    string StdErrTarget,
    bool    AppendStdErr)
{
    public bool HasStdOut => StdOutTarget is not null;
    public bool HasStdErr => StdErrTarget is not null;
    public bool HasAny    => HasStdOut || HasStdErr;

    public static readonly RedirectionInfo None = new(null, false, null, false);
}