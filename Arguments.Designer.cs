namespace MLLE
{
    partial class Arguments
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
            this.characters = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.difficulties = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.gamemodes = new System.Windows.Forms.ComboBox();
            this.playernumber = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.online = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // characters
            // 
            this.characters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.characters.FormattingEnabled = true;
            this.characters.Items.AddRange(new object[] {
            "Jazz",
            "Spaz",
            "Lori"});
            this.characters.Location = new System.Drawing.Point(229, 48);
            this.characters.Name = "characters";
            this.characters.Size = new System.Drawing.Size(222, 39);
            this.characters.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(44, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Character";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ButtonOK
            // 
            this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.Location = new System.Drawing.Point(17, 402);
            this.ButtonOK.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(267, 55);
            this.ButtonOK.TabIndex = 2;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(300, 402);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(209, 55);
            this.ButtonCancel.TabIndex = 3;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(59, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 32);
            this.label2.TabIndex = 5;
            this.label2.Text = "Difficulty";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // difficulties
            // 
            this.difficulties.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.difficulties.FormattingEnabled = true;
            this.difficulties.Items.AddRange(new object[] {
            "Medium",
            "Easy",
            "Hard",
            "Turbo"});
            this.difficulties.Location = new System.Drawing.Point(229, 111);
            this.difficulties.Name = "difficulties";
            this.difficulties.Size = new System.Drawing.Size(222, 39);
            this.difficulties.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 177);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(170, 32);
            this.label3.TabIndex = 7;
            this.label3.Text = "Game Mode";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // gamemodes
            // 
            this.gamemodes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gamemodes.FormattingEnabled = true;
            this.gamemodes.Items.AddRange(new object[] {
            "Single Player",
            "Cooperative",
            "Battle",
            "CTF",
            "Race",
            "Treasure Hunt",
            "DCTF",
            "Domination",
            "Flag Run",
            "Head Hunters",
            "Jailbreak",
            "Last Rabbit Standing",
            "Pestilence",
            "Roast Tag",
            "Team Battle",
            "TLRS",
            "XLRS"});
            this.gamemodes.Location = new System.Drawing.Point(229, 174);
            this.gamemodes.Name = "gamemodes";
            this.gamemodes.Size = new System.Drawing.Size(222, 39);
            this.gamemodes.TabIndex = 6;
            this.gamemodes.SelectedIndexChanged += new System.EventHandler(this.gamemodes_SelectedIndexChanged);
            // 
            // playernumber
            // 
            this.playernumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.playernumber.FormattingEnabled = true;
            this.playernumber.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4"});
            this.playernumber.Location = new System.Drawing.Point(229, 237);
            this.playernumber.Name = "playernumber";
            this.playernumber.Size = new System.Drawing.Size(222, 39);
            this.playernumber.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(73, 240);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 32);
            this.label4.TabIndex = 9;
            this.label4.Text = "Players";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // online
            // 
            this.online.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.online.FormattingEnabled = true;
            this.online.Items.AddRange(new object[] {
            "Local",
            "Unlisted Server",
            "Listed Server"});
            this.online.Location = new System.Drawing.Point(229, 302);
            this.online.Name = "online";
            this.online.Size = new System.Drawing.Size(222, 39);
            this.online.TabIndex = 10;
            this.online.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(73, 305);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(115, 32);
            this.label5.TabIndex = 11;
            this.label5.Text = "Online?";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Arguments
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(526, 473);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.online);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.playernumber);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.gamemodes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.difficulties);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.characters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Arguments";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Command Line Arguments";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox characters;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ButtonOK;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox difficulties;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox gamemodes;
        private System.Windows.Forms.ComboBox playernumber;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox online;
        private System.Windows.Forms.Label label5;
    }
}