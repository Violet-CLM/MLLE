partial class LayerPropertiesForm
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
            this.OKButton = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.LayerSelect = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.YLabel = new System.Windows.Forms.Label();
            this.YSpeed = new System.Windows.Forms.TextBox();
            this.AutoYLabel = new System.Windows.Forms.Label();
            this.AutoYSpeed = new System.Windows.Forms.TextBox();
            this.XLabel = new System.Windows.Forms.Label();
            this.XSpeed = new System.Windows.Forms.TextBox();
            this.AutoXSpeed = new System.Windows.Forms.TextBox();
            this.AutoXLabel = new System.Windows.Forms.Label();
            this.WidthLabel = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.WidthBox = new System.Windows.Forms.NumericUpDown();
            this.HeightBox = new System.Windows.Forms.NumericUpDown();
            this.LimitVisibleRegion = new System.Windows.Forms.CheckBox();
            this.TileHeight = new System.Windows.Forms.CheckBox();
            this.TileWidth = new System.Windows.Forms.CheckBox();
            this.HeightLabel = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.Param1 = new System.Windows.Forms.NumericUpDown();
            this.Param2 = new System.Windows.Forms.NumericUpDown();
            this.Param3 = new System.Windows.Forms.NumericUpDown();
            this.RedLabel = new System.Windows.Forms.Label();
            this.GreenLabel = new System.Windows.Forms.Label();
            this.BlueLabel = new System.Windows.Forms.Label();
            this.TextureModeSelect = new System.Windows.Forms.ComboBox();
            this.ColorLabel = new System.Windows.Forms.Label();
            this.Stars = new System.Windows.Forms.CheckBox();
            this.ColorBox = new System.Windows.Forms.Panel();
            this.TextureMode = new System.Windows.Forms.CheckBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.Copy4 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WidthBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeightBox)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Param1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Param2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Param3)).BeginInit();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(278, 12);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(278, 41);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 1;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.LayerSelect);
            this.groupBox1.Location = new System.Drawing.Point(13, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 52);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Layer";
            // 
            // LayerSelect
            // 
            this.LayerSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LayerSelect.FormattingEnabled = true;
            this.LayerSelect.Items.AddRange(new object[] {
            "1: Foreground layer #2",
            "2: Foreground layer #1",
            "3: Sprite foreground layer",
            "4: Sprite layer",
            "5: Background layer #1",
            "6: Background layer #2",
            "7: Background layer #3",
            "8: Background layer"});
            this.LayerSelect.Location = new System.Drawing.Point(7, 20);
            this.LayerSelect.Name = "LayerSelect";
            this.LayerSelect.Size = new System.Drawing.Size(246, 21);
            this.LayerSelect.TabIndex = 0;
            this.LayerSelect.SelectedIndexChanged += new System.EventHandler(this.LayerSelect_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.YLabel);
            this.groupBox2.Controls.Add(this.YSpeed);
            this.groupBox2.Controls.Add(this.AutoYLabel);
            this.groupBox2.Controls.Add(this.AutoYSpeed);
            this.groupBox2.Controls.Add(this.XLabel);
            this.groupBox2.Controls.Add(this.XSpeed);
            this.groupBox2.Controls.Add(this.AutoXSpeed);
            this.groupBox2.Controls.Add(this.AutoXLabel);
            this.groupBox2.Location = new System.Drawing.Point(13, 70);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(259, 73);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Relative Movement";
            // 
            // YLabel
            // 
            this.YLabel.AutoSize = true;
            this.YLabel.Location = new System.Drawing.Point(6, 49);
            this.YLabel.Name = "YLabel";
            this.YLabel.Size = new System.Drawing.Size(48, 13);
            this.YLabel.TabIndex = 7;
            this.YLabel.Text = "Y-Speed";
            // 
            // YSpeed
            // 
            this.YSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.YSpeed.Location = new System.Drawing.Point(60, 46);
            this.YSpeed.Name = "YSpeed";
            this.YSpeed.Size = new System.Drawing.Size(50, 20);
            this.YSpeed.TabIndex = 6;
            this.YSpeed.Text = "0";
            // 
            // AutoYLabel
            // 
            this.AutoYLabel.AutoSize = true;
            this.AutoYLabel.Location = new System.Drawing.Point(124, 49);
            this.AutoYLabel.Name = "AutoYLabel";
            this.AutoYLabel.Size = new System.Drawing.Size(73, 13);
            this.AutoYLabel.TabIndex = 5;
            this.AutoYLabel.Text = "Auto Y-Speed";
            // 
            // AutoYSpeed
            // 
            this.AutoYSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AutoYSpeed.Location = new System.Drawing.Point(203, 46);
            this.AutoYSpeed.Name = "AutoYSpeed";
            this.AutoYSpeed.Size = new System.Drawing.Size(50, 20);
            this.AutoYSpeed.TabIndex = 4;
            this.AutoYSpeed.Text = "0";
            // 
            // XLabel
            // 
            this.XLabel.AutoSize = true;
            this.XLabel.Location = new System.Drawing.Point(6, 23);
            this.XLabel.Name = "XLabel";
            this.XLabel.Size = new System.Drawing.Size(48, 13);
            this.XLabel.TabIndex = 3;
            this.XLabel.Text = "X-Speed";
            // 
            // XSpeed
            // 
            this.XSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.XSpeed.Location = new System.Drawing.Point(60, 20);
            this.XSpeed.Name = "XSpeed";
            this.XSpeed.Size = new System.Drawing.Size(50, 20);
            this.XSpeed.TabIndex = 2;
            this.XSpeed.Text = "0";
            // 
            // AutoXSpeed
            // 
            this.AutoXSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AutoXSpeed.Location = new System.Drawing.Point(203, 20);
            this.AutoXSpeed.Name = "AutoXSpeed";
            this.AutoXSpeed.Size = new System.Drawing.Size(50, 20);
            this.AutoXSpeed.TabIndex = 0;
            this.AutoXSpeed.Text = "0";
            // 
            // AutoXLabel
            // 
            this.AutoXLabel.AutoSize = true;
            this.AutoXLabel.Location = new System.Drawing.Point(124, 23);
            this.AutoXLabel.Name = "AutoXLabel";
            this.AutoXLabel.Size = new System.Drawing.Size(73, 13);
            this.AutoXLabel.TabIndex = 1;
            this.AutoXLabel.Text = "Auto X-Speed";
            // 
            // WidthLabel
            // 
            this.WidthLabel.AutoSize = true;
            this.WidthLabel.Location = new System.Drawing.Point(6, 23);
            this.WidthLabel.Name = "WidthLabel";
            this.WidthLabel.Size = new System.Drawing.Size(35, 13);
            this.WidthLabel.TabIndex = 3;
            this.WidthLabel.Text = "Width";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.WidthBox);
            this.groupBox3.Controls.Add(this.HeightBox);
            this.groupBox3.Controls.Add(this.LimitVisibleRegion);
            this.groupBox3.Controls.Add(this.TileHeight);
            this.groupBox3.Controls.Add(this.TileWidth);
            this.groupBox3.Controls.Add(this.WidthLabel);
            this.groupBox3.Controls.Add(this.HeightLabel);
            this.groupBox3.Location = new System.Drawing.Point(13, 149);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(259, 93);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Properties";
            // 
            // WidthBox
            // 
            this.WidthBox.Location = new System.Drawing.Point(60, 19);
            this.WidthBox.Maximum = new decimal(new int[] {
            1023,
            0,
            0,
            0});
            this.WidthBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.WidthBox.Name = "WidthBox";
            this.WidthBox.Size = new System.Drawing.Size(50, 20);
            this.WidthBox.TabIndex = 8;
            this.WidthBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // HeightBox
            // 
            this.HeightBox.Location = new System.Drawing.Point(203, 20);
            this.HeightBox.Maximum = new decimal(new int[] {
            1023,
            0,
            0,
            0});
            this.HeightBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.HeightBox.Name = "HeightBox";
            this.HeightBox.Size = new System.Drawing.Size(50, 20);
            this.HeightBox.TabIndex = 7;
            this.HeightBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // LimitVisibleRegion
            // 
            this.LimitVisibleRegion.AutoSize = true;
            this.LimitVisibleRegion.Location = new System.Drawing.Point(9, 70);
            this.LimitVisibleRegion.Name = "LimitVisibleRegion";
            this.LimitVisibleRegion.Size = new System.Drawing.Size(111, 17);
            this.LimitVisibleRegion.TabIndex = 6;
            this.LimitVisibleRegion.Text = "Limit visible region";
            this.LimitVisibleRegion.UseVisualStyleBackColor = true;
            // 
            // TileHeight
            // 
            this.TileHeight.AutoSize = true;
            this.TileHeight.Location = new System.Drawing.Point(127, 49);
            this.TileHeight.Name = "TileHeight";
            this.TileHeight.Size = new System.Drawing.Size(75, 17);
            this.TileHeight.TabIndex = 5;
            this.TileHeight.Text = "Tile height";
            this.TileHeight.UseVisualStyleBackColor = true;
            this.TileHeight.CheckedChanged += new System.EventHandler(this.TileHeight_CheckedChanged);
            // 
            // TileWidth
            // 
            this.TileWidth.AutoSize = true;
            this.TileWidth.Location = new System.Drawing.Point(9, 49);
            this.TileWidth.Name = "TileWidth";
            this.TileWidth.Size = new System.Drawing.Size(71, 17);
            this.TileWidth.TabIndex = 4;
            this.TileWidth.Text = "Tile width";
            this.TileWidth.UseVisualStyleBackColor = true;
            // 
            // HeightLabel
            // 
            this.HeightLabel.AutoSize = true;
            this.HeightLabel.Location = new System.Drawing.Point(124, 23);
            this.HeightLabel.Name = "HeightLabel";
            this.HeightLabel.Size = new System.Drawing.Size(38, 13);
            this.HeightLabel.TabIndex = 1;
            this.HeightLabel.Text = "Height";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.Param1);
            this.groupBox4.Controls.Add(this.Param2);
            this.groupBox4.Controls.Add(this.Param3);
            this.groupBox4.Controls.Add(this.RedLabel);
            this.groupBox4.Controls.Add(this.GreenLabel);
            this.groupBox4.Controls.Add(this.BlueLabel);
            this.groupBox4.Controls.Add(this.TextureModeSelect);
            this.groupBox4.Controls.Add(this.ColorLabel);
            this.groupBox4.Controls.Add(this.Stars);
            this.groupBox4.Controls.Add(this.ColorBox);
            this.groupBox4.Controls.Add(this.TextureMode);
            this.groupBox4.Location = new System.Drawing.Point(13, 249);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(259, 93);
            this.groupBox4.TabIndex = 9;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "                    ";
            // 
            // Param1
            // 
            this.Param1.Location = new System.Drawing.Point(203, 41);
            this.Param1.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.Param1.Name = "Param1";
            this.Param1.Size = new System.Drawing.Size(50, 20);
            this.Param1.TabIndex = 13;
            // 
            // Param2
            // 
            this.Param2.Location = new System.Drawing.Point(86, 66);
            this.Param2.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.Param2.Name = "Param2";
            this.Param2.Size = new System.Drawing.Size(50, 20);
            this.Param2.TabIndex = 11;
            // 
            // Param3
            // 
            this.Param3.Location = new System.Drawing.Point(203, 65);
            this.Param3.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.Param3.Name = "Param3";
            this.Param3.Size = new System.Drawing.Size(50, 20);
            this.Param3.TabIndex = 9;
            // 
            // RedLabel
            // 
            this.RedLabel.AutoSize = true;
            this.RedLabel.Enabled = false;
            this.RedLabel.Location = new System.Drawing.Point(143, 43);
            this.RedLabel.Name = "RedLabel";
            this.RedLabel.Size = new System.Drawing.Size(54, 13);
            this.RedLabel.TabIndex = 13;
            this.RedLabel.Text = "Fade Red";
            // 
            // GreenLabel
            // 
            this.GreenLabel.AutoSize = true;
            this.GreenLabel.Enabled = false;
            this.GreenLabel.Location = new System.Drawing.Point(6, 68);
            this.GreenLabel.Name = "GreenLabel";
            this.GreenLabel.Size = new System.Drawing.Size(63, 13);
            this.GreenLabel.TabIndex = 11;
            this.GreenLabel.Text = "Fade Green";
            // 
            // BlueLabel
            // 
            this.BlueLabel.AutoSize = true;
            this.BlueLabel.Enabled = false;
            this.BlueLabel.Location = new System.Drawing.Point(142, 68);
            this.BlueLabel.Name = "BlueLabel";
            this.BlueLabel.Size = new System.Drawing.Size(55, 13);
            this.BlueLabel.TabIndex = 9;
            this.BlueLabel.Text = "Fade Blue";
            // 
            // TextureModeSelect
            // 
            this.TextureModeSelect.Enabled = false;
            this.TextureModeSelect.FormattingEnabled = true;
            this.TextureModeSelect.Location = new System.Drawing.Point(9, 15);
            this.TextureModeSelect.Name = "TextureModeSelect";
            this.TextureModeSelect.Size = new System.Drawing.Size(244, 21);
            this.TextureModeSelect.TabIndex = 4;
            this.TextureModeSelect.SelectedIndexChanged += new System.EventHandler(this.TextureModeSelect_SelectedIndexChanged);
            // 
            // ColorLabel
            // 
            this.ColorLabel.AutoSize = true;
            this.ColorLabel.Enabled = false;
            this.ColorLabel.Location = new System.Drawing.Point(171, 46);
            this.ColorLabel.Name = "ColorLabel";
            this.ColorLabel.Size = new System.Drawing.Size(58, 13);
            this.ColorLabel.TabIndex = 3;
            this.ColorLabel.Text = "Fade Color";
            this.ColorLabel.Visible = false;
            // 
            // Stars
            // 
            this.Stars.AutoSize = true;
            this.Stars.Enabled = false;
            this.Stars.Location = new System.Drawing.Point(9, 42);
            this.Stars.Name = "Stars";
            this.Stars.Size = new System.Drawing.Size(102, 17);
            this.Stars.TabIndex = 2;
            this.Stars.Text = "Parallaxing stars";
            this.Stars.UseVisualStyleBackColor = true;
            // 
            // ColorBox
            // 
            this.ColorBox.BackColor = System.Drawing.Color.Black;
            this.ColorBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ColorBox.Enabled = false;
            this.ColorBox.Location = new System.Drawing.Point(145, 42);
            this.ColorBox.Name = "ColorBox";
            this.ColorBox.Size = new System.Drawing.Size(20, 20);
            this.ColorBox.TabIndex = 1;
            this.ColorBox.Visible = false;
            this.ColorBox.Click += new System.EventHandler(this.panel1_Click);
            // 
            // TextureMode
            // 
            this.TextureMode.AutoSize = true;
            this.TextureMode.Location = new System.Drawing.Point(9, -1);
            this.TextureMode.Name = "TextureMode";
            this.TextureMode.Size = new System.Drawing.Size(91, 17);
            this.TextureMode.TabIndex = 0;
            this.TextureMode.Text = "Texture mode";
            this.TextureMode.UseVisualStyleBackColor = true;
            this.TextureMode.CheckedChanged += new System.EventHandler(this.TextureMode_CheckedChanged);
            // 
            // colorDialog1
            // 
            this.colorDialog1.AnyColor = true;
            // 
            // Copy4
            // 
            this.Copy4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Copy4.Location = new System.Drawing.Point(278, 319);
            this.Copy4.Name = "Copy4";
            this.Copy4.Size = new System.Drawing.Size(75, 23);
            this.Copy4.TabIndex = 10;
            this.Copy4.Text = "Copy Layer4";
            this.Copy4.UseVisualStyleBackColor = true;
            this.Copy4.Click += new System.EventHandler(this.Copy4_Click);
            // 
            // LayerPropertiesForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(365, 354);
            this.Controls.Add(this.Copy4);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LayerPropertiesForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Layer Properties";
            this.Load += new System.EventHandler(this.LayerPropertiesForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WidthBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeightBox)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Param1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Param2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Param3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox LayerSelect;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label YLabel;
        private System.Windows.Forms.TextBox YSpeed;
        private System.Windows.Forms.Label AutoYLabel;
        private System.Windows.Forms.TextBox AutoYSpeed;
        private System.Windows.Forms.Label XLabel;
        private System.Windows.Forms.TextBox XSpeed;
        private System.Windows.Forms.Label AutoXLabel;
        private System.Windows.Forms.TextBox AutoXSpeed;
        private System.Windows.Forms.Label WidthLabel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox LimitVisibleRegion;
        private System.Windows.Forms.CheckBox TileHeight;
        private System.Windows.Forms.CheckBox TileWidth;
        private System.Windows.Forms.Label HeightLabel;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Panel ColorBox;
        private System.Windows.Forms.CheckBox TextureMode;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label ColorLabel;
        private System.Windows.Forms.CheckBox Stars;
        private System.Windows.Forms.ComboBox TextureModeSelect;
        private System.Windows.Forms.Label RedLabel;
        private System.Windows.Forms.Label GreenLabel;
        private System.Windows.Forms.Label BlueLabel;
        private System.Windows.Forms.NumericUpDown WidthBox;
        private System.Windows.Forms.NumericUpDown HeightBox;
        private System.Windows.Forms.NumericUpDown Param1;
        private System.Windows.Forms.NumericUpDown Param2;
        private System.Windows.Forms.NumericUpDown Param3;
        private System.Windows.Forms.Button Copy4;
    }