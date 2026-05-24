# SyncCloud

SyncCloud is a Windows utility for synchronizing a Microsoft OneDrive folder with a local folder. It can run as a WinForms GUI or as a command-line tool using the same sync engine.

## Requirements

- Windows
- .NET SDK 10.0 or later
- A cloud folder path that contains `OneDrive`

The project targets `net10.0-windows` and uses Windows Forms.

## Build

From the repository root:

```powershell
dotnet build SyncCloud.sln
```

The debug executable is written to:

```text
SyncCloud\bin\Debug\net10.0-windows\SyncCloud.exe
```

## Run The GUI

Running the executable with no arguments opens the WinForms interface:

```powershell
SyncCloud\bin\Debug\net10.0-windows\SyncCloud.exe
```

You can also prefill the GUI controls from the command line:

```powershell
SyncCloud\bin\Debug\net10.0-windows\SyncCloud.exe --gui --cloud "C:\Users\me\OneDrive\Docs" --local "D:\Docs" --mode synchronize
```

## Run From The Command Line

In CLI mode, supplying sync options runs the sync action directly:

```powershell
SyncCloud\bin\Debug\net10.0-windows\SyncCloud.exe --cloud "C:\Users\me\OneDrive\Docs" --local "D:\Docs" --mode synchronize
```

Available options:

| Option | Purpose |
| --- | --- |
| `--cloud <path>`, `--cloud-folder <path>` | Set the cloud folder path. |
| `--local <path>`, `--local-folder <path>` | Set the local folder path. |
| `--mode <synchronize|to-local|to-cloud>` | Select the sync direction. |
| `--remove-cloud[=true|false]` | Delete cloud files after copying them to local when mode is `to-local`. |
| `--no-remove-cloud` | Clear the remove-cloud option. |
| `--show-copy-only[=true|false]`, `--copy-only[=true|false]` | Show copy/remove progress while suppressing check messages. |
| `--no-show-copy-only`, `--no-copy-only` | Clear the copy-only progress option. |
| `--yes`, `-y`, `--force` | Continue when cloud and local folder names do not appear to match. |
| `--sync`, `--run` | Accepted for readability; CLI mode runs sync by default. |
| `--gui` | Open the GUI instead of running the CLI sync. |
| `--help`, `-h`, `/?` | Print command-line help. |

## Sync Behavior

- Both folder paths must exist before sync starts.
- The cloud path must contain `OneDrive`.
- By default, folder names below the OneDrive folder must match the local folder path. Use `--yes` only when the mismatch is intentional.
- Files are copied when the destination is missing or the source file has a newer last-write timestamp.
- `synchronize` copies newer or missing files in both directions.
- `to-local` copies from cloud to local.
- `to-cloud` copies from local to cloud.
- `--remove-cloud` only removes cloud files after a successful copy in `to-local` mode.

The application does not perform content hashing or conflict merging. Timestamp comparison is the current conflict policy.

## Project Layout

```text
SyncCloud.sln
SyncCloud/
  SyncCloud.csproj
  Program.cs
  Form1.cs
  Form1.Designer.cs
  CommandLineArguments.cs
  SyncEngine.cs
  SyncOptions.cs
  SyncResult.cs
  ActionMode.cs
  NativeConsole.cs
```

## Verification

The current project was verified with:

```powershell
dotnet build SyncCloud.sln
SyncCloud\bin\Debug\net10.0-windows\SyncCloud.exe --help
```

Both commands completed successfully on the local Windows workspace.
