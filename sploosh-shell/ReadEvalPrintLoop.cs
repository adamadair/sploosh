namespace AwaShell;

public static class ReadEvalPrintLoop
{
    public static string Prompt { get; set; } = "$";
    
    public static void Loop()
    {
        while (true)
        {
            
            string? input = ShellIo.Prompt(Prompt);
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
