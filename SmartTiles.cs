using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Extra.Collections;

namespace MLLE
{
    class SmartTile
    {
        internal List<ushort>[] TileAssignments = new List<ushort>[100];
        internal ushort PreviewTileID;
        internal string Name = "Smart Tile";
        internal HashSet<ushort> AllPossibleTiles = new HashSet<ushort>();

        internal SmartTile()
        {
            for (var i = 0; i < TileAssignments.Length; ++i)
                TileAssignments[i] = new List<ushort>();
        }
        internal SmartTile(SmartTile other)
        {
            for (var i = 0; i < TileAssignments.Length; ++i)
                TileAssignments[i] = new List<ushort>(other.TileAssignments[i]);
            PreviewTileID = other.PreviewTileID;
            Name = other.Name;
            AllPossibleTiles.UnionWith(other.AllPossibleTiles);
        }
        internal void UpdateAllPossibleTiles()
        {
            AllPossibleTiles.Clear();
            foreach (var assignment in TileAssignments)
                AllPossibleTiles.UnionWith(assignment);
            PreviewTileID = 1; //todo
        }
        /*class Rule
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
                internal bool Applies(ArrayMap<ushort> localTiles)
                {
                    return TileIDs.Contains(localTiles[2 + X, 2 + Y]) != Not;
                }
            }
            Condition[] Conditions;
            internal Rule(ushort[] t, Condition[] c)
            {
                TileIDs = t;
                Conditions = c;
            }
            internal bool Applies(ArrayMap<ushort> localTiles, ref ushort result)
            {
                foreach (Condition condition in Conditions)
                    if (!condition.Applies(localTiles))
                        return false;
                result = TileIDs[Mainframe._r.Next(TileIDs.Length)];
                return true;
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
                else if (line[0] == '#') //ignore comments
                    continue;
                else
                {
                    var words = line.Split(' ');
                    if (stillInDictionaryMode)
                    {
                        tileGroups.Add(words[0], CreateTileIDList(words.Skip(1), tileGroups));
                    }
                    else
                    {
                        switch (words.Length) {
                            case 1:
                                workingSmartTile.Rules.Add(new Rule(CreateTileIDList(words, tileGroups), workingConditions.ToArray()));
                                workingConditions.Clear();
                                break;
                            case 2:
                                if (!ushort.TryParse(words[0], out workingSmartTile.PreviewTileID))
                                    Debug.WriteLine("Invalid line " + line);
                                else
                                    workingSmartTile.NonLocalTargets = CreateTileIDList(words.Skip(1), tileGroups);
                                break;
                            default:
                                workingConditions.Add(new Rule.Condition(words, tileGroups));
                                break;
                        }
                    }
                }
            }
            if (workingSmartTile.Rules.Count != 0)
                result.Add(workingSmartTile);
            smartTiles = result.ToArray();
            return true;
        }*/
        public bool Apply(ref ushort result, ArrayMap<ushort> localTiles, bool directAction)
        {
            if (directAction || AllPossibleTiles.Contains(result)) //otherwise this is outside the scope of this smart tile and should be left alone
            {
                ArrayMap<bool?> LocalTilesAreRelated = new ArrayMap<bool?>(5,5);
                Func<int, int, bool> getRelatedness = (x, y) =>
                {
                    x += 2;
                    y += 2;
                    return LocalTilesAreRelated[x, y] ?? (LocalTilesAreRelated[x, y] = AllPossibleTiles.Contains(localTiles[x, y])).Value;
                };

                int assignmentID = 47;
                switch (
                    (getRelatedness( 0, -1) ? 1 : 0) |
                    (getRelatedness( 0,  1) ? 2 : 0) |
                    (getRelatedness(-1,  0) ? 4 : 0) |
                    (getRelatedness( 1,  0) ? 8 : 0)
                ) {
                    case 0:
                        assignmentID = 47;
                        break;
                    case 1: //U
                        assignmentID = 77;
                        break;
                    case 2: //D
                        assignmentID = 57;
                        break;
                    case 3: //UD
                        assignmentID = 67;
                        break;
                    case 4: //L
                        assignmentID = 32;
                        break;
                    case 5: //LU
                        assignmentID = 25; //todo
                        break;
                    case 6: //LD
                        assignmentID = 5; //todo
                        break;
                    case 7: //LUD
                        assignmentID = 15; //todo
                        break;
                    case 8: //R
                        assignmentID = 30;
                        break;
                    case 9: //RU
                        assignmentID = 23; //todo
                        break;
                    case 10: //RD
                        assignmentID = 3; //todo
                        break;
                    case 11: //RUD
                        assignmentID = 13; //todo
                        break;
                    case 12: //LR
                        assignmentID = 31;
                        break;
                    case 13: //LRU
                        assignmentID = 24; //todo
                        break;
                    case 14: //LRD
                        assignmentID = 4; //todo
                        break;
                    case 15: //LRUD
                        assignmentID = 14; //todo
                        break;
                }

                var assignment = TileAssignments[assignmentID];
                if (assignment.Count == 0)
                    assignment = TileAssignments[47]; //todo
                if (assignment.Count == 1) //simpler case
                {
                    result = assignment[0];
                    return true;
                }
                if (assignment.Count > 1)
                {
                    result = assignment[new Random().Next() % assignment.Count];
                    return true;
                }
            }
            return false;
        }
    }
    partial class Mainframe
    {
        List<SmartTile> SmartTiles = new List<SmartTile>();
        private bool CheckForSmartTileFile()
        {
            SmartTiles.Clear();
            if (!J2L.HasTiles)
                return false;
            string filepath = Path.ChangeExtension(J2L.Tilesets[0].FullFilePath, ".MLLESet");
            if (!File.Exists(filepath)) //check in JJ2 folder
                if (!File.Exists(filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(filepath)))) //check in MLLE folder
                    return false;
            return false;// SmartTile.DefineSmartTiles(filepath, out SmartTiles);
        }
    }
}
