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
        void DrawColor(int x, int y)
        {
            using (Graphics g = Graphics.FromImage(ImageImage))
                g.FillRectangle(Colors[Image[x + y * 32]], new Rectangle(x * 8, y * 8, 8, 8));
            pictureBox1.Image = ImageImage;
        }

        internal bool ShowForm(ref byte[] image, byte[] originalImage, Palette palette)
        {
            if (palette != null)
            {
                PaletteImage original = new PaletteImage(5, 0, true, false);
                original.Palette = palette;
                original.Location = new Point(OKButton.Location.X, ButtonCancel.Location.Y + (ButtonCancel.Location.Y - OKButton.Location.Y));
                Controls.Add(original);
                for (uint i = 0; i < Palette.PaletteSize; ++i)
                    Colors[i] = new SolidBrush(Palette.Convert(palette.Colors[i]));
            }

            Image = (image ?? originalImage).Clone() as byte[];
            OriginalImage = originalImage;
            ImageImage = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            DrawImage();

            ShowDialog();

            if (false)
            {
                image = Image;
                return true;
            }

            return false;
        }
    }
}
