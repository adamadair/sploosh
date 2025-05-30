namespace AwaShell;
/// <summary>
/// The main entry point for the shell application.
/// </summary>
class Program
{
    /*
     * TODO: Implement a proper command line argument parser to handle options like:
     *  --version, --help, --config, etc.
     */
    static void Main(string[] args)
    {
        // For now, just run the REPL loop
        ReadEvalPrintLoop.Loop();
    }
}