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
        return Color.FromArgb(src[0], src[1], src[2], src[3]);
    }
    public static byte[] Convert(Color src)
    {
        return new byte[4] { src.R, src.G, src.B, src.A };
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
}


namespace MLLE
{
    class PaletteImage : PictureBox
    {
        public const int PaletteLengthOnEitherDimension = 16;
        int ColorSize, BorderSize, ColorTotalSize;

        public int NumberOfColorsToSelectAtATime = 1;

        private Palette _Palette;
        public Palette Palette {
            get {
                return _Palette;
            }
            set
            {
                _Palette = value;
                Update(Enumerable.Range(0, (int)Palette.PaletteSize).ToArray());
            }
        }


        private Bitmap ImageImage;
        //private BitmapData ImageData;
        //private byte[] Bytes;

        public PaletteImage(int colorSize, int borderSize)
        {
            Width = Height = (int)((ColorTotalSize = (ColorSize = colorSize) + (BorderSize = borderSize) * 2) * PaletteLengthOnEitherDimension);
            ImageImage = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ColorsSelected = new bool[Palette.PaletteSize];

            MouseDown += Clicked;
            MouseDoubleClick += DoubleClicked;
        }

        private int getSelectedColor(MouseEventArgs e)
        {
            return (e.X / ColorTotalSize) + (e.Y / ColorTotalSize * PaletteLengthOnEitherDimension);
        }
        private void Clicked(object sender, MouseEventArgs e)
        {
            int colorSelected = getSelectedColor(e);
            SetSelected(
                Enumerable.Range(
                    colorSelected & ~(NumberOfColorsToSelectAtATime - 1),
                    NumberOfColorsToSelectAtATime
                ).ToArray(),
                !ColorsSelected[colorSelected]
            );
        }
        private void DoubleClicked(object sender, MouseEventArgs e)
        {
            int colorSelected = getSelectedColor(e);
            ColorsSelected[colorSelected] = true;

            ColorDialog colorDialog1 = new ColorDialog();
            colorDialog1.Color = Palette.Convert(Palette[colorSelected]);
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var color = Palette.Convert(colorDialog1.Color);
                List<int> numbersOfSelectedColors = new List<int>();
                for (int i = 0; i < Palette.PaletteSize; ++i)
                    if (ColorsSelected[i])
                    {
                        Palette[i] = color;
                        numbersOfSelectedColors.Add(i);
                    }
                if (numbersOfSelectedColors.Count == 1) //only this one
                    ColorsSelected[colorSelected] = false;
                Update(numbersOfSelectedColors.ToArray());
            }
        }

        public bool[] ColorsSelected { get; }
        private void Update(int[] colorsToUpdate)
        {
            using (Graphics g = Graphics.FromImage(ImageImage))
            {
                using (Brush transparent = new SolidBrush(BackColor))
                    foreach (var colorToUpdate in colorsToUpdate) {
                        var color = Palette[colorToUpdate];
                        int left = (colorToUpdate % PaletteLengthOnEitherDimension) * ColorTotalSize;
                        int top = (colorToUpdate / PaletteLengthOnEitherDimension) * ColorTotalSize;
                        g.FillRectangle(ColorsSelected[colorToUpdate] ? Brushes.Black : transparent, new Rectangle(left, top, ColorTotalSize, ColorTotalSize)); //border
                        using (Brush brush = new SolidBrush(Color.FromArgb(color[0], color[1], color[2])))
                                g.FillRectangle(brush, new Rectangle(left + BorderSize, top + BorderSize, ColorSize, ColorSize)); //fill
                    }
                Image = ImageImage;
            }

            /*ImageData = ImageImage.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Bytes = new byte[ImageData.Height * ImageData.Stride];
            {
                System.Drawing.Drawing2D.Rect
            }
            Marshal.Copy(Bytes, 0, ImageData.Scan0, Bytes.Length);
            ImageImage.UnlockBits(ImageData);
            Image = ImageImage;*/
        }
        public void SetSelected(int[] colorsToSelect, bool selecting)
        {
            foreach (var colorToSelect in colorsToSelect)
                ColorsSelected[colorToSelect] = selecting;
            Update(colorsToSelect);
        }
    }
}
