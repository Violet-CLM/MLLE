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
    public partial class TilesetForm : Form
    {
        J2TFile Tileset;
        J2LFile Level;
        List<J2TFile> Tilesets;
        uint MaxTilesSupportedByLevel;
        uint NumberOfTilesInThisLevelBesidesThisTileset;
        bool Result = false;
        Palette LevelPalette;
        Bitmap TilesetOriginalColorsImage;
        Bitmap Tileset32BitTiles = null;
        int InitialTileCount, InitialFirstTile;
        public TilesetForm()
        {
            InitializeComponent();
        }
        internal bool ShowForm(J2TFile tileset, J2LFile level, int max, uint number)
        {
            Tileset = tileset;
            Level = level;
            Tilesets = level.Tilesets;
            LevelPalette = level.Palette;
            MaxTilesSupportedByLevel = (uint)max;
            NumberOfTilesInThisLevelBesidesThisTileset = number;
            inputLast.Maximum = Tileset.TotalNumberOfTiles;
            inputLast.Value = (InitialFirstTile = (int)Tileset.FirstTile) + (InitialTileCount = (int)Tileset.TileCount);
            inputFirst.Maximum = inputLast.Value - 1;
            inputFirst.Value = Tileset.FirstTile;
            inputLast.Minimum = inputFirst.Value + 1;
            ButtonDelete.Visible = Tilesets.Contains(Tileset);
            CreateImageFromTileset();
            UpdatePreviewControls();
            ShowDialog();
            return Result;
        }

        internal void DrawTilesetUsingRemappedLevelPalette()
        {
            Image image = TilesetOriginalColorsImage.Clone() as Bitmap;
            var palette = image.Palette;
            var entries = palette.Entries;
            switch (Tileset.ColorImportStyle)
            {
                case J2TFile.ColorImportStyles.normal8bit:
                    for (uint i = 0; i < Palette.PaletteSize; ++i)
                        entries[i] = Palette.Convert(LevelPalette.Colors[i]);
                    break;
                case J2TFile.ColorImportStyles.remapped8bit:
                    for (uint i = 0; i < Palette.PaletteSize; ++i)
                        entries[i] = Palette.Convert(LevelPalette.Colors[Tileset.ColorRemapping[i]]);
                    break;
                case J2TFile.ColorImportStyles.normal24bit: //already perfect
                    break;
                case J2TFile.ColorImportStyles.alternatePalette24bit:
                    var alternatePalette = Level.PlusPropertyList.NamedPalettes[Tileset.AlternatePaletteMappingID24Bit].Palette.Colors;
                    for (uint i = 0; i < Palette.PaletteSize; ++i)
                        entries[i] = Palette.Convert(alternatePalette[i]);
                    break;
            }
            image.Palette = palette;
            if (Tileset.VersionType == Version.Plus)
            {
                pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (var gr = Graphics.FromImage(pictureBox1.Image))
                {
                    gr.DrawImage(image, new Point(0, 0));
                    gr.DrawImage(Tileset32BitTiles, new Point(0, 0));
                }
            }
            else
            {
                pictureBox1.Image = image;
            }
            pictureBox1.Refresh();
        }

        private void CreateImageFromTileset()
        {
            pictureBox1.Height = (int)(Tileset.TotalNumberOfTiles + 9) / 10 * 32;

            Bitmap image, image32 = null;
            BitmapData data, data32 = null;
            byte[] bytes, bytes32 = null;

            image = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            bytes = new byte[data.Height * data.Stride];
            if (Tileset.VersionType == Version.Plus)
            {
                image32 = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                data32 = image32.LockBits(new Rectangle(0, 0, image32.Width, image32.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                bytes32 = new byte[data32.Height * data32.Stride];
            }

            for (uint i = 0; i < Tileset.TotalNumberOfTiles; ++i)
            {
                var tileImage = Tileset.Images[Tileset.ImageAddress[i]];
                var xOrg = (i % 10) * 32;
                var yOrg = i / 10 * 32;
                if (tileImage.Length == 32 * 32)
                    for (uint y = 0; y < 32; ++y)
                        Array.Copy(tileImage, y * 32, bytes, xOrg + (yOrg + y) * data.Stride, 32);
                else
                {
                    xOrg *= 4;
                    for (uint y = 0; y < 32; ++y)
                    {
                        var srcStart = y * 32 * 4;
                        var dstStart = xOrg + (yOrg + y) * data32.Stride;
                        for (uint x = 0; x < 32; ++x, srcStart += 4, dstStart += 4)
                        { //change from RGBA to BGRA :(
                            bytes32[dstStart + 0] = tileImage[srcStart + 2];
                            bytes32[dstStart + 1] = tileImage[srcStart + 1];
                            bytes32[dstStart + 2] = tileImage[srcStart + 0];
                            bytes32[dstStart + 3] = tileImage[srcStart + 3];
                        }
                    }
                }
            }
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            image.UnlockBits(data);
            Tileset.Palette.Apply(image);
            TilesetOriginalColorsImage = image;

            if (Tileset.VersionType == Version.Plus)
            {
                Marshal.Copy(bytes32, 0, data32.Scan0, bytes32.Length);
                image32.UnlockBits(data32);
                Tileset32BitTiles = image32;
            }

            foreach (var palette in Level.PlusPropertyList.NamedPalettes)
                ColorsList.Items.Add(palette.Name);
            if (Tileset.ColorImportStyle != J2TFile.ColorImportStyles.alternatePalette24bit)
                ColorsList.SelectedIndex = (int)Tileset.ColorImportStyle;
            else
                ColorsList.SelectedIndex = 3 + Tileset.AlternatePaletteMappingID24Bit;
        }

        private void UpdatePreviewControls()
        {
            var proposedTileCount = inputLast.Value - inputFirst.Value;
            var proposedTotal = NumberOfTilesInThisLevelBesidesThisTileset + proposedTileCount;
            outputMath.Text = String.Format("{0} + {1} =\n{2}/{3}", NumberOfTilesInThisLevelBesidesThisTileset, proposedTileCount, proposedTotal, MaxTilesSupportedByLevel);
            outputMath.ForeColor = (OKButton.Enabled = proposedTotal <= MaxTilesSupportedByLevel) ? Color.Black : Color.Red;
            EdgePanelLeft.Location = GetPointFromTileID((int)inputFirst.Value, 0);
            EdgePanelRight.Location = GetPointFromTileID((int)inputLast.Value - 1, 32);
        }

        bool AlreadyGaveConsentToDeletingTiles = false;
        bool DeleteReferencedTilesWarning()
        {
            if (AlreadyGaveConsentToDeletingTiles) return true;
            if (MessageBox.Show("This level contains references to one or more of the tiles you are removing. Those references will be deleted.", "Undefined Tiles", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand) != DialogResult.OK)
                return false;
            return AlreadyGaveConsentToDeletingTiles = true;
        }
        static ushort SetTileToZero(ushort tileID)
        {
            return 0;
        }
        private int GetTilesetFirstTileIDInLevel()
        {
            int TilesetFirstTileIDInLevel = 0;
            foreach (J2TFile otherTileset in Tilesets)
                if (otherTileset == Tileset)
                    break;
                else
                    TilesetFirstTileIDInLevel += (int)otherTileset.TileCount;
            return TilesetFirstTileIDInLevel;
        }
        private void OKButton_Click(object sender, EventArgs e)
        {
            int FirstTile = (int)inputFirst.Value;
            int LastTile = (int)inputLast.Value;
            int InitialLastTile = InitialFirstTile + InitialTileCount;
            int TilesetFirstTileIDInLevel = GetTilesetFirstTileIDInLevel();

            if (!Tilesets.Contains(Tileset))
                Tilesets.Add(Tileset);
            else
            {
                AlreadyGaveConsentToDeletingTiles = false;
                int offsetFromChangedFirst = InitialFirstTile - FirstTile;
                int offsetFromChangedLast = InitialLastTile - LastTile;

                if (offsetFromChangedFirst < 0) //InitialFirstTile < FirstTile
                    if (!Level.ChangeRangeOfTiles(
                        TilesetFirstTileIDInLevel,
                        TilesetFirstTileIDInLevel - offsetFromChangedFirst - 1,
                        DeleteReferencedTilesWarning,
                        SetTileToZero
                    ))
                        return;
                if (offsetFromChangedLast > 0) //InitialLastTile > LastTile
                    if (!Level.ChangeRangeOfTiles(
                        TilesetFirstTileIDInLevel - InitialFirstTile + LastTile,
                        TilesetFirstTileIDInLevel - InitialFirstTile + InitialLastTile - 1,
                        DeleteReferencedTilesWarning,
                        SetTileToZero
                    ))
                        return;
                
                if (offsetFromChangedLast != 0) //InitialLastTile != LastTile
                    Level.ChangeRangeOfTiles(
                        TilesetFirstTileIDInLevel + InitialLastTile - InitialFirstTile,
                        Level.AnimOffset - 1,
                        () => { return true; },
                        (ushort tileID) => { return (ushort)(tileID - offsetFromChangedLast); }
                    );
                if (offsetFromChangedFirst != 0) //InitialFirstTile != FirstTile
                    Level.ChangeRangeOfTiles(
                        TilesetFirstTileIDInLevel,
                        Level.AnimOffset - 1,
                        () => { return true; },
                        (ushort tileID) => { return (ushort)(tileID + offsetFromChangedFirst); }
                    );
            }

            Tileset.FirstTile = (uint)FirstTile;
            Tileset.TileCount = (uint)(LastTile - FirstTile);

            Result = true;
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            AlreadyGaveConsentToDeletingTiles = false;
            int TilesetFirstTileIDInLevel = GetTilesetFirstTileIDInLevel();

            if (Level.ChangeRangeOfTiles(
                TilesetFirstTileIDInLevel,
                TilesetFirstTileIDInLevel + InitialTileCount - 1,
                DeleteReferencedTilesWarning,
                SetTileToZero
            ))
            {
                Level.ChangeRangeOfTiles(
                    TilesetFirstTileIDInLevel,
                    Level.AnimOffset - 1,
                    () => { return true; },
                    (ushort tileID) => { return (ushort)(tileID - InitialTileCount); }
                );
                Tilesets.Remove(Tileset);
                Result = true;
                Dispose();
            }
        }

        private void inputFirst_ValueChanged(object sender, EventArgs e)
        {
            inputLast.Minimum = inputFirst.Value + 1;
            UpdatePreviewControls();
        }

        private void inputLast_ValueChanged(object sender, EventArgs e)
        {
            inputFirst.Maximum = inputLast.Value - 1;
            UpdatePreviewControls();
        }

        int GetMouseTileIDFromTileset(MouseEventArgs e)
        {
            var pictureOrigin = pictureBox1.AutoScrollOffset;
            return ((e.X - pictureOrigin.X) / 32 + (e.Y - pictureOrigin.Y) / 32 * 10);
        }
        Point GetPointFromTileID(int tileID, int xAdjust)
        {
            return new Point(
                pictureBox1.Left + (tileID % 10) * 32 - EdgePanelLeft.Width / 2 + xAdjust,
                pictureBox1.Top + (tileID / 10) * 32 - (EdgePanelLeft.Height - 32) / 2
            );
        }


        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Text = "Setup Extra Tileset \u2013 " + GetMouseTileIDFromTileset(e);
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            Text = "Setup Extra Tileset";
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                    inputLast.Value = GetMouseTileIDFromTileset(e) + 1;
                else
                    inputFirst.Value = GetMouseTileIDFromTileset(e);
            }
            catch { } //invalid value because of Minimum/Maximum of that textbox; do nothing
        }

        int LastSelectedIndex = -1;
        private void ColorsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var newSelectedIndex = (sender as ListBox).SelectedIndex;
            switch (newSelectedIndex)
            {
                case 1:
                    if (LastSelectedIndex < 0 || new SpriteRecolorForm().ShowForm(LevelPalette, TilesetOriginalColorsImage.Clone() as Bitmap, ref Tileset.ColorRemapping, BackColor))
                    {
                        Tileset.ColorImportStyle = J2TFile.ColorImportStyles.remapped8bit;
                    }
                    else
                    {
                        Tileset.ColorImportStyle = J2TFile.ColorImportStyles.normal8bit;
                        ColorsList.SelectedIndex = 0;
                        return;
                    }
                    break;
                case 0:
                    Tileset.ColorImportStyle = J2TFile.ColorImportStyles.normal8bit;
                    break;
                case 2:
                    Tileset.ColorImportStyle = J2TFile.ColorImportStyles.normal24bit;
                    break;
                default:
                    Tileset.ColorImportStyle = J2TFile.ColorImportStyles.alternatePalette24bit;
                    Tileset.AlternatePaletteMappingID24Bit = (byte)(newSelectedIndex - 3);
                    break;
            }
            LastSelectedIndex = newSelectedIndex;
            DrawTilesetUsingRemappedLevelPalette();
        }
    }
}
