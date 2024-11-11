using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLLE
{
    public partial class TilesetSelection : Form
    {
        private delegate void PanelDelegate(Panel p);
        private delegate void PanelFlowDelegate(Panel p, FlowLayoutPanel f);
        private delegate void PanelStringDelegate(Panel p, string f);

        private Object gridlock = new Object(), textlock = new Object();

        PaletteImage TilesetPalette = new PaletteImage(3, 0, true, false);

        public TilesetSelection()
        {
            InitializeComponent();
        }
        internal void ShowForm(Mainframe.NameAndFilename[] files, Color transparentColor)
        {
            TilesetPalette.Location = new Point(buttonCancel.Right - TilesetPalette.Width, buttonCancel.Top - TilesetPalette.Height - 10);
            TilesetPalette.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Controls.Add(TilesetPalette);

            label3.Text = ""; //hide until a tileset is chosen

            new Thread(new ThreadStart(() =>
            {
                foreach (var f in files)
                {
                    try
                    {
                        Panel panel = new Panel();
                        panel.Size = new Size(180, 180);
                        panel.BackColor = SystemColors.Control;
                        panel.Margin = new Padding(10);

                        PictureBox pictureBox1 = new PictureBox();
                        pictureBox1.Size = new Size(160, 160);
                        pictureBox1.Location = new Point(10, 10);

                        J2TFile tileset = new J2TFile(f.Filepath);
                        tileset.Palette.Colors[0] = Palette.Convert(transparentColor);

                        var image = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                        byte[] bytes = new byte[data.Height * data.Stride];
                        uint numberOfTilesToDraw = Math.Min(tileset.TotalNumberOfTiles, 100); //we're drawing a 160x160 area, which is the first 100 tiles, though not all tilesets even have 100 tiles
                        for (uint i = 0; i < numberOfTilesToDraw; ++i)
                        {
                            var tileImage = tileset.Images[tileset.ImageAddress[i]];
                            var xOrg = (i % 10) * 16;
                            var yOrg = i / 10 * 16;
                            for (uint x = 0; x < 32; x += 2) //+= 2 because this is a 0.5 size thumbnail
                                for (uint y = 0; y < 32; y += 2)
                                {
                                    bytes[xOrg + x / 2 + (yOrg + y / 2) * data.Stride] = tileImage[x + y * 32];
                                }
                        }
                        Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
                        image.UnlockBits(data);
                        tileset.Palette.Apply(image);
                        pictureBox1.Image = image;
                        f.Thumbnail = image;
                        f.CRC32 = tileset.Crc32;
                        //pictureBox1.Enabled = false;
                        string tooltipText = f.Name /*+ " (" + Path.GetFileNameWithoutExtension(f.Filename) + ")"*/;
                        toolTip1.SetToolTip(pictureBox1, tooltipText);
                        // toolTip1.SetToolTip(panel, tooltipText);
                        panel.Controls.Add(pictureBox1);
                        string filter;
                        lock (textlock)
                        {
                            filter = textBox1.Text.Trim();
                        }
                        if (filter != string.Empty)
                                panel.Visible = f.FilterText.Contains(filter.ToLowerInvariant());

                        panel.Tag = f.FilterText;
                        pictureBox1.Tag = tileset.Name + "\n" + tileset.FilenameOnly + "\n" + tileset.TileCount.ToString() + " tiles";
                        pictureBox1.MouseEnter += PictureBox_MouseEnter;
                        pictureBox1.MouseLeave += PictureBox_MouseLeave;
                        pictureBox1.MouseClick += PictureBox1_MouseClick;

                        lock (gridlock)
                        {
                            Invoke(new PanelFlowDelegate(delegate(Panel p, FlowLayoutPanel l) { l.Controls.Add(p); }), new object[] { panel, flowLayoutPanel1 });
                        }
                    } catch {
                        //just skip this one lol
                    }

                    //if (++fileID == 12) //debug
                    //    break;
                }
            })).Start();

            ShowDialog();
        }

        Panel SelectedPanel;
        PictureBox SelectedPictureBox;

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (SelectedPanel != null)
                SelectedPanel.BackColor = SystemColors.Control; //deselect
            SelectedPictureBox = sender as PictureBox;
            SelectedPanel = SelectedPictureBox.Parent as Panel;
            label3.Text = SelectedPictureBox.Tag as string;
            TilesetPalette.Palette = new Palette(SelectedPictureBox.Image.Palette);
            TilesetPalette.Update(PaletteImage.AllPaletteColors);
        }

        private void PictureBox_MouseLeave(object sender, EventArgs e)
        {
            Panel panel = (sender as Control).Parent as Panel;
            panel.BackColor = panel == SelectedPanel ? SystemColors.ControlDark : SystemColors.Control;
        }
        private static void PictureBox_MouseEnter(object sender, EventArgs e)
        {
            (sender as Control).Parent.BackColor = SystemColors.Highlight;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string filter;
            lock (textlock)
            {
                filter = textBox1.Text.Trim();
            }
            if (filter == string.Empty)
            {/*
                lock (gridlock)
                {
                    Parallel.ForEach<Control>(flowLayoutPanel1.Controls.Cast<Panel>(), panel =>
                    // foreach (var panel in flowLayoutPanel1.Controls)
                    {
                        Invoke(new PanelDelegate(delegate (Panel p) { p.Visible = true; }), new object[] { panel });
                        //(panel as Control).Visible = ((panel as Control).Tag as string).Contains(filter);
                    });
                    //foreach (var panel in flowLayoutPanel1.Controls)
                      // (panel as Control).Visible = true;
                }*/
            }
            else
            {
                filter = filter.ToLowerInvariant();
                if (SelectedPanel != null && !(SelectedPanel.Tag as string).Contains(filter)) //the selected tileset was filtered out of the search results
                {
                    SelectedPanel.BackColor = SystemColors.Control;
                    SelectedPanel = null;
                    SelectedPictureBox = null;
                    TilesetPalette.Image = null;
                    label3.ResetText();
                }
                lock (gridlock)
                {
                    flowLayoutPanel1.SuspendLayout();
                    //Parallel.ForEach(flowLayoutPanel1.Controls.Cast<Panel>(), (panel) =>
                    foreach (Panel panel in flowLayoutPanel1.Controls)
                    {
                        //string f = filter;
                        //Invoke(new PanelDelegate(delegate(Panel p){ p.Visible = (p.Tag as string).Contains(filter); }), new object[] { panel });
                        panel.Visible = (panel.Tag as string).Contains(filter);
                    };
                    flowLayoutPanel1.ResumeLayout();
                }
            }
        }
    }
}
