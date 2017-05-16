using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MLLE
{
    public partial class TilesetForm : Form
    {
        J2TFile Tileset;
        List<J2TFile> Tilesets;
        uint MaxTilesSupportedByLevel;
        uint NumberOfTilesInThisLevelBesidesThisTileset;
        bool Result = false;
        public TilesetForm()
        {
            InitializeComponent();
        }
        internal bool ShowForm(J2TFile tileset, List<J2TFile> tilesets, int max, uint number)
        {
            Tileset = tileset;
            Tilesets = tilesets;
            MaxTilesSupportedByLevel = (uint)max;
            NumberOfTilesInThisLevelBesidesThisTileset = number;
            inputLast.Maximum = Tileset.TotalNumberOfTiles;
            inputLast.Value = Tileset.FirstTile + Tileset.TileCount;
            inputFirst.Maximum = inputLast.Value - 1;
            inputFirst.Value = Tileset.FirstTile;
            inputLast.Minimum = inputFirst.Value + 1;
            ButtonDelete.Visible = Tilesets.Contains(Tileset);
            UpdateOutputString();
            ShowDialog();
            return Result;
        }

        private void UpdateOutputString()
        {
            var proposedTileCount = inputLast.Value - inputFirst.Value;
            var proposedTotal = NumberOfTilesInThisLevelBesidesThisTileset + proposedTileCount;
            outputMath.Text = String.Format("{0} + {1} =\n{2}/{3}", NumberOfTilesInThisLevelBesidesThisTileset, proposedTileCount, proposedTotal, MaxTilesSupportedByLevel);
            outputMath.ForeColor = (OKButton.Enabled = proposedTotal <= MaxTilesSupportedByLevel) ? Color.Black : Color.Red;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            Tileset.FirstTile = (uint)inputFirst.Value;
            Tileset.TileCount = (uint)(inputLast.Value - inputFirst.Value);
            if (!Tilesets.Contains(Tileset))
                Tilesets.Add(Tileset);
            Result = true;
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            Tilesets.Remove(Tileset);
            Result = true;
            Dispose();
        }

        private void inputFirst_ValueChanged(object sender, EventArgs e)
        {
            inputLast.Minimum = inputFirst.Value + 1;
            UpdateOutputString();
        }

        private void inputLast_ValueChanged(object sender, EventArgs e)
        {
            inputFirst.Maximum = inputLast.Value - 1;
            UpdateOutputString();
        }
    }
}
