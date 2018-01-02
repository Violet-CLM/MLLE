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
    }
    partial class Mainframe
    {
        List<SmartTile> SmartTiles = new List<SmartTile>();
        private bool CheckForSmartTileFile()
        {
            SmartTiles.Clear();
            if (!J2L.HasTiles)
                return false;
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLLESet - " + Path.GetFileNameWithoutExtension(J2L.MainTilesetFilename) + ".txt");
            Debug.WriteLine(filepath);
            if (File.Exists(filepath))
            {
                Dictionary<string, ushort[]> tileGroups = new Dictionary<string, ushort[]>();
                bool stillInDictionaryMode = true;

                var lines = File.ReadAllLines(filepath);
                foreach (string line in lines) {
                    if (line == String.Empty) //section break
                    {
                        stillInDictionaryMode = false;
                    }
                    else
                    {
                        var words = line.Split(' ');
                        if (stillInDictionaryMode)
                        {
                            List<ushort> ids = new List<ushort>();
                            foreach (var potentialid in words.Skip(1))
                            {
                                ushort id;
                                if (ushort.TryParse(potentialid, out id))
                                    ids.Add(id);
                                else
                                    ids.AddRange(tileGroups[potentialid]);
                            }
                            tileGroups.Add(words[0], ids.ToArray());
                            Debug.WriteLine(words[0]);
                            Debug.WriteLine(ids.Count);
                        }
                        else
                        {
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
