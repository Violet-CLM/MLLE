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
        internal bool ShowForm(ListViewItem newRecord, string tileDirectory, string fileExtension)
        {
            Record = newRecord;
            FileExtension = fileExtension;
            var allImages = (Directory.GetFiles(tileDirectory, "*.png").Concat(Directory.GetFiles(tileDirectory, "*.gif")).Concat(Directory.GetFiles(tileDirectory, "*.tif")).Concat(Directory.GetFiles(tileDirectory, "*.tiff")).Concat(Directory.GetFiles(tileDirectory, "*.bmp"))).Select(val => Path.GetFileName(val)).ToArray();
            boxImage.Items.AddRange(allImages);
            boxMask.Items.AddRange(allImages);
            boxMask.SelectedIndex = 0;
            if (!string.IsNullOrWhiteSpace(boxFilename.Text = newRecord.SubItems[1].Text))
            {
                boxName.Text = newRecord.Text;
                boxFilename.Enabled = false;
                boxImage.SelectedIndex = boxImage.FindStringExact(newRecord.SubItems[2].Text);
                boxMask.SelectedIndex = boxMask.FindStringExact(newRecord.SubItems[3].Text);
            }
            filenameManuallyChanged = !boxFilename.Enabled;
            ShowDialog();
            return result;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if (boxImage.SelectedItem != null && boxName.Text.Trim() != string.Empty && boxFilename.Text.Trim() != string.Empty)
            {
                Record.Text = boxName.Text;
                Record.SubItems[1].Text = Path.ChangeExtension(boxFilename.Text, FileExtension);
                Record.SubItems[2].Text = boxImage.SelectedItem.ToString();
                Record.SubItems[3].Text = (boxMask.SelectedIndex > 0) ? boxMask.SelectedItem.ToString() : Record.SubItems[2].Text;
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
