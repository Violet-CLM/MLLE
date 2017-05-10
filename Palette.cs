using System.IO;

namespace MLLE
{
    class Palette
    {
        const uint PaletteSize = 256;
        public byte[][] Colors = new byte[PaletteSize][];

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
    }
}
