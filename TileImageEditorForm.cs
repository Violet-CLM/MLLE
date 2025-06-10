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
    public partial class TileImageEditorForm : Form
    {
        public TileImageEditorForm()
        {
            InitializeComponent();
        }

        byte[] Image, OriginalImage;
        int DimensionX, DimensionY;
        new int Scale;
        static byte[] ClipboardMask = null;
        SolidBrush[] Colors = new SolidBrush[Palette.PaletteSize];
        Bitmap ImageImage;

        byte _primaryColor, _secondaryColor;
        byte PrimaryColor
        { get
            {
                return _primaryColor;
            } set
            {
                _primaryColor = value;
                panel1.BackColor = Colors[_primaryColor].Color;
            }
        }
        byte SecondaryColor
        {
            get
            {
                return _secondaryColor;
            }
            set
            {
                _secondaryColor = value;
                panel2.BackColor = Colors[_secondaryColor].Color;
            }
        }

        string FormTitle;

        struct ChangedPixel
        {
            public byte Color;
            public int Location;
            public ChangedPixel(byte c, int l) { Color = c; Location = l; }
        }
        Stack<List<ChangedPixel>> UndoBuffer = new Stack<List<ChangedPixel>>(), RedoBuffer = new Stack<List<ChangedPixel>>();

        void DrawImage()
        {
            for (int x = 0; x < DimensionX; ++x)
                for (int y = 0; y < DimensionY; ++y)
                    DrawColor(x, y);
        }

        private void control_MouseLeave(object sender, EventArgs e)
        {
            Text = FormTitle;
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox picture = sender as PictureBox;
            if (!(picture).ClientRectangle.Contains(e.Location))
                return;

            bool isImage = picture == pictureBox1;
            int imageDimensions = isImage ? Scale : 5;
            int x = (e.X - picture.AutoScrollOffset.X) / imageDimensions;
            int y = (e.Y - picture.AutoScrollOffset.Y) / imageDimensions;
            int xy = y * (isImage ? DimensionX : 16) + x;
            byte color = isImage ? Image[xy] : (byte)xy;

            if (e.Button != MouseButtons.None)
            {
                if (!ReplaceColorButton.Checked || ModifierKeys == Keys.Control || !isImage)
                {
                    if ((ModifierKeys == Keys.Control && EditingImage) || !isImage) //eyedropper
                    {
                        if (e.Button == MouseButtons.Left)
                            PrimaryColor = color;
                        else
                            SecondaryColor = color;
                    }
                    else
                    {
                        byte colorToDraw = (e.Button == MouseButtons.Left) ? PrimaryColor : SecondaryColor;
                        if (color != colorToDraw) //otherwise there's no point in doing anything
                        {
                            RedoBuffer.Clear();
                            var PixelsBeingChanged = new List<ChangedPixel>();
                            if (FillButton.Checked)
                                Fill(new Point(x, y), colorToDraw, color, PixelsBeingChanged);
                            else
                            {
                                PixelsBeingChanged.Add(new ChangedPixel(Image[xy], xy));
                                Image[xy] = colorToDraw;
                                DrawColor(x, y);
                            }
                            UndoBuffer.Push(PixelsBeingChanged);
                        }
                    }
                }
            }
            Text = FormTitle + " \u2013 " + color;
        }

        private void ReplaceColor(byte dst, byte src)
        {
            RedoBuffer.Clear();
            var PixelsBeingChanged = new List<ChangedPixel>();
            for (int i = 0; i < Image.Length; ++i)
                if (Image[i] == src)
                {
                    PixelsBeingChanged.Add(new ChangedPixel(Image[i], i));
                    Image[i] = dst;
                    DrawColor(i % DimensionX, i / DimensionX);
                }
            UndoBuffer.Push(PixelsBeingChanged);
        }
        private void Fill(Point loc, byte color, byte colorToFill, List<ChangedPixel> PixelsBeingChanged)
        {
            List<Point> Points = new List<Point> { loc };
            var DrawColor = Colors[color];
            var maxX = DimensionX - 1;
            var maxY = DimensionY - 1;
            using (Graphics g = Graphics.FromImage(ImageImage))
                for (int i = 0; i < Points.Count; ++i)
                {
                    Point point = Points[i];
                    int xy = point.X | (point.Y * DimensionX);
                    if (Image[xy] == colorToFill) //hasn't already been drawn to
                    {
                        if (point.X > 0 && Image[xy - 1] == colorToFill)
                            Points.Add(new Point(point.X - 1, point.Y));
                        if (point.X < maxX && Image[xy + 1] == colorToFill)
                            Points.Add(new Point(point.X + 1, point.Y));
                        if (point.Y > 0 && Image[xy - DimensionX] == colorToFill)
                            Points.Add(new Point(point.X, point.Y - 1));
                        if (point.Y < maxY && Image[xy + DimensionX] == colorToFill)
                            Points.Add(new Point(point.X, point.Y + 1));
                        PixelsBeingChanged.Add(new ChangedPixel(Image[xy], xy));
                        Image[xy] = color;
                        g.FillRectangle(DrawColor, new Rectangle(point.X * Scale, point.Y * Scale, Scale, Scale));
                    }
                }
            pictureBox1.Image = ImageImage;
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (ReplaceColorButton.Checked && ModifierKeys != Keys.Control && (sender as PictureBox) == pictureBox1) //and therefore this must be an image, not a mask
                ReplaceColor(e.Button == MouseButtons.Right ? SecondaryColor : PrimaryColor, e.Button == MouseButtons.Right ? PrimaryColor : SecondaryColor);
            else
                pictureBox_MouseMove(sender, e);
        }

        private void panel_MouseEnter(object sender, EventArgs e)
        {
            Text = FormTitle + " \u2013 " + ((sender == panel1) ? PrimaryColor : SecondaryColor);
        }

        bool result = false;
        private void OKButton_Click(object sender, EventArgs e)
        {
            result = true;
            Dispose();
        }
        
        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeEntireImageUndoable();
            Image = OriginalImage.Clone() as byte[];
            DrawImage();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeEntireImageUndoable();
            Image = new byte[Image.Length];
            DrawImage();
        }

        private void flipHorizontallyFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeEntireImageUndoable();
            Image = Enumerable.Range(0, Image.Length).Select(val => Image[(val / DimensionX * DimensionX) + (DimensionX - 1 - (val % DimensionX))]).ToArray();
            DrawImage();
        }

        private void flipVerticallyIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeEntireImageUndoable();
            int bottomLeftCorner = DimensionX * (DimensionY - 1);
            Image = Enumerable.Range(0, Image.Length).Select(val => Image[(val % DimensionX) + bottomLeftCorner - (val / DimensionX * DimensionX)]).ToArray(); //todo should any of these be DimensionY?
            DrawImage();
        }

        private void rotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DimensionX == DimensionY) //square image, as is typical for tiles and textures (outside of TEXTURE::STATIC layers)
                MakeEntireImageUndoable();
            else
                ResizeCanvas(new Size(DimensionY, DimensionX)); //swap axes

            int bottomLeftCorner = DimensionY * (DimensionX - 1); //because ResizeCanvas was already called, X and Y are swapped from what is logical in these two lines
            Image = Enumerable.Range(0, Image.Length).Select(val => Image[bottomLeftCorner - ((val % DimensionX) * DimensionY) + (val / DimensionX)]).ToArray(); //
            DrawImage();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EditingImage)
            {
                Bitmap bitmap = new Bitmap(DimensionX, DimensionY, PixelFormat.Format8bppIndexed);
                originalPalette.Palette.Apply(bitmap);
                BitmapStuff.ByteArrayToBitmap(Image, bitmap, true);
                BitmapStuff.CopyBitmapToClipboard(bitmap);
            }
            else
            {
                ClipboardMask = Image.Clone() as byte[]; //a lot simpler but limited to this particular application
                pasteToolStripMenuItem.Enabled = true;
            }
        }

        private void ResizeCanvas(Size newSize)
        {
            if (DimensionX != newSize.Width || DimensionY != newSize.Height)
            {
                DimensionX = newSize.Width;
                DimensionY = newSize.Height;
                pictureBox1.Size = newSize;
                ImageImage = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Width = InitialSize.Width + Math.Max(DimensionX - 256, 0);
                Height = InitialSize.Height + Math.Max(DimensionY - 256, 0);
                FormTitle = Height.ToString();
                RedoBuffer.Clear();
                UndoBuffer.Clear();
            }
        }

        private void pasteImage(bool over, bool recolor, bool allowResizing)
        {
            Bitmap pastedBitmap = BitmapStuff.GetBitmapFromClipboard(allowResizing ? (Size?)null : new Size(DimensionX, DimensionY));
            if (pastedBitmap != null) //no errors, everything about the header looks right
            {
                var clipboard = BitmapStuff.BitmapToByteArray(pastedBitmap);

                if (recolor) {
                    byte[] colorRemappings = null;
                    if (new SpriteRecolorForm().ShowForm(originalPalette.Palette, pastedBitmap.Clone() as Bitmap, ref colorRemappings, Color.Black))
                    {
                        for (int i = 0; i < clipboard.Length; ++i)
                            clipboard[i] = colorRemappings[clipboard[i]];
                    }
                    else //user hit Cancel button
                        return;
                }

                if (pastedBitmap.Width == DimensionX && pastedBitmap.Height == DimensionY) //same size as before
                {
                    MakeEntireImageUndoable();
                    if (over)
                    {
                        for (int i = 0; i < clipboard.Length; ++i)
                            if (clipboard[i] > 1)
                                Image[i] = clipboard[i];
                    }
                    else //under
                    {
                        for (int i = 0; i < clipboard.Length; ++i)
                            if (Image[i] == 0)
                                Image[i] = clipboard[i];
                    }
                }
                else
                {
                    ResizeCanvas(pastedBitmap.Size);
                    Image = clipboard; //if the size is different, you're replacing the whole image, not pasting over/under
                }
                DrawImage();
            }

        }
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EditingImage)
                pasteImage(true, false, AllowResizing);
            else
            {
                MakeEntireImageUndoable();
                for (int i = 0; i < ClipboardMask.Length; ++i)
                    if (ClipboardMask[i] != 0)
                        Image[i] = ClipboardMask[i];
                DrawImage();
            }
        }

        private void pasteUnderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pasteImage(false, false, false);
        }
        private void pasteAndRecolorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pasteImage(true, true, AllowResizing);
        }

        private void TileImageEditorForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode) {
                case Keys.F:
                    flipHorizontallyFToolStripMenuItem_Click(null, null);
                    break;
                case Keys.I:
                    flipVerticallyIToolStripMenuItem_Click(null, null);
                    break;
                case Keys.R:
                    rotateToolStripMenuItem_Click(null, null);
                    break;
                case Keys.Delete:
                case Keys.X:
                    clearToolStripMenuItem_Click(null, null);
                    break;
                case Keys.C:
                    if (e.Control)
                    {
                        copyToolStripMenuItem_Click(null, null);
                        break;
                    }
                    else return;
                case Keys.V:
                    if (e.Control && pasteToolStripMenuItem.Enabled)
                    {
                        pasteToolStripMenuItem_Click(null, null);
                        break;
                    }
                    else return;
                case Keys.Z:
                    if (e.Control)
                    {
                        Undo();
                        break;
                    }
                    else return;
                case Keys.Y:
                    if (e.Control)
                    {
                        Redo();
                        break;
                    }
                    else return;
                default:
                    return;
            }
            e.SuppressKeyPress = true;  // Stops other controls on the form receiving event.
        }

        private void PaintbrushButton_Click(object sender, EventArgs e)
        {
            PaintbrushButton.Checked = true;
            FillButton.Checked = ReplaceColorButton.Checked = false;
        }

        private void FillButton_Click(object sender, EventArgs e)
        {
            FillButton.Checked = true;
            PaintbrushButton.Checked = ReplaceColorButton.Checked = false;
        }

        private void ReplaceColorButton_Click(object sender, EventArgs e)
        {
            ReplaceColorButton.Checked = true;
            PaintbrushButton.Checked = FillButton.Checked = false;
        }

        void DrawColor(int x, int y)
        {
            using (Graphics g = Graphics.FromImage(ImageImage))
                g.FillRectangle(Colors[Image[x + y * DimensionX]], new Rectangle(x * Scale, y * Scale, Scale, Scale));
            pictureBox1.Image = ImageImage;
        }

        bool EditingImage;
        bool AllowResizing;
        PaletteImage originalPalette;
        Size InitialSize;
        internal int ShowForm(ref byte[] image, byte[] originalImage, Palette palette, int textureWidth, int scale, bool allowResizing)
        {
            Scale = scale; //8 or 1
            AllowResizing = allowResizing;
            InitialSize = Size;
            if (EditingImage = (palette != null)) //image
            {
                originalPalette = new PaletteImage(5, 0, true, false);
                originalPalette.Palette = palette;
                originalPalette.Location = new Point(OKButton.Location.X, ButtonCancel.Location.Y + (ButtonCancel.Location.Y - OKButton.Location.Y));
                originalPalette.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                originalPalette.MouseLeave += control_MouseLeave;
                originalPalette.MouseMove += pictureBox_MouseMove;
                originalPalette.MouseDown += pictureBox_MouseDown;
                Controls.Add(originalPalette);

                for (uint i = 0; i < Palette.PaletteSize; ++i)
                    Colors[i] = new SolidBrush(Palette.Convert(palette.Colors[i]));

                if (allowResizing)
                {
                    DimensionX = DimensionY = 256;
                    ResizeCanvas(new Size(textureWidth, (image ?? originalImage).Length / textureWidth));
                }
                else
                {
                    DimensionX = DimensionY = pictureBox1.Width / scale;
                }

                PrimaryColor = SecondaryColor = 0;

                FormTitle = "Edit Tile Image";
                resetToolStripMenuItem.Enabled = originalImage != null;
            }
            else //mask
            {
                Colors[0] = new SolidBrush(BackColor);
                Colors[1] = new SolidBrush(Color.Black);

                DimensionX = DimensionY = 32; //all tile masks are the same size

                PrimaryColor = 1;
                SecondaryColor = 0;

                Text = FormTitle = "Edit Tile Mask";

                toolStrip1.Items.Remove(ReplaceColorButton);
            }
            if (image != null)
                Text = (FormTitle += "*");


            Image = (image ?? originalImage).Clone() as byte[];
            OriginalImage = originalImage;
            ImageImage = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            DrawImage();

            pasteToolStripMenuItem.Enabled = EditingImage || ClipboardMask != null;
            pasteUnderToolStripMenuItem.Enabled = EditingImage; //no use in pasting under a mask
            pasteAndRecolorToolStripMenuItem.Enabled = EditingImage;

            ShowDialog();

            foreach (var brush in Colors)
                if (brush != null)
                    brush.Dispose();

            if (result) { //clicked OK
                bool changed = !Image.SequenceEqual(image ?? originalImage);
                if (originalImage != null && Image.SequenceEqual(originalImage))
                    image = null;
                else
                    image = Image;
                return changed ? DimensionX : 0;
            }

            return 0;
        }


        private void toolsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            undoToolStripMenuItem.Enabled = UndoBuffer.Count > 0;
            redoToolStripMenuItem.Enabled = RedoBuffer.Count > 0;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Undo();
        }
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Redo();
        }
        private void Undo()
        {
            SwapBuffers(UndoBuffer, RedoBuffer);
        }
        private void Redo()
        {
            SwapBuffers(RedoBuffer, UndoBuffer);
        }
        private void SwapBuffers(Stack<List<ChangedPixel>> a, Stack<List<ChangedPixel>> b)
        {
            if (a.Count > 0)
            {
                var oldRecord = a.Pop();
                var newRecord = new List<ChangedPixel>();
                foreach (var pixels in oldRecord)
                {
                    int xy = pixels.Location;
                    newRecord.Add(new ChangedPixel(Image[xy], xy));
                    Image[xy] = pixels.Color;
                    DrawColor(xy % DimensionX, xy / DimensionX);
                }
                b.Push(newRecord);
            }
        }

        void MakeEntireImageUndoable()
        {
            RedoBuffer.Clear();
            var PixelsBeingChanged = new List<ChangedPixel>();
            for (int i = 0; i < Image.Length; ++i)
                PixelsBeingChanged.Add(new ChangedPixel(Image[i], i));
            UndoBuffer.Push(PixelsBeingChanged);
        }
    }
}
