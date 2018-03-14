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
        internal void ShowForm(IniFile settings, string versionString, string tileDirectory)
        {
            Settings = settings;
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
                for (int i = 2; i <= 4; ++i)
                    newRecord.SubItems.Add(match.Groups[i==2?1:i].Value);
                newRecord.Tag = match.Groups[5].Value;
                AllTilesets.Add(newRecord); //regardless of version, because we'll be saving these later
                if (match.Groups[5].Value == versionString) //otherwise it's a tileset destined for a different game
                    listView1.Items.Add(newRecord);

            }
            ShowDialog();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
