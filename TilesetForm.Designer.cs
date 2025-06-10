﻿namespace MLLE
{
    partial class TilesetForm
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.EdgePanelRight = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.EdgePanelLeft = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.inputFirst = new System.Windows.Forms.NumericUpDown();
            this.inputLast = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.outputMath = new System.Windows.Forms.Label();
            this.ButtonDelete = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ColorsList = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.EdgePanelRight.SuspendLayout();
            this.EdgePanelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputFirst)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputLast)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(320, 50);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox1, "Left or right click to set this as the first or last imported tile.");
            this.pictureBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseClick);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.pictureBox1_MouseLeave);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.EdgePanelRight);
            this.panel1.Controls.Add(this.EdgePanelLeft);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(344, 320);
            this.panel1.TabIndex = 1;
            // 
            // EdgePanelRight
            // 
            this.EdgePanelRight.BackColor = System.Drawing.Color.White;
            this.EdgePanelRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EdgePanelRight.CausesValidation = false;
            this.EdgePanelRight.Controls.Add(this.label4);
            this.EdgePanelRight.Enabled = false;
            this.EdgePanelRight.Location = new System.Drawing.Point(138, 232);
            this.EdgePanelRight.Name = "EdgePanelRight";
            this.EdgePanelRight.Size = new System.Drawing.Size(10, 36);
            this.EdgePanelRight.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-2, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(13, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "<";
            // 
            // EdgePanelLeft
            // 
            this.EdgePanelLeft.BackColor = System.Drawing.Color.White;
            this.EdgePanelLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EdgePanelLeft.CausesValidation = false;
            this.EdgePanelLeft.Controls.Add(this.label3);
            this.EdgePanelLeft.Enabled = false;
            this.EdgePanelLeft.Location = new System.Drawing.Point(121, 232);
            this.EdgePanelLeft.Name = "EdgePanelLeft";
            this.EdgePanelLeft.Size = new System.Drawing.Size(10, 36);
            this.EdgePanelLeft.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-2, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(13, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = ">";
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(362, 39);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(90, 23);
            this.ButtonCancel.TabIndex = 5;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.Location = new System.Drawing.Point(362, 10);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(90, 23);
            this.OKButton.TabIndex = 4;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(359, 121);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Tiles >=";
            // 
            // inputFirst
            // 
            this.inputFirst.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.inputFirst.Location = new System.Drawing.Point(362, 137);
            this.inputFirst.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.inputFirst.Name = "inputFirst";
            this.inputFirst.Size = new System.Drawing.Size(96, 20);
            this.inputFirst.TabIndex = 7;
            this.inputFirst.ValueChanged += new System.EventHandler(this.inputFirst_ValueChanged);
            // 
            // inputLast
            // 
            this.inputLast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.inputLast.Location = new System.Drawing.Point(362, 176);
            this.inputLast.Name = "inputLast";
            this.inputLast.Size = new System.Drawing.Size(96, 20);
            this.inputLast.TabIndex = 9;
            this.inputLast.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.inputLast.ValueChanged += new System.EventHandler(this.inputLast_ValueChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(359, 160);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Tiles <";
            // 
            // outputMath
            // 
            this.outputMath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputMath.AutoSize = true;
            this.outputMath.Location = new System.Drawing.Point(359, 199);
            this.outputMath.Name = "outputMath";
            this.outputMath.Size = new System.Drawing.Size(35, 13);
            this.outputMath.TabIndex = 10;
            this.outputMath.Text = "label3";
            // 
            // ButtonDelete
            // 
            this.ButtonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonDelete.Location = new System.Drawing.Point(362, 68);
            this.ButtonDelete.Name = "ButtonDelete";
            this.ButtonDelete.Size = new System.Drawing.Size(90, 23);
            this.ButtonDelete.TabIndex = 11;
            this.ButtonDelete.Text = "Delete";
            this.ButtonDelete.UseVisualStyleBackColor = true;
            this.ButtonDelete.Click += new System.EventHandler(this.ButtonDelete_Click);
            // 
            // ColorsList
            // 
            this.ColorsList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ColorsList.FormattingEnabled = true;
            this.ColorsList.Items.AddRange(new object[] {
            "[Same Indices]",
            "[Remapped...]",
            "[Tileset Palette]"});
            this.ColorsList.Location = new System.Drawing.Point(362, 276);
            this.ColorsList.Name = "ColorsList";
            this.ColorsList.ScrollAlwaysVisible = true;
            this.ColorsList.Size = new System.Drawing.Size(96, 56);
            this.ColorsList.TabIndex = 13;
            this.ColorsList.SelectedIndexChanged += new System.EventHandler(this.ColorsList_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(359, 260);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "8-bit tile colors:";
            // 
            // TilesetForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(464, 339);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ColorsList);
            this.Controls.Add(this.ButtonDelete);
            this.Controls.Add(this.outputMath);
            this.Controls.Add(this.inputLast);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.inputFirst);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TilesetForm";
            this.ShowInTaskbar = false;
            this.Text = "Setup Extra Tileset";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.EdgePanelRight.ResumeLayout(false);
            this.EdgePanelRight.PerformLayout();
            this.EdgePanelLeft.ResumeLayout(false);
            this.EdgePanelLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputFirst)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputLast)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown inputFirst;
        private System.Windows.Forms.NumericUpDown inputLast;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label outputMath;
        private System.Windows.Forms.Button ButtonDelete;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel EdgePanelRight;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel EdgePanelLeft;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox ColorsList;
        private System.Windows.Forms.Label label5;
    }
}