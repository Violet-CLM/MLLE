using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MLLE
{
    enum InputControlType { Numbox, Checkbox, Dropdown, DropdownUsedForStrings, Textbox }

    public partial class EventForm : Form
    {
        const int ParameterVerticalMargin = -37;
        byte MostParametersSeenThusFar = 1;
        Mainframe SourceForm;
        string[] CurrentEvent;
        string[][] LevelSpecificEventStringList;
        byte CurrentEventID;
        Version version;
        //public uint resultint;
        AGAEvent WorkingEvent;
        int bitpush = 0;
        InputControlType[] ParameterInputType = new InputControlType[24];
        int[] LastParameterValues = new int[24];
        string[] LastParameterStrings = new string[6];
        int ReadCycledValues, ReadCycledStrings, WriteCycledValues, WriteCycledStrings;
        NumericUpDown[] ParamBoxes;
        Label[] ParamLabels;
        CheckBox[] ParamCheckboxes;
        CheckBox[] BitBoxes;
        ComboBox[] ParamDropdowns;
        Button[] ParamSoundPlayerButtons;
        TextBox[] ParamTextboxes;
        Dictionary<ComboBox, List<ComboBox>> CombosPointingToCombos;
        bool SafeToCalculate = false, SafeToCacheOldParameters = false;
        static List<UInt32> LastUsedEvents = new List<UInt32>(0);
        List<Mainframe.StringAndIndex> FlatEventList;
        public EventForm(Mainframe parent, TreeNode[] nodes, Version theVersion, AGAEvent inputevent, string[][] eventStrings, ImageList treeImageList)
        {
            WorkingEvent = inputevent;
            SourceForm = parent;
            parent.SelectReturnAGAEvent = null;
            version = theVersion;
            LevelSpecificEventStringList = eventStrings;
            InitializeComponent();
            int arrayLength = (version == Version.AGA) ? 24 : 6;
            ParamBoxes = new NumericUpDown[arrayLength]; ParamBoxes[0] = numericUpDown1;
            ParamSoundPlayerButtons = new Button[arrayLength]; ParamSoundPlayerButtons[0] = button1; button1.Tag = numericUpDown1;
            ParamLabels = new Label[arrayLength]; ParamLabels[0] = label1;
            ParamCheckboxes = new CheckBox[arrayLength]; ParamCheckboxes[0] = checkBox1;
            ParamDropdowns = new ComboBox[arrayLength]; ParamDropdowns[0] = comboBox1;
            if (version == Version.AGA)
            {
                BitBoxes = new CheckBox[] { BitBox1, BitBox2, BitBox3, BitBox4, BitBox5, BitBox6, BitBox7, BitBox8, BitBox9, BitBox10 };
                ParamTextboxes = new TextBox[24]; ParamTextboxes[0] = textBox1;
            }
            else textBox1.Dispose();
            CombosPointingToCombos = new Dictionary<ComboBox, List<ComboBox>> { { comboBox1, new List<ComboBox>() } };
            Tree.ImageList = treeImageList;
            Tree.ShowLines = treeImageList == null;
            Tree.Indent = (treeImageList == null) ? 19 : 6;
            Tree.DrawMode = (treeImageList == null) ? TreeViewDrawMode.Normal : TreeViewDrawMode.OwnerDrawAll;
            Tree.Nodes.Add("0", "(none)", 0); //always present
            Tree.Nodes.AddRange(nodes);
            Tree.Sort();
            if (version != Version.AGA)
            {
                Tree.Sorted = false; //add new things freely in arbitary orders
                if (LastUsedEvents.Count > 0)
                {
                    var recentNodes = new TreeNode("[Recent]");
                    foreach (UInt32 lastEvent in LastUsedEvents)
                    {
                        var node = new TreeNode(SourceForm.NameEvent(lastEvent, "(unknown)"), (int)lastEvent & 0xFF, (int)lastEvent & 0xFF);
                        node.Tag = lastEvent;
                        recentNodes.Nodes.Add(node);
                    }
                    Tree.Nodes.Add(recentNodes);
                }
            }
            ListBox.Size = Tree.Size;
            FlatEventList = SourceForm.FlatEventLists[SourceForm.J2L.VersionType];
        }

        void SetTextOfAndPossiblyCreateLabel(byte id, string text)
        {
            if (ParamLabels[id] == null)
            {
                ParamLabels[id] = new Label();
                this.panel1.Controls.Add(ParamLabels[id]);
                ParamLabels[id].AutoSize = true;
                ParamLabels[id].Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                ParamLabels[id].Location = new System.Drawing.Point(3, ParamLabels[0].Location.Y - ParameterVerticalMargin * id);
                ParamLabels[id].Size = new System.Drawing.Size(10, 13);
            }
            ParamLabels[id].Text = text;
        }
        Control RetrieveAndPossiblyCreateControl(Control[] controlArray, byte id)
        {
            if (controlArray[id] == null)
            {
                if (controlArray == ParamCheckboxes)
                {
                    ParamCheckboxes[id] = new CheckBox();
                    ParamCheckboxes[id].AutoSize = true;
                    ParamCheckboxes[id].UseVisualStyleBackColor = true;
                    ParamCheckboxes[id].CheckStateChanged += new System.EventHandler(this.checkBox_CheckStateChanged);
                }
                else if (controlArray == ParamBoxes)
                {
                    ParamBoxes[id] = new NumericUpDown();
                    ParamBoxes[id].AllowDrop = true;
                    ParamBoxes[id].ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
                    ParamBoxes[id].DragDrop += new System.Windows.Forms.DragEventHandler(this.numericUpDown_DragDrop);
                    ParamBoxes[id].DragEnter += new System.Windows.Forms.DragEventHandler(this.input_DragEnter);
                    ParamBoxes[id].Size = new System.Drawing.Size(80, 20);
                }
                else if (controlArray == ParamDropdowns)
                {
                    ParamDropdowns[id] = new ComboBox();
                    ParamDropdowns[id].DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                    ParamDropdowns[id].DropDownWidth = 200;
                    ParamDropdowns[id].FormattingEnabled = true;
                    ParamDropdowns[id].Size = new System.Drawing.Size(80, 21);
                    ParamDropdowns[id].SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
                    CombosPointingToCombos.Add(ParamDropdowns[id], new List<ComboBox>());
                }
                else if (controlArray == ParamTextboxes)
                {
                    ParamTextboxes[id] = new TextBox();
                    ParamTextboxes[id].Size = new System.Drawing.Size(80, 20);
                }
                else if (controlArray == ParamSoundPlayerButtons)
                {
                    ParamSoundPlayerButtons[id] = new Button();
                    ParamSoundPlayerButtons[id].Size = new System.Drawing.Size(20, 20);
                    ParamSoundPlayerButtons[id].Tag = ParamBoxes[id];
                    ParamSoundPlayerButtons[id].Click += new System.EventHandler(this.SoundButton_Click);
                }
                this.panel1.Controls.Add(controlArray[id]);
                controlArray[id].Location = new System.Drawing.Point(controlArray[0].Location.X, controlArray[0].Location.Y - ParameterVerticalMargin * id);
            }
            return controlArray[id];
        }

        int GetParamValue(byte id)
        {
            switch (ParameterInputType[id])
            {
                case InputControlType.Checkbox: return ((CheckBox)RetrieveAndPossiblyCreateControl(ParamCheckboxes, id))/*ParamCheckboxes[id]*/.Checked ? 1 : 0;
                case InputControlType.Dropdown: return ((ComboBox)RetrieveAndPossiblyCreateControl(ParamDropdowns, id))/*ParamDropdowns[id]*/.SelectedIndex;
                case InputControlType.Numbox: return (int)((NumericUpDown)RetrieveAndPossiblyCreateControl(ParamBoxes, id)).Value;
                default: return 0;
            }
        }
        void SetParamValue(byte id, int value)
        {
            switch (ParameterInputType[id])
            {
                case InputControlType.Checkbox: ((CheckBox)RetrieveAndPossiblyCreateControl(ParamCheckboxes, id)).Checked = value != 0; break;
                case InputControlType.Dropdown: case InputControlType.DropdownUsedForStrings: ((ComboBox)RetrieveAndPossiblyCreateControl(ParamDropdowns, id)).SelectedIndex = value % ParamDropdowns[id].Items.Count; break;
                case InputControlType.Textbox: ((TextBox)RetrieveAndPossiblyCreateControl(ParamBoxes, id)).Text = value.ToString(); break;
                case InputControlType.Numbox: default: ((NumericUpDown)RetrieveAndPossiblyCreateControl(ParamBoxes, id)).Value = value; break;
            }
        }

        private void EventForm_Load(object sender, EventArgs e)
        {
            CurrentEventID = (byte)(WorkingEvent.ID & 255);
            CurrentEvent = LevelSpecificEventStringList[CurrentEventID];
            MostParametersSeenThusFar = (byte)(Math.Max(1, CurrentEvent.Length - 5));
            if (version != Version.AGA)
            {
                Generator.Visible = SourceForm.GeneratorEventID != null;
                if (!Generator.Visible) Tree.Height += (Generator.Bottom - Tree.Bottom);
            }
            Dictionary<EnableableTitles, String> labels = SourceForm.EnableableStrings[version];
            Illuminate.Text = labels[EnableableTitles.Illuminate];
            Illuminate.Visible = Illuminate.Text != "" && version != Version.AGA;
            ModeLabel.Text = labels[EnableableTitles.DiffLabel];
            ModeSelect.Items.Add(labels[EnableableTitles.Diff1]);
            ModeSelect.Items.Add(labels[EnableableTitles.Diff2]);
            ModeSelect.Items.Add(labels[EnableableTitles.Diff3]);
            ModeSelect.Items.Add(labels[EnableableTitles.Diff4]);
            ModeLabel.Visible = ModeSelect.Visible = ModeLabel.Text != "" && version != Version.AGA;
            Bitfield.Visible = !(BitBox1.Visible = BitBox2.Visible = BitBox3.Visible = BitBox4.Visible = BitBox5.Visible = BitBox6.Visible = BitBox7.Visible = BitBox8.Visible = BitBox9.Visible = BitBox10.Visible = version == Version.AGA);
            if (version == Version.AGA)
            {
                for (byte i = 0; i < WorkingEvent.Longs.Length; i++)
                {
                    if (i < WorkingEvent.Strings.Length) { LastParameterStrings[i] = WorkingEvent.Strings[i] ?? ""; }
                    if (i < WorkingEvent.Bits.Length) { BitBoxes[i].Checked = WorkingEvent.Bits[i]; }
                    LastParameterValues[i] = WorkingEvent.Longs[i];
                }
                Findit(CurrentEventID);
            }
            else
            {
                CheckEverythingForThisNewEvent();
            }
            //SafeToCalculate = true;
            SafeToCacheOldParameters = true;
        }

        private void CheckEverythingForThisNewEvent()
        {
            ModeSelect.SelectedIndex = (int)J2LFile.GetRawBits(WorkingEvent.ID, 8, 2);
            Illuminate.Checked = (WorkingEvent.ID & 1024) == 1024;
            int[] parmvalues = Mainframe.ExtractParameterValues(WorkingEvent.ID, CurrentEvent);
            for (byte i = 0; i < 6; i++) LastParameterValues[i] = parmvalues[i];
            if (CurrentEventID == SourceForm.GeneratorEventID) { SetupEvent(); Generator.Checked = true; Findit((byte)parmvalues[0]); }
            else Findit(CurrentEventID);
        }

        private void Findit(byte value)
        {
            try { Tree.SelectedNode = Tree.Nodes.Find(value.ToString(), true)[0]; }
            catch { Tree.Nodes.Add(value.ToString(), LevelSpecificEventStringList[value][0]); Findit(value); }
        }
        private void ButtonCancel_Click(object sender, EventArgs e) { Close(); }
        public void ResetTree() { Tree.Nodes.Clear(); }

        internal int[] GetParameterRange(int raw)
        {
            if (raw >= 0) return new int[2] { 0, (int)Math.Pow(2, raw) - 1 };
            else return new int[2] { -(int)Math.Pow(2, Math.Abs(raw) - 1), (int)Math.Pow(2, Math.Abs(raw) - 1) - 1 };
        }

        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (Tree.SelectedNode.GetNodeCount(false) == 0)
            {
                if (Generator.Checked)
                {
                    numericUpDown1.Value = Convert.ToByte(Tree.SelectedNode.Name);
                }
                else
                {
                    try //regular event
                    {
                        CurrentEventID = Convert.ToByte(Tree.SelectedNode.Name);
                        SetupEvent();
                    }
                    catch //[Recent] event
                    {
                        useNewArbitaryEvent(Convert.ToUInt32(Tree.SelectedNode.Tag));
                    }
                }
            }
        }

        private void Generator_CheckedChanged(object sender, EventArgs e)
        {
            if (Generator.Checked)
            {
                if (SafeToCalculate)
                {
                    numericUpDown1.Maximum = 255;
                    numericUpDown1.Value = CurrentEventID;
                    CurrentEventID = (byte)SourceForm.GeneratorEventID;
                    SetupEvent();
                }
            }
            else
            {
                CurrentEventID = (byte)numericUpDown1.Value;
                SetupEvent();
            }
        }

        internal void SetupEvent()
        {
            CurrentEvent = LevelSpecificEventStringList[CurrentEventID];
            int[] range;
            string mode;
            SafeToCalculate = false;
            ReadCycledValues = ReadCycledStrings = WriteCycledValues = WriteCycledStrings = 0;
            if (SafeToCacheOldParameters) for (byte i = 0; i < MostParametersSeenThusFar; i++)
                {
                    switch (ParameterInputType[i])
                    {
                        case InputControlType.Textbox:
                            LastParameterStrings[WriteCycledStrings] = ParamTextboxes[i].Text;
                            WriteCycledStrings++;
                            break;
                        case InputControlType.DropdownUsedForStrings:
                            LastParameterStrings[WriteCycledStrings] = (string)ParamDropdowns[i].SelectedItem;
                            WriteCycledStrings++;
                            break;
                        default:
                            LastParameterValues[WriteCycledValues] = GetParamValue(i);
                            WriteCycledValues++;
                            break;
                    }
                }
            MostParametersSeenThusFar = (byte)Math.Max(MostParametersSeenThusFar, CurrentEvent.Length - 5);
            for (byte i = 0; i < MostParametersSeenThusFar; i++)
            {
                RetrieveAndPossiblyCreateControl(ParamDropdowns, i);
                RetrieveAndPossiblyCreateControl(ParamBoxes, i);
                RetrieveAndPossiblyCreateControl(ParamSoundPlayerButtons, i);
                RetrieveAndPossiblyCreateControl(ParamCheckboxes, i);
                if (version == Version.AGA) RetrieveAndPossiblyCreateControl(ParamTextboxes, i);
                if (ParamDropdowns[i] != null) CombosPointingToCombos[ParamDropdowns[i]].Clear();
                if (CurrentEvent.Length - 5 > i)
                {
                    mode = CurrentEvent[i + 5].Split(':')[1];
                    switch (mode.Substring(0, 1))
                    {
                        case "B":
                        case "b":
                        case "C":
                        case "c":
                            ParameterInputType[i] = InputControlType.Checkbox;
                            ParamCheckboxes[i].Checked = LastParameterValues[ReadCycledValues] != 0;
                            ParamCheckboxes[i].Text = CurrentEvent[i + 5].Split(':')[0];
                            SetTextOfAndPossiblyCreateLabel(i, ParamCheckboxes[i].Text);
                            ParamLabels[i].Visible = ParamCheckboxes[i].Visible = true; ParamSoundPlayerButtons[i].Visible = ParamBoxes[i].Visible = ParamDropdowns[i].Visible = false;
                            if (version == Version.AGA) ParamTextboxes[i].Visible = false;
                            ReadCycledValues++;
                            break;
                        case "T":
                        case "t":
                            ParameterInputType[i] = InputControlType.Dropdown;
                            SetTextOfAndPossiblyCreateLabel(i, CurrentEvent[5 + i].Split(':')[0]);
                            ParamLabels[i].Visible = ParamDropdowns[i].Visible = true; ParamSoundPlayerButtons[i].Visible = ParamBoxes[i].Visible = ParamCheckboxes[i].Visible = false;
                            if (version == Version.AGA) ParamTextboxes[i].Visible = false;
                            ParamDropdowns[i].Items.Clear();
                            ParamDropdowns[i].Items.AddRange(SourceForm.J2L.Text);
                            ParamDropdowns[i].SelectedIndex = LastParameterValues[ReadCycledValues];
                            ReadCycledValues++;
                            break;
                        case "P":
                        case "p":
                            ParameterInputType[i] = InputControlType.Dropdown;
                            SetTextOfAndPossiblyCreateLabel(i, CurrentEvent[5 + i].Split(':')[0]);
                            ParamLabels[i].Visible = ParamDropdowns[i].Visible = true; ParamSoundPlayerButtons[i].Visible = ParamBoxes[i].Visible = ParamCheckboxes[i].Visible = false;
                            if (version == Version.AGA) ParamTextboxes[i].Visible = false;
                            ParamDropdowns[i].Items.Clear();
                            CombosPointingToCombos[ParamDropdowns[Convert.ToByte(mode.Substring(1, 1))]].Add(ParamDropdowns[i]);
                            ParamDropdowns[i].Items.AddRange(((string)ParamDropdowns[Convert.ToByte(mode.Substring(1, 1))].SelectedItem).Split('|'));
                            if (ParamDropdowns[i].Items.Count == 0) ParamDropdowns[i].Items.Add("");
                            ParamDropdowns[i].SelectedIndex = Math.Min(ParamDropdowns[i].Items.Count - 1, LastParameterValues[ReadCycledValues]);
                            ReadCycledValues++;
                            break;
                        case "{":
                            ParameterInputType[i] = InputControlType.Dropdown;
                            SetTextOfAndPossiblyCreateLabel(i, CurrentEvent[5 + i].Split(':')[0]);
                            ParamLabels[i].Visible = ParamDropdowns[i].Visible = true; ParamSoundPlayerButtons[i].Visible = ParamBoxes[i].Visible = ParamCheckboxes[i].Visible = false;
                            if (version == Version.AGA) ParamTextboxes[i].Visible = false;
                            ParamDropdowns[i].Items.Clear();
                            ParamDropdowns[i].Items.AddRange(mode.Substring(1, mode.LastIndexOf('}') - 1).Split(','));
                            ParamDropdowns[i].SelectedIndex = Math.Min(ParamDropdowns[i].Items.Count - 1, LastParameterValues[ReadCycledValues]);
                            ReadCycledValues++;
                            break;
                        case "s":
                        case "S":
                            ParameterInputType[i] = InputControlType.Numbox;
                            SetTextOfAndPossiblyCreateLabel(i, CurrentEvent[5 + i].Split(':')[0]);
                            ParamLabels[i].Visible = ParamSoundPlayerButtons[i].Visible = ParamBoxes[i].Visible = true; ParamCheckboxes[i].Visible = ParamDropdowns[i].Visible = false;
                            if (version == Version.AGA) ParamTextboxes[i].Visible = false;
                            range = GetParameterRange(Convert.ToInt32(mode.Substring(1)));
                            ParamBoxes[i].Minimum = range[0]; ParamBoxes[i].Maximum = range[1];
                            foreach (Control c in ParamBoxes[i].Controls)
                                toolTip.SetToolTip(c, String.Format("Enter a number between {0} and {1}.", range[0], range[1]));
                            try
                            {
                                ParamBoxes[i].Value = LastParameterValues[ReadCycledValues];
                            }
                            catch
                            {
                                ParamBoxes[i].Value = 0;
                            }
                            ReadCycledValues++;
                            break;
                        case "A":
                        case "a":
                            switch (mode.Substring(1, 1))
                            {
                                case "T":
                                case "t":
                                    ParameterInputType[i] = InputControlType.Dropdown;
                                    SetTextOfAndPossiblyCreateLabel(i, CurrentEvent[5 + i].Split(':')[0]);
                                    ParamLabels[i].Visible = ParamDropdowns[i].Visible = true; ParamSoundPlayerButtons[i].Visible = ParamBoxes[i].Visible = ParamCheckboxes[i].Visible = false;
                                    if (version == Version.AGA) ParamTextboxes[i].Visible = false;
                                    ParamDropdowns[i].Items.Clear();
                                    ParamDropdowns[i].Items.AddRange(SourceForm.J2L.Text);
                                    ParamDropdowns[i].SelectedIndex = LastParameterValues[ReadCycledValues];
                                    ReadCycledValues++;
                                    break;
                                case "{":
                                    ParameterInputType[i] = InputControlType.DropdownUsedForStrings;
                                    SetTextOfAndPossiblyCreateLabel(i, CurrentEvent[5 + i].Split(':')[0]);
                                    ParamLabels[i].Visible = ParamDropdowns[i].Visible = true; ParamSoundPlayerButtons[i].Visible = ParamBoxes[i].Visible = ParamCheckboxes[i].Visible = false;
                                    if (version == Version.AGA) ParamTextboxes[i].Visible = false;
                                    ParamDropdowns[i].Items.Clear();
                                    ParamDropdowns[i].Items.AddRange(mode.Substring(2, mode.LastIndexOf('}') - 2).Split(','));
                                    try { ParamDropdowns[i].SelectedIndex = ParamDropdowns[i].FindStringExact(LastParameterStrings[ReadCycledStrings], 0); }
                                    catch { ParamDropdowns[i].SelectedIndex = 0; }
                                    ReadCycledStrings++;
                                    break;
                                case "h":
                                case "H":
                                    ParameterInputType[i] = InputControlType.Textbox;
                                    SetTextOfAndPossiblyCreateLabel(i, CurrentEvent[5 + i].Split(':')[0]);
                                    ParamSoundPlayerButtons[i].Visible = ParamBoxes[i].Visible = ParamCheckboxes[i].Visible = ParamDropdowns[i].Visible = false;
                                    if (version == Version.AGA) ParamLabels[i].Visible = ParamTextboxes[i].Visible = true;
                                    ParamTextboxes[i].Text = LastParameterStrings[ReadCycledStrings];
                                    ReadCycledStrings++;
                                    break;
                                case "l":
                                case "L":
                                default:
                                    ParameterInputType[i] = InputControlType.Numbox;
                                    SetTextOfAndPossiblyCreateLabel(i, CurrentEvent[5 + i].Split(':')[0]);
                                    ParamLabels[i].Visible = ParamBoxes[i].Visible = true; ParamSoundPlayerButtons[i].Visible = ParamCheckboxes[i].Visible = ParamDropdowns[i].Visible = false;
                                    if (version == Version.AGA) ParamTextboxes[i].Visible = false;
                                    ParamBoxes[i].Minimum = int.MinValue; ParamBoxes[i].Maximum = int.MaxValue;
                                    foreach (Control c in ParamBoxes[i].Controls)
                                        toolTip.SetToolTip(c, String.Format("Enter a number between {0} and {1}.", int.MinValue, int.MaxValue));
                                    try
                                    {
                                        ParamBoxes[i].Value = LastParameterValues[ReadCycledValues];
                                    }
                                    catch
                                    {
                                        ParamBoxes[i].Value = 0;
                                    }
                                    ReadCycledValues++;
                                    break;
                            }
                            break;
                        default:
                            ParameterInputType[i] = InputControlType.Numbox;
                            SetTextOfAndPossiblyCreateLabel(i, CurrentEvent[5 + i].Split(':')[0]);
                            ParamLabels[i].Visible = ParamBoxes[i].Visible = true; ParamSoundPlayerButtons[i].Visible = ParamCheckboxes[i].Visible = ParamDropdowns[i].Visible = false;
                            if (version == Version.AGA) ParamTextboxes[i].Visible = false;
                            range = GetParameterRange(Convert.ToInt32(mode));
                            ParamBoxes[i].Minimum = range[0]; ParamBoxes[i].Maximum = range[1];
                            foreach (Control c in ParamBoxes[i].Controls)
                                toolTip.SetToolTip(c, String.Format("Enter a number between {0} and {1}.", range[0], range[1]));
                            try
                            {
                                ParamBoxes[i].Value = LastParameterValues[ReadCycledValues];
                            }
                            catch
                            {
                                ParamBoxes[i].Value = 0;
                            }
                            ReadCycledValues++;
                            break;
                    }
                }
                else { ParamLabels[i].Visible = ParamBoxes[i].Visible = ParamCheckboxes[i].Visible = ParamDropdowns[i].Visible = false; if (version == Version.AGA) ParamTextboxes[i].Visible = false; }
            }
            SafeToCalculate = true;
            if (!(version == Version.AGA))
            {
                WorkingEvent.ID = CalculateOutputEvent();
                Bitfield.Text = Convert.ToString((int)WorkingEvent.ID, 2).PadLeft(32, '0');
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            SafeToCalculate = false;
            if (version == Version.AGA)
            {
                WorkingEvent = new AGAEvent(0);
                WorkingEvent.ID = CurrentEventID;
                WriteCycledStrings = WriteCycledValues = 0;
                for (byte i = 0; i < 10; i++) { WorkingEvent.Bits[i] = BitBoxes[i].Checked; }
                for (byte i = 0; CurrentEvent.Length - 5 > i; i++)
                {
                    if (i < WorkingEvent.Strings.Length) { WorkingEvent.Strings[i] = ""; }
                    switch (ParameterInputType[i])
                    {
                        case InputControlType.Textbox:
                            WorkingEvent.Strings[WriteCycledStrings] = ParamTextboxes[i].Text;
                            WriteCycledStrings++;
                            break;
                        case InputControlType.DropdownUsedForStrings:
                            WorkingEvent.Strings[WriteCycledStrings] = (string)ParamDropdowns[i].SelectedItem;
                            WriteCycledStrings++;
                            break;
                        default:
                            WorkingEvent.Longs[WriteCycledValues] = GetParamValue(i);
                            WriteCycledValues++;
                            break;
                    }
                }
                SourceForm.SelectReturnAGAEvent = WorkingEvent;
            }
            else
            {
                var outputEvent = CalculateOutputEvent();
                if (outputEvent != 0)
                { //not a strictly necessary check, I guess, but it feels a little silly keeping track of event 0
                    while (LastUsedEvents.Remove(outputEvent)) { } //no duplicates
                    LastUsedEvents.Insert(0, outputEvent);
                    while (LastUsedEvents.Count > 10)
                        LastUsedEvents.RemoveAt(10);
                }
                SourceForm.SelectReturnAGAEvent = new AGAEvent(outputEvent);
            }
            Close();
        }

        private void Tree_ItemDrag(object sender, ItemDragEventArgs e) // www.codeguru.com/forum/showpost.php?p=1817797&postcount=2
        {
            Tree.DoDragDrop(e.Item, DragDropEffects.Move);
        }
        private void input_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode))) e.Effect = DragDropEffects.Move;
        }
        private void numericUpDown_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode tn = (TreeNode)e.Data.GetData(typeof(TreeNode));
                try { ((NumericUpDown)sender).Value = Convert.ToInt32(tn.Name); } catch { ((NumericUpDown)sender).Value = 0; }
            }
        }

        private int? GetValueOr(int value, byte id, int? def)
        {
            if (value >= 0) return value;
            else return GetParameterRange(Convert.ToInt32(CurrentEvent[id + 5].Split(':')[1]))[0] * -2 + value;
        }
        static internal int GetNumberAtEndOfString(string str, bool includeNegative)
        {
            return Convert.ToInt32(new System.Text.RegularExpressions.Regex("(?<number>" + (includeNegative ? "-?" : "") + "\\d+)$").Match(str).ToString());
        }

        private uint CalculateOutputEvent()
        {
            uint result = (uint)(CurrentEventID | ModeSelect.SelectedIndex << 8 | ((Illuminate.Checked) ? 1024 : 0));
            bitpush = 12;
            //string size;
            for (byte i = 0; i < MostParametersSeenThusFar && i < CurrentEvent.Length - 5; i++)
            {
                result |= (uint)GetValueOr(GetParamValue(i), i, 0) << bitpush;
                //size = CurrentEvent[i + 5].Split(':')[1];
                //bitpush += Convert.ToByte(size.Substring(size.Length-1,1));
                bitpush += GetNumberAtEndOfString(CurrentEvent[i + 5], false);
            }
            result |= WorkingEvent.ID & (uint)(0xFFFFFFFFuL << bitpush); //keep hidden parameter values
            return result;
        }

        private void PopulatePointingBoxes(ComboBox sender)
        {
            foreach (ComboBox pointer in CombosPointingToCombos[(ComboBox)sender])
            {
                pointer.Items.Clear();
                pointer.Items.AddRange(((string)sender.SelectedItem).Split('|'));
                if (pointer.Items.Count == 0) pointer.Items.Add("");
                pointer.SelectedIndex = 0;
            }
        }
        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulatePointingBoxes((ComboBox)sender);
            if (SafeToCalculate && version != Version.AGA)
            {
                WorkingEvent.ID = CalculateOutputEvent();
                Bitfield.Text = Convert.ToString((int)WorkingEvent.ID, 2).PadLeft(32, '0');
            }
        }

        private void checkBox_CheckStateChanged(object sender, EventArgs e)
        {
            if (SafeToCalculate && version != Version.AGA)
            {
                WorkingEvent.ID = CalculateOutputEvent();
                Bitfield.Text = Convert.ToString((int)WorkingEvent.ID, 2).PadLeft(32, '0');
            }
        }

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (SafeToCalculate && version != Version.AGA)
            {
                WorkingEvent.ID = CalculateOutputEvent();
                Bitfield.Text = Convert.ToString((int)WorkingEvent.ID, 2).PadLeft(32, '0');
            }
        }

        BinaryReader j2a; //sound code by Neobeo

        private void Bitfield_MouseClick(object sender, MouseEventArgs e)
        {
            useNewArbitaryEvent(WorkingEvent.ID ^ (0x80000000u >> ((e.X - 3) / 6)));
        }
        private void useNewArbitaryEvent(uint newEvent)
        {
            WorkingEvent.ID = newEvent;
            CurrentEventID = (byte)(WorkingEvent.ID & 255);
            CurrentEvent = LevelSpecificEventStringList[CurrentEventID];
            SafeToCacheOldParameters = false;
            CheckEverythingForThisNewEvent();
            Tree_AfterSelect(null, null);
            SafeToCacheOldParameters = true;
            Bitfield.Text = Convert.ToString((int)WorkingEvent.ID, 2).PadLeft(32, '0');
        }

        private void EventNameInput_TextChanged(object sender, EventArgs e)
        {
            ListBox.Items.Clear();
            if (ListBox.Visible = (EventNameInput.Text.Length > 0))
            {
                ListBox.Items.AddRange(FlatEventList.Where(i => System.Globalization.CultureInfo.InvariantCulture.CompareInfo.IndexOf(i.String, EventNameInput.Text, System.Globalization.CompareOptions.IgnoreCase) >= 0).Cast<object>().ToArray());
            }
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListBox.SelectedItem != null)
            {
                var results = Tree.Nodes.Find(((Mainframe.StringAndIndex)ListBox.SelectedItem).Index.ToString(), true);
                if (results.Length == 1)
                {
                    EventNameInput.Clear();
                    Tree.SelectedNode = results[0];
                }
            }
        }

        private void Tree_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (!(e.DrawDefault = e.Node.Nodes.Count == 0)) //folder node
            {
                Rectangle bounds = e.Bounds;
                if (e.Node.Parent != null) bounds.X += Tree.Indent;
                ControlPaint.DrawCheckBox(//https://stackoverflow.com/questions/22382471/ownerdrawn-treeview-winforms
                    e.Graphics,
                    new Rectangle(
                        new Point(bounds.X, bounds.Y + 1),
                        Tree.ImageList.ImageSize
                    ),
                    e.Node.IsExpanded ? ButtonState.Checked : ButtonState.Normal
                );
                bounds.X += Tree.ImageList.ImageSize.Width + 2;
                Font font = new Font((sender as TreeView).Font, FontStyle.Bold);
                if (e.Node.IsSelected)
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, bounds);
                StringFormat stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(e.Node.Text, font, e.Node.IsSelected ? SystemBrushes.HighlightText : SystemBrushes.ControlText, bounds, stringFormat);
            }
        }

        int i, s, v, sets;
        static readonly byte[] magic = { 82, 73, 70, 70, 87, 65, 86, 69, 102, 109, 116, 32,
                                                16, 0, 0, 0, 1, 0, 1, 0, 100, 97, 116, 97 };
        private void SoundButton_Click(object sender, EventArgs e)
        {
            if (j2a == null)
            {
                j2a = new BinaryReader(File.Open(Path.Combine(SourceForm.DefaultDirectories[version], "Anims.j2a"), FileMode.Open, FileAccess.Read), J2File.FileEncoding);
                j2a.ReadBytes(24);
                sets = j2a.ReadInt32();
            }
            v = s = (int)((NumericUpDown)((Button)sender).Tag).Value;
            ((Button)sender).BackColor = ButtonOK.BackColor;
            try
            {
                if (SourceForm.AmbientSounds[version][v] == null)
                {
                    MemoryStream stream = SourceForm.AmbientSounds[version][v] = new MemoryStream(65536);
                    for (i = 0; i < sets; i++)
                    {
                        j2a.BaseStream.Position = 28 + i * 4;
                        j2a.BaseStream.Position = j2a.ReadInt32() + 5;
                        int samples = j2a.ReadByte();
                        if (s < samples) break;
                        else s -= samples;
                    }
                    if (i == sets) return;

                    j2a.ReadBytes(6);
                    int[] size = new int[8]; for (i = 0; i < 8; i++) size[i] = j2a.ReadInt32();
                    j2a.ReadBytes(size[0] + size[2] + size[4] + 2);

                    using (var bz = new BinaryReader(new System.IO.Compression.DeflateStream(j2a.BaseStream, System.IO.Compression.CompressionMode.Decompress, true), J2File.FileEncoding))
                    {
                        for (i = 0; i < s; i++) bz.ReadBytes(bz.ReadInt32() - 4);
                        bz.ReadBytes(64);
                        int mul = bz.ReadInt16() / 4 + 1; bz.ReadInt16();
                        int length = bz.ReadInt32(); bz.ReadInt64();
                        int rate = bz.ReadInt32(); bz.ReadInt64();
                        //Console.WriteLine("Length: {0:0.000}s", (double)length / rate);
                        length *= mul;

                        var bw = new BinaryWriter(stream, J2File.FileEncoding);
                        {
                            bw.Write(magic, 0, 4);
                            bw.Write(length + 36);
                            bw.Write(magic, 4, 16);
                            bw.Write(rate);
                            bw.Write(rate * mul);
                            bw.Write(mul * 0x80001);
                            bw.Write(magic, 20, 4);
                            bw.Write(length);
                            for (i = 0; i < length; i++) bw.Write((byte)(bz.ReadByte() ^ (mul << 7)));
                        }
                    }
                }
                SourceForm.AmbientSounds[version][v].Seek(0, 0);
                new System.Media.SoundPlayer(SourceForm.AmbientSounds[version][v]).PlaySync();
            }
            catch { ((Button)sender).BackColor = Color.Red; }
        }

        private void EventForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (j2a != null) { j2a.Dispose(); j2a = null; }
        }
    }
}

