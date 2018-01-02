using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace MLLE
{
    class SmartTile
    {
        internal ushort TileID;
        class Rule
        {
            ushort[] TileIDs;
            internal class Condition
            {
                int X, Y;
                bool Not;
                ushort[] TileIDs;
                internal Condition(string[] words, Dictionary<string, ushort[]> groups)
                {
                    if (int.TryParse(words[0], out X) && Math.Abs(X) <= 2 && int.TryParse(words[1], out Y) && Math.Abs(Y) <= 2) //2? maybe?
                    {
                        Not = words[2] == "!";
                        TileIDs = CreateTileIDList(words.Skip(Not ? 3 : 2), groups);
                    }
                    else
                        Debug.WriteLine("Invalid condition coordinates");
                }
            }
            Condition[] Conditions;
            internal Rule(ushort[] t, Condition[] c)
            {
                TileIDs = t;
                Conditions = c;
            }
        }
        List<Rule> Rules = new List<Rule>();

        static ushort[] CreateTileIDList(IEnumerable<string> words, Dictionary<string, ushort[]> groups)
        {
            var list = new List<ushort>();
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
            var result = new List<SmartTile>();
            var tileGroups = new Dictionary<string, ushort[]>();
            bool stillInDictionaryMode = true;

            var lines = File.ReadAllLines(filepath);
            SmartTile workingSmartTile = null;
            List<Rule.Condition> workingConditions = new List<Rule.Condition>();
            foreach (string line in lines)
            {
                if (line == String.Empty) //section break
                {
                    if (stillInDictionaryMode)
                        stillInDictionaryMode = false;
                    else
                        result.Add(workingSmartTile);
                    workingSmartTile = new SmartTile();
                }
                else
                {
                    var words = line.Split(' ');
                    if (stillInDictionaryMode)
                    {
                        tileGroups.Add(words[0], CreateTileIDList(words.Skip(1), tileGroups));
                    }
                    else
                    {
                        if (words.Length == 1)
                        {
                            if (words[0][0] == '\t') //tab
                            {
                                words[0] = words[0].Substring(1);
                                workingSmartTile.Rules.Add(new Rule(CreateTileIDList(words, tileGroups), workingConditions.ToArray()));
                                workingConditions.Clear();
                            }
                            else
                            {
                                if (!ushort.TryParse(words[0], out workingSmartTile.TileID))
                                    Debug.WriteLine("Invalid line " + line);
                            }
                        }
                        else if (words.Length >= 3)
                        {
                            workingConditions.Add(new Rule.Condition(words, tileGroups));
                        }
                        else
                            Debug.WriteLine("Invalid line " + line);
                    }
                }
            }
            if (workingSmartTile.Rules.Count != 0)
                result.Add(workingSmartTile);
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
