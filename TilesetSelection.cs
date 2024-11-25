using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLLE
{
    public partial class TilesetSelection : Form
    {
        private delegate void PanelDelegate(Panel p);
        private delegate void FacadeDelegate(FacadeControl p);
        private delegate void PanelFlowDelegate(Panel p, FlowLayoutPanel f);
        private delegate void PanelStringDelegate(Panel p, string f);

        private Object
           // gridlock = new Object(),
            textlock = new Object();
        private class FacadeControl : Control
        {
            private SolidBrush invalidPen, hoveringPen, selectedPen, labelPen;
            internal int NumberOfTilesetsToDraw = 0;
            internal int FirstTilesetToDraw = 0;
            internal Mainframe.NameAndFilename[] Tilesets;
            internal Mainframe.NameAndFilename SelectedTileset = null;

            internal int MaximumTilesetsThatCanBeDrawnAtOnce() //8 by default, but the user may resize the window
            {
                return (Width / 200) * (Height / 200);
            }

            internal struct CurrentlyShownTileset
            {
                internal Rectangle rectangle;
                internal Mainframe.NameAndFilename tileset;

                public CurrentlyShownTileset(Rectangle rectangle, Mainframe.NameAndFilename tileset)
                {
                    this.rectangle = rectangle;
                    this.tileset = tileset;
                }
            }
            internal List<CurrentlyShownTileset> CurrentlyShownTilesets = new List<CurrentlyShownTileset>();

            public FacadeControl()
            {
                invalidPen = new SolidBrush(SystemColors.Control);
                hoveringPen = new SolidBrush(SystemColors.Highlight);
                selectedPen = new SolidBrush(SystemColors.ControlDark);
                labelPen = new SolidBrush(SystemColors.ControlText);
                SetStyle(ControlStyles.ResizeRedraw, true); // make sure the control is redrawn every time it is resized
                DoubleBuffered = true;
            }

            internal void ScrollUp()
            {
                int maxNumberOfTilesetsToGoBackUp = MaximumTilesetsThatCanBeDrawnAtOnce();
                while (true)
                {
                    if (FirstTilesetToDraw == 0) //can't scroll more than this!
                        break;
                    if (Tilesets[--FirstTilesetToDraw].Show)
                    {
                        if (--maxNumberOfTilesetsToGoBackUp == 0)
                            break;
                    }
                }
                Invalidate();
            }
            internal void ScrollDown()
            {
                int maxNumberOfTilesetsToGoDown = MaximumTilesetsThatCanBeDrawnAtOnce() * 2;
                while (true)
                {
                    if (++FirstTilesetToDraw == NumberOfTilesetsToDraw)
                    {
                        break;
                    }
                    if (Tilesets[FirstTilesetToDraw].Show)
                    {
                        if (--maxNumberOfTilesetsToGoDown == 0)
                            break;
                    }
                }
                ScrollUp();
            }

            protected override void OnPaint(PaintEventArgs pe)
            {
                CurrentlyShownTilesets.Clear();

                Graphics g = pe.Graphics;

                Point cursor = PointToClient(MousePosition);

                int xPos = 10, yPos = 10;
                //getFirstTilesetID();
                for (int tilesetID = FirstTilesetToDraw; tilesetID < NumberOfTilesetsToDraw; ++tilesetID) {
                    var tileset = Tilesets[tilesetID];
                    if (!tileset.Show)
                        continue;

                    Rectangle dimensions = new Rectangle(xPos, yPos, 180, 180);
                    CurrentlyShownTilesets.Add(new CurrentlyShownTileset(dimensions, tileset));
                    g.FillRectangle(
                        dimensions.Contains(cursor) ? hoveringPen : tileset == SelectedTileset ? selectedPen : invalidPen,
                        dimensions
                    );
                    g.DrawImageUnscaled(tileset.Thumbnail, xPos + 10, yPos + 10);
                    g.DrawString(tileset.Name, Label.DefaultFont, labelPen, xPos - 7, yPos - 10);
                    if ((xPos += 200) > Width - 190)
                    {
                        xPos = 10;
                        if ((yPos += 200) > Height - 190)
                            break;
                    }
                }
            }
        }
        FacadeControl MyFacade = new FacadeControl();

        PaletteImage TilesetPalette = new PaletteImage(3, 0, true, false);

        public TilesetSelection()
        {
            InitializeComponent();
        }
        internal void ShowForm(Mainframe.NameAndFilename[] files, Color transparentColor)
        {

            TilesetPalette.Location = new Point(buttonCancel.Right - TilesetPalette.Width, buttonCancel.Top - TilesetPalette.Height - 10);
            TilesetPalette.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Controls.Add(TilesetPalette);

            MyFacade.Dock = DockStyle.Fill;
            MyFacade.Tilesets = files;
            MyFacade.MouseEnter += MyFacade_MouseEnter;
            MyFacade.MouseMove += MyFacade_MouseMove;
            MyFacade.MouseClick += MyFacade_MouseClick;
            MouseWheel += MyFacade_MouseWheel;
            customDrawHolder.Controls.Add(MyFacade);

            label3.Text = ""; //hide until a tileset is chosen

            buttonOkay.Enabled = false;

            new Thread(new ThreadStart(() =>
            {
                foreach (var f in files)
                {
                    try
                    {
                        f.FilterText = (f.Name + " " + Path.GetFileNameWithoutExtension(f.Filepath)).ToLowerInvariant();

                        /*Panel panel = new Panel();
                        panel.Size = new Size(180, 180);
                        panel.BackColor = SystemColors.Control;
                        panel.Margin = new Padding(10);

                        PictureBox pictureBox1 = new PictureBox();
                        pictureBox1.Size = new Size(160, 160);
                        pictureBox1.Location = new Point(10, 10);*/

                        J2TFile tileset = new J2TFile(f.Filepath);
                        tileset.Palette.Colors[0] = Palette.Convert(transparentColor);

                        var image = new Bitmap(160, 160, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                        byte[] bytes = new byte[data.Height * data.Stride];
                        uint numberOfTilesToDraw = Math.Min(tileset.TotalNumberOfTiles, 100); //we're drawing a 160x160 area, which is the first 100 tiles, though not all tilesets even have 100 tiles
                        for (uint i = 0; i < numberOfTilesToDraw; ++i)
                        {
                            var tileImage = tileset.Images[tileset.ImageAddress[i]];
                            var xOrg = (i % 10) * 16;
                            var yOrg = i / 10 * 16;
                            for (uint x = 0; x < 32; x += 2) //+= 2 because this is a 0.5 size thumbnail
                                for (uint y = 0; y < 32; y += 2)
                                {
                                    bytes[xOrg + x / 2 + (yOrg + y / 2) * data.Stride] = tileImage[x + y * 32];
                                }
                        }
                        Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
                        image.UnlockBits(data);
                        tileset.Palette.Apply(image);
                        //pictureBox1.Image = image;
                        f.Thumbnail = image;
                        f.CRC32 = tileset.Crc32;

                        /*
                        //pictureBox1.Enabled = false;
                        string tooltipText = f.Name;// + " (" + Path.GetFileNameWithoutExtension(f.Filename) + ")";
                        toolTip1.SetToolTip(pictureBox1, tooltipText);
                        // toolTip1.SetToolTip(panel, tooltipText);
                        panel.Controls.Add(pictureBox1);*/

                        string filter;
                        lock (textlock)
                        {
                            filter = textBox1.Text.Trim();
                        }
                        if (filter != string.Empty)
                            f.Show = f.FilterText.Contains(filter.ToLowerInvariant());
                        else
                            f.Show = true;

                        /*panel.Tag = f.FilterText;
                        pictureBox1.Tag = tileset.Name + "\n" + tileset.FilenameOnly + "\n" + tileset.TileCount.ToString() + " tiles";
                        pictureBox1.MouseEnter += PictureBox_MouseEnter;
                        pictureBox1.MouseLeave += PictureBox_MouseLeave;
                        pictureBox1.MouseClick += PictureBox1_MouseClick;*/

                        //lock (gridlock)
                        {
                            MyFacade.NumberOfTilesetsToDraw += 1;
                            if (f.Show)
                                Invoke(new FacadeDelegate(delegate (FacadeControl p) { p.Invalidate(); }), new object[] { MyFacade }); //can't do this if the main thread is stuck on the lock statement in text updated...
                        }

                    } catch {
                        //just skip this one lol
                    }

                    //if (++fileID == 12) //debug
                    //    break;
                }
            })).Start();

            ShowDialog();
        }

        private void MyFacade_MouseEnter(object sender, EventArgs e)
        {
            MyFacade.Focus(); //for mouse wheel
        }

        private void MyFacade_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta <= -120)
                MyFacade.ScrollDown();
            else if (e.Delta >= 120)
                MyFacade.ScrollUp();
        }

        private void SelectTileset(Mainframe.NameAndFilename newSelectedTileset)
        {
            MyFacade.SelectedTileset = newSelectedTileset;
            label3.Text = MyFacade.SelectedTileset.Name + "\n" + Path.GetFileName(MyFacade.SelectedTileset.Filepath);// + "\n" + tileset.TileCount.ToString() + " tiles";
            TilesetPalette.Palette = new Palette(MyFacade.SelectedTileset.Thumbnail.Palette);
            TilesetPalette.Update(PaletteImage.AllPaletteColors);
            buttonOkay.Enabled = true;
        }

        private void MyFacade_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (var possibleTileset in MyFacade.CurrentlyShownTilesets)
            {
                if (possibleTileset.rectangle.Contains(e.Location)) {
                    SelectTileset(possibleTileset.tileset);
                    break;
                }
            }
        }

        private void MyFacade_MouseMove(object sender, MouseEventArgs e)
        {
            MyFacade.Invalidate();
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            MyFacade.ScrollUp();
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            MyFacade.ScrollDown();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string filter;
            lock (textlock)
            {
                filter = textBox1.Text.Trim();
            }
            if (filter == string.Empty)
            {
                //lock (gridlock)
                {
                    for (int tilesetID = 0; tilesetID < MyFacade.NumberOfTilesetsToDraw; ++tilesetID)
                        MyFacade.Tilesets[tilesetID].Show = true;
                }
            }
            else
            {
                filter = filter.ToLowerInvariant();
                
                if (MyFacade.SelectedTileset != null && !MyFacade.SelectedTileset.FilterText.Contains(filter)) //the selected tileset was filtered out of the search results
                {
                    MyFacade.SelectedTileset = null;
                    TilesetPalette.Image = null;
                    buttonOkay.Enabled = false;
                    label3.ResetText();
                }
                //lock (gridlock)
                {
                    for (int tilesetID = 0; tilesetID < MyFacade.NumberOfTilesetsToDraw; ++tilesetID)
                        MyFacade.Tilesets[tilesetID].Show = MyFacade.Tilesets[tilesetID].FilterText.Contains(filter);
                    MyFacade.FirstTilesetToDraw = 0; //simplest
                }
            }
            MyFacade.Invalidate();
        }

        private void textBox1_MouseEnter(object sender, EventArgs e)
        {
            textBox1.Focus();
        }
    }
}
