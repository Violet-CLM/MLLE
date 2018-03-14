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
        bool result = false;
        internal bool ShowForm(ListViewItem newRecord, string tileDirectory)
        {
            Record = newRecord;
            var allImages = (Directory.GetFiles(tileDirectory, "*.pcx").Concat(Directory.GetFiles(tileDirectory, "*.bmp"))).Select(val => Path.GetFileName(val)).ToArray();
            boxImage.Items.AddRange(allImages);
            boxMask.Items.AddRange(allImages);
            if (!string.IsNullOrWhiteSpace(boxFilename.Text = newRecord.SubItems[1].Text))
            {
                boxName.Text = newRecord.Text;
                boxFilename.Enabled = false;
                boxImage.SelectedIndex = boxImage.FindStringExact(newRecord.SubItems[2].Text);
                boxMask.SelectedIndex = boxMask.FindStringExact(newRecord.SubItems[3].Text);
            }
            ShowDialog();
            return result;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Record.Text = boxName.Text;
            Record.SubItems[1].Text = boxFilename.Text;
            Record.SubItems[2].Text = boxImage.SelectedItem.ToString();
            Record.SubItems[3].Text = (boxMask.SelectedIndex > 0) ? boxMask.SelectedItem.ToString() : Record.SubItems[2].Text;
            result = true;
            Close();
        }
    }
}
