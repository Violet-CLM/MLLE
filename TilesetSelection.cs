using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLLE
{
    public partial class TilesetSelection : Form
    {
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
                pictureBox1.BackColor = transparentColor;
                pictureBox1.Location = new Point(10, 10);

                J2TFile tileset = new J2TFile(f.Filename);
                tileset.Palette.Colors[0] = Palette.Convert(transparentColor);

                var image = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                byte[] bytes = new byte[data.Height * data.Stride];
                //Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                uint numberOfTilesToDraw = Math.Min(tileset.TotalNumberOfTiles, 100);
                for (uint i = 0; i < numberOfTilesToDraw; ++i)
                {
                    var tileImage = tileset.Images[tileset.ImageAddress[i]];
                    var xOrg = (i % 10) * 16;
                    var yOrg = i / 10 * 16;
                    for (uint x = 0; x < 32; x += 2)
                        for (uint y = 0; y < 32; y += 2)
                        {
                            bytes[xOrg + x/2 + (yOrg + y/2) * data.Stride] = tileImage[x + y * 32];
                        }
                }
                Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
                image.UnlockBits(data);
                tileset.Palette.Apply(image);
                pictureBox1.Image = image;
                panel.Controls.Add(pictureBox1);
                flowLayoutPanel1.Controls.Add(panel);

                if (++fileID == 12) //debug
                    break;
            }

            ShowDialog();
        }
    }
}
