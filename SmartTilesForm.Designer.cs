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
            this.components = new System.ComponentModel.Container();
            this.tilesetPicture = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.smartPicture = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.HighlightPanel = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.framesPicture = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.tilesetPicture)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.smartPicture)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.framesPicture)).BeginInit();
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
            this.toolTip1.SetToolTip(this.tilesetPicture, "Left or right click to set this as the first or last imported tile.");
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
            this.toolTip1.SetToolTip(this.smartPicture, "Left or right click to set this as the first or last imported tile.");
            this.smartPicture.MouseClick += new System.Windows.Forms.MouseEventHandler(this.smartPicture_MouseClick);
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
            this.panel3.Location = new System.Drawing.Point(706, 76);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(56, 256);
            this.panel3.TabIndex = 2;
            // 
            // framesPicture
            // 
            this.framesPicture.Location = new System.Drawing.Point(0, 0);
            this.framesPicture.Margin = new System.Windows.Forms.Padding(0);
            this.framesPicture.Name = "framesPicture";
            this.framesPicture.Size = new System.Drawing.Size(32, 32);
            this.framesPicture.TabIndex = 0;
            this.framesPicture.TabStop = false;
            this.toolTip1.SetToolTip(this.framesPicture, "Preview frames");
            this.framesPicture.MouseClick += new System.Windows.Forms.MouseEventHandler(this.framesPicture_MouseClick);
            // 
            // SmartTilesForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(793, 339);
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
            this.Text = "Define Smart Tiles";
            ((System.ComponentModel.ISupportInitialize)(this.tilesetPicture)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.smartPicture)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.framesPicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox tilesetPicture;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox smartPicture;
        private System.Windows.Forms.Panel HighlightPanel;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.PictureBox framesPicture;
    }
}