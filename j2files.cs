using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Ionic.Zlib;
using Ionic.Crc;
using Extra.Collections;

public enum Version { JJ2, TSF, O, GorH, BC, AGA, AmbiguousBCO, Plus };
public enum VersionChangeResults { Success, TilesetTooBig, TooManyAnimatedTiles, UnsupportedConversion };
public enum SavingResults { Success, UndefinedTiles, NoTilesetSelected, TilesetIsDifferentVersion, Error };
public enum OpeningResults { Success, SuccessfulButAmbiguous, PasswordNeeded, WrongPassword, UnexpectedFourCC, IncorrectEncoding, SecurityEnvelopeDamaged, Error };
public enum InsertFrameResults { Success, Full, StackOverflow };
public enum BuildResults { Success, DifferentDimensions, BadDimensions, ImageWrongFormat, Image32WrongFormat, MaskWrongFormat, TooBigForVersion, MaskNeedsPaletteFor32BitImages, VersionDoesNotSupport32BitImage };

abstract class J2File //The fields shared by .j2l and .j2t files. No methods/interface just yet, though that would be cool too.
{
    internal readonly static Encoding FileEncoding = Encoding.GetEncoding(1252); //Windows-1252
    internal readonly static string StandardHeader = "                      Jazz Jackrabbit 2 Data File\x0D\x0A\x0D\x0A         Retail distribution of this data is prohibited without\x0D\x0A             written permission from Epic MegaGames, Inc.\x0D\x0A\x0D\x0A\x1A";
    internal string Header; //The copyright notice
    internal string Magic; //"LEVL" or "TILE," depending
    public string Name; //Self-explanatory
    internal string FilenameOnly, FullFilePath;
    public uint Size; //Stored in the files for some reason
    public int Crc32; //To make sure the file hasn't been tampered with
    internal Version VersionType; //Using an enum, the ACTUAL version of each file. "BC" lumps in BC proper and also 1.10o since there is no internal distinction.
    internal int MaxTiles
    {
        get
        {
            switch (VersionType)
            {
                case Version.AGA:
                case Version.TSF:
                case Version.Plus:
                    return 4096;
                default:
                    return 1024;
            }
        }
    }
    public int[] CompressedDataLength = new int[4];
    public int[] UncompressedDataLength = new int[4];
    internal MemoryStream[] UncompressedData = new MemoryStream[4];
    internal MemoryStream[] CompressedData = new MemoryStream[4];
    public readonly static Dictionary<Version, string> FullVersionNames = new Dictionary<Version, string>() {
        {Version.TSF, "The Secret Files (v1.24)"},
        {Version.JJ2, "Jazz 2 (v1.20-1.23)"},
        {Version.BC, "Battery Check"},
        {Version.O, "Jazz 2 BETA v1.10o"},
        {Version.AmbiguousBCO, "Battery Check/Jazz 2 BETA v1.10o"},
        {Version.AGA, "Animaniacs: A Gigantic Adventure"},
        {Version.GorH, "Jazz 2 OEM v1.00g/h"} };

    protected static byte[] getBytes(Encoding encoding, string s, int length)
    {
        byte[] bytes = new byte[length];
        encoding.GetBytes(s, 0, s.Length, bytes, 0);
        return bytes;
    }
}

class J2TFile : J2File
{
    internal uint Signature;
    public Palette Palette;
    public uint TotalNumberOfTiles; //Again, good for looping.
    public uint FirstTile = 0, TileCount; //for multiple-tileset purposes
    public bool[] IsFullyOpaque; //Not too useful, but there's a shortcut bool to indicate that a tile has no transparency at ALL and may be drawn more simply.
    //internal byte[] unknown1;
    public uint[] ImageAddress; //Where in Data2 the 1024 pixels for each tile are. Here divided by 1024 just to be straight-forward.
    //internal uint[] unknown2;
    public uint[] TransparencyMaskAddress;
    internal uint[] TransparencyMaskOffset;
    //internal uint[] unknown3;
    public uint[] MaskAddress;
    //public uint[] FlippedMaskAddress;
    internal byte[][] TransparencyMaskJCS_Style;
    internal byte[][] TransparencyMaskJJ2_Style;
    internal byte[][] Images;
    internal ushort data3Counter = 0;
    internal byte[][] Masks;

    public byte[] ColorRemapping = null; //for levels with multiple tilesets
    public static readonly byte[] DefaultColorRemapping = Enumerable.Range(0, (int)Palette.PaletteSize).Select(val => (byte)val).ToArray();

    static internal bool[] Convert128BitsToBoolMask(byte[] bits)
    {
        bool[] output = new bool[1024];
        for (short i = 0; i < 1024; i++) output[i] = (bits[i / 8] & (byte)Math.Pow(2, (i % 8))) != 0;
        return output;
    }
    static internal byte[] Convert128BitsToByteMask(byte[] bits)
    {
        byte[] output = new byte[1024];
        for (short i = 0; i < 1024; i++) output[i] = ((bits[i / 8] & (byte)Math.Pow(2, (i % 8))) != 0) ? (byte)1 : (byte)0;
        return output;
    }
    static internal byte[] ConvertByteMaskTo128Bits(byte[] bytes)
    {
        byte[] output = new byte[128];
        for (int i = 0; i < 128; ++i)
        {
            byte val = 0;
            for (int j = 0; j < 8; ++j)
                val |= (byte)(bytes[(i << 3) | j] << j);
            output[i] = val;
        }
        return output;
    }
    static internal byte[] ProduceMasklessTileByteMask()
    { return Convert128BitsToByteMask(new byte[128] {
    15,15,15,15,15,15,15,15,
    15,15,15,15,15,15,15,15,
    240,240,240,240,240,240,240,240,
    240,240,240,240,240,240,240,240,
    15,15,15,15,15,15,15,15,
    15,15,15,15,15,15,15,15,
    240,240,240,240,240,240,240,240,
    240,240,240,240,240,240,240,240,
    15,15,15,15,15,15,15,15,
    15,15,15,15,15,15,15,15,
    240,240,240,240,240,240,240,240,
    240,240,240,240,240,240,240,240,
    15,15,15,15,15,15,15,15,
    15,15,15,15,15,15,15,15,
    240,240,240,240,240,240,240,240,
    240,240,240,240,240,240,240,240
    });
    }

