using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Extra.Collections;

namespace MLLE
{
    public partial class LayerPropertiesForm : Form
    {
        Mainframe SourceForm;
        byte CurrentLayer;
        Layer DataSource;
        bool ShowLayerSelection;
        int[] newrectangle;
        internal LayerPropertiesForm(Mainframe parent, Layer layer, bool showLayerSelection)
        {
            SourceForm = parent;
            DataSource = layer;
            CurrentLayer = (byte)((showLayerSelection) ? parent.J2L.AllLayers.IndexOf(layer) : 0);
            ShowLayerSelection = showLayerSelection;
            InitializeComponent();
        }
        private void LayerPropertiesForm_Load(object sender, EventArgs e)
        {
            int heightBetweenRows = 26;
            if (SourceForm.TextureTypes.Count == 0)
            {
                TextureMode.Parent = groupBox3;
                TextureMode.Location = new Point(TileHeight.Location.X, LimitVisibleRegion.Location.Y);
                groupBox4.Hide();
                groupBox4.Height = 0;
            }
            else if (SourceForm.TextureTypes.Count == 1)
            {
                TextureModeSelect.Visible = false;
                if (SourceForm.TextureTypes[0][1] == "+") //color, not three number boxes
                    groupBox4.Height -= heightBetweenRows;
            }
            if (!SourceForm.EnableableBools[SourceForm.J2L.VersionType][EnableableTitles.BoolDevelopingForPlus])
            {
                groupBoxPlus.Hide();
                groupBoxPlus.Height = 0;
                groupBox4.Left = groupBoxPlus.Left;
                if (TextureModeSelect.Visible)
                {
                    TextureModeSelect.Width += TextureModeSelect.Left - TextureSurfaceSelect.Left;
                    TextureModeSelect.Left = TextureSurfaceSelect.Left;
                }
                else
                {
                    foreach (Control foo in new Control[] { Param1, Param2, Param3, RedLabel, GreenLabel, BlueLabel, Stars, ColorBox, ColorLabel })
                        foo.Top -= heightBetweenRows;
                    groupBox4.Height -= heightBetweenRows;
                }
                TextureSurfaceSelect.Visible = false;
                TintColor.Visible = TintColorLabel.Visible = TextureSource.Visible = TextureSourceDraw.Visible = Fade.Visible = XFade.Visible = XFadeLabel.Visible = YFade.Visible = YFadeLabel.Visible = false;
                foreach (Control foo in new Control[] { Param1, Param2, Param3, RedLabel, GreenLabel, BlueLabel, Stars, ColorBox, ColorLabel })
                    foo.Top -= heightBetweenRows * 2;
                groupBox4.Height -= heightBetweenRows * 2;
            }
            else //yes developing for plus
            {
                TextureMode.Visible = false;
                groupBox4.Text = "Texture Mode";
                if (!TextureModeSelect.Visible) //only if you're doing weird stuff to your ini
                {
                    TextureSurfaceSelect.Width += TextureModeSelect.Right - TextureSurfaceSelect.Right;
                }
                SpriteParamMapping.Items.AddRange(SourceForm.J2L.PlusPropertyList.NamedPalettes.Select(np => np.Name).ToArray());
            }
            TextureModeSelect.Items.Clear();
            for (ushort i = 0; i < SourceForm.TextureTypes.Count; i++) TextureModeSelect.Items.Add(SourceForm.TextureTypes[i][0].Trim());
            if (!ShowLayerSelection)
            {
                ButtonApply.Hide();
                groupBox1.Hide();
                var amountToShrinkWindow = groupBox2.Location.Y - groupBox1.Location.Y;
                foreach (GroupBox foo in new GroupBox[] { groupBox2, groupBoxPlus })
                { foo.Location = new Point(foo.Location.X, foo.Location.Y - amountToShrinkWindow); }
                if (DataSource.id == J2LFile.SpriteLayerID || DataSource.id == 7)
                    Copy4.Hide();
                ReadLayer(DataSource);
            }
            else
            {
                LayerSelect.Items.AddRange(SourceForm.J2L.AllLayers.ToArray());
                LayerSelect.SelectedIndex = CurrentLayer;
            }
            ClientSize = new Size(ClientSize.Width, Math.Max(groupBoxPlus.Bottom, groupBox4.Bottom) + (groupBoxPlus.Visible ? 62 : 12));
        }

