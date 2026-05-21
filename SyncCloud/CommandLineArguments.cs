namespace SyncCloud
{
  internal sealed class CommandLineArguments
  {
    public static readonly string Usage = string.Join(
      Environment.NewLine,
      "SyncCloud command line usage:",
      "  SyncCloud.exe --cloud <OneDrive path> --local <local path> [options]",
      "",
      "Options:",
      "  --cloud, --cloud-folder <path>       Set the Cloud Folder edit box.",
      "  --local, --local-folder <path>       Set the Local Folder edit box.",
      "  --mode <synchronize|to-local|to-cloud>",
      "                                      Select the mode radio button.",
      "  --remove-cloud[=true|false]          Set the RemoveCloud check box.",
      "  --no-remove-cloud                    Clear the RemoveCloud check box.",
      "  --show-copy-only[=true|false]        Set the showCopyOnly check box.",
      "  --copy-only[=true|false]             Alias for --show-copy-only.",
      "  --no-show-copy-only                  Clear the showCopyOnly check box.",
      "  --yes, -y, --force                   Continue when folder names do not match.",
      "  --sync, --run                        Run the sync button action. This is optional in CLI mode.",
      "  --gui                                Open the WinForms UI, prefilled with supplied options.",
      "  --help, -h, /?                       Show this help text.",
      "",
      "Examples:",
      "  SyncCloud.exe --cloud \"C:\\Users\\me\\OneDrive\\Docs\" --local \"D:\\Docs\" --mode synchronize",
      "  SyncCloud.exe --cloud \"C:\\Users\\me\\OneDrive\\Photos\" --local \"D:\\Photos\" --mode to-local --remove-cloud --show-copy-only");

    private CommandLineArguments()
    {
    }

    public SyncOptions Options { get; } = new SyncOptions();
    public bool ShowHelp { get; private set; }
    public bool UseGui { get; private set; }
    public List<string> Errors { get; } = new List<string>();

    public static CommandLineArguments Parse(string[] args)
    {
      CommandLineArguments result = new CommandLineArguments();
      if (args.Length == 0)
      {
        result.UseGui = true;
        return result;
      }

      for (int i = 0; i < args.Length; i++)
      {
        string arg = args[i];
        string lower = arg.ToLowerInvariant();
        switch (lower)
        {
          case "--help":
          case "-h":
          case "/?":
            result.ShowHelp = true;
            break;
          case "--gui":
            result.UseGui = true;
            break;
          case "--sync":
          case "--run":
            break;
          case "--cloud":
          case "--cloud-folder":
            result.Options.CloudFolder = ReadValue(args, ref i, arg, result.Errors);
            break;
          case "--local":
          case "--local-folder":
            result.Options.LocalFolder = ReadValue(args, ref i, arg, result.Errors);
            break;
          case "--mode":
            ParseMode(ReadValue(args, ref i, arg, result.Errors), result);
            break;
          case "--remove-cloud":
            result.Options.RemoveCloud = ReadOptionalBool(args, ref i, true, arg, result.Errors);
            break;
          case "--no-remove-cloud":
            result.Options.RemoveCloud = false;
            break;
          case "--show-copy-only":
          case "--copy-only":
            result.Options.ShowCopyOnly = ReadOptionalBool(args, ref i, true, arg, result.Errors);
            break;
          case "--no-show-copy-only":
          case "--no-copy-only":
            result.Options.ShowCopyOnly = false;
            break;
          case "--yes":
          case "-y":
          case "--force":
            result.Options.AllowFolderMismatch = true;
            break;
          default:
            if (TryReadAssignment(arg, "--cloud=", value => result.Options.CloudFolder = value) ||
                TryReadAssignment(arg, "--cloud-folder=", value => result.Options.CloudFolder = value) ||
                TryReadAssignment(arg, "--local=", value => result.Options.LocalFolder = value) ||
                TryReadAssignment(arg, "--local-folder=", value => result.Options.LocalFolder = value) ||
                TryReadAssignment(arg, "--mode=", value => ParseMode(value, result)) ||
                TryReadAssignment(arg, "--remove-cloud=", value => result.Options.RemoveCloud = ParseBool(value, arg, result.Errors)) ||
                TryReadAssignment(arg, "--show-copy-only=", value => result.Options.ShowCopyOnly = ParseBool(value, arg, result.Errors)) ||
                TryReadAssignment(arg, "--copy-only=", value => result.Options.ShowCopyOnly = ParseBool(value, arg, result.Errors)))
            {
              break;
            }

            result.Errors.Add("Unknown option: " + arg);
            break;
        }
      }

      return result;
    }

    private static string ReadValue(string[] args, ref int index, string option, List<string> errors)
    {
      if (index + 1 >= args.Length || args[index + 1].StartsWith("-"))
      {
        errors.Add("Missing value for " + option + ".");
        return "";
      }

      index++;
      return args[index];
    }

    private static bool ReadOptionalBool(string[] args, ref int index, bool defaultValue, string option, List<string> errors)
    {
      if (index + 1 >= args.Length || args[index + 1].StartsWith("-"))
      {
        return defaultValue;
      }

      index++;
      return ParseBool(args[index], option, errors);
    }

    private static bool ParseBool(string value, string option, List<string> errors)
    {
      switch (value.ToLowerInvariant())
      {
        case "true":
        case "yes":
        case "1":
        case "on":
          return true;
        case "false":
        case "no":
        case "0":
        case "off":
          return false;
        default:
          errors.Add("Invalid boolean value for " + option + ": " + value);
          return false;
      }
    }

    private static void ParseMode(string value, CommandLineArguments result)
    {
      switch (value.ToLowerInvariant())
      {
        case "sync":
        case "synchronize":
        case "synchronized":
        case "both":
          result.Options.Mode = ActionMode.Synchronize;
          break;
        case "to-local":
        case "tolocal":
        case "local":
          result.Options.Mode = ActionMode.toLocal;
          break;
        case "to-cloud":
        case "tocloud":
        case "cloud":
          result.Options.Mode = ActionMode.toCloud;
          break;
        default:
          result.Errors.Add("Invalid mode: " + value + ". Use synchronize, to-local, or to-cloud.");
          break;
      }
    }

    private static bool TryReadAssignment(string arg, string prefix, Action<string> assign)
    {
      if (!arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
      {
        return false;
      }

      assign(arg.Substring(prefix.Length));
      return true;
    }
  }
}
