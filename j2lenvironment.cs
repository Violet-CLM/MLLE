using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using TexLib;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Ini;

enum TransparencySource { JCS_Style, JJ2_Style }
class TexturedJ2L : J2LFile
{
    public double AtlasFraction;
    public int AtlasLength;

    private int _imageAtlas = 0;
    public int ImageAtlas
    {
        get { return _imageAtlas; }
        set
        {
            if (TexturesHaveBeenGenerated)
                GL.DeleteTexture(_imageAtlas);
            _imageAtlas = value;
        }
    }
    public bool TexturesHaveBeenGenerated { get { return _imageAtlas != 0; } }

    private int _maskAtlas = 0;
    public int MaskAtlas
    {
        get { return _maskAtlas; }
        set
        {
            if (_maskAtlas != 0)
                GL.DeleteTexture(_maskAtlas);
            _maskAtlas = value;
        }
    }


    public static Color TranspColor, Tile0Color, DeadspaceColor;

    internal static Dictionary<Version, int> EventAtlas = new Dictionary<Version, int> {
        {Version.BC, 0 },
        {Version.O, 0 },
        {Version.JJ2, 0 },
        {Version.TSF, 0 },
        {Version.AGA, 0 },
        {Version.GorH, 0 },
        };
    internal static Dictionary<Version, string[]> TileTypeNames = new Dictionary<Version, string[]> {
        {Version.BC, null },
        {Version.O, null },
        {Version.JJ2, null },
        {Version.TSF, null },
        {Version.AGA, null },
        {Version.GorH, null },
        };
    internal static Dictionary<Version, int> TileTypeAtlas = new Dictionary<Version, int> {
        {Version.BC, 0 },
        {Version.O, 0 },
        {Version.JJ2, 0 },
        {Version.TSF, 0 },
        {Version.AGA, 0 },
        {Version.GorH, 0 },
        };
    internal static Dictionary<Version, string[][]> IniEventListing = new Dictionary<Version, string[][]> {
        {Version.BC, null },
        {Version.O, null },
        {Version.JJ2, null },
        {Version.TSF, null },
        {Version.AGA, null },
        {Version.GorH, null },
        };