        private void ReadLayer(Layer layer)
        {
            WidthBox.Value = layer.Width;
            HeightBox.Value = layer.Height;
            //PHASEWidthBox.Text = layer.Width.ToString();
            //PHASEHeightBox.Text = layer.Height.ToString();
            TileWidth.Checked = layer.TileWidth;
            TileHeight.Checked = layer.TileHeight;
            XSpeed.Text = layer.XSpeed.ToString();
            AutoXSpeed.Text = layer.AutoXSpeed.ToString();
            YSpeed.Text = layer.YSpeed.ToString();
            AutoYSpeed.Text = layer.AutoYSpeed.ToString();
            LimitVisibleRegion.Checked = layer.LimitVisibleRegion;
            TextureMode.Checked = layer.TextureSurface != 0;
            TextureSurfaceSelect.SelectedIndex = layer.TextureSurface;
            Stars.Checked = layer.HasStars;
            Param1.Value = layer.TexturParam1;
            Param2.Value = layer.TexturParam2;
            Param3.Value = layer.TexturParam3;
            try
            {
                ParamDropdown1.SelectedIndex = (int)Param1.Value;
            }
            catch { }
            try
            {
                ParamDropdown2.SelectedIndex = (int)Param2.Value;
            }
            catch { }
            try
            {
                ParamDropdown3.SelectedIndex = (int)Param3.Value;
            }
            catch { }
            //PHASEParam1.Text = layer.TexturParam1.ToString();
            //PHASEParam2.Text = layer.TexturParam2.ToString();
            //PHASEParam3.Text = layer.TexturParam3.ToString();
            ColorBox.BackColor = Color.FromArgb(layer.TexturParam1, layer.TexturParam2, layer.TexturParam3);
            try
            {
                TextureModeSelect.SelectedIndex = layer.TextureMode;
            }
            catch
            {
                //oh well
            }
            XOffset.Text = layer.WaveX.ToString();
            YOffset.Text = layer.WaveY.ToString();
            NameBox.Text = layer.Name;
            Hidden.Checked = layer.Hidden;
            SpriteMode.SelectedIndex = layer.SpriteMode;
            SpriteParam.Value = layer.SpriteParam;
            try {
                SpriteParamMapping.SelectedIndex = layer.SpriteParam;
            } catch {
                //oh well
            }
            RotationAngle.Value = layer.RotationAngle;
            RotationRadiusMultiplier.Value = layer.RotationRadiusMultiplier;
            XSModel.SelectedIndex = layer.XSpeedModel;
            YSModel.SelectedIndex = layer.YSpeedModel;
            Fade.Checked = layer.Fade != 0;
            TintColor.Value = layer.Fade;
            XFade.Text = layer.XFade.ToString();
            YFade.Text = layer.YFade.ToString();
            InnerX.Text = layer.InnerX.ToString();
            InnerY.Text = layer.InnerY.ToString();
            InnerAutoX.Text = layer.InnerAutoX.ToString();
            InnerAutoY.Text = layer.InnerAutoY.ToString();
            TextureColorZeroIsTransparent.Checked = layer.TextureColorZeroIsTransparent;
            if (layer.Texture >= 0)
            {
                TextureSource.SelectedIndex = layer.Texture;
                TextureWidth = 256;
            }
            else
            {
                if (TextureSource.Items.Count == 16)
                    TextureSource.Items.Add("[Custom]");
                TextureSource.SelectedIndex = 16;
                Texture = layer.TextureImage;
                TextureWidth = layer.TextureWidth;
            }
        }