    public J2TFile() {}
    public J2TFile(string filename)
    {
        FilenameOnly = Path.GetFileName(FullFilePath = filename);
        Encoding encoding = FileEncoding;
        using (BinaryReader binreader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read), encoding))
        {
            #region header
            Header = (binreader.PeekChar() == 32) ? new string(binreader.ReadChars(180)) : "";
            Magic = new string(binreader.ReadChars(4));
            Signature = binreader.ReadUInt32();
            Name = new string(binreader.ReadChars(32)).TrimEnd('\0');
            ushort VersionNumber = binreader.ReadUInt16();
            Size = binreader.ReadUInt32();
            Crc32 = binreader.ReadInt32();
            for (byte i = 0; i < 4; i++)
            {
                CompressedDataLength[i] = binreader.ReadInt32();
                UncompressedDataLength[i] = binreader.ReadInt32();
            }
            #region setup version-specific sizes
            switch (VersionNumber)
            {
                case 0x200:
                    if (UncompressedDataLength[0] == 107524)
                    {
                        VersionType = Version.AGA;
                    }
                    else if (Header.Length == 0)
                    {
                        VersionType = Version.AmbiguousBCO;
                    }
                    else
                    {
                        VersionType = Version.JJ2;
                    }
                    break;
                case 0x201:
                    VersionType = Version.TSF;
                    break;
                case 0x300:
                    VersionType = Version.Plus;
                    break;
            }
            IsFullyOpaque = new bool[MaxTiles];
            //unknown1 = new byte[MaxTiles];
            ImageAddress = new uint[MaxTiles];
            //unknown2 = new uint[MaxTiles];
            TransparencyMaskAddress = new uint[MaxTiles];
            TransparencyMaskOffset = new uint[MaxTiles];
            //unknown3 = new uint[MaxTiles];
            MaskAddress = new uint[MaxTiles];
            //FlippedMaskAddress = new uint[MaxTiles];
            if (VersionType != Version.Plus)
            {
                TransparencyMaskJCS_Style = new byte[MaxTiles][];
                TransparencyMaskJJ2_Style = new byte[MaxTiles][];
            }
            #endregion setup version-specific sizes
            for (byte i = 0; i < 4; i++)
                UncompressedData[i] = new MemoryStream(ZlibStream.UncompressBuffer(binreader.ReadBytes(CompressedDataLength[i])));
            #endregion header
            #region data1
            BinaryReader data1reader = new BinaryReader(UncompressedData[0], encoding);
            Palette = new Palette(data1reader);
            TileCount = TotalNumberOfTiles = data1reader.ReadUInt32();
            for (short i = 0; i < MaxTiles; i++) IsFullyOpaque[i] = data1reader.ReadBoolean();
            data1reader.BaseStream.Seek(MaxTiles * sizeof(byte), SeekOrigin.Current); // for(short i = 0; i < MaxTiles; i++) unknown1[i] = data1reader.ReadByte();
            for (short i = 0; i < MaxTiles; i++) ImageAddress[i] = data1reader.ReadUInt32();
            data1reader.BaseStream.Seek(MaxTiles * sizeof(UInt32), SeekOrigin.Current); // for (short i = 0; i < MaxTiles; i++) unknown2[i] = data1reader.ReadUInt32();
            if (VersionType != Version.Plus)
            {
                for (short i = 0; i < MaxTiles; i++) TransparencyMaskAddress[i] = data1reader.ReadUInt32();
                data1reader.BaseStream.Seek(MaxTiles * sizeof(UInt32), SeekOrigin.Current); // for (short i = 0; i < MaxTiles; i++) unknown3[i] = data1reader.ReadUInt32();
            }
            for (short i = 0; i < MaxTiles; i++) MaskAddress[i] = data1reader.ReadUInt32() / 128;
            data1reader.BaseStream.Seek(MaxTiles * sizeof(UInt32), SeekOrigin.Current); // for (short i = 0; i < MaxTiles; i++) FlippedMaskAddress[i] = data1reader.ReadUInt32() / 128;
            #endregion data1
            #region data2
            var imageData = UncompressedData[1].ToArray();
            var imageDict = new Dictionary<uint, uint>();
            var imageList = new List<byte[]>();
            for (int i = 0; i < MaxTiles; i++)
            {
                uint address = ImageAddress[i];
                if (!imageDict.ContainsKey(address))
                {
                    imageDict[address] = (uint)imageList.Count;
                    var sliceOfImageData = new byte[((address & 0x80000000) == 0) ? (32 * 32) : (32 * 32 * 4)];
                    Array.Copy(imageData, address & 0x7FFFFFFF, sliceOfImageData, 0, sliceOfImageData.Length);
                    imageList.Add(sliceOfImageData);
                }
                ImageAddress[i] = imageDict[address];
            }
            Images = imageList.ToArray();
            #endregion
            #region data3
            if (VersionType != Version.Plus)
            {
                BinaryReader data3reader = new BinaryReader(UncompressedData[2], encoding);
                uint data3Pointer = 0;
                while (data3Pointer < UncompressedDataLength[2])
                {
                    TransparencyMaskOffset[data3Counter] = data3Pointer;
                    TransparencyMaskJCS_Style[data3Counter] = Convert128BitsToByteMask(data3reader.ReadBytes(128)); data3Pointer += 128;
                    TransparencyMaskJJ2_Style[data3Counter] = new byte[1024];// for (ushort i = 0; i < 1024; i++) tmasksjj2[data3counter][i] = 0;
                    for (int row = 0; row < 32; row++)
                    {
                        int data3Skip = row * 32;
                        int data3BlockNumber = data3reader.ReadByte(); data3Pointer++;
                        for (int i = 0; i < data3BlockNumber; i++)
                        {
                            data3Skip += data3reader.ReadByte(); data3Pointer++;
                            int data3BlockNumber2 = data3Skip + data3reader.ReadByte(); data3Pointer++;
                            for (; data3Skip < data3BlockNumber2; data3Skip++) TransparencyMaskJJ2_Style[data3Counter][data3Skip] = 1;
                        }
                    }
                    data3Counter++;
                }
            }
            #endregion data3
            #region data4
            BinaryReader data4reader = new BinaryReader(UncompressedData[3], encoding);
            Masks = new byte[UncompressedDataLength[3] / 128][];
            for (short i = 0; i < UncompressedDataLength[3] / 128; i++) Masks[i] = Convert128BitsToByteMask(data4reader.ReadBytes(128));
            #endregion data4
        }
    }
    internal J2TFile(BinaryReader binreader) //for .LEV tilesets only, started partway through the early EDIT section
    {
        VersionType = Version.GorH;
        FilenameOnly = new string(binreader.ReadChars(binreader.ReadByte()));
        TileCount = TotalNumberOfTiles = binreader.ReadUInt32();

        data3Counter = (ushort)TotalNumberOfTiles;
        IsFullyOpaque = new bool[1024];
        //unknown1 = new byte[1024];
        ImageAddress = new uint[1024];
        Images = new byte[TotalNumberOfTiles][];
        //unknown2 = new uint[1024];
        TransparencyMaskAddress = new uint[1024];
        TransparencyMaskOffset = new uint[1024];
        for (ushort i = 0; i < TotalNumberOfTiles; i++)
            ImageAddress[i] = TransparencyMaskAddress[i] = TransparencyMaskOffset[i] = i;
        //unknown3 = new uint[1024];
        MaskAddress = new uint[1024];
        //FlippedMaskAddress = new uint[1024];
        TransparencyMaskJCS_Style = new byte[1024][];
        TransparencyMaskJJ2_Style = new byte[1024][];
        Images[0] = TransparencyMaskJCS_Style[0] = TransparencyMaskJJ2_Style[0] = new byte[1024];
    }
    internal void ReadFromTILEDATA(BinaryReader binreader, byte[] TileTypes)
    {
        byte rawTransBits, infoByte, elapsedRowDistance, rowTarget;
        int pixelDestination;
        for (ushort tile = 1; tile < TotalNumberOfTiles; tile++)
        {
            Images[tile] = new byte[1024];
            TransparencyMaskJCS_Style[tile] = new byte[1024];
            TransparencyMaskJJ2_Style[tile] = new byte[1024];
            rawTransBits = binreader.ReadByte();
            IsFullyOpaque[tile] = (rawTransBits == 240 && TileTypes[tile] == 0);
            //if (IsFullyOpaque[tile]) Console.WriteLine(tile);
            bool[] quadrantIsNontransparent = new bool[4];
            for (byte i = 0; i < 4; i++) quadrantIsNontransparent[i] = ((rawTransBits & (16 << i)) != 0);
            for (byte quadrant = 0; quadrant < 4; quadrant++)
            {
                if (quadrantIsNontransparent[quadrant])
                {
                    for (ushort pixel = 0; pixel < 256; pixel++)
                    {
                        pixelDestination = pixel % 16 + quadrant % 2 * 16 + pixel / 16 * 32 + quadrant / 2 * 512;
                        Images[tile][pixelDestination] = binreader.ReadByte();
                        TransparencyMaskJCS_Style[tile][pixelDestination] = TransparencyMaskJJ2_Style[tile][pixelDestination] = (byte)((TileTypes[tile] > 0 && Images[tile][pixelDestination] == 1) ? 0 : 1);
                    }
                }
                else
                {
                    binreader.ReadBytes(6); //size, width, height
                    for (byte row = 0; row < 16; row++)
                    {
                        elapsedRowDistance = 0;
                        while (true)
                        {
                            infoByte = binreader.ReadByte();
                            if (infoByte == 128) break;
                            rowTarget = (byte)((infoByte & 31) + elapsedRowDistance);
                            if ((infoByte & 128) == 128)
                            {
                                for (; elapsedRowDistance < rowTarget; elapsedRowDistance++)
                                {
                                    pixelDestination = elapsedRowDistance + quadrant % 2 * 16 + row * 32 + quadrant / 2 * 512;
                                    Images[tile][pixelDestination] = binreader.ReadByte();
                                    TransparencyMaskJCS_Style[tile][pixelDestination] = TransparencyMaskJJ2_Style[tile][pixelDestination] = 1;
                                }
                            }
                            else elapsedRowDistance = rowTarget;
                        }
                    }
                }
            }

        }

    }
    internal void ReadFromMASK(BinaryReader binreader)
    {
        ushort newTile, lessTile = 0;
        ushort oldTile = 1;
        Masks = new byte[TotalNumberOfTiles][];
        Masks[0] = new byte[1024];
        while (true)
        {
            newTile = binreader.ReadUInt16();
            if (newTile == 0xFFFF)
            {
                for (; oldTile < TotalNumberOfTiles; oldTile++) MaskAddress[oldTile] = lessTile;
                break;
            }
            else
            {
                for (; oldTile < (newTile & 1023); oldTile++)
                {
                    if (lessTile == 0) { Masks[oldTile] = J2TFile.ProduceMasklessTileByteMask(); lessTile = oldTile; }
                    MaskAddress[oldTile] = lessTile;
                }
                if ((newTile & 32768) == 32768)
                {
                    MaskAddress[newTile & 1023] = binreader.ReadUInt16();
                }
                else
                {
                    Masks[newTile] = J2TFile.Convert128BitsToByteMask(binreader.ReadBytes(128));
                    MaskAddress[newTile] = newTile;
                }
                oldTile++;
            }
        }
    }
    internal void WriteToTDATA(BinaryWriter TDATA, byte[] TileTypes)
    {
        byte[] tile, transtile;
        long preQuadrantOffset;
        byte columnCount;
        byte previousTransp;
        for (ushort i = 1; i < TotalNumberOfTiles; i++)
        {
            transtile = TransparencyMaskJCS_Style[Array.BinarySearch(TransparencyMaskOffset, 0, (int)data3Counter, TransparencyMaskAddress[i])];
            tile = Images[ImageAddress[i]];
            bool[] quadrantIsNontransparent = new bool[4];
            for (byte quadrant = 0; quadrant < 4; quadrant++)
            {
                quadrantIsNontransparent[quadrant] = true;
                if (TileTypes[i] == 0) for (ushort pixel = 0; pixel < 256; pixel++) if (transtile[pixel % 16 + pixel / 16 * 32 + quadrant % 2 * 16 + quadrant / 2 * 512] == 0) { quadrantIsNontransparent[quadrant] = false; break; }
            }
            TDATA.Write((byte)(((quadrantIsNontransparent[0]) ? 16 : 0) | ((quadrantIsNontransparent[1]) ? 32 : 0) | ((quadrantIsNontransparent[2]) ? 64 : 0) | ((quadrantIsNontransparent[3]) ? 128 : 0)));
            for (byte quadrant = 0; quadrant < 4; quadrant++)
            {
                if (quadrantIsNontransparent[quadrant])
                {
                    for (ushort j = 0; j < 256; j++)
                    {
                        byte pixel = tile[j % 16 + j / 16 * 32 + quadrant % 2 * 16 + quadrant / 2 * 512];
                        TDATA.Write((TileTypes[i] > 0 && pixel == 0) ? (byte)1 : pixel);
                    }
                }
                else
                {
                    preQuadrantOffset = TDATA.BaseStream.Length;
                    TDATA.Write(new byte[2]);
                    TDATA.Write((ushort)16); TDATA.Write((ushort)16);
                    for (byte row = 0; row < 16; row++)
                    {
                        columnCount = 1;
                        previousTransp = (byte)(transtile[row * 32 + quadrant / 2 * 512 + quadrant % 2 * 16]);
                        for (byte column = 1; column < 16; column++)
                        {
                            switch (previousTransp)
                            {
                                case 0:
                                    switch (transtile[row * 32 + quadrant / 2 * 512 + column + quadrant % 2 * 16])
                                    {
                                        case 0:
                                            columnCount++;
                                            break;
                                        case 1:
                                            TDATA.Write(columnCount);
                                            previousTransp = 1;
                                            columnCount = 1;
                                            break;
                                    }
                                    break;
                                case 1:
                                    switch (transtile[row * 32 + quadrant / 2 * 512 + column + quadrant % 2 * 16])
                                    {
                                        case 0:
                                            TDATA.Write((byte)(columnCount | 128));
                                            for (byte p = 0; p < columnCount; p++) TDATA.Write(tile[row * 32 + quadrant / 2 * 512 + column - columnCount + p + quadrant % 2 * 16]);
                                            previousTransp = 0;
                                            columnCount = 1;
                                            break;
                                        case 1:
                                            columnCount++;
                                            break;
                                    }
                                    break;
                            }
                        }
                        if (previousTransp == 1) { TDATA.Write((byte)(columnCount | 128)); for (byte p = 0; p < columnCount; p++) TDATA.Write(tile[row * 32 + quadrant / 2 * 512 + 16 - columnCount + p + quadrant % 2 * 16]); }
                        TDATA.Write((byte)128);
                    }
                    TDATA.Seek((int)(preQuadrantOffset - TDATA.BaseStream.Length), SeekOrigin.End);
                    TDATA.Write((ushort)(TDATA.BaseStream.Length - preQuadrantOffset - 2));
                    TDATA.Seek(0, SeekOrigin.End);
                }
            }
        }
    }
    internal void WriteToEMSK(BinaryWriter EMSK)
    {
        bool[] shouldBeOpaque = new bool[8];
        for (ushort i = 0; i < TotalNumberOfTiles; i++)
        {
            var transtile = TransparencyMaskJJ2_Style[Array.BinarySearch(TransparencyMaskOffset, 0, (int)data3Counter, TransparencyMaskAddress[i])];
            for (byte block = 0; block < 8; block++)
            {
                for (byte bit = 0; bit < 8; bit++) shouldBeOpaque[bit] = (
                   transtile[bit * 4 + block * 128] == 1 &&
                   transtile[bit * 4 + block * 128 + 1] == 1 &&
                   transtile[bit * 4 + block * 128 + 2] == 1 &&
                   transtile[bit * 4 + block * 128 + 3] == 1 &&
                   transtile[bit * 4 + block * 128 + 32] == 1 &&
                   transtile[bit * 4 + block * 128 + 33] == 1 &&
                   transtile[bit * 4 + block * 128 + 34] == 1 &&
                   transtile[bit * 4 + block * 128 + 35] == 1 &&
                   transtile[bit * 4 + block * 128 + 64] == 1 &&
                   transtile[bit * 4 + block * 128 + 65] == 1 &&
                   transtile[bit * 4 + block * 128 + 66] == 1 &&
                   transtile[bit * 4 + block * 128 + 67] == 1 &&
                   transtile[bit * 4 + block * 128 + 96] == 1 &&
                   transtile[bit * 4 + block * 128 + 97] == 1 &&
                   transtile[bit * 4 + block * 128 + 98] == 1 &&
                   transtile[bit * 4 + block * 128 + 99] == 1
                   );
                EMSK.Write((byte)(
                    (shouldBeOpaque[0] ? 0 : 1) |
                    (shouldBeOpaque[1] ? 0 : 2) |
                    (shouldBeOpaque[2] ? 0 : 4) |
                    (shouldBeOpaque[3] ? 0 : 8) |
                    (shouldBeOpaque[4] ? 0 : 16) |
                    (shouldBeOpaque[5] ? 0 : 32) |
                    (shouldBeOpaque[6] ? 0 : 64) |
                    (shouldBeOpaque[7] ? 0 : 128)
                    ));
            }
        }
    }
    internal void WriteToMASK(BinaryWriter MASK, bool[] IsEachTileUsed)
    {
        ushort[] firstMaskInstance = new ushort[TotalNumberOfTiles];
        uint maskAddress;
        for (ushort i = 1; i < TotalNumberOfTiles; i++) if (IsEachTileUsed[i])
            {
                maskAddress = MaskAddress[i];
                if (firstMaskInstance[maskAddress] == 0)
                {
                    firstMaskInstance[maskAddress] = i;
                    MASK.Write(i);
                    for (ushort j = 0; j < 1024; j += 8)
                    {
                        MASK.Write((byte)(
                            (Masks[maskAddress][j + 0] == 1 ? 1 : 0) |
                            (Masks[maskAddress][j + 1] == 1 ? 2 : 0) |
                            (Masks[maskAddress][j + 2] == 1 ? 4 : 0) |
                            (Masks[maskAddress][j + 3] == 1 ? 8 : 0) |
                            (Masks[maskAddress][j + 4] == 1 ? 16 : 0) |
                            (Masks[maskAddress][j + 5] == 1 ? 32 : 0) |
                            (Masks[maskAddress][j + 6] == 1 ? 64 : 0) |
                            (Masks[maskAddress][j + 7] == 1 ? 128 : 0)
                            ));
                    }
                }
                else
                {
                    MASK.Write((ushort)(i | 32768));
                    MASK.Write(firstMaskInstance[maskAddress]);
                }
            }
        MASK.Write((ushort)0xFFFF);

    }
    //public byte[] GetImage(ushort id) { return Images[ImageAddress[id]]; }
    //public byte[] GetMask(ushort id) { return Masks[MaskAddress[id]]; }
    //public byte[] GetFMask(ushort id) { return Masks[FlippedMaskAddress[id]]; }

    public BuildResults Build(Bitmap image, Bitmap image32, Bitmap mask, string name, bool versionIsPlusCompatible)
    {
        if (image != null && image32 != null && (image.Width != image32.Width || image.Height != image32.Height))
            return BuildResults.DifferentDimensions;
        Bitmap mainImage = image ?? image32;
        if (mainImage.Width != mask.Width || mainImage.Height != mask.Height)
            return BuildResults.DifferentDimensions;
        if (mainImage.Width != 320 || mainImage.Height % 32 != 0)
            return BuildResults.BadDimensions;
        if (image != null && image.PixelFormat != PixelFormat.Format8bppIndexed)
            return BuildResults.ImageWrongFormat;
        if (image32 != null) {
            if (!versionIsPlusCompatible)
                return BuildResults.VersionDoesNotSupport32BitImage;
            if (!(image32.PixelFormat == PixelFormat.Format32bppArgb || image32.PixelFormat == PixelFormat.Format32bppRgb || image32.PixelFormat == PixelFormat.Format24bppRgb))
                return BuildResults.Image32WrongFormat;
            if (image == null && mask.PixelFormat != PixelFormat.Format8bppIndexed)
                return BuildResults.MaskNeedsPaletteFor32BitImages;
            VersionType = Version.Plus;
        }
        if (!mask.PixelFormat.HasFlag(PixelFormat.Indexed))
            return BuildResults.MaskWrongFormat;
        TileCount = (uint)mainImage.Height / 32 * 10;
        if (TileCount > MaxTiles)
        {
            if (VersionType == Version.JJ2)
                VersionType = Version.TSF; //cheat
            if (TileCount > MaxTiles) //still
                return BuildResults.TooBigForVersion;
        }
        else if (TileCount <= 1020 && VersionType == Version.TSF)
            VersionType = Version.JJ2; //increase accessibility

        Header = (VersionType == Version.JJ2 || VersionType == Version.TSF || VersionType == Version.Plus) ? StandardHeader : "";
        Magic = "TILE";
        Signature = 0xAFBEADDEu; //DEADBEAF, rather
        Name = name;

        Images = new byte[MaxTiles][];
        Masks = new byte[MaxTiles][];
        IsFullyOpaque = new bool[MaxTiles];
        TransparencyMaskJCS_Style = new byte[MaxTiles][];

        Palette = new Palette();
        var paletteSource = (image ?? mask).Palette; //if image32 is defined, it's possible for only the mask to be a paletted image, so the palette has to sneak in through there
        for (uint i = 1; i < Palette.PaletteSize - 1; ++i)
            Palette.Colors[i] = Palette.Convert(paletteSource.Entries[i]);
        Palette.Colors[0] = new byte[] { 0, 0, 0, 0 }; //transparency must always be black, for MMX reasons
        Palette.Colors[15] = Palette.Colors[255] = new byte[] { 255, 255, 255, 255 }; //these colors are both always white

        {
            var imageIndices = image != null ? new byte[image.Width * image.Height] : null;
            var image32Indices = image32 != null ? new byte[image32.Width * image32.Height * 4] : null;
            var maskIndices = new byte[mask.Width * mask.Height];
            var allBmps = new Bitmap[] { image, image32, mask };
            var allIndices = new byte[][] { imageIndices, image32Indices, maskIndices };

            for (int i = 0; i < 3; ++i)
                if (allBmps[i] != null)
                {
                    var data = allBmps[i].LockBits(new Rectangle(0, 0, allBmps[i].Width, allBmps[i].Height), ImageLockMode.ReadOnly, allBmps[i].PixelFormat);
                    int width = allBmps[i].Width;
                    if (i == 1)
                    {
                        if (allBmps[1].PixelFormat == PixelFormat.Format24bppRgb)
                            width *= 3;
                        else
                            width *= 4;
                    }
                    for (int y = 0; y < allBmps[i].Height; ++y)
                        Marshal.Copy(new IntPtr((int)data.Scan0 + data.Stride * y), allIndices[i], width * y, width);
                    allBmps[i].UnlockBits(data);
                }

            var empty32BitTile = new byte[32 * 32 * 4];
            for (int tileID = 0; tileID < (int)TileCount; ++tileID)
            {
                var imageArray = new byte[32*32];
                var image32Array = new byte[32*32 * 4];
                var transpArray = new byte[32*32];
                var maskArray = Masks[tileID] = new byte[32*32];
                int x = (tileID % 10) * 32, y = (tileID / 10) * 32;
                for (int xx = 0; xx < 32; ++xx)
                    for (int yy = 0; yy < 32; ++yy)
                    {
                        int tileIndex = xx | (yy << 5);
                        int sourceIndex = (x | xx) + (y | yy) * 320;
                        maskArray[tileIndex] = (byte)(maskIndices[sourceIndex] != 0 ? 1 : 0);
                        bool pixelIsFullyOpaque = true;
                        if (image != null)
                        {
                            byte color = imageIndices[sourceIndex];
                            if (color == 1) color = 0;
                            transpArray[tileIndex] = (byte)((pixelIsFullyOpaque = (imageArray[tileIndex] = color) != 0) ? 1 : 0);
                        }
                        if (VersionType == Version.Plus)
                        {
                            tileIndex <<= 2;
                            switch (image32.PixelFormat)
                            {
                                case PixelFormat.Format32bppArgb:
                                    sourceIndex <<= 2;
                                    for (int ch = 0; ch < 3; ++ch)
                                        image32Array[tileIndex | (2 - ch)] = image32Indices[sourceIndex | ch]; //reverse BGR to RGB
                                    pixelIsFullyOpaque |= (image32Array[tileIndex | 3] = image32Indices[sourceIndex | 3]) == 255; //...A
                                    break;
                                case PixelFormat.Format32bppRgb:
                                case PixelFormat.Format24bppRgb:
                                    sourceIndex *= (image32.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                                    for (int ch = 0; ch < 3; ++ch)
                                        pixelIsFullyOpaque |= ((image32Array[tileIndex | ch] = image32Indices[sourceIndex | ch]) != 0);
                                    image32Array[tileIndex | 3] = (byte)(pixelIsFullyOpaque ? 255 : 0); //transparent is color 0,0,0
                                    break;
                            }
                        }
                        if (!pixelIsFullyOpaque) IsFullyOpaque[tileID] = false;
                    }
                if (VersionType != Version.Plus || image32Array.SequenceEqual(empty32BitTile)) { //only use the 8-bit image if the 32-bit image is empty/nonexistent
                    Images[tileID] = imageArray;
                    TransparencyMaskJCS_Style[tileID] = transpArray;
                } else {
                    Images[tileID] = image32Array;
                }
            }
        }

        TransparencyMaskJJ2_Style = TransparencyMaskJCS_Style.Clone() as byte[][];

        return BuildResults.Success;
    }

    public struct ByteArrayKey //https://stackoverflow.com/questions/33031968/using-byte-array-as-dictionary-key?noredirect=1&lq=1
    {
        public readonly byte[] Bytes;
        private readonly int _hashCode;

        public override bool Equals(object obj)
        {
            var other = (ByteArrayKey)obj;
            return Compare(Bytes, other.Bytes);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        private static int GetHashCode(byte[] bytes)
        {
            unchecked
            {
                var hash = 17;
                for (var i = 0; i < bytes.Length; i++)
                {
                    hash = hash * 23 + bytes[i];
                }
                return hash;
            }
        }

        public ByteArrayKey(byte[] bytes)
        {
            Bytes = bytes;
            _hashCode = GetHashCode(bytes);
        }

        public static ByteArrayKey Create(byte[] bytes)
        {
            return new ByteArrayKey(bytes);
        }

        public static unsafe bool Compare(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                var l = a1.Length;
                for (var i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*(long*)x1 != *(long*)x2) return false;
                if ((l & 4) != 0)
                {
                    if (*(int*)x1 != *(int*)x2) return false;
                    x1 += 4;
                    x2 += 4;
                }
                if ((l & 2) != 0)
                {
                    if (*(short*)x1 != *(short*)x2) return false;
                    x1 += 2;
                    x2 += 2;
                }
                if ((l & 1) != 0) if (*x1 != *x2) return false;
                return true;
            }
        }
    }

    static byte[] GenerateTransparencyInstructionsFromTransparencyMask(byte[] input) //based on JJ2+'s jjPIXELMAP tile saving code
    {
        var instructions = new List<byte>(ConvertByteMaskTo128Bits(input));
        
        for (int pixelID = 0; pixelID < 32*32; ) { //see http://www.jazz2online.com/wiki/index.php?J2T_File_Format
            byte numberOfOpaqueRegionsInRow = 0;
            byte lastByteOpacity = 0;
            byte lengthOfLastByteSeries = 0;
            var rowInstructions = new List<byte>();
            for (int column = 0; column < 32; ++column)
            {
                if (input[pixelID++] != lastByteOpacity)
                {
                    rowInstructions.Add(lengthOfLastByteSeries);
                    lengthOfLastByteSeries = 1;
                    if ((lastByteOpacity ^= 1) == 1)
                        ++numberOfOpaqueRegionsInRow;
                }
                else
                {
                    lengthOfLastByteSeries += 1;
                }
            }
            if (lastByteOpacity == 1)
                rowInstructions.Add(lengthOfLastByteSeries);
            instructions.Add(numberOfOpaqueRegionsInRow);
            instructions.AddRange(rowInstructions);
        }

        return instructions.ToArray();
    }

    public SavingResults Save(string filepath)
    {
        FilenameOnly = Path.GetFileName(FullFilePath = filepath);
        Encoding encoding = FileEncoding;

        for (int i = 0; i < 4; i++)
            UncompressedData[i] = new MemoryStream();
        using (BinaryWriter data1writer = new BinaryWriter(UncompressedData[0], encoding))
        using (BinaryWriter data2writer = new BinaryWriter(UncompressedData[1], encoding))
        using (BinaryWriter data3writer = new BinaryWriter(UncompressedData[2], encoding))
        using (BinaryWriter data4writer = new BinaryWriter(UncompressedData[3], encoding))
        {
            foreach (var color in Palette.Colors)
                data1writer.Write(color);
            data1writer.Write(TileCount);
            for (int i = 0; i < MaxTiles * 2; ++i)
                data1writer.Write(i < TileCount ? IsFullyOpaque[i] : false);

            byte[][]
                TransparencyInstructions = new byte[TileCount][],
                MaskBits = new byte[TileCount][],
                ReversedMaskBits = new byte[TileCount][];
            for (uint i = 0; i < TileCount; ++i)
            {
                if (VersionType != Version.Plus)
                    TransparencyInstructions[i] = GenerateTransparencyInstructionsFromTransparencyMask(TransparencyMaskJCS_Style[i]);
                MaskBits[i] = ConvertByteMaskTo128Bits(Masks[i]);
                ReversedMaskBits[i] = ConvertByteMaskTo128Bits(
                    Masks[i]
                     .Select((x, index) => new { x, index }) //https://stackoverflow.com/questions/11427413/linq-select-5-items-per-iteration
                     .GroupBy(x => x.index / 32, y => y.x)
                     .Select(r => r.Reverse())
                     .SelectMany(b => b) //https://stackoverflow.com/questions/1590723/flatten-list-in-linq
                     .ToArray()
               );
            }

            var Sources = new byte[][][] { Images, TransparencyInstructions, MaskBits, ReversedMaskBits };
            var MaskDictionary = new Dictionary<ByteArrayKey, uint>();
            var Destinations = new Dictionary<ByteArrayKey, uint>[] { new Dictionary<ByteArrayKey, uint>(), new Dictionary<ByteArrayKey, uint>(), MaskDictionary, MaskDictionary };
            var Writers = new BinaryWriter[] { data2writer, data3writer, data4writer, data4writer };

            for (int dataID = 0; dataID < 4; ++dataID)
            {
                var source = Sources[dataID];
                var dest = Destinations[dataID];
                var writer = Writers[dataID];
                int numberOfTimesToWrite = (dataID < 2) ? MaxTiles * 2 : MaxTiles;
                for (int tileID = 0; tileID < numberOfTimesToWrite; ++tileID)
                {
                    if (VersionType != Version.Plus || dataID != 1)
                    {
                        if (tileID < TileCount)
                        {
                            var bytes = new ByteArrayKey(source[tileID]);
                            if (!dest.ContainsKey(bytes))
                            {
                                var pointerToNewTile = (uint)writer.BaseStream.Length;
                                if (VersionType == Version.Plus && dataID == 0 && source[tileID].Length == 32 * 32 * 4)
                                    pointerToNewTile |= 0x80000000u;
                                dest[bytes] = pointerToNewTile;
                                writer.Write(bytes.Bytes);
                            }
                            System.Diagnostics.Debug.WriteLine(dest[bytes].ToString("x"));
                            data1writer.Write(dest[bytes]);
                        }
                        else
                            data1writer.Write(0);
                    }
                }
            }

            if (VersionType == Version.Plus)
                data3writer.Write(false); //non-empty

            using (BinaryWriter binwriter = new BinaryWriter(
                File.Open(FullFilePath, FileMode.Create, FileAccess.Write),
                encoding
            ))
            {
                binwriter.Write(encoding.GetBytes(Header)); //the copyright notice
                binwriter.Write(encoding.GetBytes(Magic)); // 'TILE'
                binwriter.Write(Signature); // 'DEADBEAF'
                binwriter.Write(getBytes(encoding, Name, 32));
                binwriter.Write((ushort)((VersionType != Version.Plus) ? (VersionType == Version.TSF) ? 0x201 : 0x200 : 0x300));
                binwriter.Write(new byte[40]); // To be filled in later with filesize, CRC32, and the compressed and uncompressed data lengths, for a total of 10 longs or 40 bytes.
                CRC32 CRCCalculator = new CRC32();
                for (int i = 0; i < 4; i++)
                {
                    UncompressedDataLength[i] = (int)UncompressedData[i].Length;
                    var zcomparray = ZlibStream.CompressBuffer(UncompressedData[i].ToArray());
                    binwriter.Write(zcomparray);
                    CompressedDataLength[i] = zcomparray.Length;
                    CRCCalculator.SlurpBlock(zcomparray, 0, zcomparray.Length);
                }
                binwriter.Seek(encoding.GetByteCount(Header) + 42, 0);
                binwriter.Write((int)(binwriter.BaseStream.Length));
                binwriter.Write(CRCCalculator.Crc32Result); Crc32 = CRCCalculator.Crc32Result;
                for (int i = 0; i < 4; i++)
                {
                    binwriter.Write(CompressedDataLength[i]);
                    binwriter.Write(UncompressedDataLength[i]);
                }
            }
        }

        return SavingResults.Success;
    }
}

