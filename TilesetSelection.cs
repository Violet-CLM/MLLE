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

        private class RatingsImage : PictureBox
        {
            public int Rating; //1-indexed
            public RatingsImage(int initialRating = 3)
            {
                Width = Properties.Resources.Ratings.Width;
                Height = Properties.Resources.Ratings.Height / 5;
                Image = new Bitmap(Width, Height);

                SetRating(initialRating);
            }
            public void SetRating(int newRating)
            {
                if (Rating != newRating)
                {
                    Rating = newRating;

                    using (Graphics g = Graphics.FromImage(Image))
                        g.DrawImage(Properties.Resources.Ratings, new Rectangle(new Point(0, 0), Size), new Rectangle(new Point(0, (Rating - 1) * Height), Size), GraphicsUnit.Pixel);
                    Invalidate();
                }
            }
            public void SetRating(MouseEventArgs e)
            {
                SetRating(e.X / (Width / 5) + 1);
            }
        }
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
        RatingsImage FilterRating = new RatingsImage(), TilesetRating = new RatingsImage();

        PaletteImage TilesetPalette = new PaletteImage(3, 0, true, false);

        public TilesetSelection()
        {
            InitializeComponent();
        }
        internal Mainframe.NameAndFilename ShowForm(Mainframe.NameAndFilename[] files, Color transparentColor)
        {

            TilesetPalette.Location = new Point(buttonCancel.Right - TilesetPalette.Width, buttonCancel.Top - TilesetPalette.Height - 10);
            TilesetPalette.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Controls.Add(TilesetPalette);

            MyFacade.Dock = DockStyle.Fill;
            MyFacade.Tilesets = files;
            MyFacade.MouseMove += MyFacade_MouseMove;
            MyFacade.MouseClick += MyFacade_MouseClick;
            MouseWheel += MyFacade_MouseWheel;
            customDrawHolder.Controls.Add(MyFacade);

            FilterRating.Location = new Point(label1.Right + 16, label1.Top);
            TilesetRating.Location = new Point(labelRating.Left + 2, labelRating.Bottom + 10);
            TilesetRating.Visible = false;
            TilesetRating.Anchor = labelRating.Anchor;

            FilterRating.MouseClick += FilterRating_MouseClick;
            TilesetRating.MouseClick += TilesetRating_MouseClick;

            Controls.Add(FilterRating);
            Controls.Add(TilesetRating);

            label3.Text = ""; //hide until a tileset is chosen

            buttonOkay.Enabled = false;

            new Thread(new ThreadStart(() =>
            {
                string thumbnailFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLLE Thumbnails");
                if (!Directory.Exists(thumbnailFolder))
                    Directory.CreateDirectory(thumbnailFolder);

                foreach (var f in files)
                {
                    try
                    {
                        if (f.Thumbnail is null) //haven't yet looked at this tileset in this window in this session
                        {
                            if (string.IsNullOrEmpty(f.FilterText))
                                f.FilterText = (f.Name + " " + Path.GetFileNameWithoutExtension(f.Filepath)).ToLowerInvariant();
                            if (string.IsNullOrEmpty(f.ThumbnailFilepath)) //probably true in exactly the same cases as the previous if(), but hey
                                f.ThumbnailFilepath = Path.Combine(thumbnailFolder, Path.GetFileNameWithoutExtension(f.Filepath) + "." + f.CRC32.ToString("X8") + ".png");

                            try
                            {
                                f.Thumbnail = new Bitmap(new MemoryStream(File.ReadAllBytes(f.ThumbnailFilepath))); //load from thumbnail cache folder, where it was created a previous time this window was opened (in the same or different MLLE session). https://stackoverflow.com/questions/4803935/free-file-locked-by-new-bitmapfilepath
                                f.Rating = f.Thumbnail.Palette.Entries[2].R;
                                if (f.Rating == 0 || f.Rating > 5) //something went wrong
                                    f.Rating = 3;
                            }
                            catch //cached image does not exist yet
                            {
                                J2TFile tileset = new J2TFile(f.Filepath);

                                var image = new Bitmap(160, 160, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                                var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                                byte[] bytes = new byte[data.Height * data.Stride];
                                uint numberOfTilesToDraw = Math.Min(tileset.TotalNumberOfTiles, 100); //we're drawing a 160x160 area, which is the first 100 tiles, though not all tilesets even have 100 tiles
                                for (uint i = 0; i < numberOfTilesToDraw; ++i)
                                {
                                    var tileImage = tileset.Images[tileset.ImageAddress[i]];
                                    var xOrg = (i % 10) * 16;
                                    var yOrg = i / 10 * 16;
                                    if (tileImage.Length == 32 * 32) //8-bit tile
                                        for (uint x = 0; x < 32; x += 2) //+= 2 because this is a 0.5 size thumbnail
                                            for (uint y = 0; y < 32; y += 2)
                                            {
                                                bytes[xOrg + x / 2 + (yOrg + y / 2) * data.Stride] = tileImage[x + y * 32];
                                            }
                                        else //32-bit tile
                                            for (uint x = 0; x < 32; x += 2)
                                                for (uint y = 0; y < 32; y += 2)
                                                {
                                                    bytes[xOrg + x / 2 + (yOrg + y / 2) * data.Stride] = tileset.Palette.FindNearestColor(new ArraySegment<byte>(tileImage, (int)(x * 4 + y * 128), 3).ToArray());
                                                }
                                }
                                Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
                                image.UnlockBits(data);
                                //save some metadata in Colors[2], which should never be used in any tilesets, and even if it is, this is just the preview thumbnail, so we should be fine
                                tileset.Palette.Colors[2][0] = f.Rating; //3, may be changed later
                                tileset.Palette.Colors[2][1] = (byte)(tileset.TileCount);
                                tileset.Palette.Colors[2][2] = (byte)(tileset.TileCount >> 8);
                                tileset.Palette.Apply(image, transparentColor);
                                f.Thumbnail = image;
                                f.CRC32 = tileset.Crc32; //should be the same...

                                image.Save(f.ThumbnailFilepath, ImageFormat.Png); //loading from PNG is much faster than loading from J2T, so cache this for later
                            }
                        }

                        string filter;
                        lock (textlock)
                        {
                            filter = textBox1.Text.Trim();
                        }
                        if (filter != string.Empty)
                            f.Show = f.Rating >= FilterRating.Rating && f.FilterText.Contains(filter.ToLowerInvariant());
                        else
                            f.Show = f.Rating >= FilterRating.Rating;

                        //lock (gridlock)
                        {
                            MyFacade.NumberOfTilesetsToDraw += 1;
                            if (f.Show)
                                Invoke(new FacadeDelegate(delegate (FacadeControl p) { p.Invalidate(); }), new object[] { MyFacade }); //can't do this if the main thread is stuck on the lock statement in text updated...
                        }

                    } catch {
                        //just skip this one lol
                    }
                }
            })).Start();

            ShowDialog();

            return MyFacade.SelectedTileset;
        }

        private void TilesetRating_MouseClick(object sender, MouseEventArgs e)
        {
            TilesetRating.SetRating(e);
            var currentTileset = MyFacade.SelectedTileset;
            currentTileset.Rating = (byte)TilesetRating.Rating;
            var tilesetPalette = currentTileset.Thumbnail.Palette;
            Color oldColor1 = tilesetPalette.Entries[2];
            tilesetPalette.Entries[2] = Color.FromArgb(currentTileset.Rating, oldColor1.G, oldColor1.B);
            currentTileset.Thumbnail.Palette = tilesetPalette;
            currentTileset.Thumbnail.Save(currentTileset.ThumbnailFilepath, ImageFormat.Png);
        }

        private void FilterRating_MouseClick(object sender, MouseEventArgs e)
        {
            FilterRating.SetRating(e);
            textBox1_TextChanged(null, null);
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
            Color tilesetMetadata = MyFacade.SelectedTileset.Thumbnail.Palette.Entries[2];
            label3.Text = MyFacade.SelectedTileset.Name + "\n" + Path.GetFileName(MyFacade.SelectedTileset.Filepath) + "\n" + (tilesetMetadata.G | (tilesetMetadata.B << 8)).ToString() + " tiles";
            TilesetPalette.Palette = new Palette(MyFacade.SelectedTileset.Thumbnail.Palette);
            TilesetPalette.Update(PaletteImage.AllPaletteColors);
            TilesetRating.SetRating(newSelectedTileset.Rating);
            buttonOkay.Enabled = true;
            TilesetRating.Visible = true;
            labelRating.Visible = true;
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
            if (string.IsNullOrEmpty(filter))
            {
                //lock (gridlock)
                {
                    for (int tilesetID = 0; tilesetID < MyFacade.NumberOfTilesetsToDraw; ++tilesetID)
                    {
                        var tileset = MyFacade.Tilesets[tilesetID];
                        tileset.Show = tileset.Rating >= FilterRating.Rating;
                    }
                }
            }
            else
            {
                filter = filter.ToLowerInvariant();
                
                //lock (gridlock)
                {
                    for (int tilesetID = 0; tilesetID < MyFacade.NumberOfTilesetsToDraw; ++tilesetID)
                    {
                        var tileset = MyFacade.Tilesets[tilesetID];
                        tileset.Show = tileset.Rating >= FilterRating.Rating && tileset.FilterText.Contains(filter);
                    }
                }
            }

            if (MyFacade.SelectedTileset != null && !MyFacade.SelectedTileset.Show) //the selected tileset was filtered out of the search results
            {
                MyFacade.SelectedTileset = null;
                TilesetPalette.Image = null;
                buttonOkay.Enabled = false;
                label3.ResetText();
                TilesetRating.Visible = false;
                labelRating.Visible = false;
            }
            MyFacade.FirstTilesetToDraw = 0; //simplest
            MyFacade.Invalidate();
        }

        private void textBox1_MouseEnter(object sender, EventArgs e)
        {
            (sender as Control).Focus();
        }

        private void buttonOkay_Click(object sender, EventArgs e)
        {
            if (MyFacade.SelectedTileset != null) //should always be true, but let's play it safe
            {
                Dispose();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            MyFacade.SelectedTileset = null;
            Dispose();
        }
    }
}
