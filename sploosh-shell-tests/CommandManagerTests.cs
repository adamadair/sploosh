using System.Text.Json;
using AwaShell;
using Xunit.Abstractions;

namespace sploosh_shell_tests;

public class CommandManagerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CommandManagerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    // Command Text: echo "adam adair was a good old man" | wc -w
    const string ParsedCommandWithPipelineJson = """
    {"Executable":"echo","Arguments":["adam adair was a good old man"],"Redirects":{"StdOutTarget":null,"AppendStdOut":false,"StdErrTarget":null,"AppendStdErr":false,"HasStdOut":false,"HasStdErr":false,"HasAny":false},"RunInBackground":false,"PipeTarget":{"Executable":"wc","Arguments":["-w"],"Redirects":{"StdOutTarget":null,"AppendStdOut":false,"StdErrTarget":null,"AppendStdErr":false,"HasStdOut":false,"HasStdErr":false,"HasAny":false},"RunInBackground":false,"PipeTarget":null,"IsPipelineStart":false},"IsPipelineStart":true}
    """;
    [Fact]
    public void Execute_ExecuteParsedCommand_WithPipeline_ShouldExecute()
    {
        var parsedCommand = JsonSerializer.Deserialize<ParsedCommand>(ParsedCommandWithPipelineJson);
        CommandManager.Execute(parsedCommand);
    }
}