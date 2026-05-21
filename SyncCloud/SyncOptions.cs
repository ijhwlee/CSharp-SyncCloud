namespace SyncCloud
{
  public sealed class SyncOptions
  {
    public string CloudFolder { get; set; } = "";
    public string LocalFolder { get; set; } = "";
    public bool RemoveCloud { get; set; }
    public bool ShowCopyOnly { get; set; }
    public bool AllowFolderMismatch { get; set; }
    public ActionMode Mode { get; set; } = ActionMode.Synchronize;
  }
}
