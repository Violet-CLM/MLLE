using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MLLE
{
    public partial class LevelProperties : Form
    {
        Mainframe SourceForm;
        int NumberInFilename;
        int ResultNumber;
        string filename;
        public bool MusicChanged = false;
        bool DataLoaded = false;
        public LevelProperties(Mainframe parent)
        {
            SourceForm = parent;
            InitializeComponent();
        }

        private void LevelProperties_Load(object sender, EventArgs e)
        {
            #region visibility
            int heightdiff = (NextLevel.Location.Y - LevelName.Location.Y);
            label4.Text = SourceForm.EnableableStrings[SourceForm.J2L.VersionType][EnableableTitles.SecretLevelName];
            if (label4.Text == "")
            {
                label4.Visible = SecretLevel.Visible = BrowseSecret.Visible = false;
                foreach (Control foo in new Control[] { label5, BonusLevel, label8, Arguments, argumentsGenerate, IsMultiplayer, HideHCL, groupBox2, groupBox3 }) foo.Location = new Point(foo.Location.X, foo.Location.Y - heightdiff);
                groupBox1.Height -= heightdiff;
                Height -= heightdiff;
            }
            label5.Text = SourceForm.EnableableStrings[SourceForm.J2L.VersionType][EnableableTitles.BonusLevelName];
            if (label5.Text == "")
            {
                label5.Visible = BonusLevel.Visible = false;
                foreach (Control foo in new Control[] { label8, Arguments, argumentsGenerate, IsMultiplayer, HideHCL, groupBox2, groupBox3 }) foo.Location = new Point(foo.Location.X, foo.Location.Y - heightdiff);
                groupBox1.Height -= heightdiff;
                Height -= heightdiff;
            }
            if (!SourceForm.EnableableBools[SourceForm.J2L.VersionType][EnableableTitles.BoolDevelopingForPlus])
            {
                label8.Visible = Arguments.Visible = argumentsGenerate.Visible = false;
                foreach (Control foo in new Control[] { IsMultiplayer, HideHCL, groupBox2, groupBox3 }) foo.Location = new Point(foo.Location.X, foo.Location.Y - heightdiff);
                groupBox1.Height -= heightdiff;
                Height -= heightdiff;
            }
            IsMultiplayer.Text = SourceForm.EnableableStrings[SourceForm.J2L.VersionType][EnableableTitles.Multiplayer];
            if (IsMultiplayer.Text == "") { IsMultiplayer.Visible = false; }
            HideHCL.Text = SourceForm.EnableableStrings[SourceForm.J2L.VersionType][EnableableTitles.HideInHCL];
            if (HideHCL.Text == "")
            {
                HideHCL.Visible = false;
                if (IsMultiplayer.Text == "")
                {
                    //heightdiff = (IsMultiplayer.Location.Y - Arguments.Location.Y);
                    foreach (Control foo in new Control[] { groupBox2, groupBox3 }) foo.Location = new Point(foo.Location.X, foo.Location.Y - heightdiff);
                    groupBox1.Height -= heightdiff;
                    Height -= heightdiff;
                }
                else
                {
                    IsMultiplayer.Location = HideHCL.Location;
                }
            }
            groupBox3.Text = SourceForm.EnableableStrings[SourceForm.J2L.VersionType][EnableableTitles.Splitscreen];
            if (groupBox3.Text == "") { groupBox3.Visible = false; }
            groupBox2.Text = SourceForm.EnableableStrings[SourceForm.J2L.VersionType][EnableableTitles.Lighting];
            if (groupBox2.Text == "")
            {
                groupBox2.Visible = false;
                if (groupBox3.Text == "")
                {
                    Height -= (groupBox2.Height + groupBox2.Top - groupBox1.Bottom);
                }
                else
                {
                    groupBox3.Location = groupBox2.Location;
                }
            }
            #endregion visibility
            #region values
            LevelName.Text = SourceForm.J2L.Name;
            NextLevel.Text = SourceForm.J2L.NextLevel;
            filename = Path.GetFileNameWithoutExtension(SourceForm.J2L.FilenameOnly);
            int CleverLength = TryToBeClever(filename, 0, 1);
            if (CleverLength > 0) NextLevel.Items.Insert(0, filename.Substring(0, filename.Length - CleverLength) + (ResultNumber + 1).ToString());
            SecretLevel.Text = SourceForm.J2L.SecretLevel;
            BonusLevel.Text = SourceForm.J2L.BonusLevel;
            MusicFile.Text = SourceForm.J2L.Music;
            IsMultiplayer.Checked = SourceForm.J2L.LevelMode > 0;
            HideHCL.Checked = SourceForm.J2L.IsHiddenInHCL;
            StartLight.Value = (int)(SourceForm.J2L.StartLight * 1.5625);
            MinLight.Value = (int)(SourceForm.J2L.MinLight * 1.5625);
            if (SourceForm.J2L.UsesVerticalSplitscreen == true) radioButton2.Checked = true; else radioButton1.Checked = true;
            Arguments.Text = SourceForm.J2L.PlusPropertyList.CommandLineArguments;
            DataLoaded = true;
            #endregion values
        }

        internal int TryToBeClever(string filename, int oldNumber, int numberOfChars)
        {
            if (Int32.TryParse(filename.Substring(filename.Length - numberOfChars, numberOfChars), out NumberInFilename)) return TryToBeClever(filename, NumberInFilename, numberOfChars + 1);
            else { ResultNumber = oldNumber; return numberOfChars - 1; }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            SourceForm.J2L.Name = LevelName.Text;
            SourceForm.J2L.NextLevel = NextLevel.Text;
            SourceForm.J2L.SecretLevel = SecretLevel.Text;
            SourceForm.J2L.BonusLevel = BonusLevel.Text;
            SourceForm.J2L.Music = MusicFile.Text;
            SourceForm.J2L.LevelMode = IsMultiplayer.Checked ? (byte)1 : (byte)0;
            SourceForm.J2L.IsHiddenInHCL = HideHCL.Checked;
            SourceForm.J2L.StartLight = (byte)Math.Ceiling(StartLight.Value / (decimal)1.5625);
            SourceForm.J2L.MinLight = (byte)Math.Ceiling(MinLight.Value / (decimal)1.5625);
            SourceForm.J2L.UsesVerticalSplitscreen = radioButton2.Checked;
            SourceForm.J2L.PlusPropertyList.CommandLineArguments = Arguments.Text;
            SourceForm.LevelHasBeenModified = true;
            Dispose();
        }

        private void BrowseNext_Click(object sender, EventArgs e)
        {
            DialogResult result = SourceForm.OpenJ2LDialog.ShowDialog();
            if (result == DialogResult.OK) NextLevel.Text = Path.GetFileNameWithoutExtension(SourceForm.OpenJ2LDialog.FileName);
        }

        private void BrowseMusic_Click(object sender, EventArgs e)
        {
            DialogResult result = OpenMusicDialog.ShowDialog();
            if (result == DialogResult.OK) MusicFile.Text = (Path.GetExtension(OpenMusicDialog.FileName) == "j2b") ? Path.GetFileNameWithoutExtension(OpenMusicDialog.FileName) : Path.GetFileName(OpenMusicDialog.FileName);
        }

        private void BrowseSecret_Click(object sender, EventArgs e)
        {
            DialogResult result = SourceForm.OpenJ2LDialog.ShowDialog();
            if (result == DialogResult.OK) SecretLevel.Text = Path.GetFileNameWithoutExtension(SourceForm.OpenJ2LDialog.FileName);
        }

        private void MusicFile_TextChanged(object sender, EventArgs e)
        {
            if (DataLoaded) MusicChanged = true;
        }

        private void argumentsGenerate_Click(object sender, EventArgs e)
        {
            new Arguments(Arguments).ShowDialog();
        }
    }
}
