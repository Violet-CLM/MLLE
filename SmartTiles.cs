using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace MLLE
{
    class SmartTile
    {
        ushort TileID;
        class Rule
        {
            List<ushort> TileIDs = new List<ushort>();
            class Condition
            {
                int X, Y;
                bool Not;
                List<ushort> TileIDs = new List<ushort>();
            }
            List<Condition> Conditions = new List<Condition>();
        }
        List<Rule> Rules = new List<Rule>();

        static ushort[] CreateTileIDList(IEnumerable<string> words, Dictionary<string, ushort[]> groups)
        {
            List<ushort> list = new List<ushort>();
            foreach (var potentialid in words)
            {
                ushort id;
                ushort[] ids;
                if (ushort.TryParse(potentialid, out id))
                    list.Add(id);
                else if (groups.TryGetValue(potentialid, out ids))
                    list.AddRange(ids);
                else
                    Debug.WriteLine("Error with label " + potentialid);
            }
            return list.ToArray();
        }
        public static bool DefineSmartTiles(string filepath, out SmartTile[] smartTiles)
        {
            List<SmartTile> result = new List<SmartTile>();
            Dictionary<string, ushort[]> tileGroups = new Dictionary<string, ushort[]>();
            bool stillInDictionaryMode = true;

            var lines = File.ReadAllLines(filepath);
            foreach (string line in lines)
            {
                if (line == String.Empty) //section break
                {
                    stillInDictionaryMode = false;
                }
                else
                {
                    var words = line.Split(' ');
                    if (stillInDictionaryMode)
                    {
                        tileGroups.Add(words[0], CreateTileIDList(words.Skip(1), tileGroups));
                        Debug.WriteLine(words[0]);
                        Debug.WriteLine(tileGroups[words[0]].Length);
                    }
                    else
                    {
                    }
                }
            }
            smartTiles = result.ToArray();
            return true;
        }
    }
    partial class Mainframe
    {
        SmartTile[] SmartTiles = new SmartTile[0];
        private bool CheckForSmartTileFile()
        {
            SmartTiles = new SmartTile[0];
            if (!J2L.HasTiles)
                return false;
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLLESet - " + Path.GetFileNameWithoutExtension(J2L.MainTilesetFilename) + ".txt");
            if (File.Exists(filepath))
                return SmartTile.DefineSmartTiles(filepath, out SmartTiles);
            return false;
        }
    }
}
