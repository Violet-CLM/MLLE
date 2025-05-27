using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLLE
{
    public partial class SelectTileSet : Form
    {
        public SelectTileSet()
        {
            InitializeComponent();
        }

        ListViewItem Record;
        string FileExtension;
        bool result = false;
        internal bool ShowForm(ListViewItem newRecord, string tileDirectory, string fileExtension, bool VersionIsPlusCompatible)
        {
            Record = newRecord;
            FileExtension = fileExtension;
            var allImages = (Directory.GetFiles(tileDirectory, "*.png").Concat(Directory.GetFiles(tileDirectory, "*.gif")).Concat(Directory.GetFiles(tileDirectory, "*.tif")).Concat(Directory.GetFiles(tileDirectory, "*.tiff")).Concat(Directory.GetFiles(tileDirectory, "*.bmp"))).Select(val => Path.GetFileName(val)).ToArray();
            boxImage.Items.AddRange(allImages);
            box32.Items.AddRange(allImages);
            boxMask.Items.AddRange(allImages);
            boxImage.SelectedIndex = 0;
            box32.SelectedIndex = 0;
            boxMask.SelectedIndex = 0;
            if (!string.IsNullOrWhiteSpace(boxFilename.Text = newRecord.SubItems[1].Text))
            {
                boxName.Text = newRecord.Text;
                boxFilename.Enabled = false;
                boxImage.SelectedIndex = boxImage.FindStringExact(newRecord.SubItems[2].Text);
                boxMask.SelectedIndex = boxMask.FindStringExact(newRecord.SubItems[4].Text);
                box32.SelectedIndex = boxMask.FindStringExact(newRecord.SubItems[3].Text);
            }
            filenameManuallyChanged = !boxFilename.Enabled;
            if (!VersionIsPlusCompatible)
            {
                label5.Enabled = box32.Enabled = false;
            }
            ShowDialog();
            return result;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if ((boxImage.SelectedIndex > 0 || (box32.SelectedIndex > 0 && boxMask.SelectedIndex > 0)) && boxName.Text.Trim() != string.Empty && boxFilename.Text.Trim() != string.Empty)
            {
                Record.Text = boxName.Text;
                Record.SubItems[1].Text = Path.ChangeExtension(boxFilename.Text, FileExtension);
                Record.SubItems[2].Text = (boxImage.SelectedIndex > 0) ? boxImage.SelectedItem.ToString() : "";
                Record.SubItems[3].Text = (box32.SelectedIndex > 0) ? box32.SelectedItem.ToString() : "";
                Record.SubItems[4].Text = (boxMask.SelectedIndex > 0) ? boxMask.SelectedItem.ToString() : Record.SubItems[2].Text;
                result = true;
                Close();
            }
            else
                MessageBox.Show("Please fill out all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void boxName_TextChanged(object sender, EventArgs e)
        {
            if (boxName.Focused && !filenameManuallyChanged)
            {
                boxFilename.Text = boxName.Text + FileExtension;
            }
        }

        bool filenameManuallyChanged;
        private void boxFilename_TextChanged(object sender, EventArgs e)
        {
            if (boxFilename.Focused)
              filenameManuallyChanged = true;
        }
    }
}
