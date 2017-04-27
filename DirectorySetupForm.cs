using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Ini;

namespace MLLE
{
    public partial class DirectorySetupForm : Form
    {
        private IniFile Settings;
        private static bool result;
        private TextBox[] TextBoxes;
        public DirectorySetupForm(IniFile settings)
        {
            InitializeComponent();
            Settings = settings;
            TextBoxes = new TextBox[] { DirectoryJJ2, DirectoryTSF, Directory110o, Directory100g, DirectoryBC, DirectoryAGA };
            Button[] Buttons = new Button[] { button0, button1, button2, button3, button4, button5 };
            for (uint i = 0; i < TextBoxes.Length; ++i)
            {
                Buttons[i].Tag = TextBoxes[i];
                Buttons[i].Click += browseButton_Click;
                TextBoxes[i].Text = Settings.IniReadValue("Paths", (string)TextBoxes[i].Tag) ?? "";
            }
        }
        public static bool ShowForm(IniFile settings)
        {
            DirectorySetupForm DSF = new DirectorySetupForm(settings);
            DSF.ShowDialog();
            return result;
        }

        private void BrowseDirectory(TextBox box)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                box.Text = folderBrowserDialog1.SelectedPath;
        }

        private void browseButton_Click(object sender, EventArgs e) { BrowseDirectory((TextBox)((Button)sender).Tag); }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            bool atLeastOneBoxWasFilled = false;
            foreach (TextBox box in TextBoxes)
            {
                if (box.Text.Length > 0) //otherwise skip over this one, the user doesn't have the game
                {
                    if (Directory.Exists(box.Text))
                    {
                        atLeastOneBoxWasFilled = true;
                        Settings.IniWriteValue("Paths", (string)box.Tag, box.Text);
                        if ((Settings.IniReadValue("Miscellaneous", "DefaultGame") ?? "") == "") Settings.IniWriteValue("Miscellaneous", "DefaultGame", (string)box.Tag);
                    }
                    else
                    {
                        MessageBox.Show(box.Text + " is not a valid directory! Please leave this textbox empty if you do not have this game.", "Invalid Directory", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                }
            }
            result = atLeastOneBoxWasFilled;
            Dispose();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            result = false;
            Dispose();
        }
    }
}
