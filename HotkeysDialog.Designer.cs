namespace MLLE
{
    partial class HotkeysDialog
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
            this.dataGridViewHotkeys = new System.Windows.Forms.DataGridView();
            this.buttonClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewHotkeys)).BeginInit();
            this.SuspendLayout();
            //
            // dataGridViewHotkeys
            //
            this.dataGridViewHotkeys.AllowUserToAddRows = false;
            this.dataGridViewHotkeys.AllowUserToDeleteRows = false;
            this.dataGridViewHotkeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewHotkeys.Location = new System.Drawing.Point(12, 12);
            this.dataGridViewHotkeys.Name = "dataGridViewHotkeys";
            this.dataGridViewHotkeys.ReadOnly = true;
            this.dataGridViewHotkeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewHotkeys.Size = new System.Drawing.Size(660, 400);
            this.dataGridViewHotkeys.TabIndex = 0;
            //
            // buttonClose
            //
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.Location = new System.Drawing.Point(597, 418);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            //
            // HotkeysDialog
            //
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(684, 451);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.dataGridViewHotkeys);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HotkeysDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MLLE Hotkeys";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewHotkeys)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewHotkeys;
        private System.Windows.Forms.Button buttonClose;
    }
}