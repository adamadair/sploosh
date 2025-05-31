using AwaShell;
using System;
using System.Linq;
using Xunit;

namespace sploosh_shell_tests;

public class CommandParserTests
{
    [Fact]
    public void ParseTokens_ShouldParseSimpleCommand()
    {
        // Arrange
        var tokens = new[] { "ls", "-la" };

        // Act
        var result = CommandParser.ParseTokens(tokens);

        // Assert
        Assert.Equal("ls", result.Executable);
        Assert.Single(result.Arguments);
        Assert.Equal("-la", result.Arguments[0]);
        Assert.False(result.RunInBackground);
        Assert.False(result.Redirects.HasAny);
        Assert.Null(result.PipeTarget);
    }

    [Fact]
    public void ParseTokens_ShouldHandleStdoutRedirection()
    {
        // Arrange
        var tokens = new[] { "echo", "hello", ">", "output.txt" };

        // Act
        var result = CommandParser.ParseTokens(tokens);

        // Assert
        Assert.Equal("echo", result.Executable);
        Assert.Single(result.Arguments);
        Assert.Equal("hello", result.Arguments[0]);
        Assert.True(result.Redirects.HasStdOut);
        Assert.Equal("output.txt", result.Redirects.StdOutTarget);
        Assert.False(result.Redirects.AppendStdOut);
    }

    [Fact]
    public void ParseTokens_ShouldHandleStdoutAppendRedirection()
    {
        // Arrange
        var tokens = new[] { "echo", "hello", ">>", "output.txt" };

        // Act
        var result = CommandParser.ParseTokens(tokens);

        // Assert
        Assert.Equal("echo", result.Executable);
        Assert.Single(result.Arguments);
        Assert.Equal("hello", result.Arguments[0]);
        Assert.True(result.Redirects.HasStdOut);
        Assert.Equal("output.txt", result.Redirects.StdOutTarget);
        Assert.True(result.Redirects.AppendStdOut);
    }

    [Fact]
    public void ParseTokens_ShouldHandleStderrRedirection()
    {
        // Arrange
        var tokens = new[] { "grep", "pattern", "file.txt", "2>", "error.log" };

        // Act
        var result = CommandParser.ParseTokens(tokens);

        // Assert
        Assert.Equal("grep", result.Executable);
        Assert.Equal(2, result.Arguments.Count);
        Assert.Equal("pattern", result.Arguments[0]);
        Assert.Equal("file.txt", result.Arguments[1]);
        Assert.True(result.Redirects.HasStdErr);
        Assert.Equal("error.log", result.Redirects.StdErrTarget);
        Assert.False(result.Redirects.AppendStdErr);
    }

    [Fact]
    public void ParseTokens_ShouldHandleStderrAppendRedirection()
    {
        // Arrange
        var tokens = new[] { "grep", "pattern", "file.txt", "2>>", "error.log" };

        // Act
        var result = CommandParser.ParseTokens(tokens);

        // Assert
        Assert.Equal("grep", result.Executable);
        Assert.Equal(2, result.Arguments.Count);
        Assert.Equal("pattern", result.Arguments[0]);
        Assert.Equal("file.txt", result.Arguments[1]);
        Assert.True(result.Redirects.HasStdErr);
        Assert.Equal("error.log", result.Redirects.StdErrTarget);
        Assert.True(result.Redirects.AppendStdErr);
    }

    [Fact]
    public void ParseTokens_ShouldHandleBothStdoutAndStderrRedirection()
    {
        // Arrange
        var tokens = new[] { "command", ">", "output.txt", "2>", "error.log" };

        // Act
        var result = CommandParser.ParseTokens(tokens);

        // Assert
        Assert.Equal("command", result.Executable);
        Assert.Empty(result.Arguments);
        Assert.True(result.Redirects.HasStdOut);
        Assert.Equal("output.txt", result.Redirects.StdOutTarget);
        Assert.True(result.Redirects.HasStdErr);
        Assert.Equal("error.log", result.Redirects.StdErrTarget);
    }

    [Fact]
    public void ParseTokens_ShouldHandleBackgroundExecution()
    {
        // Arrange
        var tokens = new[] { "sleep", "60", "&" };

        // Act
        var result = CommandParser.ParseTokens(tokens);

        // Assert
        Assert.Equal("sleep", result.Executable);
        Assert.Single(result.Arguments);
        Assert.Equal("60", result.Arguments[0]);
        Assert.True(result.RunInBackground);
    }

