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
    //uses https://www.codeproject.com/Articles/31105/A-ComboBox-with-a-CheckedListBox-as-a-Dropdown
    public partial class SmartTilesForm : Form
    {
        J2TFile Tileset;
        bool Result = false;
        SmartTile WorkingSmartTile;
        ushort AndValue;
        public SmartTilesForm()
        {
            InitializeComponent();
        }
        internal bool ShowForm(SmartTile workingSmartTile, J2TFile tileset, List<SmartTile> smartTiles, int workingSmartTileIndex = -1)
        {
            Tileset = tileset;
            WorkingSmartTile = workingSmartTile;
            for (int otherSmartTileID = 0; otherSmartTileID < smartTiles.Count; ++otherSmartTileID) {
                checkedComboBox1.Items.Add(
                    smartTiles[otherSmartTileID].Name,
                    otherSmartTileID == workingSmartTileIndex ?
                        CheckState.Indeterminate :
                        workingSmartTile.Friends.Contains(otherSmartTileID) ?
                            CheckState.Checked :
                            CheckState.Unchecked
                );
            }
            checkedComboBox1.SetItemCheckState(0, checkedComboBox1.GetItemCheckState(0)); //fixes issue of control not updating text preview in response to Items.Add
            checkedComboBox1.ItemCheck += (s, e) => { if (e.CurrentValue == CheckState.Indeterminate) e.NewValue = CheckState.Indeterminate; }; //don't let the indeterminate item (this smarttile itself) be altered
            AndValue = ((SmartTile.ushortComparer)WorkingSmartTile.AllPossibleTiles.Comparer).AndValue;
            textBox1.Text = WorkingSmartTile.Name;
            CreateImageFromTileset();

            using (new System.Threading.Timer(RedrawTiles, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0.5)))
                ShowDialog();
            return Result;
        }

        int elapsed = 0;
        static Point PointFromTileID(int tileID) {
            return new Point((tileID % 10) * 32, (tileID / 10) * 32);
        }
        private void RedrawTiles(object state)
        {
            lock (smartPicture)
            {
                var image = smartPicture.Image;
                using (Graphics graphics = Graphics.FromImage(image))
                    for (int i = 0; i < WorkingSmartTile.TileAssignments.Length; ++i)
                    {
                        var assignment = WorkingSmartTile.TileAssignments[i];
                        if (assignment.Count > 0)
                            lock (tilesetPicture)
                            {
                                DrawTilesetTileAt(graphics, PointFromTileID(i), assignment[elapsed % assignment.Count]);
                            }
                    }
                smartPicture.Image = image;
            }

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
                WorkingSmartTile.Friends.Clear();
                for (int i = 0; i < checkedComboBox1.Items.Count; ++i)
                    if (checkedComboBox1.GetItemCheckState(i) == CheckState.Checked) //indeterminate doesn't count
                        WorkingSmartTile.Friends.Add(i);
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

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);
        private void tilesetPicture_MouseClick(object sender, MouseEventArgs e)
        {
            if (CurrentSmartTileID >= 0 && CurrentSmartTileID < WorkingSmartTile.TileAssignments.Length)
            {
                ushort newTileID = (ushort)GetMouseTileIDFromTileset(e);
                var assignment = WorkingSmartTile.TileAssignments[CurrentSmartTileID];
                if (e.Button == MouseButtons.Left)
                {
                    if ((GetKeyState((int)Keys.F) & 0x8000) != 0)
                        newTileID |= (ushort)(AndValue + 1);
                    if ((GetKeyState((int)Keys.I) & 0x8000) != 0)
                        newTileID |= 0x2000;
                    assignment.Add(newTileID);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (assignment.RemoveAll(potentialTileIDToRemove => (potentialTileIDToRemove & AndValue) == (newTileID & AndValue)) == 0)
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

        static readonly Size TileSize = new Size(32, 32);
        static readonly Rectangle RectangleAtOrigin = new Rectangle(0, 0, 32, 32);
        void DrawTilesetTileAt(Graphics graphics, Point dest, int tileID)
        {
            int flipFlags = (tileID >> 12) & 3;
            if (AndValue == (1024 - 1) && ((tileID & 1024) == 1024))
                flipFlags |= 1;
            switch (flipFlags) {
                case 0:
                    graphics.TranslateTransform(dest.X, dest.Y);
                    break;
                case 1:
                    graphics.TranslateTransform(dest.X + 32, dest.Y);
                    graphics.ScaleTransform(-1, 1);
                    break;
                case 2:
                    graphics.TranslateTransform(dest.X, dest.Y + 32);
                    graphics.ScaleTransform(1, -1);
                    break;
                case 3:
                    graphics.TranslateTransform(dest.X + 32, dest.Y + 32);
                    graphics.ScaleTransform(-1, -1);
                    break;
            }
            graphics.DrawImage(tilesetPicture.Image, RectangleAtOrigin, new Rectangle(PointFromTileID(tileID & AndValue), TileSize), GraphicsUnit.Pixel);
            graphics.ResetTransform();
        }

        void UpdateFramesPreview()
        {
            if (CurrentSmartTileID >= 0 && CurrentSmartTileID < WorkingSmartTile.TileAssignments.Length)
            {
                var frames = WorkingSmartTile.TileAssignments[CurrentSmartTileID];
                framesPicture.Height = frames.Count * 32;
                if (frames.Count > 0)
                {
                    lock (tilesetPicture)
                    {
                        framesPicture.Image = new Bitmap(32, framesPicture.Height);
                        var image = framesPicture.Image;
                        using (Graphics graphics = Graphics.FromImage(image))
                            for (int i = 0; i < frames.Count; ++i)
                                DrawTilesetTileAt(graphics, new Point(0, i * 32), frames[i]);
                        framesPicture.Image = image;
                    }
                }
                else
                {
                    lock (smartPicture)
                    {
                        var image = smartPicture.Image;
                        var rectangle = new Rectangle(PointFromTileID(CurrentSmartTileID), TileSize);
                        using (Graphics graphics = Graphics.FromImage(image))
                            graphics.DrawImage(Properties.Resources.SmartTilesPermutations, rectangle, rectangle, GraphicsUnit.Pixel);
                        smartPicture.Image = image;
                    }
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
