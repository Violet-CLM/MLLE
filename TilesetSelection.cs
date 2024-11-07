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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLLE
{
    public partial class TilesetSelection : Form
    {
        PaletteImage TilesetPalette = new PaletteImage(3, 0, true, false);

        public TilesetSelection()
        {
            InitializeComponent();
        }
        internal void ShowForm(Mainframe.NameAndFilename[] files, Color transparentColor)
        {
            int fileID = 0;
            foreach (var f in files)
            {
                Panel panel = new Panel();
                panel.Size = new Size(180, 180);
                panel.BackColor = SystemColors.Control;
                panel.Margin = new Padding(10);

                PictureBox pictureBox1 = new PictureBox();
                pictureBox1.Size = new Size(160, 160);
                pictureBox1.Location = new Point(10, 10);

                J2TFile tileset = new J2TFile(f.Filename);
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
                            bytes[xOrg + x/2 + (yOrg + y/2) * data.Stride] = tileImage[x + y * 32];
                        }
                }
                Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
                image.UnlockBits(data);
                tileset.Palette.Apply(image);
                pictureBox1.Image = image;
                //pictureBox1.Enabled = false;
                string tooltipText = f.Name /*+ " (" + Path.GetFileNameWithoutExtension(f.Filename) + ")"*/;
                toolTip1.SetToolTip(pictureBox1, tooltipText);
               // toolTip1.SetToolTip(panel, tooltipText);
                panel.Controls.Add(pictureBox1);
                flowLayoutPanel1.Controls.Add(panel);

                pictureBox1.Tag = tileset;
                pictureBox1.MouseEnter += PictureBox_MouseEnter;
                pictureBox1.MouseLeave += PictureBox_MouseLeave;
                pictureBox1.MouseClick += PictureBox1_MouseClick;

                if (++fileID == 12) //debug
                    break;
            }

            TilesetPalette.Location = new Point(buttonCancel.Right - TilesetPalette.Width, buttonCancel.Top - TilesetPalette.Height - 10);
            TilesetPalette.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Controls.Add(TilesetPalette);

            label3.Text = ""; //hide until a tileset is chosen

            ShowDialog();
        }

        Panel SelectedPanel;
        PictureBox SelectedPictureBox;
        J2TFile SelectedTileset;

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (SelectedPanel != null)
                SelectedPanel.BackColor = SystemColors.Control; //deselect
            SelectedPictureBox = sender as PictureBox;
            SelectedTileset = SelectedPictureBox.Tag as J2TFile;
            SelectedPanel = SelectedPictureBox.Parent as Panel;
            label3.Text = SelectedTileset.Name + "\n" + SelectedTileset.FilenameOnly + "\n" + SelectedTileset.TileCount.ToString() + " tiles";
            TilesetPalette.Palette = SelectedTileset.Palette;
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
    }
}
