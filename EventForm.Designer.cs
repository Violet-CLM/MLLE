namespace MLLE
{
    partial class EventForm
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
            this.Tree = new System.Windows.Forms.TreeView();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Illuminate = new System.Windows.Forms.CheckBox();
            this.Generator = new System.Windows.Forms.CheckBox();
            this.ModeSelect = new System.Windows.Forms.ComboBox();
            this.ModeLabel = new System.Windows.Forms.Label();
            this.Bitfield = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.BitBox1 = new System.Windows.Forms.CheckBox();
            this.BitBox2 = new System.Windows.Forms.CheckBox();
            this.BitBox3 = new System.Windows.Forms.CheckBox();
            this.BitBox4 = new System.Windows.Forms.CheckBox();
            this.BitBox5 = new System.Windows.Forms.CheckBox();
            this.BitBox10 = new System.Windows.Forms.CheckBox();
            this.BitBox9 = new System.Windows.Forms.CheckBox();
            this.BitBox8 = new System.Windows.Forms.CheckBox();
            this.BitBox7 = new System.Windows.Forms.CheckBox();
            this.BitBox6 = new System.Windows.Forms.CheckBox();
            this.ListBox = new System.Windows.Forms.ListBox();
            this.EventNameInput = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tree
            // 
            this.Tree.AllowDrop = true;
            this.Tree.HideSelection = false;
            this.Tree.Location = new System.Drawing.Point(12, 31);
            this.Tree.Name = "Tree";
            this.Tree.Size = new System.Drawing.Size(329, 233);
            this.Tree.TabIndex = 51;
            this.Tree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.Tree_ItemDrag);
            this.Tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.Tree_AfterSelect);
            // 
            // ButtonOK
            // 
            this.ButtonOK.Location = new System.Drawing.Point(347, 17);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(115, 23);
            this.ButtonOK.TabIndex = 1;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(347, 47);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(115, 23);
            this.ButtonCancel.TabIndex = 2;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "label1";
            this.label1.Visible = false;
            // 
            // Illuminate
            // 
            this.Illuminate.AutoSize = true;
            this.Illuminate.Location = new System.Drawing.Point(12, 293);
            this.Illuminate.Name = "Illuminate";
            this.Illuminate.Size = new System.Drawing.Size(135, 17);
            this.Illuminate.TabIndex = 16;
            this.Illuminate.Text = "Illuminate Surroundings";
            this.Illuminate.UseVisualStyleBackColor = true;
            this.Illuminate.CheckedChanged += new System.EventHandler(this.checkBox_CheckStateChanged);
            // 
            // Generator
            // 
            this.Generator.AutoSize = true;
            this.Generator.Location = new System.Drawing.Point(12, 270);
            this.Generator.Name = "Generator";
            this.Generator.Size = new System.Drawing.Size(178, 17);
            this.Generator.TabIndex = 17;
            this.Generator.Text = "Create a generator for this event";
            this.Generator.UseVisualStyleBackColor = true;
            this.Generator.CheckedChanged += new System.EventHandler(this.Generator_CheckedChanged);
            // 
            // ModeSelect
            // 
            this.ModeSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ModeSelect.FormattingEnabled = true;
            this.ModeSelect.Location = new System.Drawing.Point(244, 289);
            this.ModeSelect.Name = "ModeSelect";
            this.ModeSelect.Size = new System.Drawing.Size(85, 21);
            this.ModeSelect.TabIndex = 18;
            this.ModeSelect.SelectedIndexChanged += new System.EventHandler(this.checkBox_CheckStateChanged);
            // 
            // ModeLabel
            // 
            this.ModeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ModeLabel.Location = new System.Drawing.Point(145, 293);
            this.ModeLabel.Name = "ModeLabel";
            this.ModeLabel.Size = new System.Drawing.Size(93, 13);
            this.ModeLabel.TabIndex = 19;
            this.ModeLabel.Text = "Difficulty";
            this.ModeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Bitfield
            // 
            this.Bitfield.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Bitfield.AutoSize = true;
            this.Bitfield.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.Bitfield.Location = new System.Drawing.Point(142, 15);
            this.Bitfield.Margin = new System.Windows.Forms.Padding(0);
            this.Bitfield.Name = "Bitfield";
            this.Bitfield.Size = new System.Drawing.Size(199, 13);
            this.Bitfield.TabIndex = 20;
            this.Bitfield.Text = "00000000000000000000000000000000";
            this.Bitfield.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Bitfield_MouseClick);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.AllowDrop = true;
            this.numericUpDown1.Location = new System.Drawing.Point(6, 20);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(80, 20);
            this.numericUpDown1.TabIndex = 21;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            this.numericUpDown1.DragDrop += new System.Windows.Forms.DragEventHandler(this.numericUpDown_DragDrop);
            this.numericUpDown1.DragEnter += new System.Windows.Forms.DragEventHandler(this.input_DragEnter);
            // 
            // comboBox1
            // 
            this.comboBox1.AllowDrop = true;
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.DropDownWidth = 200;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(6, 20);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(80, 21);
            this.comboBox1.TabIndex = 27;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(6, 22);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(48, 17);
            this.checkBox1.TabIndex = 33;
            this.checkBox1.Text = "True";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckStateChanged += new System.EventHandler(this.checkBox_CheckStateChanged);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.AutoScrollMinSize = new System.Drawing.Size(0, 200);
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Controls.Add(this.numericUpDown1);
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Location = new System.Drawing.Point(347, 78);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(115, 232);
            this.panel1.TabIndex = 39;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(88, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(20, 20);
            this.button1.TabIndex = 35;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.SoundButton_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 20);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(80, 20);
            this.textBox1.TabIndex = 34;
            // 
            // BitBox1
            // 
            this.BitBox1.AutoSize = true;
            this.BitBox1.Location = new System.Drawing.Point(12, 270);
            this.BitBox1.Name = "BitBox1";
            this.BitBox1.Size = new System.Drawing.Size(47, 17);
            this.BitBox1.TabIndex = 40;
            this.BitBox1.Text = "Bit 1";
            this.BitBox1.UseVisualStyleBackColor = true;
            // 
            // BitBox2
            // 
            this.BitBox2.AutoSize = true;
            this.BitBox2.Location = new System.Drawing.Point(81, 270);
            this.BitBox2.Name = "BitBox2";
            this.BitBox2.Size = new System.Drawing.Size(47, 17);
            this.BitBox2.TabIndex = 41;
            this.BitBox2.Text = "Bit 2";
            this.BitBox2.UseVisualStyleBackColor = true;
            // 
            // BitBox3
            // 
            this.BitBox3.AutoSize = true;
            this.BitBox3.Location = new System.Drawing.Point(150, 270);
            this.BitBox3.Name = "BitBox3";
            this.BitBox3.Size = new System.Drawing.Size(47, 17);
            this.BitBox3.TabIndex = 42;
            this.BitBox3.Text = "Bit 3";
            this.BitBox3.UseVisualStyleBackColor = true;
            // 
            // BitBox4
            // 
            this.BitBox4.AutoSize = true;
            this.BitBox4.Location = new System.Drawing.Point(219, 270);
            this.BitBox4.Name = "BitBox4";
            this.BitBox4.Size = new System.Drawing.Size(47, 17);
            this.BitBox4.TabIndex = 43;
            this.BitBox4.Text = "Bit 4";
            this.BitBox4.UseVisualStyleBackColor = true;
            // 
            // BitBox5
            // 
            this.BitBox5.AutoSize = true;
            this.BitBox5.Location = new System.Drawing.Point(279, 270);
            this.BitBox5.Name = "BitBox5";
            this.BitBox5.Size = new System.Drawing.Size(47, 17);
            this.BitBox5.TabIndex = 44;
            this.BitBox5.Text = "Bit 5";
            this.BitBox5.UseVisualStyleBackColor = true;
            // 
            // BitBox10
            // 
            this.BitBox10.AutoSize = true;
            this.BitBox10.Location = new System.Drawing.Point(279, 293);
            this.BitBox10.Name = "BitBox10";
            this.BitBox10.Size = new System.Drawing.Size(53, 17);
            this.BitBox10.TabIndex = 49;
            this.BitBox10.Text = "Bit 10";
            this.BitBox10.UseVisualStyleBackColor = true;
            // 
            // BitBox9
            // 
            this.BitBox9.AutoSize = true;
            this.BitBox9.Location = new System.Drawing.Point(219, 293);
            this.BitBox9.Name = "BitBox9";
            this.BitBox9.Size = new System.Drawing.Size(47, 17);
            this.BitBox9.TabIndex = 48;
            this.BitBox9.Text = "Bit 9";
            this.BitBox9.UseVisualStyleBackColor = true;
            // 
            // BitBox8
            // 
            this.BitBox8.AutoSize = true;
            this.BitBox8.Location = new System.Drawing.Point(150, 293);
            this.BitBox8.Name = "BitBox8";
            this.BitBox8.Size = new System.Drawing.Size(47, 17);
            this.BitBox8.TabIndex = 47;
            this.BitBox8.Text = "Bit 8";
            this.BitBox8.UseVisualStyleBackColor = true;
            // 
            // BitBox7
            // 
            this.BitBox7.AutoSize = true;
            this.BitBox7.Location = new System.Drawing.Point(81, 293);
            this.BitBox7.Name = "BitBox7";
            this.BitBox7.Size = new System.Drawing.Size(47, 17);
            this.BitBox7.TabIndex = 46;
            this.BitBox7.Text = "Bit 7";
            this.BitBox7.UseVisualStyleBackColor = true;
            // 
            // BitBox6
            // 
            this.BitBox6.AutoSize = true;
            this.BitBox6.Location = new System.Drawing.Point(12, 293);
            this.BitBox6.Name = "BitBox6";
            this.BitBox6.Size = new System.Drawing.Size(47, 17);
            this.BitBox6.TabIndex = 45;
            this.BitBox6.Text = "Bit 6";
            this.BitBox6.UseVisualStyleBackColor = true;
            // 
            // ListBox
            // 
            this.ListBox.FormattingEnabled = true;
            this.ListBox.Location = new System.Drawing.Point(12, 31);
            this.ListBox.Name = "ListBox";
            this.ListBox.Size = new System.Drawing.Size(329, 225);
            this.ListBox.TabIndex = 50;
            this.ListBox.Visible = false;
            this.ListBox.SelectedIndexChanged += new System.EventHandler(this.ListBox_SelectedIndexChanged);
            // 
            // EventNameInput
            // 
            this.EventNameInput.Location = new System.Drawing.Point(12, 8);
            this.EventNameInput.Name = "EventNameInput";
            this.EventNameInput.Size = new System.Drawing.Size(127, 20);
            this.EventNameInput.TabIndex = 0;
            this.EventNameInput.TextChanged += new System.EventHandler(this.EventNameInput_TextChanged);
            // 
            // EventForm
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(474, 322);
            this.Controls.Add(this.EventNameInput);
            this.Controls.Add(this.ListBox);
            this.Controls.Add(this.BitBox10);
            this.Controls.Add(this.BitBox9);
            this.Controls.Add(this.BitBox8);
            this.Controls.Add(this.BitBox7);
            this.Controls.Add(this.BitBox6);
            this.Controls.Add(this.BitBox5);
            this.Controls.Add(this.BitBox4);
            this.Controls.Add(this.BitBox3);
            this.Controls.Add(this.BitBox2);
            this.Controls.Add(this.BitBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.Bitfield);
            this.Controls.Add(this.ModeLabel);
            this.Controls.Add(this.ModeSelect);
            this.Controls.Add(this.Generator);
            this.Controls.Add(this.Illuminate);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.Tree);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EventForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set active event";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EventForm_FormClosing);
            this.Load += new System.EventHandler(this.EventForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView Tree;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox Illuminate;
        private System.Windows.Forms.CheckBox Generator;
        internal System.Windows.Forms.ComboBox ModeSelect;
        private System.Windows.Forms.Label ModeLabel;
        private System.Windows.Forms.Label Bitfield;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox BitBox1;
        private System.Windows.Forms.CheckBox BitBox2;
        private System.Windows.Forms.CheckBox BitBox3;
        private System.Windows.Forms.CheckBox BitBox4;
        private System.Windows.Forms.CheckBox BitBox5;
        private System.Windows.Forms.CheckBox BitBox10;
        private System.Windows.Forms.CheckBox BitBox9;
        private System.Windows.Forms.CheckBox BitBox8;
        private System.Windows.Forms.CheckBox BitBox7;
        private System.Windows.Forms.CheckBox BitBox6;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox ListBox;
        private System.Windows.Forms.TextBox EventNameInput;
    }
}
