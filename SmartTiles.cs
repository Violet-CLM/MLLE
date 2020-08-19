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
        internal ushort[] PreviewTileIDs = new ushort[9];
        internal string Name = "Smart Tile";
        private J2TFile Tileset;
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

            internal bool Applies(ArrayMap<ushort> tileMap, List<SmartTile> otherSmartTiles, IEqualityComparer<ushort> comparer)
            {
                ushort tileID = tileMap[X+2, Y+3];
                return Not ^ ((OtherSmartTileID == -1) ? SpecificTiles.Contains(tileID, comparer) : (otherSmartTiles[OtherSmartTileID].TilesICanPlace.Contains(tileID, comparer) || otherSmartTiles[OtherSmartTileID].Extras.Contains(tileID, comparer)));
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
            PreviewTileIDs = other.PreviewTileIDs.Clone() as ushort[];
            Name = other.Name;
            Friends = new List<int>(other.Friends);
            TilesICanPlace = new HashSet<ushort>(other.TilesICanPlace.Comparer);
            TilesICanPlace.UnionWith(other.TilesICanPlace);
            TilesIGoNextTo = new HashSet<ushort>(other.TilesIGoNextTo.Comparer);
            TilesIGoNextTo.UnionWith(other.TilesIGoNextTo);
            Tileset = other.Tileset;
        }
        internal SmartTile(bool maxTiles4096, J2TFile tileset)
        {
            for (int i = 0; i < Assignments.Length; ++i)
                Assignments[i] = new Assignment();
            ushortComparer comparer = maxTiles4096 ? ushortComparer.compare124 : ushortComparer.compare123;
            TilesICanPlace = new HashSet<ushort>(comparer);
            TilesIGoNextTo = new HashSet<ushort>(comparer);
            Tileset = tileset;
        }
        internal SmartTile(bool maxTiles4096, J2TFile tileset, ushort fileVersion, BinaryReader reader, uint tileOffset) : this(maxTiles4096, tileset)
        {
            Name = reader.ReadString();
            Action<List<ushort>> conditionallyAddTileIDToList = (list) =>
            {
                ushort tileID = reader.ReadUInt16();
                int offsetTileID = (int)((tileID & 0xFFF) - tileset.FirstTile);
                if (offsetTileID >= 0 && offsetTileID < tileset.TileCount)
                {
                    if (!maxTiles4096 && (tileID & 0x1000) != 0) //hflip
                        tileID ^= 0x1400;
                    list.Add((ushort)(tileID + tileOffset));
                }
            };
            for (int i = 0; i < Assignments.Length; ++i) {
                Assignment assignment = Assignments[i];
                for (int numberOfTileIDs = reader.ReadByte(); numberOfTileIDs > 0; --numberOfTileIDs)
                    conditionallyAddTileIDToList(assignment.Tiles);
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
                        {
                            for (int numberOfSpecificTiles = reader.ReadByte(); numberOfSpecificTiles > 0; --numberOfSpecificTiles)
                                conditionallyAddTileIDToList(newRule.SpecificTiles);
                        }
                        int intendedNumberOfResults = reader.ReadByte();
                        for (int numberOfResults = intendedNumberOfResults; numberOfResults > 0; --numberOfResults)
                            conditionallyAddTileIDToList(newRule.Result);
                        var rules = assignment.Rules;
                        if (intendedNumberOfResults != 0 && newRule.Result.Count == 0) { //none of the tiles can be found in this subset
                            Rule condition;
                            while (rules.Count != 0 && (condition = rules.Last()).Result.Count == 0) //remove all "and" rules leading up to this one, to avoid confusing any other logic.
                                rules.Remove(condition);
                        } else
                            rules.Add(newRule);
                    }
                }
            }
            if (fileVersion >= 1)
            {
                for (int friendCount = reader.ReadByte(); friendCount > 0; --friendCount)
                    Friends.Add(reader.ReadByte());
            }
        }
        internal void UpdateTilesIGonextTo(List<SmartTile> smartTiles)
        {
            foreach (int friendID in Friends)
            {
                SmartTile smartTile = smartTiles[friendID];
                TilesIGoNextTo.UnionWith(smartTile.TilesICanPlace);
                TilesIGoNextTo.UnionWith(smartTile.Extras);
            }
        }
        static readonly private int[] MandatoryAssignmentIDs = { 11, 14, 47 };
        internal void UpdateAllPossibleTiles(List<SmartTile> smartTiles, bool updateFriends = true)
        {
            TilesICanPlace.Clear();
            foreach (var assignment in Assignments)
                if (assignment.Tiles != Extras)
                    assignment.UnionWith(TilesICanPlace);

            TilesIGoNextTo.Clear();
            TilesIGoNextTo.UnionWith(TilesICanPlace);
            TilesIGoNextTo.UnionWith(Extras);

            if (updateFriends)
                UpdateTilesIGonextTo(smartTiles);

            ushort qualifyingTileID = Assignments[MandatoryAssignmentIDs.First(i => !Assignments[i].Empty)].Tiles[0];
            ArrayMap<ushort> surroundingTiles = new ArrayMap<ushort>(3 + 2 * 2, 3 + 3 + 2);
            for (int x = 2; x < 5; ++x)
                for (int y = 3; y < 6; ++y)
                    surroundingTiles[x, y] = qualifyingTileID;
            for (int pass = 0; pass < 2; ++pass) //why not
                for (int x = 2; x < 5; ++x)
                    for (int y = 3; y < 6; ++y)
                    {
                        ArrayMap<ushort> localTiles = new ArrayMap<ushort>(5, 6);
                        for (int xx = 0; xx < 5; ++xx)
                            for (int yy = 0; yy < 6; ++yy)
                                localTiles[xx, yy] = surroundingTiles[x + xx - 2, y + yy - 3];
                        ushort tileID = surroundingTiles[x, y];
                        if (Apply(localTiles, ref tileID)) //not sure why this would return false, but...
                            surroundingTiles[x, y] = tileID;
                    }
            PreviewTileIDs = Enumerable.Range(0, 9).Select(i => surroundingTiles[(i % 3) + 2, (i / 3) + 3]).ToArray();
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
            new ushort[] {68},
            new ushort[] {69},

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
        static readonly internal int[,] AssignmentPairings = new int[38,2]{
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
            {48,49},
            {50,51},
            {52,53},
            {54,75},
            {55,74},
            {56,76},
            {60,61},
            {62,63},
            {68,69},
            {78,79},
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
        public bool Apply(ArrayMap<ushort> localTiles, ref ushort tileID)
        {
            ArrayMap<bool?> LocalTilesAreRelated = new ArrayMap<bool?>(5,6);
            Func<int, int, bool> getRelatedness = (x, y) =>
            {
                x += 2;
                y += 3;
                return LocalTilesAreRelated[x, y] ?? (LocalTilesAreRelated[x, y] = TilesIGoNextTo.Contains(localTiles[x, y], TilesIGoNextTo.Comparer)).Value;
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
                    if (getRelatedness(0, 2))
                    {
                        if (getRelatedness(-1, 1) && !getRelatedness(1, 1) && getRelatedness(-1, 2))
                        { assignmentID = 80; break; }
                        else if (getRelatedness(1, 1) && !getRelatedness(-1, 1) && getRelatedness(1, 2))
                        { assignmentID = 81; break; }
                    }
                    assignmentID = 57;
                    break;
                case 3: //UD
                    assignmentID = 67;
                    break;
                case 4: //L
                    assignmentID = 32;
                    break;
                case 5: //LU
                    if (!getRelatedness(-1, -1)) //thin
                    {
                        if (getRelatedness(1, -1) && !getRelatedness(0, -2) && Assignments[94].Tiles.Count != 0) //horizontal tube slope
                            assignmentID = 94;
                        else if (getRelatedness(-1, 1) && !getRelatedness(-2, 0)) //vertical tube slope
                            assignmentID = 87;
                        else
                            assignmentID = 25;
                    }
                    else //thick
                    {
                        if (!getRelatedness(1, -1) && !getRelatedness(0, -2) && getRelatedness(-1, -2) && !getRelatedness(-1, -3))
                        {
                            assignmentID = 93;
                            break;
                        }
                        if (Assignments[53].Tiles.Count != 0 && getRelatedness(1, -1)) //ceiling slope
                        {
                            if (getRelatedness(0, -2))
                            {
                                if (getRelatedness(-1, -2) && getRelatedness(1, -2) && Assignments[43].Tiles.Count != 0)
                                {
                                    assignmentID = 53;
                                    break;
                                }
                            }
                            else
                            {
                                if (Assignments[63].Tiles.Count != 0)
                                {
                                    assignmentID = 53;
                                    break;
                                }
                            }
                        }
                        if (Assignments[75].Tiles.Count != 0 && getRelatedness(-1, 1)) //wall slope
                        {
                            if (getRelatedness(-2, 0))
                            {
                                if (getRelatedness(-2, -1) && Assignments[74].Tiles.Count != 0)
                                {
                                    assignmentID = 75;
                                    break;
                                }
                            }
                            else
                            {
                                if (Assignments[76].Tiles.Count != 0)
                                {
                                    assignmentID = 75;
                                    break;
                                }
                            }
                        }
                        assignmentID = 22;
                    }
                    break;
                case 6: //LD
                    if (!getRelatedness(-1, 1)) //thin
                    {
                        if (getRelatedness(1, 1) && !getRelatedness(0, 2) && Assignments[85].Tiles.Count != 0) //horizontal tube slope
                            assignmentID = 85;
                        else if (getRelatedness(-1, -1) && !getRelatedness(-2, 0)) //vertical tube slope
                            assignmentID = 97;
                        else
                            assignmentID = 5;
                    }
                    else //thick
                    {
                        if (Assignments[41].Tiles.Count != 0) //floor slope
                        {
                            if (getRelatedness(1, 1)) //normal slope
                            {
                                if (Assignments[getRelatedness(0, 2) ? (getRelatedness(1, 2) ? (getRelatedness(-1, 2) ? 51 : 79) : 99) : 61].Tiles.Count != 0) //99 is a cheat, it will be false, there's no such tile
                                {
                                    assignmentID = 41;
                                    break;
                                }
                            }
                            else //downward slope at the end of the platform
                            {
                                if (getRelatedness(-1, -1) && !getRelatedness(-1, -2)) //slope continues above but is not outright wall
                                {
                                    if (!getRelatedness(0, 2))
                                    {
                                        if (Assignments[93].Tiles.Count != 0)
                                        {
                                            assignmentID = 41;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (getRelatedness(-1, 2) && Assignments[83].Tiles.Count != 0)
                                        {
                                            assignmentID = 41;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (Assignments[65].Tiles.Count != 0 && getRelatedness(-1, -1)) //wall slope
                        {
                            if (getRelatedness(-2, 0))
                            {
                                if (getRelatedness(-2, 1) && Assignments[64].Tiles.Count != 0)
                                {
                                    assignmentID = 65;
                                    break;
                                }
                            }
                            else
                            {
                                if (Assignments[66].Tiles.Count != 0)
                                {
                                    assignmentID = 65;
                                    break;
                                }
                            }
                        }
                        assignmentID = 2;
                    }
                    break;
                case 7: //LUD
                    if (getRelatedness(-1, -1))
                        assignmentID = getRelatedness(-1, 1) ? (!getRelatedness(1, -1) && !getRelatedness(0, -2) && getRelatedness(-1, -2) && !getRelatedness(-1, -3) ? 83 : 12) : (getRelatedness(-2, 0) ? 27 : 56);
                    else
                        assignmentID = getRelatedness(-1, 1) ? (getRelatedness(1, -1) || getRelatedness(0, -2) ? (getRelatedness(-2, 0) ? 37 : 46) : 90) : 15;
                    break;
                case 8: //R
                    assignmentID = 30;
                    break;
                case 9: //RU
                    if (!getRelatedness(1, -1)) //thin
                    {
                        if (getRelatedness(-1, -1) && !getRelatedness(0, -2) && Assignments[95].Tiles.Count != 0) //horizontal tube slope
                            assignmentID = 95;
                        else if (getRelatedness(1, 1) && !getRelatedness(2, 0)) //vertical tube slope
                            assignmentID = 96;
                        else
                            assignmentID = 23;
                    }
                    else //thick
                    {
                        if (!getRelatedness(-1, -1) && !getRelatedness(0, -2) && getRelatedness(1, -2) && !getRelatedness(1, -3))
                        {
                            assignmentID = 92;
                            break;
                        }
                        if (Assignments[52].Tiles.Count != 0 && getRelatedness(-1, -1)) //ceiling slope
                        {
                            if (getRelatedness(0, -2))
                            {
                                if (getRelatedness(1, -2) && getRelatedness(-1, -2) && Assignments[42].Tiles.Count != 0)
                                {
                                    assignmentID = 52;
                                    break;
                                }
                            }
                            else
                            {
                                if (Assignments[62].Tiles.Count != 0)
                                {
                                    assignmentID = 52;
                                    break;
                                }
                            }
                        }
                        if (Assignments[54].Tiles.Count != 0 && getRelatedness(1, 1)) //wall slope
                        {
                            if (getRelatedness(2, 0))
                            {
                                if (getRelatedness(2, -1) && Assignments[55].Tiles.Count != 0)
                                {
                                    assignmentID = 54;
                                    break;
                                }
                            }
                            else
                            {
                                if (Assignments[56].Tiles.Count != 0)
                                {
                                    assignmentID = 54;
                                    break;
                                }
                            }
                        }
                        assignmentID = 20;
                    }
                    break;
                case 10: //RD
                    if (!getRelatedness(1, 1)) //thin
                    {
                        if (getRelatedness(-1, 1) && !getRelatedness(0, 2) && Assignments[84].Tiles.Count != 0) //horizontal tube slope
                            assignmentID = 84;
                        else if (getRelatedness(1, -1) && !getRelatedness(2, 0)) //vertical tube slope
                            assignmentID = 86;
                        else
                            assignmentID = 3;
                    }
                    else //thick
                    {
                        if (Assignments[40].Tiles.Count != 0) //floor slope
                        {
                            if (getRelatedness(-1, 1)) //normal slope
                            {
                                if (Assignments[getRelatedness(0, 2) ? (getRelatedness(-1, 2) ? (getRelatedness(1, 2) ? 50 : 78) : 99) : 60].Tiles.Count != 0)
                                {
                                    assignmentID = 40;
                                    break;
                                }
                            }
                            else //downward slope at the end of the platform
                            {
                                if (getRelatedness(1, -1) && !getRelatedness(1, -2)) //slope continues above but is not outright wall
                                {
                                    if (!getRelatedness(0, 2))
                                    {
                                        if (Assignments[92].Tiles.Count != 0)
                                        {
                                            assignmentID = 40;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (getRelatedness(1, 2) && Assignments[82].Tiles.Count != 0)
                                        {
                                            assignmentID = 40;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (Assignments[44].Tiles.Count != 0 && getRelatedness(1, -1)) //wall slope
                        {
                            if (getRelatedness(2, 0))
                            {
                                if (getRelatedness(2, 1) && Assignments[45].Tiles.Count != 0)
                                {
                                    assignmentID = 44;
                                    break;
                                }
                            }
                            else
                            {
                                if (Assignments[46].Tiles.Count != 0)
                                {
                                    assignmentID = 44;
                                    break;
                                }
                            }
                        }
                        assignmentID = 0;
                    }
                    break;
                case 11: //RUD
                    if (getRelatedness(1, -1))
                        assignmentID = getRelatedness(1, 1) ? (!getRelatedness(-1, -1) && !getRelatedness(0, -2) && getRelatedness(1, -2) && !getRelatedness(1, -3) ? 82 : 10) : (getRelatedness(2, 0) ? 26 : 76);
                    else
                        assignmentID = getRelatedness(1, 1) ? (getRelatedness(-1, -1) || getRelatedness(0, -2) ? (getRelatedness(2, 0) ? 36 : 66) : 91) : 13;
                    break;
                case 12: //LR
                    assignmentID = 31;
                    break;
                case 13: //LRU
                    if (getRelatedness(-1, -1))
                        assignmentID = getRelatedness(1, -1) ? 21 : (getRelatedness(0, -2) ? 18 : 61);
                    else
                        assignmentID = getRelatedness(1, -1) ? (getRelatedness(0, -2) ? 19 : 60) : 24;
                    break;
                case 14: //LRD
                    if (getRelatedness(-1, 1))
                        assignmentID = getRelatedness(1, 1) ? 1 : (getRelatedness(0, 2) ? 8 : 63);
                    else
                        assignmentID = getRelatedness(1, 1) ? (getRelatedness(0, 2) ? 9 : 62) : 4;
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
                            assignmentID = (getRelatedness(-1, -1) && !getRelatedness(0,-2)) ? 79 : 69;
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
                            assignmentID = (getRelatedness(1, -1) && !getRelatedness(0, -2)) ? 78 : 68;
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

            ResolveAssignmentIDToSomethingWithTilesInIt(ref assignmentID);

            bool lastRuleApplied = true;
            foreach (Rule rule in Assignments[assignmentID].Rules)
            {
                List<ushort> frames = rule.Result;
                bool applies = lastRuleApplied && rule.Applies(localTiles, Tileset.SmartTiles, TilesICanPlace.Comparer);
                if (frames.Count == 0) //and
                {
                    lastRuleApplied = applies;
                }
                else //then
                {
                    if (applies)
                    {
                        tileID = frames[frames.Count == 1 ? 0 : Rand.Next(frames.Count)];
                        return true;
                    }
                    lastRuleApplied = true;
                }
            }

            var tiles = Assignments[assignmentID].Tiles;
            if (tiles.Count >= 1)
                tileID = tiles[tiles.Count == 1 ? 0 : Rand.Next(tiles.Count)];
            else
                return false;
            return true;
        }

        private void ResolveAssignmentIDToSomethingWithTilesInIt(ref int assignmentID)
        {
            while (Assignments[assignmentID].Tiles.Count == 0)
            {
                ushort[] alternatives = AlternativeAssignments[assignmentID];
                int alternativeID = 0;
                for (; alternativeID < alternatives.Length - 1; ++alternativeID)
                    if (!Assignments[alternatives[alternativeID]].Empty)
                        break;
                assignmentID = alternatives[alternativeID];
            }
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
            if (TilesICanPlace.Contains(tileID, TilesICanPlace.Comparer)) //in a symmetrical assignment
                return true; //but don't alter
            return false;
        }
    }
    partial class Mainframe
    {
        List<SmartTile> SmartTiles = new List<SmartTile>();
        private void SmartFlipTile(ref ushort tileID, bool vertical)
        {
            foreach (MLLE.SmartTile smartTile in SmartTiles)
                if (smartTile.SmartFlipTile(ref tileID, vertical))
                    return;
            //if it can't be found
            if (!vertical)
                tileID ^= (ushort)J2L.MaxTiles;
            else
                tileID ^= 0x2000;
        }

        private void LoadSmartTiles(bool all)
        {
            if (J2L.HasTiles)
            {
                bool maxTiles4096 = J2L.MaxTiles == 4096;
                int max = all ? J2L.Tilesets.Count : 1;
                uint previousTileCount = 0;
                for (int i = 0; i < max; ++i)
                {
                    J2L.Tilesets[i].LoadSmartTiles(maxTiles4096, previousTileCount);
                    previousTileCount += J2L.Tilesets[i].TileCount;
                }
            }

            SmartTiles.Clear();
            SmartTile zero = new SmartTile(J2L.MaxTiles == 4096, null);
            zero.Assignments[11].Tiles.Add(0);
            zero.TilesICanPlace.Add(0);
            zero.PreviewTileIDs = new ushort[9];
            zero.Name = "(Blank)";
            SmartTiles.Add(zero);
            foreach (J2TFile tileset in J2L.Tilesets)
                SmartTiles.AddRange(tileset.SmartTiles);
            OverSmartTiles.Visible = SmartTiles.Count > 1;
        }
    }
}

partial class J2TFile
{
    internal List<MLLE.SmartTile> SmartTiles = new List<MLLE.SmartTile>();
    internal bool LoadSmartTiles(bool maxTiles4096, uint tileOffset)
    {
        SmartTiles.Clear();
        string filepath = Path.ChangeExtension(FullFilePath, ".MLLESet");
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
                tileOffset -= FirstTile;
                for (int numberOfSmartTiles = reader.ReadByte(); numberOfSmartTiles > 0; --numberOfSmartTiles)
                    SmartTiles.Add(new MLLE.SmartTile(maxTiles4096, this, version, reader, tileOffset));
                foreach (MLLE.SmartTile smartTile in SmartTiles)
                    smartTile.UpdateAllPossibleTiles(SmartTiles, false);
                foreach (MLLE.SmartTile smartTile in SmartTiles)
                    smartTile.UpdateTilesIGonextTo(SmartTiles);
            }
        }
        return success;
    }
    internal void SaveSmartTiles()
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(Path.ChangeExtension(FullFilePath, ".MLLESet"), FileMode.Create), J2File.FileEncoding)) {
            writer.Write((ushort)2); //version
            writer.Write((byte)SmartTiles.Count);
            foreach (MLLE.SmartTile smartTile in SmartTiles)
            {
                bool convertTo4096 = (smartTile.TilesICanPlace.Comparer as MLLE.SmartTile.ushortComparer).AndValue == 0x400 - 1;
                Action<List<ushort>> writeList = (tileIDs) =>
                {
                    writer.Write((byte)tileIDs.Count);
                    foreach (ushort tileID in tileIDs)
                    {
                        if (convertTo4096 && (tileID & 0x400) != 0)
                            writer.Write((ushort)(tileID ^ 0x1400));
                        else
                            writer.Write(tileID);
                    }
                }; 
                writer.Write(smartTile.Name);
                foreach (MLLE.SmartTile.Assignment assignment in smartTile.Assignments) //constant length (100), don't need to preface this with anything
                {
                    writeList(assignment.Tiles);
                    var rules = assignment.Rules;
                    writer.Write((byte)rules.Count);
                    foreach (MLLE.SmartTile.Rule rule in rules)
                    {
                        writer.Write((sbyte)rule.X);
                        writer.Write((sbyte)rule.Y);
                        writer.Write(rule.Not);
                        writer.Write((sbyte)rule.OtherSmartTileID);
                        if (rule.OtherSmartTileID == -1)
                            writeList(rule.SpecificTiles);
                        writeList(rule.Result);
                    }
                }
                writer.Write((byte)smartTile.Friends.Count);
                foreach (int friendID in smartTile.Friends)
                    writer.Write((byte)friendID);
            }
        }
    }
}