    [Fact]
    public void ParseTokens_ShouldHandlePipeline()
    {
        // Arrange
        var tokens = new[] { "ls", "-la", "|", "grep", "txt" };

        // Act
        var result = CommandParser.ParseTokens(tokens);

        // Assert
        Assert.Equal("ls", result.Executable);
        Assert.Single(result.Arguments);
        Assert.Equal("-la", result.Arguments[0]);
        Assert.NotNull(result.PipeTarget);
        Assert.True(result.IsPipelineStart);
        
        // Check the pipe target
        Assert.Equal("grep", result.PipeTarget.Executable);
        Assert.Single(result.PipeTarget.Arguments);
        Assert.Equal("txt", result.PipeTarget.Arguments[0]);
    }

    [Fact]
    public void ParseTokens_ShouldHandleComplexPipeline()
    {
        // Arrange
        var tokens = new[] { "find", ".", "-name", "*.cs", "|", "grep", "class", "|", "wc", "-l" };

        // Act
        var result = CommandParser.ParseTokens(tokens);

        // Assert
        Assert.Equal("find", result.Executable);
        Assert.Equal(3, result.Arguments.Count);
        Assert.Equal(".", result.Arguments[0]);
        Assert.Equal("-name", result.Arguments[1]);
        Assert.Equal("*.cs", result.Arguments[2]);
        
        // First pipe
        Assert.NotNull(result.PipeTarget);
        Assert.Equal("grep", result.PipeTarget.Executable);
        Assert.Single(result.PipeTarget.Arguments);
        Assert.Equal("class", result.PipeTarget.Arguments[0]);
        
        // Second pipe
        Assert.NotNull(result.PipeTarget.PipeTarget);
        Assert.Equal("wc", result.PipeTarget.PipeTarget.Executable);
        Assert.Single(result.PipeTarget.PipeTarget.Arguments);
        Assert.Equal("-l", result.PipeTarget.PipeTarget.Arguments[0]);
    }

    [Fact]
    public void ParseTokens_ShouldHandlePipelineWithRedirection()
    {
        // Arrange
        var tokens = new[] { "ls", "-la", "|", "grep", "txt", ">", "results.txt" };

        // Act
        var result = CommandParser.ParseTokens(tokens);

        // Assert
        Assert.Equal("ls", result.Executable);
        Assert.Single(result.Arguments);
        Assert.Equal("-la", result.Arguments[0]);
        
        // Check the pipe target with redirection
        Assert.NotNull(result.PipeTarget);
        Assert.Equal("grep", result.PipeTarget.Executable);
        Assert.Single(result.PipeTarget.Arguments);
        Assert.Equal("txt", result.PipeTarget.Arguments[0]);
        Assert.True(result.PipeTarget.Redirects.HasStdOut);
        Assert.Equal("results.txt", result.PipeTarget.Redirects.StdOutTarget);
    }

    [Fact]
    public void ParseTokens_EmptyTokensArray_ThrowsArgumentException()
    {
        // Arrange
        var tokens = Array.Empty<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => CommandParser.ParseTokens(tokens));
        Assert.Equal("Tokens cannot be empty", exception.Message);
    }

    [Fact]
    public void ParseTokens_MissingStdoutRedirectionTarget_ThrowsFormatException()
    {
        // Arrange
        var tokens = new[] { "echo", "hello", ">" };

        // Act & Assert
        var exception = Assert.Throws<FormatException>(() => CommandParser.ParseTokens(tokens));
        Assert.Equal("Missing target for stdout redirection", exception.Message);
    }

    [Fact]
    public void ParseTokens_MissingStderrRedirectionTarget_ThrowsFormatException()
    {
        // Arrange
        var tokens = new[] { "echo", "hello", "2>" };

        // Act & Assert
        var exception = Assert.Throws<FormatException>(() => CommandParser.ParseTokens(tokens));
        Assert.Equal("Missing target for stderr redirection", exception.Message);
    }

    [Fact]
    public void ParseTokens_MissingPipeTarget_ThrowsFormatException()
    {
        // Arrange
        var tokens = new[] { "ls", "-la", "|" };

        // Act & Assert
        var exception = Assert.Throws<FormatException>(() => CommandParser.ParseTokens(tokens));
        Assert.Equal("Missing target for pipeline", exception.Message);
    }

    [Fact]
    public void ParseTokens_StdoutAndStdErrToSameFile()
    {
        // Arrange
        var tokens = new[] { "command", ">", "output.txt", "2>", "output.txt" };

        // Act
        var result = CommandParser.ParseTokens(tokens);

        // Assert
        Assert.Equal("command", result.Executable);
        Assert.Empty(result.Arguments);
        Assert.True(result.Redirects.HasStdOut);
        Assert.Equal("output.txt", result.Redirects.StdOutTarget);
        Assert.True(result.Redirects.HasStdErr);
        Assert.Equal("output.txt", result.Redirects.StdErrTarget);
    }
}
