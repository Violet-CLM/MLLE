﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Ini;

namespace MLLE
{
    public partial class TileSetOrganizer : Form
    {
        public TileSetOrganizer()
        {
            InitializeComponent();
        }
        List<ListViewItem> AllTilesets;
        IniFile Settings;
        Version VersionType;
        string VersionString;
        string TileDirectory;
        bool VersionIsPlusCompatible;
        internal void ShowForm(IniFile settings, Version versionType, string tileDirectory, bool versionIsPlusCompatible)
        {
            Settings = settings;
            VersionType = versionType;
            VersionString = VersionType.ToString();
            TileDirectory = tileDirectory;
            if (!Directory.Exists(tileDirectory))
                Directory.CreateDirectory(tileDirectory);
            VersionIsPlusCompatible = versionIsPlusCompatible;
            if (!VersionIsPlusCompatible)
            {
                Size = MinimumSize = new Size(MinimumSize.Width - sourceImage32.Width, MinimumSize.Height);
                sourceImage32.Width = 0;
            }
            listView1.Columns[listView1.Columns.Count - 1].Width = -2;
            RefreshList();
            ShowDialog();
        }
        private void RefreshList() {
            AllTilesets = new List<ListViewItem>();
            listView1.Items.Clear();
            int iniTilesetIndex = 1;
            while (true)
            {
                string iniText = Settings.IniReadValue("Tilesets", (iniTilesetIndex++).ToString());
                if (string.IsNullOrWhiteSpace(iniText)) //run out of subsequently numbered ini lines
                    break;
                var match = System.Text.RegularExpressions.Regex.Match(iniText, "\\s*\"([^\"]+)\",\\s*\"([^\"]+)\",\\s*\"([^\"]*)\",\\s*\"([^\"]*)\",(?:\\s*\"([^\"]+)\",)?\\s*(TSF|JJ2|BC|AGA|GorH)\\s*");
                if (!match.Success) //something went wrong
                    continue; //oh well!
                ListViewItem newRecord = new ListViewItem(match.Groups[2].Value);
                newRecord.SubItems.Add(match.Groups[1].Value);
                newRecord.SubItems.Add(match.Groups[4].Value);
                newRecord.SubItems.Add(match.Groups[5].Value);
                newRecord.SubItems.Add(match.Groups[3].Value);
                newRecord.Tag = match.Groups[6].Value;
                ThinkAboutAdding(newRecord);
            }
        }