        private bool ApplyChanges()
        {
            newrectangle = null;
            lock (DataSource)
            {
                if (SourceForm.EnableableBools[SourceForm.J2L.VersionType][EnableableTitles.BoolDevelopingForPlus]) //any width/height allowed for textured layers, in principle
                {
                    if (TextureSurfaceSelect.SelectedIndex != 0 && TextureSource.SelectedIndex == 0)
                    {
                        int layerSize = (((int)WidthBox.Value + 3) & ~3) * (int)HeightBox.Value;
                        if (layerSize < 8 * 8)
                        {
                            switch (MessageBox.Show("You are trying to save a textured layer using [From Tiles] as its texture image, but the layer is too small. Would you like to resize the layer to 8x8?", "Layer too small", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning))
                            {
                                case DialogResult.Yes:
                                    WidthBox.Value = HeightBox.Value = 8;
                                    break;
                                case DialogResult.Cancel:
                                    return false;
                            }
                        }
                    }
                }
                if (DataSource.Width != WidthBox.Value || DataSource.Height != HeightBox.Value)
                {
                    if (DataSource.HasTiles)
                    {
                        newrectangle = LayerAlign.Show(CurrentLayer, (int)WidthBox.Value - (int)DataSource.Width, (int)HeightBox.Value - (int)DataSource.Height);
                        if (newrectangle == null) return false;
                        SourceForm.Undoable = new Stack<MLLE.Mainframe.LayerAndSpecificTiles>(SourceForm.Undoable.Where(action => action.Layer != DataSource));
                        SourceForm.Redoable = new Stack<MLLE.Mainframe.LayerAndSpecificTiles>(SourceForm.Redoable.Where(action => action.Layer != DataSource));
                    }
                    else
                    {
                        DataSource.TileMap = new ArrayMap<ushort> ((uint)WidthBox.Value, (uint)HeightBox.Value);
                    }
                }

                DataSource.Width = (uint)WidthBox.Value;
                DataSource.Height = (uint)HeightBox.Value;
                if (TileWidth.Checked)
                    switch (DataSource.Width % 4)
                    {
                        case 0: DataSource.RealWidth = DataSource.Width; break;
                        case 2: DataSource.RealWidth = DataSource.Width * 2; break;
                        default: DataSource.RealWidth = DataSource.Width * 4; break;
                    }
                else DataSource.RealWidth = DataSource.Width;
                
                if (newrectangle != null)
                {
                    ArrayMap<ushort> newTileMap = new ArrayMap<ushort>(DataSource.Width, DataSource.Height);
                    for (ushort x = 0; x < DataSource.Width; x++) for (ushort y = 0; y < DataSource.Height; y++)
                        {
                            newTileMap[x, y] = (
                                x >= -newrectangle[2] &&
                                newrectangle[2] + x < DataSource.TileMap.GetLength(0) &&
                                y >= -newrectangle[0] &&
                                newrectangle[0] + y < DataSource.TileMap.GetLength(1)
                                )
                                ? DataSource.TileMap[newrectangle[2] + x, newrectangle[0] + y]
                                : (ushort)0;
                        }

                    if (DataSource.id == J2LFile.SpriteLayerID) //sprite layer, i.e. events are associated with this one
                    {
                        if (SourceForm.J2L.VersionType == Version.AGA)
                        {
                            AGAEvent[,] newAGAMap = new AGAEvent[DataSource.Width, DataSource.Height];
                            for (ushort x = 0; x < DataSource.Width; x++) for (ushort y = 0; y < DataSource.Height; y++)
                                    newAGAMap[x, y] = (
                                        x >= -newrectangle[2] &&
                                        newrectangle[2] + x < DataSource.TileMap.GetLength(0) &&
                                        y >= -newrectangle[0] &&
                                        newrectangle[0] + y < DataSource.TileMap.GetLength(1)
                                        )
                                        ? SourceForm.J2L.AGA_EventMap[newrectangle[2] + x, newrectangle[0] + y]
                                        : new AGAEvent();
                            SourceForm.J2L.AGA_EventMap = newAGAMap;
                        }
                        else
                        {
                            var oldEventMap = SourceForm.J2L.EventMap;
                            uint[,] newEventMap = new uint[DataSource.Width, DataSource.Height];

                            PlusPropertyList rememberPlusEventBasedSettings = new PlusPropertyList(null);
                            rememberPlusEventBasedSettings.ReadFromEventMap(oldEventMap);
                            rememberPlusEventBasedSettings.WriteToEventMap(newEventMap);

                            int oldEventMapRightColumn = oldEventMap.GetLength(0) - 1;
                            int oldEventMapBottomRow = oldEventMap.GetLength(1) - 1;

                            for (ushort x = 0; x < DataSource.Width; x++) for (ushort y = 0; y < DataSource.Height - 1; y++) //-1 because events in the bottom row don't work, except for a few events parsed by JJ2+ that were handled above
                                {
                                    newEventMap[x, y] = (
                                        x >= -newrectangle[2] &&
                                        newrectangle[2] + x <= oldEventMapRightColumn &&
                                        y >= -newrectangle[0] &&
                                        newrectangle[0] + y < oldEventMapBottomRow
                                        )
                                        ? oldEventMap[newrectangle[2] + x, newrectangle[0] + y]
                                        : (ushort)0;
                                }
                            SourceForm.J2L.EventMap = newEventMap;
                        }
                    }
                    DataSource.TileMap = newTileMap;
                }

                DataSource.TileWidth = TileWidth.Checked;
                DataSource.TileHeight = TileHeight.Checked;
                Single.TryParse(XSpeed.Text, out DataSource.XSpeed);
                Single.TryParse(YSpeed.Text, out DataSource.YSpeed);
                Single.TryParse(AutoXSpeed.Text, out DataSource.AutoXSpeed);
                Single.TryParse(AutoYSpeed.Text, out DataSource.AutoYSpeed);
                Single.TryParse(XOffset.Text, out DataSource.WaveX);
                Single.TryParse(YOffset.Text, out DataSource.WaveY);
                DataSource.LimitVisibleRegion = LimitVisibleRegion.Checked;
                DataSource.TextureSurface = (byte)(TextureMode.Visible ? (TextureMode.Checked ? 1 : 0) : TextureSurfaceSelect.SelectedIndex);
                DataSource.HasStars = Stars.Checked;
                DataSource.TexturParam1 = (byte)(ParamDropdown1.Visible ? ParamDropdown1.SelectedIndex : Param1.Value);
                DataSource.TexturParam2 = (byte)(ParamDropdown2.Visible ? ParamDropdown2.SelectedIndex : Param2.Value);
                DataSource.TexturParam3 = (byte)(ParamDropdown3.Visible ? ParamDropdown3.SelectedIndex : Param3.Value);
                DataSource.TextureMode = (byte)TextureModeSelect.SelectedIndex;
                DataSource.Name = NameBox.Text;
                DataSource.Hidden = Hidden.Checked;
                DataSource.SpriteMode = (byte)SpriteMode.SelectedIndex;
                if (SpriteMode.SelectedIndex < 48) //not MAPPING or TRANSLUCENTMAPPING
                    DataSource.SpriteParam = (byte)SpriteParam.Value;
                else
                    DataSource.SpriteParam = (byte)SpriteParamMapping.SelectedIndex;
                DataSource.RotationAngle = (int)RotationAngle.Value;
                DataSource.RotationRadiusMultiplier = (int)RotationRadiusMultiplier.Value;
                DataSource.XSpeedModel = (byte)XSModel.SelectedIndex;
                DataSource.YSpeedModel = (byte)YSModel.SelectedIndex;
                DataSource.Fade = !(TextureModeSelect.SelectedIndex == 6 && TextureSurfaceSelect.SelectedIndex != 0) ? (byte)(Fade.Checked ? 192 : 0) : (byte)TintColor.Value;
                Single.TryParse(XFade.Text, out DataSource.XFade);
                Single.TryParse(YFade.Text, out DataSource.YFade);
                Single.TryParse(InnerX.Text, out DataSource.InnerX);
                Single.TryParse(InnerY.Text, out DataSource.InnerY);
                Single.TryParse(InnerAutoX.Text, out DataSource.InnerAutoX);
                Single.TryParse(InnerAutoY.Text, out DataSource.InnerAutoY);
                DataSource.TextureColorZeroIsTransparent = TextureColorZeroIsTransparent.Checked;
                DataSource.Texture = TextureSource.SelectedIndex < 16 ? (sbyte)TextureSource.SelectedIndex : (sbyte)-1;
                DataSource.TextureImage = Texture;
                DataSource.TextureWidth = TextureWidth;
                SourceForm.LevelHasBeenModified = true;
            }

            return true;
        }

