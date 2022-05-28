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
            if (SourceForm.TextureTypes.Count == 0) { TextureMode.Parent = groupBox3; TextureMode.Location = new Point(TileHeight.Location.X, LimitVisibleRegion.Location.Y); ClientSize = new Size(ClientSize.Width, ClientSize.Height - (groupBox4.Bottom - groupBox3.Bottom)); }
            else if (SourceForm.TextureTypes.Count == 1)
            {
                TextureModeSelect.Visible = false;
                foreach (Control foo in new Control[] { Param1, Param2, Param3, RedLabel, GreenLabel, BlueLabel, Stars, ColorBox, ColorLabel })
                { foo.Location = new Point(foo.Location.X, foo.Location.Y - TextureModeSelect.Height); }
                /*Param1.Location = new Point(Param1.Location.X, Param1.Location.Y - TextureModeSelect.Height);
                Param2.Location = new Point(Param2.Location.X, Param2.Location.Y - TextureModeSelect.Height);
                Param3.Location = new Point(Param3.Location.X, Param3.Location.Y - TextureModeSelect.Height);
                RedLabel.Location = new Point(RedLabel.Location.X, RedLabel.Location.Y - TextureModeSelect.Height);
                GreenLabel.Location = new Point(GreenLabel.Location.X, GreenLabel.Location.Y - TextureModeSelect.Height);
                BlueLabel.Location = new Point(BlueLabel.Location.X, BlueLabel.Location.Y - TextureModeSelect.Height);
                Stars.Location = new Point(Stars.Location.X, Stars.Location.Y - TextureModeSelect.Height);
                ColorBox.Location = new Point(ColorBox.Location.X, ColorBox.Location.Y - TextureModeSelect.Height);
                ColorLabel.Location = new Point(ColorLabel.Location.X, ColorLabel.Location.Y - TextureModeSelect.Height);*/
                if (SourceForm.TextureTypes[0][1] == "+")
                {
                    groupBox4.Height -= TextureModeSelect.Height + (Param3.Location.Y - Param1.Location.Y);
                    Height -= TextureModeSelect.Height + (Param3.Location.Y - Param1.Location.Y);
                }
                else
                {
                    groupBox4.Height -= TextureModeSelect.Height;
                    Height -= TextureModeSelect.Height;
                }
            }
            if (!SourceForm.EnableableBools[SourceForm.J2L.VersionType][EnableableTitles.BoolDevelopingForPlus])
            {
                groupBoxPlus.Hide();
                var amountToShrinkWindow = groupBox3.Location.Y - groupBoxPlus.Location.Y;
                groupBox3.Location = new Point(groupBox3.Location.X, groupBox3.Location.Y - amountToShrinkWindow);
                groupBox4.Location = new Point(groupBox4.Location.X, groupBox4.Location.Y - amountToShrinkWindow);
                Height -= amountToShrinkWindow;
                TextureModeSelect.Width += TextureModeSelect.Left - TextureSurfaceSelect.Left;
                TextureModeSelect.Left = TextureSurfaceSelect.Left;
                TextureSurfaceSelect.Visible = false;
            }
            else //yes developing for plus
            {
                TextureMode.Visible = false;
                groupBox4.Text = "Texture Mode";
            }
            TextureModeSelect.Items.Clear();
            for (ushort i = 0; i < SourceForm.TextureTypes.Count; i++) TextureModeSelect.Items.Add(SourceForm.TextureTypes[i][0].Trim());
            if (!ShowLayerSelection)
            {
                ButtonApply.Hide();
                groupBox1.Hide();
                var amountToShrinkWindow = groupBox2.Location.Y - groupBox1.Location.Y;
                foreach (GroupBox foo in new GroupBox[] { groupBox2, groupBox3, groupBox4, groupBoxPlus })
                { foo.Location = new Point(foo.Location.X, foo.Location.Y - amountToShrinkWindow); }
                Height -= amountToShrinkWindow;
                if (DataSource.id == J2LFile.SpriteLayerID || DataSource.id == 7)
                    Copy4.Hide();
                ReadLayer(DataSource);
            }
            else
            {
                LayerSelect.Items.AddRange(SourceForm.J2L.AllLayers.ToArray());
                LayerSelect.SelectedIndex = CurrentLayer;
            }
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
            RotationAngle.Value = layer.RotationAngle;
            RotationRadiusMultiplier.Value = layer.RotationRadiusMultiplier;
            XSModel.SelectedIndex = layer.XSpeedModel;
            YSModel.SelectedIndex = layer.YSpeedModel;
        }

        private bool ApplyChanges()
        {
            newrectangle = null;
            lock (DataSource)
            {
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
                DataSource.TexturParam1 = (byte)Param1.Value;
                DataSource.TexturParam2 = (byte)Param2.Value;
                DataSource.TexturParam3 = (byte)Param3.Value;
                DataSource.TextureMode = (byte)TextureModeSelect.SelectedIndex;
                DataSource.Name = NameBox.Text;
                DataSource.Hidden = Hidden.Checked;
                DataSource.SpriteMode = (byte)SpriteMode.SelectedIndex;
                DataSource.SpriteParam = (byte)SpriteParam.Value;
                DataSource.RotationAngle = (int)RotationAngle.Value;
                DataSource.RotationRadiusMultiplier = (int)RotationRadiusMultiplier.Value;
                DataSource.XSpeedModel = (byte)XSModel.SelectedIndex;
                DataSource.YSpeedModel = (byte)YSModel.SelectedIndex;
                SourceForm.LevelHasBeenModified = true;
            }

            return true;
        }

        private void TextureModeSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] properties = SourceForm.TextureTypes[TextureModeSelect.SelectedIndex];
            if (properties.Length >= 3)
            {
                Stars.Text = properties[2].Trim();
                Stars.Visible = true;
            }
            else { Stars.Visible = false; }

            Param1.Visible = Param2.Visible = Param3.Visible = RedLabel.Visible = GreenLabel.Visible = BlueLabel.Visible = false;
            if (properties.Length > 1 && properties[1] == "+")
            {
                ColorLabel.Text = properties[3].Trim();
                ColorLabel.Visible = ColorBox.Visible = true;
            }
            else
            {
                if (properties.Length >= 4) { Param1.Visible = RedLabel.Visible = true; RedLabel.Text = properties[3].Trim(); }
                if (properties.Length >= 5) { Param2.Visible = GreenLabel.Visible = true; GreenLabel.Text = properties[4].Trim(); }
                if (properties.Length >= 6) { Param3.Visible = BlueLabel.Visible = true; BlueLabel.Text = properties[5].Trim(); }
                ColorLabel.Visible = ColorBox.Visible = false;
            }
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
                    XOffset.Enabled = YOffset.Enabled = groupBox2.Enabled = TextureMode.Enabled = (DataSource.id != J2LFile.SpriteLayerID)
                ) ||
                SourceForm.EnableableBools[SourceForm.J2L.VersionType][EnableableTitles.BoolDevelopingForPlus]
            );
            Copy4.Enabled = (DataSource.id != J2LFile.SpriteLayerID && DataSource.id != 7);
            ButtonApply.Enabled = false;
        }

        private void TextureMode_CheckedChanged(object sender, EventArgs e)
        {
            TextureModeSelect.Enabled = Stars.Enabled = ColorBox.Enabled = ColorLabel.Enabled = Param1.Enabled = Param2.Enabled = Param3.Enabled = RedLabel.Enabled = GreenLabel.Enabled = BlueLabel.Enabled = TextureMode.Checked;
            WidthBox.Enabled = HeightBox.Enabled = WidthLabel.Enabled = HeightLabel.Enabled = !TextureMode.Checked;
            if (TextureMode.Checked) WidthBox.Value = HeightBox.Value = 8;
            else { WidthBox.Value = DataSource.Width; HeightBox.Value = DataSource.Height; }
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
        }
    }
}