        void SaveList()
        {
            int iniTilesetIndex = 0;
            AllTilesets.Sort((x,y) => x.SubItems[1].Text.CompareTo(y.SubItems[1].Text));
            while (true)
            {
                string keyName = (iniTilesetIndex + 1).ToString();
                if (iniTilesetIndex < AllTilesets.Count)
                {
                    ListViewItem record = AllTilesets[iniTilesetIndex];
                    Settings.IniWriteValue("Tilesets", keyName, "\"" + record.SubItems[1].Text + "\", \"" + record.Text + "\", \"" + record.SubItems[4].Text + "\", \"" + record.SubItems[2].Text + "\", " + (record.SubItems[3].Text.Length > 0 ? ("\"" + record.SubItems[3].Text + "\", ") : "") + (record.Tag as string));
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
            for (int i = 0; i < 4; ++i)
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
            if (new SelectTileSet().ShowForm(newRecord, TileDirectory, VersionType != Version.AGA ? ".j2t" : ".til", VersionIsPlusCompatible))
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

        private void Remove_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1 && MessageBox.Show("This will remove MLLE's record of which image files you used to make this tileset. The tileset file will still exist, but you will not be able to rebuild it. Do you wish to continue?", "Remove Tileset Listing", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                AllTilesets.Remove(listView1.SelectedItems[0]);
                listView1.Items.Remove(listView1.SelectedItems[0]);
                SaveList();
            }
        }

        private void Build_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                var record = listView1.SelectedItems[0];
                var J2T = new J2TFile();
                J2T.VersionType = VersionType;
                Bitmap image = null, image32 = null, mask = null;
                string sourceFilepath = "";
                try
                {
                    string sourceFilename = record.SubItems[2].Text;
                    if (sourceFilename.Length > 0)
                        image = (Bitmap)Bitmap.FromFile(sourceFilepath = Path.Combine(TileDirectory, sourceFilename));
                    sourceFilename = record.SubItems[3].Text;
                    if (sourceFilename.Length > 0)
                        image32 = (Bitmap)Bitmap.FromFile(sourceFilepath = Path.Combine(TileDirectory, sourceFilename));
                    sourceFilename = record.SubItems[4].Text;
                    if (sourceFilename.Length > 0)
                        mask = (Bitmap)Bitmap.FromFile(sourceFilepath = Path.Combine(TileDirectory, sourceFilename));

                    Palette backupPalette = null;
                    Build:
                    switch (J2T.Build(image, image32, mask, record.Text, VersionIsPlusCompatible, backupPalette))
                    {
                        case BuildResults.DifferentDimensions:
                            if (image32 != null)
                                MessageBox.Show(String.Format("The images and the mask must be the same dimensions. Your images are {0} by {1} and {2} by {3}, and your mask is {4} by {5}.", image.Width, image.Height, image32.Width, image32.Height, mask.Width, mask.Height), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            else
                                MessageBox.Show(String.Format("The image and the mask must be the same dimensions. Your image is {0} by {1}, and your mask is {2} by {3}.", (image ?? image32).Width, (image ?? image32).Height, mask.Width, mask.Height), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case BuildResults.BadDimensions:
                            MessageBox.Show(String.Format("A tileset image must be 320 pixels wide and a multiple of 32 pixels high. Your image is {0} by {1}.", (image ?? image32).Width, (image ?? image32).Height), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case BuildResults.ImageWrongFormat:
                            MessageBox.Show("Your image file is saved in an incorrect color mode instead of 8-bit color with no transparency (color 0 is used for transparency instead).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case BuildResults.Image32WrongFormat:
                            MessageBox.Show("Your 32-bit image file is saved in an incorrect color mode instead of 24-bit or 32-bit color.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case BuildResults.MaskWrongFormat:
                            MessageBox.Show("Your mask file is saved in an incorrect color mode. A tileset mask must use indexed color with no transparency (color 0 is used for transparency instead).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case BuildResults.TooBigForVersion:
                            MessageBox.Show(String.Format("Your tileset images are too big. The tile limit for a {0} tileset is {1} tiles, but your tileset contains {2}.", J2File.FullVersionNames[J2T.VersionType], J2T.MaxTiles, ((image ?? image32).Height / 32 * 10)), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case BuildResults.MaskNeedsPaletteFor32BitImages:
                            if (MessageBox.Show("None of your images define an 8-bit palette. Use a generic Jazz-2-styled 8-bit palette instead?", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                            {
                                using (BinaryReader binreader = new BinaryReader(new MemoryStream(MLLE.Properties.Resources.GenericPalette), J2File.FileEncoding))
                                {
                                    binreader.BaseStream.Seek(4, SeekOrigin.Begin); //"Color Table" header
                                    backupPalette = new Palette(binreader);
                                }
                                goto Build;
                            }
                            break;
                        case BuildResults.Success:
                            string fullFilePath = Path.Combine(Directory.GetParent(TileDirectory).ToString(), record.SubItems[1].Text);
                            if (J2T.Save(fullFilePath) != SavingResults.Success)
                                MessageBox.Show("Something went wrong.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            else
                                MessageBox.Show(String.Format("OK: Your tileset '{0}' has been built at {1} with {2} tiles ({3} Kb).", record.Text, fullFilePath, ((image ?? image32).Height / 32 * 10), new System.IO.FileInfo(fullFilePath).Length / 1024), "Successful Build", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            MessageBox.Show("Something went wrong.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show(sourceFilepath + " not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch
                {
                    MessageBox.Show(sourceFilepath + " does not use an image format supported by this program. (Try PNG, GIF, TIFF, or BMP.)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (image != null)
                        image.Dispose();
                    if (image32 != null)
                        image32.Dispose();
                    if (mask != null)
                        mask.Dispose();
                }
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            RefreshList();
        }
    }
}
