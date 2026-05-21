namespace SyncCloud
{
  public sealed class SyncResult
  {
    public SyncResult(int syncedFiles, string cloudFolder, string localFolder)
    {
      SyncedFiles = syncedFiles;
      CloudFolder = cloudFolder;
      LocalFolder = localFolder;
    }

    public int SyncedFiles { get; }
    public string CloudFolder { get; }
    public string LocalFolder { get; }

    public string Message
    {
      get
      {
        if (SyncedFiles > 0)
        {
          return "Cloud : " + CloudFolder + " and local : " + LocalFolder + " are synched for " + SyncedFiles + " files.";
        }

        return "Cloud : " + CloudFolder + " and local : " + LocalFolder + " are already synched.";
      }
    }
  }
}
