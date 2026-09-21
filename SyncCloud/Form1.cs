namespace SyncCloud
{
  public partial class Form1 : Form
  {
    private bool cloudFolderSet;
    private bool localFolderSet;
    private bool removeCloud;
    private bool showCopyOnly;
    private ActionMode actionMode = ActionMode.Synchronize;

    public Form1()
      : this(new SyncOptions())
    {
    }

    public Form1(SyncOptions initialOptions)
    {
      InitializeComponent();
      ApplyOptions(initialOptions);
    }

    private void ApplyOptions(SyncOptions options)
    {
      textBoxCloudFolder.Text = options.CloudFolder;
      textBoxLocalFolder.Text = options.LocalFolder;
      cloudFolderSet = !string.IsNullOrWhiteSpace(options.CloudFolder);
      localFolderSet = !string.IsNullOrWhiteSpace(options.LocalFolder);

      removeCloud = options.RemoveCloud;
      showCopyOnly = options.ShowCopyOnly;
      actionMode = options.Mode;

      checkBox1.Checked = removeCloud;
      checkBox2.Checked = showCopyOnly;
      radioToLocal.Checked = actionMode == ActionMode.toLocal;
      radioToCloud.Checked = actionMode == ActionMode.toCloud;
      radioSynchronize.Checked = actionMode == ActionMode.Synchronize;
      UpdateSyncButtonState();
    }

    private void btnBrowseCloudFolder_Click(object sender, EventArgs e)
    {
      if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
      {
        textBoxCloudFolder.Text = folderBrowserDialog1.SelectedPath;
        cloudFolderSet = true;
        UpdateSyncButtonState();
      }
    }

    private void btnBrowseLocalFolder_Click(object sender, EventArgs e)
    {
      if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
      {
        textBoxLocalFolder.Text = folderBrowserDialog2.SelectedPath;
        localFolderSet = true;
        UpdateSyncButtonState();
      }
    }

    private void btnExit_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void SetActionButtons(bool enable)
    {
      btnExit.Enabled = enable;
      btnBrowseCloudFolder.Enabled = enable;
      btnBrowseLocalFolder.Enabled = enable;
      UpdateSyncButtonState(enable);
    }

    private async void btnSync_Click(object sender, EventArgs e)
    {
      if (!cloudFolderSet || !localFolderSet)
      {
        MessageBox.Show("Please set cloud and/or local folder");
        return;
      }

      SyncOptions options = GetCurrentOptions();
      IReadOnlyList<string> errors = SyncEngine.Validate(options);
      if (errors.Count > 0)
      {
        string mismatchError = errors.FirstOrDefault(error => error.Contains("folder names do not seem to match", StringComparison.OrdinalIgnoreCase)) ?? "";
        IEnumerable<string> hardErrors = errors.Where(error => error != mismatchError);
        if (hardErrors.Any())
        {
          MessageBox.Show(string.Join(Environment.NewLine, hardErrors), "Check Folders", MessageBoxButtons.OK);
          return;
        }

        string msg = "Cloud and local folder seems not match, are you sure to continue?";
        msg += "\nCloud : " + textBoxCloudFolder.Text;
        msg += "\nLocal : " + textBoxLocalFolder.Text;
        DialogResult result = MessageBox.Show(msg, "Check Folders", MessageBoxButtons.YesNo);
        if (result == DialogResult.No || result == DialogResult.Cancel || result == DialogResult.Abort)
        {
          textBoxProgress.Clear();
          AppendProgress("Cancelled.");
          return;
        }

        options.AllowFolderMismatch = true;
      }

      SetActionButtons(false);
      textBoxProgress.Clear();
      AppendProgress("Working...");
      try
      {
        SyncEngine syncEngine = new SyncEngine(AppendProgress, GetProgressWidth);
        SyncResult result = await syncEngine.SyncAsync(options);
        MessageBox.Show(result.Message, "Sync Result", MessageBoxButtons.OK);
        AppendProgress("Finished.");
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Sync Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        AppendProgress("Failed: " + ex.Message);
      }
      finally
      {
        SetActionButtons(true);
      }
    }

    private SyncOptions GetCurrentOptions()
    {
      return new SyncOptions
      {
        CloudFolder = textBoxCloudFolder.Text,
        LocalFolder = textBoxLocalFolder.Text,
        RemoveCloud = removeCloud,
        ShowCopyOnly = showCopyOnly,
        Mode = actionMode
      };
    }

    private int GetProgressWidth()
    {
      int characterWidth = TextRenderer.MeasureText("=", textBoxProgress.Font,
        Size.Empty, TextFormatFlags.NoPadding).Width;
      // Leave room for the text box margins and vertical scrollbar.
      int availableWidth = textBoxProgress.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4;
      return Math.Max(1, availableWidth / Math.Max(1, characterWidth));
    }

    private void AppendProgress(string message)
    {
      if (InvokeRequired)
      {
        BeginInvoke(new Action<string>(AppendProgress), message);
        return;
      }

      textBoxProgress.AppendText(message);
      if (!message.EndsWith(Environment.NewLine))
      {
        textBoxProgress.AppendText(Environment.NewLine);
      }
    }

    private void UpdateSyncButtonState(bool actionsEnabled = true)
    {
      btnSync.Enabled = actionsEnabled && cloudFolderSet && localFolderSet;
    }

    private void radioButton_CheckedChanged(object sender, EventArgs e)
    {
      if (sender is not RadioButton radioButton || !radioButton.Checked)
      {
        return;
      }

      if (radioButton.Name == "radioToLocal")
      {
        actionMode = ActionMode.toLocal;
      }
      else if (radioButton.Name == "radioToCloud")
      {
        actionMode = ActionMode.toCloud;
      }
      else if (radioButton.Name == "radioSynchronize")
      {
        actionMode = ActionMode.Synchronize;
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
}
