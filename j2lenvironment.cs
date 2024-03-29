﻿using System;
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
    internal static Dictionary<Version, int> EventSpriteAtlas = new Dictionary<Version, int> {
        {Version.BC, 0 },
        {Version.O, 0 },
        {Version.JJ2, 0 },
        {Version.TSF, 0 },
        //{Version.AGA, 0 },
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
    static Color CommonTransparencyTransformation(byte[] pixel, byte tileType)
    {
        return Color.FromArgb((tileType != 1) ? byte.MaxValue : 192, pixel[0], pixel[1], pixel[2]);
    }
    static Color ghTransparencyTransformation(byte[] pixel, byte tileType)
    {
        return Color.FromArgb((tileType < 1 || tileType > 3) ? byte.MaxValue : 192, pixel[0], pixel[1], pixel[2]);
    }
    static Color plusTransparencyTransformation(byte[] pixel, byte tileType)
    {
        if (tileType == 3) //invisible
            return Color.FromArgb(0, pixel[0], pixel[1], pixel[2]);
        //if (tileType == 5) //heat effect
        //?
        if (tileType == 6)
        { //frozen
            int brightness = (7499 * pixel[2] + pixel[0] + 2 * (pixel[0] + 2 * (pixel[0] + 288 * 17 * pixel[0])) + 38446 * pixel[1]) >> 16;
            return Color.FromArgb(128, brightness >> 1, Math.Min(32 + (brightness << 1), byte.MaxValue), Math.Min(brightness * brightness + 32, byte.MaxValue));
        }
        return CommonTransparencyTransformation(pixel, tileType);
    }
    static Dictionary<Version, Func<byte[], byte, Color>> TileTypeColorTransformations = new Dictionary<Version, Func<byte[], byte, Color>> {
        {Version.BC, CommonTransparencyTransformation},
        {Version.O, CommonTransparencyTransformation},
        {Version.JJ2, plusTransparencyTransformation},
        {Version.TSF, plusTransparencyTransformation},
        {Version.AGA, CommonTransparencyTransformation},
        {Version.GorH, ghTransparencyTransformation},
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

    public uint getTileInTilesetID(uint tileInLevelID, out J2TFile J2T)
    {
        uint tileInTilesetID = tileInLevelID;
        int tilesetID = 0;
        while (true)
        {
            J2T = Tilesets[tilesetID++];
            if (tileInTilesetID >= J2T.TileCount)
                tileInTilesetID -= J2T.TileCount;
            else
                break;
        }
        return tileInTilesetID + J2T.FirstTile;
    }
    public void Generate_Textures(TransparencySource source = TransparencySource.JJ2_Style, bool includeMasks = false, Palette palette = null)
    {
        Color usedColor = Tile0Color;
        var transformation = TileTypeColorTransformations[VersionType];
        if (palette == null)
            palette = Palette;
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
        for (ushort tileInLevelID = 0; tileInLevelID < TileCount; tileInLevelID++)
        {
            J2TFile J2T;
            uint tileInTilesetID = getTileInTilesetID(tileInLevelID, out J2T);

            bool customTileImage;
            byte[] tileTrans;
            byte[] tile = PlusPropertyList.TileImages[tileInLevelID];
            if (!(customTileImage = (tile != null)))
            {
                tile = J2T.Images[J2T.ImageAddress[tileInTilesetID]];
                tileTrans = ((source == TransparencySource.JJ2_Style) ? J2T.TransparencyMaskJJ2_Style : J2T.TransparencyMaskJCS_Style)[Array.BinarySearch(J2T.TransparencyMaskOffset, 0, (int)J2T.data3Counter, J2T.TransparencyMaskAddress[tileInTilesetID])];
            } else
                tileTrans = tile;
            var colorRemapping = (J2T.ColorRemapping == null || customTileImage) ? J2TFile.DefaultColorRemapping : J2T.ColorRemapping;

            var mask = PlusPropertyList.TileMasks[tileInLevelID] ?? J2T.Masks[J2T.MaskAddress[tileInTilesetID]];

            for (short j = 0; j < 32*32*4; j += 4)
            {
                bool transparentPixel = tileTrans[j / 4] == 0;
                var color = Palette.Convert(!transparentPixel ? transformation(palette[colorRemapping[tile[j / 4]]], TileTypes[tileInLevelID]) : usedColor, true);
                if (transparentPixel)
                    color[3] = 0;
                for (byte k = 0; k < 4; k++)
                {
                    int atlasDrawingLocation = tileInLevelID % AtlasLength * 128 + tileInLevelID / AtlasLength * AtlasLength * 4096 + j % 128 + j / 128 * AtlasLength * 128 + k;
                    workingAtlases[0][atlasDrawingLocation] = color[k];
                    if (includeMasks)
                        workingAtlases[1][atlasDrawingLocation] = (k == 3) ? (mask[j / 4] == 1) ? (byte)196 : (byte)0 : (mask[j / 4] == 1) ? (byte)0 : GetLevelFromColor(usedColor, k);
                }
            }

            if (tileInLevelID == 0) usedColor = TranspColor;
        }
        ImageAtlas = TexUtil.CreateRGBATexture(AtlasLength * 32, AtlasLength * 32, workingAtlases[0]);
        if (includeMasks) MaskAtlas = TexUtil.CreateRGBATexture(AtlasLength * 32, AtlasLength * 32, workingAtlases[1]);
    }
    public Bitmap[] RenderTilesetAsImage(TransparencySource source, bool includeMasks, Palette palette = null) //very similar to TilesetForm::CreateImageFromTileset
    {
        uint imageHeight = (TileCount + 9) / 10 * 32;

        var image = new Bitmap(320, (int)imageHeight, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
        (palette ?? Palette).Apply(image, Color.FromArgb(87, 0, 203));
        var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
        byte[] bytes = new byte[data.Height * data.Stride];

        Bitmap mask = null;
        System.Drawing.Imaging.BitmapData maskData = null;
        byte[] maskBytes = null;
        if (includeMasks)
        {
            mask = new Bitmap(320, (int)imageHeight, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            maskData = mask.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            maskBytes = new byte[maskData.Height * maskData.Stride];
            var paletteM = mask.Palette;
            paletteM.Entries[0] = Color.FromArgb(87, 0, 203);
            paletteM.Entries[1] = Color.Black;
            mask.Palette = image.Palette;
        }

        for (ushort tileInLevelID = 0; tileInLevelID < TileCount; tileInLevelID++)
        {
            J2TFile J2T;
            uint tileInTilesetID = getTileInTilesetID(tileInLevelID, out J2T);

            bool customTileImage;
            byte[] tileTrans;
            byte[] tile = PlusPropertyList.TileImages[tileInLevelID];
            if (!(customTileImage = (tile != null)))
            {
                tile = J2T.Images[J2T.ImageAddress[tileInTilesetID]];
                tileTrans = ((source == TransparencySource.JJ2_Style) ? J2T.TransparencyMaskJJ2_Style : J2T.TransparencyMaskJCS_Style)[Array.BinarySearch(J2T.TransparencyMaskOffset, 0, (int)J2T.data3Counter, J2T.TransparencyMaskAddress[tileInTilesetID])];
            }
            else
                tileTrans = tile;
            var colorRemapping = (J2T.ColorRemapping == null || customTileImage) ? J2TFile.DefaultColorRemapping : J2T.ColorRemapping;
            
            var xOrg = (tileInLevelID % 10) * 32;
            var yOrg = tileInLevelID / 10 * 32;
            for (uint x = 0; x < 32; ++x)
                for (uint y = 0; y < 32; ++y)
                {
                    uint xy = x + y * 32;
                    if (tileTrans[xy] != 0)
                        bytes[xOrg + x + (yOrg + y) * data.Stride] = colorRemapping[tile[xy]];
                }

            if (includeMasks)
            {
                byte[] tileMask = PlusPropertyList.TileMasks[tileInLevelID] ?? J2T.Masks[J2T.MaskAddress[tileInTilesetID]];
                xOrg = (tileInLevelID % 10) * 32 / 8;
                yOrg = tileInLevelID / 10 * 32;
                for (uint x = 0; x < 32; ++x)
                    for (uint y = 0; y < 32; ++y)
                        if (tileMask[x + y * 32] != 0)
                            maskBytes[xOrg + (x >> 3) + (yOrg + y) * maskData.Stride] |= (byte)(0x80 >> (int)(x & 7));
            }
        }

        Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
        image.UnlockBits(data);
        if (includeMasks)
        {
            Marshal.Copy(maskBytes, 0, maskData.Scan0, maskBytes.Length);
            mask.UnlockBits(maskData);
        }
        return new Bitmap[] { image, mask };
    }
    public void RerenderTile(uint tileInLevelID)
    {
        var transformation = TileTypeColorTransformations[VersionType];
        Palette palette = Palette;
        var transparentColor = Color.FromArgb(
            0,
            GetLevelFromColor(TranspColor, 0),
            GetLevelFromColor(TranspColor, 1),
            GetLevelFromColor(TranspColor, 2)
        );
        using (Bitmap bmp = new Bitmap(32, 32))
        {
            byte[] newTile = PlusPropertyList.TileImages[tileInLevelID];
            if (newTile == null) {
                J2TFile J2T;
                uint tileInTilesetID = getTileInTilesetID(tileInLevelID, out J2T);

                byte[] oldTile = J2T.Images[J2T.ImageAddress[tileInTilesetID]];
                byte[] tileTrans = J2T.TransparencyMaskJJ2_Style[Array.BinarySearch(J2T.TransparencyMaskOffset, 0, (int)J2T.data3Counter, J2T.TransparencyMaskAddress[tileInTilesetID])];

                var colorRemapping = J2T.ColorRemapping ?? J2TFile.DefaultColorRemapping;
                for (int x = 0; x < 32; x++)
                    for (int y = 0; y < 32; y++)
                        if (tileTrans[x + y * 32] == 0)
                            bmp.SetPixel(x, y, transparentColor);
                        else
                            bmp.SetPixel(x, y, transformation(palette[colorRemapping[oldTile[x + y * 32]]], TileTypes[tileInLevelID]));
            } else { //for angelscript-edited tile images, there is no distinction between image and transparency
                for (int x = 0; x < 32; x++)
                    for (int y = 0; y < 32; y++)
                        if (newTile[x + y * 32] == 0)
                            bmp.SetPixel(x, y, transparentColor);
                        else
                            bmp.SetPixel(x, y, transformation(palette[newTile[x + y * 32]], TileTypes[tileInLevelID]));
            }
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, 32, 32), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, (int)tileInLevelID % AtlasLength * 32, (int)tileInLevelID / AtlasLength * 32, 32, 32, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);
        }
    }
    public void RerenderTileMask(uint tileInLevelID)
    {
        var colors = new Color[2] { Color.FromArgb(
            0,
            GetLevelFromColor(TranspColor, 0),
            GetLevelFromColor(TranspColor, 1),
            GetLevelFromColor(TranspColor, 2)
        ), Color.Black };
        byte[] tileMask = PlusPropertyList.TileMasks[tileInLevelID];
        if (tileMask == null)
        {
            J2TFile J2T;
            uint tileInTilesetID = getTileInTilesetID(tileInLevelID, out J2T);
            tileMask = J2T.Masks[J2T.MaskAddress[tileInTilesetID]];
        }
        using (Bitmap bmp = new Bitmap(32, 32))
        {
            for (int x = 0; x < 32; x++)
                for (int y = 0; y < 32; y++)
                    bmp.SetPixel(x, y, colors[tileMask[x + y * 32]]);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, 32, 32), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, (int)tileInLevelID % AtlasLength * 32, (int)tileInLevelID / AtlasLength * 32, 32, 32, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);
        }
    }

    public VersionChangeResults ChangeTileset(string filename, bool avoidRedundancy = true, Dictionary<Version, string> defaultDirectories = null, Palette overridePalette = null)
    {
        if (avoidRedundancy && Path.GetFileName(filename) == MainTilesetFilename) return VersionChangeResults.Success;
        else {
            VersionChangeResults result = base.ChangeTileset(filename, avoidRedundancy, defaultDirectories);
            if (result == VersionChangeResults.Success) { if (TexturesHaveBeenGenerated) Generate_Textures(TransparencySource.JJ2_Style, true, overridePalette); }
            return result;
        }
    }
    public static string[] GetDontUseEventListingForEventID(byte eventID)
    {
        return new string[] { (eventID == 0) ? "(none)" : "Event " + eventID.ToString(), "-", "", "DON'T", "USE" };
    }
    public static void ProduceEventStringsFromIni(Version version, IniFile eventIni, IniFile typeIni, bool overwriteOldLists = false)
    {
        if (!overwriteOldLists && IniEventListing[version] != null)
            return;
        string[][] StringList = IniEventListing[version] = new string[256][];
        for (int i = 0; i < 256; i++)
        {
            if (eventIni.IniReadValue("Events", i.ToString()) != "")
            {
                StringList[i] = (((eventIni != typeIni && typeIni.IniReadValue("Events", i.ToString()).Length > 0) ? typeIni : eventIni).IniReadValue("Events", i.ToString()).Split('|'));
                for (byte j = 0; j < StringList[i].Length; j++) StringList[i][j] = StringList[i][j].TrimEnd();
            }
            else StringList[i] = GetDontUseEventListingForEventID((byte)i);
        }
    }
    public static void ProduceTypeIcons(Version version, IniFile typeIni, bool overwriteOldImage = false)
    {
        if (!overwriteOldImage && TileTypeAtlas[version] != 0)
            return;
        
        RectangleF rectT = new RectangleF(-2, 0, 256, 32);

        using (SolidBrush white = new SolidBrush(Color.White))
        using (Font arial = new Font(new FontFamily("Arial"), 8))
        using (Bitmap type_bmp = new Bitmap(128, 128))
        using (Graphics totalgfx = Graphics.FromImage(type_bmp))
        using (StringFormat formatType = new StringFormat())
        {
            formatType.LineAlignment = StringAlignment.Center;
            formatType.Alignment = StringAlignment.Near;
            totalgfx.Clear(Color.FromArgb(128, 0, 0, 0));
            TileTypeNames[version] = new string[16];

            for (int i = 1; i < 16; i++)
                if ((TileTypeNames[version][i] = typeIni.IniReadValue("Tiles", i.ToString()) ?? "") != "")
                    using (Bitmap single_bmp = new Bitmap(32, 32))
                    using (Graphics gfx = Graphics.FromImage(single_bmp))
                    {
                        gfx.DrawString(typeIni.IniReadValue("Tiles", i.ToString()), arial, white, rectT, formatType);
                        totalgfx.DrawImage(single_bmp, i % 4 * 32, i / 4 * 32);
                    }
            if (TileTypeAtlas[version] != 0)
                GL.DeleteTexture(TileTypeAtlas[version]);
            TileTypeAtlas[version] = TexUtil.CreateTextureFromBitmap(type_bmp);
        }
    }
    public static void ProduceEventIcons(Version version, string[][] StringList, ref bool[] whichAreDrawnAsText, ref System.Windows.Forms.ImageList treeImageList, bool overwriteOldImage = false, byte? GeneratorEventID = null, string spriteFilename = "")
    {
        if (!overwriteOldImage && EventAtlas[version] != 0)
            return;
        if (whichAreDrawnAsText == null)
            whichAreDrawnAsText = new bool[256];

        RectangleF rectE = new RectangleF(-16, 0, 64, 32);

        using (SolidBrush white = new SolidBrush(Color.White))
        using (Font arial = new Font(new FontFamily("Arial"), 8))
        using (Bitmap text_bmp = new Bitmap(512, 512))
        using (Bitmap text_bmp2 = (spriteFilename != "" && EventSpriteAtlas.ContainsKey(version)) ? new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLLE Event Sprites - " + spriteFilename + ".png")) : new Bitmap(1, 1))
        using (Graphics totalgfx = Graphics.FromImage(text_bmp))
        using (Graphics totalgfx2 = Graphics.FromImage(text_bmp2))
        using (StringFormat formatEvent = new StringFormat())
        {
            formatEvent.Alignment = formatEvent.LineAlignment = StringAlignment.Center;
            totalgfx.Clear(Color.FromArgb(128, 0, 0, 0));
            treeImageList.Images.Add(new Bitmap(1, 1)); //event 0
            for (int i = 1; i < 256; i++)
            {
                if (i != GeneratorEventID)
                {
                    using (Bitmap single_bmp = new Bitmap(32, 32))
                    using (Graphics gfx = Graphics.FromImage(single_bmp))
                    {
                        if (StringList[i].Length > 4 && StringList[i][4].TrimEnd() != "")
                            gfx.DrawString(StringList[i][3] + "\n" + StringList[i][4], arial, white, rectE, formatEvent);
                        else
                            gfx.DrawString(StringList[i][3], arial, white, rectE, formatEvent);
                        totalgfx.DrawImage(single_bmp,i%16*32, i/16*32);
                        if (text_bmp2.Width == 1024) //not AGA
                            if (whichAreDrawnAsText[i] || ((text_bmp2.GetPixel(i % 16 * 64 + 32, i / 16 * 64 + 32).A == 0) && (whichAreDrawnAsText[i] = true))) { //a custom event, or else there's no sprite defined for this event, as measured by checking the middle pixel
                                int x = i % 16 * 64 + 16, y = i / 16 * 64 + 16;
                                totalgfx2.Clip = new Region(new Rectangle(x - 16, y - 16, 64, 64));
                                totalgfx2.Clear(Color.Transparent);
                                totalgfx2.ResetClip();
                                totalgfx2.FillRegion(new SolidBrush(Color.FromArgb(128, 0, 0, 0)), new Region(new Rectangle(x, y, 32, 32)));
                                totalgfx2.DrawImage(single_bmp, x, y, 32, 32); //use the standard text preview instead
                                treeImageList.Images.Add(new Bitmap(1,1));
                            } else {
                                int size = treeImageList.ImageSize.Height; //same as width
                                Bitmap dst = new Bitmap(size, size);
                                int srcLeft = i % 16 * 64;
                                int srcTop = i / 16 * 64;
                                var bottom = 0;
                                var left = 64;
                                var right = 0;
                                var top = 64;

                                var bmpData = text_bmp2.LockBits(new Rectangle(srcLeft, srcTop, 64, 64), System.Drawing.Imaging.ImageLockMode.ReadOnly, text_bmp2.PixelFormat);
                                unsafe //https://stackoverflow.com/questions/248141/remove-surrounding-whitespace-from-an-image
                                {
                                    var dataPtr = (byte*)bmpData.Scan0;

                                    for (var y = 0; y < 64; y++)
                                    {
                                        for (var x = 0; x < 64; x++)
                                        {
                                            if ((dataPtr + x*4)[3] != 0)
                                            {
                                                if (x < left)
                                                    left = x;

                                                if (x >= right)
                                                    right = x + 1;

                                                if (y < top)
                                                    top = y;

                                                if (y >= bottom)
                                                    bottom = y + 1;
                                            }
                                        }

                                        dataPtr += bmpData.Stride;
                                    }
                                }
                                text_bmp2.UnlockBits(bmpData);
                                if (right != 0)
                                {
                                    int width = right - left;
                                    int height = bottom - top;
                                    if (width < size)
                                    {
                                        left -= (size - width) / 2;
                                        width = size;
                                    }
                                    if (height < size)
                                    {
                                        top -= (size - height) / 2;
                                        height = size;
                                    }
                                    if (width > height)
                                    {
                                        top -= (width - height) / 2;
                                        height = width;
                                    }
                                    else if (height > width)
                                    {
                                        left -= (height - width) / 2;
                                        width = height;
                                    }
                                    using (Graphics g = Graphics.FromImage(dst))
                                    {
                                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                        g.DrawImage(
                                            text_bmp2,
                                            new Rectangle(0, 0, dst.Width, dst.Height),
                                            new Rectangle(srcLeft + left, srcTop + top, width, height),
                                            GraphicsUnit.Pixel
                                        );
                                    }
                                    treeImageList.Images.Add(dst);
                                }
                                else
                                    treeImageList.Images.Add(new Bitmap(1, 1));
                            }
                    }
                }
                else
                {
                    totalgfx.Clip = new Region(new Rectangle(i % 16 * 32, i / 16 * 32, 32, 32));
                    totalgfx.Clear(Color.Transparent);
                    using (Bitmap single_bmp = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Generator.png")))
                    totalgfx.DrawImage(single_bmp, i % 16 * 32, i / 16 * 32);
                    totalgfx.ResetClip();
                    treeImageList.Images.Add(new Bitmap(1, 1));
                }
            }
            if (EventAtlas[version] != 0)
                GL.DeleteTexture(EventAtlas[version]);
            EventAtlas[version] = TexUtil.CreateTextureFromBitmap(text_bmp);
            if (text_bmp2.Width == 1024) //not AGA
            {
                if (EventSpriteAtlas[version] != 0)
                    GL.DeleteTexture(EventSpriteAtlas[version]);
                EventSpriteAtlas[version] = TexUtil.CreateTextureFromBitmap(text_bmp2);
            }
        }
    }
}