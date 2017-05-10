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
    internal bool TexturesHaveBeenGenerated = false;
    internal double[] AtlasRatio = new double[4];
    public int[] AtlasID = new int[8];
    internal byte[] pixel, tile, tileTrans, mask;
    public Color TranspColor, Tile0Color, DeadspaceColor;
    internal bool transp;
    internal static byte[] eventbackdrop = new byte[4096];
    internal static RectangleF rectE = new RectangleF(-16, 0, 64, 32);
    internal static RectangleF rectT = new RectangleF(-2, 0, 256, 32);
    public int[] Atlases = new int[2];
    public double AtlasFraction;
    public int AtlasLength;
    //internal static Bitmap text_bmp;
    internal int atlasDrawingLocation;
    internal byte[][] workingAtlases = new byte[2][];
    internal Dictionary<Version, int?> EventAtlas = new Dictionary<Version, int?> {
        {Version.BC, null },
        {Version.O, null },
        {Version.JJ2, null },
        {Version.TSF, null },
        {Version.AGA, null },
        {Version.GorH, null },
        };
    internal Dictionary<Version, string[]> TileTypeNames = new Dictionary<Version, string[]> {
        {Version.BC, null },
        {Version.O, null },
        {Version.JJ2, null },
        {Version.TSF, null },
        {Version.AGA, null },
        {Version.GorH, null },
        };
    internal Dictionary<Version, int?> TileTypeAtlas = new Dictionary<Version, int?> {
        {Version.BC, null },
        {Version.O, null },
        {Version.JJ2, null },
        {Version.TSF, null },
        {Version.AGA, null },
        {Version.GorH, null },
        };
    internal Dictionary<Version, string[][]> IniEventListing = new Dictionary<Version, string[][]> {
        {Version.BC, null },
        {Version.O, null },
        {Version.JJ2, null },
        {Version.TSF, null },
        {Version.AGA, null },
        {Version.GorH, null },
        };

    public void Generate_Blank_Tile_Texture()
    {
        workingAtlases[0] = new byte[4096];
        pixel = new byte[4] { Tile0Color.R, Tile0Color.G, Tile0Color.B, 255 };
        for (ushort i = 0; i < 4096; i++) workingAtlases[0][i] = pixel[i % 4];
        Atlases[0] = TexUtil.CreateRGBATexture(32, 32, workingAtlases[0]);
        AtlasLength = 1;
        AtlasFraction = 1;
        TexturesHaveBeenGenerated = true;
    }
    /* public void Old_Generate_Textures(TransparencySource source = TransparencySource.JJ2_Style, bool PinkTransparency = true)
    {
        //texturenumberlist = new int[J2T.TileCount];
        //VBOid = new uint[J2T.TileCount*2];
        //GL.GenBuffers((int)J2T.TileCount, VBOid);
        for (ushort a = 0; a < J2T.TileCount; a+=1030)
        {
            workingAtlas = new byte[4096 * Math.Min(1030,(J2T.TileCount-a))];
            for (ushort i = a; i < a+1030 && i < J2T.TileCount; i++)
            {
                tile = J2T.Images[J2T.ImageAddress[i]];
                transp = TileTypes[i] == 1;
                for (short j = 0; j < 4096; j += 4)
                {
                    pixel = J2T.Palette[(int)tile[j / 4]];
                    for (byte k = 0; k < 4; k++) workingAtlas[i % 10 * 128 + i % 1030 / 10 * 40960 + j % 128 + j / 128 * 1280 + k] = (k == 3) ? (((source == TransparencySource.JCS_Style) ? J2T.TransparencyMaskJCS_Style : J2T.TransparencyMaskJJ2_Style)[Array.BinarySearch(J2T.TransparencyMaskOffset, 0, (int)J2T.data3Counter, J2T.TransparencyMaskAddress[i])][j / 4] == 1) ? ((transp) ? (byte)192 : (byte)255) : (byte)0 : (((source == TransparencySource.JCS_Style) ? J2T.TransparencyMaskJCS_Style : J2T.TransparencyMaskJJ2_Style)[Array.BinarySearch(J2T.TransparencyMaskOffset, 0, (int)J2T.data3Counter, J2T.TransparencyMaskAddress[i])][j / 4] == 1) ? pixel[k] : (i == 0 && k == 0) ? (byte)72 : PinkTransparentColors[k] ;
                }
                //texturenumberlist[i] = TexUtil.CreateRGBATexture(32, 32, alphatile);
            }
            AtlasID[a / 1030] = TexUtil.CreateRGBATexture(320, 32 * (workingAtlas.Length / 40960), workingAtlas);
            AtlasRatio[a/1030] = 1d / (workingAtlas.Length/40960);
            //for (ushort i = a; i < a+1030 && i < J2T.TileCount; i++) for (short j = 0; j < 4096; j += 4) workingAtlas[j] = J2T.Masks
        }
        TexturesHaveBeenGenerated = true;
    } */
    public byte GetLevelFromColor(Color color, byte level)
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
        for (byte i = 0; i < 5; i++) if (J2T.TileCount < 16 << (i * 2)) { AtlasLength = 128 << i; workingAtlases[0] = new byte[AtlasLength * AtlasLength * 4]; if (includeMasks) workingAtlases[1] = new byte[AtlasLength * AtlasLength * 4]; AtlasLength /= 32; AtlasFraction = 1.0d / AtlasLength; break; }
        for (ushort i = 0; i < J2T.TileCount; i++)
        {
            tile = J2T.Images[J2T.ImageAddress[i]];
            tileTrans = transSource[Array.BinarySearch(J2T.TransparencyMaskOffset, 0, (int)J2T.data3Counter, J2T.TransparencyMaskAddress[i])];
            mask = J2T.Masks[J2T.MaskAddress[i]];
            transp = TileTypes[i] == 1;
            for (short j = 0; j < 4096; j += 4)
            {
                pixel = palette[(int)tile[j / 4]];
                if (includeMasks) for (byte k = 0; k < 4; k++)
                {
                    atlasDrawingLocation = i % AtlasLength * 128 + i / AtlasLength * AtlasLength * 4096 + j % 128 + j / 128 * AtlasLength * 128 + k;
                    workingAtlases[0][atlasDrawingLocation] = (k == 3) ? (tileTrans[j / 4] == 1) ? ((transp) ? (byte)192 : (byte)255) : (byte)0 : (tileTrans[j / 4] == 1) ? pixel[k] : GetLevelFromColor(usedColor, k);
                    workingAtlases[1][atlasDrawingLocation] = (k == 3) ? (mask[j / 4] == 1) ? (byte)196 : (byte)0 : (mask[j / 4] == 1) ? (byte)0 : GetLevelFromColor(usedColor, k);
                }
                else for (byte k = 0; k < 4; k++) workingAtlases[0][i % AtlasLength * 128 + i / AtlasLength * AtlasLength * 4096 + j % 128 + j / 128 * AtlasLength * 128 + k] = (k == 3) ? (tileTrans[j / 4] == 1) ? ((transp) ? (byte)192 : (byte)255) : (byte)0 : (tileTrans[j / 4] == 1) ? pixel[k] : GetLevelFromColor(usedColor, k);
            }
            if (i == 0) usedColor = TranspColor;
        }
        Atlases[0] = TexUtil.CreateRGBATexture(AtlasLength * 32, AtlasLength * 32, workingAtlases[0]);
        if (includeMasks) Atlases[1] = TexUtil.CreateRGBATexture(AtlasLength * 32, AtlasLength * 32, workingAtlases[1]);
        TexturesHaveBeenGenerated = true;
    }
    public void Degenerate_Textures()
    {
        if (TexturesHaveBeenGenerated)
        {
            GL.DeleteTexture(Atlases[0]);
        }
        TexturesHaveBeenGenerated = false;
    }
    public VersionChangeResults ChangeTileset(string filename, bool avoidRedundancy = true, Dictionary<Version, string> defaultDirectories = null, Palette overridePalette = null)
    {
        if (avoidRedundancy && Path.GetFileName(filename) == Tileset) return VersionChangeResults.Success;
        else {
            VersionChangeResults result = base.ChangeTileset(filename, avoidRedundancy, defaultDirectories);
            if (result == VersionChangeResults.Success) { if (TexturesHaveBeenGenerated) Degenerate_Textures(); Generate_Textures(TransparencySource.JJ2_Style, true, overridePalette); }
            return result;
        }
    }
    public void ProduceEventAndTypeListsFromIni(Version version, IniFile eventIni, IniFile typeIni, bool overwriteOldLists = false)
    {
        if (!overwriteOldLists && !(EventAtlas[version] == null || IniEventListing[version] == null)) return;
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
            TileTypeAtlas[version] = TexUtil.CreateTextureFromBitmap(type_bmp);
        }
    }
    /*public static string[][] Old_ReadEventIni(string filename)
    {
        IniFile ini = new IniFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename));
        string[][] StringList = new string[256][];
        for (int i = 0; i < 256; i++) if (ini.IniReadValue("Events", i.ToString()) != "")
            {
                StringList[i] = (ini.IniReadValue("Events", i.ToString()).Split('|'));
                for (byte j = 0; j < StringList[i].Length; j++) StringList[i][j] = StringList[i][j].TrimEnd();
            }
        else StringList[i] = new string[] { (i == 0) ? "(none)" : "Event " + i.ToString(), "-", "", "DON'T", "USE" };
        return StringList;
    }*/
    /*public static void Old_GenerateEventNameTextures(ref int[] eventtexturenumberlist, string[][] eventsfromini)
        {
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center; format.LineAlignment = StringAlignment.Center;
            SolidBrush white = new SolidBrush(Color.White) ;
            using (Font arial = new Font(new FontFamily("Arial"), 8)) using (text_bmp) using (format)
            {
                for (ushort i = 0; i < 4096; i++) eventbackdrop[i] = (i % 4 == 3) ? (byte)164 : (byte)0;
                for (int i = 1; i < 256; i++) if (eventsfromini[i] != null)
                {
                        text_bmp = new Bitmap(32, 32);
                        Graphics gfx = Graphics.FromImage(text_bmp);
                        gfx.Clear(Color.FromArgb(128, 0, 0, 0));
                        if (eventsfromini[i].Length > 4 && eventsfromini[i][4].TrimEnd() != "") gfx.DrawString(eventsfromini[i][3] + "\n" + eventsfromini[i][4], arial, white, rectE, format);
                        else gfx.DrawString(eventsfromini[i][3], arial, white, rectE, format);
                        eventtexturenumberlist[i] = TexUtil.CreateTextureFromBitmap(text_bmp);
                }
                text_bmp.Dispose();
            }
        }*/
}