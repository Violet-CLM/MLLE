namespace MLLE
{
    partial class LevelProperties
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.Arguments = new System.Windows.Forms.TextBox();
            this.HideHCL = new System.Windows.Forms.CheckBox();
            this.NextLevel = new System.Windows.Forms.ComboBox();
            this.IsMultiplayer = new System.Windows.Forms.CheckBox();
            this.BrowseSecret = new System.Windows.Forms.Button();
            this.BrowseMusic = new System.Windows.Forms.Button();
            this.BrowseNext = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.BonusLevel = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SecretLevel = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.MusicFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.LevelName = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.MinLight = new System.Windows.Forms.NumericUpDown();
            this.StartLight = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.OpenMusicDialog = new System.Windows.Forms.OpenFileDialog();
            this.argumentsGenerate = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinLight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.StartLight)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // ButtonOK
            // 
            this.ButtonOK.Location = new System.Drawing.Point(739, 31);
            this.ButtonOK.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(200, 55);
            this.ButtonOK.TabIndex = 0;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(736, 103);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(200, 55);
            this.ButtonCancel.TabIndex = 1;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.argumentsGenerate);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.Arguments);
            this.groupBox1.Controls.Add(this.HideHCL);
            this.groupBox1.Controls.Add(this.NextLevel);
            this.groupBox1.Controls.Add(this.IsMultiplayer);
            this.groupBox1.Controls.Add(this.BrowseSecret);
            this.groupBox1.Controls.Add(this.BrowseMusic);
            this.groupBox1.Controls.Add(this.BrowseNext);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.BonusLevel);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.SecretLevel);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.MusicFile);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.LevelName);
            this.groupBox1.Location = new System.Drawing.Point(35, 31);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.groupBox1.Size = new System.Drawing.Size(685, 477);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(19, 358);
            this.label8.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(168, 31);
            this.label8.TabIndex = 12;
            this.label8.Text = "CmdLnArgs";
            // 
            // Arguments
            // 
            this.Arguments.Location = new System.Drawing.Point(200, 358);
            this.Arguments.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.Arguments.MaxLength = 250;
            this.Arguments.Name = "Arguments";
            this.Arguments.Size = new System.Drawing.Size(287, 38);
            this.Arguments.TabIndex = 13;
            // 
            // HideHCL
            // 
            this.HideHCL.AutoSize = true;
            this.HideHCL.Location = new System.Drawing.Point(27, 420);
            this.HideHCL.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.HideHCL.Name = "HideHCL";
            this.HideHCL.Size = new System.Drawing.Size(421, 36);
            this.HideHCL.TabIndex = 10;
            this.HideHCL.Text = "Hide in Home-Cooked Levels";
            this.HideHCL.UseVisualStyleBackColor = true;
            // 
            // NextLevel
            // 
            this.NextLevel.FormattingEnabled = true;
            this.NextLevel.Items.AddRange(new object[] {
            "ending",
            "endepis"});
            this.NextLevel.Location = new System.Drawing.Point(200, 110);
            this.NextLevel.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.NextLevel.MaxLength = 31;
            this.NextLevel.Name = "NextLevel";
            this.NextLevel.Size = new System.Drawing.Size(287, 39);
            this.NextLevel.TabIndex = 3;
            // 
            // IsMultiplayer
            // 
            this.IsMultiplayer.AutoSize = true;
            this.IsMultiplayer.Location = new System.Drawing.Point(464, 420);
            this.IsMultiplayer.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.IsMultiplayer.Name = "IsMultiplayer";
            this.IsMultiplayer.Size = new System.Drawing.Size(192, 36);
            this.IsMultiplayer.TabIndex = 11;
            this.IsMultiplayer.Text = "Multiplayer";
            this.IsMultiplayer.UseVisualStyleBackColor = true;
            // 
            // BrowseSecret
            // 
            this.BrowseSecret.Location = new System.Drawing.Point(509, 234);
            this.BrowseSecret.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.BrowseSecret.Name = "BrowseSecret";
            this.BrowseSecret.Size = new System.Drawing.Size(160, 48);
            this.BrowseSecret.TabIndex = 8;
            this.BrowseSecret.Text = "Browse...";
            this.BrowseSecret.UseVisualStyleBackColor = true;
            this.BrowseSecret.Click += new System.EventHandler(this.BrowseSecret_Click);
            // 
            // BrowseMusic
            // 
            this.BrowseMusic.Location = new System.Drawing.Point(509, 172);
            this.BrowseMusic.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.BrowseMusic.Name = "BrowseMusic";
            this.BrowseMusic.Size = new System.Drawing.Size(160, 48);
            this.BrowseMusic.TabIndex = 6;
            this.BrowseMusic.Text = "Browse...";
            this.BrowseMusic.UseVisualStyleBackColor = true;
            this.BrowseMusic.Click += new System.EventHandler(this.BrowseMusic_Click);
            // 
            // BrowseNext
            // 
            this.BrowseNext.Location = new System.Drawing.Point(509, 110);
            this.BrowseNext.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.BrowseNext.Name = "BrowseNext";
            this.BrowseNext.Size = new System.Drawing.Size(160, 48);
            this.BrowseNext.TabIndex = 4;
            this.BrowseNext.Text = "Browse...";
            this.BrowseNext.UseVisualStyleBackColor = true;
            this.BrowseNext.Click += new System.EventHandler(this.BrowseNext_Click);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(19, 296);
            this.label5.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(168, 31);
            this.label5.TabIndex = 9;
            this.label5.Text = "Bonus level";
            // 
            // BonusLevel
            // 
            this.BonusLevel.Location = new System.Drawing.Point(200, 296);
            this.BonusLevel.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.BonusLevel.MaxLength = 31;
            this.BonusLevel.Name = "BonusLevel";
            this.BonusLevel.Size = new System.Drawing.Size(463, 38);
            this.BonusLevel.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 234);
            this.label4.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(164, 32);
            this.label4.TabIndex = 7;
            this.label4.Text = "Secret level";
            // 
            // SecretLevel
            // 
            this.SecretLevel.Location = new System.Drawing.Point(200, 234);
            this.SecretLevel.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.SecretLevel.MaxLength = 31;
            this.SecretLevel.Name = "SecretLevel";
            this.SecretLevel.Size = new System.Drawing.Size(287, 38);
            this.SecretLevel.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 172);
            this.label3.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 32);
            this.label3.TabIndex = 5;
            this.label3.Text = "Music file";
            // 
            // MusicFile
            // 
            this.MusicFile.Location = new System.Drawing.Point(200, 172);
            this.MusicFile.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.MusicFile.MaxLength = 31;
            this.MusicFile.Name = "MusicFile";
            this.MusicFile.Size = new System.Drawing.Size(287, 38);
            this.MusicFile.TabIndex = 5;
            this.MusicFile.TextChanged += new System.EventHandler(this.MusicFile_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 110);
            this.label2.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(140, 32);
            this.label2.TabIndex = 3;
            this.label2.Text = "Next level";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 48);
            this.label1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(162, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Level name";
            // 
            // LevelName
            // 
            this.LevelName.Location = new System.Drawing.Point(200, 48);
            this.LevelName.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.LevelName.MaxLength = 31;
            this.LevelName.Name = "LevelName";
            this.LevelName.Size = new System.Drawing.Size(463, 38);
            this.LevelName.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.MinLight);
            this.groupBox2.Controls.Add(this.StartLight);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(35, 525);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.groupBox2.Size = new System.Drawing.Size(293, 176);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Ambient Lighting";
            // 
            // MinLight
            // 
            this.MinLight.Location = new System.Drawing.Point(141, 105);
            this.MinLight.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.MinLight.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.MinLight.Name = "MinLight";
            this.MinLight.Size = new System.Drawing.Size(123, 38);
            this.MinLight.TabIndex = 13;
            this.MinLight.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // StartLight
            // 
            this.StartLight.Location = new System.Drawing.Point(141, 43);
            this.StartLight.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.StartLight.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.StartLight.Name = "StartLight";
            this.StartLight.Size = new System.Drawing.Size(123, 38);
            this.StartLight.TabIndex = 12;
            this.StartLight.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 110);
            this.label7.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(69, 32);
            this.label7.TabIndex = 1;
            this.label7.Text = "Min.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 48);
            this.label6.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 32);
            this.label6.TabIndex = 0;
            this.label6.Text = "Start";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioButton2);
            this.groupBox3.Controls.Add(this.radioButton1);
            this.groupBox3.Location = new System.Drawing.Point(347, 525);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.groupBox3.Size = new System.Drawing.Size(373, 176);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Splitscreen";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(16, 112);
            this.radioButton2.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(297, 36);
            this.radioButton2.TabIndex = 15;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Vertical Splitscreen";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(16, 55);
            this.radioButton1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(330, 36);
            this.radioButton1.TabIndex = 14;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Horizontal Splitscreen";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // OpenMusicDialog
            // 
            this.OpenMusicDialog.DefaultExt = "j2b";
            this.OpenMusicDialog.Filter = "Jazz 2 Music Files|*.j2b|All Modules|*.j2b;*.it;*.mod;*.s3m;*.xm|MP3 Files|*.mp3";
            this.OpenMusicDialog.FilterIndex = 2;
            this.OpenMusicDialog.Title = "Open";
            // 
            // argumentsGenerate
            // 
            this.argumentsGenerate.Location = new System.Drawing.Point(509, 352);
            this.argumentsGenerate.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.argumentsGenerate.Name = "argumentsGenerate";
            this.argumentsGenerate.Size = new System.Drawing.Size(160, 48);
            this.argumentsGenerate.TabIndex = 14;
            this.argumentsGenerate.Text = "Generate";
            this.argumentsGenerate.UseVisualStyleBackColor = true;
            this.argumentsGenerate.Click += new System.EventHandler(this.argumentsGenerate_Click);
            // 
            // LevelProperties
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(971, 737);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LevelProperties";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Level Properties";
            this.Load += new System.EventHandler(this.LevelProperties_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinLight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.StartLight)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox HideHCL;
        private System.Windows.Forms.ComboBox NextLevel;
        private System.Windows.Forms.Button BrowseSecret;
        private System.Windows.Forms.Button BrowseMusic;
        private System.Windows.Forms.Button BrowseNext;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox BonusLevel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox SecretLevel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox MusicFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox LevelName;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown MinLight;
        private System.Windows.Forms.NumericUpDown StartLight;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.CheckBox IsMultiplayer;
        private System.Windows.Forms.OpenFileDialog OpenMusicDialog;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox Arguments;
        private System.Windows.Forms.Button argumentsGenerate;
    }
}
