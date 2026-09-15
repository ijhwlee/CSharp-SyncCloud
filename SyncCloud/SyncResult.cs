namespace SyncCloud
{
  public sealed class SyncResult
  {
    public SyncResult(int syncedFiles, string cloudFolder, string localFolder, int skippedCloudFiles = 0)
    {
      SyncedFiles = syncedFiles;
      CloudFolder = cloudFolder;
      LocalFolder = localFolder;
      SkippedCloudFiles = skippedCloudFiles;
    }

    public int SyncedFiles { get; }
    public string CloudFolder { get; }
    public string LocalFolder { get; }
    public int SkippedCloudFiles { get; }

    public string Message
    {
      get
      {
        string skippedText = SkippedCloudFiles > 0
          ? " Skipped " + SkippedCloudFiles + " inaccessible cloud " + (SkippedCloudFiles == 1 ? "file." : "files.")
          : "";

        if (SyncedFiles > 0)
        {
          return "Cloud : " + CloudFolder + " and local : " + LocalFolder + " are synched for " + SyncedFiles + " files." + skippedText;
        }

        return "Cloud : " + CloudFolder + " and local : " + LocalFolder + " are already synched." + skippedText;
      }
    }
  }
}
