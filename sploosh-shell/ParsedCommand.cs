using System;
using System.Collections.Generic;

namespace AwaShell;

/// <summary>
/// Represents one fully-parsed shell command, including its arguments
/// and any execution modifiers (redirection, background flag, pipeline, etc.).
/// Immutable so tests stay simple and thread-safety is free.
/// </summary>
public sealed record ParsedCommand(
    string Executable,                       // e.g. "echo", "ls", "grep"
    IReadOnlyList<string> Arguments,         // everything after the command name
    RedirectionInfo Redirects,               // stdout / stderr targets
    bool RunInBackground = false,            // trailing '&'
    ParsedCommand PipeTarget = null)        // the next command in a pipeline (|), null if none
{
    /// <summary>
    /// True if any output should be piped rather than sent to the console.
    /// </summary>
    public bool IsPipelineStart => PipeTarget is not null;
    
}