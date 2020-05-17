using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLLE
{
    //based on http://www.pinvoke.net/default.aspx/Structures/BITMAPINFOHEADER.html
    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;

        public BITMAPINFOHEADER(Size bitmapSize)
        {
            biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
            biWidth = bitmapSize.Width;
            biHeight = bitmapSize.Height;
            biPlanes = 1;
            biBitCount = 8;
            biCompression = 0;
            biSizeImage = biWidth * biHeight;
            biXPelsPerMeter = biYPelsPerMeter = 0x0ED4; //doesn't really matter
            biClrUsed = 256;
            biClrImportant = 256;
        }

        //confirm that this is a proper DIB header we got from the clipboard
        public bool Matches(Size intendedSize)
        {
            if (intendedSize != null)
            {
                if (biWidth != intendedSize.Width || biHeight != intendedSize.Height)
                    return false;
            }
            if (biSizeImage != biWidth * biHeight) //don't support negative biHeight values... they're easier to handle but seemingly less common.
                return false;
            if (biPlanes != 1 || biBitCount != 8 || biCompression != 0)
                return false;
            if (biClrUsed != 256 && biClrUsed != 0)
                return false;
            return true;
        }
    }

    class BitmapStuff
    {
        //from https://stackoverflow.com/questions/2384/read-binary-file-into-a-struct... supposedly this isn't as fast as reading each property individually, but at this scale it doesn't matter
        private static T ReadStruct<T>(System.IO.BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }
        private static void WriteStruct<T>(T structToWrite, System.IO.BinaryWriter writer)
        {
            byte[] buff = new byte[Marshal.SizeOf(typeof(T))];//Create Buffer
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);//Hands off GC
                                                                        //Marshal the structure
            Marshal.StructureToPtr(structToWrite, handle.AddrOfPinnedObject(), false);
            writer.Write(buff);
            handle.Free();
        }

        static public void ByteArrayToBitmap(byte[] byteArray, Bitmap bitmap, bool flipV = false)
        {
            if (byteArray.Length == bitmap.Width * bitmap.Height)
            {
                if (flipV)
                {
                    byteArray = Enumerable.Range(0, 32).Select(y => byteArray.Skip((31-y) * 32).Take(32)).SelectMany(r => r).ToArray();
                }
                var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                Marshal.Copy(byteArray, 0, data.Scan0, byteArray.Length);
                bitmap.UnlockBits(data);
            }
        }
        static public byte[] BitmapToByteArray(Bitmap bitmap)
        {
            byte[] byteArray = new byte[bitmap.Width * bitmap.Height];
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            Marshal.Copy(data.Scan0, byteArray, 0, byteArray.Length);
            bitmap.UnlockBits(data);
            return byteArray;
        }

        static public bool ClipboardHasBitmap()
        {
            return Clipboard.ContainsData(DataFormats.Dib);
        }
        static public Bitmap GetBitmapFromClipboard(Size intendedSize)
        {
            if (ClipboardHasBitmap())
                using (var memStream = Clipboard.GetData(DataFormats.Dib) as System.IO.MemoryStream)
                    if (memStream.Length > Marshal.SizeOf(typeof(BITMAPINFOHEADER)) + Palette.PaletteSize * 4)
                        using (var reader = new System.IO.BinaryReader(memStream, J2File.FileEncoding, true))
                        {
                            BITMAPINFOHEADER header = ReadStruct<BITMAPINFOHEADER>(reader);
                            if (header.Matches(intendedSize))
                            {
                                Bitmap result = new Bitmap(header.biWidth, header.biHeight, PixelFormat.Format8bppIndexed);

                                var palette = result.Palette;
                                var entries = palette.Entries;
                                for (uint i = 0; i < Palette.PaletteSize; ++i)
                                {
                                    byte[] BGRA = reader.ReadBytes(4);
                                    entries[i] = Color.FromArgb(byte.MaxValue, BGRA[2], BGRA[1], BGRA[0]);
                                }
                                result.Palette = palette;
                                ByteArrayToBitmap(reader.ReadBytes(header.biSizeImage), result, true);
                                return result;
                            }
                        }
            //else
            return null;
        }
        static public void CopyBitmapToClipboard(Bitmap bitmap)
        {
            using (System.IO.MemoryStream memStream = new System.IO.MemoryStream())
            {
                using (var writer = new System.IO.BinaryWriter(memStream, J2File.FileEncoding, true))
                {
                    WriteStruct(new BITMAPINFOHEADER(bitmap.Size), writer);
                    for (uint i = 0; i < Palette.PaletteSize; ++i) //reverse color order
                    {
                        var bytesToRearrange = bitmap.Palette.Entries[i];
                        writer.Write(bytesToRearrange.B);
                        writer.Write(bytesToRearrange.G);
                        writer.Write(bytesToRearrange.R);
                        writer.Write(byte.MaxValue);
                    }
                    writer.Write(BitmapToByteArray(bitmap));
                }

                memStream.Seek(0, System.IO.SeekOrigin.Begin);
                var dob = new DataObject();
                dob.SetData(DataFormats.Dib, false, memStream);
                Clipboard.SetDataObject(dob, true);
            }
        }
    }
}
