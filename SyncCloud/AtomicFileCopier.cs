namespace SyncCloud
{
  internal static class AtomicFileCopier
  {
    private const int BufferSize = 81920;
    private const FileAttributes AccessAttributes = FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System;

    public static async Task CopyAsync(string source, string destination)
    {
      string sourcePath = Path.GetFullPath(source);
      string destinationPath = Path.GetFullPath(destination);
      if (string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase))
      {
        throw new IOException("Source and destination must be different files.");
      }

      // Staging beside the destination keeps the final replacement on the same volume.
      string temporaryPath = Path.Combine(Path.GetDirectoryName(destinationPath)!, ".synccloud-" + Guid.NewGuid().ToString("N") + ".tmp");
      FileAttributes? originalAttributes = null;
      bool restoreAccessAttributes = false;
      try
      {
        DateTime sourceWriteTimeUtc;
        // Open the cloud source before creating anything at the destination.
        using (FileStream input = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true))
        {
          sourceWriteTimeUtc = File.GetLastWriteTimeUtc(sourcePath);
          using (FileStream output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize, useAsync: true))
          {
            await input.CopyToAsync(output);
            if (output.Length != input.Length)
            {
              throw new IOException("The copied length does not match the source: " + sourcePath);
            }
            output.Flush(flushToDisk: true);
          }
        }

        File.SetLastWriteTimeUtc(temporaryPath, sourceWriteTimeUtc);
        if (File.Exists(destinationPath))
        {
          originalAttributes = File.GetAttributes(destinationPath);
          restoreAccessAttributes = true;
          if ((originalAttributes.Value & FileAttributes.ReadOnly) != 0)
          {
            File.SetAttributes(destinationPath, originalAttributes.Value & ~FileAttributes.ReadOnly);
          }
          // Never truncate a good destination, even if opening/reading the source fails.
          File.Replace(temporaryPath, destinationPath, destinationBackupFileName: null);
        }
        else
        {
          // Do not overwrite a file that appeared while this copy was in progress.
          File.Move(temporaryPath, destinationPath);
        }
      }
      finally
      {
        try
        {
          if (restoreAccessAttributes && originalAttributes.HasValue && File.Exists(destinationPath))
          {
            FileAttributes current = File.GetAttributes(destinationPath);
            File.SetAttributes(destinationPath, (current & ~AccessAttributes) | (originalAttributes.Value & AccessAttributes));
          }
        }
        finally
        {
          // File.Delete is harmless after a successful move/replace (the temp is gone).
          File.Delete(temporaryPath);
        }
      }
    }
  }
}
