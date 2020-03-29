namespace MLLE
{
    partial class PackageLevelForm
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.checkboxMissing = new System.Windows.Forms.CheckBox();
            this.CancelButton = new System.Windows.Forms.Button();
            this.checkboxMultipleLevels = new System.Windows.Forms.CheckBox();
            this.checkboxExcludeDefault = new System.Windows.Forms.CheckBox();
            this.checkboxIncludeMusic = new System.Windows.Forms.CheckBox();
            this.checkboxAngelscriptFunctions = new System.Windows.Forms.CheckBox();
            this.checkBoxMLLESet = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Window;
            this.textBox1.Location = new System.Drawing.Point(12, 78);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(390, 107);
            this.textBox1.TabIndex = 0;
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(327, 191);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "Start";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // checkboxMissing
            // 
            this.checkboxMissing.AutoSize = true;
            this.checkboxMissing.Checked = true;
            this.checkboxMissing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxMissing.Location = new System.Drawing.Point(16, 10);
            this.checkboxMissing.Name = "checkboxMissing";
            this.checkboxMissing.Size = new System.Drawing.Size(136, 17);
            this.checkboxMissing.TabIndex = 3;
            this.checkboxMissing.Text = "Prompt for Missing Files";
            this.checkboxMissing.UseVisualStyleBackColor = true;
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(246, 192);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 4;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // checkboxMultipleLevels
            // 
            this.checkboxMultipleLevels.AutoSize = true;
            this.checkboxMultipleLevels.Checked = true;
            this.checkboxMultipleLevels.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxMultipleLevels.Location = new System.Drawing.Point(16, 34);
            this.checkboxMultipleLevels.Name = "checkboxMultipleLevels";
            this.checkboxMultipleLevels.Size = new System.Drawing.Size(142, 17);
            this.checkboxMultipleLevels.TabIndex = 5;
            this.checkboxMultipleLevels.Text = "Package Multiple Levels";
            this.checkboxMultipleLevels.UseVisualStyleBackColor = true;
            // 
            // checkboxExcludeDefault
            // 
            this.checkboxExcludeDefault.AutoSize = true;
            this.checkboxExcludeDefault.Checked = true;
            this.checkboxExcludeDefault.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxExcludeDefault.Location = new System.Drawing.Point(228, 34);
            this.checkboxExcludeDefault.Name = "checkboxExcludeDefault";
            this.checkboxExcludeDefault.Size = new System.Drawing.Size(123, 17);
            this.checkboxExcludeDefault.TabIndex = 6;
            this.checkboxExcludeDefault.Text = "Exclude Official Files";
            this.checkboxExcludeDefault.UseVisualStyleBackColor = true;
            // 
            // checkboxIncludeMusic
            // 
            this.checkboxIncludeMusic.AutoSize = true;
            this.checkboxIncludeMusic.Checked = true;
            this.checkboxIncludeMusic.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxIncludeMusic.Location = new System.Drawing.Point(228, 10);
            this.checkboxIncludeMusic.Name = "checkboxIncludeMusic";
            this.checkboxIncludeMusic.Size = new System.Drawing.Size(92, 17);
            this.checkboxIncludeMusic.TabIndex = 7;
            this.checkboxIncludeMusic.Text = "Include Music";
            this.checkboxIncludeMusic.UseVisualStyleBackColor = true;
            // 
            // checkboxAngelscriptFunctions
            // 
            this.checkboxAngelscriptFunctions.AutoSize = true;
            this.checkboxAngelscriptFunctions.Location = new System.Drawing.Point(16, 55);
            this.checkboxAngelscriptFunctions.Name = "checkboxAngelscriptFunctions";
            this.checkboxAngelscriptFunctions.Size = new System.Drawing.Size(211, 17);
            this.checkboxAngelscriptFunctions.TabIndex = 8;
            this.checkboxAngelscriptFunctions.Text = "Search AngelScript Function Filenames";
            this.checkboxAngelscriptFunctions.UseVisualStyleBackColor = true;
            // 
            // checkBoxMLLESet
            // 
            this.checkBoxMLLESet.AutoSize = true;
            this.checkBoxMLLESet.Location = new System.Drawing.Point(228, 55);
            this.checkBoxMLLESet.Name = "checkBoxMLLESet";
            this.checkBoxMLLESet.Size = new System.Drawing.Size(163, 17);
            this.checkBoxMLLESet.TabIndex = 9;
            this.checkBoxMLLESet.Text = "Include Smart Tile Definitions";
            this.checkBoxMLLESet.UseVisualStyleBackColor = true;
            // 
            // PackageLevelForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 226);
            this.Controls.Add(this.checkBoxMLLESet);
            this.Controls.Add(this.checkboxAngelscriptFunctions);
            this.Controls.Add(this.checkboxIncludeMusic);
            this.Controls.Add(this.checkboxExcludeDefault);
            this.Controls.Add(this.checkboxMultipleLevels);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.checkboxMissing);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PackageLevelForm";
            this.ShowInTaskbar = false;
            this.Text = "Package in ZIP file";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.CheckBox checkboxMissing;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.CheckBox checkboxMultipleLevels;
        private System.Windows.Forms.CheckBox checkboxExcludeDefault;
        private System.Windows.Forms.CheckBox checkboxIncludeMusic;
        private System.Windows.Forms.CheckBox checkboxAngelscriptFunctions;
        private System.Windows.Forms.CheckBox checkBoxMLLESet;
    }
}