    public void Generate_Blank_Tile_Texture()
    {
        byte[] singleTileImage = new byte[32 * 32 * 4];
        var pixel = new byte[4] { Tile0Color.R, Tile0Color.G, Tile0Color.B, 255 };
        for (ushort i = 0; i < singleTileImage.Length; i++) singleTileImage[i] = pixel[i % 4];
        ImageAtlas = TexUtil.CreateRGBATexture(32, 32, singleTileImage);
        AtlasLength = 1;
        AtlasFraction = 1;
    }
    public static byte GetLevelFromColor(Color color, byte level)
    {
        switch (level)
        {
            case 0: return color.R;
            case 1: return color.G;
            case 2: return color.B;
            default: return color.A;
        }
    }
    public void Generate_Textures(TransparencySource source = TransparencySource.JJ2_Style, bool includeMasks = false, Palette palette = null)
    {
        byte[][] transSource;
        Color usedColor = Tile0Color;
        if (source == TransparencySource.JJ2_Style) transSource = J2T.TransparencyMaskJJ2_Style;
        else transSource = J2T.TransparencyMaskJCS_Style;
        if (palette == null)
            palette = J2T.Palette;
        byte[][] workingAtlases = new byte[2][];
        for (byte i = 0; i < 5; i++)
            if (TileCount < 16 << (i * 2)) {
                AtlasLength = 128 << i;
                workingAtlases[0] = new byte[AtlasLength * AtlasLength * 4];
                if (includeMasks)
                    workingAtlases[1] = new byte[AtlasLength * AtlasLength * 4];
                AtlasLength /= 32;
                AtlasFraction = 1.0d / AtlasLength;
                break;
            }
        for (ushort i = 0; i < TileCount; i++)
        {
            var tile = J2T.Images[J2T.ImageAddress[i]];
            var tileTrans = transSource[Array.BinarySearch(J2T.TransparencyMaskOffset, 0, (int)J2T.data3Counter, J2T.TransparencyMaskAddress[i])];
            var mask = J2T.Masks[J2T.MaskAddress[i]];
            bool transp = TileTypes[i] == 1;
            for (short j = 0; j < 4096; j += 4)
            {
                var pixel = palette[(int)tile[j / 4]];
                if (includeMasks) for (byte k = 0; k < 4; k++)
                {
                    int atlasDrawingLocation = i % AtlasLength * 128 + i / AtlasLength * AtlasLength * 4096 + j % 128 + j / 128 * AtlasLength * 128 + k;
                    workingAtlases[0][atlasDrawingLocation] = (k == 3) ? (tileTrans[j / 4] == 1) ? ((transp) ? (byte)192 : (byte)255) : (byte)0 : (tileTrans[j / 4] == 1) ? pixel[k] : GetLevelFromColor(usedColor, k);
                    workingAtlases[1][atlasDrawingLocation] = (k == 3) ? (mask[j / 4] == 1) ? (byte)196 : (byte)0 : (mask[j / 4] == 1) ? (byte)0 : GetLevelFromColor(usedColor, k);
                }
                else for (byte k = 0; k < 4; k++) workingAtlases[0][i % AtlasLength * 128 + i / AtlasLength * AtlasLength * 4096 + j % 128 + j / 128 * AtlasLength * 128 + k] = (k == 3) ? (tileTrans[j / 4] == 1) ? ((transp) ? (byte)192 : (byte)255) : (byte)0 : (tileTrans[j / 4] == 1) ? pixel[k] : GetLevelFromColor(usedColor, k);
            }
            if (i == 0) usedColor = TranspColor;
        }
       ImageAtlas = TexUtil.CreateRGBATexture(AtlasLength * 32, AtlasLength * 32, workingAtlases[0]);
        if (includeMasks) MaskAtlas = TexUtil.CreateRGBATexture(AtlasLength * 32, AtlasLength * 32, workingAtlases[1]);
    }
    public VersionChangeResults ChangeTileset(string filename, bool avoidRedundancy = true, Dictionary<Version, string> defaultDirectories = null, Palette overridePalette = null)
    {
        if (avoidRedundancy && Path.GetFileName(filename) == Tileset) return VersionChangeResults.Success;
        else {
            VersionChangeResults result = base.ChangeTileset(filename, avoidRedundancy, defaultDirectories);
            if (result == VersionChangeResults.Success) { if (TexturesHaveBeenGenerated) Generate_Textures(TransparencySource.JJ2_Style, true, overridePalette); }
            return result;
        }
    }
    public static void ProduceEventAndTypeListsFromIni(Version version, IniFile eventIni, IniFile typeIni, bool overwriteOldLists = false)
    {
        if (!overwriteOldLists && !(EventAtlas[version] == 0 || IniEventListing[version] == null)) return;
        string[][] StringList = IniEventListing[version] = new string[256][];
        for (int i = 0; i < 256; i++)
        {
            if (eventIni.IniReadValue("Events", i.ToString()) != "")
            {
                StringList[i] = (((eventIni != typeIni && typeIni.IniReadValue("Events", i.ToString()).Length > 0) ? typeIni : eventIni).IniReadValue("Events", i.ToString()).Split('|'));
                for (byte j = 0; j < StringList[i].Length; j++) StringList[i][j] = StringList[i][j].TrimEnd();
            }
            else StringList[i] = new string[] { (i == 0) ? "(none)" : "Event " + i.ToString(), "-", "", "DON'T", "USE" };
        }
        StringFormat formatEvent = new StringFormat(), formatType = new StringFormat();
        formatEvent.Alignment = formatEvent.LineAlignment = formatType.LineAlignment = StringAlignment.Center;
        formatType.Alignment = StringAlignment.Near;
        SolidBrush white = new SolidBrush(Color.White);
        RectangleF rectE = new RectangleF(-16, 0, 64, 32);
        RectangleF rectT = new RectangleF(-2, 0, 256, 32);
        using (Font arial = new Font(new FontFamily("Arial"), 8)) using (Bitmap text_bmp = new Bitmap(512, 512)) using (Bitmap type_bmp = new Bitmap(128,128)) using (formatType) using (formatEvent)
        {
            Bitmap single_bmp; Graphics gfx;
            Graphics totalgfx = Graphics.FromImage(text_bmp);
            totalgfx.Clear(Color.FromArgb(128, 0, 0, 0));
            for (int i = 1; i < 256; i++)
                {
                    single_bmp = new Bitmap(32, 32);
                    gfx = Graphics.FromImage(single_bmp);
                    //gfx.Clear(Color.FromArgb(128, 0, 0, 0));
                    if (StringList[i].Length > 4 && StringList[i][4].TrimEnd() != "") gfx.DrawString(StringList[i][3] + "\n" + StringList[i][4], arial, white, rectE, formatEvent);
                    else gfx.DrawString(StringList[i][3], arial, white, rectE, formatEvent);
                    totalgfx.DrawImage(single_bmp,i%16*32, i/16*32);
                }
            if (EventAtlas[version] != 0)
                GL.DeleteTexture(EventAtlas[version]);
            EventAtlas[version] = TexUtil.CreateTextureFromBitmap(text_bmp);

            totalgfx = Graphics.FromImage(type_bmp);
            totalgfx.Clear(Color.FromArgb(128, 0, 0, 0));
            TileTypeNames[version] = new string[16];
            for (int i = 1; i < 16; i++) if ((TileTypeNames[version][i] = typeIni.IniReadValue("Tiles", i.ToString()) ?? "") != "")
                {
                    single_bmp = new Bitmap(32, 32);
                    gfx = Graphics.FromImage(single_bmp);
                    //gfx.Clear(Color.FromArgb(128, 0, 0, 0));
                    gfx.DrawString(typeIni.IniReadValue("Tiles", i.ToString()), arial, white, rectT, formatType);
                    totalgfx.DrawImage(single_bmp, i % 4 * 32, i / 4 * 32);
                }
            if (TileTypeAtlas[version] != 0)
                GL.DeleteTexture(TileTypeAtlas[version]);
            TileTypeAtlas[version] = TexUtil.CreateTextureFromBitmap(type_bmp);
        }
    }
}