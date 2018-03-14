using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ini;

namespace MLLE
{
    public partial class TileSetOrganizer : Form
    {
        public TileSetOrganizer()
        {
            InitializeComponent();
        }
        List<ListViewItem> AllTilesets = new List<ListViewItem>();
        IniFile Settings;
        string VersionString;
        string TileDirectory;
        internal void ShowForm(IniFile settings, string versionString, string tileDirectory)
        {
            Settings = settings;
            VersionString = versionString;
            TileDirectory = tileDirectory;
            int iniTilesetIndex = 1;
            listView1.Columns[listView1.Columns.Count - 1].Width = -2;
            while (true)
            {
                string iniText = Settings.IniReadValue("Tilesets", (iniTilesetIndex++).ToString());
                if (string.IsNullOrWhiteSpace(iniText)) //run out of subsequently numbered ini lines
                    break;
                var match = System.Text.RegularExpressions.Regex.Match(iniText, "\\s*\"([^\"]+)\",\\s*\"([^\"]+)\",\\s*\"([^\"]+)\",\\s*\"([^\"]+)\",\\s*(TSF|JJ2|BC\\AGA\\GorH)\\s*");
                if (!match.Success) //something went wrong
                    continue; //oh well!
                ListViewItem newRecord = new ListViewItem(match.Groups[2].Value);
                newRecord.SubItems.Add(match.Groups[2].Value);
                newRecord.SubItems.Add(match.Groups[4].Value);
                newRecord.SubItems.Add(match.Groups[3].Value);
                newRecord.Tag = match.Groups[5].Value;
                ThinkAboutAdding(newRecord);

            }
            ShowDialog();
        }

        void SaveList()
        {
            int iniTilesetIndex = 0;
            while (true)
            {
                string keyName = (iniTilesetIndex + 1).ToString();
                if (iniTilesetIndex < AllTilesets.Count)
                {
                    ListViewItem record = AllTilesets[iniTilesetIndex];
                    Settings.IniWriteValue("Tilesets", keyName, "\"" + record.SubItems[1].Text + "\", \"" + record.Text + "\", \"" + record.SubItems[3].Text + "\", \"" + record.SubItems[2].Text + "\", " + (record.Tag as string));
                }
                else if (!string.IsNullOrWhiteSpace(Settings.IniReadValue("Tilesets", keyName))) //leftover line number that doesn't need to exist anymore after the list got shorter
                    Settings.IniWriteValue("Tilesets", keyName, null); //delete it
                else
                    break;
                ++iniTilesetIndex;
            }
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            var newRecord = new ListViewItem();
            for (int i = 0; i < 3; ++i)
                newRecord.SubItems.Add(new ListViewItem.ListViewSubItem());
            newRecord.Tag = VersionString;
            Select(newRecord);
        }

        private bool ThinkAboutAdding(ListViewItem newRecord)
        {
            if (!AllTilesets.Contains(newRecord))
            {
                AllTilesets.Add(newRecord); //regardless of version, because we'll be saving these later
                if ((newRecord.Tag as string) == VersionString) //otherwise it's a tileset destined for a different game
                    listView1.Items.Add(newRecord);
                return true;
            }
            return false;
        }

        private void Select(ListViewItem newRecord)
        {
            if (new SelectTileSet().ShowForm(newRecord, TileDirectory))
            {
                ThinkAboutAdding(newRecord);
                SaveList();
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
                Select(listView1.SelectedItems[0]);
        }
    }
}