        private void DecideBetweenDropdownAndNumber(Label label, ComboBox dropdown, NumericUpDown number)
        {
            label.Visible = true;
            var match = System.Text.RegularExpressions.Regex.Match(label.Text, "(.+)\\{(.+)\\}");
            if (match.Success)
            {
                label.Text = match.Groups[1].Value;
                dropdown.Visible = true;
                dropdown.Items.Clear();
                dropdown.Items.AddRange(match.Groups[2].Value.Split(','));
                try
                {
                    dropdown.SelectedIndex = (int)number.Value;
                }
                catch { }
            }
            else
            {
                number.Visible = true;
            }
        }

        private void TextureModeSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] properties = SourceForm.TextureTypes[TextureModeSelect.SelectedIndex];
            if (properties.Length >= 3)
            {
                Stars.Text = properties[2].Trim();
                Stars.Visible = Stars.Text.Length > 0;
            }
            else { Stars.Visible = false; }

            Param1.Visible = Param2.Visible = Param3.Visible =
            ParamDropdown1.Visible = ParamDropdown2.Visible = ParamDropdown3.Visible =
            RedLabel.Visible = GreenLabel.Visible = BlueLabel.Visible =
                false;
            if (properties.Length > 1 && properties[1] == "+")
            {
                ColorLabel.Text = properties[3].Trim();
                ColorLabel.Visible = ColorBox.Visible = true;
            }
            else
            {
                if (properties.Length >= 4)  {

                    RedLabel.Text = properties[3].Trim();
                    DecideBetweenDropdownAndNumber(RedLabel, ParamDropdown1, Param1);
                }
                if (properties.Length >= 5) {
                    GreenLabel.Text = properties[4].Trim();
                    DecideBetweenDropdownAndNumber(GreenLabel, ParamDropdown2, Param2);
                }
                if (properties.Length >= 6) {
                    BlueLabel.Text = properties[5].Trim();
                    DecideBetweenDropdownAndNumber(BlueLabel, ParamDropdown3, Param3);
                    if (TextureModeSelect.SelectedIndex != 4)
                    {
                        Param3.Minimum = 0;
                        Param3.Maximum = 255;
                    }
                    else
                    {
                        Param3.Minimum = -128;
                        Param3.Maximum = 127;
                    }
                }
                ColorLabel.Visible = ColorBox.Visible = false;
            }

