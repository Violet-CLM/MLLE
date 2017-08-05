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

        private void ResetButton_Click(object sender, EventArgs e)
        {
            Image = OriginalImage.Clone() as byte[];
            DrawImage();
        }

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

            if (e.Button == MouseButtons.Left)
            {
                if (ModifierKeys == Keys.Control || !isImage)
                    PrimaryColor = color;
                else if (color != PrimaryColor)
                {
                    Image[xy] = PrimaryColor;
                    DrawColor(x, y);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (ModifierKeys == Keys.Control || !isImage)
                    SecondaryColor = color;
                else if (color != SecondaryColor)
                {
                    Image[xy] = SecondaryColor;
                    DrawColor(x, y);
                }
            }

            Text = FormTitle + " \u2013 " + color;
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
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

        void DrawColor(int x, int y)
        {
            using (Graphics g = Graphics.FromImage(ImageImage))
                g.FillRectangle(Colors[Image[x + y * 32]], new Rectangle(x * 8, y * 8, 8, 8));
            pictureBox1.Image = ImageImage;
        }

        internal bool ShowForm(ref byte[] image, byte[] originalImage, Palette palette)
        {
            if (palette != null) //image
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
            }

            Image = (image ?? originalImage).Clone() as byte[];
            OriginalImage = originalImage;
            ImageImage = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            DrawImage();

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
    }
}
