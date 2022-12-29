using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Linq;
using Ionic.Zlib;
using System;

namespace MLLE
{
    public partial struct PlusPropertyList
    {

        [DescriptionAttribute("If False, the Team Trigger will be Off for Blue and On for Red.\nIf True, the Team Trigger will be On for Blue and Off for Red."),
        CategoryAttribute("Triggers")]
        public bool TeamTriggerOnForBlue { get; set; }

        private static void TryToSetTrigger(ref uint internalField, string value)
        {
            uint tryParse;
            if (uint.TryParse(value, out tryParse) && tryParse <= 31)
                internalField = tryParse + 1;
            else
                internalField = 0;
        }
        private static string GetStringRepresentationOfTrigger(uint internalField)
        {
            return (internalField > 0) ?
                (internalField - 1).ToString() :
                "None"
            ;
        }
        private uint teamTrigger;
        [DescriptionAttribute("If an integer from 0-31, that trigger will base its status on each player's team."),
        CategoryAttribute("Triggers")]
        public string TeamTrigger
        {
            get { return GetStringRepresentationOfTrigger(teamTrigger); }
            set { TryToSetTrigger(ref teamTrigger, value); }
        }

        private uint serverTrigger;
        [DescriptionAttribute("If an integer from 0-31, that trigger will be On for the server and Off for all clients."),
        CategoryAttribute("Triggers")]
        public string ServerTrigger
        {
            get { return GetStringRepresentationOfTrigger(serverTrigger); }
            set { TryToSetTrigger(ref serverTrigger, value); }
        }

        private uint overtimeTrigger;
        [DescriptionAttribute("If an integer from 0-31, that trigger will turn On at the beginning of Overtime."),
        CategoryAttribute("Triggers")]
        public string OvertimeTrigger
        {
            get { return GetStringRepresentationOfTrigger(overtimeTrigger); }
            set { TryToSetTrigger(ref overtimeTrigger, value); }
        }