            if (TextureSurfaceSelect.Visible)
            {
                Fade.Visible = (TextureModeSelect.SelectedIndex <= 1 || TextureModeSelect.SelectedIndex == 5); //warp horizon, tunnel, cylinder
                Fade.Enabled = TextureMode.Checked && Fade.Visible;
                TintColor.Visible = TintColorLabel.Visible = TextureMode.Checked && TextureModeSelect.SelectedIndex == 6; //reflection
                TintColor.Enabled = TextureMode.Checked && TintColor.Visible;
            }
            TextureSource.Enabled = TextureMode.Checked && TextureModeSelect.SelectedIndex != 2;
            TextureSourceDraw.Enabled = TextureSource.Enabled && SourceForm.J2L.HasTiles;
            SpriteMode.Enabled = !TextureMode.Checked || TextureModeSelect.SelectedIndex != 6;
            TextureColorZeroIsTransparent.Enabled = TextureMode.Checked && TextureModeSelect.SelectedIndex != 6;
            string fadeSuffix = "Fade";
            if (TextureModeSelect.SelectedIndex == 2 || TextureModeSelect.SelectedIndex == 3)
                fadeSuffix = "Pivot";
            else if (TextureModeSelect.SelectedIndex == 4)
                fadeSuffix = "Amplitude";
            else if (TextureModeSelect.SelectedIndex == 7)
                fadeSuffix = "Scale";
            XFadeLabel.Text = "X-" + fadeSuffix;
            YFadeLabel.Text = (TextureModeSelect.SelectedIndex != 6) ? "Y-" + fadeSuffix : "Top";
            GenericInputChanged(sender, e);
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = ColorBox.BackColor;
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK) ColorBox.BackColor = colorDialog1.Color;
            Param1.Value = colorDialog1.Color.R;
            Param2.Value = colorDialog1.Color.G;
            Param3.Value = colorDialog1.Color.B;
            GenericInputChanged(sender, e);
        }

        private void CancelButton_Click(object sender, EventArgs e) { Close(); }

        private void LayerSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentLayer = (byte)LayerSelect.SelectedIndex;
            ReadLayer(DataSource = SourceForm.J2L.AllLayers[CurrentLayer]);
            TileWidth.Enabled = TileHeight.Enabled = (
                (
                    XSModel.Enabled = YSModel.Enabled = XOffset.Enabled = YOffset.Enabled = TextureMode.Enabled = groupBox2.Enabled = groupBox4.Enabled = (DataSource.id != J2LFile.SpriteLayerID)
                ) ||
                SourceForm.EnableableBools[SourceForm.J2L.VersionType][EnableableTitles.BoolDevelopingForPlus]
            );
            Copy4.Enabled = (DataSource.id != J2LFile.SpriteLayerID && DataSource.id != 7);
            ButtonApply.Enabled = false;
        }

        private void TextureMode_CheckedChanged(object sender, EventArgs e)
        {
            XFade.Enabled = YFade.Enabled = XFadeLabel.Enabled = YFadeLabel.Enabled = TextureModeSelect.Enabled = Stars.Enabled = ColorBox.Enabled = ColorLabel.Enabled = Param1.Enabled = Param2.Enabled = Param3.Enabled = RedLabel.Enabled = GreenLabel.Enabled = BlueLabel.Enabled = TextureMode.Checked;
            TextureSource.Enabled = TextureMode.Checked && TextureModeSelect.SelectedIndex != 2;
            TextureSourceDraw.Enabled = TextureSource.Enabled && SourceForm.J2L.HasTiles;
            TextureColorZeroIsTransparent.Enabled = TextureMode.Checked && TextureModeSelect.SelectedIndex != 6; //not reflection
            if (TextureSurfaceSelect.Visible)
            {
                Fade.Visible = (TextureModeSelect.SelectedIndex <= 1 || TextureModeSelect.SelectedIndex == 5); //warp horizon, tunnel, cylinder
                Fade.Enabled = TextureMode.Checked && Fade.Visible;
                TintColor.Visible = TintColorLabel.Visible = TextureMode.Checked && TextureModeSelect.SelectedIndex == 6; //reflection
                TintColor.Enabled = TextureMode.Checked && TintColor.Visible;
            }
            SpriteMode.Enabled = !TextureMode.Checked || TextureModeSelect.SelectedIndex != 6;
            if (!SourceForm.EnableableBools[SourceForm.J2L.VersionType][EnableableTitles.BoolDevelopingForPlus])
            {
                if (TextureMode.Checked) {
                    WidthBox.Value = HeightBox.Value = 8;
                    WidthBox.Enabled = HeightBox.Enabled = WidthLabel.Enabled = HeightLabel.Enabled = false;
                } else {
                    WidthBox.Value = DataSource.Width;
                    HeightBox.Value = DataSource.Height;
                    WidthBox.Enabled = HeightBox.Enabled = WidthLabel.Enabled = HeightLabel.Enabled = true;
                }
            } //else, in a JJ2+ level, the texture might take its image from plus.j2d or a pixelmap, so the layer size no longer needs to be 8x8 (or otherwise 64+ tiles)
            TileHeight_CheckedChanged(sender, e);
        }
        private void TileHeight_CheckedChanged(object sender, EventArgs e) {
            LimitVisibleRegion.Enabled = !(TextureMode.Checked || TileHeight.Checked);
            GenericInputChanged(sender, e);
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            ApplyChanges();
            Close();
        }

        private void Copy4_Click(object sender, EventArgs e)
        {
            string name = NameBox.Text;
            ReadLayer(SourceForm.J2L.SpriteLayer);
            NameBox.Text = name; //I guess?
        }

        private void GenericInputChanged(object sender, EventArgs e)
        {
            ButtonApply.Enabled = true;
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            if (ApplyChanges())
            {
                LayerSelect.Items[LayerSelect.SelectedIndex] = SourceForm.J2L.AllLayers[LayerSelect.SelectedIndex].ToString();
                ButtonApply.Enabled = false;
            }
        }

        private void TextureSurfaceSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextureMode.Checked = TextureSurfaceSelect.SelectedIndex != 0;
            groupBoxInner.Visible = TextureSurfaceSelect.SelectedIndex == 3 || TextureSurfaceSelect.SelectedIndex == 4;
            SpriteMode.Enabled = !TextureMode.Checked || TextureModeSelect.SelectedIndex != 6;
            GenericInputChanged(sender, e);
        }

        private void TextureSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TextureSource.SelectedIndex < 16)
                while (TextureSource.Items.Count > 16) //includes [Custom]
                    TextureSource.Items.RemoveAt(16);
            GenericInputChanged(sender, e);
        }

        byte[] Texture = null;
        int TextureWidth;
        private static readonly Bitmap[] TextureImages = { Properties.Resources._1_normal, Properties.Resources._2_psych, Properties.Resources._3_medivo, Properties.Resources._4_diamb, Properties.Resources._5_wisetyness, Properties.Resources._6_blade, Properties.Resources._7_mez02, Properties.Resources._8_windstormfortress, Properties.Resources._9_raneforusv, Properties.Resources._10_corruptedsanctuary, Properties.Resources._11_xargon, Properties.Resources._12_tubelectric, Properties.Resources._13_wtf, Properties.Resources._14_muckamoknight, Properties.Resources._15_desolation };
        private void TextureSourceDraw_Click(object sender, EventArgs e)
        {
            if (!SourceForm.J2L.HasTiles)
                return; //no palette, can't do any editing
            byte[] texture;
            if (TextureSource.SelectedIndex == 0) //from tiles
            {
                texture = new byte[256 * 256];
                if (DataSource.Width * DataSource.Height >= 8*8)
                {
                    for (int i = 0; i < 8 * 8; ++i)
                    {
                        uint tileID = DataSource.TileMap[i % DataSource.Width, i / DataSource.Width];
                        if (tileID < SourceForm.J2L.TileCount)
                        {
                            byte[] tileImageAsBytes = SourceForm.J2L.PlusPropertyList.TileImages[tileID];
                            if (tileImageAsBytes == null)
                            {
                                J2TFile J2T;
                                uint tileInTilesetID = SourceForm.J2L.getTileInTilesetID(tileID, out J2T);
                                tileImageAsBytes = J2T.Images[J2T.ImageAddress[tileInTilesetID]];
                                if (J2T.ColorRemapping != null)
                                    tileImageAsBytes = tileImageAsBytes.Select(c => J2T.ColorRemapping[c]).ToArray();
                            }
                            int firstResultByteIndex = (i & 7) * 32 + (i / 8) * 32 * 256;
                            for (int b = 0; b < 32 * 32; ++b)
                                texture[firstResultByteIndex + (b & 31) + (b >> 5) * 256] = tileImageAsBytes[b];
                        }
                    }
                }
            }
            else if (TextureSource.SelectedIndex >= 16) //custom
            {
                texture = Texture;
            }
            else
            {
                texture = BitmapStuff.BitmapToByteArray(TextureImages[TextureSource.SelectedIndex - 1]);
            }
            int newWidth = new TileImageEditorForm().ShowForm(
                ref texture,
                texture,
                SourceForm.J2L.Palette,
                TextureWidth, 1,
                TextureModeSelect.SelectedIndex == 7 //static
            );
            if (newWidth != 0)
            {
                if (TextureSource.Items.Count == 16)
                    TextureSource.Items.Add("[Custom]");
                TextureSource.SelectedIndex = 16;
                Texture = texture;
                TextureWidth = newWidth;
                GenericInputChanged(sender, e);
            }
        }

        private void SetSpeedNames(int selectedIndex, Label normalL, TextBox normal, Label autoL, TextBox auto, char prefix, string start, string end)
        {
            normalL.Enabled = normal.Enabled = autoL.Enabled = auto.Enabled = selectedIndex != 4; //not Fit Level
            if (selectedIndex != 5) //not Speed Multipliers
            {
                normalL.Text = prefix + "-Speed";
                autoL.Text = "Auto " + prefix + "-Speed";
            }
            else
            {
                normalL.Text = start;
                autoL.Text = end;
            }
            GenericInputChanged(null, null);
        }
        private void YSModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            LimitVisibleRegion.Enabled = (YSModel.SelectedIndex == 0 || YSModel.SelectedIndex == 2) && DataSource.id != J2LFile.SpriteLayerID; //either Normal or Both Speeds
            SetSpeedNames(YSModel.SelectedIndex, YLabel, YSpeed, AutoYLabel, AutoYSpeed, 'Y', "Top Pos", "Bottom Pos");
        }
        private void XSModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSpeedNames(XSModel.SelectedIndex, XLabel, XSpeed, AutoXLabel, AutoXSpeed, 'X', "Left Pos", "Right Pos");
        }

        private void SpriteMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenericInputChanged(sender, e);
            SpriteParam.Visible = !(SpriteParamMapping.Visible = (SpriteMode.SelectedIndex >= 48)); //48 and 49 = MAPPING and TRANSLUCENTMAPPING
        }
    }
}