class Layer
{
    public int id;
    public bool TileWidth;
    public bool TileHeight;
    public bool LimitVisibleRegion;
    public bool IsTextured;
    public bool HasStars;
    public byte unknown1;
    public bool HasTiles
    {
        get { return id == J2LFile.SpriteLayerID || TileMap.Count != 0; }
    }
    public bool isDefault { get { return id >= 0; } }

    public uint Width;
    public uint RealWidth;
    public uint Height;
    public int ZAxis;
    public byte unknown2;
    public float AutoXSpeed;
    public float AutoYSpeed;
    public float XSpeed;
    public float YSpeed;
    public byte TextureMode;
    public byte TexturParam1;
    public byte TexturParam2;
    public byte TexturParam3;

    public string Name;
    public float WaveX;
    public float WaveY;
    public bool Hidden;
    public byte SpriteMode, SpriteParam;
    public int RotationAngle, RotationRadiusMultiplier;

    public ArrayMap<ushort> TileMap;

    public Layer(int i, uint raw, bool LEVstyle = false)
    {
        id = i;
        TileWidth = (raw & 1) == 1;
        TileHeight = (raw & 2) == 2;
        if (LEVstyle)
        {
            //HasTiles = (raw & 4) == 4; //not needed since maintained with TileMap.Count
            LimitVisibleRegion = (raw & 8) == 8;
            IsTextured = (raw & 16) == 16;
        }
        else
        {
            LimitVisibleRegion = (raw & 4) == 4;
            IsTextured = (raw & 8) == 8;
            HasStars = (raw & 16) == 16;
        }

        Name = DefaultNames[i];
        RotationAngle = DefaultRotationAngles[i];
        RotationRadiusMultiplier = DefaultRotationRadiusMultipliers[i];
    }

    public Layer(Layer other)
    {
        id = -1;
        TileWidth = other.TileWidth;
        TileHeight = other.TileHeight;
        LimitVisibleRegion = other.LimitVisibleRegion;
        IsTextured = other.IsTextured;
        HasStars = other.HasStars;
        unknown1 = other.unknown1;
        Width = other.Width;
        RealWidth = other.RealWidth;
        Height = other.Height;
        //ZAxis//eh idk
        unknown2 = other.unknown2;
        AutoXSpeed = other.AutoXSpeed;
        AutoYSpeed = other.AutoYSpeed;
        XSpeed = other.XSpeed;
        YSpeed = other.YSpeed;
        TextureMode = other.TextureMode;
        TexturParam1 = other.TexturParam1;
        TexturParam2 = other.TexturParam2;
        TexturParam3 = other.TexturParam3;

        Name = "Copy of " + other.Name;
        WaveX = other.WaveX;
        WaveY = other.WaveY;
        Hidden = other.Hidden;
        SpriteMode = other.SpriteMode;
        SpriteParam = other.SpriteParam;
        RotationAngle = other.RotationAngle;
        RotationRadiusMultiplier = other.RotationRadiusMultiplier;

        TileMap = new ArrayMap<ushort>(Width, Height);
        for (ushort x = 0; x < Width; x++)
            for (ushort y = 0; y < Height; y++)
                TileMap[x, y] = other.TileMap[x, y];
}

    static readonly uint[] DefaultWidths = {864, 576, 256, 256, 171, 114, 76, 8};
    static readonly uint[] DefaultHeights = { 216, 144, 64, 64, 43, 29, 19, 8 };
    static readonly float[] DefaultSpeeds = { 3.375F, 2.25F, 1, 1, 0.666672F, 0.444458F, 0.29631F, 0 };
    public static readonly string[] DefaultNames = new string[8] { "Foreground Layer #2", "Foreground Layer #1", "Sprite Foreground Layer", "Sprite Layer", "Background Layer #1", "Background Layer #2", "Background Layer #3", "Background Layer" };
    static readonly int[] DefaultRotationAngles = { -512, -256, 0, 0, 0, 256, 512, 768 };
    static readonly int[] DefaultRotationRadiusMultipliers = { 4, 3, 0, 0, 2, 2, 1, 1 };
    public Layer(int i) //using default values (i.e. called when creating a new level from scratch)
    {
        id = (byte)i;

        unknown1 = unknown2 = 0;
        TextureMode = TexturParam1 = TexturParam2 = TexturParam3 = 0;
        WaveX = WaveY = 0;
        ZAxis = (i * 100) - 300;

        Width = RealWidth = DefaultWidths[i];
        Height = DefaultHeights[i];
        XSpeed = YSpeed = DefaultSpeeds[i];
        AutoXSpeed = AutoYSpeed = 0;

        TileWidth = TileHeight = (i == 7);
        LimitVisibleRegion = IsTextured = HasStars = false;

        Name = DefaultNames[i];
        RotationAngle = DefaultRotationAngles[i];
        RotationRadiusMultiplier = DefaultRotationRadiusMultipliers[i];

        TileMap = new ArrayMap<ushort>(Width, Height);
    }

    public Layer()
    {
        Width = RealWidth = Height = 1;
        Name = "New Layer";
        id = -1;
        TileMap = new ArrayMap<ushort>(Width, Height);
    }

    public uint GetSize()
    {
        return (RealWidth+3)/4*4 * Height;
    }
    /*public ushort GetTileAt(int x, int y)
    {
        if (x < RealWidth && y < Height && x >= 0 && y >= 0) return TileMap[x, y];
        else return 0;
    }*/
    public void GetOriginNumbers(int xPosition, int yPosition, ref int widthReduced, ref int heightReduced, ref int xOrigin, ref int yOrigin, ref int upperLeftX, ref int upperLeftY, bool useLayer8Speeds)
    {
        if (id == 7 && !useLayer8Speeds)
        {
            upperLeftX = -32 - widthReduced;
            upperLeftY = -32 - (LimitVisibleRegion ? heightReduced * 2 : heightReduced);
        }
        else
        {
            upperLeftX = (int)Math.Floor(xPosition * XSpeed - widthReduced) - 32;
            upperLeftY = (int)(yPosition * YSpeed - (LimitVisibleRegion ? heightReduced * 2 : heightReduced)) - 32;
        }
        xOrigin = -32 - (upperLeftX % 32);
        upperLeftX /= 32;
        yOrigin = -32 - (upperLeftY % 32);
        upperLeftY /= 32;
    }
    public void GetFixedCornerOriginNumbers(int xPosition, int yPosition, int widthReduced, int heightReduced, ref int xOrigin, ref int yOrigin, ref int upperLeftX, ref int upperLeftY, byte tileSize, bool applyWaveAsOffsets, bool useLayer8Speeds)
    {
        /*if (id == 7)
        {
            upperLeftX = -tileSize;
            upperLeftY = -tileSize - (LimitVisibleRegion ? heightReduced * 2 : 0);
        }
        else
        {
            upperLeftX = (int)Math.Floor(xPosition * XSpeed) - tileSize;
            upperLeftY = (int)(yPosition * YSpeed - (LimitVisibleRegion ? heightReduced * 2 : 0)) - tileSize;
        }*/
        if (id == 7 && !useLayer8Speeds)
        {
            upperLeftX = -32;
            upperLeftY = -32;
        }
        else
        {
            upperLeftX = (int)Math.Floor(xPosition * XSpeed - widthReduced) - tileSize;
            upperLeftY = (int)(yPosition * YSpeed - ((LimitVisibleRegion && !TileHeight) ? heightReduced * 2 : heightReduced)) - tileSize;
        }
        if (applyWaveAsOffsets)
        {
            upperLeftX += (int)(WaveX * tileSize / 32);
            upperLeftY += (int)(WaveY * tileSize / 32);
        }
        xOrigin = -tileSize - (upperLeftX % tileSize);
        upperLeftX /= tileSize;
        yOrigin = -tileSize - (upperLeftY % tileSize);
        upperLeftY /= tileSize;
    }

    internal bool PlusOnly
    {
        get
        {
            if (!isDefault)
                return true;
            if (Hidden)
                return true;
            if (SpriteMode != 0 || SpriteParam != 0)
                return true;
            if (RotationAngle != DefaultRotationAngles[id] || RotationRadiusMultiplier != DefaultRotationRadiusMultipliers[id])
                return true;
            if (Name != DefaultNames[id])
                return true;
            return false;
        }
    }
    internal bool ContainsVerticallyFlippedTiles
    {
        get
        {
            if (HasTiles)
                foreach (ushort tileID in TileMap)
                    if ((tileID & 0x2000) != 0)
                        return true;
            return false;
        }
    }

    static System.Text.RegularExpressions.Regex NumberPrefixedNamePattern = new System.Text.RegularExpressions.Regex("^\\d: .+");
    public override string ToString()
    {
        if (NumberPrefixedNamePattern.Match(Name).Success) //.LEV files include "1: ", "2: ", etc. in their layer names, and we don't want to display "4: 4: Sprite Layer"
            return Name;
        if (!isDefault)
            return Name;
        return (id + 1) + ": " + Name;
    }
}

class AnimatedTile
{
    public byte Speed;
    public byte FrameCount;
    public ushort[] Sequence = new ushort[64];
    public ushort Framewait;
    public ushort Random;
    public ushort PingPongWait;
    public bool IsPingPong;
    public Queue<ushort> FrameList = new Queue<ushort>(new ushort[1]{0});
    internal int hitherto = -1;

    public AnimatedTile() {}
    public AnimatedTile(AnimatedTile oldAnim)
    {
        Speed = oldAnim.Speed;
        FrameCount = oldAnim.FrameCount;
        for (byte i = 0; i < 64;) Sequence[i] = oldAnim.Sequence[i++];
        Framewait = oldAnim.Framewait;
        Random = oldAnim.Random;
        PingPongWait = oldAnim.PingPongWait;
        IsPingPong = oldAnim.IsPingPong;
        hitherto = oldAnim.hitherto;
        FrameList = new Queue<ushort>(oldAnim.FrameList);
    }

    public InsertFrameResults InsertFrame(byte location, ushort nuFrame)
    {
        if (FrameCount == 64) return InsertFrameResults.Full;
        else
        {
            for (byte i = FrameCount; i > location; ) Sequence[i] = Sequence[--i];
            Sequence[location] = nuFrame;
            FrameCount++;
            return InsertFrameResults.Success;
        }
    }
    public void DeleteFrame(byte location)
    {
        for (byte i = location; i < FrameCount && i < 63; ) Sequence[i] = Sequence[++i]; FrameCount--;
    }

    public void JustBeenEdited(int frame) {
        FrameList.Clear();
        if (FrameCount > 0) {
            GenerateFrameList();
            int mod = (frame * Speed / 70) % FrameList.Count();
            for (int i = 0; i < mod; i++) FrameList.Dequeue();
        }
        else FrameList.Enqueue(0);
        hitherto = frame * Speed / 70;
    }

    public void Reset()
    {
    FrameList = new Queue<ushort>(new ushort[1]{0});
    hitherto = -1;
    }

    private void GenerateFrameList(int random = 0)
    {
        for (uint i = 0; i < FrameCount; i++) FrameList.Enqueue(Sequence[i]);
        if (IsPingPong)
        {
            for (uint i = 0; i < PingPongWait; i++) FrameList.Enqueue(Sequence[FrameCount - 1]);
            for (uint i = 0; i < FrameCount; i++) FrameList.Enqueue(Sequence[FrameCount - 1 - i]);
            for (uint i = 0; i < Framewait + random; i++) FrameList.Enqueue(Sequence[0]);
        }
        else for (uint i = 0; i < Framewait + random; i++) FrameList.Enqueue(Sequence[FrameCount - 1]);
    }
    public void Advance(int frame, int random=0)
    {
        int newTime = frame * Speed / 70;
        if (newTime > hitherto)
        {
            if (FrameCount>0) FrameList.Dequeue();
            hitherto = newTime;
        }
        if (FrameList.Count() == 0) GenerateFrameList(random);
    }

