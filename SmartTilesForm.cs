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
    public partial class SmartTilesForm : Form
    {
        J2TFile Tileset;
        bool Result = false;
        SmartTile WorkingSmartTile;
        public SmartTilesForm()
        {
            InitializeComponent();
        }
        internal bool ShowForm(SmartTile workingSmartTile, J2TFile tileset)
        {
            Tileset = tileset;
            WorkingSmartTile = workingSmartTile;
            textBox1.Text = WorkingSmartTile.Name;
            CreateImageFromTileset();

            using (new System.Threading.Timer(RedrawTiles, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0.5)))
                ShowDialog();
            return Result;
        }

        int elapsed = 0;
        static Rectangle RectangleFromTileID(int tileID) {
            return new Rectangle((tileID % 10) * 32, (tileID / 10) * 32, 32, 32);
        }
        private void RedrawTiles(object state)
        {
            var image = smartPicture.Image;
            using (Graphics graphics = Graphics.FromImage(image))
                for (int i = 0; i < WorkingSmartTile.TileAssignments.Length; ++i) {
                    var assignment = WorkingSmartTile.TileAssignments[i];
                    if (assignment.Count > 0)
                    {
                        graphics.DrawImage(tilesetPicture.Image, RectangleFromTileID(i), RectangleFromTileID(assignment[elapsed % assignment.Count]), GraphicsUnit.Pixel);
                    }
                }
            smartPicture.Image = image;

            ++elapsed;
        }

        private void CreateImageFromTileset()
        {
            //there are enough windows that show tileset images you'd think I should turn some/all of this into a method somewhere
            tilesetPicture.Height = (int)(Tileset.TotalNumberOfTiles + 9) / 10 * 32;
            var image = new Bitmap(tilesetPicture.Width, tilesetPicture.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            byte[] bytes = new byte[data.Height * data.Stride];
            //Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            for (uint i = 0; i < Tileset.TotalNumberOfTiles; ++i)
            {
                var tileImage = Tileset.Images[Tileset.ImageAddress[i]];
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

            var palette = image.Palette;
            var entries = palette.Entries;
            entries[0] = TexturedJ2L.TranspColor;
            for (uint i = 1; i < Palette.PaletteSize; ++i)
                entries[i] = Palette.Convert(Tileset.Palette.Colors[i]);
            image.Palette = palette;

            tilesetPicture.Image = image;
            smartPicture.Image = Properties.Resources.SmartTilesPermutations;
            framesPicture.Image = new Bitmap(32, 32);
        }
        
        private void OKButton_Click(object sender, EventArgs e)
        {
            if (
                WorkingSmartTile.TileAssignments[11].Count > 0 ||
                WorkingSmartTile.TileAssignments[14].Count > 0 ||
                WorkingSmartTile.TileAssignments[47].Count > 0
            )
            {
                Result = true;
                WorkingSmartTile.Name = textBox1.Text;
            } else {
                if (MessageBox.Show("To define a Smart Tile you must pick at least one tile for at least one of the following default permutation IDs: 11, 14, or 47.", "Insufficient Definition", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    return;
                //else fall through:
            }
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
        
        int GetMouseTileIDFromTileset(MouseEventArgs e)
        {
            var pictureOrigin = tilesetPicture.AutoScrollOffset;
            return ((e.X - pictureOrigin.X) / 32 + (e.Y - pictureOrigin.Y) / 32 * 10);
        }
        Point GetPointFromTileID(int tileID, int xAdjust)
        {
            return new Point(
                tilesetPicture.Left + (tileID % 10) * 32,
                tilesetPicture.Top + (tileID / 10) * 32
            );
        }


        private void tilesetPicture_MouseMove(object sender, MouseEventArgs e)
        {
            Text = "Define Smart Tiles \u2013 " + GetMouseTileIDFromTileset(e);
        }
        private void smartPicture_MouseMove(object sender, MouseEventArgs e)
        {
            Text = "Define Smart Tiles \u2013 " + (e.X / 32 + e.Y / 32 * 10);
        }
        private void tilesetPicture_MouseLeave(object sender, EventArgs e)
        {
            Text = "Define Smart Tiles";
        }

        private void tilesetPicture_MouseClick(object sender, MouseEventArgs e)
        {
            if (CurrentSmartTileID >= 0 && CurrentSmartTileID < WorkingSmartTile.TileAssignments.Length)
            {
                ushort newTileID = (ushort)GetMouseTileIDFromTileset(e);
                var assignment = WorkingSmartTile.TileAssignments[CurrentSmartTileID];
                if (e.Button == MouseButtons.Left)
                    assignment.Add(newTileID);
                else if (e.Button == MouseButtons.Right)
                {
                    if (!assignment.Remove(newTileID))
                        return;
                }
                else
                    return;
                elapsed = assignment.Count - 1;
                RedrawTiles(null);
                UpdateFramesPreview();
            }
        }

        int CurrentSmartTileID = -1;
        private void smartPicture_MouseClick(object sender, MouseEventArgs e)
        {
            int newSmartTileID = e.X / 32 + e.Y / 32 * 10;
            if (SmartTile.AlternativeAssignments[newSmartTileID] != null) //not an empty space in the image
            {
                HighlightPanel.Visible = true;
                HighlightPanel.Location = new Point((e.X & ~31) + 11, (e.Y & ~31) + 11); //11 is half of (32-10), and the highlight is 10x10
                CurrentSmartTileID = newSmartTileID;

                if (e.Button == MouseButtons.Right)
                {
                    WorkingSmartTile.TileAssignments[CurrentSmartTileID].Clear();
                }

                UpdateFramesPreview();
            }
        }

        void UpdateFramesPreview()
        {
            if (CurrentSmartTileID >= 0 && CurrentSmartTileID < WorkingSmartTile.TileAssignments.Length)
            {
                var frames = WorkingSmartTile.TileAssignments[CurrentSmartTileID];
                framesPicture.Height = frames.Count * 32;
                if (frames.Count > 0)
                {
                    framesPicture.Image = new Bitmap(32, framesPicture.Height);
                    var image = framesPicture.Image;
                    using (Graphics graphics = Graphics.FromImage(image))
                        for (int i = 0; i < frames.Count; ++i)
                            graphics.DrawImage(tilesetPicture.Image, new Rectangle(0, i * 32, 32, 32), RectangleFromTileID(frames[i]), GraphicsUnit.Pixel);
                    framesPicture.Image = image;
                }
                else
                {
                    var image = smartPicture.Image;
                    var rectangle = RectangleFromTileID(CurrentSmartTileID);
                    using (Graphics graphics = Graphics.FromImage(image))
                        graphics.DrawImage(Properties.Resources.SmartTilesPermutations, rectangle, rectangle, GraphicsUnit.Pixel);
                    smartPicture.Image = image;
                }
            }
            else framesPicture.Height = 0;
        }

        private void framesPicture_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var y = e.Y - framesPicture.AutoScrollOffset.Y;
                if (y < framesPicture.Height) //just to be safe
                {
                    WorkingSmartTile.TileAssignments[CurrentSmartTileID].RemoveAt(y / 32);
                    UpdateFramesPreview();
                }
            }
        }
    }
}
