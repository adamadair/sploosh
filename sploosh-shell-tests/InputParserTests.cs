using AwaShell;
using Xunit;

namespace sploosh_shell_tests;


public class InputParserTests
{
    [Fact]
    public void Parse_ShouldHandleUnquotedInput()
    {
        var input = "arg1 arg2 arg3";
        var result = InputParser.Parse(input);

        Assert.Equal(new[] { "arg1", "arg2", "arg3" }, result);
    }
    
    private static InputParserTest[] DoubleQuotedTests = new InputParserTest[]
    {
        new InputParserTest { Input = "\"/tmp/quz/'f \\28\\'\"", Expected = new []{"/tmp/quz/'f \\28\\'" }},
    };
    

    [Fact]
    public void Parse_ShouldHandleDoubleQuotedInput()
    {
        foreach (var test in DoubleQuotedTests)
        {
            var result = InputParser.Parse(test.Input);
            Assert.Equal(test.Expected, result);
        }
    }

    [Fact]
    public void Parse_ShouldHandleSingleQuotedInput()
    {
    var input = "'arg1 arg2' arg3";
    var result = InputParser.Parse(input);

    Assert.Equal(new[] { "arg1 arg2", "arg3" }, result);
    }

    [Fact]
    public void Parse_ShouldHandleEscapedCharacters()
    {

    }

    [Fact]
    public void Parse_ShouldThrowExceptionForUnclosedDoubleQuotes()
    {
        var input = "\"arg1 arg2";

        Assert.Throws<FormatException>(() => InputParser.Parse(input));
    }

    [Fact]
    public void Parse_ShouldThrowExceptionForUnclosedSingleQuotes()
    {
        var input = "'arg1 arg2";

        Assert.Throws<FormatException>(() => InputParser.Parse(input));
    }

    [Fact]
    public void Parse_ShouldThrowExceptionForTrailingEscapeCharacter()
    {
        var input = "arg1 arg2\\";

        Assert.Throws<FormatException>(() => InputParser.Parse(input));
    }
}

class InputParserTest
{
    public InputParserTest() 
    { 
        Input = string.Empty;
        Expected = [];
    }
    public string Input {get;set;}
    public string[] Expected {get;set;}
}


