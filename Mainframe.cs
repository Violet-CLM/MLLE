using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using TexLib;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Ini;
using Un4seen.Bass;
using Extra.Collections;

namespace MLLE
{
    public enum EnableableTitles { SecretLevelName, BonusLevelName, Lighting, Splitscreen, HideInHCL, Multiplayer, BoolDevelopingForPlus, UseText, SaveAndRun, SaveAndRunArgs, Illuminate, DiffLabel, Diff1, Diff2, Diff3, Diff4 }
    public enum FocusedZone { None, Tileset, Level, AnimationEditing }
    public enum SelectionType { New, Add, Subtract, Rectangle, HollowRectangle }
    public enum AtlasID { Null, Image, Mask, EventNames, Selection, Generator, TileTypes }
    public enum TilesetOverlay { None, TileTypes, Events, Masks }

    public partial class Mainframe : Form
    {
        #region variable declaration
        internal TexturedJ2L J2L;
        internal IniFile Settings = new IniFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLLE.ini"));
        bool SafeToDisplay = false;// ResizingOccured = false;
        List<string> AllTilesets;
        static string[][] DefaultHotKolors = new string[3][] { new string[3] { "32", "24", "80" }, new string[3] { "72", "48", "168" }, new string[3] { "79", "48", "168" } };
        Color[] HotKolors = new Color[3];
        Keys AddSelectionKey, SubtractSelectionKey;
        public Dictionary<Version, string> DefaultDirectories;
        public Dictionary<Version, NameAndFilename[]> AllTilesetLists = new Dictionary<Version, NameAndFilename[]> {
        {Version.BC, null },
        {Version.O, null },
        {Version.JJ2, null },
        {Version.TSF, null },
        {Version.AGA, null },
        {Version.GorH, null },
        };
        //internal int[] eventtexturenumberlist = new int[256];
        internal int GeneratorOverlay, SelectionOverlay;
        //internal string[][] EventsFromIni;
        internal List<string[]> TextureTypes = new List<string[]>(4);
        internal string[] CurrentIniLine;
        //public List<TreeNode> TreeNodeList = new List<TreeNode>();
        public Dictionary<Version, List<TreeNode>[]> TreeStructure = new Dictionary<Version, List<TreeNode>[]> {
        {Version.BC, null },
        {Version.O, null },
        {Version.JJ2, null },
        {Version.TSF, null },
        {Version.AGA, null },
        {Version.GorH, null },
        };
        //public string BonusLevelName;
        public Dictionary<Version, Dictionary<EnableableTitles, string>> EnableableStrings = new Dictionary<Version, Dictionary<EnableableTitles, string>> {
        {Version.BC, null },
        {Version.O, null },
        {Version.JJ2, null },
        {Version.TSF, null },
        {Version.AGA, null },
        {Version.GorH, null },
        };
        public Dictionary<Version, Dictionary<EnableableTitles, bool>> EnableableBools = new Dictionary<Version, Dictionary<EnableableTitles, bool>> {
        {Version.BC, null },
        {Version.O, null },
        {Version.JJ2, null },
        {Version.TSF, null },
        {Version.AGA, null },
        {Version.GorH, null },
        };
        public Dictionary<Version, MemoryStream[]> AmbientSounds = new Dictionary<Version, MemoryStream[]> {
        {Version.BC, null },
        {Version.O, null },
        {Version.JJ2, null },
        {Version.TSF, null },
        //{Version.AGA, null },
        {Version.GorH, null },
        };
        public static Dictionary<Version, int> DefaultFileExtension = new Dictionary<Version, int> {
        {Version.BC, 0},
        {Version.O, 0 },
        {Version.JJ2, 0},
        {Version.TSF, 0},
        {Version.AGA, 1},
        {Version.GorH, 2},
        };
        public static Dictionary<Version, string> ProfileIniFilename = new Dictionary<Version, string> {
        {Version.BC, "MLLEProfile - Battery Check"},
        {Version.O, "MLLEProfile - 110o"},
        {Version.JJ2, "MLLEProfile - JJ2"},
        {Version.TSF, "MLLEProfile - TSF"},
        {Version.AGA, "MLLEProfile - AGA"},
        {Version.GorH, "MLLEProfile - 100gh"},
        };
        public static string[] DefaultFileExtensionStrings = new string[] { ".j2l", ".lvl", ".lev" };
        public byte? GeneratorEventID = null, StartPositionEventID = null;

        //int desiredindex;
        byte CurrentLayer = 3;
        byte ZoomTileSize = 32;
        float ZoomTileFactor = 1;

        internal bool LevelDisplayLoaded = false;
        internal bool EventDisplayMode = true;
        MaskMode MaskDisplayMode;
        ParallaxMode ParallaxDisplayMode;
        internal byte ParallaxEventDisplayType = 0;
        internal uint PlusTriggerZone = 0;

        private bool levelHasBeenModified = false;
        internal bool LevelHasBeenModified
        {
            get { return levelHasBeenModified; }
            set
            {
                if (value != levelHasBeenModified)
                {
                    UpdateTitle(value);
                }
                levelHasBeenModified = value;
            }
        }

        ManualResetEvent _suspendEvent = new ManualResetEvent(true);
        Stopwatch sw = new Stopwatch();
        internal static Random _r = new Random();
        double accumulator = 0, milliseconds = 0;
        int idleCounter = 0;
        float GameTime = 0;
        int GameTick = 0;
        int LatestFPS = 0;
        public struct StringAndIndex
        {
            public string String;
            public int Index;
            public StringAndIndex(string str, int i) { String = str; Index = i; }
            public override String ToString() { return String; }
        }
        public struct NameAndFilename
        {
            public string Name;
            public string Filename;
            public NameAndFilename(string n, string f) { Name = n; Filename = f; }
            public override String ToString() { return Name; }
        }

        public AGAEvent ActiveEvent;
        #endregion variable declaration

        #region Form Business
        public Mainframe()
        {
            InitializeComponent();
        }