        public class TriggerIDListConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
            {
                if (destinationType == typeof(System.String) && value is TriggerIDList)
                {
                    var triggers = ((TriggerIDList)value).Triggers;

                    string result = "";
                    for (uint i = 0; i < 32; ++i)
                    {
                        if (triggers[i])
                        {
                            if (result.Length > 0)
                                result += ",";
                            result += i.ToString();
                        }
                    }
                    if (result.Length > 0)
                        return result;
                    return "None";
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        [TypeConverterAttribute(typeof(TriggerIDListConverter))]
        public class TriggerIDList
        {
            public bool[] Triggers = new bool[32];
            public bool Trigger00 { get { return Triggers[0]; } set { Triggers[0] = value; } }
            public bool Trigger01 { get { return Triggers[1]; } set { Triggers[1] = value; } }
            public bool Trigger02 { get { return Triggers[2]; } set { Triggers[2] = value; } }
            public bool Trigger03 { get { return Triggers[3]; } set { Triggers[3] = value; } }
            public bool Trigger04 { get { return Triggers[4]; } set { Triggers[4] = value; } }
            public bool Trigger05 { get { return Triggers[5]; } set { Triggers[5] = value; } }
            public bool Trigger06 { get { return Triggers[6]; } set { Triggers[6] = value; } }
            public bool Trigger07 { get { return Triggers[7]; } set { Triggers[7] = value; } }
            public bool Trigger08 { get { return Triggers[8]; } set { Triggers[8] = value; } }
            public bool Trigger09 { get { return Triggers[9]; } set { Triggers[9] = value; } }
            public bool Trigger10 { get { return Triggers[10]; } set { Triggers[10] = value; } }
            public bool Trigger11 { get { return Triggers[11]; } set { Triggers[11] = value; } }
            public bool Trigger12 { get { return Triggers[12]; } set { Triggers[12] = value; } }
            public bool Trigger13 { get { return Triggers[13]; } set { Triggers[13] = value; } }
            public bool Trigger14 { get { return Triggers[14]; } set { Triggers[14] = value; } }
            public bool Trigger15 { get { return Triggers[15]; } set { Triggers[15] = value; } }
            public bool Trigger16 { get { return Triggers[16]; } set { Triggers[16] = value; } }
            public bool Trigger17 { get { return Triggers[17]; } set { Triggers[17] = value; } }
            public bool Trigger18 { get { return Triggers[18]; } set { Triggers[18] = value; } }
            public bool Trigger19 { get { return Triggers[19]; } set { Triggers[19] = value; } }
            public bool Trigger20 { get { return Triggers[20]; } set { Triggers[20] = value; } }
            public bool Trigger21 { get { return Triggers[21]; } set { Triggers[21] = value; } }
            public bool Trigger22 { get { return Triggers[22]; } set { Triggers[22] = value; } }
            public bool Trigger23 { get { return Triggers[23]; } set { Triggers[23] = value; } }
            public bool Trigger24 { get { return Triggers[24]; } set { Triggers[24] = value; } }
            public bool Trigger25 { get { return Triggers[25]; } set { Triggers[25] = value; } }
            public bool Trigger26 { get { return Triggers[26]; } set { Triggers[26] = value; } }
            public bool Trigger27 { get { return Triggers[27]; } set { Triggers[27] = value; } }
            public bool Trigger28 { get { return Triggers[28]; } set { Triggers[28] = value; } }
            public bool Trigger29 { get { return Triggers[29]; } set { Triggers[29] = value; } }
            public bool Trigger30 { get { return Triggers[30]; } set { Triggers[30] = value; } }
            public bool Trigger31 { get { return Triggers[31]; } set { Triggers[31] = value; } }

            public TriggerIDList(TriggerIDList other) {
                if (other == null)
                    return;
                for (uint i = 0; i < 32; ++i)
                    Triggers[i] = other.Triggers[i];
            }
        }
        private TriggerIDList startOffTriggers;
        [DescriptionAttribute("These Triggers will always start out as Off for clients who join servers, regardless of their current status for the host."),
        CategoryAttribute("Triggers")]
        public TriggerIDList StartOffTriggers { get { return startOffTriggers; } set { startOffTriggers = value; } }


        [DescriptionAttribute("Whether there's any active weather effect at the start of the level."),
        CategoryAttribute("Snow")]
        public bool IsSnowing { get; set; }

        [DescriptionAttribute("Whether the starting weather effect is specified to only take effect on transparent tiles, i.e. appear to be limited to outdoors areas."),
        CategoryAttribute("Snow")]
        public bool IsSnowingOutdoorsOnly { get; set; }

        [DescriptionAttribute("Intensity of the starting weather effect."),
        CategoryAttribute("Snow")]
        public byte SnowingIntensity { get; set; }

        public enum SnowTypeEnum
        {
            Snow, Flower, Rain, Leaf
        }
        [DescriptionAttribute("Type of the starting weather effect."),
        CategoryAttribute("Snow")]
        public SnowTypeEnum SnowingType { get; set; }



        public enum PitStyleEnum
        {
            FallForever = 0, InstantDeathPit = 255, StandOnPlatform = 1
        }
        [DescriptionAttribute("What should happen to players who fall to the bottom of the level?"),
        CategoryAttribute("Miscellaneous")]
        public PitStyleEnum PitStyle { get; set; }

        [DescriptionAttribute("If set to False, using a coin warp in Single Player mode will not turn all remaining coins into red and green gems."),
        CategoryAttribute("Miscellaneous")]
        public bool WarpsTransmuteCoins { get; set; }

        [DescriptionAttribute("If set to True, box objects (trigger crates, all wooden crates, bird morph monitors, and also bird cages) spawned from Generator objects will derive their parameters from the tile they begin at, not the tile they are created at. If the Generator object is in the air, the crate will appear on top of the nearest solid tile below the Generator, and will get its parameters from the tile there."),
        CategoryAttribute("Miscellaneous")]
        public bool DelayGeneratedCrateOrigins { get; set; }

        [DescriptionAttribute("The current degree of echo, as also can be set by the \"Echo\" event."),
        CategoryAttribute("Miscellaneous")]
        public int Echo { get; set; }

        [DescriptionAttribute("The color of darkness used with ambient lighting."),
        CategoryAttribute("Miscellaneous")]
        public Color DarknessColor { get; set; }

        
        private static void TryToSetOptionalByte(ref Int16 internalField, string value)
        {
            byte tryParse;
            if (byte.TryParse(value, out tryParse))
                internalField = tryParse;
            else
                internalField = -1;
        }
        private static string GetStringRepresentationOfOptionalByte(Int16 internalField)
        {
            return (internalField >= 0) ?
                (internalField).ToString() :
                "None"
            ;
        }
        private Int16 weaponHookPrefix;
        [DescriptionAttribute("If a number from 0 to 255, this value will be used as a uint8 prefix for every packet sent by any custom weapons in this level that send packets, which should allow you to distinguish them from other packets generated by your level script."),
        CategoryAttribute("Miscellaneous")]
        public string WeaponHookPrefix
        {
            get { return GetStringRepresentationOfOptionalByte(weaponHookPrefix); }
            set { TryToSetOptionalByte(ref weaponHookPrefix, value); }
        }




        [DescriptionAttribute("How fast water moves up or down when the water level is set (by event or AngelScript function) with the \"Instant\" parameter set to false."),
        CategoryAttribute("Water")]
        public float WaterChangeSpeed { get; set; }

        public enum WaterInteractionEnum
        {
            PositionBased, Swim, LowGravity
        }
        [DescriptionAttribute("How local players should react to being underwater. If this property is set to Swim, they will swim; if LowGravity, they will use regular physics but will fall more slowly than usual. If this property is set to PositionBased, the game will choose between the effects of Swim or LowGravity depending on whether the water level is lower or greater than 128 tiles. This property has no effects on other objects or on sound effects, which always move more slowly/sound different underwater."),
        CategoryAttribute("Water")]
        public WaterInteractionEnum WaterInteraction { get; set; }

        [DescriptionAttribute("Which layer, 1-8, water is drawn in front of when visible. Set to any non-existing layer number to make water invisible. Note that this is a purely visual setting, and putting water behind the sprite layer will not prevent players from swimming in it.\nIf the order of layers has been changed, this property's distance from 4 is its distance from the sprite layer, e.g. leaving it at 1 means that it will be drawn in front of the third layer in front of the sprite layer. (And therefore, if the sprite layer is the first, second, or third layer in the drawing order, water will not be drawn at all.)"),
        CategoryAttribute("Water")]
        public int WaterLayer { get; set; }

        public enum WaterLightingEnum
        {
            None, Global, Lagunicus = 3
        }
        [DescriptionAttribute("How water and ambient lighting should interact in the level."),
        CategoryAttribute("Water")]
        public WaterLightingEnum WaterLighting { get; set; }

        [DescriptionAttribute("How high the water should start at, in pixels."),
        CategoryAttribute("Water")]
        public float WaterLevel { get; set; }

        [DescriptionAttribute("Changes the colors used by water in 16-bit color. Leave both this and WaterGradientStop at Black to use the default colors."),
        CategoryAttribute("Water")]
        public Color WaterGradientStart { get; set; }

        [DescriptionAttribute("Changes the colors used by water in 16-bit color. Leave both this and WaterGradientStop at Black to use the default colors."),
        CategoryAttribute("Water")]
        public Color WaterGradientStop { get; set; }


        
        [DescriptionAttribute("Which event (if >32) will be used as a +15 Ammo Crate for weapon #7."),
        CategoryAttribute("Missing Objects")]
        public byte Gun7Crate { get; set; }
        [DescriptionAttribute("Which event (if >32) will be used as a +15 Ammo Crate for weapon #8."),
        CategoryAttribute("Missing Objects")]
        public byte Gun8Crate { get; set; }
        [DescriptionAttribute("Which event (if >32) will be used as a +15 Ammo Crate for weapon #9."),
        CategoryAttribute("Missing Objects")]
        public byte Gun9Crate { get; set; }


        [Browsable(false)]
        internal Palette Palette;
        [Browsable(false)]
        internal bool ReapplyPalette;


        public class NamedPalette
        {
            internal Palette Palette = new Palette();
            internal string Name;
        }
        [Browsable(false)]
        internal List<NamedPalette> NamedPalettes;


        [Browsable(false)]
        internal byte[][] ColorRemappings;


        [Browsable(false)]
        internal byte[][] TileImages;
        [Browsable(false)]
        internal byte[][] TileMasks;

        internal class OffGridObject
        {
            internal Point location;
            internal uint bits;
            public OffGridObject(OffGridObject other)
            {
                if (other == null)
                    return;
                location = new Point(other.location.X, other.location.Y);
                bits = other.bits;
            }
            public OffGridObject(Point l, uint b)
            {
                location = l;
                bits = b;
            }
        }
        internal List<OffGridObject> OffGridObjects;


        internal class Weapon
        {
            public const int NumberOfCommonOptions = 6;

            internal string Name;
            internal int[] Options;
            internal Weapon() { }
            internal Weapon(string n, int[] o) { Name = n; Options = o; }

            internal Weapon Clone()
            {
                return new Weapon(Name, Options.Clone() as int[]);
            }
            public override bool Equals(object other)
            {
                if (other == null || GetType() != other.GetType())
                    return false;

                Weapon otherWeapon = other as Weapon;
                return Name.Equals(otherWeapon.Name) && Options.SequenceEqual(otherWeapon.Options);
            }
        }
        static readonly Weapon[] WeaponDefaults = new Weapon[9] {
            new Weapon("Blaster",           new int[]{-1,1,0, 3, 3, 3}),
            new Weapon("Bouncer",           new int[]{-1,0,1, 3, 3, 0,  0,1}),
            new Weapon("Ice",               new int[]{-1,0,1, 3, 3, 0,  1}),
            new Weapon("Seeker",            new int[]{-1,0,1,10,10, 0}),
            new Weapon("RFs",               new int[]{-1,0,1,10,10, 0}),
            new Weapon("Toaster",           new int[]{-1,0,1, 3, 3, 0}),
            new Weapon("TNT",               new int[]{-1,0,0,10,10, 0,  0}),
            new Weapon("Gun8",              new int[]{-1,0,0,10,10, 0,  0}),
            new Weapon("Electro Blaster",   new int[]{-1,0,0,10,10, 0})
        };
        [Browsable(false)]
        internal Weapon[] Weapons;

        [Browsable(false)]
        internal string CommandLineArguments;

        const float DefaultWaterLevel = 0x7FFF;
        public PlusPropertyList(PlusPropertyList? other) : this()
        {
            ColorRemappings = new byte[Mainframe.RecolorableSpriteNames.Length][];
            TileImages = new byte[4096][];
            TileMasks = new byte[4096][];

            if (other.HasValue)
            {
                this = other.Value;

                startOffTriggers = new TriggerIDList(other.Value.startOffTriggers);

                if (other.Value.Palette == null)
                    Palette = null;
                else
                {
                    Palette = new Palette();
                    Palette.CopyFrom(other.Value.Palette);
                }
                NamedPalettes = other.Value.NamedPalettes.Select(o => new NamedPalette{ Name = o.Name, Palette = o.Palette }).ToList();

                for (int i = 0; i < other.Value.ColorRemappings.Length; ++i)
                    if (other.Value.ColorRemappings[i] != null)
                        ColorRemappings[i] = other.Value.ColorRemappings[i].Clone() as byte[];

                for (int i = 0; i < TileImages.Length; ++i)
                {
                    if (other.Value.TileImages[i] == null)
                        TileImages[i] = null;
                    else
                    {
                        TileImages[i] = other.Value.TileImages[i].Clone() as byte[];
                    }
                }

                for (int i = 0; i < TileMasks.Length; ++i)
                {
                    if (other.Value.TileMasks[i] == null)
                        TileMasks[i] = null;
                    else
                    {
                        TileMasks[i] = other.Value.TileMasks[i].Clone() as byte[];
                    }
                }

                Weapons = other.Value.Weapons.Select(w => w.Clone()).ToArray();
                OffGridObjects = other.Value.OffGridObjects.Select(o => new OffGridObject(o)).ToList();
                CommandLineArguments = other.Value.CommandLineArguments;
            }
            else
            {
                startOffTriggers = new TriggerIDList(null);
                DarknessColor = Color.Black;
                WarpsTransmuteCoins = true;
                WaterChangeSpeed = 1;
                WaterLayer = 1;
                WaterLevel = DefaultWaterLevel;
                WaterGradientStart = Color.Black;
                WaterGradientStop = Color.Black;
                Gun7Crate = Gun7Crate = Gun9Crate = 0;
                Palette = null;
                NamedPalettes = new List<NamedPalette>();
                ReapplyPalette = true;
                Weapons = WeaponDefaults.Select(w => w.Clone()).ToArray();
                weaponHookPrefix = -1;
                OffGridObjects = new List<OffGridObject>();
                CommandLineArguments = string.Empty;
            }
        }

        const uint TriggerZone = 246;
        public void ReadFromEventMap(uint[,] EventMap)
        {
            var BottomEventRow = EventMap.GetLength(1) - 1;
            var RightEventColumn = EventMap.GetLength(0) - 1;
            { //set simple triggers
                uint PotentialTriggerZone = EventMap[0, BottomEventRow];
                if ((PotentialTriggerZone & 0xFFu) == TriggerZone)
                {
                    teamTrigger = ((PotentialTriggerZone >> 12) & 31) + 1;
                    TeamTriggerOnForBlue = ((PotentialTriggerZone >> 17) & 1) != 0;
                }
                else
                {
                    teamTrigger = 0;
                    TeamTriggerOnForBlue = false; //this line may not be needed but it can't hurt
                }

                PotentialTriggerZone = EventMap[1, BottomEventRow];
                if ((PotentialTriggerZone & 0xFFu) == TriggerZone)
                    serverTrigger = ((PotentialTriggerZone >> 12) & 31) + 1;
                else
                    serverTrigger = 0;

                PotentialTriggerZone = EventMap[2, BottomEventRow];
                if ((PotentialTriggerZone & 0xFFu) == TriggerZone)
                    overtimeTrigger = ((PotentialTriggerZone >> 12) & 31) + 1;
                else
                    overtimeTrigger = 0;
            } { //set pit style
                uint PitEvent = EventMap[RightEventColumn, BottomEventRow] & 0xFFu;
                switch (PitEvent)
                {
                    case (uint)PitStyleEnum.InstantDeathPit:
                    case (uint)PitStyleEnum.StandOnPlatform:
                        PitStyle = (PitStyleEnum)PitEvent;
                        break;
                    default:
                        PitStyle = PitStyleEnum.FallForever;
                        break;
                }
            } { //set start-off triggers
                for (uint i = 0; i < 32; ++i)
                    startOffTriggers.Triggers[i] = false;
                while (--RightEventColumn > 2) //columns 0,1,2, are reserved for other purposes and may not therefore be interpreted as Starts Off
                {
                    uint PotentialTriggerZone = EventMap[RightEventColumn, BottomEventRow];
                    if ((PotentialTriggerZone & 0xFFu) == TriggerZone)
                        startOffTriggers.Triggers[(PotentialTriggerZone >> 12) & 31] = true;
                    //else
                    //    break;
                }
            }
        }
        public void WriteToEventMap(uint[,] EventMap)
        {
            var BottomEventRow = EventMap.GetLength(1) - 1;
            var RightEventColumn = EventMap.GetLength(0) - 1;
            { //set simple triggers
                EventMap[0, BottomEventRow] =
                    (teamTrigger == 0) ?
                        0 :
                        TriggerZone | ((teamTrigger - 1) << 12) | (uint)(TeamTriggerOnForBlue ? (1 << 17) : 0)
                ;
                EventMap[1, BottomEventRow] =
                    (serverTrigger == 0) ?
                        0 :
                        TriggerZone | ((serverTrigger - 1) << 12)
                ;
                EventMap[2, BottomEventRow] =
                    (overtimeTrigger == 0) ?
                        0 :
                        TriggerZone | ((overtimeTrigger - 1) << 12)
                ;
            }
            { //set pit style
                EventMap[RightEventColumn, BottomEventRow] = (uint)PitStyle;
            }
            { //set start-off triggers
                for (uint x = 3; x < RightEventColumn; ++x)
                    EventMap[x, BottomEventRow] = 0; //remove any old trigger zones before adding the current set
                for (uint i = 0; i < 32; ++i)
                    if (startOffTriggers.Triggers[i])
                    {
                        if (--RightEventColumn <= 2)
                        {
                            MessageBox.Show("Layer 4 is not wide enough to define that many different Start Off triggerIDs.", "Level not wide enough", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }
                        EventMap[RightEventColumn, BottomEventRow] = TriggerZone | (i << 12);
                    }
            }
        }

        internal bool LevelNeedsData5
        {
            get
            {
                var comparableBlack = Color.Black.ToArgb();
                if (
                    IsSnowing ||
                    IsSnowingOutdoorsOnly ||
                    SnowingIntensity != 0 ||
                    SnowingType != SnowTypeEnum.Snow ||
                    !WarpsTransmuteCoins ||
                    DelayGeneratedCrateOrigins ||
                    Echo != 0 ||
                    DarknessColor.ToArgb() != comparableBlack ||
                    WaterChangeSpeed != 1 ||
                    WaterInteraction != WaterInteractionEnum.PositionBased ||
                    WaterLayer != 1 ||
                    WaterLighting != WaterLightingEnum.None ||
                    WaterLevel != DefaultWaterLevel ||
                    WaterGradientStart.ToArgb() != comparableBlack ||
                    WaterGradientStop.ToArgb() != comparableBlack ||
                    Gun7Crate != 0 || Gun8Crate != 0 || Gun9Crate != 0 ||
                    Palette != null ||
                    NamedPalettes.Count != 0 ||
                    //ReapplyPalette only is meaningful if Palette != null, so we don't check it separately
                    ColorRemappings.FirstOrDefault(it => it != null) != null ||
                    TileImages.FirstOrDefault(it => it != null) != null ||
                    TileMasks.FirstOrDefault(it => it != null) != null ||
                    weaponHookPrefix >= 0 ||
                    !Weapons.SequenceEqual(WeaponDefaults) ||
                    OffGridObjects.Count != 0
                    //CommandLineArguments is stored in the .j2as, not the .j2l, so it doesn't get checked here
                )
                    return true;
                return false;
            }
        }
        internal bool CreateData5Section(out byte[] Data5, out WeaponsForm.ExtendedWeapon[] CustomWeapons, List<J2TFile> Tilesets, List<Layer> Layers, uint[,] EventMap)
        {
            Data5 = null;
            CustomWeapons = new WeaponsForm.ExtendedWeapon[9];
            var data5header = new MemoryStream();

            using (MemoryStream data5body = new MemoryStream())
            using (BinaryWriter data5writer = new BinaryWriter(data5header, J2LFile.FileEncoding))
            using (BinaryWriter data5bodywriter = new BinaryWriter(data5body, J2LFile.FileEncoding))
            {
                data5writer.Write(MLLEData5MagicString.ToCharArray());
                data5writer.Write(CurrentMLLEData5Version);

                data5bodywriter.Write(IsSnowing);
                data5bodywriter.Write(IsSnowingOutdoorsOnly);
                data5bodywriter.Write(SnowingIntensity);
                data5bodywriter.Write((byte)SnowingType);
                data5bodywriter.Write(WarpsTransmuteCoins);
                data5bodywriter.Write(DelayGeneratedCrateOrigins);
                data5bodywriter.Write(Echo);
                data5bodywriter.Write(DarknessColor.ToArgb());
                data5bodywriter.Write(WaterChangeSpeed);
                data5bodywriter.Write((byte)WaterInteraction);
                data5bodywriter.Write(WaterLayer);
                data5bodywriter.Write((byte)WaterLighting);
                data5bodywriter.Write(WaterLevel);
                data5bodywriter.Write(WaterGradientStart.ToArgb());
                data5bodywriter.Write(WaterGradientStop.ToArgb());

                data5bodywriter.Write(Palette != null);
                if (Palette != null) {
                    Palette.WriteLEVStyle(data5bodywriter);
                    data5bodywriter.Write(ReapplyPalette);
                }

                data5bodywriter.Write((byte)NamedPalettes.Count);
                foreach (NamedPalette namedPalette in NamedPalettes)
                {
                    data5bodywriter.Write(namedPalette.Name);
                    namedPalette.Palette.WriteLEVStyle(data5bodywriter);
                }

                foreach (byte[] remappings in ColorRemappings)
                {
                    if (remappings == null)
                        data5bodywriter.Write(false);
                    else
                    {
                        data5bodywriter.Write(true);
                        for (int i = 0; i < remappings.Length; ++i)
                            data5bodywriter.Write(remappings[i]);
                    }
                }

                data5bodywriter.Write((byte)(Tilesets.Count - 1));
                for (int tilesetID = 1; tilesetID < Tilesets.Count; ++tilesetID) //Tilesets[0] is already mentioned in Data1, after all
                {
                    var tileset = Tilesets[tilesetID];
                    data5bodywriter.Write(Path.ChangeExtension(tileset.FilenameOnly, ".j2t")); //convert "J2T" to "j2t" so JJ2+ doesn't reject the jjTilesFromTileset call
                    data5bodywriter.Write((ushort)tileset.FirstTile);
                    data5bodywriter.Write((ushort)tileset.TileCount);
                    byte[] remappings = tileset.ColorRemapping;
                    data5bodywriter.Write(remappings != null);
                    if (remappings != null)
                        for (int i = 0; i < remappings.Length; ++i)
                            data5bodywriter.Write(remappings[i]);
                }

                data5bodywriter.Write((uint)Layers.Count);
                foreach (Layer layer in Layers)
                {
                    data5bodywriter.Write((sbyte)layer.id);
                    data5bodywriter.Write(layer.Name);
                    data5bodywriter.Write(layer.Hidden);
                    data5bodywriter.Write(layer.SpriteMode);
                    data5bodywriter.Write(layer.SpriteParam);
                    data5bodywriter.Write(layer.RotationAngle);
                    data5bodywriter.Write(layer.RotationRadiusMultiplier);
                    data5bodywriter.Write(layer.XSpeedModel);
                    data5bodywriter.Write(layer.YSpeedModel);
                    data5bodywriter.Write(layer.TextureSurface);
                    data5bodywriter.Write(layer.Fade);
                    data5bodywriter.Write(layer.XFade);
                    data5bodywriter.Write(layer.YFade);
                    data5bodywriter.Write(layer.InnerX);
                    data5bodywriter.Write(layer.InnerY);
                    data5bodywriter.Write(layer.InnerAutoX);
                    data5bodywriter.Write(layer.InnerAutoY);
                    data5bodywriter.Write(layer.Texture);
                    if (layer.Texture < 0)
                        data5bodywriter.Write(layer.TextureImage);
                }

                int levelTileCount = Tilesets.Sum(ts => (int)ts.TileCount);
                foreach (byte[][] images in new byte[][][] { TileImages, TileMasks }) {
                    int numberOfImages = images.Take(levelTileCount).Count(it => it != null);
                    data5bodywriter.Write((ushort)numberOfImages);
                    if (numberOfImages > 0)
                        for (int i = 1; i < levelTileCount; ++i)
                        {
                            byte[] image = images[i];
                            if (image != null)
                            {
                                data5bodywriter.Write((ushort)i);
                                data5bodywriter.Write(image);
                            }
                        }
                }

                data5bodywriter.Write(weaponHookPrefix);
                for (int weaponID = 0; weaponID < Weapons.Length; ++weaponID)
                {
                    Weapon weapon = Weapons[weaponID];
                    Weapon defaultWeapon = WeaponDefaults[weaponID];
                    bool isGun8InSlot8 = weaponID == 7 && weapon.Name == "Gun8"; //hardcoded laxer standards... parameter need not match
                    bool isCustom = !isGun8InSlot8 && !(weapon.Name.Equals(defaultWeapon.Name) && weapon.Options.Skip(Weapon.NumberOfCommonOptions).SequenceEqual(defaultWeapon.Options.Skip(Weapon.NumberOfCommonOptions)));
                    data5bodywriter.Write(isCustom);
                    int[] options = weapon.Options;
                    data5bodywriter.Write(options[0]);
                    data5bodywriter.Write((byte)options[1]);
                    data5bodywriter.Write(options[2] != 0);
                    data5bodywriter.Write((byte)options[3]);
                    data5bodywriter.Write((byte)options[4]);
                    data5bodywriter.Write((byte)options[5]);
                    if (weaponID == 6)
                        data5bodywriter.Write(Gun7Crate);
                    else if (weaponID == 7)
                        data5bodywriter.Write(Gun8Crate);
                    else if (weaponID == 8)
                        data5bodywriter.Write(Gun9Crate);
                    if (isGun8InSlot8)
                        data5bodywriter.Write((byte)options[Weapon.NumberOfCommonOptions]);
                    else if (isCustom)
                    {
                        data5bodywriter.Write(weapon.Name);
                        var extendedWeapon = WeaponsForm.GetAllAvailableWeapons().Find(w => w.Name == weapon.Name);
                        if (extendedWeapon == null)
                        {
                            MessageBox.Show(String.Format("Sorry, the MLLE \"Weapons\" folder did not include any .ini file defining a weapon with the name \"{0}.\"", weapon.Name), "Weapon not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                        CustomWeapons[weaponID] = extendedWeapon;
                        data5bodywriter.Write(extendedWeapon.OptionTypes.Skip(Weapon.NumberOfCommonOptions).Select(o => o == WeaponsForm.ExtendedWeapon.oTypes.Int ? sizeof(int) : sizeof(byte)).Sum());
                        for (int optionID = Weapon.NumberOfCommonOptions; optionID < options.Length; ++optionID)
                        {
                            switch (extendedWeapon.OptionTypes[optionID]) {
                                case WeaponsForm.ExtendedWeapon.oTypes.Bool:
                                    data5bodywriter.Write(options[optionID] != 0);
                                    break;
                                case WeaponsForm.ExtendedWeapon.oTypes.Dropdown:
                                    data5bodywriter.Write((byte)options[optionID]);
                                    break;
                                case WeaponsForm.ExtendedWeapon.oTypes.Int:
                                default:
                                    data5bodywriter.Write(options[optionID]);
                                    break;
                            }
                        }
                    }
                }

                int eventX = 0, eventY = 0;
                data5bodywriter.Write((ushort)OffGridObjects.Count);
                foreach (OffGridObject obj in OffGridObjects)
                {
                    data5bodywriter.Write((ushort)obj.location.X);
                    data5bodywriter.Write((ushort)obj.location.Y);
                    data5bodywriter.Write(obj.bits);
                    bool addedOffGridEventForServers = false;
                    do {
                        if (EventMap[eventX, eventY] == 0)
                        {
                            EventMap[eventX, eventY] = 0xFFFFF3FE;
                            addedOffGridEventForServers = true;
                        }
                        if (++eventX == EventMap.GetLength(0))
                        {
                            eventX = 0;
                            if (++eventY == EventMap.GetLength(1))
                            {
                                MessageBox.Show("Sorry, there were insufficient empty tiles in the level to save all the off-grid objects. In order to be able to save the level, you will need to make layer 4 larger, remove some off-grid objects, or remove some events.", "Not enough event space", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                        }
                    } while (!addedOffGridEventForServers);
                }

                var data5bodycompressed = ZlibStream.CompressBuffer(data5body.ToArray());
                data5writer.Write((uint)data5bodycompressed.Length);
                data5writer.Write((uint)data5body.Length);
                data5writer.Write(data5bodycompressed);
            }
            Data5 = data5header.ToArray();
            return true;
        }
        internal string LevelIsReadable(byte[] Data5, List<J2TFile> Tilesets, List<Layer> Layers, uint[,] EventMap, string Filepath)
        {
            if (Data5 == null || Data5.Length < 20) //level stops at the end of data4, as is good and proper
            {
                this = new PlusPropertyList(null);
                return null;
            }
            using (BinaryReader data5reader = new BinaryReader(new MemoryStream(Data5), J2LFile.FileEncoding))
            {
                if (new string(data5reader.ReadChars(4)) != MLLEData5MagicString)
                    return "This level was saved in some unknown editor software that this release of MLLE does not recognize. Resaving it might inadvertently corrupt data saved in it, so MLLE will not open it. You could try checking to see if there is a more recent MLLE release that adds support for this format.";
                uint data5Version = data5reader.ReadUInt32();
                if (data5Version > CurrentMLLEData5Version)
                    return "This level appears to have been saved in a more recent release of MLLE and uses one or more features that this release cannot read. Please try downloading the latest MLLE release and trying again.";
                //should be okay to read at this point
                
                this = new PlusPropertyList(null);

                uint csize = data5reader.ReadUInt32();
                uint usize = data5reader.ReadUInt32();
                using (BinaryReader data5bodyreader = new BinaryReader(new MemoryStream(ZlibStream.UncompressBuffer(data5reader.ReadBytes((int)csize)))))
                {
                    IsSnowing = data5bodyreader.ReadBoolean();
                    IsSnowingOutdoorsOnly = data5bodyreader.ReadBoolean();
                    SnowingIntensity = data5bodyreader.ReadByte();
                    SnowingType = (SnowTypeEnum)data5bodyreader.ReadByte();
                    WarpsTransmuteCoins = data5bodyreader.ReadBoolean();
                    DelayGeneratedCrateOrigins = data5bodyreader.ReadBoolean();
                    Echo = data5bodyreader.ReadInt32();
                    DarknessColor = Color.FromArgb(data5bodyreader.ReadInt32());
                    WaterChangeSpeed = data5bodyreader.ReadSingle();
                    WaterInteraction = (WaterInteractionEnum)data5bodyreader.ReadByte();
                    WaterLayer = data5bodyreader.ReadInt32();
                    WaterLighting = (WaterLightingEnum)data5bodyreader.ReadByte();
                    WaterLevel = data5bodyreader.ReadSingle();
                    WaterGradientStart = Color.FromArgb(data5bodyreader.ReadInt32());
                    WaterGradientStop = Color.FromArgb(data5bodyreader.ReadInt32());

                    if (data5bodyreader.ReadBoolean()) {
                        Palette = new Palette(data5bodyreader, true);
                        if (data5Version >= 0x106)
                            ReapplyPalette = data5bodyreader.ReadBoolean();
                        else
                            ReapplyPalette = false;
                    }

                    if (data5Version >= 0x106) //additional palettes (for SPRITE::MAPPING) were added in MLLE-Include-1.6
                    {
                        var extraPaletteCount = data5bodyreader.ReadByte();
                        while (extraPaletteCount-- != 0)
                        {
                            NamedPalettes.Add(new NamedPalette {
                                Name = data5bodyreader.ReadString(),
                                Palette = new Palette(data5bodyreader, true)
                            });
                        }
                    }

                    for (int i = 0; i < Mainframe.RecolorableSpriteNames.Length; ++i)
                        if ((i < 11 || data5Version >= 0x105) && data5bodyreader.ReadBoolean()) //the recolorable sprite list was expanded in MLLE-Include-1.5
                        {
                            ColorRemappings[i] = new byte[Palette.PaletteSize];
                            for (uint j = 0; j < Palette.PaletteSize; ++j)
                                ColorRemappings[i][j] = data5bodyreader.ReadByte();
                        }
                        else
                            ColorRemappings[i] = null;

                    var extraTilesetCount = data5bodyreader.ReadByte();
                    for (uint tilesetID = 0; tilesetID < extraTilesetCount; ++tilesetID)
                    {
                        var tilesetFilepath = Path.Combine(Path.GetDirectoryName(Filepath), data5bodyreader.ReadString());
                        if (File.Exists(tilesetFilepath))
                        {
                            var tileset = new J2TFile(tilesetFilepath);
                            tileset.FirstTile = data5bodyreader.ReadUInt16();
                            tileset.TileCount = data5bodyreader.ReadUInt16();
                            if (data5bodyreader.ReadBoolean())
                            {
                                tileset.ColorRemapping = new byte[Palette.PaletteSize];
                                for (uint i = 0; i < Palette.PaletteSize; ++i)
                                    tileset.ColorRemapping[i] = data5bodyreader.ReadByte();
                            }
                            Tilesets.Add(tileset);
                        }
                        else
                        {
                            this = new PlusPropertyList(null);
                            return "Additional tileset \"" + tilesetFilepath + "\" not found; MLLE will stop trying to read this Data5 section.";
                        }
                    }

                    if (data5Version >= 0x102) //layers were added in MLLE-Include-1.2
                    {
                        var defaultLayers = Layers.ToArray();
                        Layers.Clear();
                        int layerCount = (int)data5bodyreader.ReadUInt32();
                        List<Layer> nonDefaultLayers = new List<Layer>();
                        for (int i = 8; i < layerCount; i += 8)
                        {
                            J2LFile extraLevel = new J2LFile();
                            string levelFilePath = GetExtraDataLevelFilepath(Filepath, i / 8 - 1);
                            if (File.Exists(levelFilePath))
                            {
                                extraLevel.OpenLevel(levelFilePath, ref Data5, SecurityStringOverride: J2LFile.SecurityStringExtraDataNotForDirectEditing);
                                nonDefaultLayers.AddRange(extraLevel.DefaultLayers);
                            }
                            else
                            {
                                this = new PlusPropertyList(null);
                                return "Additional file \"" + levelFilePath + "\" not found; MLLE will stop trying to read this Data5 section.";
                            }
                        }
                        int nextNonDefaultLayerID = 0;
                        for (uint i = 0; i < layerCount; ++i)
                        {
                            sbyte layerID = data5bodyreader.ReadSByte();
                            Layer layer;
                            if (layerID >= 0)
                                layer = defaultLayers[layerID];
                            else
                            {
                                layer = nonDefaultLayers[nextNonDefaultLayerID++];
                                layer.id = -1;
                            }
                            layer.Name = data5bodyreader.ReadString();
                            layer.Hidden = data5bodyreader.ReadBoolean();
                            layer.SpriteMode = data5bodyreader.ReadByte();
                            layer.SpriteParam = data5bodyreader.ReadByte();
                            layer.RotationAngle = data5bodyreader.ReadInt32();
                            layer.RotationRadiusMultiplier = data5bodyreader.ReadInt32();
                            if (data5Version >= 0x106) //MLLE-Include-1.6 adds a bunch of layer properties from JJ2+ 5.10
                            {
                                layer.XSpeedModel = data5bodyreader.ReadByte();
                                layer.YSpeedModel = data5bodyreader.ReadByte();
                                layer.TextureSurface = data5bodyreader.ReadByte();
                                layer.Fade = data5bodyreader.ReadByte();
                                layer.XFade = data5bodyreader.ReadSingle();
                                layer.YFade = data5bodyreader.ReadSingle();
                                layer.InnerX = data5bodyreader.ReadSingle();
                                layer.InnerY = data5bodyreader.ReadSingle();
                                layer.InnerAutoX = data5bodyreader.ReadSingle();
                                layer.InnerAutoY = data5bodyreader.ReadSingle();
                                layer.Texture = data5bodyreader.ReadSByte();
                                if (layer.Texture < 0)
                                    layer.TextureImage = data5bodyreader.ReadBytes(256 * 256);
                            }
                            else if (layerID < 0) //don't assume that every eighth layer from a mlle-data level has layer 8 speed models (but the other default values are fine)
                            {
                                layer.XSpeedModel = layer.YSpeedModel = 0;
                            }
                            Layers.Add(layer);
                        }

                        if (data5Version >= 0x103) //edited tiles were added in MLLE-Include-1.3
                        {
                            int levelTileCount = Tilesets.Sum(ts => (int)ts.TileCount);
                            foreach (byte[][] images in new byte[][][] { TileImages, TileMasks })
                            {
                                int numberOfImages = data5bodyreader.ReadUInt16();
                                for (int i = 0; i < numberOfImages; ++i)
                                {
                                    int tileID = data5bodyreader.ReadUInt16();
                                    images[tileID] = data5bodyreader.ReadBytes(32 * 32);
                                }
                            }

                            if (data5Version >= 0x105) //weapons were added in MLLE-Include-1.5(w)
                            {
                                if (data5Version >= 0x106) //DefaultWeaponHook constructor argument was added in MLLE-Include-1.6(w)
                                    weaponHookPrefix = data5bodyreader.ReadInt16();
                                for (int weaponID = 0; weaponID < 9; ++weaponID)
                                {
                                    bool customWeapon = data5bodyreader.ReadBoolean();
                                    Weapon weapon = Weapons[weaponID];
                                    weapon.Options[0] = data5bodyreader.ReadInt32(); //maximum
                                    for (int optionID = 1; optionID < Weapon.NumberOfCommonOptions; ++optionID)
                                        weapon.Options[optionID] = data5bodyreader.ReadByte(); //birds and crates and gems and replenishment
                                    if (weaponID == 6)
                                        Gun7Crate = data5bodyreader.ReadByte();
                                    else if (weaponID == 7)
                                        Gun8Crate = data5bodyreader.ReadByte();
                                    else if (weaponID == 8)
                                        Gun9Crate = data5bodyreader.ReadByte();
                                    if (customWeapon)
                                    {
                                        string name = data5bodyreader.ReadString();
                                        var extendedWeapon = WeaponsForm.GetAllAvailableWeapons().Find(w => w.Name == name);
                                        if (extendedWeapon == null)
                                        {
                                            this = new PlusPropertyList(null);
                                            return "Sorry, the MLLE \"Weapons\" folder did not include any .ini file defining a weapon with the name \"" + name + ".\" You will need to download or otherwise obtain such a .ini file before you can open this level. Consider contacting the level's author for help.";
                                        }
                                        
                                        Weapons[weaponID].Name = name;
                                        Array.Resize(ref Weapons[weaponID].Options, extendedWeapon.Options.Length);
                                        data5bodyreader.ReadInt32(); //length of jjSTREAM
                                        for (int optionID = Weapon.NumberOfCommonOptions; optionID < weapon.Options.Length; ++optionID)
                                            switch (extendedWeapon.OptionTypes[optionID]) {
                                                case WeaponsForm.ExtendedWeapon.oTypes.Bool:
                                                case WeaponsForm.ExtendedWeapon.oTypes.Dropdown:
                                                    weapon.Options[optionID] = data5bodyreader.ReadByte();
                                                    break;
                                                case WeaponsForm.ExtendedWeapon.oTypes.Int:
                                                    weapon.Options[optionID] = data5bodyreader.ReadInt32();
                                                    break;
                                            }
                                    } else if (weaponID == 7)
                                        weapon.Options[Weapon.NumberOfCommonOptions] = data5bodyreader.ReadByte(); //Gun8 style
                                }

                                if (data5Version >= 0x106) //offgridobjects were added in MLLE-Include-1.6
                                {
                                    ushort objects = data5bodyreader.ReadUInt16();
                                    if (objects != 0)
                                    {
                                        for (int y = 0; y < EventMap.GetLength(1); y++)
                                            for (int x = 0; x < EventMap.GetLength(0); x++)
                                                if (EventMap[x, y] == 0xFFFFF3FE)
                                                    EventMap[x, y] = 0;
                                                else if (EventMap[x, y] == 0)
                                                    goto doneSearchingFor254;
                                        doneSearchingFor254:
                                        do {
                                            ushort x = data5bodyreader.ReadUInt16();
                                            ushort y = data5bodyreader.ReadUInt16();
                                            uint bits = data5bodyreader.ReadUInt32();
                                            OffGridObjects.Add(new OffGridObject(new Point(x, y), bits));
                                        } while (--objects != 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