    public void ChangeVersion(ref Version nuVersion, ref uint tileCount, ref ushort numberOfAnimations)
    {
        switch (nuVersion)
        {
            case Version.JJ2:
            case Version.BC:
            case Version.O:
            case Version.GorH:
                for (byte i = 0; i < FrameCount; i++)
                {
                    if (Sequence[i] > 4095 + tileCount) Sequence[i] -= 7168;
                    else if (Sequence[i] >= tileCount)
                    {
                        if (Sequence[i] >= 4096 - numberOfAnimations) Sequence[i] -= 3072;
                        else Sequence[i] += 8192;
                    }
                }
                Reset();
                break;
            case Version.TSF:
            case Version.AGA:
                for (byte i = 0; i < FrameCount; i++)
                {
                    if (Sequence[i] >= 8192) Sequence[i] -= 8192;
                    else if (Sequence[i] > 1023 + tileCount) Sequence[i] += 7168;
                    else if (Sequence[i] >= tileCount) Sequence[i] += 3072;
                }
                Reset();
                break;
            default:
                break;
        }
    }
}

public struct AGAEvent
{
    internal uint ID;
    internal bool[] Bits;
    internal string[] Strings;
    internal int[] Longs;
    internal AGAEvent(uint id = 0) { ID = id; Bits = new bool[0]; Strings = new string[0]; Longs = new int[0]; }
    internal AGAEvent(int marker)
    {
        ID = 0;
        Bits = new bool[10];
        Strings = new string[6]{"","","","","",""};
        Longs = new int[22];
        for (byte i = 0; i < 10; i++) Bits[i] = (marker & (32 << i)) > 0;
    }
    internal bool HasParameters()
    {
        return !(
            Longs[0] == 0 &&
            Longs[1] == 0 &&
            Longs[2] == 0 &&
            Longs[3] == 0 &&
            Longs[4] == 0 &&
            Longs[5] == 0 &&
            Longs[6] == 0 &&
            Longs[7] == 0 &&
            Longs[8] == 0 &&
            Longs[9] == 0 &&
            Longs[10] == 0 &&
            Longs[11] == 0 &&
            Longs[12] == 0 &&
            Longs[13] == 0 &&
            Longs[14] == 0 &&
            Longs[15] == 0 &&
            Longs[16] == 0 &&
            Longs[17] == 0 &&
            Longs[18] == 0 &&
            Longs[19] == 0 &&
            Longs[20] == 0 &&
            Longs[21] == 0 &&
            (Strings[0] ?? "") == "" &&
            (Strings[1] ?? "") == "" &&
            (Strings[2] ?? "") == ""
            );
    }
    internal int GetNumberOfParameters()
    {
        for (byte i = 0; i < 22; i += 2) if (Longs[i] == 0 && Longs[i + 1] == 0) return i;
        return 22;
    }
    internal int GetNumberOfBytesItWillTakeToWriteStrings()
    {
        int len = 30;
        for (sbyte i = 5; i >= 0; i--)
        {
            if ((Strings[i] ?? "") == "") len -= 5;
            else
            {
                for (; i >= 0; i--) len += Strings[i].Length;
                break;
            }
        }
        /*if ((Strings[2] ?? "") == "")
        {
            if ((Strings[1] ?? "") == "")
            {
                if ((Strings[0] ?? "") == "") return 0;
                else return Strings[0].Length + 5;
            }
            else return Strings[0].Length + Strings[1].Length + 10;
        }
        else return Strings[0].Length + Strings[1].Length + Strings[2].Length + 15;*/
        return len;
    }
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        else return (ID == ((AGAEvent)obj).ID);
    }
}

class J2LFile : J2File
{
    #region variable declaration
    internal byte[] PasswordHash;
    public bool IsHiddenInHCL;

    public ushort JCSHorizontalFocus;
    public ushort JCSVerticalFocus;
    internal byte JCSFocusedLayer;
    public byte MinLight;
    public byte StartLight;
    public ushort NumberOfAnimations;
    public bool UsesVerticalSplitscreen;
    public byte LevelMode;
    internal string MainTilesetFilename;
    internal List<J2TFile> Tilesets = new List<J2TFile>(0);
    public bool HasTiles { get { return Tilesets.Count > 0; } }
    public uint TileCount { get { return (uint)Tilesets.Sum(tileset => (int)tileset.TileCount); } }
    internal string BonusLevel;
    internal string NextLevel;
    internal string SecretLevel;
    internal string Music;
    public string[] Text = new string[16];
    public const int SpriteLayerID = 3;
    public Layer[] DefaultLayers = new Layer[8];
    public Layer SpriteLayer { get { return DefaultLayers[SpriteLayerID]; } }
    public List<Layer> AllLayers;
    internal byte[] unknownsection;
    internal ushort AnimOffset;
    public uint[] EventTiles;
    internal bool[] IsEachTileFlipped;
    public byte[] TileTypes;
    internal bool[] IsEachTileUsed;
    internal AnimatedTile[] Animations;
    internal uint[,] EventMap;
    internal ushort[][] Dictionary;

    public string[][] AGA_SoundPointer;
    internal byte[] AGA_unknownsection;
    internal List<String> AGA_LocalEvents; // AGA
    internal List<String> AGA_GlobalEvents;
    internal AGAEvent[,] AGA_EventMap;

    internal MLLE.PlusPropertyList PlusPropertyList = new MLLE.PlusPropertyList(null);
    internal bool ContainsVerticallyFlippedTiles { get
        {
            if (DefaultLayers.FirstOrDefault(layer => (layer.PlusOnly || layer.ContainsVerticallyFlippedTiles)) != null)
                return true;
            foreach (AnimatedTile CurrentAnimatedTile in Animations)
                foreach (ushort tileID in CurrentAnimatedTile.Sequence)
                    if ((tileID & 0x2000) != 0) //flipped vertically
                        return true;
            return false;
        }
    }
    internal bool LevelNeedsData5 { get
        {
            if (Tilesets.Count > 1 || !AllLayers.SequenceEqual(DefaultLayers) || PlusPropertyList.LevelNeedsData5)
                return true;
            return false;
        }
    }

    internal Palette Palette { get
        {
            return PlusPropertyList.Palette ?? Tilesets[0].Palette;
        }
    }

    const uint SecurityStringMLLE = 0xBACABEEF;
    const uint SecurityStringPassworded = 0xBA00BE00;
    public const uint SecurityStringExtraDataNotForDirectEditing = 0xBA01BE01;
    const uint SecurityStringInsecure = 0;

    internal byte LEVunknown1;
    internal byte LEVunknown2;
    internal short LEVMysteriousTextShort;
    #endregion variable declaration

    private void CreateGlobalAGAEventsListIfNeedBe()
    {
        if (AGA_GlobalEvents == null)
        {
            AGA_GlobalEvents = new List<String>();
            Ini.IniFile ini = new Ini.IniFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AGAEventPointerList.ini"));
            for (int i = 0; i < 256; i++) AGA_GlobalEvents.Add(ini.IniReadValue("Pointers", i.ToString()).Trim());
        }
    }
    public void SwapEvents(uint one, uint two)
    {
        for (ushort x = 0; x < EventMap.GetLength(0); x++) for (ushort y = 0; y < EventMap.GetLength(1); y++)
            {
                if (EventMap[x, y] == one) EventMap[x, y] = two;
                else if (EventMap[x, y] == two) EventMap[x, y] = one;
            }
    }
    public uint GetRawBitsAtTile(int x, int y, int offset, int count) //obvious wrapper of the below
    {
        return GetRawBits(EventMap[x, y], offset, count);
    }
    public static uint GetRawBits(uint b, int offset, int count)
    {
        return (uint)((b >> offset) & ((1 << count) - 1)); // barrkel.blogspot.com
    }
    public ushort GetFrame(ushort raw, ref bool isFlipped, ref bool isVFlipped)
    {
        if (raw >= MaxTiles)
        {
            isFlipped |= (raw & MaxTiles) != 0;
            isVFlipped |= (raw & 0x2000) != 0;
            raw %= (ushort)MaxTiles;
        }
        if (raw < TileCount) return raw;
        else if (NumberOfAnimations >= MaxTiles - raw)
        {
            try
            {
                return GetFrame(Animations[NumberOfAnimations - (MaxTiles - raw)].FrameList.Peek(), ref isFlipped, ref isVFlipped);
            }
            catch //threading issue, I think
            {
                return 0;
            }
        }
        else return 0;
    }
    public void SetPassword() { PasswordHash[0] = 0; PasswordHash[1] = 0xBA; PasswordHash[2] = 0xBE; }
    public void SetPassword(string newpassword)
    {
        int inPutWord = new CRC32().GetCrc32(new MemoryStream(Encoding.ASCII.GetBytes(newpassword)));
        PasswordHash[0] = (byte)(inPutWord >> 16 & 0xff);
        PasswordHash[1] = (byte)(inPutWord >> 8 & 0xff);
        PasswordHash[2] = (byte)(inPutWord & 0xff);
    }

    int[] AGAMostValues = new int[256], AGAMostStrings = new int[256];

