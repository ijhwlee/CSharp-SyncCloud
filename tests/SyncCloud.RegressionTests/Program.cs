using SyncCloud;

string testRoot = Path.Combine(Path.GetTempPath(), "SyncCloud-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(testRoot);
int passed = 0;
try
{
  await Run("new file: bytes and source timestamp", async dir =>
  {
    var (source, destination) = Paths(dir);
    byte[] bytes = Enumerable.Range(0, 200000).Select(i => (byte)(i % 251)).ToArray();
    File.WriteAllBytes(source, bytes);
    DateTime timestamp = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
    File.SetLastWriteTimeUtc(source, timestamp);
    await AtomicFileCopier.CopyAsync(source, destination);
    Require(File.ReadAllBytes(destination).SequenceEqual(bytes), "Copied bytes differ.");
    Require(File.GetLastWriteTimeUtc(destination) == timestamp, "Timestamp was not preserved.");
  });
  await Run("successful replacement preserves access attributes", async dir =>
  {
    var (source, destination) = Paths(dir);
    File.WriteAllText(source, "complete new data");
    File.WriteAllText(destination, "old data");
    FileAttributes flags = FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System;
    File.SetAttributes(destination, flags);
    await AtomicFileCopier.CopyAsync(source, destination);
    Require(File.ReadAllText(destination) == "complete new data", "Replacement failed.");
    Require((File.GetAttributes(destination) & flags) == flags, "Access attributes changed.");
    Require(File.GetLastWriteTimeUtc(destination) == File.GetLastWriteTimeUtc(source), "Replacement timestamp differs.");
  });
  await Run("missing source does not create a destination", async dir =>
  {
    var (source, destination) = Paths(dir);
    await ExpectIoFailure(() => AtomicFileCopier.CopyAsync(source, destination));
    Require(!File.Exists(destination), "Failed copy created a destination.");
  });
  await Run("missing source preserves existing data and attributes", async dir =>
  {
    var (source, destination) = Paths(dir);
    File.WriteAllText(destination, "valuable original");
    File.SetAttributes(destination, FileAttributes.ReadOnly);
    DateTime originalTime = File.GetLastWriteTimeUtc(destination);
    await ExpectIoFailure(() => AtomicFileCopier.CopyAsync(source, destination));
    Require(File.ReadAllText(destination) == "valuable original", "Original was truncated.");
    Require(File.GetLastWriteTimeUtc(destination) == originalTime, "Original timestamp changed.");
    Require((File.GetAttributes(destination) & FileAttributes.ReadOnly) != 0, "Read-only flag changed.");
  });
  await Run("locked source preserves existing destination", async dir =>
  {
    var (source, destination) = Paths(dir);
    File.WriteAllText(source, "new data");
    File.WriteAllText(destination, "original");
    using var locked = new FileStream(source, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    await ExpectIoFailure(() => AtomicFileCopier.CopyAsync(source, destination));
    Require(File.ReadAllText(destination) == "original", "Original changed after source-open failure.");
  });
  await Run("failed replacement cleans staging and preserves original", async dir =>
  {
    var (source, destination) = Paths(dir);
    File.WriteAllText(source, "new data");
    File.WriteAllText(destination, "original");
    File.SetAttributes(destination, FileAttributes.ReadOnly);
    using (var locked = new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
      await ExpectIoFailure(() => AtomicFileCopier.CopyAsync(source, destination));
    }
    Require(File.ReadAllText(destination) == "original", "Original changed after replacement failure.");
    Require((File.GetAttributes(destination) & FileAttributes.ReadOnly) != 0, "Read-only flag was not restored.");
  });
  await Run("sync retry skips successfully copied files", async dir =>
  {
    string cloud = Directory.CreateDirectory(Path.Combine(dir, "OneDrive")).FullName;
    string local = Directory.CreateDirectory(Path.Combine(dir, "local")).FullName;
    File.WriteAllText(Path.Combine(cloud, "item.txt"), "content");
    var options = new SyncOptions { CloudFolder = cloud, LocalFolder = local, Mode = ActionMode.toLocal };
    Require((await new SyncEngine().SyncAsync(options)).SyncedFiles == 1, "First sync did not copy.");
    Require((await new SyncEngine().SyncAsync(options)).SyncedFiles == 0, "Retry recopied unchanged content.");
  });
  await Run("remove-cloud does not delete source on failed copy", async dir =>
  {
    string cloud = Directory.CreateDirectory(Path.Combine(dir, "OneDrive")).FullName;
    string local = Directory.CreateDirectory(Path.Combine(dir, "local")).FullName;
    string source = Path.Combine(cloud, "item.txt");
    File.WriteAllText(source, "valuable source");
    using var locked = new FileStream(source, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    var options = new SyncOptions { CloudFolder = cloud, LocalFolder = local, Mode = ActionMode.toLocal, RemoveCloud = true };
    await ExpectIoFailure(async () => { await new SyncEngine().SyncAsync(options); });
    Require(File.Exists(source), "Source was removed after failure.");
    Require(!File.Exists(Path.Combine(local, "item.txt")), "Failure created an empty destination.");
  });
  await Run("zero-length source is valid", async dir =>
  {
    var (source, destination) = Paths(dir);
    File.WriteAllBytes(source, []);
    await AtomicFileCopier.CopyAsync(source, destination);
    Require(new FileInfo(destination).Length == 0, "Empty source was not copied.");
  });
  await Run("same-path copy is rejected without damage", async dir =>
  {
    var (source, _) = Paths(dir);
    File.WriteAllText(source, "original");
    await ExpectIoFailure(() => AtomicFileCopier.CopyAsync(source, source));
    Require(File.ReadAllText(source) == "original", "Same-path copy damaged source.");
  });
  Console.WriteLine($"Passed {passed} regression checks.");
}
finally
{
  // Only this invocation's newly created, GUID-named fixture directory is removed.
  foreach (string file in Directory.EnumerateFiles(testRoot, "*", SearchOption.AllDirectories))
    File.SetAttributes(file, FileAttributes.Normal);
  Directory.Delete(testRoot, recursive: true);
}

async Task Run(string name, Func<string, Task> check)
{
  string dir = Directory.CreateDirectory(Path.Combine(testRoot, passed.ToString())).FullName;
  await check(dir);
  Require(!Directory.EnumerateFiles(dir, ".synccloud-*.tmp", SearchOption.AllDirectories).Any(), "Staging file leaked.");
  passed++;
  Console.WriteLine("PASS " + name);
}
static (string Source, string Destination) Paths(string dir) => (Path.Combine(dir, "source.bin"), Path.Combine(dir, "destination.bin"));
static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
static async Task ExpectIoFailure(Func<Task> action)
{
  try { await action(); }
  catch (IOException) { return; }
  catch (UnauthorizedAccessException) { return; }
  throw new Exception("Expected a file-access failure.");
}
