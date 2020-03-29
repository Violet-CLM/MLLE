namespace MLLE
{
    partial class SmartTilesForm
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
            this.tilesetPicture = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.smartPicture = new System.Windows.Forms.PictureBox();
            this.framesPicture = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.HighlightPanel = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.AddRuleButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.checkedComboBox1 = new CheckComboBoxTest.CheckedComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.tilesetPicture)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.smartPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.framesPicture)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tilesetPicture
            // 
            this.tilesetPicture.Location = new System.Drawing.Point(0, 0);
            this.tilesetPicture.Margin = new System.Windows.Forms.Padding(0);
            this.tilesetPicture.Name = "tilesetPicture";
            this.tilesetPicture.Size = new System.Drawing.Size(320, 50);
            this.tilesetPicture.TabIndex = 0;
            this.tilesetPicture.TabStop = false;
            this.tilesetPicture.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tilesetPicture_MouseClick);
            this.tilesetPicture.MouseLeave += new System.EventHandler(this.tilesetPicture_MouseLeave);
            this.tilesetPicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tilesetPicture_MouseMove);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.tilesetPicture);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(344, 320);
            this.panel1.TabIndex = 1;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(706, 39);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 5;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.Location = new System.Drawing.Point(706, 10);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 4;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // smartPicture
            // 
            this.smartPicture.Location = new System.Drawing.Point(0, 0);
            this.smartPicture.Margin = new System.Windows.Forms.Padding(0);
            this.smartPicture.Name = "smartPicture";
            this.smartPicture.Size = new System.Drawing.Size(320, 320);
            this.smartPicture.TabIndex = 0;
            this.smartPicture.TabStop = false;
            this.smartPicture.MouseClick += new System.Windows.Forms.MouseEventHandler(this.smartPicture_MouseClick);
            this.smartPicture.MouseLeave += new System.EventHandler(this.tilesetPicture_MouseLeave);
            this.smartPicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.smartPicture_MouseMove);
            // 
            // framesPicture
            // 
            this.framesPicture.Location = new System.Drawing.Point(0, 0);
            this.framesPicture.Margin = new System.Windows.Forms.Padding(0);
            this.framesPicture.Name = "framesPicture";
            this.framesPicture.Size = new System.Drawing.Size(32, 32);
            this.framesPicture.TabIndex = 0;
            this.framesPicture.TabStop = false;
            this.framesPicture.MouseClick += new System.Windows.Forms.MouseEventHandler(this.framesPicture_MouseClick);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.HighlightPanel);
            this.panel2.Controls.Add(this.smartPicture);
            this.panel2.Location = new System.Drawing.Point(374, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(320, 320);
            this.panel2.TabIndex = 2;
            // 
            // HighlightPanel
            // 
            this.HighlightPanel.BackColor = System.Drawing.Color.White;
            this.HighlightPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.HighlightPanel.CausesValidation = false;
            this.HighlightPanel.Enabled = false;
            this.HighlightPanel.Location = new System.Drawing.Point(80, 103);
            this.HighlightPanel.Name = "HighlightPanel";
            this.HighlightPanel.Size = new System.Drawing.Size(10, 10);
            this.HighlightPanel.TabIndex = 6;
            this.HighlightPanel.Visible = false;
            // 
            // panel3
            // 
            this.panel3.AutoScroll = true;
            this.panel3.Controls.Add(this.framesPicture);
            this.panel3.Location = new System.Drawing.Point(706, 103);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(56, 228);
            this.panel3.TabIndex = 2;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(527, 337);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(254, 20);
            this.textBox1.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(483, 340);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 341);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Rules:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(233, 341);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Friends:";
            // 
            // panel4
            // 
            this.panel4.AutoScroll = true;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel4.Controls.Add(this.AddRuleButton);
            this.panel4.Location = new System.Drawing.Point(15, 358);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(766, 122);
            this.panel4.TabIndex = 11;
            // 
            // AddRuleButton
            // 
            this.AddRuleButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.AddRuleButton.Location = new System.Drawing.Point(0, 0);
            this.AddRuleButton.Name = "AddRuleButton";
            this.AddRuleButton.Size = new System.Drawing.Size(762, 32);
            this.AddRuleButton.TabIndex = 12;
            this.AddRuleButton.Text = "+";
            this.AddRuleButton.UseVisualStyleBackColor = true;
            this.AddRuleButton.Click += new System.EventHandler(this.AddRuleButton_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(706, 68);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 12;
            this.button1.Text = "Delete";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkedComboBox1
            // 
            this.checkedComboBox1.CheckOnClick = true;
            this.checkedComboBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.checkedComboBox1.DropDownHeight = 1;
            this.checkedComboBox1.FormattingEnabled = true;
            this.checkedComboBox1.IntegralHeight = false;
            this.checkedComboBox1.Location = new System.Drawing.Point(283, 337);
            this.checkedComboBox1.Name = "checkedComboBox1";
            this.checkedComboBox1.Size = new System.Drawing.Size(121, 21);
            this.checkedComboBox1.TabIndex = 9;
            this.checkedComboBox1.ValueSeparator = ", ";
            // 
            // SmartTilesForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(793, 491);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkedComboBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SmartTilesForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Define Smart Tiles";
            ((System.ComponentModel.ISupportInitialize)(this.tilesetPicture)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.smartPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.framesPicture)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox tilesetPicture;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox smartPicture;
        private System.Windows.Forms.Panel HighlightPanel;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.PictureBox framesPicture;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private CheckComboBoxTest.CheckedComboBox checkedComboBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button AddRuleButton;
        private System.Windows.Forms.Button button1;
    }
}