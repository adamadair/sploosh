using AwaShell.ReadLine;

namespace sploosh_shell_tests;

public class ExternalCommandProviderTests
{
    [Fact]
    public void GetCandidates_ShouldReturnExternalCommands_WhenTokenMatches()
    {
        var provider = new ExternalCommandProvider();
        var token = "wh";
        var ctx = new CompletionContext("wh", -1);
        
        var candidates = provider.GetCandidates(token, ctx).ToList();
        Assert.True(candidates.Count > 0, "Expected to find candidates for the token 'wh' but found none.");
        Assert.Contains("ich", candidates);
    }

    [Fact]
    public void ContainsCommand_ShouldContainLsCommand()
    {
        var commandsToTest = new[] { "ls", "which", "grep" };
        var provider = new ExternalCommandProvider();
        
        foreach (var command in commandsToTest)
        {
            var result = provider.ContainsCommand(command);
            Assert.True(result, $"Expected provider to contain command '{command}' but it did not.");
        }
    }
}