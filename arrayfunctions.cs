using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    static class ArrayFunctions
    {
        public static void TwoDSlice(ref ushort[][] data, ref ushort[][] results, int xs, int ys, ushort deflt=0)
        {
            for (int y = 0; y < results.Length; y++)
            {
                for (int x = 0; x < results[0].Length; x++)
                {
                    results[y][x] = ((ys+y<=data.Length) && (xs+x <= data[0].Length)) ? data[ys + y][xs + x] : deflt;
                }
            }
        }
        public static void TwoDPrint(ushort[][] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                foreach (ushort j in data[i])
                {
                    Console.Write(j);
                }
                Console.WriteLine();
            }
        }
    }
