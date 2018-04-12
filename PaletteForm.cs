using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace MLLE
{
    public partial class PaletteForm : Form
    {
        Palette DefaultPalette;

        public PaletteForm()
        {
            InitializeComponent();
        }

        Palette InitialPalette;
        PaletteImage PaletteImage = new PaletteImage(15, 2, false, true);
        internal Palette ShowForm(Palette plusPalette, Palette defaultPalette)
        {
            DefaultPalette = defaultPalette;
            InitialPalette = (plusPalette ?? DefaultPalette);

            for (int i = 0; i < 10; ++i)
                PaletteImage.ColorDisabled[i] = PaletteImage.ColorDisabled[Palette.PaletteSize - 10 + i] = true; //transparency, and default windows colors
            PaletteImage.Location = new Point(12, OKButton.Location.Y);
            PaletteImage.Palette = InitialPalette;
            PaletteImage.MouseMove += PaletteImageMouseMove;
            PaletteImage.MouseLeave += PaletteImageMouseLeave;
            Controls.Add(PaletteImage);

            ShowDialog();

            return (!PaletteImage.Palette.Equals(InitialPalette)) ? PaletteImage.Palette : null;
        }


        private void ResetButton_Click(object sender, System.EventArgs e)
        {
            PaletteImage.Palette = DefaultPalette;
        }

        private void ButtonCancel_Click(object sender, System.EventArgs e)
        {
            PaletteImage.Palette.CopyFrom(InitialPalette);
            Close();
        }

        private void OKButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void select1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            select1ToolStripMenuItem.Checked = true;
            select8ToolStripMenuItem.Checked = select16ToolStripMenuItem.Checked = false;
            PaletteImage.NumberOfColorsToSelectAtATime = 1;
        }

        private void select8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            select8ToolStripMenuItem.Checked = true;
            select1ToolStripMenuItem.Checked = select16ToolStripMenuItem.Checked = false;
            PaletteImage.NumberOfColorsToSelectAtATime = 8;
        }

        private void select16ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            select16ToolStripMenuItem.Checked = true;
            select1ToolStripMenuItem.Checked = select8ToolStripMenuItem.Checked = false;
            PaletteImage.NumberOfColorsToSelectAtATime = 16;
        }

        private void selectNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PaletteImage.SetSelected(PaletteImage.AllPaletteColors, false);
        }

        void SwapChannels(int a, int b, int c)
        {
            var selections = PaletteImage.GetSelectedIndices();
            foreach (var selectedIndex in selections) {
                var paletteEntry = PaletteImage.Palette[selectedIndex];
                PaletteImage.Palette[selectedIndex] = new byte[] { paletteEntry[a], paletteEntry[b], paletteEntry[c], byte.MaxValue };
            }
            PaletteImage.Update(selections);
        }

        private void swapRedGreenToolStripMenuItem_Click(object sender, EventArgs e) { SwapChannels(1, 0, 2);  }
        private void swapGreenBlueToolStripMenuItem_Click(object sender, EventArgs e) { SwapChannels(0, 2, 1); }
        private void swapBlueRedToolStripMenuItem_Click(object sender, EventArgs e) { SwapChannels(2, 1, 0); }

        private void gradientToolStripMenuItem_Click(object sender, EventArgs e) //pretty much copied from JJ2+
        {
            var selections = PaletteImage.GetSelectedIndices();
            var length = selections.Length - 1;
            if (selections.Length > 2) //long enough to make a gradient from
            {
                Color first = Palette.Convert(PaletteImage.Palette[selections[0]]);
                Color last = Palette.Convert(PaletteImage.Palette[selections[length]]);
                float[] steps = new float[3] { (last.R - first.R) / (float)length, (last.G - first.G) / (float)length, (last.B - first.B) / (float)length };
                for (int i = 1; i < length; i++)
                    PaletteImage.Palette[selections[i]] = new byte[] { (byte)(first.R + (steps[0] * i)), (byte)(first.G + (steps[1] * i)), (byte)(first.B + (steps[2] * i)), byte.MaxValue };
                PaletteImage.Update(selections);
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var filepath = openFileDialog1.FileName;
                if (Path.GetExtension(filepath) == ".j2t") //J2TFile has zero error checking, so hopefully this works out
                {
                    PaletteImage.Palette = new J2TFile(filepath).Palette;
                }
                else
                    using (BinaryReader binreader = new BinaryReader(File.Open(filepath, FileMode.Open, FileAccess.Read), J2TFile.FileEncoding))
                    {
                        if (binreader.BaseStream.Length < 1024)
                            return;
                        else if (binreader.BaseStream.Length == 1032) //"color map" palette
                            binreader.BaseStream.Seek(4, SeekOrigin.Begin);
                        PaletteImage.Palette = new Palette(binreader);
                    }
            }
        }

        private void toolsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var count = PaletteImage.NumberOfSelectedColors;
            gradientToolStripMenuItem.Enabled =
                count >= 3;
            swapBlueRedToolStripMenuItem.Enabled = swapGreenBlueToolStripMenuItem.Enabled = swapRedGreenToolStripMenuItem.Enabled =
                count > 0;
        }

        private void toolsToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            gradientToolStripMenuItem.Enabled = true; //so that Ctrl+G is always available
        }
        private void PaletteImageMouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender as Control).ClientRectangle.Contains(e.Location))
                return;
            Text = "Level Palette \u2013 " + PaletteImage.getSelectedColor(e);
        }
        
        private void PaletteImageMouseLeave(object sender, EventArgs e)
        {
            Text = "Level Palette";
        }
    }
}
