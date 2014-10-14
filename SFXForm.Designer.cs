
    partial class SFXForm
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
            this.InLevel = new System.Windows.Forms.ListBox();
            this.Files = new System.Windows.Forms.ListBox();
            this.Sounds = new System.Windows.Forms.ListBox();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // InLevel
            // 
            this.InLevel.FormattingEnabled = true;
            this.InLevel.Items.AddRange(new object[] {
            "0: ?",
            "1: ?",
            "2: ?",
            "3: Blank?",
            "4: Hurt1",
            "5: Hurt2",
            "6: ?",
            "7: ?",
            "8: ?",
            "9: ?",
            "10: ?",
            "11: Blank?",
            "12: Blank?",
            "13: ?",
            "14: ?",
            "15: ?",
            "16: ?",
            "17: ?",
            "18: ?",
            "19: ?",
            "20: ?",
            "21: ?",
            "22: ?",
            "23: ?",
            "24: ?",
            "25: ?",
            "26: ?",
            "27: ?",
            "28: Blank?",
            "29: Blank?",
            "30: Blank?",
            "31: Blank?",
            "32: item",
            "33: heart",
            "34: cake",
            "35: sundae",
            "36: poster",
            "37: woodsmash",
            "38: glasssmash",
            "39: helptext",
            "40: ?",
            "41: ?",
            "42: ?",
            "43: ?",
            "44: ?",
            "45: ?",
            "46: ?",
            "47: Theme"});
            this.InLevel.Location = new System.Drawing.Point(12, 25);
            this.InLevel.Name = "InLevel";
            this.InLevel.Size = new System.Drawing.Size(120, 342);
            this.InLevel.TabIndex = 0;
            this.InLevel.SelectedIndexChanged += new System.EventHandler(this.InLevel_SelectedIndexChanged);
            // 
            // Files
            // 
            this.Files.FormattingEnabled = true;
            this.Files.Items.AddRange(new object[] {
            "(null)"});
            this.Files.Location = new System.Drawing.Point(140, 25);
            this.Files.Name = "Files";
            this.Files.Size = new System.Drawing.Size(120, 277);
            this.Files.TabIndex = 1;
            this.Files.SelectedIndexChanged += new System.EventHandler(this.Files_SelectedIndexChanged);
            // 
            // Sounds
            // 
            this.Sounds.FormattingEnabled = true;
            this.Sounds.Location = new System.Drawing.Point(266, 25);
            this.Sounds.Name = "Sounds";
            this.Sounds.Size = new System.Drawing.Size(120, 342);
            this.Sounds.TabIndex = 2;
            this.Sounds.SelectedIndexChanged += new System.EventHandler(this.Sounds_SelectedIndexChanged);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(140, 342);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(120, 23);
            this.ButtonCancel.TabIndex = 3;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonOK
            // 
            this.ButtonOK.Location = new System.Drawing.Point(140, 313);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(120, 23);
            this.ButtonOK.TabIndex = 4;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Function";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(140, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "File";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(266, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Sound";
            // 
            // SFXForm
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(401, 377);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.Sounds);
            this.Controls.Add(this.Files);
            this.Controls.Add(this.InLevel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SFXForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Choose sound effects";
            this.Load += new System.EventHandler(this.SFXForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox InLevel;
        private System.Windows.Forms.ListBox Files;
        private System.Windows.Forms.ListBox Sounds;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }