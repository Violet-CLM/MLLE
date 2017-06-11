using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MLLE
{
    public partial class SpriteRecolorForm : Form
    {
        bool Result = false;
        Palette ImageOriginalPalette = new Palette();
        Palette LevelPalette;
        ColorPalette ImageWorkingPalette;
        public SpriteRecolorForm()
        {
            InitializeComponent();
        }
        internal bool ShowForm(Palette palette, J2TFile tileset)
        {
            //CreateImageFromTileset(tileset);
            ShowDialog();
            return Result;
        }
        internal bool ShowForm(Palette palette, Bitmap bitmap)
        {
            //CreateImageFromTileset(tileset);
            pictureBox1.Image = bitmap;
            pictureBox1.Size = bitmap.Size;
            Width -= (320 - pictureBox1.Width); //this doesn't work very well because windows have a minimum size, dictated by their border/menu properties, that I cannot fully override

            LevelPalette = palette;
            ImageWorkingPalette = bitmap.Palette;
            for (uint i = 0; i < Palette.PaletteSize; ++i)
            {
                ImageOriginalPalette.Colors[i] = Palette.Convert(ImageWorkingPalette.Entries[i]);
                ImageWorkingPalette.Entries[i] = Palette.Convert(LevelPalette.Colors[i]);
            }
            bitmap.Palette = ImageWorkingPalette;

            ShowDialog();
            return Result;
        }

        /*internal static void GetColorPaletteFromTileset(J2TFile tileset, Bitmap image)
        {
            var palette = image.Palette;
            var entries = palette.Entries;
            for (uint i = 0; i < 256; ++i)
            {
                var color = tileset.Palette.Colors[i];
                entries[i] = Color.FromArgb(color[0], color[1], color[2]);
            }
            image.Palette = palette;
        }

        private void CreateImageFromTileset(J2TFile tileset)
        {
            pictureBox1.Height = (int)(tileset.TotalNumberOfTiles + 9) / 10 * 32;
            var image = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            byte[] bytes = new byte[data.Height * data.Stride];
            //Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            for (uint i = 0; i < tileset.TotalNumberOfTiles; ++i)
            {
                var tileImage = tileset.Images[tileset.ImageAddress[i]];
                var xOrg = (i % 10) * 32;
                var yOrg = i / 10 * 32;
                for (uint x = 0; x < 32; ++x)
                    for (uint y = 0; y < 32; ++y)
                    {
                        bytes[xOrg + x + (yOrg + y) * data.Stride] = tileImage[x + y*32];
                    }
            }
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            image.UnlockBits(data);
            pictureBox1.Image = image;

            GetColorPaletteFromTileset(tileset, image);
        }*/

        private void OKButton_Click(object sender, EventArgs e)
        {
            Result = true;
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
