using System.Runtime.InteropServices;
using System.Text;

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
      Encoding outputEncoding = GetConsoleOutputEncoding();
      Stream output = Console.OpenStandardOutput();
      if (output != Stream.Null)
      {
        Console.SetOut(new StreamWriter(output, outputEncoding) { AutoFlush = true });
      }

      Encoding errorEncoding = GetConsoleOutputEncoding();
      Stream error = Console.OpenStandardError();
      if (error != Stream.Null)
      {
        Console.SetError(new StreamWriter(error, errorEncoding) { AutoFlush = true });
      }
    }

    private static Encoding GetConsoleOutputEncoding()
    {
      Encoding encoding = Console.OutputEncoding;
      if (encoding.CodePage == Encoding.UTF8.CodePage)
      {
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
      }

      return encoding;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeConsole();
  }
}
