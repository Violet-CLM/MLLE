namespace MLLE
{
    partial class PaletteForm
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
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.LoadButton = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.selectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.select1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.select8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.select16ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gradientToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.swapRedGreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.swapGreenBlueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.swapBlueRedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SaveButton = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.ButtonDelete = new System.Windows.Forms.Button();
            this.NameLabel = new System.Windows.Forms.Label();
            this.NameBox = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(327, 59);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 1;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(327, 30);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetButton.Location = new System.Drawing.Point(327, 332);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(75, 23);
            this.ResetButton.TabIndex = 4;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // LoadButton
            // 
            this.LoadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadButton.Location = new System.Drawing.Point(327, 244);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(75, 23);
            this.LoadButton.TabIndex = 2;
            this.LoadButton.Text = "Load...";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // colorDialog1
            // 
            this.colorDialog1.FullOpen = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectionToolStripMenuItem,
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(414, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // selectionToolStripMenuItem
            // 
            this.selectionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectNoneToolStripMenuItem,
            this.toolStripSeparator1,
            this.select1ToolStripMenuItem,
            this.select8ToolStripMenuItem,
            this.select16ToolStripMenuItem});
            this.selectionToolStripMenuItem.Name = "selectionToolStripMenuItem";
            this.selectionToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.selectionToolStripMenuItem.Text = "Selection";
            // 
            // selectNoneToolStripMenuItem
            // 
            this.selectNoneToolStripMenuItem.Name = "selectNoneToolStripMenuItem";
            this.selectNoneToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.selectNoneToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.selectNoneToolStripMenuItem.Text = "Select None";
            this.selectNoneToolStripMenuItem.Click += new System.EventHandler(this.selectNoneToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(176, 6);
            // 
            // select1ToolStripMenuItem
            // 
            this.select1ToolStripMenuItem.Checked = true;
            this.select1ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.select1ToolStripMenuItem.Name = "select1ToolStripMenuItem";
            this.select1ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D1)));
            this.select1ToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.select1ToolStripMenuItem.Text = "Select 1";
            this.select1ToolStripMenuItem.Click += new System.EventHandler(this.select1ToolStripMenuItem_Click);
            // 
            // select8ToolStripMenuItem
            // 
            this.select8ToolStripMenuItem.Name = "select8ToolStripMenuItem";
            this.select8ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D8)));
            this.select8ToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.select8ToolStripMenuItem.Text = "Select 8";
            this.select8ToolStripMenuItem.Click += new System.EventHandler(this.select8ToolStripMenuItem_Click);
            // 
            // select16ToolStripMenuItem
            // 
            this.select16ToolStripMenuItem.Name = "select16ToolStripMenuItem";
            this.select16ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D6)));
            this.select16ToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.select16ToolStripMenuItem.Text = "Select 16";
            this.select16ToolStripMenuItem.Click += new System.EventHandler(this.select16ToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gradientToolStripMenuItem,
            this.toolStripSeparator2,
            this.swapRedGreenToolStripMenuItem,
            this.swapGreenBlueToolStripMenuItem,
            this.swapBlueRedToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            this.toolsToolStripMenuItem.DropDownClosed += new System.EventHandler(this.toolsToolStripMenuItem_DropDownClosed);
            this.toolsToolStripMenuItem.DropDownOpening += new System.EventHandler(this.toolsToolStripMenuItem_DropDownOpening);
            // 
            // gradientToolStripMenuItem
            // 
            this.gradientToolStripMenuItem.Name = "gradientToolStripMenuItem";
            this.gradientToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.gradientToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.gradientToolStripMenuItem.Text = "Gradient";
            this.gradientToolStripMenuItem.Click += new System.EventHandler(this.gradientToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(161, 6);
            // 
            // swapRedGreenToolStripMenuItem
            // 
            this.swapRedGreenToolStripMenuItem.Name = "swapRedGreenToolStripMenuItem";
            this.swapRedGreenToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.swapRedGreenToolStripMenuItem.Text = "Swap Red/Green";
            this.swapRedGreenToolStripMenuItem.Click += new System.EventHandler(this.swapRedGreenToolStripMenuItem_Click);
            // 
            // swapGreenBlueToolStripMenuItem
            // 
            this.swapGreenBlueToolStripMenuItem.Name = "swapGreenBlueToolStripMenuItem";
            this.swapGreenBlueToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.swapGreenBlueToolStripMenuItem.Text = "Swap Green/Blue";
            this.swapGreenBlueToolStripMenuItem.Click += new System.EventHandler(this.swapGreenBlueToolStripMenuItem_Click);
            // 
            // swapBlueRedToolStripMenuItem
            // 
            this.swapBlueRedToolStripMenuItem.Name = "swapBlueRedToolStripMenuItem";
            this.swapBlueRedToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.swapBlueRedToolStripMenuItem.Text = "Swap Blue/Red";
            this.swapBlueRedToolStripMenuItem.Click += new System.EventHandler(this.swapBlueRedToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "j2t";
            this.openFileDialog1.Filter = "Tilesets|*.j2t|Color Tables|*.pal|All files|*.*";
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveButton.Location = new System.Drawing.Point(327, 273);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 3;
            this.SaveButton.Text = "Save...";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(4, 344);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(1);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(227, 17);
            this.checkBox1.TabIndex = 7;
            this.checkBox1.Text = "Reapply palette after dying in Single Player";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // ButtonDelete
            // 
            this.ButtonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonDelete.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.ButtonDelete.Location = new System.Drawing.Point(327, 88);
            this.ButtonDelete.Name = "ButtonDelete";
            this.ButtonDelete.Size = new System.Drawing.Size(75, 23);
            this.ButtonDelete.TabIndex = 8;
            this.ButtonDelete.Text = "Delete";
            this.ButtonDelete.UseVisualStyleBackColor = true;
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Location = new System.Drawing.Point(4, 344);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(35, 13);
            this.NameLabel.TabIndex = 9;
            this.NameLabel.Text = "Name";
            // 
            // NameBox
            // 
            this.NameBox.Location = new System.Drawing.Point(46, 344);
            this.NameBox.Name = "NameBox";
            this.NameBox.Size = new System.Drawing.Size(185, 20);
            this.NameBox.TabIndex = 10;
            // 
            // PaletteForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(414, 367);
            this.Controls.Add(this.NameBox);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.ButtonDelete);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.LoadButton);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PaletteForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Level Palette";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem selectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectNoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem select1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem select8ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem select16ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gradientToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem swapRedGreenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem swapGreenBlueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem swapBlueRedToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button ButtonDelete;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.TextBox NameBox;
    }
}