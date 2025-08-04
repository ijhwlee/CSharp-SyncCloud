using System.Diagnostics;
using System.IO;

namespace SyncCloud
{
  public partial class Form1 : Form
  {
    private bool cloudFolderSet = false;
    private bool localFolderSet = false;
    private bool removeCloud = false;
    private bool showCopyOnly = false;
    private ActionMode actionMode = ActionMode.Synchronize;
    public Form1()
    {
      InitializeComponent();
      btnSync.Enabled = false;
      checkBox1.Checked = removeCloud;
      checkBox2.Checked = showCopyOnly;
    }

    private void btnBrowseCloudFolder_Click(object sender, EventArgs e)
    {
      if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
      {
        textBoxCloudFolder.Text = folderBrowserDialog1.SelectedPath;
        cloudFolderSet = true;
        if (localFolderSet)
        {
          btnSync.Enabled = true;
        }
      }
    }

    private void btnBrowseLocalFolder_Click(object sender, EventArgs e)
    {
      if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
      {
        textBoxLocalFolder.Text = folderBrowserDialog2.SelectedPath;
        localFolderSet = true;
        if (cloudFolderSet)
        {
          btnSync.Enabled = true;
        }
      }
    }

    private void btnExit_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void setActionButtons(bool enable)
    {
      btnExit.Enabled = enable;
      btnSync.Enabled = enable;
      btnBrowseCloudFolder.Enabled = enable;
      btnBrowseLocalFolder.Enabled = enable;
    }
    private async void btnSync_Click(object sender, EventArgs e)
    {
      setActionButtons(false);
      if (cloudFolderSet && localFolderSet)
      {
        textBoxProgress.Clear();
        textBoxProgress.AppendText("Working..."+ Environment.NewLine);
        if (!textBoxCloudFolder.Text.Contains("OneDrive"))
        {
          string msg = "Cloud folder does not contain OneDrive, please re-select.\nCurrently only support MS OneDrive.";
          msg += "\nCloud : " + textBoxCloudFolder.Text;
          msg += "\nLocal : " + textBoxLocalFolder.Text;
          MessageBox.Show(msg, "Check Folders", MessageBoxButtons.OK);
          return;
        }
        if (!checkFolders())
        {
          string msg = "Cloud and local folder seems not match, are you sure to continue?";
          msg += "\nCloud : " + textBoxCloudFolder.Text;
          msg += "\nLocal : " + textBoxLocalFolder.Text;
          if (MessageBox.Show(msg, "Check Folders", MessageBoxButtons.YesNo) == DialogResult.No)
            return;
        }
        string[] cloudFolders = await Task.Run(()=>Directory.GetDirectories(textBoxCloudFolder.Text));
        string[] localFolders = await Task.Run(()=>Directory.GetDirectories(textBoxLocalFolder.Text));
        int syncFile = await syncFoldersAsync(cloudFolders, localFolders, textBoxCloudFolder.Text, textBoxLocalFolder.Text);
        /*Debug.WriteLine("[DEBUG-hwlee]List of directories in "+ textBoxCloudFolder.Text+":");
        foreach(String d in directories) 
        {
          Debug.WriteLine(d);
        }*/
        string[] cloudFiles = await Task.Run(()=>Directory.GetFiles(textBoxCloudFolder.Text));
        string[] localFiles = await Task.Run(()=>Directory.GetFiles(textBoxLocalFolder.Text));
        syncFile += await syncFilesAsync(textBoxCloudFolder.Text, textBoxLocalFolder.Text);
        string msg1 = "";
        if (syncFile > 0)
        {
          msg1 = "Cloud : " + textBoxCloudFolder.Text + " and local : " + textBoxLocalFolder.Text + " are synched for " + syncFile + " files.";
        }
        else
        {
          msg1 = "Cloud : " + textBoxCloudFolder.Text + " and local : " + textBoxLocalFolder.Text + " are already synched.";
        }
        MessageBox.Show(msg1, "Sync Result", MessageBoxButtons.OK);
        /*Debug.WriteLine("[DEBUG-hwlee]List of files in " + textBoxCloudFolder.Text + ":");
        foreach (String f in files)
        {
          Debug.WriteLine(f);
        }*/
        textBoxProgress.AppendText("Finished."+ Environment.NewLine);
      }
      else
      {
        MessageBox.Show("Please set cloud and/or local folder");
      }
      setActionButtons(true);
    }
    private async Task<int> syncFoldersAsync(string[] cloud, string[] local, string parentCloud, string parentLocal)
    {
      int syncFile = 0;
      List<Task<int>> tasks = new List<Task<int>>();
      // for each folder of cloud
      foreach (string folder in cloud)
      {
        string? localFolder = getLocal(folder, local);
        if (!showCopyOnly)
          textBoxProgress.AppendText("   Checking cloud folder " + folder);
        if (localFolder == null)
        {
          string[] tokens = folder.Split('\\');
          string path = tokens[tokens.Length - 1];
          localFolder = parentLocal + "\\" + path;
          //Debug.WriteLine("[DEBUG-hwlee]Creating cloud folder : " + folder + " to local folder : " + localFolder);
          await Task.Run(()=>System.IO.Directory.CreateDirectory(localFolder));
        }
        //syncFile += await syncFilesAsync(folder, localFolder);
        tasks.Add(syncFilesAsync(folder, localFolder));
      }
      int[] ints = await Task.WhenAll(tasks);
      foreach (int i in ints)
      {
        syncFile += i;
      }
      // for each folder of local
      foreach (string folder in local)
      {
        string? cloudFolder = getLocal(folder, cloud);
        if (!showCopyOnly)
          textBoxProgress.AppendText("   Checking local folder " + folder);
        if (cloudFolder == null)
        {
          string[] tokens = folder.Split('\\');
          string path = tokens[tokens.Length - 1];
          cloudFolder = parentCloud + "\\" + path;
          //Debug.WriteLine("[DEBUG-hwlee]Creating local folder : " + folder + " to cloud folder : " + cloudFolder);
          await Task.Run(()=>System.IO.Directory.CreateDirectory(cloudFolder));
          syncFile += await syncFilesAsync(cloudFolder, folder);
        }
      }
      return syncFile;
    }
    private async Task<int> syncFilesAsync(string cloud, string local)
    {
      const int buffer_size = 81920; // 80 KB buffer size for file operations
      int syncFile = 0;
      string[] cloudFolders = await Task.Run(()=>Directory.GetDirectories(cloud));
      string[] localFolders = await Task.Run(()=>Directory.GetDirectories(local));
      syncFile += await syncFoldersAsync(cloudFolders, localFolders, cloud, local);

      string[] cloudFiles = await Task.Run(()=>Directory.GetFiles(cloud));
      string[] localFiles = await Task.Run(()=>Directory.GetFiles(local));
      foreach (string f in cloudFiles)
      {
        string fileName = Path.GetFileName(f);
        string localName = local + "\\" + fileName;
        if (File.Exists(localName))
        {
          //Debug.WriteLine("[DEBUG-hwlee]Synching file : " + f + " to local : " + localName);
          if (!showCopyOnly)
            textBoxProgress.AppendText("Synching file : " + f + " to local : " + localName + Environment.NewLine);
          long cloudTime = File.GetLastWriteTimeUtc(f).ToFileTime();
          long localTime = File.GetLastWriteTimeUtc(localName).ToFileTime();
          //Debug.WriteLine("[DEBUG-hwlee]cloudTime : " + cloudTime + ", localTime : " + localTime);
          if ((actionMode == ActionMode.toCloud || actionMode == ActionMode.Synchronize) && cloudTime < localTime)
          {
            //File.Copy(localName, f, true);
            var fileInfo = new FileInfo(f);
            bool isReadOnly = false;
            if (fileInfo.Exists && fileInfo.IsReadOnly)
            {
              fileInfo.IsReadOnly = false; // Clear read-only attribute to avoid issues with copying
              isReadOnly = true;
            }
            FileAttributes attr = File.GetAttributes(f);
            bool isHidden = (attr & FileAttributes.Hidden) != 0;
            bool isSystem = (attr & FileAttributes.System) != 0;
            bool isArchive = (attr & FileAttributes.Archive) != 0;
            if (isHidden || isSystem || isReadOnly)
            {
              File.SetAttributes(f, FileAttributes.Normal);
            }
            using (var outStream = new FileStream(f, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: buffer_size, useAsync:true))
            {
              using (var inStream = new FileStream(localName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: buffer_size, useAsync:true))
              {
                //await inStream.CopyToAsync(outStream);
                byte[] buffer = new byte[buffer_size];
                int bytesRead;
                while ((bytesRead = await inStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                  await outStream.WriteAsync(buffer, 0, bytesRead);
                }
                // Preserve attributes and timestamps
                File.SetAttributes(f, File.GetAttributes(localName));
                File.SetCreationTime(f, File.GetCreationTime(localName));
                File.SetLastAccessTime(f, File.GetLastAccessTime(localName));
                File.SetLastWriteTime(f, File.GetLastWriteTime(localName));
              }
            }
            //Debug.WriteLine("[DEBUG-hwlee]Copying file : " + localName + " to  : " + f);
            textBoxProgress.AppendText("    Copying file : " + localName + " to : " + f + Environment.NewLine);
            syncFile++;
          }
          else if ((actionMode == ActionMode.toLocal || actionMode == ActionMode.Synchronize) && cloudTime > localTime)
          {
            //File.Copy(f, localName, true);
            var fileInfo = new FileInfo(localName);
            bool isReadOnly = false;
            if (fileInfo.Exists && fileInfo.IsReadOnly)
            {
              fileInfo.IsReadOnly = false; // Clear read-only attribute to avoid issues with copying
              isReadOnly = true;
            }
            FileAttributes attr = File.GetAttributes(localName);
            bool isHidden = (attr & FileAttributes.Hidden) != 0;
            bool isSystem = (attr & FileAttributes.System) != 0;
            bool isArchive = (attr & FileAttributes.Archive) != 0;
            if (isHidden || isSystem || isReadOnly)
            {
              File.SetAttributes(localName, FileAttributes.Normal);
            }
            using (var outStream = new FileStream(localName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: buffer_size, useAsync:true))
            {
              using (var inStream = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: buffer_size, useAsync:true))
              {
                //await inStream.CopyToAsync(outStream);
                byte[] buffer = new byte[buffer_size];
                int bytesRead;
                while ((bytesRead = await inStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                  await outStream.WriteAsync(buffer, 0, bytesRead);
                }
                // Preserve attributes and timestamps
                File.SetAttributes(localName, File.GetAttributes(f));
                File.SetCreationTime(localName, File.GetCreationTime(f));
                File.SetLastAccessTime(localName, File.GetLastAccessTime(f));
                File.SetLastWriteTime(localName, File.GetLastWriteTime(f));
              }
            }
            //Debug.WriteLine("[DEBUG-hwlee]Copying file : " + f + " to  : " + localName);
            textBoxProgress.AppendText("    Copying file : " + f + " to : " + localName + Environment.NewLine);
            if (removeCloud && actionMode == ActionMode.toLocal)
            {
              await Task.Run(() => File.Delete(f));
              textBoxProgress.AppendText("    Removing file : " + f + " from cloud " + Environment.NewLine);
            }
            syncFile++;
          }
        }
        else if (actionMode == ActionMode.toLocal || actionMode == ActionMode.Synchronize) // local file does not exist
        {
          if (!showCopyOnly)
            textBoxProgress.AppendText("Synching file : " + f + " to local : " + localName + Environment.NewLine);
          //Debug.WriteLine("[DEBUG-hwlee]Copying file : " + f + " to local : " + localName);
          textBoxProgress.AppendText("Copying file : " + f + " to local : " + localName + Environment.NewLine);
          //File.Copy(f, localName, true);
          //var fileInfo = new FileInfo(localName);
          using (var outStream = new FileStream(localName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: buffer_size, useAsync:true))
          {
            using (var inStream = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: buffer_size, useAsync:true))
            {
              //await inStream.CopyToAsync(outStream);
              byte[] buffer = new byte[buffer_size];
              int bytesRead;
              while ((bytesRead = await inStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
              {
                await outStream.WriteAsync(buffer, 0, bytesRead);
              }
              // Preserve attributes and timestamps
              File.SetAttributes(localName, File.GetAttributes(f));
              File.SetCreationTime(localName, File.GetCreationTime(f));
              File.SetLastAccessTime(localName, File.GetLastAccessTime(f));
              File.SetLastWriteTime(localName, File.GetLastWriteTime(f));
            }
          }
          if (removeCloud && actionMode == ActionMode.toLocal)
          {
            //File.Delete(f);
            await Task.Run(() => File.Delete(f));
            textBoxProgress.AppendText("    Removing file : " + f + " from cloud " + Environment.NewLine);
          }
          syncFile++;
        }
      }
      if (actionMode == ActionMode.Synchronize) return syncFile;
      //Debug.WriteLine("[DEBUG-hwlee]syncFiles: actionMode = " + actionMode + "====================================");
      foreach (string f in localFiles)
      {
        string fileName = Path.GetFileName(f);
        string cloudName = cloud + "\\" + fileName;
        if (!showCopyOnly)
          textBoxProgress.AppendText("Synching file : " + f + " to cloud : " + cloudName + Environment.NewLine);
        long localTime = File.GetLastWriteTimeUtc(f).ToFileTime();
        long cloudTime = File.GetLastWriteTimeUtc(cloudName).ToFileTime();
        if ((actionMode == ActionMode.toCloud || actionMode == ActionMode.Synchronize) && !File.Exists(cloudName))
        {
          //Debug.WriteLine("[DEBUG-hwlee]Copying file : " + f + " to cloud : " + cloudName);
          //File.Copy(f, cloudName, true);
          using (var outStream = new FileStream(cloudName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: buffer_size, useAsync:true))
          {
            using (var inStream = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: buffer_size, useAsync:true))
            {
              var fileInfo = new FileInfo(cloudName);
              fileInfo.Attributes = FileAttributes.Normal; // Clear attributes to avoid issues with copying
              //await inStream.CopyToAsync(outStream);
              byte[] buffer = new byte[buffer_size];
              int bytesRead;
              while ((bytesRead = await inStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
              {
                await outStream.WriteAsync(buffer, 0, bytesRead);
              }
              // Preserve attributes and timestamps
              File.SetAttributes(cloudName, File.GetAttributes(f));
              File.SetCreationTime(cloudName, File.GetCreationTime(f));
              File.SetLastAccessTime(cloudName, File.GetLastAccessTime(f));
              File.SetLastWriteTime(cloudName, File.GetLastWriteTime(f));
            }
          }
          textBoxProgress.AppendText("Copying file : " + f + " to cloud : " + cloudName + Environment.NewLine);
          syncFile++;
        }
        else if ((actionMode == ActionMode.toCloud || actionMode == ActionMode.Synchronize) && File.Exists(cloudName) && localTime > cloudTime)
        {
          //Debug.WriteLine("[DEBUG-hwlee]Copying file : " + f + " to cloud : " + cloudName);
          //File.Copy(f, cloudName, true);
          var fileInfo = new FileInfo(cloudName);
          bool isReadOnly = false;
          if (fileInfo.Exists && fileInfo.IsReadOnly)
          {
            fileInfo.IsReadOnly = false; // Clear read-only attribute to avoid issues with copying
            isReadOnly = true;
          }
          FileAttributes attr = File.GetAttributes(cloudName);
          bool isHidden = (attr & FileAttributes.Hidden) != 0;
          bool isSystem = (attr & FileAttributes.System) != 0;
          bool isArchive = (attr & FileAttributes.Archive) != 0;
          if (isHidden || isSystem || isReadOnly)
          {
            File.SetAttributes(cloudName, FileAttributes.Normal);
          }
          using (var outStream = new FileStream(cloudName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: buffer_size, useAsync:true))
          {
            using (var inStream = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: buffer_size, useAsync:true))
            {
              //await inStream.CopyToAsync(outStream);
              byte[] buffer = new byte[buffer_size];
              int bytesRead;
              while ((bytesRead = await inStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
              {
                await outStream.WriteAsync(buffer, 0, bytesRead);
              }
              // Preserve attributes and timestamps
              File.SetAttributes(cloudName, File.GetAttributes(f));
              File.SetCreationTime(cloudName, File.GetCreationTime(f));
              File.SetLastAccessTime(cloudName, File.GetLastAccessTime(f));
              File.SetLastWriteTime(cloudName, File.GetLastWriteTime(f));
            }
          }
          textBoxProgress.AppendText("Copying file : " + f + " to cloud : " + cloudName + Environment.NewLine);
          syncFile++;
        }
      }
      return syncFile;
    }
    private string? getLocal(string folder, string[] local)
    {
      string[] tokens = folder.Split('\\');
      string path = tokens[tokens.Length - 1];
      foreach (string d in local)
      {
        string[] localTokens = d.Split("\\");
        string localPath = localTokens[localTokens.Length - 1];
        if (path.ToLower() == localPath.ToLower())
        {
          //Debug.WriteLine("[DEBUG-hwlee]====== Found folder : " + folder + " in local : " + d);
          return d;
        }
      }
      //Debug.WriteLine("[DEBUG-hwlee]======= Not Found folder : " + folder + " in local");
      return null; // Not found
    }
    private async Task syncFolderAsync(string cloud, string local)
    {
      string[] cloudFolders = await Task.Run(()=>Directory.GetDirectories(cloud));
      string[] localFolders = await Task.Run(()=>Directory.GetDirectories(local));
      await syncFoldersAsync(cloudFolders, localFolders, cloud, local);
    }
    private bool checkFolders()
    {
      bool match = false;
      string[] cloudTokens = textBoxCloudFolder.Text.Split('\\');
      //Debug.WriteLine("[DEBUG-hwlee]List of tokens in " + textBoxCloudFolder.Text + ":");
      foreach (String d in cloudTokens)
      {
        Debug.WriteLine(d);
      }
      string[] localTokens = textBoxLocalFolder.Text.Split('\\');
      //Debug.WriteLine("[DEBUG-hwlee]List of tokens in " + textBoxLocalFolder.Text + ":");
      foreach (String d in localTokens)
      {
        Debug.WriteLine(d);
      }
      int count = cloudTokens.Length < localTokens.Length ? cloudTokens.Length : localTokens.Length;
      //Debug.WriteLine("[DEBUG-hwlee]count = " + count + ", cloud = " + cloudTokens.Length + ", local = " + localTokens.Length);
      int idxCloud = cloudTokens.Length - 1;
      int idxLocal = localTokens.Length - 1;
      match = true;
      while (count > 0 && !cloudTokens[idxCloud].Contains("OneDrive"))
      {
        if (cloudTokens[idxCloud].ToLower() != localTokens[idxLocal].ToLower())
        {
          match = false;
          break;
        }
        idxCloud--;
        idxLocal--;
        count--;
      }
      return match;
    }

    private void radioButton_CheckedChanged(object sender, EventArgs e)
    {
      //Debug.WriteLine("[DEBUG-hwlee]radioButton_CheckedChanged sender is " + sender + " ==============================");
      if (sender.GetType() == typeof(RadioButton))
      {
        RadioButton radioButton = (RadioButton)sender;
        //Debug.WriteLine("[DEBUG-hwlee]radioButton_CheckedChanged sender.Name is " + radioButton.Name);
        if (radioButton.Name == "radioToLocal")
        {
          actionMode = ActionMode.toLocal;
          //Debug.WriteLine("[DEBUG-hwlee]actionMode is " + actionMode);
        }
        else if (radioButton.Name == "radioToCloud")
        {
          actionMode = ActionMode.toCloud;
          //Debug.WriteLine("[DEBUG-hwlee]actionMode is " + actionMode);
        }
        else if (radioButton.Name == "radioSynchronize")
        {
          actionMode = ActionMode.Synchronize;
          //Debug.WriteLine("[DEBUG-hwlee]actionMode is " + actionMode);
        }
      }
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      removeCloud = ((CheckBox)sender).Checked;
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
      showCopyOnly = checkBox2.Checked;
    }
  }
  public enum ActionMode { toLocal, toCloud, Synchronize }
}