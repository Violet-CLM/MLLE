﻿namespace MLLE
{
    partial class SelectTileSet
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
            this.OK = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.boxName = new System.Windows.Forms.TextBox();
            this.boxFilename = new System.Windows.Forms.TextBox();
            this.boxImage = new System.Windows.Forms.ComboBox();
            this.boxMask = new System.Windows.Forms.ComboBox();
            this.box32 = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(227, 8);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 4;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(227, 37);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 5;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Filename";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "8-bit   Image";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Mask";
            // 
            // boxName
            // 
            this.boxName.Location = new System.Drawing.Point(67, 8);
            this.boxName.MaxLength = 31;
            this.boxName.Name = "boxName";
            this.boxName.Size = new System.Drawing.Size(154, 20);
            this.boxName.TabIndex = 0;
            this.boxName.TextChanged += new System.EventHandler(this.boxName_TextChanged);
            // 
            // boxFilename
            // 
            this.boxFilename.Location = new System.Drawing.Point(67, 34);
            this.boxFilename.MaxLength = 250;
            this.boxFilename.Name = "boxFilename";
            this.boxFilename.Size = new System.Drawing.Size(154, 20);
            this.boxFilename.TabIndex = 1;
            this.boxFilename.TextChanged += new System.EventHandler(this.boxFilename_TextChanged);
            // 
            // boxImage
            // 
            this.boxImage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boxImage.FormattingEnabled = true;
            this.boxImage.Items.AddRange(new object[] {
            "(none)"});
            this.boxImage.Location = new System.Drawing.Point(67, 61);
            this.boxImage.Name = "boxImage";
            this.boxImage.Size = new System.Drawing.Size(154, 21);
            this.boxImage.Sorted = true;
            this.boxImage.TabIndex = 2;
            // 
            // boxMask
            // 
            this.boxMask.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boxMask.FormattingEnabled = true;
            this.boxMask.Items.AddRange(new object[] {
            "(auto mask)"});
            this.boxMask.Location = new System.Drawing.Point(67, 113);
            this.boxMask.Name = "boxMask";
            this.boxMask.Size = new System.Drawing.Size(154, 21);
            this.boxMask.Sorted = true;
            this.boxMask.TabIndex = 3;
            // 
            // box32
            // 
            this.box32.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.box32.FormattingEnabled = true;
            this.box32.Items.AddRange(new object[] {
            "(none)"});
            this.box32.Location = new System.Drawing.Point(67, 86);
            this.box32.Name = "box32";
            this.box32.Size = new System.Drawing.Size(154, 21);
            this.box32.Sorted = true;
            this.box32.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 89);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "32-bit Image";
            // 
            // SelectTileSet
            // 
            this.AcceptButton = this.OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(314, 146);
            this.Controls.Add(this.box32);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.boxMask);
            this.Controls.Add(this.boxImage);
            this.Controls.Add(this.boxFilename);
            this.Controls.Add(this.boxName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectTileSet";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Tile Set";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox boxName;
        private System.Windows.Forms.TextBox boxFilename;
        private System.Windows.Forms.ComboBox boxImage;
        private System.Windows.Forms.ComboBox boxMask;
        private System.Windows.Forms.ComboBox box32;
        private System.Windows.Forms.Label label5;
    }
}