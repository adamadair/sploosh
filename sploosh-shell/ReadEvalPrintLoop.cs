using System;
using System.Linq;

namespace AwaShell;

/// <summary>
/// Read-Eval-Print Loop (REPL) for the shell.
/// </summary>
public static class ReadEvalPrintLoop
{
    public static string Prompt => Settings.Prompt;
    
    public static void Loop()
    {
        // Initialize the ReadLine library
        ReadLine.ReadLine.HistoryEnabled = true;
        ReadLine.ReadLine.InitializeHistory();
        while (true)
        {
            string input = ReadLine.ReadLine.Read(Prompt);
            if (input == null)
                break;
            
            try
            {
                // Parse the input
                string[] tokens = InputParser.Parse(input);

                // Check for empty input
                if (tokens.Length == 0)
                    continue;

                // Create ParsedCommand object
                var parsedCommand = CommandParser.ParseTokens(tokens);
                // Execute the command
                bool continueLoop = CommandManager.Execute(parsedCommand);
                // At this point it is safe to save the command history. 
                // Any commands that cause an exception should not be saved to 
                // history.
                //editor.SaveHistory();
                
                if (!continueLoop)
                    break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                ReadLine.ReadLine.SaveHistory();
            }
        }
    }
}
