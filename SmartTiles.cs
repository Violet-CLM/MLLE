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

            List<ushort> previewTileSource = TileAssignments[1];
            if (previewTileSource.Count == 0)
            {
                previewTileSource = TileAssignments[11];
                if (previewTileSource.Count == 0)
                {
                    previewTileSource = TileAssignments[14];
                    if (previewTileSource.Count == 0)
                        previewTileSource = TileAssignments[47];
                }
            }
            PreviewTileID = previewTileSource[Rand.Next(previewTileSource.Count)];
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

        static readonly internal ushort[][] AlternativeAssignments = new ushort[100][]{
            new ushort[] {1, 3, 10},
            new ushort[] {4, 11},
            new ushort[] {1, 5, 12},
            new ushort[] {0, 14},
            new ushort[] {1, 14},
            new ushort[] {2, 14},
            new ushort[] {43, 11},
            new ushort[] {42, 11},
            new ushort[] {1},
            new ushort[] {1},

            new ushort[] {13, 11},
            new ushort[] {14, 47},
            new ushort[] {15, 11},
            new ushort[] {10, 14},
            new ushort[] {47, 11},
            new ushort[] {12, 14},
            new ushort[] {51, 11},
            new ushort[] {50, 11},
            new ushort[] {21},
            new ushort[] {21},

            new ushort[] {21, 23, 10},
            new ushort[] {24, 11},
            new ushort[] {21, 25, 12},
            new ushort[] {20, 14},
            new ushort[] {21, 14},
            new ushort[] {22, 14},
            new ushort[] {10},
            new ushort[] {12},
            new ushort[] {9, 36, 11},
            new ushort[] {8, 37, 11},

            new ushort[] {31, 0, 10, 47},
            new ushort[] {1, 11},
            new ushort[] {31, 2, 12, 47},
            null,
            null,
            new ushort[] {47}, //"extra"
            new ushort[] {10},
            new ushort[] {12},
            new ushort[] {19, 26, 11},
            new ushort[] {18, 27, 11},

            new ushort[] {0},
            new ushort[] {2},
            new ushort[] {7},
            new ushort[] {6},
            new ushort[] {0},
            new ushort[] {17},
            new ushort[] {37},
            new ushort[] {1, 11, 14},
            new ushort[] {17},
            new ushort[] {16},

            new ushort[] {17},
            new ushort[] {16},
            new ushort[] {20},
            new ushort[] {22},
            new ushort[] {20},
            new ushort[] {7},
            new ushort[] {27},
            new ushort[] {67, 0, 1, 47},
            new ushort[] {11},
            new ushort[] {11},

            new ushort[] {19},
            new ushort[] {18},
            new ushort[] {9},
            new ushort[] {8},
            new ushort[] {16},
            new ushort[] {2},
            new ushort[] {36},
            new ushort[] {10, 47},
            new ushort[] {17},
            new ushort[] {16},

            null,
            null,
            null,
            null,
            new ushort[] {6},
            new ushort[] {22},
            new ushort[] {26},
            new ushort[] {67, 20, 21, 47},
            null,
            null,
            
            new ushort[] {57},
            new ushort[] {57},
            new ushort[] {10},
            new ushort[] {12},
            new ushort[] {3},
            new ushort[] {5},
            new ushort[] {3},
            new ushort[] {25},
            null,
            null,
            
            new ushort[] {37},
            new ushort[] {36},
            new ushort[] {20},
            new ushort[] {22},
            new ushort[] {25},
            new ushort[] {23},
            new ushort[] {23},
            new ushort[] {5},
            null,
            null
        };

        Random Rand = new Random();
        public bool Apply(ref ushort result, ArrayMap<ushort> localTiles, bool directAction)
        {
            if (directAction || (!TileAssignments[35/*"extra"*/].Contains(result) && AllPossibleTiles.Contains(result))) //otherwise this is outside the scope of this smart tile and should be left alone
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
                    case 0: //no neighbors at all
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
                        assignmentID = getRelatedness(-1,-1) ? 22 : 25;
                        break;
                    case 6: //LD
                        assignmentID = getRelatedness(-1, 1) ? 2 : 5;
                        break;
                    case 7: //LUD
                        if (getRelatedness(-1, -1)) {
                            if (getRelatedness(-1, 1))
                                assignmentID = 12;
                            else
                                assignmentID = 27;
                        } else {
                            if (getRelatedness(-1, 1))
                                assignmentID = 37;
                            else
                                assignmentID = 15;
                        }
                        break;
                    case 8: //R
                        assignmentID = 30;
                        break;
                    case 9: //RU
                        assignmentID = getRelatedness(1, -1) ? 20 : 23;
                        break;
                    case 10: //RD
                        assignmentID = getRelatedness(1, 1) ? 0 : 3;
                        break;
                    case 11: //RUD
                        if (getRelatedness(1, -1)) {
                            if (getRelatedness(-1, 1))
                                assignmentID = 10;
                            else
                                assignmentID = 26;
                        } else {
                            if (getRelatedness(1, 1))
                                assignmentID = 36;
                            else
                                assignmentID = 13;
                        }
                        break;
                    case 12: //LR
                        assignmentID = 31;
                        break;
                    case 13: //LRU
                        if (getRelatedness(-1, 1)) {
                            if (getRelatedness(1, 1))
                                assignmentID = 21;
                            else
                                assignmentID = 18;
                        } else {
                            if (getRelatedness(1, 1))
                                assignmentID = 19;
                            else
                                assignmentID = 24;
                        }
                        break;
                    case 14: //LRD
                        if (getRelatedness(-1, 1)) {
                            if (getRelatedness(1, 1))
                                assignmentID = 1;
                            else
                                assignmentID = 8;
                        } else {
                            if (getRelatedness(1, 1))
                                assignmentID = 9;
                            else
                                assignmentID = 4;
                        }
                        break;
                    case 15: //LRUD
                        switch (
                            (getRelatedness(-1, -1) ? 1 : 0) |
                            (getRelatedness( 1, -1) ? 2 : 0) |
                            (getRelatedness( 1,  1) ? 4 : 0) |
                            (getRelatedness(-1,  1) ? 8 : 0)
                        ) {
                            case 0: //no corners at all, full pipe plus
                                assignmentID = 14;
                                break;
                            case 1:
                                assignmentID = 39;
                                break;
                            case 2:
                                assignmentID = 38;
                                break;
                            case 3:
                                assignmentID = 58;
                                break;
                            case 4:
                                assignmentID = 28;
                                break;
                            case 5:
                                assignmentID = 69;
                                break;
                            case 6:
                                assignmentID = 48;
                                break;
                            case 7:
                                assignmentID = 7;
                                break;
                            case 8:
                                assignmentID = 29;
                                break;
                            case 9:
                                assignmentID = 49;
                                break;
                            case 10:
                                assignmentID = 68;
                                break;
                            case 11:
                                assignmentID = 6;
                                break;
                            case 12:
                                assignmentID = 59;
                                break;
                            case 13:
                                assignmentID = 16;
                                break;
                            case 14:
                                assignmentID = 17;
                                break;
                            case 15: //totally surrounded, normal wall tile
                                assignmentID = 11;
                                break;
                        }
                        break;
                }

                while (TileAssignments[assignmentID].Count == 0)
                {
                    ushort[] alternatives = AlternativeAssignments[assignmentID];
                    int alternativeID = 0;
                    for (; alternativeID < alternatives.Length - 1; ++alternativeID)
                        if (TileAssignments[alternatives[alternativeID]].Count != 0)
                            break;
                    assignmentID = alternatives[alternativeID];
                }

                var assignment = TileAssignments[assignmentID];
                if (assignment.Count == 1) //simpler case
                {
                    result = assignment[0];
                    return true;
                }
                if (assignment.Count > 1)
                {
                    result = assignment[Rand.Next(assignment.Count)];
                    return true;
                }
            }
            return false;
        }
    }
    partial class Mainframe
    {
        List<SmartTile> SmartTiles = new List<SmartTile>();
        private bool LoadSmartTiles()
        {
            SmartTiles.Clear();
            if (!J2L.HasTiles)
                return false;
            string filepath = Path.ChangeExtension(J2L.Tilesets[0].FullFilePath, ".MLLESet");
            if (!File.Exists(filepath)) //check in JJ2 folder
                if (!File.Exists(filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(filepath)))) //check in MLLE folder
                    return false;

            bool success = true;
            using (BinaryReader reader = new BinaryReader(File.Open(filepath, FileMode.Open), J2File.FileEncoding))
            {
                if (reader.ReadUInt16() > 0) {
                    System.Windows.Forms.MessageBox.Show("The file \"" + filepath + "\" was not saved in a format that this version of MLLE understands. Please make sure you have the latest MLLE release.", "Incompatible File Version", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    success = false;
                } else {
                    for (int numberOfSmartTiles = reader.ReadByte(); numberOfSmartTiles > 0; --numberOfSmartTiles)
                    {
                        SmartTile newSmartTile = new SmartTile();
                        newSmartTile.Name = reader.ReadString();
                        foreach (List<ushort> assignment in newSmartTile.TileAssignments)
                            for (int numberOfTileIDs = reader.ReadByte(); numberOfTileIDs > 0; --numberOfTileIDs)
                                assignment.Add(reader.ReadUInt16());
                        newSmartTile.UpdateAllPossibleTiles();
                        SmartTiles.Add(newSmartTile);
                    }
                }
            }
            return success;
        }
        private void SaveSmartTiles()
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(Path.ChangeExtension(J2L.Tilesets[0].FullFilePath, ".MLLESet"), FileMode.Create), J2File.FileEncoding)) {
                writer.Write((ushort)0); //version
                writer.Write((byte)SmartTiles.Count);
                foreach (SmartTile smartTile in SmartTiles)
                {
                    writer.Write(smartTile.Name);
                    foreach (List<ushort> assignment in smartTile.TileAssignments) //constant length (100), don't need to preface this with anything
                    {
                        writer.Write((byte)assignment.Count);
                        foreach (ushort tileID in assignment)
                            writer.Write(tileID);
                    }
                }
            }
        }
    }
}
