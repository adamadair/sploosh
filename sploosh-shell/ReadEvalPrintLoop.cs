using System;

namespace AwaShell;

public static class ReadEvalPrintLoop
{
    public static string Prompt => Settings.Prompt;
    
    public static void Loop()
    {
        
        while (true)
        {
            //InitCommandDictionary();
            var editor = new LineEditor();
            string input = editor.Edit(Prompt,"");
            if (input == null)
                break;
            
            try
            {
                // Parse the input
                string[] args = InputParser.Parse(input);

                // Check for empty input
                if (args.Length == 0)
                    continue;

                // Execute the command
                bool continueLoop = CommandManager.Execute(args);
                // At this point it is safe to save the command history. 
                // Any commands that cause an exception should not be saved to 
                // history.
                editor.SaveHistory();
                
                if (!continueLoop)
                    break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
