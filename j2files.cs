using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Ionic.Zlib;

public enum Version { JJ2, TSF, O, GorH, BC, AGA, AmbiguousBCO };
public enum VersionChangeResults { Success, TilesetTooBig, TooManyAnimatedTiles, UnsupportedConversion };
public enum SavingResults { Success, UndefinedTiles, NoTilesetSelected, TilesetIsDifferentVersion, Error };
public enum OpeningResults { Success, SuccessfulButAmbiguous, PasswordNeeded, WrongPassword, UnexpectedFourCC, Error };
public enum InsertFrameResults { Success, Full, StackOverflow };

abstract class J2File //The fields shared by .j2l and .j2t files. No methods/interface just yet, though that would be cool too.
{
    internal static string StandardHeader = "                      Jazz Jackrabbit 2 Data File\x0D\x0A\x0D\x0A         Retail distribution of this data is prohibited without\x0D\x0A             written permission from Epic MegaGames, Inc.\x0D\x0A\x0D\x0A\x1A";
    internal string Header; //The copyright notice
    internal string Magic; //"LEVL" or "TILE," depending
    public string Name; //Self-explanatory
    internal string FilenameOnly, FullFilePath;
    public uint Size; //Stored in the files for some reason
    public int Crc32; //To make sure the file hasn't been tampered with
    internal ushort VersionNumber; // The version as stored in the file; unfortunately, BC and AGA use the same version as JJ2 for .j2t, and BC and JJ2 share .j2l, even though the formats are slightly different.
    internal Version VersionType; //Using an enum, the ACTUAL version of each file. "BC" lumps in BC proper and also 1.10o since there is no internal distinction.
    public int[] CompressedDataLength = new int[4];
    public int[] UncompressedDataLength = new int[4];
    internal MemoryStream[] UncompressedData = new MemoryStream[4];
    internal MemoryStream[] CompressedData = new MemoryStream[4];
    public static Dictionary<Version, string> FullVersionNames = new Dictionary<Version, string>() {
        {Version.TSF, "The Secret Files (v1.24)"},
        {Version.JJ2, "Jazz 2 (v1.20-1.23)"},
        {Version.BC, "Battery Check"},
        {Version.O, "Jazz 2 BETA v1.10o"},
        {Version.AmbiguousBCO, "Battery Check/Jazz 2 BETA v1.10o"},
        {Version.AGA, "Animaniacs: A Gigantic Adventure"},
        {Version.GorH, "Jazz 2 OEM v1.00g/h"} };
}

class J2TFile : J2File
{
    internal uint Signature;
    public byte[][] Palette = new byte[256][]; //Stored as a 256-long byte array of R,B,G,A. A is always 0 because .j2t files aren't that complicated.
    internal int MaxTiles; //A simple function of VersionType: does the file support up to 4096 or only up to 1024 tiles? Good for looping and array sizes.
    public uint TileCount; //Again, good for looping.
    public bool[] IsFullyOpaque; //Not too useful, but there's a shortcut bool to indicate that a tile has no transparency at ALL and may be drawn more simply.
    internal byte[] unknown1;
    public uint[] ImageAddress; //Where in Data2 the 1024 pixels for each tile are. Here divided by 1024 just to be straight-forward.
    internal uint[] unknown2;
    public uint[] TransparencyMaskAddress;
    internal uint[] TransparencyMaskOffset;
    internal uint[] unknown3;
    public uint[] MaskAddress;
    public uint[] FlippedMaskAddress;
    internal byte[][] TransparencyMaskJCS_Style;
    internal byte[][] TransparencyMaskJJ2_Style;
    internal byte[][] Images;
    internal uint data3Pointer = 0;
    internal ushort data3Counter = 0;
    internal int data3BlockNumber = 0;
    internal int data3BlockNumber2 = 0;
    internal int data3Skip = 0;
    internal byte[][] Masks;

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
        using (BinaryReader binreader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
        {
            #region header
            Header = (binreader.PeekChar() == 32) ? new string(binreader.ReadChars(180)) : "";
            Magic = new string(binreader.ReadChars(4));
            Signature = binreader.ReadUInt32();
            Name = new string(binreader.ReadChars(32)).TrimEnd('\0');
            VersionNumber = binreader.ReadUInt16();
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
                case 512:
                    if (UncompressedDataLength[0] == 107524)
                    {
                        VersionType = Version.AGA;
                        MaxTiles = 4096;
                    }
                    else if (Header.Length == 0)
                    {
                        VersionType = Version.AmbiguousBCO;
                        MaxTiles = 1024;
                    }
                    else
                    {
                        VersionType = Version.JJ2;
                        MaxTiles = 1024;
                    }
                    break;
                case 513:
                    VersionType = Version.TSF;
                    MaxTiles = 4096;
                    break;
            }
            IsFullyOpaque = new bool[MaxTiles];
            unknown1 = new byte[MaxTiles];
            ImageAddress = new uint[MaxTiles];
            unknown2 = new uint[MaxTiles];
            TransparencyMaskAddress = new uint[MaxTiles];
            TransparencyMaskOffset = new uint[MaxTiles];
            unknown3 = new uint[MaxTiles];
            MaskAddress = new uint[MaxTiles];
            FlippedMaskAddress = new uint[MaxTiles];
            TransparencyMaskJCS_Style = new byte[MaxTiles][];
            TransparencyMaskJJ2_Style = new byte[MaxTiles][];
            #endregion setup version-specific sizes
            for (byte i = 0; i < 4; i++) UncompressedData[i] = new MemoryStream(ZlibStream.UncompressBuffer(binreader.ReadBytes(CompressedDataLength[i])));
            #endregion header
            #region data1
            BinaryReader data1reader = new BinaryReader(UncompressedData[0]);
            for (short i = 0; i < 256; i++) Palette[i] = data1reader.ReadBytes(4);
            TileCount = data1reader.ReadUInt32();
            for (short i = 0; i < MaxTiles; i++) IsFullyOpaque[i] = data1reader.ReadBoolean();
            for (short i = 0; i < MaxTiles; i++) unknown1[i] = data1reader.ReadByte();
            for (short i = 0; i < MaxTiles; i++) ImageAddress[i] = data1reader.ReadUInt32() / 1024;
            for (short i = 0; i < MaxTiles; i++) unknown2[i] = data1reader.ReadUInt32();
            for (short i = 0; i < MaxTiles; i++) TransparencyMaskAddress[i] = data1reader.ReadUInt32();
            for (short i = 0; i < MaxTiles; i++) unknown3[i] = data1reader.ReadUInt32();
            for (short i = 0; i < MaxTiles; i++) MaskAddress[i] = data1reader.ReadUInt32() / 128;
            for (short i = 0; i < MaxTiles; i++) FlippedMaskAddress[i] = data1reader.ReadUInt32() / 128;
            #endregion data1
            #region data2
            BinaryReader data2reader = new BinaryReader(UncompressedData[1]);
            Images = new byte[UncompressedDataLength[1] / 1024][];
            for (short i = 0; i < UncompressedDataLength[1] / 1024; i++) Images[i] = data2reader.ReadBytes(1024);
            #endregion
            #region data3
            BinaryReader data3reader = new BinaryReader(UncompressedData[2]);
            while (data3Pointer < UncompressedDataLength[2])
            {
                TransparencyMaskOffset[data3Counter] = data3Pointer;
                TransparencyMaskJCS_Style[data3Counter] = Convert128BitsToByteMask(data3reader.ReadBytes(128)); data3Pointer += 128;
                TransparencyMaskJJ2_Style[data3Counter] = new byte[1024];// for (ushort i = 0; i < 1024; i++) tmasksjj2[data3counter][i] = 0;
                for (byte row = 0; row < 32; row++)
                {
                    data3Skip = row * 32;
                    data3BlockNumber = data3reader.ReadByte(); data3Pointer++;
                    for (byte i = 0; i < data3BlockNumber; i++)
                    {
                        data3Skip += data3reader.ReadByte(); data3Pointer++;
                        data3BlockNumber2 = data3Skip + data3reader.ReadByte(); data3Pointer++;
                        for (; data3Skip < data3BlockNumber2; data3Skip++) TransparencyMaskJJ2_Style[data3Counter][data3Skip] = 1;
                    }
                }
                data3Counter++;
            }
            #endregion data3
            #region data4
            BinaryReader data4reader = new BinaryReader(UncompressedData[3]);
            Masks = new byte[UncompressedDataLength[3] / 128][];
            for (short i = 0; i < UncompressedDataLength[3] / 128; i++) Masks[i] = Convert128BitsToByteMask(data4reader.ReadBytes(128));
            #endregion data4
        }
    }
    //public byte[] GetImage(ushort id) { return Images[ImageAddress[id]]; }
    //public byte[] GetMask(ushort id) { return Masks[MaskAddress[id]]; }
    //public byte[] GetFMask(ushort id) { return Masks[FlippedMaskAddress[id]]; }
}

