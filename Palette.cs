using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

internal class Palette
{
    public const uint PaletteSize = 256;
    public byte[][] Colors = new byte[PaletteSize][];

    public Palette() { }
    public Palette(BinaryReader binreader, bool LEVformat = false)
    {
        for (uint i = 0; i < PaletteSize; ++i)
        {
            var color = Colors[i] = new byte[4];
            color[0] = binreader.ReadByte();
            color[1] = binreader.ReadByte();
            color[2] = binreader.ReadByte();
            if (!LEVformat)
                color[3] = binreader.ReadByte();
        }
    }

    public static Color Convert(byte[] src)
    {
        return Color.FromArgb(byte.MaxValue, src[0], src[1], src[2]);
    }
    public static byte[] Convert(Color src, bool respectAlpha = false)
    {
        return new byte[4] { src.R, src.G, src.B, respectAlpha ? src.A : byte.MaxValue };
    }

    internal void CopyFrom(Palette other)
    {
        if (other != null)
        {
            for (int i = 0; i < PaletteSize; ++i)
                Colors[i] = other.Colors[i].Clone() as byte[];
        }
    }

    public void WriteLEVStyle(BinaryWriter binwriter)
    {
        foreach (byte[] color in Colors)
        {
            binwriter.Write(color[0]);
            binwriter.Write(color[1]);
            binwriter.Write(color[2]);
        }
    }

    public byte[] this[int key]
    {
        get
        {
            return Colors[key];
        }
        set
        {
            Colors[key] = value;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        else
        {
            Palette other = obj as Palette;
            for (int i = 0; i < PaletteSize; ++i)
                if (!Colors[i].SequenceEqual(other.Colors[i]))
                    return false;
            return true;
        }
    }

    internal byte FindNearestColor(byte[] requestedColor)
    {
        byte nearestEntry = 15;
        int smallestSquaredDifference = 0x40000;
        for (int i = 16; i < 256; i++)
        {
            int squaredDifference = 0;
            for (int j = 0; j < 3; j++)
            {
                int difference = (int)Colors[i][j] - (int)requestedColor[j];
                squaredDifference += difference * difference;
            }
            if (squaredDifference < smallestSquaredDifference)
            {
                nearestEntry = (byte)i;
                smallestSquaredDifference = squaredDifference;
            }
        }
        return nearestEntry;
    }
}


namespace MLLE
{
    class PaletteImage : PictureBox
    {
        public const int PaletteLengthOnEitherDimension = 16;
        int ColorSize, BorderSize, ColorTotalSize;
        public static readonly int[] AllPaletteColors = Enumerable.Range(0, (int)Palette.PaletteSize).ToArray();

        public bool[] ColorDisabled = new bool[Palette.PaletteSize];

        public int NumberOfColorsToSelectAtATime = 1;

        private Palette _Palette = new Palette();
        public Palette Palette {
            get {
                return _Palette;
            }
            set
            {
                _Palette.CopyFrom(value);
                Update(AllPaletteColors);
            }
        }


        private Bitmap ImageImage;
        bool ReadOnly;

        public PaletteImage(int colorSize, int borderSize, bool readOnly, bool registerStandardEvents)
        {
            Width = Height = (int)((ColorTotalSize = (ColorSize = colorSize) + (BorderSize = borderSize) * 2) * PaletteLengthOnEitherDimension);
            ImageImage = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ColorsSelected = new bool[Palette.PaletteSize];

            ReadOnly = readOnly;
            if (registerStandardEvents)
            {
                MouseDown += Clicked;
                if (!ReadOnly)
                    MouseDoubleClick += DoubleClicked;
                MouseMove += Moved;
            }
        }

        public bool CurrentlySelectingColors;
        public void Moved(object sender, MouseEventArgs e)
        {
            if (!(sender as Control).ClientRectangle.Contains(e.Location))
                return;
            if (e.Button != MouseButtons.None)
            {
                if (ColorsSelected[getSelectedColor(e)] != CurrentlySelectingColors)
                    Clicked(sender, e);
            }
        }

        public int getSelectedColor(MouseEventArgs e)
        {
            return (e.X / ColorTotalSize) + (e.Y / ColorTotalSize * PaletteLengthOnEitherDimension);
        }
        public void Clicked(object sender, MouseEventArgs e)
        {
            int colorSelected = getSelectedColor(e);
            if (!ColorDisabled[colorSelected])
                SetSelected(
                    Enumerable.Range(
                        colorSelected & ~(NumberOfColorsToSelectAtATime - 1),
                        NumberOfColorsToSelectAtATime
                    ).ToArray(),
                    CurrentlySelectingColors = !ColorsSelected[colorSelected]
                );
        }

        public int[] GetSelectedIndices()
        {
            return ColorsSelected.Select((item, index) => new { Item = item, Index = index })
               .Where(pair => pair.Item && !ColorDisabled[pair.Index]) //can't hurt to check ColorDisabled here too, probably
               .Select(result => result.Index)
               .ToArray();
        }
        public int NumberOfSelectedColors { get
            {
                return ColorsSelected.Count(item => item);
            }
        }
        private void DoubleClicked(object sender, MouseEventArgs e)
        {
            int colorSelected = getSelectedColor(e);
            if (ColorDisabled[colorSelected])
                return;
            if (!ColorsSelected[colorSelected])
                Clicked(sender, e);

            ColorDialog colorDialog1 = new ColorDialog();
            colorDialog1.Color = Palette.Convert(Palette[colorSelected]);
            colorDialog1.FullOpen = true;
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var color = Palette.Convert(colorDialog1.Color);
                var selections = GetSelectedIndices();
                foreach (var selectedIndex in selections)
                    Palette[selectedIndex] = color;
                if (selections.Length == 1) //only this one
                    ColorsSelected[colorSelected] = false;
                Update(selections);
            }
        }

        public bool[] ColorsSelected { get; }
        public void Update(int[] colorsToUpdate)
        {
            var backColorAsArray = Palette.Convert(BackColor);
            using (Graphics g = Graphics.FromImage(ImageImage))
            {
                using (Brush transparent = new SolidBrush(BackColor))
                    foreach (var colorToUpdate in colorsToUpdate) {
                        var color = !ColorDisabled[colorToUpdate] ? Palette[colorToUpdate] : backColorAsArray;
                        int left = (colorToUpdate % PaletteLengthOnEitherDimension) * ColorTotalSize;
                        int top = (colorToUpdate / PaletteLengthOnEitherDimension) * ColorTotalSize;
                        g.FillRectangle(ColorsSelected[colorToUpdate] ? Brushes.Black : transparent, new Rectangle(left, top, ColorTotalSize, ColorTotalSize)); //border
                        using (Brush brush = new SolidBrush(Color.FromArgb(color[0], color[1], color[2])))
                                g.FillRectangle(brush, new Rectangle(left + BorderSize, top + BorderSize, ColorSize, ColorSize)); //fill
                    }
                Image = ImageImage;
            }
        }
        public void SetSelected(int[] colorsToSelect, bool selecting)
        {
            foreach (var colorToSelect in colorsToSelect)
                ColorsSelected[colorToSelect] = !ColorDisabled[colorToSelect] ? selecting : false;
            Update(colorsToSelect);
        }
    }
}