        private void ProcessIni(Version version)
        {
            IniFile ini = new IniFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProfileIniFilename[J2L.VersionType] + ".ini"));
            if (EnableableStrings[version] == null)
            {
                Dictionary<EnableableTitles, bool> Bools = EnableableBools[version] = new Dictionary<EnableableTitles, bool>();
                Dictionary<EnableableTitles, string> Strings = EnableableStrings[version] = new Dictionary<EnableableTitles, string>();
                Strings.Add(EnableableTitles.SecretLevelName, ini.IniReadValue("Enableable", "SecretLevel") ?? "");
                Strings.Add(EnableableTitles.BonusLevelName, ini.IniReadValue("Enableable", "BonusLevel") ?? "");
                Strings.Add(EnableableTitles.Lighting, ini.IniReadValue("Enableable", "Lighting") ?? "");
                Strings.Add(EnableableTitles.Splitscreen, ini.IniReadValue("Enableable", "Splitscreen") ?? "");
                Strings.Add(EnableableTitles.HideInHCL, ini.IniReadValue("Enableable", "HideInHCL") ?? "");
                Strings.Add(EnableableTitles.Multiplayer, ini.IniReadValue("Enableable", "Multiplayer") ?? "");
                Strings.Add(EnableableTitles.SaveAndRun, ini.IniReadValue("Enableable", "SaveAndRun") ?? "");
                Strings.Add(EnableableTitles.SaveAndRunArgs, ini.IniReadValue("Enableable", "SaveAndRunArgs") ?? "");
                Strings.Add(EnableableTitles.Illuminate, ini.IniReadValue("Enableable", "Illuminate") ?? "");
                Strings.Add(EnableableTitles.DiffLabel, ini.IniReadValue("EventTypes", "Label") ?? "");
                Strings.Add(EnableableTitles.Diff1, ini.IniReadValue("EventTypes", "0") ?? "1");
                Strings.Add(EnableableTitles.Diff2, ini.IniReadValue("EventTypes", "1") ?? "2");
                Strings.Add(EnableableTitles.Diff3, ini.IniReadValue("EventTypes", "2") ?? "3");
                Strings.Add(EnableableTitles.Diff4, ini.IniReadValue("EventTypes", "3") ?? "4");
                Bools.Add(EnableableTitles.BoolDevelopingForPlus, ini.IniReadValue("Enableable", "BoolDevelopingForPlus") != "");
                Bools.Add(EnableableTitles.UseText, ini.IniReadValue("Enableable", "BoolText") != "");
            }
            jJ2PropertiesToolStripMenuItem.Enabled = EnableableBools[version][EnableableTitles.BoolDevelopingForPlus];
            textStringsToolStripMenuItem.Enabled = EnableableBools[version][EnableableTitles.UseText];
            saveRunToolStripMenuItem.Enabled = runToolStripMenuItem.Enabled = EnableableStrings[version][EnableableTitles.SaveAndRun] != "";
            soundEffectsToolStripMenuItem.Enabled = (version == Version.AGA);
            GeneratorEventID = ((ini.IniReadValue("Enableable", "GeneratorEvent") ?? "") != "") ? Convert.ToByte(ini.IniReadValue("Enableable", "GeneratorEvent")) : (byte?)null;
            StartPositionEventID = ((ini.IniReadValue("Enableable", "StartPositionEvent") ?? "") != "") ? Convert.ToByte(ini.IniReadValue("Enableable", "StartPositionEvent")) : (byte?)null;
            DropdownPlayHere.Enabled = StartPositionEventID != null;
            if (AmbientSounds.ContainsKey(version) && AmbientSounds[version] == null)
            {
                AmbientSounds[version] = new MemoryStream[512];
            }
            IniFile baseIni;
            {
                string iniFilename = Path.ChangeExtension(Settings.IniReadValue("EventListBases", ini.IniReadValue("Events", "Base")), "ini");
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, iniFilename);
                if (File.Exists(path))
                    baseIni = new IniFile(path);
                else
                    baseIni = ini;
            }
            J2L.ProduceEventAndTypeListsFromIni(version, baseIni, ini);
            if (TreeStructure[version] == null)
            {
                List<TreeNode>[] TreeNodeLists = TreeStructure[version] = new List<TreeNode>[2];
                TreeNodeLists[0] = new List<TreeNode>();
                TreeNodeLists[1] = new List<TreeNode>();
                for (ushort i = 0; i < 256; i++)
                {
                    if (baseIni.IniReadValue("Categories", i.ToString()) == "") break;
                    else
                    {
                        CurrentIniLine = baseIni.IniReadValue("Categories", i.ToString()).Split('|');
                        TreeNodeLists[0].Add(new TreeNode(CurrentIniLine[0].TrimEnd()));
                        if (CurrentIniLine.Length == 1 || CurrentIniLine[1] == "+") TreeNodeLists[1].Add(new TreeNode(CurrentIniLine[0].TrimEnd()));
                    }
                }
                for (ushort i = 0; i < 256; i++)
                {
                    if (baseIni.IniReadValue("Subcategories", i.ToString()) == "") break;
                    else
                    {
                        CurrentIniLine = baseIni.IniReadValue("Subcategories", i.ToString()).Split('|');
                        TreeNodeLists[0].First((TreeNode node) => { return node.Text == CurrentIniLine[2].TrimEnd(); }).Nodes.Add(CurrentIniLine[1].TrimEnd(), CurrentIniLine[0].TrimEnd());
                        if (CurrentIniLine.Length == 3 || CurrentIniLine[3] == "+") TreeNodeLists[1].First((TreeNode node) => { return node.Text == CurrentIniLine[2].TrimEnd(); }).Nodes.Add(CurrentIniLine[1].TrimEnd(), CurrentIniLine[0].TrimEnd());
                    }
                }
                string[][] EventsFromIni = J2L.IniEventListing[version];
                for (ushort i = 1; i < 256; i++)
                {
                    if (EventsFromIni[i][2].Trim() != "")
                    {
                        TreeNodeLists[0].First((TreeNode node) => { return node.Nodes.ContainsKey(EventsFromIni[i][2].TrimEnd()); }).Nodes.Find(EventsFromIni[i][2].TrimEnd(), false)[0].Nodes.Add(i.ToString(), EventsFromIni[i][0].TrimEnd());
                        if (EventsFromIni[i][1] == "+") TreeNodeLists[1].First((TreeNode node) => { return node.Nodes.ContainsKey(EventsFromIni[i][2].TrimEnd()); }).Nodes.Find(EventsFromIni[i][2].TrimEnd(), false)[0].Nodes.Add(i.ToString(), EventsFromIni[i][0].TrimEnd());
                    }
                }
            }
            TiletypeDropdown.DropDownItems.Clear();
            for (byte i = 0; i < 16; i++)
            {
                string label = (i == 0) ? "Normal" : J2L.TileTypeNames[version][i];
                if (label != "")
                {
                    ToolStripMenuItem option = new ToolStripMenuItem(i.ToString() + ": " + label);
                    option.Tag = i;
                    option.Size = new System.Drawing.Size(152, 22);
                    option.Click += new System.EventHandler(this.MenuTypeInstance_Click);
                    TiletypeDropdown.DropDownItems.Add(option);
                }
            }
            TextureTypes.Clear();
            for (ushort i = 0; i < 256; i++)
            {
                if (ini.IniReadValue("Textures", i.ToString()) == "") break;
                else TextureTypes.Add(ini.IniReadValue("Textures", i.ToString()).Split('|'));
            }
            if (AllTilesetLists[version] == null) PopulateTilesetDropdown(version, ini);
            TilesetSelection.Items.Clear();
            foreach (NameAndFilename foo in AllTilesetLists[version]) TilesetSelection.Items.Add(foo);
        }
        private void PopulateTilesetDropdown(Version version, IniFile ini)
        {
            AllTilesets = (Directory.GetFiles(DefaultDirectories[version], "*.j2t").Concat(Directory.GetFiles(DefaultDirectories[version], "*.til"))).ToList<string>();
            AllTilesetLists[version] = new NameAndFilename[AllTilesets.Count];
            for (int i = 0; i < AllTilesets.Count; i++)
            {
                BinaryReader file = new BinaryReader(File.Open(AllTilesets[i], FileMode.Open, FileAccess.Read), J2File.FileEncoding);
                file.ReadBytes((file.PeekChar() == 32) ? 188 : 8);
                AllTilesetLists[version][i] = new NameAndFilename(new string(file.ReadChars(32)).TrimEnd('\0'), AllTilesets[i]);
                file.Close();
            }
        }

        void ProcessIniColorsIntoHotKolor(byte id, string group, string key)
        {
            try
            {
                string[] RGB = Settings.IniReadValue(group, key).Split(',');
                try { Byte.Parse(RGB[0].Trim()); }
                catch { RGB[0] = DefaultHotKolors[id][0]; }
                try { Byte.Parse(RGB[1].Trim()); }
                catch { RGB[1] = DefaultHotKolors[id][1]; }
                try { Byte.Parse(RGB[2].Trim()); }
                catch { RGB[2] = DefaultHotKolors[id][2]; }
                HotKolors[id] = Color.FromArgb(Int32.Parse(RGB[0].Trim()), Convert.ToInt32(RGB[1].Trim()), Convert.ToInt32(RGB[2].Trim()));
            }
            catch { HotKolors[id] = Color.Black; }
        }


        private void MakeVersionChangesAvailable()
        {
            batteryCheckToolStripMenuItem.Enabled = Directory.Exists(Settings.IniReadValue("Paths", "BC"));
            jazz2V110oToolStripMenuItem.Enabled = Directory.Exists(Settings.IniReadValue("Paths", "O"));
            jazz2V123ToolStripMenuItem.Enabled = Directory.Exists(Settings.IniReadValue("Paths", "JJ2"));
            jazz2V124ToolStripMenuItem.Enabled = Directory.Exists(Settings.IniReadValue("Paths", "TSF"));
            animaniacsToolStripMenuItem.Enabled = Directory.Exists(Settings.IniReadValue("Paths", "AGA"));
            jazz2V100ghToolStripMenuItem.Enabled = Directory.Exists(Settings.IniReadValue("Paths", "GorH"));
        }
        private bool SetupFolders()
        {
            return DirectorySetupForm.ShowForm(Settings);
        }
        private void Mainframe_Load(object sender, EventArgs e)
        {
            while (!LevelDisplayLoaded) ;

            {
                string X = Settings.IniReadValue("Window", "X");
                string Y = Settings.IniReadValue("Window", "Y");
                string Width = Settings.IniReadValue("Window", "Width");
                string Height = Settings.IniReadValue("Window", "Height");
                string Maximized = Settings.IniReadValue("Window", "Maximized");

                int Xi, Yi, Wi, Hi; bool Mb;

                if (int.TryParse(X, out Xi) && int.TryParse(Y, out Yi))
                    this.Location = new Point(Xi, Yi);
                if (int.TryParse(Width, out Wi) && int.TryParse(Height, out Hi))
                    this.Size = new Size(Wi, Hi);
                if (bool.TryParse(Maximized, out Mb))
                    this.WindowState = Mb ? FormWindowState.Maximized : FormWindowState.Normal;
            }

            if ((Settings.IniReadValue("Miscellaneous", "Initialized") ?? "") != "1") {
                if (SetupFolders())
                    Settings.IniWriteValue("Miscellaneous", "Initialized", "1");
                else
                {
                    Application.Exit();
                    return;
                }
            }
            MakeVersionChangesAvailable();

            DefaultDirectories = new Dictionary<Version, string> {
            {Version.BC, Settings.IniReadValue("Paths","BC") },
            {Version.O, Settings.IniReadValue("Paths","O") },
            {Version.JJ2, Settings.IniReadValue("Paths","JJ2") },
            {Version.TSF, Settings.IniReadValue("Paths","TSF") },
            {Version.AGA, Settings.IniReadValue("Paths","AGA") },
            {Version.GorH, Settings.IniReadValue("Paths","GorH") },
            };
            ProcessIniColorsIntoHotKolor(0, "Colors", "Deadspace");
            ProcessIniColorsIntoHotKolor(1, "Colors", "Tile0");
            ProcessIniColorsIntoHotKolor(2, "Colors", "Transparent");
            TexUtil.InitTexturing();
            J2L = new TexturedJ2L();
            J2L.DeadspaceColor = HotKolors[0];
            J2L.Tile0Color = HotKolors[1];
            J2L.TranspColor = HotKolors[2];
            switch (Settings.IniReadValue("Miscellaneous", "DefaultGame"))
            {
                case "JJ2":
                    NewJ2L(Version.JJ2);
                    break;
                case "TSF":
                    NewJ2L(Version.TSF);
                    break;
                case "O":
                    NewJ2L(Version.O);
                    break;
                case "GorH":
                    NewJ2L(Version.GorH);
                    break;
                case "BC":
                    NewJ2L(Version.BC);
                    break;
                case "AGA":
                    NewJ2L(Version.AGA);
                    break;
                default:
                    MessageBox.Show("MLLE cannot run if you have none of the games it's built for!", "go download battery check or something", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Settings.IniWriteValue("Miscellaneous", "Initialized", "0");
                    Application.Exit();
                    return;
            }
            for (ushort x = 0; x < IsEachTileSelected.Length; x++) IsEachTileSelected[x] = new bool[1026];
            for (ushort x = 0; x < ShouldEachTileBeFilledIn.Length; x++) ShouldEachTileBeFilledIn[x] = new bool[1026];
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GeneratorOverlay = TexUtil.CreateTextureFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Generator.png"));
            SelectionOverlay = TexUtil.CreateTextureFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SelectionRectangles.png"));
            GL.BindTexture(TextureTarget.Texture2D, J2L.Atlases[0]);
            GL.ClearColor(HotKolors[0]);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            LevelDisplay.SwapBuffers();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            TilesetOverlaySelection.SelectedIndex = 2;
            (DeepEditingTool = VisibleEditingTool = PaintbrushButton).Checked = true;
            Dictionary<string, Keys> StK = new Dictionary<string, Keys>
            {
                {"Shift", Keys.Shift},
                {"Control", Keys.Control},
                {"Alt", Keys.Alt}
            };
            try { AddSelectionKey = StK[Settings.IniReadValue("Hotkeys", "AddToSelection")]; }
            catch { AddSelectionKey = Keys.Shift; }
            try { SubtractSelectionKey = StK[Settings.IniReadValue("Hotkeys", "SubtractFromSelection")]; }
            catch { SubtractSelectionKey = Keys.Control; }
            ParallaxEventDisplayType = (Settings.IniReadValue("Miscellaneous", "EventParallaxMode") == "0") ? (byte)0 : (byte)1; eventsForemostToolStripMenuItem.Checked = ParallaxEventDisplayType == 1;

            var commands = Environment.GetCommandLineArgs();
            if (commands.Length > 1) switch (Path.GetExtension(commands[1].Trim()).ToLowerInvariant())
                {
                    case ".j2l":
                    case ".lev":
                    case ".lvl":
                        LoadJ2L(commands[1]);
                        break;
                    case ".j2t":
                    case ".til":
                        ChangeTileset(commands[1]);
                        break;
                    default:
                        break;
                }

            sw.Start();
            DrawThread = new Thread(TimePasses);
            DrawThread.IsBackground = true;
            DrawThread.Start();
        }
        private void Mainframe_FormClosing(object sender, FormClosingEventArgs e) {
            if (!PromptForSaving())
            {
                e.Cancel = true;
                _suspendEvent.Set();
                return;
            }
            if (DrawThread != null)
                DrawThread.Abort();

            bool windowIsMaximized = this.WindowState == FormWindowState.Maximized;
            Settings.IniWriteValue("Window", "Maximized", windowIsMaximized.ToString());
            if (!windowIsMaximized)
            {
                Settings.IniWriteValue("Window", "X", this.Location.X.ToString());
                Settings.IniWriteValue("Window", "Y", this.Location.Y.ToString());
                Settings.IniWriteValue("Window", "Width", this.Size.Width.ToString());
                Settings.IniWriteValue("Window", "Height", this.Size.Height.ToString());
            }
        }
        private void Mainframe_Paint(object sender, PaintEventArgs e)
        {
            LDScrollH.Update();
            LDScrollV.Update();
            TilesetScrollbar.Update();
        }

        internal void IdentifyTileset()
        {
            TilesetSelection.SelectedIndex = TilesetSelection.Items.IndexOf(AllTilesetLists[J2L.VersionType].FirstOrDefault((NameAndFilename nf) => { return Path.GetFileName(nf.Filename) == J2L.Tileset; }));
            //desiredindex = AllTilesets.FindIndex(delegate(string current) { return Path.GetFileName(current) == J2L.Tileset; });
            //foreach (StringAndIndex item in TilesetSelection.Items)
            //{
            //    if (item.Index == desiredindex) { TilesetSelection.SelectedIndex = TilesetSelection.Items.IndexOf(item); break; }
            //}
        }
        internal void CheckCurrentVersion()
        {
            jazz2V110oToolStripMenuItem.Checked = J2L.VersionType == Version.O;
            batteryCheckToolStripMenuItem.Checked = J2L.VersionType == Version.BC;
            jazz2V123ToolStripMenuItem.Checked = J2L.VersionType == Version.JJ2;
            jazz2V124ToolStripMenuItem.Checked = J2L.VersionType == Version.TSF;
            animaniacsToolStripMenuItem.Checked = J2L.VersionType == Version.AGA;
            jazz2V100ghToolStripMenuItem.Checked = J2L.VersionType == Version.GorH;
        }
        internal void ChangeLayer(byte number)
        {
            CurrentLayer = number;
            ResizeDisplay();
            L1Button.Checked = Gotolayer1.Checked = number == 0;
            L2Button.Checked = Gotolayer2.Checked = number == 1;
            L3Button.Checked = Gotolayer3.Checked = number == 2;
            L4Button.Checked = Gotolayer4.Checked = number == 3;
            L5Button.Checked = Gotolayer5.Checked = number == 4;
            L6Button.Checked = Gotolayer6.Checked = number == 5;
            L7Button.Checked = Gotolayer7.Checked = number == 6;
            L8Button.Checked = Gotolayer8.Checked = number == 7;
        }
        /*internal void ReadjustScrollbars()
        {
            MakeProposedScrollbarValueWork(TilesetScrollbar, TilesetScrollbar.Value);
            MakeProposedScrollbarValueWork(LDScrollH, LDScrollH.Value);
            MakeProposedScrollbarValueWork(LDScrollV, LDScrollV.Value);
        }*/
        internal int MakeProposedScrollbarValueWork(ScrollBar bar, int nuVal)
        {
            return bar.Value = Math.Max(0, Math.Min(bar.Maximum - bar.LargeChange + 1, nuVal));
        }

        enum ParallaxMode { NoParallax, FullParallax, TemporaryParallax }
        enum MaskMode { NoMask, FullMask, TemporaryMask }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.D1: { if (LastFocusedZone != FocusedZone.AnimationEditing) { ChangeLayer(0); return true; } else return false; }
                case Keys.D2: { if (LastFocusedZone != FocusedZone.AnimationEditing) { ChangeLayer(1); return true; } else return false; }
                case Keys.D3: { if (LastFocusedZone != FocusedZone.AnimationEditing) { ChangeLayer(2); return true; } else return false; }
                case Keys.D4: { if (LastFocusedZone != FocusedZone.AnimationEditing) { ChangeLayer(3); return true; } else return false; }
                case Keys.D5: { if (LastFocusedZone != FocusedZone.AnimationEditing) { ChangeLayer(4); return true; } else return false; }
                case Keys.D6: { if (LastFocusedZone != FocusedZone.AnimationEditing) { ChangeLayer(5); return true; } else return false; }
                case Keys.D7: { if (LastFocusedZone != FocusedZone.AnimationEditing) { ChangeLayer(6); return true; } else return false; }
                case Keys.D8: { if (LastFocusedZone != FocusedZone.AnimationEditing) { ChangeLayer(7); return true; } else return false; }

                case (Keys.D1 | Keys.Control): { ShowLayerProperties(0); return true; }
                case (Keys.D2 | Keys.Control): { ShowLayerProperties(1); return true; }
                case (Keys.D3 | Keys.Control): { ShowLayerProperties(2); return true; }
                case (Keys.D4 | Keys.Control): { ShowLayerProperties(3); return true; }
                case (Keys.D5 | Keys.Control): { ShowLayerProperties(4); return true; }
                case (Keys.D6 | Keys.Control): { ShowLayerProperties(5); return true; }
                case (Keys.D7 | Keys.Control): { ShowLayerProperties(6); return true; }
                case (Keys.D8 | Keys.Control): { ShowLayerProperties(7); return true; }

                case (Keys.Shift | Keys.T): { return setTileType(1); }
                case (Keys.D0 | Keys.Shift): { return setTileType(0); }
                case (Keys.D1 | Keys.Shift): { return setTileType(1); }
                case (Keys.D2 | Keys.Shift): { return setTileType(2); }
                case (Keys.D3 | Keys.Shift): { return setTileType(3); }
                case (Keys.D4 | Keys.Shift): { return setTileType(4); }
                case (Keys.D5 | Keys.Shift): { return setTileType(5); }
                case (Keys.D6 | Keys.Shift): { return setTileType(6); }
                case (Keys.D7 | Keys.Shift): { return setTileType(7); }
                case (Keys.D8 | Keys.Shift): { return setTileType(8); }
                case (Keys.D9 | Keys.Shift): { return setTileType(9); }

                case (Keys.Control | Keys.Subtract): { ZoomOut(); return true; }
                case (Keys.Control | Keys.Add): { ZoomIn(); return true; }

                case Keys.M: { if (!CurrentModifierKeys[Keys.Control]) MaskDisplayMode = MaskMode.TemporaryMask; return true; }
                case Keys.P: { if (!CurrentModifierKeys[Keys.Control]) ParallaxDisplayMode = ParallaxMode.TemporaryParallax; return true; }

                case (Keys.Control | Keys.Shift | Keys.R): { if (LastFocusedZone == FocusedZone.Level) PlayFromHere(); return true; }

                case (Keys.Control | Keys.P): { ParallaxButton.Checked = DropdownParallax.Checked = !ParallaxButton.Checked; return true; }
                case (Keys.Control | Keys.M): { MaskButton.Checked = DropdownMask.Checked = !MaskButton.Checked; return true; }
                case (Keys.Control | Keys.V): { EventsButton.Checked = DropdownEvents.Checked = !EventsButton.Checked; return true; }

                case Keys.Left: { if (LastFocusedZone == FocusedZone.Level) try { LDScrollH.Value -= LDScrollH.SmallChange; } catch { LDScrollH.Value = 0; } return true; }
                case Keys.Right: { if (LastFocusedZone == FocusedZone.Level) LDScrollH.Value = Math.Min(LDScrollH.Value + LDScrollH.SmallChange, LDScrollH.Maximum - LDScrollH.LargeChange + 1); return true; }

                case Keys.Delete:
                    {
                        if (LastFocusedZone == FocusedZone.AnimationEditing && SelectedAnimationFrame < WorkingAnimation.FrameCount) { WorkingAnimation.DeleteFrame(SelectedAnimationFrame); WorkingAnimation.JustBeenEdited(GameTick); if (AnimScrollbar.Maximum > 32) AnimScrollbar.Maximum -= 32; AnimScrollbar.Value = Math.Max(0, AnimScrollbar.Value - 32); LevelHasBeenModified = true; }
                        else if (LastFocusedZone == FocusedZone.Level) Clear(CurrentLayer);
                        return true;
                    }
                case Keys.F:
                    {
                        if (LastFocusedZone == FocusedZone.AnimationEditing) WorkingAnimation.Sequence[SelectedAnimationFrame] ^= (ushort)J2L.MaxTiles;
                        else if (CurrentStamp.Length > 0)
                        {
                            TileAndEvent[][] NuStamp = new TileAndEvent[CurrentStamp.Length][];
                            for (ushort x = 0; x < NuStamp.Length; x++)
                            {
                                NuStamp[x] = CurrentStamp[NuStamp.Length - x - 1];
                                for (ushort y = 0; y < NuStamp[x].Length; y++) NuStamp[x][y].Tile ^= (ushort)J2L.MaxTiles;
                            }
                            CurrentStamp = NuStamp;
                        }
                        return true;
                    }
                case Keys.I:
                    {
                        if (LastFocusedZone == FocusedZone.AnimationEditing) WorkingAnimation.Sequence[SelectedAnimationFrame] ^= (ushort)0x2000;
                        else if (CurrentStamp.Length > 0)
                        {
                            TileAndEvent[][] NuStamp = new TileAndEvent[CurrentStamp.Length][];
                            int height = CurrentStamp[0].Length;
                            for (ushort x = 0; x < NuStamp.Length; x++)
                            {
                                NuStamp[x] = new TileAndEvent[height];
                                for (ushort y = 0; y < height; y++)
                                    NuStamp[x][y].Tile = (ushort)(CurrentStamp[x][height - y - 1].Tile ^ (ushort)0x2000);
                            }
                            CurrentStamp = NuStamp;
                        }
                        return true;
                    }

                case (Keys.Control | Keys.E): { GrabEventAtMouse(); return true; }
                case (Keys.Shift | Keys.E): { PasteEventAtMouse(); return true; }
                case Keys.E: { SelectEventAtMouse(); return true; }

                case Keys.Oemcomma: { SetStampDimensions(1, 1); CurrentStamp[0][0] = new TileAndEvent((LastFocusedZone == FocusedZone.Level) ? J2L.Layers[CurrentLayer].TileMap[MouseTileX, MouseTileY] : (ushort)MouseTile, 0); ShowBlankTileInStamp = true; DeselectAll(); return true; }
                case (Keys.Shift | Keys.Oemcomma): { SetStampDimensions(1, 1); CurrentStamp[0][0] = new TileAndEvent((LastFocusedZone == FocusedZone.Level) ? J2L.Layers[CurrentLayer].TileMap[MouseTileX, MouseTileY] : (ushort)MouseTile, MouseAGAEvent.ID); ShowBlankTileInStamp = true; DeselectAll(); return true; }
                case Keys.Back: { ShowBlankTileInStamp = true; SetStampDimensions(1, 1); CurrentStamp[0][0] = new TileAndEvent(0, 0); DeselectAll(); return true; }

                case (Keys.Control | Keys.B):
                    {
                        if (WhereSelected != FocusedZone.None && HowSelecting == FocusedZone.None) BeginSelection(SelectionType.Subtract);
                        else { EndSelection(); MakeSelectionIntoStamp(); if (WhereSelected == FocusedZone.Level) DeselectAll(); }
                        return true;
                    }
                case (Keys.Shift | Keys.B):
                    {
                        if (LastFocusedZone == WhereSelected && HowSelecting == FocusedZone.None) BeginSelection(SelectionType.Add);
                        else if (HowSelecting != LastFocusedZone) BeginSelection(SelectionType.New);
                        else { EndSelection(); MakeSelectionIntoStamp(); if (WhereSelected == FocusedZone.Level) DeselectAll(); }
                        return true;
                    }
                case Keys.B:
                    {
                        if (LastFocusedZone != HowSelecting) BeginSelection(SelectionType.New);
                        else { EndSelection(); MakeSelectionIntoStamp(); if (WhereSelected == FocusedZone.Level) DeselectAll(); }
                        return true;
                    }
                case (Keys.Control | Keys.D):
                    {
                        DeselectAll();
                        return true;
                    }
                case (Keys.Control | Keys.C):
                    {
                        MakeSelectionIntoStamp();
                        return true;
                    }
                case (Keys.Control | Keys.X):
                    {
                        MakeSelectionIntoStamp(true);
                        return true;
                    }

                default: return base.ProcessCmdKey(ref msg, keyData);
            }
        }
        private Dictionary<Keys, bool> CurrentModifierKeys = new Dictionary<Keys, bool> { { Keys.Shift, false }, { Keys.Control, false }, { Keys.Alt, false } };

        private void Mainframe_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                /*case Keys.Shift:
                case Keys.Control:
                case Keys.Alt:
                    CurrentModifierKeys[e.KeyData] = false;
                    break;*/
                case Keys.M:
                    if (MaskDisplayMode == MaskMode.TemporaryMask) MaskDisplayMode = MaskButton.Checked ? MaskMode.FullMask : MaskMode.NoMask;
                    break;
                case Keys.P:
                    if (ParallaxDisplayMode == ParallaxMode.TemporaryParallax) ParallaxDisplayMode = ParallaxButton.Checked ? ParallaxMode.FullParallax : ParallaxMode.NoParallax;
                    break;
                default:
                    break;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) { SafeToDisplay = false; Application.Exit(); }
        private bool setTileType(byte value)
        {
            if (LastFocusedZone == FocusedZone.Tileset && MouseTile < J2L.J2T.TileCount && J2L.TileTypeNames[J2L.VersionType][value] != "")
            {
                J2L.TileTypes[MouseTile] = value;
                SetTextureTo(AtlasID.Image);
                byte[] oldTile = J2L.J2T.Images[J2L.J2T.ImageAddress[MouseTile]];
                var tileTrans = J2L.J2T.TransparencyMaskJJ2_Style[Array.BinarySearch(J2L.J2T.TransparencyMaskOffset, 0, (int)J2L.J2T.data3Counter, J2L.J2T.TransparencyMaskAddress[MouseTile])];
                using (Bitmap bmp = new Bitmap(32, 32)) //using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    for (byte x = 0; x < 32; x++)
                        for (byte y = 0; y < 32; y++)
                        {
                            byte[] pixel = J2L.J2T.Palette[oldTile[x + y * 32]];
                            if (tileTrans[x + y * 32] == 0) bmp.SetPixel(x, y, Color.FromArgb(0, J2L.GetLevelFromColor(J2L.TranspColor, 0), J2L.GetLevelFromColor(J2L.TranspColor, 1), J2L.GetLevelFromColor(J2L.TranspColor, 2)));
                            else bmp.SetPixel(x, y, Color.FromArgb((value >= 1 && value <= 3) ? 192 : 255, pixel[0], pixel[1], pixel[2]));
                        }
                    System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, 32, 32), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexSubImage2D(TextureTarget.Texture2D, 0, MouseTile % J2L.AtlasLength * 32, MouseTile / J2L.AtlasLength * 32, 32, 32, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    bmp.UnlockBits(data);
                }
                RedrawTilesetHowManyTimes = 2;
                LevelHasBeenModified = true;
                return true;
            }
            else return false;
        }
        #endregion Form Business

        #region Menu Busywork
        private void TilesetSelection_SelectedIndexChanged(object sender, EventArgs e)
        { TilesetScrollbar.Focus(); ChangeTileset(((NameAndFilename)TilesetSelection.Items[TilesetSelection.SelectedIndex]).Filename); }

        private void aboutMLLEToolStripMenuItem_Click(object sender, EventArgs e) { _suspendEvent.Reset(); new AboutBox1().ShowDialog(); _suspendEvent.Set(); }


        private void batteryCheckToolStripMenuItem_Click(object sender, EventArgs e) { ChangeVersion(Version.BC); }
        private void jazz2V110oToolStripMenuItem_Click(object sender, EventArgs e) { ChangeVersion(Version.O); }
        private void jazz2V123ToolStripMenuItem_Click(object sender, EventArgs e) { ChangeVersion(Version.JJ2); }
        private void jazz2V124ToolStripMenuItem_Click(object sender, EventArgs e) { ChangeVersion(Version.TSF); }
        private void animaniacsToolStripMenuItem_Click(object sender, EventArgs e) { ChangeVersion(Version.AGA); }
        private void jazz2V100ghToolStripMenuItem_Click(object sender, EventArgs e) { ChangeVersion(Version.GorH); }

        private void DescribableControl_MouseEnter(object sender, EventArgs e)
        {
            if (sender as Control != null) ToolTiplikePrintout.Text = (string)((Control)sender).Tag;
            else if (sender as ToolStripItem != null) ToolTiplikePrintout.Text = (string)((ToolStripItem)sender).Tag;
        }

        private void soundEffectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _suspendEvent.Reset();
            SFXForm SFX = new SFXForm(J2L.AGA_SoundPointer, DefaultDirectories[Version.AGA]);
            SFX.ShowDialog();
            if (SFX.result == DialogResult.OK) { J2L.AGA_SoundPointer = SFX.Paths; LevelHasBeenModified = true; }
            _suspendEvent.Set();
        }

        private PlusPropertyList currentPlusPropertyList = new PlusPropertyList(null);
        private void plusLevelPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _suspendEvent.Reset();
            currentPlusPropertyList.ReadFromEventMap(J2L.EventMap);
            PlusPropertyList? newPlusPropertyList = new PlusProperties().ShowForm(ref currentPlusPropertyList);
            if (newPlusPropertyList.HasValue)
            {
                currentPlusPropertyList = newPlusPropertyList.Value;
                currentPlusPropertyList.WriteToEventMap(J2L.EventMap);
                LevelHasBeenModified = true;
            }
            _suspendEvent.Set();
        }
        

        private void pathsAndFilenamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _suspendEvent.Reset();
            SetupFolders();
            MakeVersionChangesAvailable();
            _suspendEvent.Set();
        }

        private void setDeadspaceColorToolStripMenuItem_Click(object sender, EventArgs e) { SetNewColor(ref J2L.DeadspaceColor, "Deadspace", false); GL.ClearColor(J2L.DeadspaceColor); }
        private void setTile0ColorToolStripMenuItem_Click(object sender, EventArgs e) { SetNewColor(ref J2L.Tile0Color, "Tile0", true); }
        private void setTransparentColorToolStripMenuItem_Click(object sender, EventArgs e) { SetNewColor(ref J2L.TranspColor, "Transparent", true); }

        private void SetNewColor(ref Color color, string iniName, bool recompilationNeeded)
        {
            _suspendEvent.Reset();
            colorDialog1.Color = color;
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                color = colorDialog1.Color;
                Settings.IniWriteValue("Colors", iniName, string.Format("{0},{1},{2}", color.R, color.G, color.B));
                if (recompilationNeeded && J2L.J2T != null && J2L.TexturesHaveBeenGenerated)
                {
                    J2L.Degenerate_Textures();
                    J2L.Generate_Textures(includeMasks: true);
                }
                RedrawTilesetHowManyTimes = 2;
            }
            _suspendEvent.Set();
        }

        private void eventsForemostToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Settings.IniWriteValue("Miscellaneous", "EventParallaxMode", (ParallaxEventDisplayType = eventsForemostToolStripMenuItem.Checked ? (byte)1 : (byte)0).ToString());
        }

        private void DrawingToolButton_Click(object sender, EventArgs e)
        {
            DeepEditingTool = VisibleEditingTool = (ToolStripButton)sender;
            for (byte i = 0; i < DrawingTools.Items.Count; i++) if (DrawingTools.Items[i].GetType() == sender.GetType())
                    ((ToolStripButton)DrawingTools.Items[i]).Checked = false;
            DeepEditingTool.Checked = true;
        }

        private void MenuTypeInstance_Click(object sender, EventArgs e) { J2L.TileTypes[MouseTile] = (byte)((ToolStripItem)sender).Tag; LevelHasBeenModified = true; RedrawTilesetHowManyTimes = 2; }

        private void DropdownClear_Click(object sender, EventArgs e) { Clear(CurrentLayer); }
        private void ClearButton_Click(object sender, EventArgs e) { Clear(CurrentLayer); }

        private void saveAsImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _suspendEvent.Reset();
            byte OriginalTileSize = ZoomTileSize;
            LDScrollH.Value = LDScrollV.Value = 0;
            Zoom((byte)(Math.Min(32, 4096 / Math.Max(J2L.Layers[3].Width, J2L.Layers[3].Height)/4*4)));
            LevelDisplay.Width = (int)J2L.Layers[3].Width * ZoomTileSize + LDScrollH.Location.X;
            LevelDisplay.Height = (int)J2L.Layers[3].Height * ZoomTileSize + LDScrollH.Height;
            SafeToDisplay = false;
            ChangeLayer(3);
            SafeToDisplay = true;
            LevelDisplay.Refresh();
            LevelDisplay.Refresh();
            using (Bitmap bmp = new Bitmap(LevelDisplay.Width - LDScrollH.Location.X, LevelDisplay.Height - LDScrollH.Height))
            {
                System.Drawing.Imaging.BitmapData data =
                    bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                GL.ReadPixels(LDScrollH.Location.X, LDScrollH.Height, LevelDisplay.Width - LDScrollH.Location.X, LevelDisplay.Height - LDScrollH.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                bmp.UnlockBits(data);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                bmp.Save(Path.ChangeExtension(J2L.FilenameOnly, "png"), System.Drawing.Imaging.ImageFormat.Png);
            }
            LevelDisplay.Width = Width - LDScrollV.Width;
            LevelDisplay.Height = LDScrollV.Height + LDScrollH.Height;
            Zoom(OriginalTileSize);
            _suspendEvent.Set();
        } // this doesn't work right now

        private void L1Button_Click(object sender, EventArgs e) { ChangeLayer(0); }
        private void L2Button_Click(object sender, EventArgs e) { ChangeLayer(1); }
        private void L3Button_Click(object sender, EventArgs e) { ChangeLayer(2); }
        private void L4Button_Click(object sender, EventArgs e) { ChangeLayer(3); }
        private void L5Button_Click(object sender, EventArgs e) { ChangeLayer(4); }
        private void L6Button_Click(object sender, EventArgs e) { ChangeLayer(5); }
        private void L7Button_Click(object sender, EventArgs e) { ChangeLayer(6); }
        private void L8Button_Click(object sender, EventArgs e) { ChangeLayer(7); }
        private void Gotolayer1_Click(object sender, EventArgs e) { ChangeLayer(0); }
        private void Gotolayer2_Click(object sender, EventArgs e) { ChangeLayer(1); }
        private void Gotolayer3_Click(object sender, EventArgs e) { ChangeLayer(2); }
        private void Gotolayer4_Click(object sender, EventArgs e) { ChangeLayer(3); }
        private void Gotolayer5_Click(object sender, EventArgs e) { ChangeLayer(4); }
        private void Gotolayer6_Click(object sender, EventArgs e) { ChangeLayer(5); }
        private void Gotolayer7_Click(object sender, EventArgs e) { ChangeLayer(6); }
        private void Gotolayer8_Click(object sender, EventArgs e) { ChangeLayer(7); }

        private void SelectEvent_Click(object sender, EventArgs e) { SelectEventAtMouse(); }
        private void GrabEvent_Click(object sender, EventArgs e) { GrabEventAtMouse(); }
        private void PasteEvent_Click(object sender, EventArgs e) { PasteEventAtMouse(); }
        private void SetEventTS_Click(object sender, EventArgs e) { SelectEventAtMouse(); }
        private void GrabEventTS_Click(object sender, EventArgs e) { GrabEventAtMouse(); }
        private void PutEventTS_Click(object sender, EventArgs e) { PasteEventAtMouse(); }

        internal void ZoomIn() { if (ZoomTileSize < 32) { Zoom((byte)(ZoomTileSize << 1)); } }
        internal void ZoomOut() { if (ZoomTileSize > 4) { Zoom((byte)(ZoomTileSize >> 1)); } }
        private void ZoomInButton_Click(object sender, EventArgs e) { ZoomIn(); }
        private void ZoomOutButton_Click(object sender, EventArgs e) { ZoomOut(); }
        private void Zoom100_Click(object sender, EventArgs e) { Zoom(32); }
        private void Zoom50_Click(object sender, EventArgs e) { Zoom(16); }
        private void Zoom25_Click(object sender, EventArgs e) { Zoom(8); }
        private void Zoom12p5_Click(object sender, EventArgs e) { Zoom(4); }
        private void Zoom(byte newTileSize)
        {
            float Factor = newTileSize / 32F / ZoomTileFactor;
            int x = LDScrollH.Value, y = LDScrollV.Value;
            ZoomTileSize = newTileSize;
            ZoomTileFactor *= Factor;
            ResizeDisplay();
            MakeProposedScrollbarValueWork(LDScrollH, (int)((x + LevelDisplayViewportWidth / 2) * Factor) - LevelDisplayViewportWidth / 2);
            MakeProposedScrollbarValueWork(LDScrollV, (int)((y + LevelDisplayViewportHeight / 2) * Factor) - LevelDisplayViewportHeight / 2);
            Zoom100.Checked = newTileSize == 32;
            Zoom50.Checked = newTileSize == 16;
            Zoom25.Checked = newTileSize == 8;
            Zoom12p5.Checked = newTileSize == 4;
        }

        int stream = 0;
        bool MusicIsPlaying = false;
        private void playMusicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine(playMusicToolStripMenuItem.Checked);
            if (playMusicToolStripMenuItem.Checked) { BassNet.Registration("VioletCLM@gmail.com", "2X17292219152222"); PlayMusic(); }
            else
            {
                if (stream != 0)
                {
                    Bass.BASS_StreamFree(stream);
                    Bass.BASS_Free();
                }
                MusicIsPlaying = false;
            }
        }
        private bool PlayMusic()
        {
            if (!MusicIsPlaying)
            {
                MusicIsPlaying = Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                if (!MusicIsPlaying)
                {
                    Console.WriteLine("BASS: Could not initialize");
                }
            }
            else
            {
                Bass.BASS_StreamFree(stream);
                Bass.BASS_MusicFree(stream);
            }

            string filename = Path.Combine(DefaultDirectories[J2L.VersionType], J2L.Music);
            string fileextension = Path.GetExtension(filename);
            if (fileextension == "")
            {
                filename = Path.ChangeExtension(filename, fileextension = ".j2b");
                //and then... do nothing.
            }
            else if (File.Exists(filename))
            {
                if (fileextension == ".mp3")
                    stream = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_DEFAULT | BASSFlag.BASS_MUSIC_LOOP);
                else
                    stream = Bass.BASS_MusicLoad(filename, 0, 0, BASSFlag.BASS_DEFAULT | BASSFlag.BASS_MUSIC_LOOP, 0);
                Console.WriteLine(filename);

                if (stream != 0)
                {
                    Bass.BASS_ChannelPlay(stream, false);
                }
                else
                {
                    Console.WriteLine("BASS: Stream error: {0}", Bass.BASS_ErrorGetCode());
                }
                return true;
            }
            return false;
        }

        private void UndoButton_Click(object sender, EventArgs e) { Undo(); }
        private void RedoButton_Click(object sender, EventArgs e) { Redo(); }
        private void undoToolStripMenuItem_Click(object sender, EventArgs e) { Undo(); }
        private void redoToolStripMenuItem_Click(object sender, EventArgs e) { Redo(); }

        public TilesetOverlay CurrentTilesetOverlay = TilesetOverlay.None;
        private void TilesetOverlaySelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (TilesetOverlaySelection.SelectedIndex)
            {
                case 1:
                    CurrentTilesetOverlay = TilesetOverlay.Events;
                    break;
                case 2:
                    CurrentTilesetOverlay = TilesetOverlay.TileTypes;
                    break;
                case 3:
                    CurrentTilesetOverlay = TilesetOverlay.Masks;
                    break;
                case 0:
                default:
                    CurrentTilesetOverlay = TilesetOverlay.None;
                    break;
            }
            RedrawTilesetHowManyTimes = 2;
            TilesetScrollbar.Focus();
        }

        private void OverNone_Click(object sender, EventArgs e) { TilesetOverlaySelection.SelectedIndex = 0; }
        private void OverEvents_Click(object sender, EventArgs e) { TilesetOverlaySelection.SelectedIndex = 1; }
        private void OverTileTypes_Click(object sender, EventArgs e) { TilesetOverlaySelection.SelectedIndex = 2; }
        private void OverMasks_Click(object sender, EventArgs e) { TilesetOverlaySelection.SelectedIndex = 3; }


        private void textStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _suspendEvent.Reset();
            TextEdit TE = new TextEdit(J2L.Text);
            TE.ShowDialog();
            if (TE.result == DialogResult.OK) { J2L.Text = TE.workTexts; LevelHasBeenModified = true; }
            _suspendEvent.Set();
        }

        private void ShowLayerProperties(byte layer)
        {
            _suspendEvent.Reset();
            LayerPropertiesForm LP = new LayerPropertiesForm(this, layer);
            LP.ShowDialog();
            ResizeDisplay();
            _suspendEvent.Set();
        }
        private void LayerPropertiesButton_Click(object sender, EventArgs e) { ShowLayerProperties(CurrentLayer); }
        private void Properties1_Click(object sender, EventArgs e) { ShowLayerProperties(0); }
        private void Properties2_Click(object sender, EventArgs e) { ShowLayerProperties(1); }
        private void Properties3_Click(object sender, EventArgs e) { ShowLayerProperties(2); }
        private void Properties4_Click(object sender, EventArgs e) { ShowLayerProperties(3); }
        private void Properties5_Click(object sender, EventArgs e) { ShowLayerProperties(4); }
        private void Properties6_Click(object sender, EventArgs e) { ShowLayerProperties(5); }
        private void Properties7_Click(object sender, EventArgs e) { ShowLayerProperties(6); }
        private void Properties8_Click(object sender, EventArgs e) { ShowLayerProperties(7); }

        private void levelPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _suspendEvent.Reset();
            LevelProperties LP = new LevelProperties(this);
            LP.ShowDialog();
            if (LP.MusicChanged && playMusicToolStripMenuItem.Checked) PlayMusic();
            _suspendEvent.Set();
        }
        private void DropdownProperties_Click(object sender, EventArgs e)
        {
            ShowLayerProperties(CurrentLayer);
        }

        private void EventsButton_CheckedChanged(object sender, EventArgs e) { EventDisplayMode = DropdownEvents.Checked = EventsButton.Checked; }
        private void MaskButton_CheckedChanged(object sender, EventArgs e) { MaskDisplayMode = (DropdownMask.Checked = MaskButton.Checked) ? MaskMode.FullMask : MaskMode.NoMask; DropdownParallax.Enabled = ParallaxButton.Enabled = !MaskButton.Checked; }
        private void ParallaxButton_CheckedChanged(object sender, EventArgs e) { SetParallaxModeTo(DropdownParallax.Checked = ParallaxButton.Checked); }
        private void DropdownEvents_CheckedChanged(object sender, EventArgs e) { EventDisplayMode = EventsButton.Checked = DropdownEvents.Checked; }
        private void DropdownMask_CheckedChanged(object sender, EventArgs e) { MaskDisplayMode = (MaskButton.Checked = DropdownMask.Checked) ? MaskMode.FullMask : MaskMode.NoMask; DropdownParallax.Enabled = ParallaxButton.Enabled = !MaskButton.Checked; }
        private void DropdownParallax_CheckedChanged(object sender, EventArgs e) { SetParallaxModeTo(ParallaxButton.Checked = DropdownParallax.Checked); }

        private void SetParallaxModeTo(bool mode)
        {
            if (mode) { ParallaxDisplayMode = ParallaxMode.FullParallax; GL.Enable(EnableCap.Blend); /*GL.Disable(EnableCap.ScissorTest);*/ }
            else { ParallaxDisplayMode = ParallaxMode.NoParallax; GL.Disable(EnableCap.Blend); /*GL.Enable(EnableCap.ScissorTest);*/ }
        }
        #endregion Menu Busywork

        #region J2L Extensions
        internal void ChangeTileset(string filename)
        {
            var result = J2L.ChangeTileset(filename);
            if (result != VersionChangeResults.Success)
            {
                _suspendEvent.Reset();
                IdentifyTileset();
                if (result == VersionChangeResults.UnsupportedConversion) MessageBox.Show(String.Format("Sorry, {0} is not compatible with {1} levels. Please choose a different tileset and try again.", Path.GetFileName(filename), J2File.FullVersionNames[J2L.VersionType]), "Incompatible tileset", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (result == VersionChangeResults.TooManyAnimatedTiles) MessageBox.Show(String.Format("Sorry, using {0} would result in the level having too many tiles (counting animated tiles). Please choose a different tileset or reduce the number of animated tiles in the level.", filename), "Too many animated tiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _suspendEvent.Set();
            }
            else RedrawTilesetHowManyTimes = 2;
            SafeToDisplay = true;
            ResizeDisplay();
        }
        internal void ChangeVersion(Version nuversion)
        {
            _suspendEvent.Reset();
            Version aldversion = J2L.VersionType;
            VersionChangeResults result = J2L.ChangeVersion(nuversion);
            switch (result)
            {
                case VersionChangeResults.Success:
                    CheckCurrentVersion();
                    ProcessIni(nuversion);
                    if (nuversion == Version.AGA || aldversion == Version.AGA)
                    {
                        Undoable.Clear();
                        Redoable.Clear();
                    }
                    break;
                case VersionChangeResults.TilesetTooBig:
                    MessageBox.Show("The current tileset has too many tiles to change the version.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case VersionChangeResults.TooManyAnimatedTiles:
                    MessageBox.Show("There are too many tiles and animated tiles to change the version.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case VersionChangeResults.UnsupportedConversion:
                    MessageBox.Show("The desired version change is not yet available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
            _suspendEvent.Set();
        }

        private void resetLevelPasswordToolStripMenuItem_Click(object sender, EventArgs e) { J2L.SetPassword(); LevelHasBeenModified = true; }
        private void passwordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _suspendEvent.Reset();
            string NuPassword = SetPassword.ShowForm();
            if (NuPassword != null) { J2L.SetPassword(NuPassword); LevelHasBeenModified = true; }
            _suspendEvent.Set();
        }

        #region Open
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenJ2LDialog.FileName = J2L.NextLevel + DefaultFileExtensionStrings[DefaultFileExtension[J2L.VersionType]];
            _suspendEvent.Reset();
            DialogResult result = OpenJ2LDialog.ShowDialog();
            if (result == DialogResult.OK && PromptForSaving()) LoadJ2L(OpenJ2LDialog.FileName);
            _suspendEvent.Set();
        }
        internal void LoadJ2L(string filename)
        {
            SafeToDisplay = false;
            //TexturedJ2L TentativeJ2L = new TexturedJ2L();
            string newPassword = null;
            Encoding encoding = null;
            TRYTOOPEN:
            OpeningResults openResults = J2L.OpenLevel(filename, newPassword, DefaultDirectories, encoding);
            if (openResults == OpeningResults.PasswordNeeded || openResults == OpeningResults.WrongPassword)
            {
                _suspendEvent.Reset();
                newPassword = PasswordInputForm.ShowForm(openResults);
                _suspendEvent.Set();
                if (newPassword == null)
                {
                    SafeToDisplay = true;
                    return;
                }
                goto TRYTOOPEN;
            }
            else if (openResults == OpeningResults.IncorrectEncoding)
            {
                DialogResult result = MessageBox.Show("This level was saved with an incorrect encoding. Do you want to automatically fix this level's encoding?", "Incorrect Encoding", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand);
                if (result != DialogResult.OK)
                {
                    SafeToDisplay = true;
                    return;
                }
                encoding = Encoding.UTF8;
                goto TRYTOOPEN;
            }
            else if (openResults == OpeningResults.SecurityEnvelopeDamaged)
            {
                MessageBox.Show("This security envelope of this level has been damaged or is in a format not recognized by MLLE.", "This level cannot be loaded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SafeToDisplay = true;
                return;
            }
            else if (openResults == OpeningResults.Success || openResults == OpeningResults.SuccessfulButAmbiguous)
            {
                if (J2L != null && J2L.TexturesHaveBeenGenerated) J2L.Degenerate_Textures();
                //EventsFromIni = TexturedJ2L.ReadEventIni("MLLEProfile.ini");
                //TexturedJ2L.GenerateEventNameTextures(ref eventtexturenumberlist, EventsFromIni);
                //J2L = TentativeJ2L;
                if (openResults == OpeningResults.SuccessfulButAmbiguous)
                {
                    DialogResult result = MessageBox.Show("This level was saved using an ambiguous file format, making it difficult to load. Is this level intended to be played in Battery Check?", "Battery Check or Jazz 2 version 1.10o?", MessageBoxButtons.YesNo, MessageBoxIcon.Hand);
                    J2L.VersionType = (result == DialogResult.Yes) ? Version.BC : Version.O;
                    CheckCurrentVersion();
                    ProcessIni(J2L.VersionType);
                    J2L.ChangeTileset(J2L.Tileset, false, DefaultDirectories);
                }
                else
                {
                    CheckCurrentVersion();
                    ProcessIni(J2L.VersionType);
                }
                SetTitle(J2L.Name, J2L.FilenameOnly);
                J2L.Generate_Textures(TransparencySource.JJ2_Style, true);
                GL.BindTexture(TextureTarget.Texture2D, J2L.Atlases[0]);
                Undoable.Clear();
                Redoable.Clear();
                SafeToDisplay = true;
                LevelHasBeenModified = encoding != null;
                ChangeLayer(J2L.JCSFocusedLayer);
                MakeProposedScrollbarValueWork(LDScrollH, J2L.JCSHorizontalFocus);
                MakeProposedScrollbarValueWork(LDScrollV, J2L.JCSVerticalFocus);
                IdentifyTileset();
                if (playMusicToolStripMenuItem.Checked) PlayMusic();
                GameTick = 0; GameTime = 0; sw.Restart();
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e) { if (PromptForSaving()) { NewJ2L(); _suspendEvent.Set(); } }
        internal void NewJ2L(Version? version = null)
        {
            SafeToDisplay = false;
            if (J2L != null && J2L.TexturesHaveBeenGenerated) J2L.Degenerate_Textures();
            if (version == null) J2L.NewLevel(J2L.VersionType);
            else J2L.NewLevel((Version)version);
            J2L.FullFilePath = Path.Combine(DefaultDirectories[J2L.VersionType], J2L.FilenameOnly = "Made in MLLE.j2l");
            SetTitle(J2L.Name);
            CheckCurrentVersion();
            ProcessIni(J2L.VersionType);
            J2L.Generate_Blank_Tile_Texture();
            GL.BindTexture(TextureTarget.Texture2D, J2L.Atlases[0]);
            SafeToDisplay = true;
            LevelHasBeenModified = false;
            Undoable.Clear();
            Redoable.Clear();
            ChangeLayer(J2L.JCSFocusedLayer);
            if (playMusicToolStripMenuItem.Checked) PlayMusic();
            GameTick = 0; GameTime = 0; sw.Restart();
            ResizeDisplay();
        }
        #endregion Open

        #region Save
        private void saveToolStripMenuItem_Click(object sender, EventArgs e) { SaveJ2L(); }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _suspendEvent.Reset();
            SaveJ2LDialog.FileName = Path.GetFileName(J2L.FilenameOnly);
            SaveJ2LDialog.FilterIndex = DefaultFileExtension[J2L.VersionType];
            DialogResult result = SaveJ2LDialog.ShowDialog();
            if (result == DialogResult.OK) SaveJ2L(SaveJ2LDialog.FileName);
            _suspendEvent.Set();
        }
        private void SaveAndRun(string filepath, string warningText, bool storeGivenFilename)
        {
            if (SaveJ2L(filepath, false, false, storeGivenFilename) == SavingResults.Success)
            {
                string exe = Path.GetFullPath(Path.Combine(DefaultDirectories[J2L.VersionType], EnableableStrings[J2L.VersionType][EnableableTitles.SaveAndRun]));
                if (File.Exists(exe))
                {
                    string exeFolder = Path.GetDirectoryName(exe) + Path.DirectorySeparatorChar;
                    string relativeFilepath = Uri.UnescapeDataString(
                        new Uri(exeFolder)
                            .MakeRelativeUri(new Uri(filepath))
                            .ToString()
                            .Replace('/', Path.DirectorySeparatorChar)
                    );
                    string extraArgs = EnableableStrings[J2L.VersionType][EnableableTitles.SaveAndRunArgs];
                    var pro = new System.Diagnostics.Process();
                    pro.StartInfo.WorkingDirectory = DefaultDirectories[J2L.VersionType];
                    pro.StartInfo.FileName = EnableableStrings[J2L.VersionType][EnableableTitles.SaveAndRun];
                    pro.StartInfo.Arguments = (extraArgs + " " + relativeFilepath).TrimStart();
                    pro.EnableRaisingEvents = true;
                    pro.Exited += new EventHandler(SaveAndRunProgramHasExited);
                    if (MusicIsPlaying) Bass.BASS_Pause();
                    pro.Start();
                }
                else
                {
                    _suspendEvent.Reset();
                    MessageBox.Show(EnableableStrings[J2L.VersionType][EnableableTitles.SaveAndRun] + " not found!" + warningText, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _suspendEvent.Set();
                }
            }
        }
        private void SaveAndRunProgramHasExited(object sender, System.EventArgs e)
        {
            if (MusicIsPlaying) Bass.BASS_Start();
            string directory = Path.GetDirectoryName(J2L.FullFilePath);
            File.Delete(Path.Combine(directory, "MLLEGenericFilename.j2l"));
            File.Delete(Path.Combine(directory, "MLLEGenericFilename.j2as"));
        }
        private void saveRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAndRun(J2L.FullFilePath, " Level will be saved but not run.", true);
        }

        private void TrialRun()
        {
            string directory = Path.GetDirectoryName(J2L.FullFilePath);
            string originalScriptFilePath = Path.ChangeExtension(J2L.FullFilePath, ".j2as");
            if (File.Exists(originalScriptFilePath))
                File.Copy(originalScriptFilePath, Path.Combine(directory, "MLLEGenericFilename.j2as"), true);
            SaveAndRun(Path.Combine(directory, Path.ChangeExtension("MLLEGenericFilename", DefaultFileExtensionStrings[DefaultFileExtension[J2L.VersionType]])), "", false);
        }
        private void PlayFromHere()
        {
            uint oldEvent = J2L.EventMap[MouseTileX, MouseTileY];
            J2L.EventMap[MouseTileX, MouseTileY] = uint.MaxValue;
            J2L.SwapEvents((uint)StartPositionEventID, uint.MaxValue);
            TrialRun();
            J2L.SwapEvents(uint.MaxValue, (uint)StartPositionEventID);
            J2L.EventMap[MouseTileX, MouseTileY] = oldEvent;
        }
        private void runToolStripMenuItem_Click(object sender, EventArgs e) { TrialRun(); }
        private void DropdownPlayHere_Click(object sender, EventArgs e) { PlayFromHere(); }

        private bool PromptForSaving()
        {
            if (LevelHasBeenModified)
            {
                _suspendEvent.Reset();
                DialogResult result = MessageBox.Show("Save changes to " + Path.GetFileName(J2L.FilenameOnly) + "?", "Level has been modified!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Stop);
                if (result == DialogResult.Yes && SaveJ2L() == SavingResults.Success) { _suspendEvent.Reset(); return true; }
                else if (result == DialogResult.No) return true;
                else return false;
            }
            else return true;
        }
        internal SavingResults SaveJ2L(bool eraseUndefinedTiles = false, bool allowDifferentTilesetVersion = false, bool storeGivenFilename = true)
        {
            return SaveJ2L(J2L.FullFilePath, eraseUndefinedTiles, allowDifferentTilesetVersion, storeGivenFilename);
        }
        internal SavingResults SaveJ2L(string filename, bool eraseUndefinedTiles = false, bool allowDifferentTilesetVersion = false, bool storeGivenFilename = true)
        {
            _suspendEvent.Reset();
            J2L.JCSFocusedLayer = CurrentLayer;
            J2L.JCSHorizontalFocus = (ushort)LDScrollH.Value;
            J2L.JCSVerticalFocus = (ushort)LDScrollV.Value;

            byte[] Data5 = null;
            if (EnableableBools[J2L.VersionType][EnableableTitles.BoolDevelopingForPlus])
                currentPlusPropertyList.CreateData5SectionIfRequiredByLevel(ref Data5);

            SavingResults result = J2L.Save(filename, eraseUndefinedTiles, allowDifferentTilesetVersion, storeGivenFilename, Data5);
            if (result == SavingResults.Success)
            {
                SetTitle(J2L.Name, Path.GetFileName(J2L.FilenameOnly));
                LevelHasBeenModified = false;

                PlusPropertyList.RemovePriorReferencesToMLLELibrary(filename);
                if (Data5 != null)
                    currentPlusPropertyList.SaveLibrary(filename);
            }
            else if (result == SavingResults.NoTilesetSelected)
            {
                MessageBox.Show("The level cannot be saved without a tileset.", "No Tileset Selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (result == SavingResults.TilesetIsDifferentVersion)
            {
                if (J2L.J2T.VersionType == Version.GorH)
                {
                    MessageBox.Show("This level was originally saved as a Jazz 2 OEM v1.00g/h level, and does not have an external tileset file. Please choose an existing tileset file in order to save this level as any other version, or else it will not be playable. This level will not be saved.", "Tileset File Needed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    DialogResult dialogResult = MessageBox.Show(String.Format("You are saving this level as a {0} level, but {2} is only compatible with {1}. In order for the level to be playable, you will need to have and make available a {0}-compatible version of {2}. MLLE will not do this for you. Press 'OK' to continue saving or 'Cancel' to choose a different tileset.", J2File.FullVersionNames[J2L.VersionType], J2File.FullVersionNames[J2L.J2T.VersionType], J2L.Tileset), "Version Difference", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.OK)
                    {
                        result = SaveJ2L(filename, eraseUndefinedTiles, true, storeGivenFilename);
                    }
                }
            }
            else if (result == SavingResults.UndefinedTiles)
            {
                DialogResult dialogResult = MessageBox.Show("References were found to unknown tiles. These references must be deleted in order to save this level.", "Undefined Tiles", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.OK)
                {
                    result = SaveJ2L(filename, true, allowDifferentTilesetVersion, storeGivenFilename);
                }
            }
            else
            {
                //A more specific error must be added instead of using this one. This is only here for troubleshooting purposes.
                MessageBox.Show("There was an error while saving.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            _suspendEvent.Set();
            return result;
        }
        #endregion Save

        private void UpdateTitle(bool modified)
        {
            string title = Text.StartsWith("*") ? Text.Substring(1) : Text;
            SetTitle(title, modified);
        }

        private void SetTitle(string name, string filename = null, bool modified = false)
        {
            string title = name + (filename == null ? "" : " \u2013 " + filename) + " \u2013 MLLE";
            SetTitle(title, modified);
        }

        private void SetTitle(string title, bool modified)
        {
            Text = modified ? "*" + title : title;
        }

        #endregion J2L Extensions

        #region GL
        #region GL-specific variables

        internal bool AnimatedTilesVisibleOnLeft = false;
        internal int AnimatedTilesDrawHeight = 0;
        internal bool ShowBlankTileInStamp = false;
        internal int LevelDisplayViewportWidth, LevelDisplayViewportHeight;
        internal byte RedrawTilesetHowManyTimes = 0;
        internal int xspeedparallax, yspeedparallax;
        internal int xloop = 0, yloop = 0;
        internal int xorigin = 0, tempxorigin = 0;
        internal int yorigin = 0, tempyorigin = 0;
        internal int upperleftx = 0, tempupperleftx = 0;
        internal int upperlefty = 0, tempupperlefty = 0;
        internal int drawxloopsize, drawyloopsize;
        internal int eventpointer = 0;
        internal int xpos = 460;//
        internal int ypos = 640;//
        internal int widthreduced;//
        internal int heightreduced;//
        internal bool isflipped = false;
        internal bool isvflipped = false;
        internal ushort previd = 0;
        //internal byte prevatlas = 0;
        internal AtlasID PrevAtlas = AtlasID.Null;
        //internal double startAtRatioY, startAtRatioX;
        internal Layer DrawingLayer;

        internal Thread DrawThread;

        #endregion

        internal void ResizeDisplay()
        {
            LevelDisplayViewportWidth = LevelDisplay.Width - LDScrollH.Location.X;
            LevelDisplayViewportHeight = LevelDisplay.Height - LDScrollH.Height;
            SetupViewport();
            GL.Scissor(LDScrollH.Location.X, LDScrollH.Height, LevelDisplayViewportWidth, LevelDisplayViewportHeight);
            widthreduced = (LevelDisplayViewportWidth - 320) / 2;
            heightreduced = (LevelDisplayViewportHeight - 200) / 2;
            drawxloopsize = (int)Math.Ceiling(LevelDisplayViewportWidth / (float)ZoomTileSize) + 2;
            drawyloopsize = (int)Math.Ceiling(LevelDisplayViewportHeight / (float)ZoomTileSize) + 2;
            LDScrollH.LargeChange = LDScrollV.LargeChange = (LDScrollH.SmallChange = LDScrollV.SmallChange = ZoomTileSize) * 8;
            if (SafeToDisplay)
            {
                LDScrollH.Maximum = Math.Max(0, (int)J2L.Layers[CurrentLayer].Width * ZoomTileSize - LevelDisplayViewportWidth + LDScrollH.LargeChange);
                LDScrollV.Maximum = Math.Max(0, (int)J2L.Layers[CurrentLayer].Height * ZoomTileSize - LevelDisplayViewportHeight + LDScrollV.LargeChange);
                TilesetScrollbar.Maximum = Math.Max(0, (J2L.J2T == null) ? 0 : ((int)J2L.J2T.TileCount + J2L.NumberOfAnimations + 10) / 10 * 32 - TilesetScrollbar.Height + 256);
                //TilesetScrollbar.Maximum += TilesetScrollbar.LargeChange;
                TilesetScrollbar.Refresh();
                MakeProposedScrollbarValueWork(TilesetScrollbar, TilesetScrollbar.Value);
                DetermineVisibilityOfAnimatedTiles();
                MakeProposedScrollbarValueWork(LDScrollH, LDScrollH.Value);
                MakeProposedScrollbarValueWork(LDScrollV, LDScrollV.Value);
                RedrawTilesetHowManyTimes = 2;
            }
            //ResizingOccured = true;
        }
        private void LevelDisplay_Load(object sender, EventArgs e)
        {
            LevelDisplayLoaded = true;
            //Application.Idle += Application_Idle;
        }
        internal void SetupViewport()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, LevelDisplayViewportWidth, LevelDisplayViewportHeight, 0, -1, 1);
            GL.Viewport(LDScrollH.Location.X, LDScrollH.Height, LevelDisplayViewportWidth, LevelDisplayViewportHeight);
        }
        private void LevelDisplay_Resize(object sender, EventArgs e) { ResizeDisplay(); }

        private void SetTextureTo(AtlasID nuAtlas)
        {
            if (PrevAtlas != nuAtlas)
            {
                int nuInt = 0;
                switch (nuAtlas)
                {
                    case AtlasID.Mask:
                        nuInt = J2L.Atlases[1];
                        break;
                    case AtlasID.Image:
                        nuInt = J2L.Atlases[0];
                        break;
                    case AtlasID.Generator:
                        nuInt = GeneratorOverlay;
                        break;
                    case AtlasID.EventNames:
                        nuInt = (int)J2L.EventAtlas[J2L.VersionType];
                        break;
                    case AtlasID.TileTypes:
                        nuInt = (int)J2L.TileTypeAtlas[J2L.VersionType];
                        break;
                    case AtlasID.Selection:
                        nuInt = SelectionOverlay;
                        break;
                    default:
                        return;
                }
                GL.BindTexture(TextureTarget.Texture2D, nuInt);
                PrevAtlas = nuAtlas;
            }
        }

        internal void TimePasses()
        {
            while (true)
            {
                _suspendEvent.WaitOne(Timeout.Infinite);
                sw.Stop();
                milliseconds = sw.Elapsed.TotalMilliseconds;
                sw.Reset();
                sw.Start();
                idleCounter++;
                accumulator += milliseconds;
                if (accumulator > 1000)
                {
                    LatestFPS = idleCounter;
                    accumulator -= 1000;
                    idleCounter = 0; // don't forget to reset the counter!
                }
                float deltaGametime = (float)milliseconds / 20.0f;
                GameTime += deltaGametime;
                if (SafeToDisplay) for (; GameTick < GameTime; GameTick++) //Advance Frame
                    {
                        foreach (AnimatedTile anim in J2L.Animations)
                        {
                            if (anim.Random != 0) anim.Advance(GameTick, _r.Next(anim.Random + 1));
                            else anim.Advance(GameTick);
                        }
                        if (AnimationSettings.Visible) WorkingAnimation.Advance(GameTick, (WorkingAnimation.Random != 0) ? _r.Next(WorkingAnimation.Random + 1) : 0);
                    }
                else GameTick = (int)GameTime;
                if (SafeToDisplay) Invoke(new MethodInvoker(delegate () { LevelDisplay.Invalidate(); }));
            }
        }

        private void LevelDisplay_Paint(object sender, PaintEventArgs e)
        {
            GametickCounter.Text = "Elapsed: " + GameTick.ToString();
            FPSCounter.Text = "FPS: " + LatestFPS.ToString();
            if (SafeToDisplay)
            {
                #region all tileset stuff
                if (RedrawTilesetHowManyTimes != 0 || AnimatedTilesVisibleOnLeft || AnimationSettings.Visible)
                {
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(0, DrawingTools.Left, LevelDisplay.Height, 0, -1, 1);
                    GL.Viewport(0, 0, DrawingTools.Left, LevelDisplay.Height);
                    GL.Disable(EnableCap.ScissorTest);
                    if (EventDisplayMode || ParallaxButton.Checked) GL.Disable(EnableCap.Blend);
                    if (!(
                        (PrevAtlas == AtlasID.Image && CurrentTilesetOverlay != TilesetOverlay.Masks)
                        ||
                        (PrevAtlas == AtlasID.Mask && CurrentTilesetOverlay == TilesetOverlay.Masks)
                        ))
                        SetTextureTo((CurrentTilesetOverlay == TilesetOverlay.Masks) ? AtlasID.Mask : AtlasID.Image);
                    int tile = TilesetScrollbar.Value / 32 * 10;
                    if (RedrawTilesetHowManyTimes != 0)
                    {
                        #region draw tileset
                        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                        if (J2L.J2T != null)
                        {
                            //uint height = ((prevatlas >= J2L.J2T.TileCount / 1030) ? J2L.J2T.TileCount % 1030 : 1030) / 10 * 32;

                            double yfraction;
                            double xfraction;
                            for (int yoffset = -(TilesetScrollbar.Value % 32); yoffset < TilesetScrollbar.Height && tile < J2L.J2T.TileCount; tile += 10)
                            {
                                xfraction = tile % J2L.AtlasLength * J2L.AtlasFraction;
                                yfraction = tile / J2L.AtlasLength * J2L.AtlasFraction;
                                if (tile % J2L.AtlasLength + 9 < J2L.AtlasLength)
                                {
                                    GL.Begin(BeginMode.Quads);
                                    GL.TexCoord2(xfraction + J2L.AtlasFraction * 10, yfraction); GL.Vertex2(320, yoffset);
                                    GL.TexCoord2(xfraction, yfraction); GL.Vertex2(0, yoffset);
                                    yfraction += J2L.AtlasFraction; yoffset += 32;
                                    GL.TexCoord2(xfraction, yfraction); GL.Vertex2(0, yoffset);
                                    GL.TexCoord2(xfraction + J2L.AtlasFraction * 10, yfraction); GL.Vertex2(320, yoffset);
                                    GL.End();
                                }
                                else
                                {
                                    byte width = (byte)(J2L.AtlasLength - tile % J2L.AtlasLength);
                                    GL.Begin(BeginMode.Quads);
                                    GL.TexCoord2(xfraction + J2L.AtlasFraction * width, yfraction); GL.Vertex2(32 * width, yoffset);
                                    GL.TexCoord2(xfraction, yfraction); GL.Vertex2(0, yoffset);
                                    yfraction += J2L.AtlasFraction; yoffset += 32;
                                    GL.TexCoord2(xfraction, yfraction); GL.Vertex2(0, yoffset);
                                    GL.TexCoord2(xfraction + J2L.AtlasFraction * width, yfraction); GL.Vertex2(32 * width, yoffset);
                                    GL.End();
                                    xfraction = 0; yoffset -= 32;
                                    GL.Begin(BeginMode.Quads);
                                    GL.TexCoord2(xfraction + J2L.AtlasFraction * (10 - width), yfraction); GL.Vertex2(320, yoffset);
                                    GL.TexCoord2(xfraction, yfraction); GL.Vertex2(32 * width, yoffset);
                                    yfraction += J2L.AtlasFraction; yoffset += 32;
                                    GL.TexCoord2(xfraction, yfraction); GL.Vertex2(32 * width, yoffset);
                                    GL.TexCoord2(xfraction + J2L.AtlasFraction * (10 - width), yfraction); GL.Vertex2(320, yoffset);
                                    GL.End();
                                }
                            }
                        }
                        #endregion draw tileset
                        if (CurrentTilesetOverlay == TilesetOverlay.TileTypes)
                        {
                            #region draw tile types
                            GL.Enable(EnableCap.Blend);
                            SetTextureTo(AtlasID.TileTypes);
                            for (tile = TilesetScrollbar.Value / 32 * 10; tile / 10 * 32 - TilesetScrollbar.Value < LevelDisplay.Height + 31 && tile < J2L.MaxTiles; tile++)
                            {
                                if (J2L.TileTypes[tile] != 0) DrawTileType(tile % 10 * 32, tile / 10 * 32 - TilesetScrollbar.Value, J2L.TileTypes[tile]);
                            }
                            SetTextureTo(AtlasID.Image);
                            GL.Disable(EnableCap.Blend);
                            #endregion draw tile types
                        }
                    }
                    if (AnimatedTilesVisibleOnLeft)
                    {
                        #region draw animated tiles
                        var count = Math.Min(J2L.NumberOfAnimations, (TilesetScrollbar.Height - AnimatedTilesDrawHeight + 31) / 32 * 10);
                        int x = 0, y = AnimatedTilesDrawHeight;
                        for (byte i = 0; i < count; i++)
                        {
                            DrawTile(ref x, ref y, (ushort)(J2L.AnimOffset + i), 32, true);
                            if (x == 288) { x = 0; y += 32; }
                            else { x += 32; }
                        }
                        DrawColorRectangle(ref x, ref y, new Color4(24, 24, 48, 255));
                        #endregion draw animated tiles
                    }
                    if (AnimationSettings.Visible)
                    {
                        #region draw current animation
                        int y = AnimationSettings.Bottom - LevelDisplay.Top;
                        int x = -AnimScrollbar.Value;
                        DrawTile(ref x, ref y, WorkingAnimation.FrameList.Peek(), 32, true);
                        x += 32;
                        DrawColorRectangle(ref x, ref y, HotKolors[0], 32, 16);
                        x += 16;
                        for (byte i = 0; i < WorkingAnimation.FrameCount && x < AnimScrollbar.Width; i++, x += 32)
                        {
                            DrawTile(ref x, ref y, WorkingAnimation.Sequence[i], 32, true);
                        }
                        DrawColorRectangle(ref x, ref y, new Color4(24, 24, 48, 255));
                        x = SelectedAnimationFrame * 32 + 48 - AnimScrollbar.Value;
                        DrawColorRectangle(ref x, ref y, new Color4(255, 255, 255, 128));
                        #endregion draw current animation
                    }
                    if (CurrentTilesetOverlay == TilesetOverlay.Events)
                    {
                        #region draw event names
                        if (RedrawTilesetHowManyTimes != 0)
                        {
                            GL.Enable(EnableCap.Blend);
                            SetTextureTo(AtlasID.EventNames);
                            for (tile = TilesetScrollbar.Value / 32 * 10; tile / 10 * 32 - TilesetScrollbar.Value < LevelDisplay.Height + 31 && tile < J2L.MaxTiles; tile++)
                            {
                                if (J2L.EventTiles[tile] != 0) DrawEvent(tile % 10 * 32, tile / 10 * 32 - TilesetScrollbar.Value, J2L.EventTiles[tile]);
                            }
                        }
                        if (AnimatedTilesVisibleOnLeft)
                        {
                            if (RedrawTilesetHowManyTimes == 0)
                            {
                                GL.Enable(EnableCap.Blend);
                                SetTextureTo(AtlasID.EventNames);
                            }
                            var count = J2L.AnimOffset + Math.Min(J2L.NumberOfAnimations, (TilesetScrollbar.Height - AnimatedTilesDrawHeight + 31) / 32 * 10);
                            int x = 0, y = AnimatedTilesDrawHeight;
                            for (int i = J2L.AnimOffset; i < count; i++)
                            {
                                if (J2L.EventTiles[i] != 0) DrawEvent(x, y, J2L.EventTiles[i]);
                                if (x == 288) { x = 0; y += 32; }
                                else { x += 32; }
                            }
                        }
                        #endregion draw event names
                    }
                    #region selection
                    if (HowSelecting == FocusedZone.Tileset)
                    {
                        GL.Enable(EnableCap.Blend);
                        int[] Rect = new int[4];
                        Rect[0] = Math.Min(SelectionBoxCorners[0], SelectionBoxCorners[2]) * 32;
                        Rect[1] = Math.Min(SelectionBoxCorners[1], SelectionBoxCorners[3]) * 32 - TilesetScrollbar.Value;
                        Rect[2] = Math.Abs(SelectionBoxCorners[0] - SelectionBoxCorners[2]) * 32 + Rect[0];
                        Rect[3] = Math.Abs(SelectionBoxCorners[1] - SelectionBoxCorners[3]) * 32 + Rect[1];
                        DrawSelectionRectangle(Rect, 32);
                        RedrawTilesetHowManyTimes = 2;
                    }
                    if (WhereSelected == FocusedZone.Tileset)
                    {
                        EmborderSelectedTiles(0, TilesetScrollbar.Value, 32, 320, LevelDisplay.Height);
                        RedrawTilesetHowManyTimes = 2;
                    }
                    #endregion selection
                    else if (ParallaxDisplayMode != ParallaxMode.NoParallax) GL.Enable(EnableCap.Blend);
                    if (RedrawTilesetHowManyTimes != 0) RedrawTilesetHowManyTimes--;
                    SetupViewport();
                    GL.Enable(EnableCap.ScissorTest);
                }
                #endregion all tileset stuff
                #region reindeer
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                if (ParallaxDisplayMode != ParallaxMode.NoParallax && MaskDisplayMode != MaskMode.FullMask)
                {
                    xspeedparallax = (J2L.Layers[CurrentLayer].XSpeed == 0) ? 0 : (int)((LDScrollH.Value + widthreduced) / J2L.Layers[CurrentLayer].XSpeed);
                    yspeedparallax = (J2L.Layers[CurrentLayer].YSpeed == 0) ? 0 : (int)((LDScrollV.Value + (J2L.Layers[CurrentLayer].LimitVisibleRegion ? heightreduced * 2 : heightreduced)) / J2L.Layers[CurrentLayer].YSpeed);
                    SetTextureTo(AtlasID.Image);
                    GL.Enable(EnableCap.Blend);
                    if (ParallaxDisplayMode == ParallaxMode.TemporaryParallax) GL.Color4((byte)255, (byte)255, (byte)255, (byte)64);
                    for (sbyte l = 7; l >= 0; l--)
                    {
                        DrawingLayer = J2L.Layers[l];
                        if (l == CurrentLayer)
                        {
                            GL.Color4((byte)255, (byte)255, (byte)255, (byte)255);
                            if (DrawingLayer.HasTiles) Reindeer(DrawingLayer);
                            if (MaskDisplayMode == MaskMode.TemporaryMask)
                            {
                                SetTextureTo(AtlasID.Mask);
                                NoParallaxReindeer(DrawingLayer);
                                if (l == 3 && ParallaxEventDisplayType == 0 && EventDisplayMode) EventReindeer();
                                SetTextureTo(AtlasID.Image);
                            }
                            else if (l == 3 && ParallaxEventDisplayType == 0 && EventDisplayMode) { EventReindeer(); SetTextureTo(AtlasID.Image); }
                            if (ParallaxDisplayMode == ParallaxMode.TemporaryParallax) GL.Color4((byte)255, (byte)255, (byte)255, (byte)64);
                        }
                        else
                        {
                            if (DrawingLayer.HasTiles) Reindeer(DrawingLayer);
                            if (l == 3 && ParallaxEventDisplayType == 0 && EventDisplayMode) { EventReindeer(); SetTextureTo(AtlasID.Image); }
                        }
                    }
                    if (ParallaxEventDisplayType == 1 && EventDisplayMode) EventReindeer();
                }
                else
                {
                    xspeedparallax = LDScrollH.Value + widthreduced;
                    yspeedparallax = LDScrollV.Value + heightreduced;
                    GL.Disable(EnableCap.Blend);
                    if (MaskDisplayMode == MaskMode.TemporaryMask)
                    {
                        SetTextureTo(AtlasID.Image);
                        NoParallaxReindeer(J2L.Layers[CurrentLayer]);
                        SetTextureTo(AtlasID.Mask);
                        GL.Enable(EnableCap.Blend);
                        NoParallaxReindeer(J2L.Layers[CurrentLayer]);
                        if (CurrentLayer == 3 && EventDisplayMode) EventReindeer();
                    }
                    else
                    {
                        SetTextureTo((MaskDisplayMode == MaskMode.FullMask) ? AtlasID.Mask : AtlasID.Image);
                        NoParallaxReindeer(J2L.Layers[CurrentLayer]);
                        if (CurrentLayer == 3 && EventDisplayMode) { GL.Enable(EnableCap.Blend); EventReindeer(); }
                    }
                }
                #endregion reindeer
                if (HowSelecting == FocusedZone.Level)
                {
                    int[] Rect = new int[4];
                    Rect[0] = Math.Min(SelectionBoxCorners[0], SelectionBoxCorners[2]) * ZoomTileSize - LDScrollH.Value;
                    Rect[1] = Math.Min(SelectionBoxCorners[1], SelectionBoxCorners[3]) * ZoomTileSize - LDScrollV.Value;
                    Rect[2] = Math.Abs(SelectionBoxCorners[0] - SelectionBoxCorners[2]) * ZoomTileSize + Rect[0];
                    Rect[3] = Math.Abs(SelectionBoxCorners[1] - SelectionBoxCorners[3]) * ZoomTileSize + Rect[1];
                    DrawSelectionRectangle(Rect, ZoomTileSize);
                }
                if (WhereSelected == FocusedZone.Level)
                {
                    EmborderSelectedTiles(LDScrollH.Value, LDScrollV.Value, ZoomTileSize, LevelDisplayViewportWidth, LevelDisplayViewportHeight);
                }
                if (LastFocusedZone == FocusedZone.Level && VisibleEditingTool != SelectionButton && CurrentStamp.Length > 0 && J2L.J2T != null)
                {
                    int x = MouseTileX * ZoomTileSize - LDScrollH.Value;
                    int y = MouseTileY * ZoomTileSize - LDScrollV.Value;
                    SetTextureTo((MaskDisplayMode == MaskMode.FullMask) ? AtlasID.Mask : AtlasID.Image);
                    GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
                    GL.Color4((byte)255, (byte)255, (byte)255, (byte)128);
                    for (int xloop = 0, xoffset = x; xloop < CurrentStamp.Length; xloop++, xoffset += ZoomTileSize) for (int yloop = 0, yoffset = y; yloop < CurrentStamp[0].Length; yloop++, yoffset += ZoomTileSize)
                        {
                            if (CurrentStamp[xloop][yloop].Tile != null) DrawTile(ref xoffset, ref yoffset, (ushort)CurrentStamp[xloop][yloop].Tile, ZoomTileSize, Control.ModifierKeys == Keys.Shift || ShowBlankTileInStamp);
                        }
                    GL.Color4((byte)255, (byte)255, (byte)255, (byte)255);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                }
            }

            LevelDisplay.SwapBuffers();
        }

        internal void DrawSelectionRectangle(int[] Rect, byte TileSize)
        {
            GL.Enable(EnableCap.Blend);
            SetTextureTo(AtlasID.Selection);
            if (CurrentSelectionType == SelectionType.Rectangle || CurrentSelectionType == SelectionType.HollowRectangle) GL.Color4((byte)255, (byte)63, (byte)0, (byte)255);
            GL.Begin(BeginMode.Quads); //top
            GL.TexCoord2(0, .5); GL.Vertex2(Rect[0], Rect[1]);
            GL.TexCoord2(0, .75); GL.Vertex2(Rect[0], Rect[1] + TileSize / 2);
            GL.TexCoord2(.5, .75); GL.Vertex2(Rect[2] + TileSize, Rect[1] + TileSize / 2);
            GL.TexCoord2(.5, .5); GL.Vertex2(Rect[2] + TileSize, Rect[1]);
            GL.End(); //top
            GL.Begin(BeginMode.Quads); //bottom
            GL.TexCoord2(0, .75); GL.Vertex2(Rect[0], Rect[3] + TileSize / 2);
            GL.TexCoord2(0, 1); GL.Vertex2(Rect[0], Rect[3] + TileSize);
            GL.TexCoord2(.5, 1); GL.Vertex2(Rect[2] + TileSize, Rect[3] + TileSize);
            GL.TexCoord2(.5, .75); GL.Vertex2(Rect[2] + TileSize, Rect[3] + TileSize / 2);
            GL.End(); //bottom
            GL.Begin(BeginMode.Quads); //left
            GL.TexCoord2(.5, .5); GL.Vertex2(Rect[0], Rect[1]);
            GL.TexCoord2(.5, 1); GL.Vertex2(Rect[0], Rect[3] + TileSize);
            GL.TexCoord2(.75, 1); GL.Vertex2(Rect[0] + TileSize / 2, Rect[3] + TileSize);
            GL.TexCoord2(.75, .5); GL.Vertex2(Rect[0] + TileSize / 2, Rect[1]);
            GL.End(); //left
            GL.Begin(BeginMode.Quads); //right
            GL.TexCoord2(.75, .5); GL.Vertex2(Rect[2] + TileSize / 2, Rect[1]);
            GL.TexCoord2(.75, 1); GL.Vertex2(Rect[2] + TileSize / 2, Rect[3] + TileSize);
            GL.TexCoord2(1, 1); GL.Vertex2(Rect[2] + TileSize, Rect[3] + TileSize);
            GL.TexCoord2(1, .5); GL.Vertex2(Rect[2] + TileSize, Rect[1]);
            GL.End(); //right
            if (CurrentSelectionType == SelectionType.Rectangle || CurrentSelectionType == SelectionType.HollowRectangle) GL.Color4((byte)255, (byte)255, (byte)255, (byte)255);
        }
        internal void EmborderSelectedTiles(int xOffset, int yOffset, byte TileSize, int xMax, int yMax)
        {
            int animFrame = (GameTick / 4);
            SetTextureTo(AtlasID.Selection);
            GL.Enable(EnableCap.Blend);
            float majorDrawSize = TileSize / 64F, minorDrawSize = TileSize / 256F;
            //xMax += xOffset;
            //yMax += yOffset;
            int xPos, yPos, x, y;
            for (x = xOffset / TileSize + 1, xPos = -(xOffset % TileSize); xPos < xMax; x++, xPos += TileSize)
                for (y = yOffset / TileSize + 1, yPos = -(yOffset % TileSize); yPos < yMax; y++, yPos += TileSize)
                    if (IsEachTileSelected[x][y])
                    {
                        animFrame = (animFrame + 2) % 4;
                        if (!IsEachTileSelected[x][y - 1]) //top
                        {
                            GL.Begin(BeginMode.Quads);
                            GL.TexCoord2(0, .125 * animFrame); GL.Vertex2(xPos, yPos);
                            GL.TexCoord2(0, minorDrawSize + .125 * animFrame); GL.Vertex2(xPos, yPos + TileSize / 4);
                            GL.TexCoord2(majorDrawSize, minorDrawSize + .125 * animFrame); GL.Vertex2(xPos + TileSize, yPos + TileSize / 4);
                            GL.TexCoord2(majorDrawSize, .125 * animFrame); GL.Vertex2(xPos + TileSize, yPos);
                            GL.End();
                        }
                        if (!IsEachTileSelected[x - 1][y]) //left
                        {
                            GL.Begin(BeginMode.Quads);
                            GL.TexCoord2(.5 + .125 * animFrame, 0); GL.Vertex2(xPos, yPos);
                            GL.TexCoord2(.5 + .125 * animFrame, majorDrawSize); GL.Vertex2(xPos, yPos + TileSize);
                            GL.TexCoord2(.5 + minorDrawSize + .125 * animFrame, majorDrawSize); GL.Vertex2(xPos + TileSize / 4, yPos + TileSize);
                            GL.TexCoord2(.5 + minorDrawSize + .125 * animFrame, 0); GL.Vertex2(xPos + TileSize / 4, yPos);
                            GL.End();
                        }
                        animFrame = (animFrame + 2) % 4;
                        if (!IsEachTileSelected[x][y + 1]) //bottom
                        {
                            GL.Begin(BeginMode.Quads);
                            GL.TexCoord2(0, .125 * animFrame); GL.Vertex2(xPos + TileSize, yPos + TileSize);
                            GL.TexCoord2(0, minorDrawSize + .125 * animFrame); GL.Vertex2(xPos + TileSize, yPos + TileSize * .75);
                            GL.TexCoord2(majorDrawSize, minorDrawSize + .125 * animFrame); GL.Vertex2(xPos, yPos + TileSize * .75);
                            GL.TexCoord2(majorDrawSize, .125 * animFrame); GL.Vertex2(xPos, yPos + TileSize);
                            GL.End();
                        }
                        if (!IsEachTileSelected[x + 1][y]) //right
                        {
                            GL.Begin(BeginMode.Quads);
                            GL.TexCoord2(.5 + .125 * animFrame, 0); GL.Vertex2(xPos + TileSize, yPos + TileSize);
                            GL.TexCoord2(.5 + .125 * animFrame, majorDrawSize); GL.Vertex2(xPos + TileSize, yPos);
                            GL.TexCoord2(.5 + minorDrawSize + .125 * animFrame, majorDrawSize); GL.Vertex2(xPos + TileSize * .75, yPos);
                            GL.TexCoord2(.5 + minorDrawSize + .125 * animFrame, 0); GL.Vertex2(xPos + TileSize * .75, yPos + TileSize);
                            GL.End();
                        }
                    }
        }

        internal void DrawColorRectangle(ref int x, ref int y, Color4 color, byte tileWidth = 32, byte tileHeight = 32)
        {
            GL.Color4(color);
            GL.Disable(EnableCap.Texture2D);
            if (color.A != 1) GL.Enable(EnableCap.Blend);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(x, y);
            GL.Vertex2(x + tileWidth, y);
            GL.Vertex2(x + tileWidth, y + tileWidth);
            GL.Vertex2(x, y + tileWidth);
            GL.End();
            GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Enable(EnableCap.Texture2D);
        }
        internal void DrawTile(ref int x, ref int y, ushort id, byte tileSize, bool DrawTileZero = false)
        {
            if (id != 0 || DrawTileZero)
            {
                if (id != previd)
                {
                    isflipped = false;
                    isvflipped = false;
                    id = J2L.GetFrame(id, ref isflipped, ref isvflipped);
                    if (id == 0 && !DrawTileZero) return;
                    previd = id;
                }
                double startAtRatioX = id % J2L.AtlasLength * J2L.AtlasFraction;
                double startAtRatioY = id / J2L.AtlasLength;
                double yChange = J2L.AtlasFraction;
                if (isvflipped)
                {
                    yChange *= -1;
                    startAtRatioY += 1;
                }
                startAtRatioY *= J2L.AtlasFraction;
                GL.Begin(BeginMode.Quads);
                if (isflipped)
                {
                    GL.TexCoord2(startAtRatioX + J2L.AtlasFraction, startAtRatioY); GL.Vertex2(x, y);
                    GL.TexCoord2(startAtRatioX, startAtRatioY); GL.Vertex2(x + tileSize, y);
                    startAtRatioY += yChange;
                    GL.TexCoord2(startAtRatioX, startAtRatioY); GL.Vertex2(x + tileSize, y + tileSize);
                    GL.TexCoord2(startAtRatioX + J2L.AtlasFraction, startAtRatioY); GL.Vertex2(x, y + tileSize);
                }
                else
                {
                    GL.TexCoord2(startAtRatioX, startAtRatioY); GL.Vertex2(x, y);
                    GL.TexCoord2(startAtRatioX + J2L.AtlasFraction, startAtRatioY); GL.Vertex2(x + tileSize, y);
                    startAtRatioY += yChange;
                    GL.TexCoord2(startAtRatioX + J2L.AtlasFraction, startAtRatioY); GL.Vertex2(x + tileSize, y + tileSize);
                    GL.TexCoord2(startAtRatioX, startAtRatioY); GL.Vertex2(x, y + tileSize);
                }
                GL.End();
            }
        }
        internal void DrawEvent(int x, int y, uint id, byte TileSize = 32)
        {
            uint difficulty = id << 22 >> 30;
            //previd = 40000;
            GL.Color4((byte)255, (difficulty < 2) ? (byte)255 : (byte)0, (difficulty % 3 == 0) ? (byte)255 : (byte)0, (byte)255);
            byte drawid = (byte)(((id & 255) == GeneratorEventID) ? id << 12 >> 24 : id & 255);
            float xFrac = (drawid % 16) * 0.0625F, yFrac = (int)(drawid / 16) * 0.0625F;
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(xFrac, yFrac + 0.0625F); GL.Vertex2(x, y + TileSize);
            GL.TexCoord2(xFrac, yFrac); GL.Vertex2(x, y);
            GL.TexCoord2(xFrac + 0.0625F, yFrac); GL.Vertex2(x + TileSize, y);
            GL.TexCoord2(xFrac + 0.0625F, yFrac + 0.0625F); GL.Vertex2(x + TileSize, y + TileSize);
            GL.End();
            if (difficulty != 5) GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
            if ((id & 255) == GeneratorEventID)
            {
                SetTextureTo(AtlasID.Generator);
                GL.Begin(BeginMode.Quads);
                GL.TexCoord2(0, 1); GL.Vertex2(x, y + TileSize);
                GL.TexCoord2(0, 0); GL.Vertex2(x, y);
                GL.TexCoord2(1, 0); GL.Vertex2(x + TileSize, y);
                GL.TexCoord2(1, 1); GL.Vertex2(x + TileSize, y + TileSize);
                GL.End();
                SetTextureTo(AtlasID.EventNames);
            }
        }
        internal void DrawTileType(int x, int y, byte id)
        {
            float xFrac = (id % 4) * 0.25F, yFrac = (int)(id / 4) * 0.25F;
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(xFrac, yFrac + 0.25F); GL.Vertex2(x, y + 32);
            GL.TexCoord2(xFrac, yFrac); GL.Vertex2(x, y);
            GL.TexCoord2(xFrac + 0.25F, yFrac); GL.Vertex2(x + 32, y);
            GL.TexCoord2(xFrac + 0.25F, yFrac + 0.25F); GL.Vertex2(x + 32, y + 32);
            GL.End();
        }

        internal void Reindeer(Layer currentlayer)
        {
            currentlayer.GetFixedCornerOriginNumbers(xspeedparallax, yspeedparallax, widthreduced, heightreduced, ref xorigin, ref yorigin, ref upperleftx, ref upperlefty, ZoomTileSize, EnableableBools[J2L.VersionType][EnableableTitles.BoolDevelopingForPlus]);
            tempxorigin = xorigin; tempupperleftx = upperleftx;
            if (currentlayer.TileWidth)
            {
                if (tempupperleftx < 0) tempupperleftx += (int)currentlayer.Width * 1024;
                if (currentlayer.TileHeight)
                {
                    if (upperlefty < 0) upperlefty += (int)currentlayer.Height * 1024;
                    for (xloop = 0; xloop < drawxloopsize; xloop++)
                    {
                        if (tempupperleftx >= currentlayer.Width) tempupperleftx %= (int)currentlayer.Width;
                        tempyorigin = yorigin; tempupperlefty = upperlefty;
                        for (yloop = 0; yloop < drawyloopsize; yloop++)
                        {
                            if (tempupperlefty >= currentlayer.Height) tempupperlefty %= (int)currentlayer.Height;
                            DrawTile(ref tempxorigin, ref tempyorigin, currentlayer.TileMap[tempupperleftx, tempupperlefty], ZoomTileSize);
                            tempyorigin += ZoomTileSize; tempupperlefty++;
                        }
                        tempxorigin += ZoomTileSize; tempupperleftx++;
                    }
                }
                else
                {
                    for (xloop = 0; xloop < drawxloopsize; xloop++)
                    {
                        if (tempupperleftx >= currentlayer.Width) tempupperleftx %= (int)currentlayer.Width;
                        tempyorigin = yorigin; tempupperlefty = upperlefty;
                        for (yloop = 0; yloop < drawyloopsize; yloop++)
                        {
                            if (tempupperlefty >= currentlayer.Height) break;
                            else if (tempupperlefty >= 0) DrawTile(ref tempxorigin, ref tempyorigin, currentlayer.TileMap[tempupperleftx, tempupperlefty], ZoomTileSize);
                            tempyorigin += ZoomTileSize; tempupperlefty++;
                        }
                        tempxorigin += ZoomTileSize; tempupperleftx++;
                    }
                }
            }
            else
            {
                if (currentlayer.TileHeight)
                {
                    if (upperlefty < 0) upperlefty += (int)currentlayer.Height * 1024;
                    for (xloop = 0; xloop < drawxloopsize; xloop++)
                    {
                        tempyorigin = yorigin; tempupperlefty = upperlefty;
                        if (tempupperleftx >= currentlayer.Width) break;
                        else if (tempupperleftx >= 0) for (yloop = 0; yloop < drawyloopsize; yloop++)
                            {
                                if (tempupperlefty >= currentlayer.Height) tempupperlefty %= (int)currentlayer.Height;
                                DrawTile(ref tempxorigin, ref tempyorigin, currentlayer.TileMap[tempupperleftx, tempupperlefty], ZoomTileSize);
                                tempyorigin += ZoomTileSize; tempupperlefty++;
                            }
                        tempxorigin += ZoomTileSize; tempupperleftx++;
                    }
                }
                else
                {
                    for (xloop = 0; xloop < drawxloopsize; xloop++)
                    {
                        tempyorigin = yorigin; tempupperlefty = upperlefty;
                        if (tempupperleftx >= 0) for (yloop = 0; yloop < drawyloopsize; yloop++)
                            {
                                if (tempupperleftx >= currentlayer.Width || tempupperlefty >= currentlayer.Height) break;
                                else if (tempupperlefty >= 0)
                                {
                                    DrawTile(ref tempxorigin, ref tempyorigin, currentlayer.TileMap[tempupperleftx, tempupperlefty], ZoomTileSize);
                                    //if (EventDisplayMode && currentlayer.id == 3 && J2L.EventMap[tempupperleftx, tempupperlefty] != 0) DrawEvent(ref tempxorigin, ref tempyorigin, ref J2L.EventMap[tempupperleftx, tempupperlefty], J2L.GetRawBitsAtTile(tempupperleftx, tempupperlefty, 0, 2));
                                }
                                tempyorigin += ZoomTileSize; tempupperlefty++;
                            }
                        tempxorigin += ZoomTileSize; tempupperleftx++;
                    }
                }
            }
        }
        internal void NoParallaxReindeer(Layer currentlayer)
        {
            //currentlayer.GetOriginNumbers(LDScrollH.Value, LDScrollV.Value, ref widthreduced, ref heightreduced, ref xorigin, ref yorigin, ref upperleftx, ref upperlefty);
            upperleftx = LDScrollH.Value /*- widthreduced*/ - ZoomTileSize;
            upperlefty = LDScrollV.Value /*- heightreduced*/ - ZoomTileSize;
            xorigin = -ZoomTileSize - (upperleftx % ZoomTileSize);
            upperleftx /= ZoomTileSize;
            yorigin = -ZoomTileSize - (upperlefty % ZoomTileSize);
            upperlefty /= ZoomTileSize;
            tempxorigin = xorigin; tempupperleftx = upperleftx;
            for (xloop = 0; xloop < drawxloopsize; xloop++)
            {
                tempyorigin = yorigin; tempupperlefty = upperlefty;
                if (tempupperleftx >= 0) for (yloop = 0; yloop < drawyloopsize; yloop++)
                    {
                        if (tempupperleftx >= currentlayer.Width || tempupperlefty >= currentlayer.Height) break;
                        else if (tempupperlefty >= 0)
                        {
                            DrawTile(ref tempxorigin, ref tempyorigin, currentlayer.TileMap[tempupperleftx, tempupperlefty], ZoomTileSize, true);
                            //if (EventDisplayMode && currentlayer.id == 3 && J2L.EventMap[tempupperleftx, tempupperlefty] != 0) DrawEvent(ref tempxorigin, ref tempyorigin, ref J2L.EventMap[tempupperleftx, tempupperlefty], J2L.GetRawBitsAtTile(tempupperleftx, tempupperlefty, 0, 2));
                        }
                        tempyorigin += ZoomTileSize; tempupperlefty++;
                    }
                tempxorigin += ZoomTileSize; tempupperleftx++;
            }
        }
        internal void EventReindeer()
        {
            Layer currentlayer = J2L.Layers[3];
            SetTextureTo(AtlasID.EventNames);
            //upperleftx = xspeedparallax - /*widthreduced -*/ ZoomTileSize;
            //upperlefty = yspeedparallax - /*heightreduced -*/ ZoomTileSize;
            //xorigin = -ZoomTileSize - (upperleftx % ZoomTileSize);
            //upperleftx /= ZoomTileSize;
            //yorigin = -ZoomTileSize - (upperlefty % ZoomTileSize);
            //upperlefty /= ZoomTileSize;
            //tempxorigin = xorigin; tempupperleftx = upperleftx;
            currentlayer.GetFixedCornerOriginNumbers(xspeedparallax, yspeedparallax, widthreduced, heightreduced, ref xorigin, ref yorigin, ref upperleftx, ref upperlefty, ZoomTileSize, false);
            tempxorigin = xorigin; tempupperleftx = upperleftx;
            for (xloop = 0; xloop < drawxloopsize; xloop++)
            {
                tempyorigin = yorigin; tempupperlefty = upperlefty;
                if (tempupperleftx >= 0) for (yloop = 0; yloop < drawyloopsize; yloop++)
                    {
                        if (tempupperleftx >= currentlayer.Width || tempupperlefty >= currentlayer.Height) break;
                        else if (tempupperlefty >= 0)
                        {
                            if (J2L.VersionType == Version.AGA) { if (J2L.AGA_EventMap[tempupperleftx, tempupperlefty].ID != 0) DrawEvent(tempxorigin, tempyorigin, J2L.AGA_EventMap[tempupperleftx, tempupperlefty].ID, ZoomTileSize); }
                            else if (J2L.EventMap[tempupperleftx, tempupperlefty] != 0) DrawEvent(tempxorigin, tempyorigin, J2L.EventMap[tempupperleftx, tempupperlefty]/*, J2L.GetRawBitsAtTile(tempupperleftx, tempupperlefty, 0, 2)*/, ZoomTileSize);
                        }
                        tempyorigin += ZoomTileSize; tempupperlefty++;
                    }
                tempxorigin += ZoomTileSize; tempupperleftx++;
            }
        }

        /*internal void DrawAnimatedTiles()
        {
            var count = Math.Min(J2L.NumberOfAnimations,(TilesetScrollbar.Height - AnimatedTilesDrawHeight + 31) / 32 * 10);
            int x = 0, y = AnimatedTilesDrawHeight;
            for (byte i = 0; i < count; i++)
            {
                DrawTile(ref x, ref y, (ushort)(J2L.AnimOffset + i), 32, true);
                if (x == 288) { x = 0; y += 32; }
                else { x += 32; }
            }
            DrawColorTile(ref x, ref y, new Color4(24, 24, 48, 255));
            if (TilesetOverlaySelection.SelectedIndex == 1)
            {
                GL.Enable(EnableCap.Blend);
                SetTextureTo(AtlasID.EventNames);
                x = 0; y = AnimatedTilesDrawHeight;
                for (byte i = 0; i < count; i++)
                {
                    DrawEvent(x, y, J2L.EventTiles[J2L.AnimOffset+i]);
                    if (x == 288) { x = 0; y += 32; }
                    else { x += 32; }
                }
            }
        }
        internal void DrawCurrentAnimation()
        {
            int y = AnimationSettings.Bottom - LevelDisplay.Top;
            int x = 0;
            DrawTile(ref x, ref y, WorkingAnimation.FrameList.Peek(), 32, true);
            x = 48;
            for (byte i = 0; i < WorkingAnimation.FrameCount && i < 10; i++, x+=32)
            {
                DrawTile(ref x, ref y, WorkingAnimation.Sequence[i], 32, true);
            }
            DrawColorTile(ref x, ref y, new Color4(24, 24, 48, 255));
            x = SelectedAnimationFrame*32 + 48 - AnimScrollbar.Value;
            DrawColorTile(ref x, ref y, new Color4(255, 255, 255, 128));
        }*/

        internal void DetermineVisibilityOfAnimatedTiles()
        {
            if (J2L.J2T != null)
            {
                AnimatedTilesDrawHeight = (int)(J2L.J2T.TileCount * 3.2) - TilesetScrollbar.Value;
                AnimatedTilesVisibleOnLeft = AnimatedTilesDrawHeight < LevelDisplay.Height;
            }
            else { AnimatedTilesVisibleOnLeft = false; }
        }

        private void TilesetScrollbar_ValueChanged(object sender, EventArgs e)
        {
            RedrawTilesetHowManyTimes = 2; DetermineVisibilityOfAnimatedTiles();
        }
        /*private void DrawTileset()
        {
            if (J2L.J2T != null)
            {
                //uint height = ((prevatlas >= J2L.J2T.TileCount / 1030) ? J2L.J2T.TileCount % 1030 : 1030) / 10 * 32;
                
                double yfraction;
                double xfraction;
                int tile = TilesetScrollbar.Value / 32 * 10;
                for (int yoffset = -(TilesetScrollbar.Value % 32); yoffset < TilesetScrollbar.Height && tile < J2L.J2T.TileCount; tile += 10)
                {
                    xfraction = tile % J2L.AtlasLength * J2L.AtlasFraction;
                    yfraction = tile / J2L.AtlasLength * J2L.AtlasFraction;
                    if (tile % J2L.AtlasLength + 9 < J2L.AtlasLength)
                    {
                        GL.Begin(BeginMode.Quads);
                        GL.TexCoord2(xfraction + J2L.AtlasFraction * 10, yfraction); GL.Vertex2(320, yoffset);
                        GL.TexCoord2(xfraction, yfraction); GL.Vertex2(0, yoffset);
                        yfraction += J2L.AtlasFraction; yoffset += 32;
                        GL.TexCoord2(xfraction, yfraction); GL.Vertex2(0, yoffset);
                        GL.TexCoord2(xfraction + J2L.AtlasFraction * 10, yfraction); GL.Vertex2(320, yoffset);
                        GL.End();
                    }
                    else
                    {
                        byte width = (byte)(J2L.AtlasLength - tile % J2L.AtlasLength);
                        GL.Begin(BeginMode.Quads);
                        GL.TexCoord2(xfraction + J2L.AtlasFraction * width, yfraction); GL.Vertex2(32*width, yoffset);
                        GL.TexCoord2(xfraction, yfraction); GL.Vertex2(0, yoffset);
                        yfraction += J2L.AtlasFraction; yoffset += 32;
                        GL.TexCoord2(xfraction, yfraction); GL.Vertex2(0, yoffset);
                        GL.TexCoord2(xfraction + J2L.AtlasFraction * width, yfraction); GL.Vertex2(32 * width, yoffset);
                        GL.End();
                        xfraction = 0; yoffset -= 32;
                        GL.Begin(BeginMode.Quads);
                        GL.TexCoord2(xfraction + J2L.AtlasFraction * (10-width), yfraction); GL.Vertex2(320, yoffset);
                        GL.TexCoord2(xfraction, yfraction); GL.Vertex2(32 * width, yoffset);
                        yfraction += J2L.AtlasFraction; yoffset += 32;
                        GL.TexCoord2(xfraction, yfraction); GL.Vertex2(32 * width, yoffset);
                        GL.TexCoord2(xfraction + J2L.AtlasFraction * (10 - width), yfraction); GL.Vertex2(320, yoffset);
                        GL.End();
                    }
                }
                if (TilesetOverlaySelection.SelectedIndex == 1)
                {
                    GL.Enable(EnableCap.Blend);
                    SetTextureTo(AtlasID.EventNames);
                    for (tile = TilesetScrollbar.Value / 32 * 10; tile / 10 * 32 - TilesetScrollbar.Value < LevelDisplay.Height + 31; tile++)
                    {
                        if (J2L.EventTiles[tile] != 0) DrawEvent(tile % 10 * 32, tile / 10 * 32 - TilesetScrollbar.Value, J2L.EventTiles[tile]);
                    }
                }
                else if (TilesetOverlaySelection.SelectedIndex == 2)
                {
                    GL.Enable(EnableCap.Blend);
                    SetTextureTo(AtlasID.TileTypes);
                    for (tile = TilesetScrollbar.Value / 32 * 10; tile / 10 * 32 - TilesetScrollbar.Value < LevelDisplay.Height + 31; tile++)
                    {
                        if (J2L.TileTypes[tile] != 0) DrawTileType(tile % 10 * 32, tile / 10 * 32 - TilesetScrollbar.Value, J2L.TileTypes[tile]);
                    }
                }
                if (HowSelecting == FocusedZone.Tileset)
                {
                    GL.Enable(EnableCap.Blend);
                    int[] Rect = new int[4];
                    Rect[0] = Math.Min(SelectionBoxCorners[0], SelectionBoxCorners[2]) * 32;
                    Rect[1] = Math.Min(SelectionBoxCorners[1], SelectionBoxCorners[3]) * 32 - TilesetScrollbar.Value;
                    Rect[2] = Math.Abs(SelectionBoxCorners[0] - SelectionBoxCorners[2]) * 32 + Rect[0];
                    Rect[3] = Math.Abs(SelectionBoxCorners[1] - SelectionBoxCorners[3]) * 32 + Rect[1];
                    DrawSelectionRectangle(Rect, 32);
                    RedrawTilesetHowManyTimes = 2;
                }
                if (WhereSelected == FocusedZone.Tileset && SelectedTileLocations.Count > 0)
                {
                    EmborderSelectedTiles(0, TilesetScrollbar.Value, 32, 320, LevelDisplay.Height);
                    RedrawTilesetHowManyTimes = 2;
                }
                else if (ParallaxDisplayMode) GL.Enable(EnableCap.Blend);
            }
            RedrawTilesetHowManyTimes--;
        }*/
        #endregion GL

        #region Mouse movement
        #region mouse variables
        internal int MouseTileX = 0;
        internal int MouseTileY = 0;
        internal int MouseTile = 0;
        internal int OldMouseTile = 0;
        //internal uint MouseEvent = 0;
        internal AGAEvent MouseAGAEvent = new AGAEvent((uint)0);
        internal AGAEvent? SelectReturnAGAEvent = null;
        internal FocusedZone LastFocusedZone = FocusedZone.None;

        internal FocusedZone HowSelecting = FocusedZone.None;
        internal FocusedZone WhereSelected = FocusedZone.None;
        internal SelectionType CurrentSelectionType;
        internal int[] SelectionBoxCorners = new int[4];
        #endregion mouse variables


        private void AnimationSettings_MouseEnter(object sender, EventArgs e)
        {
            LastFocusedZone = FocusedZone.AnimationEditing;
            AnimScrollbar.Focus();
            LevelDisplay.ContextMenuStrip = null;
        }
        private void LevelDisplay_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.X <= DrawingTools.Left)
            {
                if (AnimationSettings.Visible && e.Y + LevelDisplay.Top > AnimationSettings.Top)
                {
                    LastFocusedZone = FocusedZone.AnimationEditing;
                    AnimScrollbar.Focus();
                    LevelDisplay.ContextMenuStrip = null;
                    MouseTile = MouseTileX = (e.X - 48 + AnimScrollbar.Value) / 32;
                    MouseTileY = 0;
                    MouseAGAEvent.ID = 0;
                }
                else
                {
                    LastFocusedZone = FocusedZone.Tileset;
                    TilesetScrollbar.Focus();
                    LevelDisplay.ContextMenuStrip = TContextMenu;
                    MouseTileX = e.X / 32;
                    MouseTileY = (e.Y + TilesetScrollbar.Value - TilesetScrollbar.Minimum) / 32;
                    if (J2L.J2T == null || MouseTileY * 10 < J2L.J2T.TileCount)
                    {
                        MouseTile = MouseTileX + MouseTileY * 10;
                        editAnimationToolStripMenuItem.Visible = deleteAnimationToolStripMenuItem.Visible = cloneAnimationToolStripMenuItem.Visible = !(TiletypeDropdown.Visible = OverlayDropdown.Visible = true);
                    }
                    else
                    {
                        MouseTile = (int)((MouseTileX + MouseTileY * 10) - J2L.J2T.TileCount + J2L.AnimOffset);
                        editAnimationToolStripMenuItem.Visible = deleteAnimationToolStripMenuItem.Visible = cloneAnimationToolStripMenuItem.Visible = !(TiletypeDropdown.Visible = OverlayDropdown.Visible = false);
                    }
                    MouseAGAEvent.ID = (J2L.VersionType == Version.AGA || MouseTile >= J2L.MaxTiles) ? 0 : J2L.EventTiles[MouseTile];
                    if (HowSelecting == FocusedZone.Tileset) { SelectionBoxCorners[2] = MouseTileX; SelectionBoxCorners[3] = MouseTileY; }
                }
            }
            else
            {
                LastFocusedZone = FocusedZone.Level;
                LDScrollV.Focus();
                LevelDisplay.ContextMenuStrip = LDContextMenu;
                MouseTileX = Math.Max(0, (e.X - LDScrollH.Location.X + LDScrollH.Value) / ZoomTileSize);
                MouseTileY = Math.Max(0, (e.Y + LDScrollV.Value) / ZoomTileSize);
                if (SafeToDisplay)
                {
                    MouseTile = MouseTileX + MouseTileY * (int)J2L.Layers[CurrentLayer].Width;
                }
                if (CurrentLayer == 3 && MouseTileX < J2L.Layers[3].Width && MouseTileX >= 0 && MouseTileY < J2L.Layers[3].Height && MouseTileY >= 0)
                {
                    if (J2L.VersionType == Version.AGA) { MouseAGAEvent = J2L.AGA_EventMap[MouseTileX, MouseTileY]; }
                    else MouseAGAEvent.ID = J2L.EventMap[MouseTileX, MouseTileY];
                }
                else MouseAGAEvent.ID = 0;
                if (HowSelecting == FocusedZone.Level) { SelectionBoxCorners[2] = MouseTileX; SelectionBoxCorners[3] = MouseTileY; }
            }
            if (MouseTile != OldMouseTile)
            {
                if (MouseHeldDownAction) TakeAction();
                UpdateMousePrintout();
            }
        }

        private void UpdateMousePrintout()
        {
            OldMouseTile = MouseTile;
            MouseTilePrintout.Text = String.Format("({0}, {1})", MouseTileX, MouseTileY);
            MouseEventPrintout.Text = (J2L.IniEventListing[J2L.VersionType][MouseAGAEvent.ID & 255][0] ?? "") + ((J2L.IniEventListing[J2L.VersionType][MouseAGAEvent.ID & 255].Length > 5 && J2L.VersionType != Version.AGA) ? " (" + String.Join(", ", PresentParameterValues(MouseAGAEvent.ID, J2L.IniEventListing[J2L.VersionType][MouseAGAEvent.ID & 255])) + ")" : "");
        }
        string[] PresentParameterValues(uint rawEvent, string[] iniEntry)
        {
            string[] output = new string[iniEntry.Length - 5];
            int[] values = ExtractParameterValues(rawEvent, iniEntry);
            string paramName;
            for (byte i = 0; i < output.Length; i++)
            {
                paramName = iniEntry[i + 5].Split(':')[0];
                if (i == 0 && paramName.Length > 4 && paramName.Substring(paramName.Length - 5, 5) == "Event") output[i] = paramName + "=" + '"' + J2L.IniEventListing[J2L.VersionType][rawEvent << 12 >> 24][0] + '"';
                else output[i] = paramName + "=" + values[i].ToString();
            }
            return output;
        }
        public static int[] ExtractParameterValues(uint rawEvent, string[] iniEntry)
        {
            int[] output = new int[6] { 0, 0, 0, 0, 0, 0 };
            int bitpush = 12;
            sbyte range;
            string mode;
            for (byte i = 0; i < 6 && iniEntry.Length - 5 > i; i++)
            {
                mode = iniEntry[i + 5].Split(':')[1];
                range = (sbyte)EventForm.GetNumberAtEndOfString(mode, true);
                /*switch (mode.Substring(0,1))
                {
                    case "-":
                    case "0":
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                    case "8":
                    case "9":
                        range = Convert.ToSByte(mode);
                        break;
                    default:
                        range = Convert.ToSByte(mode.Substring(mode.Length - 1, 1));
                        break;
                }*/
                if (range >= 0)
                {
                    output[i] = (int)J2LFile.GetRawBits(rawEvent, bitpush, range);
                    bitpush += range;
                }
                else
                {
                    if ((rawEvent & (1 << (bitpush - range - 1))) > 0) output[i] = (-(1 << (-range - 1)) + (int)J2LFile.GetRawBits(rawEvent, bitpush, -range - 1));
                    else output[i] = (int)J2LFile.GetRawBits(rawEvent, bitpush, -range - 1);
                    bitpush -= range;
                }
            }
            return output;
        }
        #endregion Mouse Movement

        #region editing functions
        //List<Point> SelectedTileLocations = new List<Point>(512);
        internal bool[][] IsEachTileSelected = new bool[1026][], ShouldEachTileBeFilledIn = new bool[1026][];
        Point UpperLeftSelectionCorner = new Point(1024, 1024), BottomRightSelectionCorner = new Point(0, 0);
        internal struct TileAndEvent
        {
            internal ushort Tile;
            internal AGAEvent? Event;
            public TileAndEvent(ushort t, AGAEvent? e) { Event = e; Tile = t; }
            public TileAndEvent(ushort t, uint? u) { Tile = t; Event = new AGAEvent(u ?? 0); }
            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType()) return false;
                else return (Event.Equals(((TileAndEvent)obj).Event) && Tile == ((TileAndEvent)obj).Tile);
            }
        }
        internal struct LayerAndSpecificTiles
        {
            internal byte Layer;
            internal Dictionary<Point, TileAndEvent> Specifics;
            public LayerAndSpecificTiles(byte b) { Layer = b; Specifics = new Dictionary<Point, TileAndEvent>(); }
        }
        Stack<LayerAndSpecificTiles> Undoable = new Stack<LayerAndSpecificTiles>(), Redoable = new Stack<LayerAndSpecificTiles>();
        TileAndEvent[][] CurrentStamp = new TileAndEvent[0][];
        private void SetStampDimensions(int x, int y)
        {
            CurrentStamp = new TileAndEvent[x][];
            for (int i = 0; i < x; i++) CurrentStamp[i] = new TileAndEvent[y];
        }
        byte SelectedAnimationFrame;

        #region Event Editing
        private void GrabEventAtMouse() { if (J2L.VersionType == Version.AGA) ActiveEvent = MouseAGAEvent; else ActiveEvent.ID = MouseAGAEvent.ID; }
        private void PasteEventAtMouse()
        {
            /*if (J2L.VersionType == Version.AGA) { if (LastFocusedZone == FocusedZone.Level) J2L.AGA_EventMap[MouseTileX, MouseTileY] = MouseAGAEvent = ActiveEvent; }
            else*/
            if (LastFocusedZone == FocusedZone.Tileset) { J2L.EventTiles[MouseTile] = MouseAGAEvent.ID = ActiveEvent.ID; RedrawTilesetHowManyTimes = 2; }
            else if (LastFocusedZone == FocusedZone.Level)
            {
                LayerAndSpecificTiles actionCenter = new LayerAndSpecificTiles(3);
                ActOnATile(MouseTileX, MouseTileY, J2L.Layers[3].TileMap[MouseTileX, MouseTileY], ActiveEvent, actionCenter, true);
                //actionCenter.Specifics.Add(new Point(MouseTileX, MouseTileY), new TileAndEvent(J2L.Layers[3].TileMap[MouseTileX, MouseTileY], (actionCenter.Layer == 3) ? J2L.EventMap[MouseTileX, MouseTileY] : (uint?)null));
                Undoable.Push(actionCenter);
                Redoable.Clear();
                LevelHasBeenModified = true;
                //J2L.EventMap[MouseTileX, MouseTileY] = MouseAGAEvent.ID = ActiveEvent.ID;
            }
            UpdateMousePrintout();
        }
        private void SelectEventAtMouse()
        {
            _suspendEvent.Reset();
            EventForm EF = new EventForm(this, TreeStructure[J2L.VersionType][(J2L.LevelMode == 1) ? 1 : 0].ToArray(), J2L.VersionType, (J2L.VersionType == Version.AGA && MouseAGAEvent.Bits == null) ? new AGAEvent(0) : MouseAGAEvent);
            EF.ShowDialog();
            if (SelectReturnAGAEvent != null) { ActiveEvent = (AGAEvent)SelectReturnAGAEvent; PasteEventAtMouse(); }
            EF.ResetTree();
            _suspendEvent.Set();
        }
        #endregion Event Editing

        #region Editing Animations
        const ushort HeightOfAnimationEditingSection = 137;
        AnimatedTile WorkingAnimation = new AnimatedTile();
        ushort CurrentAnimationID;

        private void OpenAnimClick()
        {
            if (LastFocusedZone == FocusedZone.Tileset && MouseTile >= J2L.AnimOffset)
            {
                CurrentAnimationID = (ushort)(MouseTile);
                EditAnimation(WorkingAnimation = (CurrentAnimationID < J2L.MaxTiles) ? new AnimatedTile(J2L.Animations[CurrentAnimationID - J2L.AnimOffset]) : new AnimatedTile());
            }
        }

        private void LevelDisplay_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (LastFocusedZone)
            {
                case FocusedZone.AnimationEditing:
                    {
                        if (e.Button == MouseButtons.Left && MouseTileX >= 0)
                        {
                            if (Control.ModifierKeys == Keys.Control)
                            {
                                if (WorkingAnimation.InsertFrame(SelectedAnimationFrame, WorkingAnimation.Sequence[MouseTileX]) == InsertFrameResults.Success) { SelectedAnimationFrame++; WorkingAnimation.JustBeenEdited(GameTick); AnimScrollbar.Maximum = Math.Max(0, WorkingAnimation.FrameCount * 32 + 80 - AnimScrollbar.Width + AnimScrollbar.LargeChange); }
                            }
                            else if (MouseTileX <= WorkingAnimation.FrameCount) SelectedAnimationFrame = (byte)(MouseTileX);
                        }
                        break;
                    }
                case FocusedZone.Tileset:
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            if (Control.ModifierKeys == Keys.Control)
                            {
                                if (AnimationSettings.Visible)
                                {
                                    if (WorkingAnimation.InsertFrame(SelectedAnimationFrame, (ushort)MouseTile) == InsertFrameResults.Success)
                                    {
                                        SelectedAnimationFrame++;
                                        WorkingAnimation.JustBeenEdited(GameTick);
                                        AnimScrollbar.Maximum = Math.Max(0, WorkingAnimation.FrameCount * 32 + 80 - AnimScrollbar.Width + AnimScrollbar.LargeChange);
                                    }
                                }
                            }
                            else if (AnimationSettings.Visible) return;
                            else if (HowSelecting == FocusedZone.Tileset && !MouseHeldDownSelection) EndSelection();
                        }
                        break;
                    }
                case FocusedZone.Level:
                    {
                        if (AnimationSettings.Visible) return;
                        //else if (HowSelecting == FocusedZone.Level && !MouseHeldDownSelection) EndSelection();
                        break;
                    }
                default: break;
            }
        }
        private void LevelDisplay_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e) { OpenAnimClick(); }

        private void editAnimationToolStripMenuItem_Click(object sender, EventArgs e) { OpenAnimClick(); }
        private void DeleteAnimation(int id)
        {
            _suspendEvent.Reset();
            DialogResult result = MessageBox.Show("You are about to delete this animated tile. Select \"Yes\" to remove all instances of it from the level (recommended), or \"No\" to shift any later animated tiles downwards to take its place.", "Delete Animation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Stop);
            if (result != DialogResult.Cancel) { J2L.DeleteAnimation((byte)(id - J2L.AnimOffset), result == DialogResult.Yes); UneditAnimation(); LevelHasBeenModified = true; }
            _suspendEvent.Set();
        }
        private void deleteAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteAnimation(MouseTile);
        }
        private void cloneAnimationToolStripMenuItem_Click(object sender, EventArgs e) { J2L.InsertAnimation(J2L.Animations[MouseTile - J2L.AnimOffset]); LevelHasBeenModified = true; }

        private void EditAnimation(AnimatedTile anim)
        {
            if (!AnimationSettings.Visible)
            {
                if (CurrentAnimationID >= J2L.MaxTiles) anim.Speed = 10;
                AnimSpeed.Value = anim.Speed;
                AnimPP.Checked = anim.IsPingPong;
                AnimPPDelay.Value = anim.PingPongWait;
                AnimDelay.Value = anim.Framewait;
                AnimRandDelay.Value = anim.Random;
                TilesetScrollbar.Height -= HeightOfAnimationEditingSection;
                TilesetScrollbar.Maximum += HeightOfAnimationEditingSection;
                TilesetScrollbar.Value += HeightOfAnimationEditingSection;
                AnimationSettings.Visible = AnimScrollbar.Visible = true;
                AnimScrollbar.Value = 0;
                AnimScrollbar.Maximum = Math.Max(0, anim.FrameCount * 32 + 80 - AnimScrollbar.Width + AnimScrollbar.LargeChange);
                AnimScrollbar.Update();
                DetermineVisibilityOfAnimatedTiles();
                SelectedAnimationFrame = 0;
                RedrawTilesetHowManyTimes = 2;
            }
        }
        private void UneditAnimation()
        {
            if (AnimationSettings.Visible)
            {
                TilesetScrollbar.Height += HeightOfAnimationEditingSection;
                TilesetScrollbar.Maximum -= HeightOfAnimationEditingSection;
                AnimationSettings.Visible = AnimScrollbar.Visible = false;
                MakeProposedScrollbarValueWork(TilesetScrollbar, TilesetScrollbar.Value);
            }
            DetermineVisibilityOfAnimatedTiles();
            RedrawTilesetHowManyTimes = 2;
        }

        private void AnimOK_Click(object sender, EventArgs e)
        {
            if (WorkingAnimation.FrameCount > 0)
            {
                if (CurrentAnimationID < J2L.MaxTiles) J2L.Animations[CurrentAnimationID - J2L.AnimOffset] = WorkingAnimation;
                else J2L.InsertAnimation(WorkingAnimation);
                LevelHasBeenModified = true;
                UneditAnimation();
            }
            else DeleteAnimation(CurrentAnimationID);
        }
        private void AnimCancel_Click(object sender, EventArgs e) { UneditAnimation(); }

        private void AnimSpeed_ValueChanged(object sender, EventArgs e) { WorkingAnimation.Speed = (byte)AnimSpeed.Value; WorkingAnimation.JustBeenEdited(GameTick); }
        private void AnimPPDelay_ValueChanged(object sender, EventArgs e) { WorkingAnimation.PingPongWait = (ushort)AnimPPDelay.Value; WorkingAnimation.JustBeenEdited(GameTick); }
        private void AnimRandDelay_ValueChanged(object sender, EventArgs e) { WorkingAnimation.Random = (ushort)AnimRandDelay.Value; WorkingAnimation.JustBeenEdited(GameTick); }
        private void AnimDelay_ValueChanged(object sender, EventArgs e) { WorkingAnimation.Framewait = (ushort)AnimDelay.Value; WorkingAnimation.JustBeenEdited(GameTick); }
        private void AnimPP_CheckedChanged(object sender, EventArgs e) { WorkingAnimation.IsPingPong = AnimPP.Checked; WorkingAnimation.JustBeenEdited(GameTick); }
        #endregion Editing Animations

        internal void Clear(byte LayerNumber)
        {
            if (J2L.Layers[LayerNumber].HasTiles)
            {
                LayerAndSpecificTiles ActionCenter = new LayerAndSpecificTiles(LayerNumber);
                if (WhereSelected == FocusedZone.Level)
                {
                    for (ushort x = 0; x < J2L.Layers[LayerNumber].TileMap.GetLength(0); x++) for (ushort y = 0; y < J2L.Layers[LayerNumber].TileMap.GetLength(1); y++) if (IsEachTileSelected[x + 1][y + 1])
                            {
                                ActOnATile(x, y, 0, 0, ActionCenter, true);
                            }
                }
                else
                {
                    _suspendEvent.Reset();
                    DialogResult result = MessageBox.Show(String.Format("Are you sure you want to delete the contents of layer {0}?", LayerNumber + 1), "Clear Layer", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.OK)
                    {
                        for (ushort x = 0; x < J2L.Layers[LayerNumber].TileMap.GetLength(0); x++) for (ushort y = 0; y < J2L.Layers[LayerNumber].TileMap.GetLength(1); y++) ActOnATile(x, y, 0, 0, ActionCenter, true);
                    }
                    _suspendEvent.Set();
                }
                if (ActionCenter.Specifics.Count > 0)
                {
                    Undoable.Push(ActionCenter);
                    Redoable.Clear();
                    LevelHasBeenModified = true;
                }
            }
        }

        private void BeginSelection(SelectionType type)
        {
            CurrentSelectionType = type;
            SelectionBoxCorners[0] = SelectionBoxCorners[2] = MouseTileX;
            SelectionBoxCorners[1] = SelectionBoxCorners[3] = MouseTileY;
            HowSelecting = LastFocusedZone;
            if (DeepEditingTool != SelectionButton)
            {
                DeepEditingTool.Checked = false;
                (VisibleEditingTool = SelectionButton).Checked = true;
            }
            if (HowSelecting == FocusedZone.Tileset) RedrawTilesetHowManyTimes += 2;
        }
        private void EndSelection()
        {
            if (WhereSelected == FocusedZone.Tileset)
            {
                RedrawTilesetHowManyTimes += 2;
            }
            WhereSelected = HowSelecting;
            HowSelecting = FocusedZone.None;
            MouseHeldDownSelection = false;
            if (DeepEditingTool != SelectionButton)
            {
                VisibleEditingTool.Checked = false;
                (VisibleEditingTool = DeepEditingTool).Checked = true;
            }
            int boxWidth = Math.Abs(SelectionBoxCorners[0] - SelectionBoxCorners[2]) + 1;
            int boxHeight = Math.Abs(SelectionBoxCorners[1] - SelectionBoxCorners[3]) + 1;
            if (CurrentSelectionType == SelectionType.New)
            {
                UpperLeftSelectionCorner.X = UpperLeftSelectionCorner.Y = 1024;
                BottomRightSelectionCorner.X = BottomRightSelectionCorner.Y = 0;
                foreach (bool[] col in IsEachTileSelected) for (ushort y = 0; y < col.Length; y++) col[y] = false;
            }
            //Point[] newlyBoxed = new Point[boxWidth * boxHeight];
            int nuSelectX = Math.Min(SelectionBoxCorners[0], SelectionBoxCorners[2]), nuSelectY = Math.Min(SelectionBoxCorners[1], SelectionBoxCorners[3]);
            switch (CurrentSelectionType)
            {
                case SelectionType.New:
                case SelectionType.Add:
                    {
                        ShowBlankTileInStamp = WhereSelected == FocusedZone.Tileset;
                        if (nuSelectX < UpperLeftSelectionCorner.X) UpperLeftSelectionCorner.X = nuSelectX;
                        if (nuSelectY < UpperLeftSelectionCorner.Y) UpperLeftSelectionCorner.Y = nuSelectY;
                        if (nuSelectX + boxWidth > BottomRightSelectionCorner.X) BottomRightSelectionCorner.X = nuSelectX + boxWidth;
                        if (nuSelectY + boxHeight > BottomRightSelectionCorner.Y) BottomRightSelectionCorner.Y = nuSelectY + boxHeight;
                        for (int x = nuSelectX, i = 0; i < boxWidth; x++, i++) for (int y = nuSelectY, j = 0; j < boxHeight; y++, j++) IsEachTileSelected[x + 1][y + 1] = true;
                        break;
                    }
                case SelectionType.Subtract:
                    {
                        for (int x = nuSelectX, i = 0; i < boxWidth; x++, i++) for (int y = nuSelectY, j = 0; j < boxHeight; y++, j++) IsEachTileSelected[x + 1][y + 1] = false;
                        if ((nuSelectX <= UpperLeftSelectionCorner.X && nuSelectX + boxWidth >= UpperLeftSelectionCorner.X) || (nuSelectY <= UpperLeftSelectionCorner.Y && nuSelectY + boxHeight >= UpperLeftSelectionCorner.Y)) UpperLeftSelectionCorner = new Point(RecalculateSelectionCornerCoordinates(false, false) - 1, RecalculateSelectionCornerCoordinates(false, true) - 1);
                        if ((nuSelectX <= BottomRightSelectionCorner.X && nuSelectX + boxWidth >= BottomRightSelectionCorner.X) || (nuSelectY <= BottomRightSelectionCorner.Y && nuSelectY + boxHeight >= BottomRightSelectionCorner.Y)) BottomRightSelectionCorner = new Point(RecalculateSelectionCornerCoordinates(true, false), RecalculateSelectionCornerCoordinates(true, true));
                        break;
                    }
                case SelectionType.Rectangle:
                case SelectionType.HollowRectangle:
                    TakeAction();
                    DeselectAll();
                    break;
            }
            //Text = String.Format("{0}, {1} - {2}, {3}", UpperLeftSelectionCorner.X, UpperLeftSelectionCorner.Y, BottomRightSelectionCorner.X, BottomRightSelectionCorner.Y);
        }
        private int RecalculateSelectionCornerCoordinates(bool bottomRight, bool columnsTakePriority)
        {
            for (int loop1 = 0, pos1 = (bottomRight) ? IsEachTileSelected.Length - 1 : 0; loop1 < IsEachTileSelected.Length; loop1++, pos1 += (bottomRight) ? -1 : 1)
                for (int loop2 = 0, pos2 = (bottomRight) ? IsEachTileSelected.Length - 1 : 0; loop2 < IsEachTileSelected.Length; loop2++, pos2 += (bottomRight) ? -1 : 1)
                {
                    if (IsEachTileSelected[(columnsTakePriority) ? pos2 : pos1][(columnsTakePriority) ? pos1 : pos2]) return pos1;
                }
            return (bottomRight) ? 0 : 1026;
        }

        private void DeselectAll()
        {
            if (WhereSelected == FocusedZone.Tileset) RedrawTilesetHowManyTimes = 2;
            HowSelecting = WhereSelected = FocusedZone.None;
            UpperLeftSelectionCorner.X = UpperLeftSelectionCorner.Y = 1024;
            BottomRightSelectionCorner.X = BottomRightSelectionCorner.Y = 0;
            foreach (bool[] col in IsEachTileSelected) for (ushort y = 0; y < col.Length; y++) col[y] = false;
        }
        private void MakeSelectionIntoStamp(bool cut = false)
        {
            if (WhereSelected != FocusedZone.None)
            {
                SetStampDimensions(BottomRightSelectionCorner.X - UpperLeftSelectionCorner.X, BottomRightSelectionCorner.Y - UpperLeftSelectionCorner.Y);
                for (int x = UpperLeftSelectionCorner.X; x < BottomRightSelectionCorner.X; x++)
                    for (int y = UpperLeftSelectionCorner.Y; y < BottomRightSelectionCorner.Y; y++)
                        if (IsEachTileSelected[x + 1][y + 1])
                        {
                            CurrentStamp[x - UpperLeftSelectionCorner.X][y - UpperLeftSelectionCorner.Y] = (WhereSelected == FocusedZone.Level) ? new TileAndEvent(J2L.Layers[CurrentLayer].TileMap[x, y], J2L.EventMap[x, y]) : new TileAndEvent((ushort)(x + y * 10 + ((y * 10 >= J2L.J2T.TileCount) ? J2L.AnimOffset - J2L.J2T.TileCount : 0)), J2L.EventTiles[x + y * 10]);
                            if (cut && WhereSelected == FocusedZone.Level) { J2L.Layers[CurrentLayer].TileMap[x, y] = 0; J2L.EventMap[x, y] = 0; }
                        }
            }
        }

        bool MouseHeldDownSelection = false, MouseHeldDownAction = false;
        ToolStripButton DeepEditingTool, VisibleEditingTool;

        private void LevelDisplay_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (AnimationSettings.Visible || e.Button == MouseButtons.Right || (LastFocusedZone == FocusedZone.Tileset && (J2L.J2T == null || MouseTile >= J2L.MaxTiles))) return;
            if ((DeepEditingTool == SelectionButton || LastFocusedZone == FocusedZone.Tileset) && HowSelecting == FocusedZone.None)
            {
                MouseHeldDownSelection = true;
                if (Control.ModifierKeys == Keys.Shift) BeginSelection(SelectionType.Add);
                else if (Control.ModifierKeys == Keys.Control) BeginSelection(SelectionType.Subtract);
                else BeginSelection(SelectionType.New);
            }
            else if (LastFocusedZone == FocusedZone.Level)
            {
                if (DeepEditingTool == RectangleButton) { MouseHeldDownSelection = true; BeginSelection(SelectionType.Rectangle); }
                else if (DeepEditingTool == RectangleOutlineButton) { MouseHeldDownSelection = true; BeginSelection(SelectionType.HollowRectangle); }
                else { MouseHeldDownAction = true; TakeAction(); }
            }
        }

        private void LevelDisplay_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;
            MouseHeldDownAction = false;
            if (MouseHeldDownSelection)
            {
                EndSelection();
                if (WhereSelected == FocusedZone.Tileset) MakeSelectionIntoStamp();

            }
        }

        //Rectangle DrawRect = new Rectangle();
        Point DrawPoint = new Point();
        Queue<Point> FillingQ = new Queue<Point>();
        private void ActOnATile(int x, int y, ushort? tile, uint ev, LayerAndSpecificTiles actionCenter, bool blankTilesOkay) { ActOnATile(x, y, tile, new AGAEvent(ev), actionCenter, blankTilesOkay); }
        private void ActOnATile(int x, int y, ushort? tile, AGAEvent? ev, LayerAndSpecificTiles actionCenter, bool blankTilesOkay)
        {
            {
                if (x >= 0 && y >= 0 && x < J2L.Layers[actionCenter.Layer].TileMap.GetLength(0) && y < J2L.Layers[actionCenter.Layer].TileMap.GetLength(1) && tile != null && (blankTilesOkay || tile > 0))
                {
                    if (J2L.VersionType == Version.AGA) actionCenter.Specifics.Add(new Point(x, y), new TileAndEvent(J2L.Layers[actionCenter.Layer].TileMap[x, y], (actionCenter.Layer == 3) ? J2L.AGA_EventMap[x, y] : (AGAEvent?)null));
                    else actionCenter.Specifics.Add(new Point(x, y), new TileAndEvent(J2L.Layers[actionCenter.Layer].TileMap[x, y], (actionCenter.Layer == 3) ? J2L.EventMap[x, y] : (uint?)null));
                    J2L.Layers[actionCenter.Layer].TileMap[x, y] = (ushort)tile;
                    if (actionCenter.Layer == 3)
                    {
                        if (J2L.VersionType == Version.AGA) J2L.AGA_EventMap[x, y] = ev ?? new AGAEvent(0);
                        else J2L.EventMap[x, y] = (ev == null) ? 0 : ((AGAEvent)ev).ID;
                    }
                }
            }
        }
        private Point MakeUpSomeValidStampCoordinates(bool blankTilesAreAcceptable, int MinX, int MinY, int MaxX, int MaxY, int iterations = 0)
        {
            Point p = new Point(_r.Next(MinX, MaxX), _r.Next(MinY, MaxY));
            if (CurrentStamp[p.X][p.Y].Tile == null || (CurrentStamp[p.X][p.Y].Tile == 0 && !blankTilesAreAcceptable))
            {
                return (iterations >= 15) ? new Point(MinX, MinY) : MakeUpSomeValidStampCoordinates(blankTilesAreAcceptable, MinX, MinY, MaxX, MaxY, ++iterations);
            }
            else return p;
        }
        private void TakeAction()
        {
            if (WhereSelected == FocusedZone.Tileset) DeselectAll();
            LayerAndSpecificTiles ActionCenter = new LayerAndSpecificTiles(CurrentLayer);
            bool shiftPressed = Control.ModifierKeys == Keys.Shift || Control.ModifierKeys == (Keys.Control | Keys.Shift);
            #region paintbrush
            if (VisibleEditingTool == PaintbrushButton)
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    DrawPoint = MakeUpSomeValidStampCoordinates(shiftPressed, 0, 0, CurrentStamp.Length, CurrentStamp[0].Length);
                    ActOnATile(MouseTileX, MouseTileY, CurrentStamp[DrawPoint.X][DrawPoint.Y].Tile, CurrentStamp[DrawPoint.X][DrawPoint.Y].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
                }
                else
                {
                    for (ushort x = 0; x < CurrentStamp.Length; x++)
                        for (ushort y = 0; y < CurrentStamp[0].Length; y++)
                            if (IsEachTileSelected[MouseTileX + x + 1][MouseTileY + y + 1] == IsEachTileSelected[MouseTileX + 1][MouseTileY + 1]) ActOnATile(MouseTileX + x, MouseTileY + y, CurrentStamp[x][y].Tile, CurrentStamp[x][y].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
                }
            }
            #endregion paintbrush
            #region fill
            else if (VisibleEditingTool == FillButton)
            {
                if (MouseTileX < J2L.Layers[CurrentLayer].Width && MouseTileY < J2L.Layers[CurrentLayer].Height)
                {
                    foreach (bool[] col in ShouldEachTileBeFilledIn) for (ushort y = 0; y < col.Length; y++) col[y] = false;
                    ArrayMap<ushort> TileMap = J2L.Layers[CurrentLayer].TileMap;
                    ushort TargetTileID = TileMap[MouseTileX, MouseTileY];
                    bool SelectedOnly = IsEachTileSelected[MouseTileX + 1][MouseTileY + 1];
                    TryToFillTile(MouseTileX, MouseTileY, TileMap, ref TargetTileID, ref SelectedOnly, ref shiftPressed, ref ActionCenter);
                    while (FillingQ.Count > 0)
                    {
                        Point FillPoint = FillingQ.Dequeue();
                        if (FillPoint.X > 0) TryToFillTile(FillPoint.X - 1, FillPoint.Y, TileMap, ref TargetTileID, ref SelectedOnly, ref shiftPressed, ref ActionCenter);
                        if (FillPoint.X < J2L.Layers[CurrentLayer].Width - 1) TryToFillTile(FillPoint.X + 1, FillPoint.Y, TileMap, ref TargetTileID, ref SelectedOnly, ref shiftPressed, ref ActionCenter);
                        if (FillPoint.Y > 0) TryToFillTile(FillPoint.X, FillPoint.Y - 1, TileMap, ref TargetTileID, ref SelectedOnly, ref shiftPressed, ref ActionCenter);
                        if (FillPoint.Y < J2L.Layers[CurrentLayer].Height - 1) TryToFillTile(FillPoint.X, FillPoint.Y + 1, TileMap, ref TargetTileID, ref SelectedOnly, ref shiftPressed, ref ActionCenter);
                    }
                    for (ushort x = 0; x < ShouldEachTileBeFilledIn.Length; x++)
                        for (ushort y = 0; y < ShouldEachTileBeFilledIn.Length; y++)
                            if (ShouldEachTileBeFilledIn[x][y])
                            {
                                /*if (Control.ModifierKeys == Keys.Control) DrawPoint = MakeUpSomeValidStampCoordinates(shiftPressed, 0, 0, CurrentStamp.Length, CurrentStamp[0].Length);
                                else
                                {
                                    DrawPoint.X = x - MouseTileX;
                                    while (DrawPoint.X < 0) DrawPoint.X += CurrentStamp.Length;
                                    DrawPoint.X %= CurrentStamp.Length;
                                    DrawPoint.Y = y - MouseTileY;
                                    while (DrawPoint.Y < 0) DrawPoint.Y += CurrentStamp[0].Length;
                                    DrawPoint.Y %= CurrentStamp[0].Length;
                                }
                                ActOnATile(x, y, CurrentStamp[DrawPoint.X][DrawPoint.Y].Tile, CurrentStamp[DrawPoint.X][DrawPoint.Y].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);*/
                            }
                }
            }
            #endregion fill
            #region rectangles
            else if (VisibleEditingTool == RectangleButton || VisibleEditingTool == RectangleOutlineButton)
            {
                if (SelectionBoxCorners[0] > SelectionBoxCorners[2])
                {
                    SelectionBoxCorners[0] ^= SelectionBoxCorners[2];
                    SelectionBoxCorners[2] ^= SelectionBoxCorners[0];
                    SelectionBoxCorners[0] ^= SelectionBoxCorners[2];
                }
                if (SelectionBoxCorners[1] > SelectionBoxCorners[3])
                {
                    SelectionBoxCorners[1] ^= SelectionBoxCorners[3];
                    SelectionBoxCorners[3] ^= SelectionBoxCorners[1];
                    SelectionBoxCorners[1] ^= SelectionBoxCorners[3];
                }
                ActOnATile(SelectionBoxCorners[0], SelectionBoxCorners[1], CurrentStamp[0][0].Tile, CurrentStamp[0][0].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
                if (SelectionBoxCorners[2] != SelectionBoxCorners[0]) ActOnATile(SelectionBoxCorners[2], SelectionBoxCorners[1], CurrentStamp[CurrentStamp.Length - 1][0].Tile, CurrentStamp[CurrentStamp.Length - 1][0].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
                if (SelectionBoxCorners[3] != SelectionBoxCorners[1])
                {
                    ActOnATile(SelectionBoxCorners[0], SelectionBoxCorners[3], CurrentStamp[0][CurrentStamp[0].Length - 1].Tile, CurrentStamp[0][CurrentStamp[0].Length - 1].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
                    if (SelectionBoxCorners[2] != SelectionBoxCorners[0]) ActOnATile(SelectionBoxCorners[2], SelectionBoxCorners[3], CurrentStamp[CurrentStamp.Length - 1][CurrentStamp[0].Length - 1].Tile, CurrentStamp[CurrentStamp.Length - 1][CurrentStamp[0].Length - 1].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
                }
                for (int x = SelectionBoxCorners[0] + 1; x < SelectionBoxCorners[2]; x++)
                {
                    DrawPoint = (CurrentStamp.Length > 2) ? MakeUpSomeValidStampCoordinates(shiftPressed | ShowBlankTileInStamp, 1, 0, CurrentStamp.Length - 1, 0) : MakeUpSomeValidStampCoordinates(shiftPressed | ShowBlankTileInStamp, 0, 0, CurrentStamp.Length, 0);
                    ActOnATile(x, SelectionBoxCorners[1], CurrentStamp[DrawPoint.X][DrawPoint.Y].Tile, CurrentStamp[DrawPoint.X][DrawPoint.Y].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
                    if (SelectionBoxCorners[3] != SelectionBoxCorners[1])
                    {
                        DrawPoint = (CurrentStamp.Length > 2) ? MakeUpSomeValidStampCoordinates(shiftPressed | ShowBlankTileInStamp, 1, CurrentStamp[0].Length - 1, CurrentStamp.Length - 1, CurrentStamp[0].Length - 1) : MakeUpSomeValidStampCoordinates(shiftPressed | ShowBlankTileInStamp, 0, CurrentStamp[0].Length - 1, CurrentStamp.Length, CurrentStamp[0].Length - 1);
                        ActOnATile(x, SelectionBoxCorners[3], CurrentStamp[DrawPoint.X][DrawPoint.Y].Tile, CurrentStamp[DrawPoint.X][DrawPoint.Y].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
                    }
                }
                for (int y = SelectionBoxCorners[1] + 1; y < SelectionBoxCorners[3]; y++)
                {
                    DrawPoint = (CurrentStamp[0].Length > 2) ? MakeUpSomeValidStampCoordinates(shiftPressed | ShowBlankTileInStamp, 0, 1, 0, CurrentStamp[0].Length - 1) : MakeUpSomeValidStampCoordinates(shiftPressed | ShowBlankTileInStamp, 0, 0, 0, CurrentStamp[0].Length);
                    ActOnATile(SelectionBoxCorners[0], y, CurrentStamp[DrawPoint.X][DrawPoint.Y].Tile, CurrentStamp[DrawPoint.X][DrawPoint.Y].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
                    if (SelectionBoxCorners[2] != SelectionBoxCorners[0])
                    {
                        DrawPoint = (CurrentStamp[0].Length > 2) ? MakeUpSomeValidStampCoordinates(shiftPressed | ShowBlankTileInStamp, CurrentStamp.Length - 1, 1, CurrentStamp.Length - 1, CurrentStamp[0].Length - 1) : MakeUpSomeValidStampCoordinates(shiftPressed | ShowBlankTileInStamp, CurrentStamp.Length - 1, 0, CurrentStamp.Length - 1, CurrentStamp[0].Length);
                        ActOnATile(SelectionBoxCorners[2], y, CurrentStamp[DrawPoint.X][DrawPoint.Y].Tile, CurrentStamp[DrawPoint.X][DrawPoint.Y].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
                    }
                }
                if (VisibleEditingTool == RectangleButton) for (int x = SelectionBoxCorners[0] + 1; x < SelectionBoxCorners[2]; x++) for (int y = SelectionBoxCorners[1] + 1; y < SelectionBoxCorners[3]; y++)
                        {
                            if (CurrentStamp.Length > 2 && CurrentStamp[0].Length > 2) DrawPoint = MakeUpSomeValidStampCoordinates(shiftPressed | ShowBlankTileInStamp, 1, 1, CurrentStamp.Length - 1, CurrentStamp[0].Length - 1);
                            else DrawPoint = MakeUpSomeValidStampCoordinates(shiftPressed | ShowBlankTileInStamp, 0, 0, CurrentStamp.Length, CurrentStamp[0].Length);
                            ActOnATile(x, y, CurrentStamp[DrawPoint.X][DrawPoint.Y].Tile, CurrentStamp[DrawPoint.X][DrawPoint.Y].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
                        }
            }
            #endregion rectangles

            if (ActionCenter.Specifics.Count > 0)
            {
                Undoable.Push(ActionCenter);
                Redoable.Clear();
                LevelHasBeenModified = true;
            }
        }
        private void TryToFillTile(int x, int y, ArrayMap<ushort> tileMap, ref ushort TargetTileID, ref bool select, ref bool shiftPressed, ref LayerAndSpecificTiles ActionCenter)
        {
            if (ShouldEachTileBeFilledIn[x][y] == false && IsEachTileSelected[x + 1][y + 1] == select && tileMap[x, y] == TargetTileID)
            {
                ShouldEachTileBeFilledIn[x][y] = true;
                FillingQ.Enqueue(new Point(x, y));
                if (Control.ModifierKeys == Keys.Control) DrawPoint = MakeUpSomeValidStampCoordinates(shiftPressed, 0, 0, CurrentStamp.Length, CurrentStamp[0].Length);
                else
                {
                    DrawPoint.X = x - MouseTileX;
                    while (DrawPoint.X < 0) DrawPoint.X += CurrentStamp.Length;
                    DrawPoint.X %= CurrentStamp.Length;
                    DrawPoint.Y = y - MouseTileY;
                    while (DrawPoint.Y < 0) DrawPoint.Y += CurrentStamp[0].Length;
                    DrawPoint.Y %= CurrentStamp[0].Length;
                }
                ActOnATile(x, y, CurrentStamp[DrawPoint.X][DrawPoint.Y].Tile, CurrentStamp[DrawPoint.X][DrawPoint.Y].Event, ActionCenter, shiftPressed | ShowBlankTileInStamp);
            }
        }


        internal void Undo() { SwapActionBuffers(Undoable, Redoable); }
        internal void Redo() { SwapActionBuffers(Redoable, Undoable); }
        internal void SwapActionBuffers(Stack<LayerAndSpecificTiles> grabFrom, Stack<LayerAndSpecificTiles> putInto)
        {
            if (grabFrom.Count > 0)
            {
                LayerAndSpecificTiles ReplacedActions = grabFrom.Pop(), NewActions = new LayerAndSpecificTiles(ReplacedActions.Layer); ;
                foreach (Point p in ReplacedActions.Specifics.Keys)
                {
                    if (J2L.VersionType == Version.AGA) NewActions.Specifics.Add(p, new TileAndEvent(J2L.Layers[ReplacedActions.Layer].TileMap[p.X, p.Y], J2L.AGA_EventMap[p.X, p.Y]));
                    else NewActions.Specifics.Add(p, new TileAndEvent(J2L.Layers[ReplacedActions.Layer].TileMap[p.X, p.Y], J2L.EventMap[p.X, p.Y]));
                    J2L.Layers[ReplacedActions.Layer].TileMap[p.X, p.Y] = ReplacedActions.Specifics[p].Tile;
                    if (ReplacedActions.Layer == 3)
                    {
                        if (J2L.VersionType == Version.AGA) J2L.AGA_EventMap[p.X, p.Y] = ((AGAEvent)ReplacedActions.Specifics[p].Event);
                        else J2L.EventMap[p.X, p.Y] = ((AGAEvent)ReplacedActions.Specifics[p].Event).ID;
                    }
                }
                putInto.Push(NewActions);
            }
        }

        #endregion editing functions

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) { }
        private void Mainframe_Resize(object sender, EventArgs e) { }
        private void changeVersionToolStripMenuItem_MouseHover(object sender, EventArgs e)
        {
            ((ToolStripDropDownItem)sender).ShowDropDown();
        }

        private void DrawingTools_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void Mainframe_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Mainframe_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            switch (Path.GetExtension(files[0]).ToLowerInvariant())
            {
                case ".j2l":
                case ".lev":
                case ".lvl":
                    LoadJ2L(files[0]);
                    break;
                case ".j2t":
                case ".til":
                    ChangeTileset(files[0]);
                    break;
                default:
                    break;
            }
        }

    }

    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Mainframe fram = new Mainframe();
            Application.Run(fram);
        }
    }
}