class Layer
{
    public byte id;
    public bool TileWidth;
    public bool TileHeight;
    public bool LimitVisibleRegion;
    public bool IsTextured;
    public bool HasStars;
    public byte unknown1;
    public bool HasTiles;
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
    public ushort[,] TileMap;

    public Layer(uint raw, bool LEVstyle = false)
    {
        TileWidth = (raw & 1) == 1;
        TileHeight = (raw & 2) == 2;
        if (LEVstyle)
        {
            HasTiles = (raw & 4) == 4;
            LimitVisibleRegion = (raw & 8) == 8;
            IsTextured = (raw & 16) == 16;
        }
        else
        {
            LimitVisibleRegion = (raw & 4) == 4;
            IsTextured = (raw & 8) == 8;
            HasStars = (raw & 16) == 16;
        }
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
    public void GetOriginNumbers(int xPosition, int yPosition, ref int widthReduced, ref int heightReduced, ref int xOrigin, ref int yOrigin, ref int upperLeftX, ref int upperLeftY)
    {
        if (id == 7)
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
    public void GetFixedCornerOriginNumbers(int xPosition, int yPosition, ref int widthReduced, ref int heightReduced, ref int xOrigin, ref int yOrigin, ref int upperLeftX, ref int upperLeftY, ref byte tileSize)
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
        if (id == 7)
        {
            upperLeftX = -32;
            upperLeftY = -32;
        }
        else
        {
            upperLeftX = (int)Math.Floor(xPosition * XSpeed - widthReduced) - tileSize;
            upperLeftY = (int)(yPosition * YSpeed - ((LimitVisibleRegion && !TileHeight) ? heightReduced * 2 : heightReduced)) - tileSize;
        }
        xOrigin = -tileSize - (upperLeftX % tileSize);
        upperLeftX /= tileSize;
        yOrigin = -tileSize - (upperLeftY % tileSize);
        upperLeftY /= tileSize;
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
            for (byte i = 0; i < mod; i++) FrameList.Dequeue();
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
        for (byte i = 0; i < FrameCount; i++) FrameList.Enqueue(Sequence[i]);
        if (IsPingPong)
        {
            for (byte i = 0; i < PingPongWait; i++) FrameList.Enqueue(Sequence[FrameCount - 1]);
            for (byte i = 0; i < FrameCount; i++) FrameList.Enqueue(Sequence[FrameCount - 1 - i]);
            for (byte i = 0; i < Framewait + random; i++) FrameList.Enqueue(Sequence[0]);
        }
        else for (byte i = 0; i < Framewait + random; i++) FrameList.Enqueue(Sequence[FrameCount - 1]);
    }
    public void Advance(int frame, int random=0)
    {
        if (frame * Speed / 70 > hitherto)
        {
            if (FrameCount>0) FrameList.Dequeue();
            hitherto++;
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
    internal ushort Secure1;
    internal ushort Secure2;
    internal bool HasPassword;
    internal byte JCSFocusedLayer;
    public byte MinLight;
    public byte StartLight;
    public ushort NumberOfAnimations;
    public bool UsesVerticalSplitscreen;
    public byte LevelMode;
    public uint StreamSize;
    internal string Tileset;
    internal int MaxTiles;
    internal J2TFile J2T;
    internal string BonusLevel;
    internal string NextLevel;
    internal string SecretLevel;
    internal string Music;
    public string[] Text = new string[16];
    public Layer[] Layers = new Layer[8];
    internal byte[] unknownsection;
    internal ushort AnimOffset;
    public uint[] EventTiles;
    internal bool[] IsEachTileFlipped;
    public byte[] TileTypes;
    internal bool[] IsEachTileUsed;
    internal AnimatedTile[] Animations;
    internal uint[,] EventMap;
    internal uint[,] ParameterMap;
    internal ushort[][] Dictionary;

    public string[][] AGA_SoundPointer;
    internal byte[] AGA_unknownsection;
    internal List<String> AGA_LocalEvents; // AGA
    internal List<String> AGA_GlobalEvents;
    internal AGAEvent[,] AGA_EventMap;
    //internal byte[,][] AGA_ParameterMap;

    internal byte LEVunknown1;
    internal string[] LayerNames = new string[8] {"Foreground Layer #2", "Foreground Layer #1", "Sprite Foreground Layer", "Sprite Layer", "Background Layer #1", "Background Layer #2", "Background Layer #3", "Background Layer"};
    internal byte LEVunknown2;
    bool[] quadrantIsNontransparent = new bool[4]; //used in both saving and loading, and it's small, so what the heck.
    internal short LEVMysteriousTextShort;
    #endregion variable declaration

    private void CreateGlobalAGAEventsListIfNeedBe()
    {
        if (AGA_GlobalEvents == null)
        {
            AGA_GlobalEvents = new List<String>();
            Ini.IniFile ini = new Ini.IniFile(".\\" + "AGAEventPointerList.ini");
            for (int i = 0; i < 256; i++) AGA_GlobalEvents.Add(ini.IniReadValue("Pointers", i.ToString()).Trim());
        }
    }
    internal uint GetLayerOffset(byte layer)
    {
        uint offset = 0;
        for (byte i = 0; i < layer; i++) offset += Layers[layer].GetSize();
        return offset;
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
        if (raw < J2T.TileCount) return raw;
        else if (NumberOfAnimations >= MaxTiles - raw) return GetFrame(Animations[NumberOfAnimations - (MaxTiles - raw)].FrameList.Peek(), ref isFlipped, ref isVFlipped);
        else return 0;
    }
    public void SetPassword() { PasswordHash[0] = 0; PasswordHash[1] = 0xBA; PasswordHash[2] = 0xBE; HasPassword = false; Secure1 = Secure2 = 0; }
    public void SetPassword(string newpassword)
    {
        int inPutWord = new CRC32().GetCrc32(new MemoryStream(Encoding.ASCII.GetBytes(newpassword)));
        PasswordHash[0] = (byte)(inPutWord >> 16 & 0xff);
        PasswordHash[1] = (byte)(inPutWord >> 8 & 0xff);
        PasswordHash[2] = (byte)(inPutWord & 0xff);
        Secure1 = 0xBA00;
        Secure2 = 0xBE00;
        HasPassword = true;
    }

    int[] AGAMostValues = new int[256], AGAMostStrings = new int[256];

    public OpeningResults OpenLevel(string filename, string password=null, Dictionary<Version, string> defaultDirectories = null)
    {
        using (BinaryReader binreader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
        {
            FilenameOnly = Path.GetFileName(FullFilePath = filename);
            if (binreader.PeekChar() != 'D') // not a .LEV file
            {
                #region header
                char[] tempHeader = (binreader.PeekChar() == 32) ? binreader.ReadChars(180) : new char[0];
                char[] tempMagic = binreader.ReadChars(4);
                byte[] tempPasswordHash = binreader.ReadBytes(3);
                if (tempPasswordHash[0] != 0x00 || tempPasswordHash[1] != 0xBA || tempPasswordHash[2] != 0xBE)
                {
                    if (password == null) return OpeningResults.PasswordNeeded;
                    else
                    {
                        int inPutWord = new CRC32().GetCrc32(new MemoryStream(Encoding.ASCII.GetBytes(password)));
                        if ((inPutWord >> 16 & 0xff) != tempPasswordHash[0] || (inPutWord >> 8 & 0xff) != tempPasswordHash[1] || (inPutWord & 0xff) != tempPasswordHash[2]) return OpeningResults.WrongPassword;
                    }
                }
                Header = new string(tempHeader); Magic = new string(tempMagic); PasswordHash = tempPasswordHash;
                IsHiddenInHCL = binreader.ReadBoolean();
                Name = new string(binreader.ReadChars(32));
                VersionNumber = binreader.ReadUInt16();
                #region setup version-specific sizes
                switch (VersionNumber)
                {
                    case 514:
                        VersionType = (Header.Length == 0) ? Version.AmbiguousBCO : Version.JJ2;
                        MaxTiles = 1024;
                        break;
                    case 515:
                        VersionType = Version.TSF;
                        MaxTiles = 4096;
                        break;
                    case 256:
                        VersionType = Version.AGA;
                        MaxTiles = 4096;
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
                    BinaryWriter stream2 = new BinaryWriter(File.Open("AGA" + Path.GetFileNameWithoutExtension(filename) + "Data2.dat", FileMode.Create)); stream2.Write(UncompressedData[1].ToArray()); stream2.Close();
                    //BinaryWriter stream1 = new BinaryWriter(File.Open("AGA" + Path.GetFileNameWithoutExtension(filename) + "Data1.dat", FileMode.Create)); stream1.Write(UncompressedData[0].ToArray()); stream1.Close();
                }
                #endregion header
                #region data1
                using (BinaryReader data1reader = new BinaryReader(UncompressedData[0]))
                {
                    JCSHorizontalFocus = data1reader.ReadUInt16();
                    Secure1 = data1reader.ReadUInt16();
                    JCSVerticalFocus = data1reader.ReadUInt16();
                    Secure2 = data1reader.ReadUInt16();
                    JCSFocusedLayer = data1reader.ReadByte(); HasPassword = (JCSFocusedLayer & 240) > 0; JCSFocusedLayer &= 15;
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
                    StreamSize = data1reader.ReadUInt32();
                    Name = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    Tileset = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    if (VersionType != Version.AmbiguousBCO) J2T = new J2TFile(Path.Combine((defaultDirectories == null) ? Path.GetDirectoryName(filename) : defaultDirectories[VersionType], Tileset));
                    BonusLevel = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    NextLevel = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    SecretLevel = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    Music = new string(data1reader.ReadChars(32)).TrimEnd('\0');
                    for (byte i = 0; i < 16; i++) Text[i] = new string(data1reader.ReadChars(512)).TrimEnd('\0');
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
                    for (byte i = 0; i < 8; i++) Layers[i] = new Layer(data1reader.ReadUInt32());
                    for (byte i = 0; i < 8; i++) { Layers[i].unknown1 = data1reader.ReadByte(); Layers[i].id = i; }
                    for (byte i = 0; i < 8; i++) Layers[i].HasTiles = data1reader.ReadBoolean();
                    for (byte i = 0; i < 8; i++) Layers[i].Width = data1reader.ReadUInt32();
                    for (byte i = 0; i < 8; i++) Layers[i].RealWidth = data1reader.ReadUInt32();
                    for (byte i = 0; i < 8; i++) Layers[i].Height = data1reader.ReadUInt32();
                    for (byte i = 0; i < 8; i++) Layers[i].ZAxis = data1reader.ReadInt32();
                    for (byte i = 0; i < 8; i++) Layers[i].unknown2 = data1reader.ReadByte();
                    unknownsection = data1reader.ReadBytes(64);
                    for (byte i = 0; i < 8; i++) Layers[i].XSpeed = data1reader.ReadInt32() / 65536.0F;
                    for (byte i = 0; i < 8; i++) Layers[i].YSpeed = data1reader.ReadInt32() / 65536.0F;
                    for (byte i = 0; i < 8; i++) Layers[i].AutoXSpeed = data1reader.ReadInt32() / 65536.0F;
                    for (byte i = 0; i < 8; i++) Layers[i].AutoYSpeed = data1reader.ReadInt32() / 65536.0F;
                    for (byte i = 0; i < 8; i++) Layers[i].TextureMode = data1reader.ReadByte();
                    for (byte i = 0; i < 8; i++) { Layers[i].TexturParam1 = data1reader.ReadByte(); Layers[i].TexturParam2 = data1reader.ReadByte(); Layers[i].TexturParam3 = data1reader.ReadByte(); }
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
                #region data2
                EventMap = new uint[Layers[3].Width, Layers[3].Height];
                using (BinaryReader data2reader = new BinaryReader(UncompressedData[1]))
                {
                    //ParameterMap = new uint[Layers[3].Width, Layers[3].Height];
                    if (VersionNumber != 256) // not AGA
                    {
                        uint rlong;
                        for (uint i = 0; i < UncompressedDataLength[1] / 4; i++)
                        {
                            rlong = data2reader.ReadUInt32();
                            EventMap[i % Layers[3].Width, i / Layers[3].Width] = rlong;
                            //ParameterMap[i % Layers[3].Width, i / Layers[3].Width] = rlong >> 8;
                        }
                    }
                    else // AGA
                    {
                        CreateGlobalAGAEventsListIfNeedBe();
                        AGA_LocalEvents = new List<String>();
                        AGA_EventMap = new AGAEvent[Layers[3].Width, Layers[3].Height];
                        //AGA_ParameterMap = new byte[Layers[3].Width, Layers[3].Height][];
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
                using (BinaryReader data3reader = new BinaryReader(UncompressedData[2]))
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
                using (BinaryReader data4reader = new BinaryReader(UncompressedData[3]))
                {
                    for (byte i = 0; i < 8; i++)
                    {
                        if (Layers[i].HasTiles)
                        {
                            uint w4 = (Layers[i].Width + 3) / 4;
                            uint w = w4 * 4;
                            Layers[i].TileMap = new ushort[w, Layers[i].Height];
                            for (uint y = 0; y < Layers[i].Height; y++) for (uint x = 0; x < w4; x++)
                                {
                                    ushort nuword = data4reader.ReadUInt16();
                                    for (byte k = 0; k < 4; k++) Layers[i].TileMap[x * 4 + k, y] = Dictionary[nuword][k];
                                    if (Layers[i].TileWidth && x == w4 - 1) { data4reader.ReadBytes((int)(Layers[i].RealWidth - w) / 2); }
                                }
                        }
                        else
                        {
                            Layers[i].TileMap = new ushort[Layers[i].Width, Layers[i].Height];
                        }
                    }
                }
                #endregion data4
            }
            else // is a .LEV file
            {
                VersionType = Version.GorH;
                MaxTiles = 1024;
                int SectionOffset, SectionLength;
                Console.WriteLine(binreader.ReadChars(4)); //DDCF
                binreader.ReadBytes(4); //file length
                Console.WriteLine(binreader.ReadBytes(4)); //EDIT
                SectionLength = binreader.ReadInt32(); SectionOffset = 6;
                LEVunknown1 = binreader.ReadByte();
                J2T = new J2TFile(); J2T.VersionType = Version.GorH; J2T.MaxTiles = 1024;
                Tileset = J2T.FilenameOnly = new string(binreader.ReadChars(binreader.ReadByte())); SectionOffset += J2T.FilenameOnly.Length + 1;
                J2T.TileCount = binreader.ReadUInt32();
                J2T.data3Counter = (ushort)J2T.TileCount;
                LEVunknown2 = binreader.ReadByte();
                for (byte i = 0; i < 8; i++) { LayerNames[i] = new string(binreader.ReadChars(binreader.ReadByte())); SectionOffset += LayerNames[i].Length + 1; }
                while (binreader.PeekChar() == 0) binreader.ReadByte(); //padding
                Console.WriteLine(binreader.ReadChars(4)); //EDI2
                binreader.ReadBytes(binreader.ReadInt32()); //skip EDI2 entirely
                Console.WriteLine(binreader.ReadChars(4)); //LINF
                SectionLength = binreader.ReadInt32(); SectionOffset = 10;
                VersionNumber = binreader.ReadUInt16();
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
                J2T.VersionNumber = (ushort)binreader.ReadUInt32();
                Console.WriteLine(binreader.ReadChars(4)); //INFO
                binreader.ReadChars(8); // section length, repeat of tile count
                TileTypes = new byte[1024]; for (ushort i = 1; i < J2T.TileCount; i++) TileTypes[i] = binreader.ReadByte();
                while (binreader.PeekChar() == 0) binreader.ReadByte(); //padding
                Console.WriteLine(binreader.ReadChars(4)); //DATA
                SectionLength = (int)(binreader.BaseStream.Position + 4 + binreader.ReadUInt32()); // section length
                J2T.IsFullyOpaque = new bool[1024];
                J2T.unknown1 = new byte[1024];
                J2T.ImageAddress = new uint[1024];
                J2T.Images = new byte[J2T.TileCount][];
                J2T.unknown2 = new uint[1024];
                J2T.TransparencyMaskAddress = new uint[1024];
                J2T.TransparencyMaskOffset = new uint[1024]; for (ushort i = 0; i < J2T.TileCount; i++) J2T.ImageAddress[i] = J2T.TransparencyMaskAddress[i] = J2T.TransparencyMaskOffset[i] = i;
                J2T.unknown3 = new uint[1024];
                J2T.MaskAddress = new uint[1024];
                J2T.FlippedMaskAddress = new uint[1024];
                J2T.TransparencyMaskJCS_Style = new byte[1024][];
                J2T.TransparencyMaskJJ2_Style = new byte[1024][];
                J2T.Images[0] = J2T.TransparencyMaskJCS_Style[0] = J2T.TransparencyMaskJJ2_Style[0] = new byte[1024];
                byte rawTransBits, infoByte, elapsedRowDistance, rowTarget;
                int pixelDestination;
                for (ushort tile = 1; tile < J2T.TileCount; tile++)
                {
                    J2T.Images[tile] = new byte[1024];
                    J2T.TransparencyMaskJCS_Style[tile] = new byte[1024];
                    J2T.TransparencyMaskJJ2_Style[tile] = new byte[1024];
                    rawTransBits = binreader.ReadByte();
                    J2T.IsFullyOpaque[tile] = (rawTransBits == 240 && TileTypes[tile] == 0);
                    //if (J2T.IsFullyOpaque[tile]) Console.WriteLine(tile);
                    for (byte i = 0; i < 4; i++) quadrantIsNontransparent[i] = ((rawTransBits & (16 << i)) != 0);
                    for (byte quadrant = 0; quadrant < 4; quadrant++)
                    {
                        if (quadrantIsNontransparent[quadrant])
                        {
                            for (ushort pixel = 0; pixel < 256; pixel++)
                            {
                                pixelDestination = pixel % 16 + quadrant % 2 * 16 + pixel / 16 * 32 + quadrant / 2 * 512;
                                J2T.Images[tile][pixelDestination] = binreader.ReadByte();
                                J2T.TransparencyMaskJCS_Style[tile][pixelDestination] = J2T.TransparencyMaskJJ2_Style[tile][pixelDestination] = (byte)((TileTypes[tile] > 0 && J2T.Images[tile][pixelDestination] == 1) ? 0 : 1);
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
                                            J2T.Images[tile][pixelDestination] = binreader.ReadByte();
                                            J2T.TransparencyMaskJCS_Style[tile][pixelDestination] = J2T.TransparencyMaskJJ2_Style[tile][pixelDestination] = 1;
                                        }
                                    }
                                    else elapsedRowDistance = rowTarget;
                                }
                            }
                        }
                    }

                }
                binreader.ReadBytes(SectionLength - (int)binreader.BaseStream.Position);
                Console.WriteLine(binreader.ReadChars(4)); //EMSK
                binreader.ReadBytes(binreader.ReadInt32()); // skip EMSK for now, it's complicated

                Console.WriteLine(binreader.ReadChars(4)); //MASK
                binreader.ReadBytes(4); //section length
                ushort newTile, lessTile = 0;
                ushort oldTile = 1;
                J2T.Masks = new byte[J2T.TileCount][];
                J2T.Masks[0] = new byte[1024];
                while (true)
                {
                    newTile = binreader.ReadUInt16();
                    if (newTile == 0xFFFF)
                    {
                        for (; oldTile < J2T.TileCount; oldTile++) J2T.MaskAddress[oldTile] = lessTile;
                        break;
                    }
                    else
                    {
                        for (; oldTile < (newTile & 1023); oldTile++)
                        {
                            if (lessTile == 0) { J2T.Masks[oldTile] = J2TFile.ProduceMasklessTileByteMask(); lessTile = oldTile; }
                            J2T.MaskAddress[oldTile] = lessTile;
                        }
                        if ((newTile & 32768) == 32768)
                        {
                            J2T.MaskAddress[newTile & 1023] = binreader.ReadUInt16();
                        }
                        else
                        {
                            J2T.Masks[newTile] = J2TFile.Convert128BitsToByteMask(binreader.ReadBytes(128));
                            J2T.MaskAddress[newTile] = newTile;
                        }
                        oldTile++;
                    }
                }
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
                VersionNumber = (ushort)binreader.ReadUInt32();
                Console.WriteLine(binreader.ReadChars(4)); //INFO
                SectionLength = binreader.ReadInt32(); SectionOffset = 120;
                Layers = new Layer[8];
                for (byte i = 0; i < 8; i++)
                {
                    Layers[i] = new Layer((byte)binreader.ReadInt32(), true);
                    Layers[i].id = i;
                    Layers[i].Width = binreader.ReadUInt16();
                    if (Layers[i].TileWidth) switch (Layers[i].Width % 4)
                        {
                            case 0: Layers[i].RealWidth = Layers[i].Width; break;
                            case 2: Layers[i].RealWidth = Layers[i].Width * 2; break;
                            default: Layers[i].RealWidth = Layers[i].Width * 4; break;
                        }
                    else Layers[i].RealWidth = Layers[i].Width;
                    Layers[i].Height = binreader.ReadUInt16();
                    Layers[i].ZAxis = binreader.ReadInt16();
                    Layers[i].unknown1 = binreader.ReadByte();
                    int SpeedSettings = (byte)binreader.ReadInt32();
                    if ((SpeedSettings & 1) == 1) { Layers[i].unknown2 = binreader.ReadByte(); SectionOffset++; }
                    if ((SpeedSettings & 2) == 2) { SectionOffset += 8; Layers[i].XSpeed = binreader.ReadInt32() / 65536.0F; Layers[i].YSpeed = binreader.ReadInt32() / 65536.0F; }
                    else { Layers[i].XSpeed = Layers[i].YSpeed = 0; }
                    if ((SpeedSettings & 4) == 4) { SectionOffset += 8; Layers[i].AutoXSpeed = binreader.ReadInt32() / 65536.0F; Layers[i].AutoYSpeed = binreader.ReadInt32() / 65536.0F; }
                    else { Layers[i].AutoXSpeed = Layers[i].AutoYSpeed = 0; }
                    Layers[i].HasStars = false; Layers[i].TextureMode = Layers[i].TexturParam1 = Layers[i].TexturParam2 = Layers[i].TexturParam3 = 0;
                }
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
                for (byte i = 0; i < 8; i++) if (Layers[i].HasTiles)
                    {
                        uint w16 = (Layers[i].Width + 15) / 16;
                        uint w = w16 * 16;
                        Layers[i].TileMap = new ushort[w, Layers[i].Height];
                        for (uint y = 0; y < Layers[i].Height; y++) for (uint x = 0; x < w16; x++)
                            {
                                ushort nuword = binreader.ReadUInt16(); SectionOffset += 2;
                                for (byte k = 0; k < 16; k++) Layers[i].TileMap[x * 16 + k, y] = Dictionary[nuword][k];
                            }
                    }
                    else
                    {
                        Layers[i].TileMap = new ushort[Layers[i].Width, Layers[i].Height];
                    }
                while (binreader.PeekChar() == 0) binreader.ReadByte(); //padding
                Console.WriteLine(binreader.ReadChars(4)); //EVNT
                binreader.ReadBytes(4); //section length
                EventMap = new uint[Layers[3].Width, Layers[3].Height];
                ParameterMap = new uint[Layers[3].Width, Layers[3].Height];
                uint rlong;
                for (uint i = 0; i < EventMap.Length; i++)
                {
                    rlong = binreader.ReadUInt32();
                    EventMap[i % Layers[3].Width, i / Layers[3].Width] = rlong;
                    ParameterMap[i % Layers[3].Width, i / Layers[3].Width] = rlong >> 8;
                }
                Console.WriteLine(binreader.ReadChars(4)); //TMAP
                binreader.ReadBytes(binreader.ReadInt32()); //skip TMAP entirely
                Console.WriteLine(binreader.ReadChars(4)); //CMAP
                binreader.ReadBytes(9); //section length, 256, mystery byte
                for (ushort i = 0; i < 256; i++)
                {
                    J2T.Palette[i] = new byte[4];
                    J2T.Palette[i][0] = binreader.ReadByte();
                    J2T.Palette[i][1] = binreader.ReadByte();
                    J2T.Palette[i][2] = binreader.ReadByte();
                }
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
        LevelMode = 0;
        switch (VersionType)
        {
            case Version.AGA:
                VersionNumber = 256;
                MaxTiles = 4096;
                break;
            case Version.JJ2:
                VersionNumber = 514;
                MaxTiles = 1024;
                break;
            case Version.O:
                LevelMode = 1;
                VersionNumber = 514;
                MaxTiles = 1024;
                break;
            case Version.BC:
                LevelMode = 2;
                VersionNumber = 514;
                MaxTiles = 1024;
                break;
            case Version.GorH:
                VersionNumber = 263;
                MaxTiles = 1024;
                break;
            case Version.TSF:
                VersionNumber = 515;
                MaxTiles = 4096;
                break;
        }
        Size = 0;
        Crc32 = 0;
        JCSHorizontalFocus = Secure1 = JCSVerticalFocus = Secure2 = NumberOfAnimations = 0;
        StreamSize = 0;
        HasPassword = false;
        JCSFocusedLayer = 3;
        MinLight = StartLight = 64;
        UsesVerticalSplitscreen = false;
        Name = "Untitled";
        Tileset = "";
        J2T = null;
        BonusLevel = "";
        NextLevel = "";
        SecretLevel = "";
        Music = "";
        for (byte i = 0; i < 16; i++) Text[i] = "";
        if (VersionType == Version.AGA) //pretend support!
        {
            AGA_SoundPointer = new string[48][];
        }
        for (byte i = 0; i < 8; i++)
        {
            Layers[i] = (i == 7) ? new Layer(3) : new Layer(0);
            Layers[i].unknown1 = 0;
            Layers[i].id = i;
            Layers[i].HasTiles = i == 3;
            Layers[i].unknown2 = 0;
            Layers[i].AutoXSpeed = Layers[i].AutoYSpeed = 0;
            Layers[i].TextureMode = Layers[i].TexturParam1 = Layers[i].TexturParam2 = Layers[i].TexturParam3 = 0;
            Layers[i].ZAxis = -300 + i * 100;
        }
        Layers[0].Width = Layers[0].RealWidth = 864;
        Layers[0].Height = 216;
        Layers[0].XSpeed = Layers[0].YSpeed = 3.375F;

        Layers[1].Width = Layers[1].RealWidth = 576;
        Layers[1].Height = 144;
        Layers[1].XSpeed = Layers[1].YSpeed = 2.25F;

        Layers[2].Width = Layers[2].RealWidth = 256;
        Layers[2].Height = 64;
        Layers[2].XSpeed = Layers[2].YSpeed = 1;

        Layers[3].Width = Layers[3].RealWidth = 256;
        Layers[3].Height = 64;
        Layers[3].XSpeed = Layers[3].YSpeed = 1;

        Layers[4].Width = Layers[4].RealWidth = 171;
        Layers[4].Height = 43;
        Layers[4].XSpeed = Layers[4].YSpeed = 0.666672F;

        Layers[5].Width = Layers[5].RealWidth = 114;
        Layers[5].Height = 29;
        Layers[5].XSpeed = Layers[5].YSpeed = 0.444458F;

        Layers[6].Width = Layers[6].RealWidth = 76;
        Layers[6].Height = 19;
        Layers[6].XSpeed = Layers[6].YSpeed = 0.29631F;

        Layers[7].Width = Layers[7].RealWidth = 8;
        Layers[7].Height = 8;
        Layers[7].XSpeed = Layers[7].YSpeed = 0;

        for (byte i = 0; i < 8; i++) { Layers[i].TileMap = new ushort[Layers[i].Width, Layers[i].Height]; }

        unknownsection = new byte[64];
        AnimOffset = (ushort)MaxTiles;
        EventTiles = new uint[MaxTiles];
        IsEachTileFlipped = new bool[MaxTiles];
        TileTypes = new byte[MaxTiles];
        IsEachTileUsed = new bool[MaxTiles];
        if (VersionType == Version.AGA)
        {
            AGA_EventMap = new AGAEvent[Layers[3].Width, Layers[3].Height];
            AGA_unknownsection = new byte[32768]; //yeah, no clue
        }
        Animations = new AnimatedTile[128]; for (byte i = 0; i < 128; i++) Animations[i] = new AnimatedTile();
        EventMap = new uint[256, 64];
        //ParameterMap = new uint[256, 64];
    }
    internal bool IsAnUndefinedTile(ushort id) { return false;}// (id >= MaxTiles * 2 || (id % MaxTiles >= J2T.TileCount && MaxTiles - NumberOfAnimations > id % MaxTiles)); }
    internal ushort SanitizeTileValue(ushort id) { return (IsAnUndefinedTile(id)) ? (ushort)0 : id; }
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
            if (frame % MaxTiles < J2T.TileCount)
            {
                if (isLayer3) tilesUsed[frame % MaxTiles] = true;
                if (isFlipped || frame >= MaxTiles) tilesFlipped[frame % MaxTiles] = true;
            }
            else { isFlipped |= frame > MaxTiles; FlipMaskEvaluateAnimation(Animations[NumberOfAnimations - (MaxTiles - frame % MaxTiles)], isLayer3, ref isFlipped, ref tilesUsed, ref tilesFlipped); }
        }
    }
    //public SavingResults Save() { return Save(FullFilePath, false); }
    //public SavingResults Save(string filename) { return Save(filename, false); }
    public SavingResults Save(bool eraseUndefinedTiles = false, bool allowDifferentTilesetVersion = false, bool storeGivenFilename = true) { return Save(FullFilePath, eraseUndefinedTiles, allowDifferentTilesetVersion, storeGivenFilename); }
    public SavingResults Save(string filename, bool eraseUndefinedTiles = false, bool allowDifferentTilesetVersion = false, bool storeGivenFilename = true)
    {
        if (J2T == null)
        {
            return SavingResults.NoTilesetSelected;
        }
        if (!allowDifferentTilesetVersion && J2T.VersionType != VersionType && !((VersionType == Version.GorH) || ((VersionType == Version.BC || VersionType == Version.O) && J2T.VersionType == Version.AmbiguousBCO) || (VersionType == Version.TSF && J2T.VersionType == Version.JJ2)))
        {
            return SavingResults.TilesetIsDifferentVersion;
        }
        if (!eraseUndefinedTiles)
        {
            foreach (Layer CurrentLayer in Layers) foreach (ushort tile in CurrentLayer.TileMap) if (IsAnUndefinedTile(tile)) return SavingResults.UndefinedTiles;
            for (byte i = 0; i < NumberOfAnimations; i++) foreach (ushort tile in Animations[i].Sequence) if (IsAnUndefinedTile(tile)) return SavingResults.UndefinedTiles;
        }
        if (storeGivenFilename) FilenameOnly = Path.GetFileName(FullFilePath = filename);
        using(BinaryWriter binwriter = new BinaryWriter(File.Open(filename, FileMode.Create, FileAccess.Write))) if (VersionType == Version.GorH)
            {
                #region LEV_Save
                binwriter.Write(new char[] { 'D', 'D', 'C', 'F', '&', 's','s','f' });
            using (BinaryWriter
                EDIT = new BinaryWriter(new MemoryStream()),
                EDI2 = new BinaryWriter(new MemoryStream()),
                LINF = new BinaryWriter(new MemoryStream()),
                HSTR = new BinaryWriter(new MemoryStream()),
                TILE = new BinaryWriter(new MemoryStream()),
                TINFO = new BinaryWriter(new MemoryStream()),
                TDATA = new BinaryWriter(new MemoryStream()),
                EMSK = new BinaryWriter(new MemoryStream()),
                MASK = new BinaryWriter(new MemoryStream()),
                ANIM = new BinaryWriter(new MemoryStream()),
                FLIP = new BinaryWriter(new MemoryStream()),
                LAYR = new BinaryWriter(new MemoryStream()),
                LINFO = new BinaryWriter(new MemoryStream()),
                LDATA = new BinaryWriter(new MemoryStream()),
                EVNT = new BinaryWriter(new MemoryStream()),
                TMAP = new BinaryWriter(new MemoryStream()),
                CMAP = new BinaryWriter(new MemoryStream()))
            {
                EDIT.Write((byte)3);
                EDIT.Write(Path.GetFileNameWithoutExtension(J2T.FilenameOnly));
                EDIT.Write(J2T.TileCount);
                EDIT.Write((byte)0);
                for (byte i = 0; i < 8; i++) EDIT.Write(LayerNames[i]);

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

                TINFO.Write(J2T.TileCount);
                TINFO.Write(TileTypes, 1, (int)J2T.TileCount-1);

                byte[] tile, transtile;
                long preQuadrantOffset;
                byte columnCount;
                byte previousTransp;
                for (ushort i = 1; i < J2T.TileCount; i++)
                {
                    transtile = J2T.TransparencyMaskJCS_Style[Array.BinarySearch(J2T.TransparencyMaskOffset, 0, (int)J2T.data3Counter, J2T.TransparencyMaskAddress[i])];
                    tile = J2T.Images[J2T.ImageAddress[i]];
                    for (byte quadrant = 0; quadrant < 4; quadrant++)
                    {
                        quadrantIsNontransparent[quadrant] = true;
                        if (TileTypes[i] == 0) for (ushort pixel = 0; pixel < 256; pixel++) if (transtile[pixel % 16 + pixel / 16 * 32 + quadrant%2*16 + quadrant/2*512] == 0) { quadrantIsNontransparent[quadrant] = false; break; }
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
                                                    for (byte p = 0; p < columnCount; p++) TDATA.Write(tile[row * 32 + quadrant / 2 * 512 + column-columnCount+p + quadrant % 2 * 16]);
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

                bool[] shouldBeOpaque = new bool[8];
                for (ushort i = 0; i < J2T.TileCount; i++)
                {
                    transtile = J2T.TransparencyMaskJJ2_Style[Array.BinarySearch(J2T.TransparencyMaskOffset, 0, (int)J2T.data3Counter, J2T.TransparencyMaskAddress[i])];
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

                DiscoverTilesThatAreFlippedAndOrUsedInLayer3();
                ushort[] firstMaskInstance = new ushort[J2T.TileCount];
                uint maskAddress;
                for (ushort i = 1; i < J2T.TileCount; i++) if (IsEachTileUsed[i])
                {
                    maskAddress = J2T.MaskAddress[i];
                    if (firstMaskInstance[maskAddress] == 0)
                    {
                        firstMaskInstance[maskAddress] = i;
                        MASK.Write(i);
                        for (ushort j = 0; j < 1024; j+=8)
                        {
                            MASK.Write((byte)(
                                (J2T.Masks[maskAddress][j + 0] == 1 ? 1 : 0) |
                                (J2T.Masks[maskAddress][j + 1] == 1 ? 2 : 0) |
                                (J2T.Masks[maskAddress][j + 2] == 1 ? 4 : 0) |
                                (J2T.Masks[maskAddress][j + 3] == 1 ? 8 : 0) |
                                (J2T.Masks[maskAddress][j + 4] == 1 ? 16 : 0) |
                                (J2T.Masks[maskAddress][j + 5] == 1 ? 32 : 0) |
                                (J2T.Masks[maskAddress][j + 6] == 1 ? 64 : 0) |
                                (J2T.Masks[maskAddress][j + 7] == 1 ? 128 : 0)
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

                for (ushort i = 1; i < J2T.TileCount; i++) if (IsEachTileFlipped[i]) FLIP.Write(i);
                FLIP.Write((ushort)0xFFFF);

                LAYR.Write(263);

                foreach (Layer CurrentLayer in Layers)
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
                
                using (BinaryWriter data3writer = new BinaryWriter(new MemoryStream()))
                using (BinaryWriter data4writer = new BinaryWriter(new MemoryStream()))
                {
                    List<ushort[]> attestedWords = new List<ushort[]>(2048);
                    attestedWords.Add(new ushort[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                    data3writer.Write(new byte[32] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                    ushort[] tentativeWord = new ushort[16];
                    int tentativeIndex;
                    foreach (Layer CurrentLayer in Layers) if (CurrentLayer.HasTiles) for (ushort y = 0; y < CurrentLayer.Height; y++) for (ushort x = 0; x < (CurrentLayer.Width + 15) / 16 * 16; x += 16)
                                {
                                    for (byte j = 0; j < 16; j++) tentativeWord[j] = (x + j < CurrentLayer.Width) ? SanitizeTileValue(CurrentLayer.TileMap[x + j, y]) : (ushort)0;
                                    if (CurrentLayer.id == 3 && //.LEV files may not care about this sort of thing?
                                        (
                                        (tentativeWord[0] % MaxTiles > J2T.TileCount && EventMap[x, y] != 0) ||
                                        (tentativeWord[1] % MaxTiles > J2T.TileCount && EventMap[x + 1, y] != 0) ||
                                        (tentativeWord[2] % MaxTiles > J2T.TileCount && EventMap[x + 2, y] != 0) ||
                                        (tentativeWord[3] % MaxTiles > J2T.TileCount && EventMap[x + 3, y] != 0) ||
                                        (tentativeWord[4] % MaxTiles > J2T.TileCount && EventMap[x + 4, y] != 0) ||
                                        (tentativeWord[5] % MaxTiles > J2T.TileCount && EventMap[x + 5, y] != 0) ||
                                        (tentativeWord[6] % MaxTiles > J2T.TileCount && EventMap[x + 6, y] != 0) ||
                                        (tentativeWord[7] % MaxTiles > J2T.TileCount && EventMap[x + 7, y] != 0) ||
                                        (tentativeWord[8] % MaxTiles > J2T.TileCount && EventMap[x + 8, y] != 0) ||
                                        (tentativeWord[9] % MaxTiles > J2T.TileCount && EventMap[x + 9, y] != 0) ||
                                        (tentativeWord[10] % MaxTiles > J2T.TileCount && EventMap[x + 10, y] != 0) ||
                                        (tentativeWord[11] % MaxTiles > J2T.TileCount && EventMap[x + 11, y] != 0) ||
                                        (tentativeWord[12] % MaxTiles > J2T.TileCount && EventMap[x + 12, y] != 0) ||
                                        (tentativeWord[13] % MaxTiles > J2T.TileCount && EventMap[x + 13, y] != 0) ||
                                        (tentativeWord[14] % MaxTiles > J2T.TileCount && EventMap[x + 14, y] != 0) ||
                                        (tentativeWord[15] % MaxTiles > J2T.TileCount && EventMap[x + 15, y] != 0)
                                        )
                                        ) { tentativeIndex = -1; }
                                    else tentativeIndex = attestedWords.FindIndex(delegate(ushort[] current)
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
                if (Layers[7].IsTextured) {
                    TMAP.Write(1);
                    TMAP.Write(7);
                    TMAP.Write(0);
                    for (uint i = 0; i < 65536; i++)
                    {
                        TMAP.Write(J2T.Images[J2T.ImageAddress[Layers[7].TileMap[i % 256/32, i / 8192]]][i%32+i%8192/256*32]);
                    }
                }
                else { TMAP.Write(0); TMAP.Write((ushort)0); }

                CMAP.Write(256);
                CMAP.Write((byte)1);
                for (ushort i = 0; i < 256; i++) {CMAP.Write(J2T.Palette[i][0]); CMAP.Write(J2T.Palette[i][1]); CMAP.Write(J2T.Palette[i][2]);}

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
        else
        {
            binwriter.Write(Header.ToCharArray()); // In Jazz 2 levels, the copyright notice. Otherwise a blank string.
            binwriter.Write(Magic.ToCharArray()); // 'LEVL'
            binwriter.Write(PasswordHash); // The password hash is calculated in SetPassword(), not here
            binwriter.Write(IsHiddenInHCL);
            binwriter.Write(Name.PadRight(32, '\0').ToCharArray());
            binwriter.Write((ushort)((VersionType == Version.AGA) ? 256 : (VersionType==Version.TSF) ? 515 : 514));
            binwriter.Write(new byte[40]); // To be filled in later with filesize, CRC32, and the compressed and uncompressed data lengths, for a total of 10 longs or 40 bytes.
            CRC32 CRCCalculator = new CRC32();
            for (byte i = 0; i < 4; i++) CompressedData[i] = new MemoryStream();
            using (BinaryWriter data1writer = new BinaryWriter(CompressedData[0])) //since Data3 and Data4 are written simultaneously, they use separate BinaryWriters. Data1 and Data2 get their own just for symmetry.
            using (BinaryWriter data2writer = new BinaryWriter(CompressedData[1]))
            using (BinaryWriter data3writer = new BinaryWriter(CompressedData[2]))
            using (BinaryWriter data4writer = new BinaryWriter(CompressedData[3]))
            {
                #region data1
                data1writer.Write(JCSHorizontalFocus);
                data1writer.Write(Secure1);
                data1writer.Write(JCSVerticalFocus);
                data1writer.Write(Secure2);
                data1writer.Write((byte)(JCSFocusedLayer | (HasPassword ? 240 : 0))); // SetPassword()
                data1writer.Write(MinLight);
                data1writer.Write(StartLight);
                data1writer.Write(NumberOfAnimations);
                data1writer.Write(UsesVerticalSplitscreen);
                if (VersionType == Version.BC) data1writer.Write((byte)2);
                else if (VersionType == Version.O) data1writer.Write((byte)1);
                else data1writer.Write(LevelMode);
                data1writer.Write(StreamSize); // this gets replaced later with an actual calculation
                data1writer.Write(Name.PadRight(32, '\0').ToCharArray());
                data1writer.Write(Tileset.PadRight(32, '\0').ToCharArray());
                data1writer.Write(BonusLevel.PadRight(32, '\0').ToCharArray());
                data1writer.Write(NextLevel.PadRight(32, '\0').ToCharArray());
                data1writer.Write(SecretLevel.PadRight(32, '\0').ToCharArray());
                data1writer.Write(Music.PadRight(32, '\0').ToCharArray());
                for (byte i = 0; i < 16; i++) data1writer.Write(Text[i].PadRight(512, '\0').ToCharArray());
                if (VersionType == Version.AGA) for (byte i = 0; i < 48; i++)
                    {
                        if (AGA_SoundPointer[i] == null) for (byte j = 0; j < 16; j++) data1writer.Write(0); //16 longs = 64 bytes
                        else data1writer.Write((AGA_SoundPointer[i][0] + "\\" + AGA_SoundPointer[i][1]).PadRight(64, '\0').ToCharArray());
                    }
                for (byte i = 0; i < 8; i++) data1writer.Write((Layers[i].TileWidth?1:0) + (Layers[i].TileHeight?2:0) + (Layers[i].LimitVisibleRegion?4:0) + (Layers[i].IsTextured?8:0) + (Layers[i].HasStars?16:0));
                for (byte i = 0; i < 8; i++) data1writer.Write(Layers[i].unknown1);
                for (byte i = 0; i < 8; i++) data1writer.Write(Layers[i].HasTiles);
                for (byte i = 0; i < 8; i++) data1writer.Write(Layers[i].Width);
                for (byte i = 0; i < 8; i++) data1writer.Write(Layers[i].RealWidth);
                for (byte i = 0; i < 8; i++) data1writer.Write(Layers[i].Height);
                for (byte i = 0; i < 8; i++) data1writer.Write(Layers[i].ZAxis);
                for (byte i = 0; i < 8; i++) data1writer.Write(Layers[i].unknown2);
                data1writer.Write(unknownsection);
                for (byte i = 0; i < 8; i++) data1writer.Write((int)(Layers[i].XSpeed * 65536));
                for (byte i = 0; i < 8; i++) data1writer.Write((int)(Layers[i].YSpeed * 65536));
                for (byte i = 0; i < 8; i++) data1writer.Write((int)(Layers[i].AutoXSpeed * 65536));
                for (byte i = 0; i < 8; i++) data1writer.Write((int)(Layers[i].AutoYSpeed * 65536));
                for (byte i = 0; i < 8; i++) data1writer.Write(Layers[i].TextureMode);
                for (byte i = 0; i < 8; i++) { data1writer.Write(Layers[i].TexturParam1); data1writer.Write(Layers[i].TexturParam2); data1writer.Write(Layers[i].TexturParam3); }
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
                data1writer.Seek(15,SeekOrigin.Begin); //go to the StreamSize long
                data1writer.Write((int)CompressedData[0].Length); //and replace it with the actual length of the section
                #endregion data1
                #region data2
                if (VersionType == Version.AGA) //could use some commenting
                {
                    CreateGlobalAGAEventsListIfNeedBe();
                    AGA_LocalEvents = new List<String>();
                    foreach (AGAEvent saveProspectiveEvent in AGA_EventMap)
                    {
                        if (saveProspectiveEvent.ID != 0 && !AGA_LocalEvents.Contains(AGA_GlobalEvents[(int)saveProspectiveEvent.ID])) AGA_LocalEvents.Add(AGA_GlobalEvents[(int)saveProspectiveEvent.ID]);
                    }
                    data2writer.Write((ushort)AGA_LocalEvents.Count);
                    for (ushort i = 0; i < AGA_LocalEvents.Count; i++) data2writer.Write(AGA_LocalEvents[i].PadRight(64, '\0').ToCharArray());
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
                            (AGA_EventMap[x,y].HasParameters() ? (uint)0x80000000 : 0)
                            );
                        if (AGA_EventMap[x, y].HasParameters())
                        {
                            data2writer.Write(AGA_EventMap[x, y].GetNumberOfParameters() * 4 + 8 + AGA_EventMap[x, y].GetNumberOfBytesItWillTakeToWriteStrings());
                            data2writer.Write((ushort)(AGA_EventMap[x, y].GetNumberOfBytesItWillTakeToWriteStrings() == 0 ? 0 : 2));
                            data2writer.Write((ushort)(AGA_EventMap[x, y].GetNumberOfParameters() / 2));
                            for (byte i = 0; i < AGA_EventMap[x, y].GetNumberOfParameters(); i++) data2writer.Write(AGA_EventMap[x, y].Longs[i]);
                            if ((AGA_EventMap[x, y].Strings[0] ?? "") != "" || (AGA_EventMap[x, y].Strings[1] ?? "") != "" || (AGA_EventMap[x, y].Strings[2] ?? "") != "") //needs to be updated in light of the reflection that some events use more than three strings
                            {
                                data2writer.Write(AGA_EventMap[x, y].Strings[0].Length+1);
                                data2writer.Write(AGA_EventMap[x, y].Strings[0].ToCharArray());
                                data2writer.Write((byte)0);
                                if ((AGA_EventMap[x, y].Strings[1] ?? "") != "" || (AGA_EventMap[x, y].Strings[2] ?? "") != "")
                                {
                                    data2writer.Write(AGA_EventMap[x, y].Strings[1].Length+1);
                                    data2writer.Write(AGA_EventMap[x, y].Strings[1].ToCharArray());
                                    data2writer.Write((byte)0);
                                    if ((AGA_EventMap[x, y].Strings[2] ?? "") != "")
                                    {
                                        data2writer.Write(AGA_EventMap[x, y].Strings[2].Length+1);
                                        data2writer.Write(AGA_EventMap[x, y].Strings[2].ToCharArray());
                                        data2writer.Write((byte)0);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (uint i = 0; i < EventMap.Length; i++) data2writer.Write(EventMap[i % EventMap.GetLength(0), i / EventMap.GetLength(0)]);
                    /*{
                        //data2writer.Write(ParameterMap[i % EventMap.GetLength(0), i / EventMap.GetLength(0)]<<8);
                        //data2writer.Seek(-4, SeekOrigin.Current);
                        data2writer.Write(EventMap[i % EventMap.GetLength(0), i / EventMap.GetLength(0)]);
                        //data2writer.Seek(3, SeekOrigin.Current);
                    }*/
                }
                #endregion data2
                #region data3 and data4
                List<ushort[]> attestedWords = new List<ushort[]>(2048);
                attestedWords.Add(new ushort[4] { 0, 0, 0, 0 });
                data3writer.Write(new byte[8] { 0, 0, 0, 0,0,0,0,0 });
                ushort[] tentativeWord = new ushort[4];
                int tentativeIndex;
                foreach (Layer CurrentLayer in Layers) if (CurrentLayer.HasTiles) for (ushort y = 0; y < CurrentLayer.Height; y++) for (ushort x = 0; x < (CurrentLayer.RealWidth + 3) / 4 * 4; x += 4) // remember to write by RealWidth (rounded up), not Width
                            {
                                if (CurrentLayer.TileWidth) for (byte j = 0; j < 4; j++) tentativeWord[j] = SanitizeTileValue(CurrentLayer.TileMap[(x + j) % CurrentLayer.Width, y]);
                                else for (byte j = 0; j < 4; j++) tentativeWord[j] = (x + j < CurrentLayer.Width) ? SanitizeTileValue(CurrentLayer.TileMap[x + j, y]) : (ushort)0;
                                if (CurrentLayer.id == 3 &&
                                    (
                                    (tentativeWord[0] % MaxTiles > J2T.TileCount && EventMap[x, y] != 0) ||
                                    (tentativeWord[1] % MaxTiles > J2T.TileCount && EventMap[x + 1, y] != 0) ||
                                    (tentativeWord[2] % MaxTiles > J2T.TileCount && EventMap[x + 2, y] != 0) ||
                                    (tentativeWord[3] % MaxTiles > J2T.TileCount && EventMap[x + 3, y] != 0)
                                    )
                                    ) { tentativeIndex = -1; }
                                else tentativeIndex = attestedWords.FindIndex(delegate(ushort[] current) { return tentativeWord[0] == current[0] && tentativeWord[1] == current[1] && tentativeWord[2] == current[2] && tentativeWord[3] == current[3]; });
                                if (tentativeIndex == -1) // either it actually couldn't be found, or there were events on an animated tile
                                {
                                    tentativeIndex = attestedWords.Count;
                                    attestedWords.Add(new ushort[4]);
                                    tentativeWord.CopyTo(attestedWords[tentativeIndex], 0);
                                    for (byte j = 0; j < 4; j++) data3writer.Write(tentativeWord[j]);
                                }
                                data4writer.Write((ushort)(tentativeIndex));
                            }
                #endregion data3 and data4
                for (byte i = 0; i < 4; i++)
                {
                    UncompressedDataLength[i] = (int)CompressedData[i].Length;
                    var zcomparray = ZlibStream.CompressBuffer(CompressedData[i].ToArray());
                    binwriter.Write(zcomparray);
                    CompressedDataLength[i] = zcomparray.Length;
                    CRCCalculator.SlurpBlock(zcomparray, 0, zcomparray.Length);
                }
            }
            binwriter.Seek(Header.Length + 42,0);
            binwriter.Write((int)(binwriter.BaseStream.Length));
            binwriter.Write((int)CRCCalculator.Crc32Result); Crc32 = CRCCalculator.Crc32Result;
            for (byte i = 0; i < 4; i++)
            {
                binwriter.Write((int)CompressedDataLength[i]);
                binwriter.Write((int)UncompressedDataLength[i]);
            }
        }
        return SavingResults.Success;
    }
    private void DiscoverTilesThatAreFlippedAndOrUsedInLayer3()
    {
        for (ushort i = 0; i < MaxTiles; i++) IsEachTileUsed[i] = IsEachTileFlipped[i] = false;
        bool isFlipped;
        foreach (Layer CurrentLayer in Layers)
        {
            CurrentLayer.HasTiles = CurrentLayer.id == 3;
            foreach (ushort tileUsed in CurrentLayer.TileMap)
            {
                isFlipped = false;
                if (tileUsed != 0) CurrentLayer.HasTiles = true;
                if (tileUsed % MaxTiles < J2T.TileCount)
                {
                    if (CurrentLayer.id == 3) IsEachTileUsed[tileUsed % MaxTiles] = true; // ignore this; it's only useful for .LEV saving
                    if (tileUsed >= MaxTiles) IsEachTileFlipped[tileUsed % MaxTiles] = true;
                }
                else { isFlipped = tileUsed > MaxTiles; FlipMaskEvaluateAnimation(Animations[NumberOfAnimations - (MaxTiles - tileUsed % MaxTiles)], CurrentLayer.id == 3, ref isFlipped, ref IsEachTileUsed, ref IsEachTileFlipped); }
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
        foreach (Layer CurrentLayer in Layers) if (CurrentLayer.HasTiles) for (ushort x = 0; x < CurrentLayer.Width; x++) for (ushort y = 0; y < CurrentLayer.Height; y++) IncreaseAnimationInstanceIfNeeded(ref CurrentLayer.TileMap[x, y], ref threshhold);
        for (byte i = 0; i < NumberOfAnimations; i++) for (byte j = 0; j < Animations[i].FrameCount; j++) IncreaseAnimationInstanceIfNeeded(ref Animations[i].Sequence[j], ref threshhold);
        AnimOffset++;
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
        if (NumberOfAnimations == 128 || NumberOfAnimations + J2T.TileCount + 1 == MaxTiles) return InsertFrameResults.Full;
        if (location > NumberOfAnimations) location = (byte)NumberOfAnimations;
        for (ushort i = NumberOfAnimations; i > location; ) Animations[i] = Animations[--i];
        Animations[location] = nuAnim;
        NumberOfAnimations++;
        int threshhold = AnimOffset + location;
        foreach (Layer CurrentLayer in Layers) if (CurrentLayer.HasTiles) for (ushort x = 0; x < CurrentLayer.Width; x++) for (ushort y = 0; y < CurrentLayer.Height; y++) DecreaseAnimationInstanceIfNeeded(ref CurrentLayer.TileMap[x, y], ref threshhold);
        for (byte i = 0; i < NumberOfAnimations; i++) for (byte j = 0; j < Animations[i].FrameCount; j++) DecreaseAnimationInstanceIfNeeded(ref Animations[i].Sequence[j], ref threshhold);
        AnimOffset--;
        return InsertFrameResults.Success;
    }
    private void DecreaseAnimationInstanceIfNeeded(ref ushort tile, ref int threshhold)
    {
        var nutile = tile % MaxTiles;
        if (nutile >= AnimOffset && nutile < threshhold) tile--;
    }

    public VersionChangeResults ChangeVersion(Version nuVersion)
    {
        uint J2TTileCount = (J2T == null) ? 1 : J2T.TileCount;
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
                            VersionNumber = 514;
                        }
                        if (MaxTiles == 4096)
                        {
                            MaxTiles = 1024;
                            for (byte i = 0; i < NumberOfAnimations; i++) Animations[i].ChangeVersion(ref nuVersion,ref J2TTileCount, ref NumberOfAnimations);
                            foreach (Layer CurrentLayer in Layers) if (CurrentLayer.HasTiles) for (ushort x = 0; x < CurrentLayer.Width; x++) for (ushort y = 0; y < CurrentLayer.Height; y++)
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
                        }
                        VersionType = nuVersion;
                        return VersionChangeResults.Success;
                    }
                case Version.AGA:
                case Version.TSF:
                    VersionNumber = (ushort)((nuVersion == Version.AGA) ? 256 : 515);
                    Header = (nuVersion == Version.AGA) ? "" : StandardHeader;
                    if (MaxTiles == 1024)
                    {
                        MaxTiles = 4096;
                        for (byte i = 0; i < NumberOfAnimations; i++) Animations[i].ChangeVersion(ref nuVersion, ref J2TTileCount, ref NumberOfAnimations);
                        foreach (Layer CurrentLayer in Layers) if (CurrentLayer.HasTiles) for (ushort x = 0; x < CurrentLayer.Width; x++) for (ushort y = 0; y < CurrentLayer.Height; y++)
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
                    }
                    if (nuVersion == Version.AGA) //pretend support!
                    {
                        AGA_SoundPointer = new string[48][];
                        AGA_unknownsection = new byte[32768];
                        CreateGlobalAGAEventsListIfNeedBe();
                        AGA_LocalEvents = new List<String>();
                        AGA_EventMap = new AGAEvent[Layers[3].Width, Layers[3].Height];
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
        if (avoidRedundancy && Path.GetFileName(filename) == Tileset) return VersionChangeResults.Success;
        J2TFile tryout = new J2TFile(Path.Combine((defaultDirectories == null) ? Path.GetDirectoryName(filename) : defaultDirectories[VersionType], filename));         
        //J2TFile tryout = new J2TFile(filename);
        if (VersionType == tryout.VersionType || (tryout.VersionType == Version.AmbiguousBCO && (VersionType == Version.BC || VersionType == Version.O)) || (VersionType == Version.GorH && tryout.TileCount <= 1020) || (VersionType == Version.TSF && tryout.VersionType == Version.JJ2))
        {
            if (tryout.TileCount + NumberOfAnimations < MaxTiles)
            {
                J2T = tryout;
                Tileset = Path.GetFileName(filename);
                return VersionChangeResults.Success;
            }
            else return VersionChangeResults.TooManyAnimatedTiles;
        }
        else return VersionChangeResults.UnsupportedConversion;
    }

}
