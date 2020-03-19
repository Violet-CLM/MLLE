using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MLLE
{
    //uses https://www.codeproject.com/Articles/31105/A-ComboBox-with-a-CheckedListBox-as-a-Dropdown
    public partial class SmartTilesForm : Form
    {
        J2TFile Tileset;
        DialogResult Result = DialogResult.Cancel;
        SmartTile WorkingSmartTile;
        List<string> AllSmartTileNames;
        ushort AndValue;
        public SmartTilesForm()
        {
            InitializeComponent();
        }
        internal DialogResult ShowForm(SmartTile workingSmartTile, J2TFile tileset, int workingSmartTileIndex = -1)
        {
            Tileset = tileset;
            WorkingSmartTile = workingSmartTile;
            List<SmartTile> smartTiles = tileset.SmartTiles;
            if (smartTiles.Count >= 1)
                AllSmartTileNames = smartTiles.Select(smartTile => smartTile.Name).ToList();
            else
                AllSmartTileNames = new List<string>();
            if (workingSmartTileIndex == -1)
            {
                AllSmartTileNames.Add("[this]");
                button1.Hide();
            }
            if (smartTiles.Count > 1)
            {
                for (int otherSmartTileID = 0; otherSmartTileID < smartTiles.Count; ++otherSmartTileID)
                {
                    checkedComboBox1.Items.Add(
                        otherSmartTileID != workingSmartTileIndex ? AllSmartTileNames[otherSmartTileID] : "[this]",
                        otherSmartTileID == workingSmartTileIndex ?
                            CheckState.Indeterminate :
                            workingSmartTile.Friends.Contains(otherSmartTileID) ?
                                CheckState.Checked :
                                CheckState.Unchecked
                    );
                }
                checkedComboBox1.SetItemCheckState(0, checkedComboBox1.GetItemCheckState(0)); //fixes issue of control not updating text preview in response to Items.Add
                checkedComboBox1.ItemCheck += (s, e) => { if (e.CurrentValue == CheckState.Indeterminate) e.NewValue = CheckState.Indeterminate; }; //don't let the indeterminate item (this smarttile itself) be altered
            }
            else
                checkedComboBox1.Hide();
            AndValue = ((SmartTile.ushortComparer)WorkingSmartTile.TilesICanPlace.Comparer).AndValue;
            textBox1.Text = WorkingSmartTile.Name;
            CreateImageFromTileset();
            
            using (new System.Threading.Timer(RedrawTiles, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0.5)))
                ShowDialog();
            return Result;
        }

        int elapsed = 0;
        static Point PointFromTileID(int tileID) {
            return new Point((tileID % 10) * 32, (tileID / 10) * 32);
        }
        private void RedrawTiles(object state)
        {
            lock (smartPicture)
                lock (tilesetPicture)
                {
                    var image = smartPicture.Image;
                    using (Graphics graphics = Graphics.FromImage(image))
                        for (int i = 0; i < WorkingSmartTile.Assignments.Length; ++i)
                        {
                            var tiles = WorkingSmartTile.Assignments[i].Tiles;
                            if (tiles.Count > 0)
                                DrawTilesetTileAt(graphics, PointFromTileID(i), tiles[elapsed % tiles.Count]);
                        }
                    smartPicture.Image = image;
                }

            ++elapsed;
        }

        private void CreateImageFromTileset()
        {
            //there are enough windows that show tileset images you'd think I should turn some/all of this into a method somewhere
            tilesetPicture.Height = (int)(Tileset.TotalNumberOfTiles + 9) / 10 * 32;
            var image = new Bitmap(tilesetPicture.Width, tilesetPicture.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            byte[] bytes = new byte[data.Height * data.Stride];
            //Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            for (uint i = 0; i < Tileset.TotalNumberOfTiles; ++i)
            {
                var tileImage = Tileset.Images[Tileset.ImageAddress[i]];
                var xOrg = (i % 10) * 32;
                var yOrg = i / 10 * 32;
                for (uint x = 0; x < 32; ++x)
                    for (uint y = 0; y < 32; ++y)
                    {
                        bytes[xOrg + x + (yOrg + y) * data.Stride] = tileImage[x + y*32];
                    }
            }
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            image.UnlockBits(data);

            var palette = image.Palette;
            var entries = palette.Entries;
            entries[0] = TexturedJ2L.TranspColor;
            for (uint i = 1; i < Palette.PaletteSize; ++i)
                entries[i] = Palette.Convert(Tileset.Palette.Colors[i]);
            image.Palette = palette;

            tilesetPicture.Image = image;
            smartPicture.Image = Properties.Resources.SmartTilesPermutations;
            framesPicture.Image = new Bitmap(32, 32);
        }
        
        private void OKButton_Click(object sender, EventArgs e)
        {
            if (
                !WorkingSmartTile.Assignments[11].Empty ||
                !WorkingSmartTile.Assignments[14].Empty ||
                !WorkingSmartTile.Assignments[47].Empty
            )
            {
                Result = DialogResult.OK;
                WorkingSmartTile.Name = textBox1.Text;
                WorkingSmartTile.Friends.Clear();
                for (int i = 0; i < checkedComboBox1.Items.Count; ++i)
                    if (checkedComboBox1.GetItemCheckState(i) == CheckState.Checked) //indeterminate doesn't count
                        WorkingSmartTile.Friends.Add(i);
            } else {
                if (MessageBox.Show("To define a Smart Tile you must pick at least one tile for at least one of the following default permutation IDs: 11, 14, or 47.", "Insufficient Definition", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    return;
                //else fall through:
            }
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
        
        int GetMouseTileIDFromTileset(MouseEventArgs e)
        {
            var pictureOrigin = tilesetPicture.AutoScrollOffset;
            return ((e.X - pictureOrigin.X) / 32 + (e.Y - pictureOrigin.Y) / 32 * 10);
        }
        Point GetPointFromTileID(int tileID, int xAdjust)
        {
            return new Point(
                tilesetPicture.Left + (tileID % 10) * 32,
                tilesetPicture.Top + (tileID / 10) * 32
            );
        }


        private void tilesetPicture_MouseMove(object sender, MouseEventArgs e)
        {
            Text = "Define Smart Tiles \u2013 " + GetMouseTileIDFromTileset(e);
        }
        private void smartPicture_MouseMove(object sender, MouseEventArgs e)
        {
            Text = "Define Smart Tiles \u2013 " + (e.X / 32 + e.Y / 32 * 10);
        }
        private void tilesetPicture_MouseLeave(object sender, EventArgs e)
        {
            Text = "Define Smart Tiles";
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);
        private void tilesetPicture_MouseClick(object sender, MouseEventArgs e)
        {
            if (CurrentSmartTileID >= 0 && CurrentSmartTileID < WorkingSmartTile.Assignments.Length)
            {
                ushort newTileID = (ushort)GetMouseTileIDFromTileset(e);
                var tiles = WorkingSmartTile.Assignments[CurrentSmartTileID].Tiles;
                if (e.Button == MouseButtons.Left)
                {
                    if ((GetKeyState((int)Keys.F) & 0x8000) != 0)
                        newTileID |= (ushort)(AndValue + 1);
                    if ((GetKeyState((int)Keys.I) & 0x8000) != 0)
                        newTileID |= 0x2000;
                    if (CurrentRuleID < 0)
                        tiles.Add(newTileID);
                    else
                    {
                        Panel rulePanel = (panel4.Controls[panel4.Controls.Count - 1 - CurrentRuleID] as Panel);
                        SmartTile.Rule rule = WorkingSmartTile.Assignments[CurrentSmartTileID].Rules[CurrentRuleID];
                        if (rule.Result.Count < 8)
                        {
                            rule.Result.Add(newTileID);
                            UpdateRuleResultImages(CurrentRuleID);
                        }
                        return;
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (tiles.RemoveAll(potentialTileIDToRemove => (potentialTileIDToRemove & AndValue) == (newTileID & AndValue)) == 0)
                        return;
                }
                else
                    return;
                elapsed = tiles.Count - 1;
                RedrawTiles(null);
                UpdateFramesPreview();
            }
        }

        int CurrentSmartTileID = -1;
        int CurrentRuleID = -1;
        private void smartPicture_MouseClick(object sender, MouseEventArgs e)
        {
            int newSmartTileID = e.X / 32 + e.Y / 32 * 10;
            if (SmartTile.AlternativeAssignments[newSmartTileID] != null) //not an empty space in the image
            {
                HighlightPanel.Visible = true;
                HighlightPanel.Location = new Point((e.X & ~31) + 11, (e.Y & ~31) + 11); //11 is half of (32-10), and the highlight is 10x10
                CurrentSmartTileID = newSmartTileID;

                if (e.Button == MouseButtons.Right)
                {
                    WorkingSmartTile.Assignments[CurrentSmartTileID].Rules.Clear();
                    WorkingSmartTile.Assignments[CurrentSmartTileID].Tiles.Clear();
                }

                UpdateRules();
                UpdateFramesPreview();
            }
        }

        static readonly Size TileSize = new Size(32, 32);
        static readonly Rectangle RectangleAtOrigin = new Rectangle(0, 0, 32, 32);
        void DrawTilesetTileAt(Graphics graphics, Point dest, int tileID)
        {
            int flipFlags = (tileID >> 12) & 3;
            if (AndValue == (1024 - 1) && ((tileID & 1024) == 1024))
                flipFlags |= 1;
            switch (flipFlags) {
                case 0:
                    graphics.TranslateTransform(dest.X, dest.Y);
                    break;
                case 1:
                    graphics.TranslateTransform(dest.X + 32, dest.Y);
                    graphics.ScaleTransform(-1, 1);
                    break;
                case 2:
                    graphics.TranslateTransform(dest.X, dest.Y + 32);
                    graphics.ScaleTransform(1, -1);
                    break;
                case 3:
                    graphics.TranslateTransform(dest.X + 32, dest.Y + 32);
                    graphics.ScaleTransform(-1, -1);
                    break;
            }
            graphics.DrawImage(tilesetPicture.Image, RectangleAtOrigin, new Rectangle(PointFromTileID(tileID & AndValue), TileSize), GraphicsUnit.Pixel);
            graphics.ResetTransform();
        }

        void UpdateFramesPreview()
        {
            if (CurrentSmartTileID >= 0 && CurrentSmartTileID < WorkingSmartTile.Assignments.Length)
            {
                var frames = WorkingSmartTile.Assignments[CurrentSmartTileID].Tiles;
                framesPicture.Height = frames.Count * 32;
                if (frames.Count > 0)
                {
                    lock (tilesetPicture)
                    {
                        var image = new Bitmap(32, framesPicture.Height);
                        using (Graphics graphics = Graphics.FromImage(image))
                            for (int i = 0; i < frames.Count; ++i)
                                DrawTilesetTileAt(graphics, new Point(0, i * 32), frames[i]);
                        framesPicture.Image = image;
                    }
                }
                else
                {
                    lock (smartPicture)
                    {
                        var image = smartPicture.Image;
                        var rectangle = new Rectangle(PointFromTileID(CurrentSmartTileID), TileSize);
                        using (Graphics graphics = Graphics.FromImage(image))
                            graphics.DrawImage(Properties.Resources.SmartTilesPermutations, rectangle, rectangle, GraphicsUnit.Pixel);
                        smartPicture.Image = image;
                    }
                }
            }
            else framesPicture.Height = 0;
        }

        private void framesPicture_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var y = e.Y - framesPicture.AutoScrollOffset.Y;
                if (y < framesPicture.Height) //just to be safe
                {
                    WorkingSmartTile.Assignments[CurrentSmartTileID].Tiles.RemoveAt(y / 32);
                    UpdateFramesPreview();
                }
            }
        }

        void UpdateRules()
        {
            panel4.AutoScrollPosition = new Point(0, 0);
            for (int i = panel4.Controls.Count - 1; i >= 0; --i)
                if (panel4.Controls[i] is Panel)
                    panel4.Controls.RemoveAt(i);
            CurrentRuleID = -1;
            foreach (SmartTile.Rule rule in WorkingSmartTile.Assignments[CurrentSmartTileID].Rules)
            {
                AddRule(rule);
                UpdateRuleResultImages(panel4.Controls.Count - 2);
            }
        }

        private void AddRuleButton_Click(object sender, EventArgs e)
        {
            DeselectAllRules();
            var newRule = new SmartTile.Rule();
            WorkingSmartTile.Assignments[CurrentSmartTileID].Rules.Add(newRule);
            AddRule(newRule);
        }

        static readonly System.Text.RegularExpressions.Regex SpecificTilesValidationPattern = new System.Text.RegularExpressions.Regex(@"^(?:\s*\d{1,4}\s*)(?:,\s*\d{1,4})*\s*$");
        void AddRule(SmartTile.Rule rule)
        {
            Panel panel = new Panel();
            panel.Tag = rule;
            panel.Size = new Size(panel4.ClientSize.Width, 40);
            panel.Dock = DockStyle.Top;

            Label ifLabel = new Label();
            ifLabel.Text = "if";
            ifLabel.Location = new Point(14, 12);
            ifLabel.AutoSize = true;

            PictureBox grid = new PictureBox();
            grid.Size = new Size(40, 40);
            grid.Location = new Point(32, 0);
            grid.MouseClick += (s, e) =>
            {
                rule.X = e.X / 8 - 2;
                rule.Y = e.Y / 8 - 2;
                UpdateRuleGrid(grid);
            };

            CheckBox notIn = new CheckBox();
            notIn.Text = "not in";
            notIn.Location = new Point(90, 12);
            notIn.AutoSize = true;
            notIn.Checked = rule.Not;
            notIn.CheckedChanged += (s, e) => { rule.Not = notIn.Checked; };

            TextBox specificTileCriteria = new TextBox();
            specificTileCriteria.Location = new Point(265, 10);
            specificTileCriteria.Text = string.Join(",", rule.SpecificTiles.Select(number => number.ToString()));
            specificTileCriteria.Visible = rule.OtherSmartTileID < 0;
            specificTileCriteria.LostFocus += (s, e) => {
                bool valid = SpecificTilesValidationPattern.Match(specificTileCriteria.Text).Success;
                specificTileCriteria.BackColor = valid ? Color.White : Color.Pink;
                if (valid)
                    rule.SpecificTiles = specificTileCriteria.Text.Split(',').Select(numbers => ushort.Parse(numbers.Trim())).ToList();
            };

            ComboBox criteriaSource = new ComboBox();
            criteriaSource.Items.AddRange(AllSmartTileNames.ToArray());
            criteriaSource.Items.Add("specific tiles:");
            criteriaSource.Location = new Point(150, 10);
            criteriaSource.Width = 105;
            criteriaSource.DropDownStyle = ComboBoxStyle.DropDownList;
            criteriaSource.SelectedIndex = rule.OtherSmartTileID >= 0 ? rule.OtherSmartTileID : criteriaSource.Items.Count - 1;
            criteriaSource.SelectedIndexChanged += (s, e) => {
                int newIndex = criteriaSource.SelectedIndex;
                if (newIndex == criteriaSource.Items.Count - 1)
                {
                    rule.OtherSmartTileID = -1;
                    specificTileCriteria.Show();
                } else {
                    rule.OtherSmartTileID = newIndex;
                    specificTileCriteria.Hide();
                }
            };

            Label andOrThen = new Label();
            andOrThen.Location = new Point(385, 12);
            andOrThen.AutoSize = true;
            andOrThen.Text = "and...";

            PictureBox results = new PictureBox();
            results.Location = new Point(420, 4);
            results.Size = new Size(256, 32);

            Button deleteRuleButton = new Button();
            deleteRuleButton.Text = "X";
            deleteRuleButton.Location = new Point(684, 4);
            deleteRuleButton.Size = new Size(32, 32);
            deleteRuleButton.Click += (s, e) => {
                WorkingSmartTile.Assignments[CurrentSmartTileID].Rules.Remove(panel.Tag as SmartTile.Rule);
                panel4.Controls.Remove(panel);
                DeselectAllRules();
            };

            panel.Controls.AddRange(new Control[]{ ifLabel, grid, notIn, criteriaSource, specificTileCriteria, andOrThen, results, deleteRuleButton });

            for (int vDelta = -1; vDelta <= 1; vDelta += 2)
            {
                Button adjustOrderButton = new Button();
                adjustOrderButton.Text = (vDelta == -1) ? "^" : "v";
                adjustOrderButton.Location = new Point(716, 12 + vDelta * 8);
                adjustOrderButton.Size = new Size(16, 16);
                int localVDelta = vDelta;
                adjustOrderButton.Click += (s, e) => {
                    var rules = WorkingSmartTile.Assignments[CurrentSmartTileID].Rules;
                    var index = rules.FindIndex(r => r == rule);
                    if (localVDelta == -1) {
                        if (index == 0)
                            return;
                    } else {
                        if (index == rules.Count - 1)
                            return;
                    }
                    SmartTile.Rule otherRule = rules[index + localVDelta];
                    rules[index + localVDelta] = rule;
                    rules[index] = otherRule;
                    panel4.Controls.SetChildIndex(panel, panel4.Controls.GetChildIndex(panel) - localVDelta);
                };
                panel.Controls.Add(adjustOrderButton);
            }

            foreach (Control control in panel.Controls)
            {
                control.MouseClick += (s, e) => {
                    DeselectAllRules();
                    (s as Control).Parent.BackColor = Color.White;
                    CurrentRuleID = WorkingSmartTile.Assignments[CurrentSmartTileID].Rules.FindIndex(r => r == rule);
                    if (s is PictureBox && e.Button == MouseButtons.Right)
                    {
                        var X = e.X / 32;
                        if (X < rule.Result.Count)
                        {
                            rule.Result.RemoveAt(X);
                            UpdateRuleResultImages(CurrentRuleID);
                        }
                    }
                };
            }
            UpdateRuleGrid(grid);
            panel4.Controls.Add(panel);
            panel4.Controls.SetChildIndex(panel, 1);
        }

        void DeselectAllRules()
        {
            foreach (Control possibleRulePanel in panel4.Controls)
                possibleRulePanel.BackColor = System.Drawing.SystemColors.Control;
            CurrentRuleID = -1;
        }

        void UpdateRuleResultImages(int ruleID)
        {
            var frames = WorkingSmartTile.Assignments[CurrentSmartTileID].Rules[ruleID].Result;
            Panel rulePanel = (panel4.Controls[panel4.Controls.Count - 1 - ruleID] as Panel);
            PictureBox picture = rulePanel.Controls[6] as PictureBox;
            var image = new Bitmap(256, 32);
            if (frames.Count > 0)
                lock (tilesetPicture)
                    using (Graphics graphics = Graphics.FromImage(image))
                        for (int i = 0; i < frames.Count; ++i)
                            DrawTilesetTileAt(graphics, new Point(i * 32, 0), frames[i]);
            picture.Image = image;
            (rulePanel.Controls[5] as Label).Text = (frames.Count == 0) ? "and..." : "then:";
        }

        static readonly Brush GridBrush = new SolidBrush(Color.Red);
        void UpdateRuleGrid(PictureBox grid)
        {
            Image newGrid = Properties.Resources.RuleGrid;
            SmartTile.Rule rule = grid.Parent.Tag as SmartTile.Rule;
            using (Graphics g = Graphics.FromImage(newGrid))
                g.FillRectangle(GridBrush, 16 + rule.X * 8, 16 + rule.Y * 8, 8, 8);
            grid.Image = newGrid;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Really delete this Smart Tile? This action cannot be undone.", "Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) == DialogResult.Yes)
            {
                Result = DialogResult.Abort;
                Dispose();
            }
        }
    }
}
