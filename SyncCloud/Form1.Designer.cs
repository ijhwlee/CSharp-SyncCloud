namespace SyncCloud
{
  partial class Form1
  {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
      label1 = new Label();
      label2 = new Label();
      textBoxCloudFolder = new TextBox();
      textBoxLocalFolder = new TextBox();
      btnBrowseCloudFolder = new Button();
      btnBrowseLocalFolder = new Button();
      label3 = new Label();
      textBoxProgress = new TextBox();
      folderBrowserDialog1 = new FolderBrowserDialog();
      btnExit = new Button();
      btnSync = new Button();
      folderBrowserDialog2 = new FolderBrowserDialog();
      groupBox1 = new GroupBox();
      radioSynchronize = new RadioButton();
      radioToCloud = new RadioButton();
      radioToLocal = new RadioButton();
      checkBox1 = new CheckBox();
      groupBox2 = new GroupBox();
      checkBox2 = new CheckBox();
      groupBox1.SuspendLayout();
      groupBox2.SuspendLayout();
      SuspendLayout();
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point);
      label1.Location = new Point(17, 27);
      label1.Name = "label1";
      label1.Size = new Size(120, 25);
      label1.TabIndex = 0;
      label1.Text = "Cloud Folder";
      // 
      // label2
      // 
      label2.AutoSize = true;
      label2.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point);
      label2.Location = new Point(778, 27);
      label2.Name = "label2";
      label2.Size = new Size(115, 25);
      label2.TabIndex = 1;
      label2.Text = "Local Folder";
      // 
      // textBoxCloudFolder
      // 
      textBoxCloudFolder.Enabled = false;
      textBoxCloudFolder.Location = new Point(25, 69);
      textBoxCloudFolder.Name = "textBoxCloudFolder";
      textBoxCloudFolder.Size = new Size(639, 31);
      textBoxCloudFolder.TabIndex = 2;
      // 
      // textBoxLocalFolder
      // 
      textBoxLocalFolder.Enabled = false;
      textBoxLocalFolder.Location = new Point(782, 70);
      textBoxLocalFolder.Name = "textBoxLocalFolder";
      textBoxLocalFolder.Size = new Size(708, 31);
      textBoxLocalFolder.TabIndex = 3;
      // 
      // btnBrowseCloudFolder
      // 
      btnBrowseCloudFolder.Location = new Point(143, 22);
      btnBrowseCloudFolder.Name = "btnBrowseCloudFolder";
      btnBrowseCloudFolder.Size = new Size(112, 34);
      btnBrowseCloudFolder.TabIndex = 4;
      btnBrowseCloudFolder.Text = "Browse";
      btnBrowseCloudFolder.UseVisualStyleBackColor = true;
      btnBrowseCloudFolder.Click += btnBrowseCloudFolder_Click;
      // 
      // btnBrowseLocalFolder
      // 
      btnBrowseLocalFolder.Location = new Point(899, 22);
      btnBrowseLocalFolder.Name = "btnBrowseLocalFolder";
      btnBrowseLocalFolder.Size = new Size(112, 34);
      btnBrowseLocalFolder.TabIndex = 5;
      btnBrowseLocalFolder.Text = "Browse";
      btnBrowseLocalFolder.UseVisualStyleBackColor = true;
      btnBrowseLocalFolder.Click += btnBrowseLocalFolder_Click;
      // 
      // label3
      // 
      label3.AutoSize = true;
      label3.Location = new Point(19, 219);
      label3.Name = "label3";
      label3.Size = new Size(82, 25);
      label3.TabIndex = 6;
      label3.Text = "Progress";
      // 
      // textBoxProgress
      // 
      textBoxProgress.Location = new Point(19, 254);
      textBoxProgress.Multiline = true;
      textBoxProgress.Name = "textBoxProgress";
      textBoxProgress.ScrollBars = ScrollBars.Both;
      textBoxProgress.Size = new Size(1463, 287);
      textBoxProgress.TabIndex = 7;
      textBoxProgress.WordWrap = false;
      // 
      // folderBrowserDialog1
      // 
      folderBrowserDialog1.Description = "Cloud Folder Selection";
      // 
      // btnExit
      // 
      btnExit.Location = new Point(1372, 214);
      btnExit.Name = "btnExit";
      btnExit.Size = new Size(112, 34);
      btnExit.TabIndex = 8;
      btnExit.Text = "Exit";
      btnExit.UseVisualStyleBackColor = true;
      btnExit.Click += btnExit_Click;
      // 
      // btnSync
      // 
      btnSync.Location = new Point(670, 70);
      btnSync.Name = "btnSync";
      btnSync.Size = new Size(106, 34);
      btnSync.TabIndex = 9;
      btnSync.Text = "<sync>";
      btnSync.UseVisualStyleBackColor = true;
      btnSync.Click += btnSync_Click;
      // 
      // folderBrowserDialog2
      // 
      folderBrowserDialog2.Description = "Local Folder Selection";
      // 
      // groupBox1
      // 
      groupBox1.Controls.Add(radioSynchronize);
      groupBox1.Controls.Add(radioToCloud);
      groupBox1.Controls.Add(radioToLocal);
      groupBox1.Location = new Point(284, 5);
      groupBox1.Name = "groupBox1";
      groupBox1.Size = new Size(468, 58);
      groupBox1.TabIndex = 11;
      groupBox1.TabStop = false;
      groupBox1.Text = "Mode";
      // 
      // radioSynchronize
      // 
      radioSynchronize.AutoSize = true;
      radioSynchronize.Checked = true;
      radioSynchronize.Location = new Point(328, 18);
      radioSynchronize.Name = "radioSynchronize";
      radioSynchronize.Size = new Size(134, 29);
      radioSynchronize.TabIndex = 2;
      radioSynchronize.TabStop = true;
      radioSynchronize.Text = "Synchronize";
      radioSynchronize.UseVisualStyleBackColor = true;
      radioSynchronize.CheckedChanged += radioButton_CheckedChanged;
      // 
      // radioToCloud
      // 
      radioToCloud.AutoSize = true;
      radioToCloud.Location = new Point(197, 19);
      radioToCloud.Name = "radioToCloud";
      radioToCloud.Size = new Size(107, 29);
      radioToCloud.TabIndex = 1;
      radioToCloud.Text = "to Cloud";
      radioToCloud.UseVisualStyleBackColor = true;
      radioToCloud.CheckedChanged += radioButton_CheckedChanged;
      // 
      // radioToLocal
      // 
      radioToLocal.AutoSize = true;
      radioToLocal.Location = new Point(73, 19);
      radioToLocal.Name = "radioToLocal";
      radioToLocal.Size = new Size(102, 29);
      radioToLocal.TabIndex = 0;
      radioToLocal.Text = "to Local";
      radioToLocal.UseVisualStyleBackColor = true;
      radioToLocal.CheckedChanged += radioButton_CheckedChanged;
      // 
      // checkBox1
      // 
      checkBox1.AutoSize = true;
      checkBox1.ForeColor = Color.Red;
      checkBox1.Location = new Point(1065, 24);
      checkBox1.Name = "checkBox1";
      checkBox1.Size = new Size(152, 29);
      checkBox1.TabIndex = 12;
      checkBox1.Text = "RemoveCloud";
      checkBox1.UseVisualStyleBackColor = true;
      checkBox1.CheckedChanged += checkBox1_CheckedChanged;
      // 
      // groupBox2
      // 
      groupBox2.Controls.Add(checkBox2);
      groupBox2.Location = new Point(25, 125);
      groupBox2.Name = "groupBox2";
      groupBox2.Size = new Size(639, 72);
      groupBox2.TabIndex = 13;
      groupBox2.TabStop = false;
      groupBox2.Text = "Progress Mode";
      // 
      // checkBox2
      // 
      checkBox2.AutoSize = true;
      checkBox2.Location = new Point(23, 29);
      checkBox2.Name = "checkBox2";
      checkBox2.Size = new Size(159, 29);
      checkBox2.TabIndex = 0;
      checkBox2.Text = "showCopyOnly";
      checkBox2.UseVisualStyleBackColor = true;
      checkBox2.CheckedChanged += checkBox2_CheckedChanged;
      // 
      // Form1
      // 
      AutoScaleDimensions = new SizeF(10F, 25F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(1502, 566);
      Controls.Add(groupBox2);
      Controls.Add(checkBox1);
      Controls.Add(groupBox1);
      Controls.Add(btnSync);
      Controls.Add(btnExit);
      Controls.Add(textBoxProgress);
      Controls.Add(label3);
      Controls.Add(btnBrowseLocalFolder);
      Controls.Add(btnBrowseCloudFolder);
      Controls.Add(textBoxLocalFolder);
      Controls.Add(textBoxCloudFolder);
      Controls.Add(label2);
      Controls.Add(label1);
      Icon = (Icon)resources.GetObject("$this.Icon");
      Name = "Form1";
      Text = "SyncCloud";
      groupBox1.ResumeLayout(false);
      groupBox1.PerformLayout();
      groupBox2.ResumeLayout(false);
      groupBox2.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private Label label1;
    private Label label2;
    private TextBox textBoxCloudFolder;
    private TextBox textBoxLocalFolder;
    private Button btnBrowseCloudFolder;
    private Button btnBrowseLocalFolder;
    private Label label3;
    private TextBox textBoxProgress;
    private FolderBrowserDialog folderBrowserDialog1;
    private Button btnExit;
    private Button btnSync;
    private FolderBrowserDialog folderBrowserDialog2;
    private GroupBox groupBox1;
    private RadioButton radioSynchronize;
    private RadioButton radioToCloud;
    private RadioButton radioToLocal;
    private CheckBox checkBox1;
    private GroupBox groupBox2;
    private CheckBox checkBox2;
  }
}