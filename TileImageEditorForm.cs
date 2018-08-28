using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
        static byte[] ClipboardImage = null, ClipboardMask = null;
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
            for (int x = 0; x < 32; ++x)
                for (int y = 0; y < 32; ++y)
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
            int imageDimensions = isImage ? 8 : 5;
            int x = (e.X - picture.AutoScrollOffset.X) / imageDimensions;
            int y = (e.Y - picture.AutoScrollOffset.Y) / imageDimensions;
            int xy = y * (isImage ? 32 : 16) + x;
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
            for (int i = 0; i < 32 * 32; ++i)
                if (Image[i] == src)
                {
                    PixelsBeingChanged.Add(new ChangedPixel(Image[i], i));
                    Image[i] = dst;
                    DrawColor(i & 31, i >> 5);
                }
            UndoBuffer.Push(PixelsBeingChanged);
        }
        private void Fill(Point loc, byte color, byte colorToFill, List<ChangedPixel> PixelsBeingChanged)
        {
            List<Point> Points = new List<Point> { loc };
            var DrawColor = Colors[color];
            using (Graphics g = Graphics.FromImage(ImageImage))
                for (int i = 0; i < Points.Count; ++i)
                {
                    Point point = Points[i];
                    int xy = point.X | (point.Y << 5);
                    if (Image[xy] == colorToFill) //hasn't already been drawn to
                    {
                        if (point.X > 0 && Image[xy - 1] == colorToFill)
                            Points.Add(new Point(point.X - 1, point.Y));
                        if (point.X < 31 && Image[xy + 1] == colorToFill)
                            Points.Add(new Point(point.X + 1, point.Y));
                        if (point.Y > 0 && Image[xy - 32] == colorToFill)
                            Points.Add(new Point(point.X, point.Y - 1));
                        if (point.Y < 31 && Image[xy + 32] == colorToFill)
                            Points.Add(new Point(point.X, point.Y + 1));
                        PixelsBeingChanged.Add(new ChangedPixel(Image[xy], xy));
                        Image[xy] = color;
                        g.FillRectangle(DrawColor, new Rectangle(point.X * 8, point.Y * 8, 8, 8));
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
            Image = Enumerable.Range(0, 32*32).Select(val => Image[(val & ~31) | (31 - (val & 31))]).ToArray();
            DrawImage();
        }

        private void flipVerticallyIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeEntireImageUndoable();
            Image = Enumerable.Range(0, 32 * 32).Select(val => Image[(val & 31) | (32*31 - (val & ~31))]).ToArray();
            DrawImage();
        }

        private void rotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeEntireImageUndoable();
            Image = Enumerable.Range(0, 32 * 32).Select(val => Image[(32*31 - ((val & 31) << 5)) | (val >> 5)]).ToArray();
            DrawImage();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EditingImage)
                ClipboardImage = Image.Clone() as byte[];
            else
                ClipboardMask = Image.Clone() as byte[];
            pasteToolStripMenuItem.Enabled = true;
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeEntireImageUndoable();
            byte[] clipboard = EditingImage ? ClipboardImage : ClipboardMask;
            for (int i = 0; i < 32 * 32; ++i)
                if (clipboard[i] != 0)
                    Image[i] = clipboard[i];
            DrawImage();
        }

        private void pasteUnderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeEntireImageUndoable();
            for (int i = 0; i < 32 * 32; ++i)
                if (Image[i] == 0)
                    Image[i] = ClipboardImage[i];
            DrawImage();
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
                g.FillRectangle(Colors[Image[x + y * 32]], new Rectangle(x * 8, y * 8, 8, 8));
            pictureBox1.Image = ImageImage;
        }

        bool EditingImage;

        internal bool ShowForm(ref byte[] image, byte[] originalImage, Palette palette)
        {
            if (EditingImage = (palette != null)) //image
            {
                PaletteImage original = new PaletteImage(5, 0, true, false);
                original.Palette = palette;
                original.Location = new Point(OKButton.Location.X, ButtonCancel.Location.Y + (ButtonCancel.Location.Y - OKButton.Location.Y));
                original.MouseLeave += control_MouseLeave;
                original.MouseMove += pictureBox_MouseMove;
                original.MouseDown += pictureBox_MouseDown;
                Controls.Add(original);

                for (uint i = 0; i < Palette.PaletteSize; ++i)
                    Colors[i] = new SolidBrush(Palette.Convert(palette.Colors[i]));

                PrimaryColor = SecondaryColor = 0;

                FormTitle = "Edit Tile Image";
            }
            else //mask
            {
                Colors[0] = new SolidBrush(BackColor);
                Colors[1] = new SolidBrush(Color.Black);

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

            pasteToolStripMenuItem.Enabled = (EditingImage ? ClipboardImage : ClipboardMask) != null;
            pasteUnderToolStripMenuItem.Enabled = (EditingImage) && (ClipboardImage != null); //no use in pasting under a mask

            ShowDialog();

            foreach (var brush in Colors)
                if (brush != null)
                    brush.Dispose();

            if (result && (image == null || !Image.SequenceEqual(image)))
            {
                image = (Image.SequenceEqual(originalImage)) ? null : Image;
                return true;
            }

            return false;
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
                    DrawColor(xy & 31, xy >> 5);
                }
                b.Push(newRecord);
            }
        }

        void MakeEntireImageUndoable()
        {
            RedoBuffer.Clear();
            var PixelsBeingChanged = new List<ChangedPixel>();
            for (int i = 0; i < 32*32; ++i)
                PixelsBeingChanged.Add(new ChangedPixel(Image[i], i));
            UndoBuffer.Push(PixelsBeingChanged);
        }
    }
}
