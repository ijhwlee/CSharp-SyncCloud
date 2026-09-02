namespace SyncCloud
{
  public sealed class SyncEngine
  {
    private readonly Action<string> progress;

    public SyncEngine(Action<string>? progress = null)
    {
      this.progress = progress ?? (_ => { });
    }

    public static IReadOnlyList<string> Validate(SyncOptions options)
    {
      List<string> errors = new List<string>();
      if (string.IsNullOrWhiteSpace(options.CloudFolder))
      {
        errors.Add("Cloud folder is required. Use --cloud <path>.");
      }
      else if (!Directory.Exists(options.CloudFolder))
      {
        errors.Add("Cloud folder does not exist: " + options.CloudFolder);
      }

      if (string.IsNullOrWhiteSpace(options.LocalFolder))
      {
        errors.Add("Local folder is required. Use --local <path>.");
      }
      else if (!Directory.Exists(options.LocalFolder))
      {
        errors.Add("Local folder does not exist: " + options.LocalFolder);
      }

      if (errors.Count > 0)
      {
        return errors;
      }

      if (!IsSupportedCloudFolder(options.CloudFolder))
      {
        errors.Add("Cloud folder does not contain OneDrive. Currently only MS OneDrive is supported: " + options.CloudFolder);
      }

      if (!options.AllowFolderMismatch && !FoldersSeemMatched(options.CloudFolder, options.LocalFolder))
      {
        errors.Add("Cloud and local folder names do not seem to match. Re-run with --yes to continue anyway.");
      }

      return errors;
    }

    public static bool IsSupportedCloudFolder(string cloudFolder)
    {
      return cloudFolder.Contains("OneDrive", StringComparison.OrdinalIgnoreCase);
    }

    public static bool FoldersSeemMatched(string cloudFolder, string localFolder)
    {
      string[] cloudTokens = cloudFolder.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      string[] localTokens = localFolder.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

      int count = cloudTokens.Length < localTokens.Length ? cloudTokens.Length : localTokens.Length;
      int idxCloud = cloudTokens.Length - 1;
      int idxLocal = localTokens.Length - 1;
      while (count > 0 && !cloudTokens[idxCloud].Contains("OneDrive", StringComparison.OrdinalIgnoreCase))
      {
        if (!string.Equals(cloudTokens[idxCloud], localTokens[idxLocal], StringComparison.OrdinalIgnoreCase))
        {
          return false;
        }

        idxCloud--;
        idxLocal--;
        count--;
      }

      return true;
    }

    public async Task<SyncResult> SyncAsync(SyncOptions options)
    {
      int syncedFiles = await SyncDirectoryAsync(options.CloudFolder, options.LocalFolder, options);
      return new SyncResult(syncedFiles, options.CloudFolder, options.LocalFolder);
    }

    private async Task<int> SyncDirectoryAsync(string cloudFolder, string localFolder, SyncOptions options)
    {
      int syncedFiles = 0;
      HashSet<string> syncedChildFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      if (options.Mode == ActionMode.toLocal || options.Mode == ActionMode.Synchronize)
      {
        syncedFiles += await EnsureChildFoldersAsync(
          sourceParent: cloudFolder,
          targetParent: localFolder,
          sourceIsCloud: true,
          options,
          syncedChildFolders);
      }

      if (options.Mode == ActionMode.toCloud || options.Mode == ActionMode.Synchronize)
      {
        syncedFiles += await EnsureChildFoldersAsync(
          sourceParent: localFolder,
          targetParent: cloudFolder,
          sourceIsCloud: false,
          options,
          syncedChildFolders);
      }

      if (options.Mode == ActionMode.toLocal || options.Mode == ActionMode.Synchronize)
      {
        syncedFiles += await SyncFilesToLocalAsync(cloudFolder, localFolder, options);
      }

      if (options.Mode == ActionMode.toCloud || options.Mode == ActionMode.Synchronize)
      {
        syncedFiles += await SyncFilesToCloudAsync(cloudFolder, localFolder, options);
      }

      return syncedFiles;
    }

    private async Task<int> EnsureChildFoldersAsync(
      string sourceParent,
      string targetParent,
      bool sourceIsCloud,
      SyncOptions options,
      HashSet<string> syncedChildFolders)
    {
      int syncedFiles = 0;
      string sourceLabel = sourceIsCloud ? "cloud" : "local";
      string[] sourceFolders = await Task.Run(() => Directory.GetDirectories(sourceParent));
      foreach (string sourceFolder in sourceFolders)
      {
        if (!options.ShowCopyOnly)
        {
          progress("   Checking " + sourceLabel + " folder " + sourceFolder);
        }

        string childFolderName = Path.GetFileName(sourceFolder);
        string targetFolder = Path.Combine(targetParent, childFolderName);
        if (!Directory.Exists(targetFolder))
        {
          await Task.Run(() => Directory.CreateDirectory(targetFolder));
        }

        if (!syncedChildFolders.Add(childFolderName))
        {
          continue;
        }

        syncedFiles += await SyncDirectoryAsync(
          sourceIsCloud ? sourceFolder : targetFolder,
          sourceIsCloud ? targetFolder : sourceFolder,
          options);
      }

      return syncedFiles;
    }

    private async Task<int> SyncFilesToLocalAsync(string cloudFolder, string localFolder, SyncOptions options)
    {
      int syncedFiles = 0;
      string[] cloudFiles = await Task.Run(() => Directory.GetFiles(cloudFolder));
      foreach (string cloudFile in cloudFiles)
      {
        string localName = Path.Combine(localFolder, Path.GetFileName(cloudFile));
        if (!options.ShowCopyOnly)
        {
          progress("Synching file : " + cloudFile + " to local : " + localName);
        }

        if (!File.Exists(localName) || File.GetLastWriteTimeUtc(cloudFile).ToFileTime() > File.GetLastWriteTimeUtc(localName).ToFileTime())
        {
          await AtomicFileCopier.CopyAsync(cloudFile, localName);
          progress("Copying file : " + cloudFile + " to local : " + localName);
          if (options.RemoveCloud && options.Mode == ActionMode.toLocal)
          {
            await Task.Run(() => File.Delete(cloudFile));
            progress("    Removing file : " + cloudFile + " from cloud ");
          }

          syncedFiles++;
        }
      }

      return syncedFiles;
    }

    private async Task<int> SyncFilesToCloudAsync(string cloudFolder, string localFolder, SyncOptions options)
    {
      int syncedFiles = 0;
      string[] localFiles = await Task.Run(() => Directory.GetFiles(localFolder));
      foreach (string localFile in localFiles)
      {
        string cloudName = Path.Combine(cloudFolder, Path.GetFileName(localFile));
        if (!options.ShowCopyOnly)
        {
          progress("Synching file : " + localFile + " to cloud : " + cloudName);
        }

        if (!File.Exists(cloudName) || File.GetLastWriteTimeUtc(localFile).ToFileTime() > File.GetLastWriteTimeUtc(cloudName).ToFileTime())
        {
          await AtomicFileCopier.CopyAsync(localFile, cloudName);
          progress("Copying file : " + localFile + " to cloud : " + cloudName);
          syncedFiles++;
        }
      }

      return syncedFiles;
    }

  }
}
