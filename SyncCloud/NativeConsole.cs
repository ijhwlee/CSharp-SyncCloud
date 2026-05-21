using System.Runtime.InteropServices;

namespace SyncCloud
{
  internal static class NativeConsole
  {
    private const int AttachParentProcess = -1;

    public static void EnsureForCommandLine()
    {
      AttachConsole(AttachParentProcess);
      ResetConsoleWriters();
    }

    public static void DetachForGui()
    {
      FreeConsole();
    }

    private static void ResetConsoleWriters()
    {
      Stream output = Console.OpenStandardOutput();
      if (output != Stream.Null)
      {
        Console.SetOut(new StreamWriter(output) { AutoFlush = true });
      }

      Stream error = Console.OpenStandardError();
      if (error != Stream.Null)
      {
        Console.SetError(new StreamWriter(error) { AutoFlush = true });
      }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeConsole();
  }
}
