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
        }
        internal HashSet<ushort> AllPossibleTiles;

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
            AllPossibleTiles = new HashSet<ushort>(other.AllPossibleTiles.Comparer);
            AllPossibleTiles.UnionWith(other.AllPossibleTiles);
        }
        internal SmartTile(bool maxTiles4096, ushort fileVersion, BinaryReader reader) : this()
        {
            Name = reader.ReadString();
            foreach (List<ushort> assignment in TileAssignments)
                for (int numberOfTileIDs = reader.ReadByte(); numberOfTileIDs > 0; --numberOfTileIDs)
                {
                    ushort tileID = reader.ReadUInt16();
                    if (!maxTiles4096 && (tileID & 0x1000) != 0) //hflip
                        tileID ^= 0x1400;
                    assignment.Add(tileID);
                }
            AllPossibleTiles = new HashSet<ushort>(new ushortComparer((ushort)((maxTiles4096 ? 4096 : 1024) - 1)));
            UpdateAllPossibleTiles();
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
        public bool Apply(ref ushort result, ArrayMap<ushort> localTiles, bool directAction)
        {
            if (directAction || (!TileAssignments[35/*"extra"*/].Contains(result) && AllPossibleTiles.Contains(result, AllPossibleTiles.Comparer))) //otherwise this is outside the scope of this smart tile and should be left alone
            {
                ArrayMap<bool?> LocalTilesAreRelated = new ArrayMap<bool?>(5,5);
                Func<int, int, bool> getRelatedness = (x, y) =>
                {
                    x += 2;
                    y += 2;
                    return LocalTilesAreRelated[x, y] ?? (LocalTilesAreRelated[x, y] = AllPossibleTiles.Contains(localTiles[x, y], AllPossibleTiles.Comparer)).Value;
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
                    result = assignment[0];
                else if (assignment.Count > 1)
                    result = assignment[Rand.Next(assignment.Count)];
                else
                    return false;
                return true;
            }
            return false;
        }

        internal bool SmartFlipTile(ref ushort tileID, bool vertical)
        {
            var pairings = !vertical ? AssignmentPairings : AssignmentVerticalPairings;
            int numberOfPairings = pairings.GetLength(0);
            for (int i = 0; i < numberOfPairings; ++i)
                for (int j = 0; j < 2; ++j)
                    if (TileAssignments[pairings[i, j]].Contains(tileID))
                    {
                        List<ushort> flippedAssignment = TileAssignments[pairings[i, j ^ 1]];
                        if (flippedAssignment.Count > 0)
                        {
                            tileID = flippedAssignment[Rand.Next(flippedAssignment.Count)];
                            return true;
                        }
                    }
            if (AllPossibleTiles.Contains(tileID)) //in a symmetrical assignment
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
                if (version > 0) {
                    System.Windows.Forms.MessageBox.Show("The file \"" + filepath + "\" was not saved in a format that this version of MLLE understands. Please make sure you have the latest MLLE release.", "Incompatible File Version", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    success = false;
                } else {
                    for (int numberOfSmartTiles = reader.ReadByte(); numberOfSmartTiles > 0; --numberOfSmartTiles)
                        SmartTiles.Add(new SmartTile(J2L.MaxTiles == 4096, version, reader));
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
