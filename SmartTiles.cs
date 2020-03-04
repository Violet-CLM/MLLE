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
        internal List<ushort> Extras { get { return Assignments[35].Tiles; } }
        internal List<int> Friends = new List<int>();
        internal ushort PreviewTileID;
        internal string Name = "Smart Tile";
        internal class ushortComparer : IEqualityComparer<ushort>
        {
            public ushort AndValue;
            public ushortComparer(ushort andValue) { AndValue = andValue; }
            public bool Equals(ushort x, ushort y)
            {
                return (x & AndValue) == (y & AndValue);
            }

            public int GetHashCode(ushort obj)
            {
                return EqualityComparer<ushort>.Default.GetHashCode(obj);
            }

            public static ushortComparer compare123 = new ushortComparer(1024 - 1);
            public static ushortComparer compare124 = new ushortComparer(4096 - 1);
        }
        internal HashSet<ushort> TilesICanPlace, TilesIGoNextTo;
        internal class Rule
        {
            internal int X, Y;
            internal bool Not;
            internal int OtherSmartTileID = -1;
            internal List<ushort> SpecificTiles = new List<ushort>();
            internal List<ushort> Result = new List<ushort>();
            internal Rule() { }
            internal Rule(Rule other)
            {
                X = other.X;
                Y = other.Y;
                Not = other.Not;
                OtherSmartTileID = other.OtherSmartTileID;
                SpecificTiles.AddRange(other.SpecificTiles);
                Result.AddRange(other.Result);
            }

            internal bool Applies(ArrayMap<ushort> tileMap, System.Drawing.Point location, List<SmartTile> otherSmartTiles)
            {
                int x = location.X + X;
                if (x < 0 || x >= tileMap.GetLength(0))
                    return false;
                int y = location.Y + Y;
                if (y < 0 || y >= tileMap.GetLength(1))
                    return false;
                ushort tileID = tileMap[x, y];
                return Not ^ (OtherSmartTileID == -1) ? SpecificTiles.Contains(tileID) : otherSmartTiles[OtherSmartTileID].TilesICanPlace.Contains(tileID);
            }
        }
        internal class Assignment
        {
            internal List<ushort> Tiles = new List<ushort>();
            internal List<Rule> Rules = new List<Rule>();
            internal Assignment() { }
            internal Assignment(Assignment other)
            {
                Tiles = new List<ushort>(other.Tiles);
                Rules = other.Rules.Select(rule => new Rule(rule)).ToList();
            }
            public bool Empty { get { return Tiles.Count == 0; } }
            internal void UnionWith(HashSet<ushort> hashSet)
            {
                hashSet.UnionWith(Tiles);
                foreach (Rule rule in Rules)
                    hashSet.UnionWith(rule.Result);
            }
        }
        internal Assignment[] Assignments = new Assignment[100];

        internal SmartTile() { }
        internal SmartTile(SmartTile other)
        {
            Assignments = other.Assignments.Select(ass => new Assignment(ass)).ToArray();
            PreviewTileID = other.PreviewTileID;
            Name = other.Name;
            Friends = new List<int>(other.Friends);
            TilesICanPlace = new HashSet<ushort>(other.TilesICanPlace.Comparer);
            TilesICanPlace.UnionWith(other.TilesICanPlace);
            TilesIGoNextTo = new HashSet<ushort>(other.TilesIGoNextTo.Comparer);
            TilesIGoNextTo.UnionWith(other.TilesIGoNextTo);
        }
        internal SmartTile(bool maxTiles4096, ushort fileVersion, BinaryReader reader)
        {
            Name = reader.ReadString();
            for (int i = 0; i < Assignments.Length; ++i) {
                Assignment assignment = Assignments[i] = new Assignment();
                for (int numberOfTileIDs = reader.ReadByte(); numberOfTileIDs > 0; --numberOfTileIDs)
                {
                    ushort tileID = reader.ReadUInt16();
                    if (!maxTiles4096 && (tileID & 0x1000) != 0) //hflip
                        tileID ^= 0x1400;
                    assignment.Tiles.Add(tileID);
                }
                if (fileVersion >= 2)
                {
                    for (int numberOfRules = reader.ReadByte(); numberOfRules > 0; --numberOfRules)
                    {
                        Rule newRule = new Rule();
                        newRule.X = reader.ReadSByte();
                        newRule.Y = reader.ReadSByte();
                        newRule.Not = reader.ReadBoolean();
                        newRule.OtherSmartTileID = reader.ReadSByte();
                        if (newRule.OtherSmartTileID == -1)
                            for (int numberOfSpecificTiles = reader.ReadByte(); numberOfSpecificTiles > 0; --numberOfSpecificTiles)
                                newRule.SpecificTiles.Add(reader.ReadUInt16());
                        for (int numberOfResults = reader.ReadByte(); numberOfResults > 0; --numberOfResults)
                            newRule.Result.Add(reader.ReadUInt16());
                        assignment.Rules.Add(newRule);
                    }
                }
            }
            if (fileVersion >= 1)
            {
                for (int friendCount = reader.ReadByte(); friendCount > 0; --friendCount)
                    Friends.Add(reader.ReadByte());
            }
            ushortComparer comparer = maxTiles4096 ? ushortComparer.compare124 : ushortComparer.compare123;
            TilesICanPlace = new HashSet<ushort>(comparer);
            TilesIGoNextTo = new HashSet<ushort>(comparer);
        }
        internal void UpdateAllPossibleTiles(List<SmartTile> smartTiles)
        {
            TilesICanPlace.Clear();
            foreach (var assignment in Assignments)
                if (assignment.Tiles != Extras)
                    assignment.UnionWith(TilesICanPlace);

            TilesIGoNextTo.Clear();
            TilesIGoNextTo.UnionWith(TilesICanPlace);
            TilesIGoNextTo.UnionWith(Extras);
            
            foreach (int friendID in Friends)
            {
                SmartTile smartTile = smartTiles[friendID];
                TilesIGoNextTo.UnionWith(smartTile.TilesICanPlace);
                TilesIGoNextTo.UnionWith(smartTile.Extras);
            }

            Assignment previewTileSource = Assignments[1];
            if (previewTileSource.Empty)
            {
                previewTileSource = Assignments[11];
                if (previewTileSource.Empty)
                {
                    previewTileSource = Assignments[14];
                    if (previewTileSource.Empty)
                        previewTileSource = Assignments[47];
                }
            }
            PreviewTileID = previewTileSource.Tiles[Rand.Next(previewTileSource.Tiles.Count)];
        }

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
        static readonly internal int[,] AssignmentPairings = new int[37,2]{
            {0,2},
            {3,5},
            {6,7},
            {8,9},
            {10,12},
            {13,15},
            {16,17},
            {18,19},
            {20,22},
            {23,25},
            {26,27},
            {28,29},
            {30,32},
            {36,37},
            {38,39},
            {40,41},
            {42,43},
            {44,65},
            {45,64},
            {46,66},
            {47,49},
            {50,51},
            {52,53},
            {54,75},
            {55,74},
            {56,76},
            {60,61},
            {62,63},
            {68,69},
            {80,81},
            {82,83},
            {84,85},
            {86,97},
            {87,96},
            {90,91},
            {92,93},
            {94,95}
        };
        static readonly internal int[,] AssignmentVerticalPairings = new int[32, 2]{
            {0,20},
            {1,21},
            {2,22},
            {3,23},
            {4,24},
            {5,25},
            {6,16},
            {7,17},
            {8,18},
            {9,19},
            {26,36},
            {27,37},
            {28,38},
            {29,39},
            {40,52},
            {41,53},
            {42,50},
            {43,51},
            {44,54},
            {45,55},
            {46,56},
            {57,77},
            {60,62},
            {61,63},
            {64,74},
            {65,75},
            {66,76},
            {68,69},
            {84,95},
            {85,94},
            {86,96},
            {87,97}
        };

        Random Rand = new Random();
        public bool Apply(ArrayMap<ushort> tileMap, System.Drawing.Point location, List<SmartTile> otherSmartTiles)
        {
            ArrayMap<ushort> localTiles = new ArrayMap<ushort>(5, 5);
            for (int xx = 0; xx < 5; ++xx)
            {
                int xTile = Math.Max(0, Math.Min(tileMap.GetLength(0) - 1, location.X + xx - 2));
                for (int yy = 0; yy < 5; ++yy)
                {
                    int yTile = Math.Max(0, Math.Min(tileMap.GetLength(1) - 1, location.Y + yy - 2));
                    localTiles[xx, yy] = tileMap[xTile, yTile];
                }
            }
            ArrayMap<bool?> LocalTilesAreRelated = new ArrayMap<bool?>(5,5);
            Func<int, int, bool> getRelatedness = (x, y) =>
            {
                x += 2;
                y += 2;
                return LocalTilesAreRelated[x, y] ?? (LocalTilesAreRelated[x, y] = TilesIGoNextTo.Contains(localTiles[x, y])).Value;
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
                    if (getRelatedness(-1, 1) && !getRelatedness(1, 1))
                        assignmentID = 80;
                    else if (getRelatedness(1, 1))
                        assignmentID = 81;
                    else
                        assignmentID = 57;
                    break;
                case 3: //UD
                    assignmentID = 67;
                    break;
                case 4: //L
                    assignmentID = 32;
                    break;
                case 5: //LU
                    if (getRelatedness(1, -1))
                        assignmentID = getRelatedness(-1,-1) ? 53 : 94;
                    else if (getRelatedness(-1, 1))
                        assignmentID = getRelatedness(-1,-1) ? 75 : 87;
                    else
                        assignmentID = getRelatedness(-1,-1) ? 22 : 25;
                    break;
                case 6: //LD
                    if (getRelatedness(1, 1))
                        assignmentID = getRelatedness(-1, 1) ? 41 : 85;
                    else if (getRelatedness(-1, -1))
                        assignmentID = getRelatedness(-1, 1) ? 65 : 97;
                    else
                        assignmentID = getRelatedness(-1, 1) ? 2 : 5;
                    break;
                case 7: //LUD
                    if (getRelatedness(-1, -1))
                        assignmentID = getRelatedness(-1, 1) ? 12 : 27;
                    else
                        assignmentID = getRelatedness(-1, 1) ? (getRelatedness(1, -1) || getRelatedness(0, -2) ? 37 : 90) : 15;
                    break;
                case 8: //R
                    assignmentID = 30;
                    break;
                case 9: //RU
                    if (getRelatedness(-1, -1))
                        assignmentID = getRelatedness(1, -1) ? 52 : 95;
                    else if (getRelatedness(1, 1))
                        assignmentID = getRelatedness(1, -1) ? 54 : 96;
                    else
                        assignmentID = getRelatedness(1, -1) ? 20 : 23;
                    break;
                case 10: //RD
                    if (getRelatedness(-1, 1))
                        assignmentID = getRelatedness(1, 1) ? 40 : 84;
                    else if (getRelatedness(1, -1))
                        assignmentID = getRelatedness(1, 1) ? 44 : 86;
                    else
                        assignmentID = getRelatedness(1, 1) ? 0 : 3;
                    break;
                case 11: //RUD
                    if (getRelatedness(1, -1))
                        assignmentID = getRelatedness(1, 1) ? 10 : 26;
                    else
                        assignmentID = getRelatedness(1, 1) ? (getRelatedness(-1, -1) || getRelatedness(0, -2) ? 36 : 91) : 13;
                    break;
                case 12: //LR
                    assignmentID = 31;
                    break;
                case 13: //LRU
                    if (getRelatedness(-1, -1))
                        assignmentID = getRelatedness(1, -1) ? 21 : 18;
                    else
                        assignmentID = getRelatedness(1, -1) ? 19 : 24;
                    break;
                case 14: //LRD
                    if (getRelatedness(-1, 1))
                        assignmentID = getRelatedness(1, 1) ? 1 : 8;
                    else
                        assignmentID = getRelatedness(1, 1) ? 9 : 4;
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
                            if (!getRelatedness(0, 2))
                                assignmentID = 42;
                            else if (!getRelatedness(-2, 0))
                                assignmentID = 55;
                            else
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
                            if (!getRelatedness(0, 2))
                                assignmentID = 43;
                            else if (!getRelatedness(2, 0))
                                assignmentID = 74;
                            else
                                assignmentID = 6;
                            break;
                        case 12:
                            assignmentID = 59;
                            break;
                        case 13:
                            if (!getRelatedness(0, -2))
                                assignmentID = 51;
                            else if (!getRelatedness(2, 0))
                                assignmentID = 64;
                            else
                                assignmentID = 16;
                            break;
                        case 14:
                            if (!getRelatedness(0, -2))
                                assignmentID = 50;
                            else if (!getRelatedness(-2, 0))
                                assignmentID = 45;
                            else
                                assignmentID = 17;
                            break;
                        case 15: //totally surrounded, normal wall tile
                            assignmentID = 11;
                            break;
                    }
                    break;
            }

            while (Assignments[assignmentID].Tiles.Count == 0)
            {
                ushort[] alternatives = AlternativeAssignments[assignmentID];
                int alternativeID = 0;
                for (; alternativeID < alternatives.Length - 1; ++alternativeID)
                    if (!Assignments[alternatives[alternativeID]].Empty)
                        break;
                assignmentID = alternatives[alternativeID];
            }

            bool lastRuleApplied = true;
            foreach (Rule rule in Assignments[assignmentID].Rules)
            {
                List<ushort> frames = rule.Result;
                bool applies = lastRuleApplied && rule.Applies(tileMap, location, otherSmartTiles);
                if (frames.Count == 0) //and
                {
                    lastRuleApplied = applies;
                }
                else //then
                {
                    if (applies)
                    {
                        tileMap[location.X, location.Y] = frames[frames.Count == 1 ? 0 : Rand.Next(frames.Count)];
                        return true;
                    }
                    lastRuleApplied = true;
                }
            }

            var tiles = Assignments[assignmentID].Tiles;
            if (tiles.Count >= 1)
                tileMap[location.X, location.Y] = tiles[tiles.Count == 1 ? 0 : Rand.Next(tiles.Count)];
            else
                return false;
            return true;
        }

        internal bool SmartFlipTile(ref ushort tileID, bool vertical)
        {
            var pairings = !vertical ? AssignmentPairings : AssignmentVerticalPairings;
            int numberOfPairings = pairings.GetLength(0);
            for (int i = 0; i < numberOfPairings; ++i)
                for (int j = 0; j < 2; ++j)
                    if (Assignments[pairings[i, j]].Tiles.Contains(tileID))
                    {
                        List<ushort> flippedAssignment = Assignments[pairings[i, j ^ 1]].Tiles;
                        if (flippedAssignment.Count > 0)
                        {
                            tileID = flippedAssignment[Rand.Next(flippedAssignment.Count)];
                            return true;
                        }
                    }
            if (TilesICanPlace.Contains(tileID)) //in a symmetrical assignment
                return true; //but don't alter
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
                ushort version = reader.ReadUInt16();
                if (version > 2) {
                    System.Windows.Forms.MessageBox.Show("The file \"" + filepath + "\" was not saved in a format that this version of MLLE understands. Please make sure you have the latest MLLE release.", "Incompatible File Version", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    success = false;
                } else {
                    for (int numberOfSmartTiles = reader.ReadByte(); numberOfSmartTiles > 0; --numberOfSmartTiles)
                        SmartTiles.Add(new SmartTile(J2L.MaxTiles == 4096, version, reader));
                    foreach (SmartTile smartTile in SmartTiles)
                        smartTile.UpdateAllPossibleTiles(SmartTiles);
                }
            }
            return success;
        }
        private void SaveSmartTiles()
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(Path.ChangeExtension(J2L.Tilesets[0].FullFilePath, ".MLLESet"), FileMode.Create), J2File.FileEncoding)) {
                writer.Write((ushort)2); //version
                writer.Write((byte)SmartTiles.Count);
                foreach (SmartTile smartTile in SmartTiles)
                {
                    writer.Write(smartTile.Name);
                    foreach (SmartTile.Assignment assignment in smartTile.Assignments) //constant length (100), don't need to preface this with anything
                    {
                        var tiles = assignment.Tiles;
                        writer.Write((byte)tiles.Count);
                        foreach (ushort tileID in tiles)
                            writer.Write(tileID);
                        var rules = assignment.Rules;
                        writer.Write((byte)rules.Count);
                        foreach (SmartTile.Rule rule in rules)
                        {
                            writer.Write((sbyte)rule.X);
                            writer.Write((sbyte)rule.Y);
                            writer.Write(rule.Not);
                            writer.Write((sbyte)rule.OtherSmartTileID);
                            if (rule.OtherSmartTileID == -1)
                            {
                                writer.Write((byte)rule.SpecificTiles.Count);
                                foreach (ushort tileID in rule.SpecificTiles)
                                    writer.Write(tileID);
                            }
                            writer.Write((byte)rule.Result.Count);
                            foreach (ushort tileID in rule.Result)
                                writer.Write(tileID);
                        }
                    }
                    writer.Write((byte)smartTile.Friends.Count);
                    foreach (int friendID in smartTile.Friends)
                        writer.Write((byte)friendID);
                }
            }
        }
        private void SmartFlipTile(ref ushort tileID, bool vertical)
        {
            foreach (SmartTile smartTile in SmartTiles)
                if (smartTile.SmartFlipTile(ref tileID, vertical))
                    return;
            //if it can't be found
            if (!vertical)
                tileID ^= (ushort)J2L.MaxTiles;
            else
                tileID ^= 0x2000;
        }
    }
}
