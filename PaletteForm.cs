using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace MLLE
{
    public partial class PaletteForm : Form
    {
        bool PaletteEdited = false;
        Palette DefaultPalette;
        Palette Palette = new Palette();
        Panel[] panels = new Panel[Palette.PaletteSize];

        public PaletteForm()
        {
            InitializeComponent();
        }

        void SetAllColors()
        {
            for (int i = 0; i < Palette.PaletteSize; ++i)
                panels[i].BackColor = Color.FromArgb(Palette[i][0], Palette[i][1], Palette[i][2]);
        }
        void SetColor(Panel panel, Color color)
        {
            panel.BackColor = color;
            var paletteEntry = Palette[(int)panel.Tag];
            paletteEntry[0] = color.R;
            paletteEntry[1] = color.G;
            paletteEntry[2] = color.B;
            PaletteEdited = true;
        }

        internal Palette ShowForm(Palette plusPalette, Palette defaultPalette)
        {
            DefaultPalette = defaultPalette;
            Palette.CopyFrom(plusPalette ?? DefaultPalette);
            for (int i = 0; i < Palette.PaletteSize; ++i)
            {
                Panel panel = panels[i] = new Panel();
                panel.Location = new Point(12 + (i % 16) * 19, OKButton.Location.Y + (i / 16) * 19);
                panel.Size = new Size(16, 16);
                panel.BorderStyle = BorderStyle.None;
                panel.Tag = i;
                if (i < 10 || i == 15 || i > 245)
                    panel.Enabled = false;
                else
                {
                    panel.Click += Panel_Click;
                    panel.DoubleClick += Panel_DoubleClick;
                }
                Controls.Add(panel);
            }
            SetAllColors();

            ShowDialog();

            return PaletteEdited ? Palette : null;
        }

        private void Panel_Click(object sender, EventArgs e)
        {
            Panel clickedpanel = sender as Panel;
            var newBorderStyle = clickedpanel.BorderStyle != BorderStyle.None ? BorderStyle.None : BorderStyle.Fixed3D;

            if (select1ToolStripMenuItem.Checked)
                clickedpanel.BorderStyle = newBorderStyle;
            else if (select8ToolStripMenuItem.Checked)
            {
                int firstOfGroup = (int)clickedpanel.Tag & ~7;
                for (uint i = 0; i < 8; ++i)
                    panels[firstOfGroup + i].BorderStyle = newBorderStyle;
            }
            else if (select16ToolStripMenuItem.Checked)
            {
                int firstOfGroup = (int)clickedpanel.Tag & ~15;
                for (uint i = 0; i < 16; ++i)
                    panels[firstOfGroup + i].BorderStyle = newBorderStyle;
            }
        }
        private void Panel_DoubleClick(object sender, EventArgs e)
        {
            Panel clickedpanel = sender as Panel;
            clickedpanel.BorderStyle = BorderStyle.Fixed3D;
            colorDialog1.Color = clickedpanel.BackColor;
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                uint numberOfSelectedPanels = 0;
                foreach (Panel findpanel in panels)
                    if (findpanel.BorderStyle == BorderStyle.Fixed3D)
                    {
                        ++numberOfSelectedPanels;
                        SetColor(findpanel, colorDialog1.Color);
                    }
                if (numberOfSelectedPanels == 1) //only this one
                    clickedpanel.BorderStyle = BorderStyle.None;
            }
        }


        private void ResetButton_Click(object sender, System.EventArgs e)
        {
            PaletteEdited = true;
            Palette.CopyFrom(DefaultPalette);
            SetAllColors();
        }

        private void ButtonCancel_Click(object sender, System.EventArgs e)
        {
            PaletteEdited = false;
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
        }

        private void select8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            select8ToolStripMenuItem.Checked = true;
            select1ToolStripMenuItem.Checked = select16ToolStripMenuItem.Checked = false;
        }

        private void select16ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            select16ToolStripMenuItem.Checked = true;
            select1ToolStripMenuItem.Checked = select8ToolStripMenuItem.Checked = false;
        }

        private void selectNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Panel panel in panels)
                panel.BorderStyle = BorderStyle.None;
        }

        void SwapChannels(int a, int b, int c)
        {
            foreach (Panel panel in panels)
                if (panel.BorderStyle != BorderStyle.None)
                {
                    var paletteEntry = Palette[(int)panel.Tag];
                    SetColor(panel, Color.FromArgb(paletteEntry[a], paletteEntry[b], paletteEntry[c]));
                }
        }

        private void swapRedGreenToolStripMenuItem_Click(object sender, EventArgs e) { SwapChannels(1, 0, 2);  }
        private void swapGreenBlueToolStripMenuItem_Click(object sender, EventArgs e) { SwapChannels(0, 2, 1); }
        private void swapBlueRedToolStripMenuItem_Click(object sender, EventArgs e) { SwapChannels(2, 1, 0); }

        private void gradientToolStripMenuItem_Click(object sender, EventArgs e) //pretty much copied from JJ2+
        {
            var selectedPanels = Array.FindAll(panels, panel => panel.BorderStyle != BorderStyle.None);
            var length = selectedPanels.Length - 1;
            if (selectedPanels.Length > 2) //long enough to make a gradient from
            {
                Color first = selectedPanels[0].BackColor;
                Color last = selectedPanels[length].BackColor;
                float[] steps = new float[3] { (last.R - first.R) / (float)length, (last.G - first.G) / (float)length, (last.B - first.B) / (float)length };
                for (int i = 1; i < length; i++)
                    SetColor(selectedPanels[i], Color.FromArgb((byte)(first.R + (steps[0] * i)), (byte)(first.G + (steps[1] * i)), (byte)(first.B + (steps[2] * i))));
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var filepath = openFileDialog1.FileName;
                if (Path.GetExtension(filepath) == ".j2t") //J2TFile has zero error checking, so hopefully this works out
                {
                    PaletteEdited = true;
                    Palette.CopyFrom(new J2TFile(filepath).Palette);
                    SetAllColors();
                }
                else
                    using (BinaryReader binreader = new BinaryReader(File.Open(filepath, FileMode.Open, FileAccess.Read), J2TFile.FileEncoding))
                    {
                        if (binreader.BaseStream.Length < 1024)
                            return;
                        else if (binreader.BaseStream.Length == 1032) //"color map" palette
                            binreader.BaseStream.Seek(4, SeekOrigin.Begin);
                        Palette = new Palette(binreader);
                        PaletteEdited = true;
                        SetAllColors();
                    }
            }
        }
    }
}
