using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Ionic.Zlib;

public class VOres
{
    #region variables
    internal string Filename;
    public string[] SFXnames;
    #endregion variables
    public VOres(string filename)
    {
        Filename = filename;
        using (BinaryReader binreader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
        {
            binreader.ReadBytes(20);
            int numberOfSounds = binreader.ReadInt32();
            SFXnames = new string[numberOfSounds];
            binreader.ReadBytes(5);
            int cLength = binreader.ReadInt32();
            int uLength = binreader.ReadInt32();
            binreader.ReadBytes(32);
            BinaryReader data1reader = new BinaryReader(new MemoryStream(ZlibStream.UncompressBuffer(binreader.ReadBytes(cLength))));
            for (int i = 0; i < numberOfSounds; i++)
            {
                SFXnames[i] = new string(data1reader.ReadChars(64)).TrimEnd();
            }
        }
    }
}