    public OpeningResults OpenLevel(string filename, ref byte[] Data5, string password = null, Dictionary<Version, string> defaultDirectories = null, Encoding encoding = null, uint? SecurityStringOverride = null, bool onlyInterestedInData1 = false)
    {
        encoding = encoding ?? FileEncoding;
        using (BinaryReader binreader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read), encoding))
        {
            FilenameOnly = Path.GetFileName(FullFilePath = filename);
            bool[] hasTiles = new bool[DefaultLayers.Length]; //only needed for loading; Layer has its own read-only HasTiles
            if (binreader.PeekChar() != 'D') // not a .LEV file
            {
                #region header
                char[] tempHeader = (binreader.PeekChar() == 32) ? binreader.ReadChars(180) : new char[0];
                char[] tempMagic = binreader.ReadChars(4);
                byte[] tempPasswordHash = binreader.ReadBytes(3);
                if (!onlyInterestedInData1)
                {
                    if (tempPasswordHash[0] != 0x00 || tempPasswordHash[1] != 0xBA || tempPasswordHash[2] != 0xBE)
                    {
                        if (password == null) return OpeningResults.PasswordNeeded;
                        else
                        {
                            int inPutWord = new CRC32().GetCrc32(new MemoryStream(Encoding.ASCII.GetBytes(password)));
                            if ((inPutWord >> 16 & 0xff) != tempPasswordHash[0] || (inPutWord >> 8 & 0xff) != tempPasswordHash[1] || (inPutWord & 0xff) != tempPasswordHash[2]) return OpeningResults.WrongPassword;
                        }
                    }
                }
                Header = new string(tempHeader); Magic = new string(tempMagic); PasswordHash = tempPasswordHash;
                IsHiddenInHCL = binreader.ReadBoolean();
                Name = new string(binreader.ReadChars(32));
                ushort VersionNumber = binreader.ReadUInt16();
                #region setup version-specific sizes
                switch (VersionNumber)
                {
                    case 514:
                        VersionType = (Header.Length == 0) ? Version.AmbiguousBCO : Version.JJ2;
                        break;
                    case 515:
                        VersionType = Version.TSF;
                        break;
                    case 256:
                        VersionType = Version.AGA;
                        break;
                }
                unknownsection = new byte[96];
                EventTiles = new uint[MaxTiles];
                IsEachTileFlipped = new bool[MaxTiles];
                TileTypes = new byte[MaxTiles];
                IsEachTileUsed = new bool[MaxTiles];
                #endregion setup version-specific sizes
                Size = binreader.ReadUInt32();
                Crc32 = binreader.ReadInt32();
                for (byte i = 0; i < 4; i++)
                {
                    CompressedDataLength[i] = binreader.ReadInt32();
                    UncompressedDataLength[i] = binreader.ReadInt32();
                }

                for (byte i = 0; i < 4; i++) UncompressedData[i] = new MemoryStream(ZlibStream.UncompressBuffer(binreader.ReadBytes(CompressedDataLength[i])));
                if (VersionType == Version.AGA)
                {
                    BinaryWriter stream2 = new BinaryWriter(File.Open("AGA" + Path.GetFileNameWithoutExtension(filename) + "Data2.dat", FileMode.Create), FileEncoding); stream2.Write(UncompressedData[1].ToArray()); stream2.Close();
                    //BinaryWriter stream1 = new BinaryWriter(File.Open("AGA" + Path.GetFileNameWithoutExtension(filename) + "Data1.dat", FileMode.Create), FileEncoding); stream1.Write(UncompressedData[0].ToArray()); stream1.Close();
                }
                #endregion header
                #region data1
                using (BinaryReader data1reader = new BinaryReader(UncompressedData[0], encoding))
                {
                    JCSHorizontalFocus = data1reader.ReadUInt16();
                    uint Secure = (uint)(data1reader.ReadUInt16()) << 16;
                    JCSVerticalFocus = data1reader.ReadUInt16();
                    Secure |= (uint)data1reader.ReadUInt16();
                    if (!SecurityStringOverride.HasValue)
                    {
                        switch (Secure)
                        {
                            case SecurityStringInsecure:
                            case SecurityStringPassworded:
                            case SecurityStringMLLE:
                                break;
                            default:
                                return OpeningResults.SecurityEnvelopeDamaged;
                        }
                    } else if (SecurityStringOverride.Value != Secure)
                        return OpeningResults.SecurityEnvelopeDamaged;
                    JCSFocusedLayer = (byte)(data1reader.ReadByte() & 15);
                    MinLight = data1reader.ReadByte();
                    StartLight = data1reader.ReadByte();
                    NumberOfAnimations = data1reader.ReadUInt16();
                    UsesVerticalSplitscreen = data1reader.ReadBoolean();
                    LevelMode = data1reader.ReadByte();
                    if (VersionType == Version.AmbiguousBCO)
                    {
                        if (LevelMode == 1) VersionType = Version.O;
                        else if (LevelMode == 2) VersionType = Version.BC;
                    }
                    data1reader.ReadUInt32(); //StreamSize
                    Name = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    MainTilesetFilename = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    if (VersionType != Version.AmbiguousBCO) Tilesets = new List<J2TFile>(1) { new J2TFile(Path.Combine((defaultDirectories == null) ? Path.GetDirectoryName(filename) : defaultDirectories[VersionType], MainTilesetFilename)) };
                    BonusLevel = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    NextLevel = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    SecretLevel = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    Music = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    for (byte i = 0; i < 16; i++)
                    {
                        Text[i] = new string(data1reader.ReadChars(512)).TrimEnd('\0');
                        if (encoding == FileEncoding)
                        {
                            if (Text[i].Contains("\u00EF\u00BF\u00BD")) //check if text contains the UTF-8 replacement character encoded in Windows-1252
                            {
                                return OpeningResults.IncorrectEncoding;
                            }
                        }
                        else //we're here to fix the encoding
                        {
                            //the original character was most likely a section sign, so replace the replacement character with '§'
                            Text[i] = Text[i].Replace('\uFFFD', '§');
                        }
                    }

                    if (VersionNumber == 256) //AGA .lvl files have a series of resource pointers here rather than let JJ2 figure it out by scanning the events
                    {
                        AGA_SoundPointer = new string[48][];
                        string sound;
                        for (byte i = 0; i < 48; i++)
                        {
                            sound = new string(data1reader.ReadChars(64));
                            //Console.WriteLine(sound);
                            if (sound.Trim('\0').Length > 0) AGA_SoundPointer[i] = sound.TrimEnd('\0').Split('\\');
                        }
                    }
                    for (int i = 0; i < DefaultLayers.Length; i++) DefaultLayers[i] = new Layer(i, data1reader.ReadUInt32());
                    AllLayers = new List<Layer>(DefaultLayers);
                    foreach (Layer layer in DefaultLayers) layer.unknown1 = data1reader.ReadByte();
                    for (int i = 0; i < DefaultLayers.Length; i++) hasTiles[i] = data1reader.ReadBoolean();
                    foreach (Layer layer in DefaultLayers) layer.Width = data1reader.ReadUInt32();
                    foreach (Layer layer in DefaultLayers) layer.RealWidth = data1reader.ReadUInt32();
                    foreach (Layer layer in DefaultLayers) layer.Height = data1reader.ReadUInt32();
                    foreach (Layer layer in DefaultLayers) layer.ZAxis = data1reader.ReadInt32();
                    foreach (Layer layer in DefaultLayers) layer.unknown2 = data1reader.ReadByte();
                    foreach (Layer layer in DefaultLayers) layer.WaveX = data1reader.ReadInt32() / 65536.0F;
                    foreach (Layer layer in DefaultLayers) layer.WaveY = data1reader.ReadInt32() / 65536.0F;
                    foreach (Layer layer in DefaultLayers) layer.XSpeed = data1reader.ReadInt32() / 65536.0F;
                    foreach (Layer layer in DefaultLayers) layer.YSpeed = data1reader.ReadInt32() / 65536.0F;
                    foreach (Layer layer in DefaultLayers) layer.AutoXSpeed = data1reader.ReadInt32() / 65536.0F;
                    foreach (Layer layer in DefaultLayers) layer.AutoYSpeed = data1reader.ReadInt32() / 65536.0F;
                    foreach (Layer layer in DefaultLayers) layer.TextureMode = data1reader.ReadByte();
                    foreach (Layer layer in DefaultLayers) { layer.TexturParam1 = data1reader.ReadByte(); layer.TexturParam2 = data1reader.ReadByte(); layer.TexturParam3 = data1reader.ReadByte(); }
                    AnimOffset = data1reader.ReadUInt16();
                    for (ushort i = 0; i < MaxTiles; i++) EventTiles[i] = data1reader.ReadUInt32();
                    for (ushort i = 0; i < MaxTiles; i++) IsEachTileFlipped[i] = data1reader.ReadBoolean();
                    //for (ushort i = 0; i < MaxTiles; i++) if (unknownsection2[i] != 0) Console.WriteLine(i.ToString() + ": " + unknownsection2[i].ToString());
                    TileTypes = data1reader.ReadBytes(MaxTiles);
                    //Console.WriteLine("break");
                    for (ushort i = 0; i < MaxTiles; i++) IsEachTileUsed[i] = data1reader.ReadBoolean();
                    //for (ushort i = 0; i < MaxTiles; i++) if (unknownsection3[i] != 0) Console.WriteLine(i.ToString() + ": " + unknownsection3[i].ToString());
                    if (VersionNumber == 256) AGA_unknownsection = data1reader.ReadBytes(32768); //wtf??
                    //for (int i = 0; i < 32768; i++) Console.Write(AGA_unknownsection[i]);
                    Animations = new AnimatedTile[128];
                    for (ushort i = 0; i < 128; i++)
                    {
                        Animations[i] = new AnimatedTile();
                        if (i < NumberOfAnimations)
                        {
                            Animations[i].Framewait = data1reader.ReadUInt16();
                            Animations[i].Random = data1reader.ReadUInt16();
                            Animations[i].PingPongWait = data1reader.ReadUInt16();
                            Animations[i].IsPingPong = data1reader.ReadBoolean();
                            Animations[i].Speed = data1reader.ReadByte();
                            Animations[i].FrameCount = data1reader.ReadByte();
                            for (byte j = 0; j < 64; j++) Animations[i].Sequence[j] = data1reader.ReadUInt16();
                        }
                    }
                }
                #endregion data1
                if (!onlyInterestedInData1)
                {
                    #region data2
                    EventMap = new uint[SpriteLayer.Width, SpriteLayer.Height];
                    using (BinaryReader data2reader = new BinaryReader(UncompressedData[1], encoding))
                    {
                        //ParameterMap = new uint[SpriteLayer.Width, SpriteLayer.Height];
                        if (VersionNumber != 256) // not AGA
                        {
                            uint rlong;
                            for (uint i = 0; i < UncompressedDataLength[1] / 4; i++)
                            {
                                rlong = data2reader.ReadUInt32();
                                EventMap[i % SpriteLayer.Width, i / SpriteLayer.Width] = rlong;
                                //ParameterMap[i % SpriteLayer.Width, i / SpriteLayer.Width] = rlong >> 8;
                            }
                        }
                        else // AGA
                        {
                            CreateGlobalAGAEventsListIfNeedBe();
                            AGA_LocalEvents = new List<String>();
                            AGA_EventMap = new AGAEvent[SpriteLayer.Width, SpriteLayer.Height];
                            //AGA_ParameterMap = new byte[SpriteLayer.Width, SpriteLayer.Height][];
                            ushort numberOfLocalEvents = data2reader.ReadUInt16();
                            for (ushort i = 0; i < numberOfLocalEvents; i++) AGA_LocalEvents.Add(new string(data2reader.ReadChars(64)).TrimEnd('\0'));
                            ushort loadX, loadY, loadLongCount; int loadParamSize, loadMarker, loadOffset, loadStringSize; string loadEventName = ""; bool loadHasStrings; AGAEvent loadCurrentEvent;
                            while (true)
                            {
                                try
                                {
                                    loadX = data2reader.ReadUInt16();
                                    loadY = data2reader.ReadUInt16();
                                    loadEventName = AGA_LocalEvents[data2reader.ReadUInt16()];
                                    //Console.WriteLine(String.Format("{0},{1}: {2}", loadX, loadY, loadEventName));
                                    loadMarker = data2reader.ReadInt32();
                                    loadCurrentEvent = new AGAEvent(loadMarker);
                                    loadCurrentEvent.ID = (uint)AGA_GlobalEvents.FindIndex((string pointer) => { return pointer == loadEventName; });
                                    //if ((loadMarker & ((2 << 16) - 1)) != 15840) Console.Write(String.Format("{0}: {1}", Convert.ToString(loadMarker, 2).PadLeft(32, '0'), loadEventName));
                                    if (loadMarker < 0)
                                    {
                                        loadParamSize = data2reader.ReadInt32();
                                        loadHasStrings = data2reader.ReadUInt16() == 2;
                                        loadLongCount = data2reader.ReadUInt16();
                                        /*if (loadLongCount > AGAMostValues[loadCurrentEvent.ID])
                                        {
                                            AGAMostValues[loadCurrentEvent.ID] = loadLongCount;
                                            Console.WriteLine(String.Format("{0} ({1}): {2} parameters", loadEventName, loadCurrentEvent.ID, loadLongCount * 2));
                                        }*/
                                        for (byte i = 0; i < loadLongCount * 2; i++) loadCurrentEvent.Longs[i] = data2reader.ReadInt32();
                                        if (loadHasStrings)
                                        {
                                            int numStrings = 0;
                                            loadOffset = (loadLongCount + 1) * 8;
                                            for (byte i = 0; loadOffset < loadParamSize; i++)
                                            {
                                                loadStringSize = data2reader.ReadInt32();
                                                loadCurrentEvent.Strings[i] = new String(data2reader.ReadChars(loadStringSize - 1));
                                                data2reader.ReadByte();
                                                loadOffset += loadStringSize + 4;
                                                numStrings++;
                                            }
                                            /*if (numStrings > AGAMostStrings[loadCurrentEvent.ID])
                                            {
                                                AGAMostStrings[loadCurrentEvent.ID] = loadLongCount;
                                                Console.WriteLine(String.Format("{0} ({1}): {2} strings", loadEventName, loadCurrentEvent.ID, numStrings));
                                            }*/
                                        }
                                        //AGA_ParameterMap[loadX, loadY] = data2reader.ReadBytes(loadParamSize - 4);
                                        //if ((loadMarker & ((2 << 16) - 1)) != 15840) Console.WriteLine(String.Format(" ({0})", AGA_ParameterMap[loadX, loadY][2]));
                                        //Console.WriteLine(String.Format("{0}: {1}, {2}; {3}", loadEventName, AGA_ParameterMap[loadX, loadY][0], AGA_ParameterMap[loadX, loadY][2], AGA_ParameterMap[loadX, loadY].Length - 4 - AGA_ParameterMap[loadX, loadY][2]*8));
                                        //Console.WriteLine(String.Format("{0}: {1} bytes", loadEventName, loadParamSize));
                                        //foreach (byte Byte in AGA_ParameterMap[loadX, loadY]) { Console.Write(Byte.ToString().PadLeft(3,'0')); Console.Write(' '); }
                                        //Console.WriteLine();
                                    }
                                    else { /*if ((loadMarker & ((2 << 16) - 1)) != 15840) Console.WriteLine();*/ }
                                    AGA_EventMap[loadX, loadY] = loadCurrentEvent;
                                }
                                catch { /*Console.WriteLine(loadEventName);*/ break; }
                            }
                        }
                    }
                    Console.WriteLine();
                    #endregion data2
                    #region data3
                    using (BinaryReader data3reader = new BinaryReader(UncompressedData[2], encoding))
                    {
                        Dictionary = new ushort[UncompressedDataLength[2] / 8][];
                        for (uint i = 0; i < UncompressedDataLength[2] / 8; i++)
                        {
                            Dictionary[i] = new ushort[4];
                            for (byte j = 0; j < 4; j++) Dictionary[i][j] = data3reader.ReadUInt16();
                        }
                    }
                    #endregion data3
                    #region data4
                    using (BinaryReader data4reader = new BinaryReader(UncompressedData[3], encoding))
                    {
                        for (int i = 0; i < DefaultLayers.Length; i++)
                        {
                            Layer layer = DefaultLayers[i];
                            layer.TileMap = new ArrayMap<ushort>(layer.Width, layer.Height);
                            if (hasTiles[i])
                                for (uint y = 0; y < layer.Height; y++) for (uint x = 0; x < layer.RealWidth; x += 4)
                                    {
                                        ushort nuword = data4reader.ReadUInt16();
                                        uint numberOfTilesToCopy =
                                            (x + 4 <= layer.Width) ? 4u : //nowhere near right edge of layer (as defined by Width, not RealWidth, in case Tile Width makes the two different)
                                            (x <= layer.Width) ? (layer.Width & 3) : //at right edge
                                            0u //past right edge
                                        ;
                                        for (uint k = 0; k < numberOfTilesToCopy; k++)
                                            layer.TileMap[x + k, y] = Dictionary[nuword][k];
                                    }
                        }
                    }
                    #endregion data4
                    #region data5
                    var remainingLength = binreader.BaseStream.Length - binreader.BaseStream.Position;
                    if (remainingLength > 0)
                        Data5 = binreader.ReadBytes((int)remainingLength); //let the application figure out what to do with them
                    #endregion
                }
            }
            else // is a .LEV file
            {
                VersionType = Version.GorH;
                int SectionOffset, SectionLength;
                Console.WriteLine(binreader.ReadChars(4)); //DDCF
                binreader.ReadBytes(4); //file length
                Console.WriteLine(binreader.ReadBytes(4)); //EDIT
                SectionLength = binreader.ReadInt32(); SectionOffset = 6;
                LEVunknown1 = binreader.ReadByte();
                J2TFile J2T = new J2TFile(binreader);
                Tilesets = new List<J2TFile>(1) { J2T };
                SectionOffset += (MainTilesetFilename = J2T.FilenameOnly).Length + 1;
                LEVunknown2 = binreader.ReadByte();
                string[] layerNames = new string[DefaultLayers.Length];
                for (int i = 0; i < layerNames.Length; i++) { layerNames[i] = new string(binreader.ReadChars(binreader.ReadByte())); SectionOffset += layerNames[i].Length + 1; }
                while (binreader.PeekChar() == 0) binreader.ReadByte(); //padding
                Console.WriteLine(binreader.ReadChars(4)); //EDI2
                binreader.ReadBytes(binreader.ReadInt32()); //skip EDI2 entirely
                Console.WriteLine(binreader.ReadChars(4)); //LINF
                SectionLength = binreader.ReadInt32(); SectionOffset = 10;
                /*VersionNumber =*/ binreader.ReadUInt16();
                Name = new string(binreader.ReadChars(binreader.ReadByte())); SectionOffset += Name.Length + 1;
                SecretLevel = new string(binreader.ReadChars(binreader.ReadByte())); SectionOffset += SecretLevel.Length + 1;
                Music = new string(binreader.ReadChars(binreader.ReadByte())); SectionOffset += Music.Length + 1;
                NextLevel = new string(binreader.ReadChars(binreader.ReadByte())); SectionOffset += NextLevel.Length + 1;
                BonusLevel = new string(binreader.ReadChars(binreader.ReadByte())); SectionOffset += BonusLevel.Length + 1;
                MinLight = (byte)binreader.ReadInt32();
                StartLight = (byte)binreader.ReadInt32();
                while (binreader.PeekChar() == 0) binreader.ReadByte(); //padding
                Console.WriteLine(binreader.ReadChars(4)); //HSTR
                SectionLength = binreader.ReadInt32(); SectionOffset = 2;
                LEVMysteriousTextShort = binreader.ReadInt16();
                for (byte i = 0; i < 16; i++) { Text[i] = new string(binreader.ReadChars(binreader.ReadByte())); SectionOffset += Text[i].Length + 1; }
                while (binreader.PeekChar() == 0) binreader.ReadByte(); //padding
                Console.WriteLine(binreader.ReadChars(4)); //TILE
                binreader.ReadChars(4); // section length
                /*J2T.VersionNumber = (ushort)*/binreader.ReadUInt32();
                Console.WriteLine(binreader.ReadChars(4)); //INFO
                binreader.ReadChars(8); // section length, repeat of tile count
                TileTypes = new byte[1024]; for (ushort i = 1; i < TileCount; i++) TileTypes[i] = binreader.ReadByte();
                while (binreader.PeekChar() == 0) binreader.ReadByte(); //padding
                Console.WriteLine(binreader.ReadChars(4)); //DATA
                SectionLength = (int)(binreader.BaseStream.Position + 4 + binreader.ReadUInt32()); // section length
                J2T.ReadFromTILEDATA(binreader, TileTypes);
                binreader.ReadBytes(SectionLength - (int)binreader.BaseStream.Position);
                Console.WriteLine(binreader.ReadChars(4)); //EMSK
                binreader.ReadBytes(binreader.ReadInt32()); // skip EMSK for now, it's complicated

                Console.WriteLine(binreader.ReadChars(4)); //MASK
                binreader.ReadBytes(4); //section length
                J2T.ReadFromMASK(binreader);
                while (binreader.PeekChar() == 0) binreader.ReadByte(); //padding
                Console.WriteLine(binreader.ReadChars(4)); //ANIM
                SectionLength = binreader.ReadInt32();
                NumberOfAnimations = (ushort)binreader.ReadUInt32(); SectionOffset = 4 + NumberOfAnimations * 9;
                Animations = new AnimatedTile[NumberOfAnimations];
                for (ushort i = 0; i < NumberOfAnimations; i++)
                {
                    Animations[i] = new AnimatedTile();
                    Animations[i].Framewait = binreader.ReadUInt16();
                    Animations[i].Random = binreader.ReadUInt16();
                    Animations[i].PingPongWait = binreader.ReadUInt16();
                    Animations[i].IsPingPong = binreader.ReadBoolean();
                    Animations[i].Speed = binreader.ReadByte();
                    Animations[i].FrameCount = binreader.ReadByte(); SectionOffset += Animations[i].FrameCount * 2;
                    for (byte j = 0; j < Animations[i].FrameCount; j++) Animations[i].Sequence[j] = binreader.ReadUInt16();
                }
                while (binreader.PeekChar() == 0) binreader.ReadByte(); //padding
                Console.WriteLine(binreader.ReadChars(4)); //FLIP
                binreader.ReadBytes(binreader.ReadInt32()); //skip FLIP entirely
                Console.WriteLine(binreader.ReadChars(4)); //LAYR
                binreader.ReadBytes(4); //section length
                /*VersionNumber = (ushort)*/binreader.ReadUInt32();
                Console.WriteLine(binreader.ReadChars(4)); //INFO
                SectionLength = binreader.ReadInt32(); SectionOffset = 120;
                DefaultLayers = new Layer[DefaultLayers.Length];
                for (int i = 0; i < DefaultLayers.Length; i++)
                {
                    byte raw = (byte)binreader.ReadInt32();
                    hasTiles[i] = (raw & 4) == 4;
                    Layer layer = DefaultLayers[i] = new Layer(i, raw, true);
                    layer.Width = binreader.ReadUInt16();
                    if (layer.TileWidth) switch (layer.Width % 4)
                        {
                            case 0: layer.RealWidth = layer.Width; break;
                            case 2: layer.RealWidth = layer.Width * 2; break;
                            default: layer.RealWidth = layer.Width * 4; break;
                        }
                    else layer.RealWidth = layer.Width;
                    layer.Height = binreader.ReadUInt16();
                    layer.ZAxis = binreader.ReadInt16();
                    layer.unknown1 = binreader.ReadByte();
                    int SpeedSettings = (byte)binreader.ReadInt32();
                    if ((SpeedSettings & 1) == 1) { layer.unknown2 = binreader.ReadByte(); SectionOffset++; }
                    if ((SpeedSettings & 2) == 2) { SectionOffset += 8; layer.XSpeed = binreader.ReadInt32() / 65536.0F; layer.YSpeed = binreader.ReadInt32() / 65536.0F; }
                    else { layer.XSpeed = layer.YSpeed = 0; }
                    if ((SpeedSettings & 4) == 4) { SectionOffset += 8; layer.AutoXSpeed = binreader.ReadInt32() / 65536.0F; layer.AutoYSpeed = binreader.ReadInt32() / 65536.0F; }
                    else { layer.AutoXSpeed = layer.AutoYSpeed = 0; }
                    layer.HasStars = false; layer.TextureMode = layer.TexturParam1 = layer.TexturParam2 = layer.TexturParam3 = 0;
                    layer.Name = layerNames[i];
                }
                AllLayers = new List<Layer>(DefaultLayers);
                while (binreader.PeekChar() == 0) binreader.ReadByte(); //padding
                Console.WriteLine(binreader.ReadChars(4)); //DATA
                SectionLength = binreader.ReadInt32(); SectionOffset = 6;
                AnimOffset = binreader.ReadUInt16();
                Dictionary = new ushort[binreader.ReadUInt16()][];
                binreader.ReadUInt16(); //unknown
                for (uint i = 0; i < Dictionary.Length; i++)
                {
                    Dictionary[i] = new ushort[16];
                    for (byte j = 0; j < 16; j++) Dictionary[i][j] = binreader.ReadUInt16();
                } SectionOffset += Dictionary.Length * 32;
                for (int i = 0; i < DefaultLayers.Length; i++)
                {
                    Layer layer = DefaultLayers[i];
                    layer.TileMap = new ArrayMap<ushort>(layer.Width, layer.Height);
                    if (hasTiles[i])
                        for (uint y = 0; y < layer.Height; y++) for (uint x = 0; x < layer.Width; x += 16)
                            {
                                ushort nuword = binreader.ReadUInt16(); SectionOffset += 2;
                                uint numberOfTilesToCopy =
                                    (x + 16 <= layer.Width) ? 16u : //nowhere near right edge of layer
                                    (x <= layer.Width) ? (layer.Width & 15) : //at right edge
                                    0u //past right edge
                                ;
                                for (uint k = 0; k < numberOfTilesToCopy; k++)
                                    layer.TileMap[x + k, y] = Dictionary[nuword][k];
                            }
                }
                while (binreader.PeekChar() == 0) binreader.ReadByte(); //padding
                Console.WriteLine(binreader.ReadChars(4)); //EVNT
                binreader.ReadBytes(4); //section length
                EventMap = new uint[SpriteLayer.Width, SpriteLayer.Height];
                uint rlong;
                for (uint i = 0; i < EventMap.Length; i++)
                {
                    rlong = binreader.ReadUInt32();
                    EventMap[i % SpriteLayer.Width, i / SpriteLayer.Width] = rlong;
                }
                Console.WriteLine(binreader.ReadChars(4)); //TMAP
                binreader.ReadBytes(binreader.ReadInt32()); //skip TMAP entirely
                Console.WriteLine(binreader.ReadChars(4)); //CMAP
                binreader.ReadBytes(9); //section length, 256, mystery byte
                J2T.Palette = new Palette(binreader, true);
            }
        }
        if (VersionType == Version.AmbiguousBCO) return OpeningResults.SuccessfulButAmbiguous;
        else return OpeningResults.Success;
    }
    public void NewLevel(Version version)
    {
        VersionType = version;
        Header = (VersionType == Version.JJ2 || VersionType == Version.TSF) ? StandardHeader : "";
        Magic = "LEVL";
        PasswordHash = new byte[] { 0x00, 0xBA, 0xBE };
        IsHiddenInHCL = false;
        switch (VersionType)
        {
            case Version.O:
                LevelMode = 1;
                break;
            case Version.BC:
                LevelMode = 2;
                break;
            default:
                LevelMode = 0;
                break;
        }
        Size = 0;
        Crc32 = 0;
        JCSHorizontalFocus = JCSVerticalFocus = NumberOfAnimations = 0;
        JCSFocusedLayer = 3;
        MinLight = StartLight = 64;
        UsesVerticalSplitscreen = false;
        Name = "Untitled";
        MainTilesetFilename = "";
         Tilesets = new List<J2TFile>(0);
        BonusLevel = "";
        NextLevel = "";
        SecretLevel = "";
        Music = "";
        for (byte i = 0; i < 16; i++) Text[i] = "";
        if (VersionType == Version.AGA) //pretend support!
        {
            AGA_SoundPointer = new string[48][];
        }

        for (int i = 0; i < DefaultLayers.Length; i++)
            DefaultLayers[i] = new Layer(i);
        AllLayers = new List<Layer>(DefaultLayers);

        unknownsection = new byte[64];
        AnimOffset = (ushort)MaxTiles;
        EventTiles = new uint[MaxTiles];
        IsEachTileFlipped = new bool[MaxTiles];
        TileTypes = new byte[MaxTiles];
        IsEachTileUsed = new bool[MaxTiles];
        if (VersionType == Version.AGA)
        {
            AGA_EventMap = new AGAEvent[SpriteLayer.Width, SpriteLayer.Height];
            AGA_unknownsection = new byte[32768]; //yeah, no clue
        }
        Animations = new AnimatedTile[128]; for (byte i = 0; i < 128; i++) Animations[i] = new AnimatedTile();
        EventMap = new uint[256, 64];
        //ParameterMap = new uint[256, 64];
        PlusPropertyList = new MLLE.PlusPropertyList(null);
    }
    internal bool IsAnUndefinedTile(ushort id) { return false;}// (id >= MaxTiles * 2 || (id % MaxTiles >= TileCount && MaxTiles - NumberOfAnimations > id % MaxTiles)); }
    internal ushort SanitizeTileValue(ushort id) { return (IsAnUndefinedTile(id) || id % MaxTiles == 0) ? (ushort)0 : id; }
    internal static void InputLEVSection(BinaryWriter destination, BinaryWriter source, char[] label)
    {
        while (source.BaseStream.Length % 4 > 0) source.Write('\0');
        destination.Write(label);
        destination.Write((int)source.BaseStream.Length);
        //byte[] output = new byte[source.BaseStream.Length];
        //source.BaseStream.Read(output, 0, output.Length);
        //destination.Write(output);
        source.BaseStream.Seek(0, SeekOrigin.Begin);
        for (uint i = 0; i < source.BaseStream.Length; i++ ) destination.Write((byte)source.BaseStream.ReadByte());
    }
    internal void FlipMaskEvaluateAnimation(AnimatedTile anim, bool isLayer3, ref bool isFlipped, ref bool[] tilesUsed, ref bool[] tilesFlipped)
    {
        foreach (ushort frame in anim.Sequence)
        {
            if (frame % MaxTiles < TileCount)
            {
                if (isLayer3) tilesUsed[frame % MaxTiles] = true;
                if ((isFlipped || frame >= MaxTiles) && frame % MaxTiles < Tilesets[0].TileCount) tilesFlipped[frame % MaxTiles] = true;
            }
            else
            {
                isFlipped |= frame > MaxTiles;
                FlipMaskEvaluateAnimation(
                    Animations[NumberOfAnimations - (MaxTiles - frame % MaxTiles)],
                    isLayer3,
                    ref isFlipped,
                    ref tilesUsed,
                    ref tilesFlipped);
            }
        }
    }
    public SavingResults Save(bool eraseUndefinedTiles = false, bool allowDifferentTilesetVersion = false, byte[] Data5 = null) { return Save(FullFilePath, eraseUndefinedTiles, allowDifferentTilesetVersion, false, Data5); }
    public SavingResults Save(string filename, bool eraseUndefinedTiles = false, bool allowDifferentTilesetVersion = false, bool storeGivenFilename = true, byte[] Data5 = null)
    {
        if (!HasTiles)
        {
            return SavingResults.NoTilesetSelected;
        }
        if (!allowDifferentTilesetVersion && Tilesets[0].VersionType != VersionType && !((VersionType == Version.GorH) || ((VersionType == Version.BC || VersionType == Version.O) && Tilesets[0].VersionType == Version.AmbiguousBCO) || (VersionType == Version.TSF && Tilesets[0].VersionType == Version.JJ2) || (Tilesets[0].VersionType == Version.Plus && (VersionType == Version.TSF || VersionType == Version.JJ2))))
        {
            return SavingResults.TilesetIsDifferentVersion;
        }
        if (!eraseUndefinedTiles)
        {
            /*first non-Open/New reference to DefaultLayers in this file*/ foreach (Layer CurrentLayer in DefaultLayers) foreach (ushort tile in CurrentLayer.TileMap) if (IsAnUndefinedTile(tile)) return SavingResults.UndefinedTiles;
            for (byte i = 0; i < NumberOfAnimations; i++) foreach (ushort tile in Animations[i].Sequence) if (IsAnUndefinedTile(tile)) return SavingResults.UndefinedTiles;
        }
        if (storeGivenFilename) FilenameOnly = Path.GetFileName(FullFilePath = filename);
        Encoding encoding = FileEncoding;
        if (VersionType == Version.GorH)
        {
            using (BinaryWriter binwriter = new BinaryWriter(File.Open(filename, FileMode.Create, FileAccess.Write), encoding))
            {
                #region LEV_Save
                binwriter.Write(new char[] { 'D', 'D', 'C', 'F', '&', 's', 's', 'f' });
                using (BinaryWriter
                    EDIT = new BinaryWriter(new MemoryStream(), encoding),
                    EDI2 = new BinaryWriter(new MemoryStream(), encoding),
                    LINF = new BinaryWriter(new MemoryStream(), encoding),
                    HSTR = new BinaryWriter(new MemoryStream(), encoding),
                    TILE = new BinaryWriter(new MemoryStream(), encoding),
                    TINFO = new BinaryWriter(new MemoryStream(), encoding),
                    TDATA = new BinaryWriter(new MemoryStream(), encoding),
                    EMSK = new BinaryWriter(new MemoryStream(), encoding),
                    MASK = new BinaryWriter(new MemoryStream(), encoding),
                    ANIM = new BinaryWriter(new MemoryStream(), encoding),
                    FLIP = new BinaryWriter(new MemoryStream(), encoding),
                    LAYR = new BinaryWriter(new MemoryStream(), encoding),
                    LINFO = new BinaryWriter(new MemoryStream(), encoding),
                    LDATA = new BinaryWriter(new MemoryStream(), encoding),
                    EVNT = new BinaryWriter(new MemoryStream(), encoding),
                    TMAP = new BinaryWriter(new MemoryStream(), encoding),
                    CMAP = new BinaryWriter(new MemoryStream(), encoding))
                {
                    EDIT.Write((byte)3);
                    EDIT.Write(Path.GetFileNameWithoutExtension(MainTilesetFilename));
                    EDIT.Write(TileCount);
                    EDIT.Write((byte)0);
                    for (int i = 0; i < DefaultLayers.Length; i++) EDIT.Write(DefaultLayers[i].Name);

                    EDI2.Write(1);
                    EDI2.Write(1024);
                    for (ushort i = 0; i < 1025; i++) EDI2.Write(0);

                    LINF.Write((ushort)258);
                    LINF.Write(Name);
                    LINF.Write(SecretLevel);
                    LINF.Write(Music);
                    LINF.Write(NextLevel);
                    LINF.Write(BonusLevel);
                    LINF.Write((int)MinLight);
                    LINF.Write((int)StartLight);
                    for (byte i = 0; i < 8; i++) LINF.Write(0);

                    HSTR.Write((ushort)256);
                    for (byte i = 0; i < 16; i++) HSTR.Write(Text[i]);

                    TILE.Write(263);

                    TINFO.Write(TileCount);
                    TINFO.Write(TileTypes, 1, (int)TileCount - 1);

                    J2TFile J2T = Tilesets[0];
                    J2T.WriteToTDATA(TDATA, TileTypes);
                    J2T.WriteToEMSK(EMSK);
                    DiscoverTilesThatAreFlippedAndOrUsedInLayer3();
                    J2T.WriteToMASK(MASK, IsEachTileUsed);

                    ANIM.Write((uint)NumberOfAnimations);
                    for (ushort i = 0; i < NumberOfAnimations; i++)
                    {
                        AnimatedTile an = Animations[i];
                        ANIM.Write(an.Framewait);
                        ANIM.Write(an.Random);
                        ANIM.Write(an.PingPongWait);
                        ANIM.Write(an.IsPingPong);
                        ANIM.Write(an.Speed);
                        ANIM.Write(an.FrameCount);
                        for (byte j = 0; j < an.FrameCount; j++) ANIM.Write(an.Sequence[j]);
                    }

                    for (ushort i = 1; i < TileCount; i++) if (IsEachTileFlipped[i]) FLIP.Write(i);
                    FLIP.Write((ushort)0xFFFF);

                    LAYR.Write(263);

                    foreach (Layer CurrentLayer in DefaultLayers)
                    {
                        LINFO.Write(
                                    (CurrentLayer.TileWidth ? 1 : 0) |
                                    (CurrentLayer.TileHeight ? 2 : 0) |
                                    (CurrentLayer.HasTiles ? 4 : 0) |
                                    (CurrentLayer.LimitVisibleRegion ? 8 : 0) |
                                    (CurrentLayer.IsTextured ? 16 : 0)
                                    );
                        LINFO.Write((ushort)CurrentLayer.Width);
                        LINFO.Write((ushort)CurrentLayer.Height);
                        LINFO.Write((short)CurrentLayer.ZAxis);
                        LINFO.Write(CurrentLayer.unknown1);
                        LINFO.Write(
                                    (CurrentLayer.unknown2 == 0 ? 0 : 1) |
                                    (CurrentLayer.XSpeed == 0 && CurrentLayer.YSpeed == 0 ? 0 : 2) |
                                    (CurrentLayer.AutoXSpeed == 0 && CurrentLayer.AutoYSpeed == 0 ? 0 : 4)
                                    );
                        if (CurrentLayer.unknown2 != 0) LINFO.Write(CurrentLayer.unknown2);
                        if (CurrentLayer.XSpeed != 0 || CurrentLayer.YSpeed != 0) { LINFO.Write((int)(CurrentLayer.XSpeed * 65536)); LINFO.Write((int)(CurrentLayer.YSpeed * 65536)); }
                        if (CurrentLayer.AutoXSpeed != 0 || CurrentLayer.AutoYSpeed != 0) { LINFO.Write((int)(CurrentLayer.AutoXSpeed * 65536)); LINFO.Write((int)(CurrentLayer.AutoYSpeed * 65536)); }
                    }

                    using (BinaryWriter data3writer = new BinaryWriter(new MemoryStream(), encoding))
                    using (BinaryWriter data4writer = new BinaryWriter(new MemoryStream(), encoding))
                    {
                        List<ushort[]> attestedWords = new List<ushort[]>(2048);
                        attestedWords.Add(new ushort[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                        data3writer.Write(new byte[32] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                        ushort[] tentativeWord = new ushort[16];
                        int tentativeIndex;
                        foreach (Layer CurrentLayer in DefaultLayers) if (CurrentLayer.HasTiles) for (ushort y = 0; y < CurrentLayer.Height; y++) for (ushort x = 0; x < (CurrentLayer.Width + 15) / 16 * 16; x += 16)
                                    {
                                        for (byte j = 0; j < 16; j++) tentativeWord[j] = (x + j < CurrentLayer.Width) ? SanitizeTileValue(CurrentLayer.TileMap[x + j, y]) : (ushort)0;
                                        if (CurrentLayer.id == 3 && //.LEV files may not care about this sort of thing?
                                            (
                                            (tentativeWord[0] % MaxTiles > TileCount && EventMap[x, y] != 0) ||
                                            (tentativeWord[1] % MaxTiles > TileCount && EventMap[x + 1, y] != 0) ||
                                            (tentativeWord[2] % MaxTiles > TileCount && EventMap[x + 2, y] != 0) ||
                                            (tentativeWord[3] % MaxTiles > TileCount && EventMap[x + 3, y] != 0) ||
                                            (tentativeWord[4] % MaxTiles > TileCount && EventMap[x + 4, y] != 0) ||
                                            (tentativeWord[5] % MaxTiles > TileCount && EventMap[x + 5, y] != 0) ||
                                            (tentativeWord[6] % MaxTiles > TileCount && EventMap[x + 6, y] != 0) ||
                                            (tentativeWord[7] % MaxTiles > TileCount && EventMap[x + 7, y] != 0) ||
                                            (tentativeWord[8] % MaxTiles > TileCount && EventMap[x + 8, y] != 0) ||
                                            (tentativeWord[9] % MaxTiles > TileCount && EventMap[x + 9, y] != 0) ||
                                            (tentativeWord[10] % MaxTiles > TileCount && EventMap[x + 10, y] != 0) ||
                                            (tentativeWord[11] % MaxTiles > TileCount && EventMap[x + 11, y] != 0) ||
                                            (tentativeWord[12] % MaxTiles > TileCount && EventMap[x + 12, y] != 0) ||
                                            (tentativeWord[13] % MaxTiles > TileCount && EventMap[x + 13, y] != 0) ||
                                            (tentativeWord[14] % MaxTiles > TileCount && EventMap[x + 14, y] != 0) ||
                                            (tentativeWord[15] % MaxTiles > TileCount && EventMap[x + 15, y] != 0)
                                            )
                                            ) { tentativeIndex = -1; }
                                        else tentativeIndex = attestedWords.FindIndex(delegate (ushort[] current)
                                            {
                                                return
                                                    tentativeWord[0] == current[0] &&
                                                    tentativeWord[1] == current[1] &&
                                                    tentativeWord[2] == current[2] &&
                                                    tentativeWord[3] == current[3] &&
                                                    tentativeWord[4] == current[4] &&
                                                    tentativeWord[5] == current[5] &&
                                                    tentativeWord[6] == current[6] &&
                                                    tentativeWord[7] == current[7] &&
                                                    tentativeWord[8] == current[8] &&
                                                    tentativeWord[9] == current[9] &&
                                                    tentativeWord[10] == current[10] &&
                                                    tentativeWord[11] == current[11] &&
                                                    tentativeWord[12] == current[12] &&
                                                    tentativeWord[13] == current[13] &&
                                                    tentativeWord[14] == current[14] &&
                                                    tentativeWord[15] == current[15]
                                                    ;
                                            });
                                        if (tentativeIndex == -1)
                                        {
                                            tentativeIndex = attestedWords.Count;
                                            attestedWords.Add(new ushort[16]);
                                            tentativeWord.CopyTo(attestedWords[tentativeIndex], 0);
                                            for (byte j = 0; j < 16; j++) data3writer.Write(tentativeWord[j]);
                                        }
                                        data4writer.Write((ushort)(tentativeIndex));
                                    }
                        LDATA.Write((ushort)(1024 - NumberOfAnimations));
                        LDATA.Write((ushort)attestedWords.Count);
                        LDATA.Write((ushort)0);
                        data3writer.BaseStream.Seek(0, SeekOrigin.Begin);
                        for (uint i = 0; i < data3writer.BaseStream.Length; i++) LDATA.Write((byte)data3writer.BaseStream.ReadByte());
                        data4writer.BaseStream.Seek(0, SeekOrigin.Begin);
                        for (uint i = 0; i < data4writer.BaseStream.Length; i++) LDATA.Write((byte)data4writer.BaseStream.ReadByte());
                    }

                    for (uint i = 0; i < EventMap.Length; i++) EVNT.Write(EventMap[i % EventMap.GetLength(0), i / EventMap.GetLength(0)]);

                    TMAP.Write((ushort)(257));
                    if (DefaultLayers[7].IsTextured)
                    {
                        TMAP.Write(1);
                        TMAP.Write(7);
                        TMAP.Write(0);
                        for (uint i = 0; i < 65536; i++)
                        {
                            TMAP.Write(J2T.Images[J2T.ImageAddress[DefaultLayers[7].TileMap[i % 256 / 32, i / 8192]]][i % 32 + i % 8192 / 256 * 32]);
                        }
                    }
                    else { TMAP.Write(0); TMAP.Write((ushort)0); }

                    CMAP.Write(256);
                    CMAP.Write((byte)1);
                    J2T.Palette.WriteLEVStyle(CMAP);

                    InputLEVSection(TILE, TINFO, new char[4] { 'I', 'N', 'F', 'O' });
                    InputLEVSection(TILE, TDATA, new char[4] { 'D', 'A', 'T', 'A' });
                    InputLEVSection(TILE, EMSK, new char[4] { 'E', 'M', 'S', 'K' });
                    InputLEVSection(TILE, MASK, new char[4] { 'M', 'A', 'S', 'K' });
                    InputLEVSection(TILE, ANIM, new char[4] { 'A', 'N', 'I', 'M' });
                    InputLEVSection(TILE, FLIP, new char[4] { 'F', 'L', 'I', 'P' });

                    InputLEVSection(LAYR, LINFO, new char[4] { 'I', 'N', 'F', 'O' });
                    InputLEVSection(LAYR, LDATA, new char[4] { 'D', 'A', 'T', 'A' });
                    InputLEVSection(LAYR, EVNT, new char[4] { 'E', 'V', 'N', 'T' });

                    InputLEVSection(binwriter, EDIT, new char[4] { 'E', 'D', 'I', 'T' });
                    InputLEVSection(binwriter, EDI2, new char[4] { 'E', 'D', 'I', '2' });
                    InputLEVSection(binwriter, LINF, new char[4] { 'L', 'I', 'N', 'F' });
                    InputLEVSection(binwriter, HSTR, new char[4] { 'H', 'S', 'T', 'R' });
                    InputLEVSection(binwriter, TILE, new char[4] { 'T', 'I', 'L', 'E' });
                    InputLEVSection(binwriter, LAYR, new char[4] { 'L', 'A', 'Y', 'R' });
                    InputLEVSection(binwriter, TMAP, new char[4] { 'T', 'M', 'A', 'P' });
                    InputLEVSection(binwriter, CMAP, new char[4] { 'C', 'M', 'A', 'P' });

                    binwriter.Seek(4, 0);
                    binwriter.Write((int)binwriter.BaseStream.Length - 8);
                }
                #endregion LEV_Save
            }
        }
        else
        {
            var WordMapsPerLayer = new Dictionary<Layer, List<ushort>>(AllLayers.Count);

            List<Layer[]> LayerArraysToSave = new List<Layer[]> { DefaultLayers };
            var nonDefaultLayers = new List<Layer>(AllLayers.Where(l => !l.isDefault));
            while (nonDefaultLayers.Count > 0)
            {
                var nonDefaultLayersToAddToList = new List<Layer>(nonDefaultLayers.Take(DefaultLayers.Length));
                nonDefaultLayers.RemoveRange(0, Math.Min(8, nonDefaultLayers.Count));
                while (nonDefaultLayersToAddToList.Count < 8)
                    nonDefaultLayersToAddToList.Add(new Layer());
                LayerArraysToSave.Add(nonDefaultLayersToAddToList.ToArray());
            }
            for (int i = 0; i < 4; i++)
                CompressedData[i] = new MemoryStream();

            using (BinaryWriter data3writer = new BinaryWriter(CompressedData[2], encoding, true)) //do the dictionary first, since it's shared across all .j2l files being saved
            {
                List<ushort[]> attestedWords = new List<ushort[]>(2048);
                attestedWords.Add(new ushort[4] { 0, 0, 0, 0 });
                data3writer.Write(new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
                ushort[] tentativeWord = new ushort[4];
                int tentativeIndex;
                foreach (Layer CurrentLayer in AllLayers)
                {
                    var words = WordMapsPerLayer[CurrentLayer] = new List<ushort>();
                    if (CurrentLayer.HasTiles)
                        for (ushort y = 0; y < CurrentLayer.Height; y++)
                            for (ushort x = 0; x < (CurrentLayer.RealWidth + 3) / 4 * 4; x += 4) // remember to write by RealWidth (rounded up), not Width
                            {
                                if (CurrentLayer.TileWidth) for (byte j = 0; j < 4; j++) tentativeWord[j] = SanitizeTileValue(CurrentLayer.TileMap[(x + j) % CurrentLayer.Width, y]);
                                else for (byte j = 0; j < 4; j++) tentativeWord[j] = (x + j < CurrentLayer.Width) ? SanitizeTileValue(CurrentLayer.TileMap[x + j, y]) : (ushort)0;
                                if (CurrentLayer.id == SpriteLayerID &&
                                    (
                                    (tentativeWord[0] % MaxTiles > TileCount && EventMap[x, y] != 0 && (EventMap[x, y] & (1 << 31)) == 0) ||
                                    (tentativeWord[1] % MaxTiles > TileCount && EventMap[x + 1, y] != 0 && (EventMap[x + 1, y] & (1 << 31)) == 0) ||
                                    (tentativeWord[2] % MaxTiles > TileCount && EventMap[x + 2, y] != 0 && (EventMap[x + 2, y] & (1 << 31)) == 0) ||
                                    (tentativeWord[3] % MaxTiles > TileCount && EventMap[x + 3, y] != 0 && (EventMap[x + 3, y] & (1 << 31)) == 0)
                                    )
                                    ) { tentativeIndex = -1; }
                                else tentativeIndex = attestedWords.FindIndex(delegate (ushort[] current) { return tentativeWord[0] == current[0] && tentativeWord[1] == current[1] && tentativeWord[2] == current[2] && tentativeWord[3] == current[3]; });
                                if (tentativeIndex == -1) // either it actually couldn't be found, or there were events on an animated tile
                                {
                                    tentativeIndex = attestedWords.Count;
                                    attestedWords.Add(new ushort[4]);
                                    tentativeWord.CopyTo(attestedWords[tentativeIndex], 0);
                                    for (byte j = 0; j < 4; j++) data3writer.Write(tentativeWord[j]);
                                }
                                words.Add((ushort)tentativeIndex);
                            }
                }
            }

            for (int layerArrayID = 0; layerArrayID < LayerArraysToSave.Count; ++layerArrayID) //per .j2l:
            {
                bool extraDataLevel = layerArrayID != 0;
                var layersToSave = LayerArraysToSave[layerArrayID];

                using (BinaryWriter data1writer = new BinaryWriter(CompressedData[0], encoding, true))
                {
                    uint SecurityString;
                    if (extraDataLevel) //junk level for storing extra data in
                    {
                        SecurityString = SecurityStringExtraDataNotForDirectEditing;
                    }
                    else if (Data5 != null || ContainsVerticallyFlippedTiles) //plus-only level, so damage the security envelope for JCS
                    {
                        SecurityString = SecurityStringMLLE;
                    }
                    else if (PasswordHash[0] != 0 || PasswordHash[1] != 0xBA || PasswordHash[2] != 0xBE) //has a password
                    {
                        SecurityString = SecurityStringPassworded;
                    }
                    else
                    {
                        SecurityString = SecurityStringInsecure;
                    }
                    data1writer.Write(JCSHorizontalFocus);
                    data1writer.Write((ushort)(SecurityString >> 16));
                    data1writer.Write(JCSVerticalFocus);
                    data1writer.Write((ushort)(SecurityString & 0xFFFFu));
                    data1writer.Write((byte)(JCSFocusedLayer | (SecurityString != SecurityStringInsecure ? 0xF0 : 0x00)));
                    data1writer.Write(MinLight);
                    data1writer.Write(StartLight);
                    data1writer.Write(NumberOfAnimations);
                    data1writer.Write(UsesVerticalSplitscreen);
                    if (VersionType == Version.BC) data1writer.Write((byte)2);
                    else if (VersionType == Version.O) data1writer.Write((byte)1);
                    else data1writer.Write(LevelMode);
                    data1writer.Write((uint)0); // StreamSize; this gets replaced later with an actual calculation
                    data1writer.Write(getBytes(encoding, Name, 32));
                    data1writer.Write(getBytes(encoding, MainTilesetFilename, 32));
                    data1writer.Write(getBytes(encoding, BonusLevel, 32));
                    data1writer.Write(getBytes(encoding, NextLevel, 32));
                    data1writer.Write(getBytes(encoding, SecretLevel, 32));
                    data1writer.Write(getBytes(encoding, Music, 32));
                    for (byte i = 0; i < 16; i++) data1writer.Write(getBytes(encoding, Text[i], 512));
                    if (VersionType == Version.AGA) for (byte i = 0; i < 48; i++)
                        {
                            if (AGA_SoundPointer[i] == null) for (byte j = 0; j < 16; j++) data1writer.Write(0); //16 longs = 64 bytes
                            else data1writer.Write(getBytes(encoding, (AGA_SoundPointer[i][0] + "\\" + AGA_SoundPointer[i][1]), 64));
                        }
                    foreach (Layer layer in layersToSave) data1writer.Write((layer.TileWidth ? 1 : 0) + (layer.TileHeight ? 2 : 0) + (layer.LimitVisibleRegion ? 4 : 0) + (layer.IsTextured ? 8 : 0) + (layer.HasStars ? 16 : 0));
                    foreach (Layer layer in layersToSave) data1writer.Write(layer.unknown1);
                    foreach (Layer layer in layersToSave) data1writer.Write(layer.HasTiles);
                    foreach (Layer layer in layersToSave) data1writer.Write(layer.Width);
                    foreach (Layer layer in layersToSave) data1writer.Write(layer.RealWidth);
                    foreach (Layer layer in layersToSave) data1writer.Write(layer.Height);
                    foreach (Layer layer in layersToSave) data1writer.Write(layer.ZAxis);
                    foreach (Layer layer in layersToSave) data1writer.Write(layer.unknown2);
                    foreach (Layer layer in layersToSave) data1writer.Write((int)(layer.WaveX * 65536));
                    foreach (Layer layer in layersToSave) data1writer.Write((int)(layer.WaveY * 65536));
                    foreach (Layer layer in layersToSave) data1writer.Write((int)(layer.XSpeed * 65536));
                    foreach (Layer layer in layersToSave) data1writer.Write((int)(layer.YSpeed * 65536));
                    foreach (Layer layer in layersToSave) data1writer.Write((int)(layer.AutoXSpeed * 65536));
                    foreach (Layer layer in layersToSave) data1writer.Write((int)(layer.AutoYSpeed * 65536));
                    foreach (Layer layer in layersToSave) data1writer.Write(layer.TextureMode);
                    foreach (Layer layer in layersToSave) { data1writer.Write(layer.TexturParam1); data1writer.Write(layer.TexturParam2); data1writer.Write(layer.TexturParam3); }
                    data1writer.Write((ushort)(MaxTiles - NumberOfAnimations));
                    for (ushort i = 0; i < MaxTiles; i++) data1writer.Write(EventTiles[i]); // I use MaxTiles for this and TileTypes because I upsize it to 4096 when converting to TSF, but don't bother downsizing it when converting back to 1.23
                    DiscoverTilesThatAreFlippedAndOrUsedInLayer3();
                    for (ushort i = 0; i < MaxTiles; i++) data1writer.Write(IsEachTileFlipped[i]);
                    for (ushort i = 0; i < MaxTiles; i++) data1writer.Write(TileTypes[i]);
                    for (ushort i = 0; i < MaxTiles; i++) data1writer.Write((byte)0); // yeah, this section doesn't do anything
                    if (VersionType == Version.AGA) data1writer.Write(AGA_unknownsection); // mostly zeroes, but there are some ones, so there IS information stored here... WHAT COULD IT BE?
                    for (byte i = 0; i < NumberOfAnimations; i++) // JCS: 256 for TSF, else 128. But that's not necessary.
                    {
                        //if (i < NumberOfAnimations)
                        //{
                        data1writer.Write(Animations[i].Framewait);
                        data1writer.Write(Animations[i].Random);
                        data1writer.Write(Animations[i].PingPongWait);
                        data1writer.Write(Animations[i].IsPingPong);
                        data1writer.Write(Animations[i].Speed);
                        data1writer.Write(Animations[i].FrameCount);
                        for (byte j = 0; j < 64; j++) data1writer.Write(SanitizeTileValue(Animations[i].Sequence[j]));
                        //}
                        //else data1writer.Write(new byte[137]);
                    }
                    data1writer.Seek(15, SeekOrigin.Begin); //go to the StreamSize long
                    data1writer.Write((int)CompressedData[0].Length); //and replace it with the actual length of the section
                }
                using (BinaryWriter data2writer = new BinaryWriter(CompressedData[1], encoding, true))
                {
                    if (VersionType == Version.AGA) //could use some commenting
                    {
                        CreateGlobalAGAEventsListIfNeedBe();
                        AGA_LocalEvents = new List<String>();
                        foreach (AGAEvent saveProspectiveEvent in AGA_EventMap)
                        {
                            if (saveProspectiveEvent.ID != 0 && !AGA_LocalEvents.Contains(AGA_GlobalEvents[(int)saveProspectiveEvent.ID])) AGA_LocalEvents.Add(AGA_GlobalEvents[(int)saveProspectiveEvent.ID]);
                        }
                        data2writer.Write((ushort)AGA_LocalEvents.Count);
                        for (ushort i = 0; i < AGA_LocalEvents.Count; i++) data2writer.Write(getBytes(encoding, AGA_LocalEvents[i], 64));
                        for (ushort y = 0; y < AGA_EventMap.GetLength(1); y++) for (ushort x = 0; x < AGA_EventMap.GetLength(0); x++) if (AGA_EventMap[x, y].ID != 0)
                                {
                                    data2writer.Write((ushort)x); data2writer.Write((ushort)y);
                                    data2writer.Write((ushort)AGA_LocalEvents.FindIndex((string pointer) => { return pointer == AGA_GlobalEvents[(int)AGA_EventMap[x, y].ID]; }));
                                    data2writer.Write(
                                        (AGA_EventMap[x, y].Bits[0] ? (uint)32 : 0) |
                                        (AGA_EventMap[x, y].Bits[1] ? (uint)64 : 0) |
                                        (AGA_EventMap[x, y].Bits[2] ? (uint)128 : 0) |
                                        (AGA_EventMap[x, y].Bits[3] ? (uint)256 : 0) |
                                        (AGA_EventMap[x, y].Bits[4] ? (uint)512 : 0) |
                                        (AGA_EventMap[x, y].Bits[5] ? (uint)1024 : 0) |
                                        (AGA_EventMap[x, y].Bits[6] ? (uint)2048 : 0) |
                                        (AGA_EventMap[x, y].Bits[7] ? (uint)4096 : 0) |
                                        (AGA_EventMap[x, y].Bits[8] ? (uint)8192 : 0) |
                                        (AGA_EventMap[x, y].Bits[9] ? (uint)16384 : 0) |
                                        (AGA_EventMap[x, y].HasParameters() ? (uint)0x80000000 : 0)
                                        );
                                    if (AGA_EventMap[x, y].HasParameters())
                                    {
                                        data2writer.Write(AGA_EventMap[x, y].GetNumberOfParameters() * 4 + 8 + AGA_EventMap[x, y].GetNumberOfBytesItWillTakeToWriteStrings());
                                        data2writer.Write((ushort)(AGA_EventMap[x, y].GetNumberOfBytesItWillTakeToWriteStrings() == 0 ? 0 : 2));
                                        data2writer.Write((ushort)(AGA_EventMap[x, y].GetNumberOfParameters() / 2));
                                        for (byte i = 0; i < AGA_EventMap[x, y].GetNumberOfParameters(); i++) data2writer.Write(AGA_EventMap[x, y].Longs[i]);
                                        if ((AGA_EventMap[x, y].Strings[0] ?? "") != "" || (AGA_EventMap[x, y].Strings[1] ?? "") != "" || (AGA_EventMap[x, y].Strings[2] ?? "") != "") //needs to be updated in light of the reflection that some events use more than three strings
                                        {
                                            data2writer.Write(encoding.GetByteCount(AGA_EventMap[x, y].Strings[0]) + 1);
                                            data2writer.Write(encoding.GetBytes(AGA_EventMap[x, y].Strings[0]));
                                            data2writer.Write((byte)0);
                                            if ((AGA_EventMap[x, y].Strings[1] ?? "") != "" || (AGA_EventMap[x, y].Strings[2] ?? "") != "")
                                            {
                                                data2writer.Write(encoding.GetByteCount(AGA_EventMap[x, y].Strings[1]) + 1);
                                                data2writer.Write(encoding.GetBytes(AGA_EventMap[x, y].Strings[1]));
                                                data2writer.Write((byte)0);
                                                if ((AGA_EventMap[x, y].Strings[2] ?? "") != "")
                                                {
                                                    data2writer.Write(encoding.GetByteCount(AGA_EventMap[x, y].Strings[2]) + 1);
                                                    data2writer.Write(encoding.GetBytes(AGA_EventMap[x, y].Strings[2]));
                                                    data2writer.Write((byte)0);
                                                }
                                            }
                                        }
                                    }
                                }
                    }
                    else
                    {
                        if (!extraDataLevel)
                            for (uint i = 0; i < EventMap.Length; i++) data2writer.Write(EventMap[i % EventMap.GetLength(0), i / EventMap.GetLength(0)]);
                        else //this is just an extra data level, so write some junk data to the event map... even though it might not ever get read by anything
                            data2writer.Write(new byte[layersToSave[SpriteLayerID].Width * layersToSave[SpriteLayerID].Height * sizeof(uint)]);
                    }
                }
                using (BinaryWriter data4writer = new BinaryWriter(CompressedData[3], encoding, true))
                {
                    List<ushort> words;
                    foreach (var layer in layersToSave)
                        if (WordMapsPerLayer.TryGetValue(layer, out words)) //not an empty filler layer
                            foreach (ushort tileID in words)
                                data4writer.Write(tileID);
                }
                using (BinaryWriter binwriter = new BinaryWriter(
                    File.Open(
                        !extraDataLevel ?
                            filename :
                            MLLE.PlusPropertyList.GetExtraDataLevelFilepath(filename, layerArrayID - 1),
                        FileMode.Create, FileAccess.Write),
                    encoding
                ))
                {
                    binwriter.Write(encoding.GetBytes(Header)); //the copyright notice
                    binwriter.Write(encoding.GetBytes(Magic)); // 'LEVL'
                    binwriter.Write(!extraDataLevel ? PasswordHash : new byte[] { 0x00, 0xBA, 0xBE }); // The password hash is calculated in SetPassword(), not here
                    binwriter.Write(IsHiddenInHCL || extraDataLevel); //extra .j2l files shouldn't appear in the HCL, obviously
                    binwriter.Write(getBytes(encoding, !extraDataLevel ? Name : "MLLE Extra Data", 32));
                    binwriter.Write((ushort)((VersionType == Version.AGA) ? 0x100 : (VersionType == Version.TSF) ? 0x203 : 0x202));
                    binwriter.Write(new byte[40]); // To be filled in later with filesize, CRC32, and the compressed and uncompressed data lengths, for a total of 10 longs or 40 bytes.
                    CRC32 CRCCalculator = new CRC32();
                    for (byte i = 0; i < 4; i++)
                    {
                        UncompressedDataLength[i] = (int)CompressedData[i].Length;
                        var zcomparray = ZlibStream.CompressBuffer(CompressedData[i].ToArray());
                        binwriter.Write(zcomparray);
                        CompressedDataLength[i] = zcomparray.Length;
                        CRCCalculator.SlurpBlock(zcomparray, 0, zcomparray.Length);
                    }
                    if (!extraDataLevel && Data5 != null)
                    {
                        binwriter.Write(Data5); //immediately after the compressed Data4 block
                        CRCCalculator.SlurpBlock(Data5, 0, Data5.Length);
                    }
                    binwriter.Seek(encoding.GetByteCount(Header) + 42, 0);
                    binwriter.Write((int)(binwriter.BaseStream.Length));
                    binwriter.Write(CRCCalculator.Crc32Result); Crc32 = CRCCalculator.Crc32Result;
                    for (byte i = 0; i < 4; i++)
                    {
                        binwriter.Write(CompressedDataLength[i]);
                        binwriter.Write(UncompressedDataLength[i]);
                    }
                }

                foreach (MemoryStream stream in CompressedData)
                    if (stream != CompressedData[2]) //not the dictionary
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.SetLength(0);
                    }
            }
            foreach (MemoryStream stream in CompressedData)
                stream.Dispose();
        }
        return SavingResults.Success;
    }

    private void DiscoverTilesThatAreFlippedAndOrUsedInLayer3()
    {
        for (ushort i = 0; i < MaxTiles; i++) IsEachTileUsed[i] = IsEachTileFlipped[i] = false;
        IsEachTileUsed[0] = true;
        bool isFlipped;
        foreach (Layer CurrentLayer in AllLayers)
        {
            foreach (ushort tileUsed in CurrentLayer.TileMap)
            {
                isFlipped = false;
                if (tileUsed % MaxTiles < TileCount)
                {
                    if (CurrentLayer.id == SpriteLayerID) IsEachTileUsed[tileUsed % MaxTiles] = true; // ignore this; it's only useful for .LEV saving
                    if (tileUsed >= MaxTiles && tileUsed % MaxTiles < Tilesets[0].TileCount) IsEachTileFlipped[tileUsed % MaxTiles] = true;
                }
                else
                {
                    isFlipped = tileUsed > MaxTiles;
                    FlipMaskEvaluateAnimation(
                        Animations[NumberOfAnimations - (MaxTiles - tileUsed % MaxTiles)],
                        CurrentLayer.id == 3,
                        ref isFlipped,
                        ref IsEachTileUsed,
                        ref IsEachTileFlipped);
                }
            }
        }
    }

    public void ResetAllAnimatedTiles() { for (byte i = 0; i < NumberOfAnimations; i++) Animations[i].Reset(); }
    public void DeleteAnimation(byte id, bool adjustLaterAnims)
    {
        for (byte i = id; i < NumberOfAnimations;)
        {
            if (i == 127) Animations[i] = new AnimatedTile();
            else Animations[i] = Animations[++i];
        }
        NumberOfAnimations--;
        int threshhold = (adjustLaterAnims) ? AnimOffset + id : MaxTiles - 1;
        foreach (Layer CurrentLayer in AllLayers)
            if (CurrentLayer.HasTiles)
                for (ushort x = 0; x < CurrentLayer.Width; x++)
                    for (ushort y = 0; y < CurrentLayer.Height; y++)
                        IncreaseAnimationInstanceIfNeeded(CurrentLayer.TileMap, x, y, ref threshhold);
        for (byte i = 0; i < NumberOfAnimations; i++)
            for (byte j = 0; j < Animations[i].FrameCount; j++)
                IncreaseAnimationInstanceIfNeeded(ref Animations[i].Sequence[j], ref threshhold);
        AnimOffset++;
    }
    private void IncreaseAnimationInstanceIfNeeded(ArrayMap<ushort> tileMap, int x, int y, ref int threshhold)
    {
        ushort tile = tileMap[x, y];
        IncreaseAnimationInstanceIfNeeded(ref tile, ref threshhold);
        tileMap[x, y] = tile;
    }
    private void IncreaseAnimationInstanceIfNeeded(ref ushort tile, ref int threshhold)
    {
        var nutile = tile % MaxTiles;
        if (nutile >= AnimOffset)
        {
            if (nutile == threshhold) tile = 0;
            else if (nutile < threshhold) tile++;
        }
    }
    public InsertFrameResults InsertAnimation(AnimatedTile nuAnim, byte location = 255)
    {
        if (NumberOfAnimations == 128 || NumberOfAnimations + TileCount + 1 == MaxTiles) return InsertFrameResults.Full;
        if (location > NumberOfAnimations) location = (byte)NumberOfAnimations;
        for (ushort i = NumberOfAnimations; i > location; ) Animations[i] = Animations[--i];
        Animations[location] = nuAnim;
        NumberOfAnimations++;
        int threshhold = AnimOffset + location;
        foreach (Layer CurrentLayer in AllLayers)
            if (CurrentLayer.HasTiles)
                for (ushort x = 0; x < CurrentLayer.Width; x++)
                    for (ushort y = 0; y < CurrentLayer.Height; y++)
                        DecreaseAnimationInstanceIfNeeded(CurrentLayer.TileMap, x, y, ref threshhold);
        for (byte i = 0; i < NumberOfAnimations; i++)
            for (byte j = 0; j < Animations[i].FrameCount; j++)
                DecreaseAnimationInstanceIfNeeded(ref Animations[i].Sequence[j], ref threshhold);
        AnimOffset--;
        return InsertFrameResults.Success;
    }
    private void DecreaseAnimationInstanceIfNeeded(ArrayMap<ushort> tileMap, int x, int y, ref int threshhold)
    {
        ushort tile = tileMap[x, y];
        DecreaseAnimationInstanceIfNeeded(ref tile, ref threshhold);
        tileMap[x, y] = tile;
    }
    private void DecreaseAnimationInstanceIfNeeded(ref ushort tile, ref int threshhold)
    {
        var nutile = tile % MaxTiles;
        if (nutile >= AnimOffset && nutile < threshhold) tile--;
    }

    public VersionChangeResults ChangeVersion(Version nuVersion)
    {
        uint J2TTileCount = (!HasTiles) ? 1 : TileCount;
        if (nuVersion != VersionType) switch (nuVersion)
            {
                case Version.BC:
                case Version.O:
                case Version.JJ2:
                case Version.GorH:
                    if (J2TTileCount > 1020) return VersionChangeResults.TilesetTooBig;
                    else if (J2TTileCount + NumberOfAnimations > 1023) return VersionChangeResults.TooManyAnimatedTiles;
                    else
                    {
                        if (nuVersion != Version.GorH)
                        {
                            Header = (nuVersion == Version.BC || nuVersion == Version.O) ? "" : StandardHeader;
                        }
                        if (MaxTiles == 4096)
                        {
                            for (byte i = 0; i < NumberOfAnimations; i++) Animations[i].ChangeVersion(ref nuVersion,ref J2TTileCount, ref NumberOfAnimations);
                            foreach (Layer CurrentLayer in AllLayers) if (CurrentLayer.HasTiles) for (ushort x = 0; x < CurrentLayer.Width; x++) for (ushort y = 0; y < CurrentLayer.Height; y++)
                                        {
                                            if (CurrentLayer.TileMap[x, y] > 4095 + J2TTileCount) CurrentLayer.TileMap[x, y] -= 7168; //Flipped animations
                                            else if (CurrentLayer.TileMap[x, y] >= J2TTileCount)
                                            {
                                                if (CurrentLayer.TileMap[x, y] >= 4096 - NumberOfAnimations) CurrentLayer.TileMap[x, y] -= 3072; //Animations and flipped
                                                else CurrentLayer.TileMap[x, y] += 8192; //Leftover +1020s from tileset change
                                            }
                                        }
                            /*Array.Resize(ref EventTiles, 1024);
                            Array.Resize(ref unknownsection2, 1024);
                            Array.Resize(ref TileTypes, 1024);
                            Array.Resize(ref unknownsection3, 1024);*/
                            AnimOffset -= 4096 - 1024;
                        }
                        VersionType = nuVersion;
                        return VersionChangeResults.Success;
                    }
                case Version.AGA:
                case Version.TSF:
                    Header = (nuVersion == Version.AGA) ? "" : StandardHeader;
                    if (MaxTiles == 1024)
                    {
                        for (byte i = 0; i < NumberOfAnimations; i++) Animations[i].ChangeVersion(ref nuVersion, ref J2TTileCount, ref NumberOfAnimations);
                        foreach (Layer CurrentLayer in AllLayers) if (CurrentLayer.HasTiles) for (ushort x = 0; x < CurrentLayer.Width; x++) for (ushort y = 0; y < CurrentLayer.Height; y++)
                                    {
                                        if (CurrentLayer.TileMap[x, y] > 8192) CurrentLayer.TileMap[x, y] -= 8192; //Restored leftovers
                                        else if (CurrentLayer.TileMap[x, y] > 1023 + J2TTileCount) CurrentLayer.TileMap[x, y] += 7168; //Flipped animations
                                        else if (CurrentLayer.TileMap[x, y] >= J2TTileCount) CurrentLayer.TileMap[x, y] += 3072; //Animations and flipped tiles
                                    }
                        if (EventTiles.Length == 1024)
                        {
                            Array.Resize(ref EventTiles, 4096);
                            Array.Resize(ref IsEachTileFlipped, 4096);
                            Array.Resize(ref TileTypes, 4096);
                            Array.Resize(ref IsEachTileUsed, 4096);
                        }
                        AnimOffset += 4096 - 1024;
                    }
                    if (nuVersion == Version.AGA) //pretend support!
                    {
                        AGA_SoundPointer = new string[48][];
                        AGA_unknownsection = new byte[32768];
                        CreateGlobalAGAEventsListIfNeedBe();
                        AGA_LocalEvents = new List<String>();
                        AGA_EventMap = new AGAEvent[SpriteLayer.Width, SpriteLayer.Height];
                    }
                    VersionType = nuVersion;
                    return VersionChangeResults.Success;
                default:
                    return VersionChangeResults.UnsupportedConversion;
            }
        else return VersionChangeResults.Success;
    }
    public VersionChangeResults ChangeTileset(string filename, bool avoidRedundancy = true, Dictionary<Version, string> defaultDirectories = null)
    {
        if (avoidRedundancy && Path.GetFileName(filename) == MainTilesetFilename) return VersionChangeResults.Success;
        J2TFile tryout = new J2TFile(Path.Combine((defaultDirectories == null) ? Path.GetDirectoryName(filename) : defaultDirectories[VersionType], filename));         
        //J2TFile tryout = new J2TFile(filename);
        if (VersionType == tryout.VersionType || (tryout.VersionType == Version.AmbiguousBCO && (VersionType == Version.BC || VersionType == Version.O)) || (VersionType == Version.GorH && tryout.TotalNumberOfTiles <= 1020) || (VersionType == Version.TSF && tryout.VersionType == Version.JJ2) || (tryout.VersionType == Version.Plus && (VersionType == Version.TSF || VersionType == Version.JJ2)))
        {
            int numberOfTilesBesidesThoseOfTheFirstTileset = NumberOfAnimations;
            if (Tilesets.Count > 1)
                numberOfTilesBesidesThoseOfTheFirstTileset += (int)(TileCount - Tilesets[0].TileCount);
            if (tryout.TotalNumberOfTiles + numberOfTilesBesidesThoseOfTheFirstTileset < MaxTiles)
            {
                if (!HasTiles)
                {
                    Tilesets = new List<J2TFile>(1) { null };
                }
                else if (Tilesets.Count > 1)
                {
                    int oldTilesetTileCount = (int)Tilesets[0].TileCount;
                    int newTilesetTileCount = (int)tryout.TileCount;
                    int tileCountDifference = oldTilesetTileCount - newTilesetTileCount;
                    if (tileCountDifference != 0) //differently sized tilesets
                    {
                        if (tileCountDifference > 0) //new tileset is smaller
                            ChangeRangeOfTiles( //delete tiles used by the old tileset that no longer exist
                                newTilesetTileCount,
                                oldTilesetTileCount - 1,
                                () => { return true; },
                                (ushort tileID) => { return 0; }
                            );
                        ChangeRangeOfTiles( //move tiles used by subsequent tilesets up or down as needed
                            oldTilesetTileCount,
                            AnimOffset - 1,
                            () => { return true; },
                            (ushort tileID) => { return (ushort)(tileID - tileCountDifference); }
                        );
                    }
                }
                Tilesets[0] = tryout;
                MainTilesetFilename = Path.GetFileName(filename);
                return VersionChangeResults.Success;
            }
            else return VersionChangeResults.TooManyAnimatedTiles;
        }
        else return VersionChangeResults.UnsupportedConversion;
    }
    
    internal bool ChangeRangeOfTiles(int first, int last, Func<bool> getPermission, Func<ushort, ushort> action)
    {
        if (first > last)
            return false; //something went wrong
        bool permissionGotten = false;

        foreach (Layer CurrentLayer in AllLayers)
            if (CurrentLayer.HasTiles)
                for (uint x = 0; x < CurrentLayer.Width; ++x)
                    for (uint y = 0; y < CurrentLayer.Height; ++y)
                    {
                        var prospectiveTileToChange = CurrentLayer.TileMap[x, y] % MaxTiles;
                        if (prospectiveTileToChange >= first && prospectiveTileToChange <= last)
                        {
                            if (!permissionGotten)
                            {
                                if (!getPermission())
                                    return false;
                                permissionGotten = true;
                            }
                            CurrentLayer.TileMap[x, y] = action(CurrentLayer.TileMap[x, y]);
                        }
                    }
        foreach (AnimatedTile CurrentAnimatedTile in Animations)
        {
            var sequence = CurrentAnimatedTile.Sequence;
            for (int frameID = 0; frameID < sequence.Length; ++frameID)
            {
                var prospectiveTileToChange = sequence[frameID] % MaxTiles;
                if (prospectiveTileToChange >= first && prospectiveTileToChange <= last)
                {
                    if (!permissionGotten)
                    {
                        if (!getPermission())
                            return false;
                        permissionGotten = true;
                    }
                    sequence[frameID] = action(sequence[frameID]);
                }
            }
        }

        return true;
    }
}
