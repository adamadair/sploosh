namespace AwaShell;

class Program
{
    static void Main(string[] args)
    {
        // Initialize the shell
        //ShellIo.Init();
        
        // Run the REPL loop
        ReadEvalPrintLoop.Loop();
        
        // Cleanup if necessary
        //ShellIo.Cleanup();
    }
}