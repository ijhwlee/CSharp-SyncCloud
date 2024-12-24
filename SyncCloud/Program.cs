namespace SyncCloud
{
  internal static class Program
  {
    /// <summary>
    ///  The main entry point for the application.
    ///  This program is a utility to syncronize cloud data into local hard
    /// </summary>
    [STAThread]
    static void Main()
    {
      // To customize application configuration such as set high DPI settings or default font,
      // see https://aka.ms/applicationconfiguration.
      ApplicationConfiguration.Initialize();
      Application.Run(new Form1());
    }
  }
}