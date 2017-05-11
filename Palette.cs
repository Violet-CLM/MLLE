using System.IO;
using System.Linq;

internal class Palette
{
    public const uint PaletteSize = 256;
    public byte[][] Colors = new byte[PaletteSize][];

    public Palette() { }
    public Palette(BinaryReader binreader, bool LEVformat = false)
    {
        for (uint i = 0; i < PaletteSize; ++i)
        {
            var color = Colors[i] = new byte[4];
            color[0] = binreader.ReadByte();
            color[1] = binreader.ReadByte();
            color[2] = binreader.ReadByte();
            if (!LEVformat)
                color[3] = binreader.ReadByte();
        }
    }

    internal void CopyFrom(Palette other)
    {
        if (other != null)
        {
            for (int i = 0; i < PaletteSize; ++i)
                Colors[i] = other.Colors[i].Clone() as byte[];
        }
    }

    public void WriteLEVStyle(BinaryWriter binwriter)
    {
        foreach (byte[] color in Colors)
        {
            binwriter.Write(color[0]);
            binwriter.Write(color[1]);
            binwriter.Write(color[2]);
        }
    }

    public byte[] this[int key]
    {
        get
        {
            return Colors[key];
        }
        set
        {
            Colors[key] = value;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        else
        {
            Palette other = obj as Palette;
            for (int i = 0; i < PaletteSize; ++i)
                if (!Colors[i].SequenceEqual(other.Colors[i]))
                    return false;
            return true;
        }
    }
}
