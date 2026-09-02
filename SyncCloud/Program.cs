namespace SyncCloud
{
  internal static class Program
  {
    /// <summary>
    ///  The main entry point for the application.
    ///  This program is a utility to syncronize cloud data into local hard
    /// </summary>
    [STAThread]
    static int Main(string[] args)
    {
      CommandLineArguments commandLineArguments = CommandLineArguments.Parse(args);
      if (commandLineArguments.ShowHelp)
      {
        NativeConsole.EnsureForCommandLine();
        Console.WriteLine(CommandLineArguments.Usage);
        return 0;
      }

      if (commandLineArguments.Errors.Count > 0)
      {
        NativeConsole.EnsureForCommandLine();
        foreach (string error in commandLineArguments.Errors)
        {
          Console.Error.WriteLine("Error: " + error);
        }
        Console.Error.WriteLine();
        Console.Error.WriteLine(CommandLineArguments.Usage);
        return 2;
      }

      if (commandLineArguments.UseGui)
      {
        ApplicationConfiguration.Initialize();
        NativeConsole.DetachForGui();
        Application.Run(new Form1(commandLineArguments.Options));
        return 0;
      }

      NativeConsole.EnsureForCommandLine();
      return RunFromCommandLineAsync(commandLineArguments.Options).GetAwaiter().GetResult();
    }

    private static async Task<int> RunFromCommandLineAsync(SyncOptions options)
    {
      IReadOnlyList<string> errors = SyncEngine.Validate(options);
      if (errors.Count > 0)
      {
        foreach (string error in errors)
        {
          Console.Error.WriteLine("Error: " + error);
        }
        return 2;
      }

      Console.WriteLine("Working...");
      SyncEngine syncEngine = new SyncEngine(message => Console.WriteLine(message));
      try
      {
        SyncResult result = await syncEngine.SyncAsync(options);
        Console.WriteLine("Finished.");
        Console.WriteLine(result.Message);
        return 0;
      }
      catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
      {
        Console.Error.WriteLine($"Sync failed ({error.GetType().Name}, HRESULT 0x{error.HResult:X8}): {error.Message}");
        return 1;
      }
    }
  }
}
