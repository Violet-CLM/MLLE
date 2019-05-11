using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ini;

namespace MLLE
{
    public partial class WeaponsForm : Form
    {
        PlusPropertyList.Weapon[] weaponsInProgress;
        PlusPropertyList.Weapon[] weaponsSource;

        public WeaponsForm()
        {
            InitializeComponent();
        }

        class ExtendedWeapon : PlusPropertyList.Weapon, IComparable<ExtendedWeapon>
        {
            public Bitmap Image;
            enum oTypes { Int, Bool, Dropdown };
            string[] OptionNames;
            oTypes[] OptionTypes;
            string[][] OptionOptions;
            public ExtendedWeapon(string[] s)
            {
                Name = s[0];

                Image = (string.IsNullOrEmpty(s[1])) ? new Bitmap(1, 1): new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Weapons", s[1]));

                if (!string.IsNullOrEmpty(s[4]))
                {
                    string[] optionsSplitByPipes = s[4].Split('|').Select(ss => ss.Trim()).ToArray();
                    int numberOfOptions = optionsSplitByPipes.Length; //usually zero
                    Options = new int[numberOfOptions];
                    OptionNames = new string[numberOfOptions];
                    OptionTypes = new oTypes[numberOfOptions];
                    OptionOptions = new string[numberOfOptions][];
                    for (int i = 0; i < numberOfOptions; ++i)
                    {
                        string[] optionSplitByColons = optionsSplitByPipes[i].Split(':').Select(ss => ss.Trim()).ToArray();
                        OptionNames[i] = optionSplitByColons[0];

                        string optionType = optionSplitByColons[1];
                        if (optionType.Equals("bool", StringComparison.OrdinalIgnoreCase))
                            OptionTypes[i] = oTypes.Bool;
                        else if (optionType[0] == '{' && optionType[optionType.Length - 1] == '}')
                        {
                            OptionTypes[i] = oTypes.Dropdown;
                            OptionOptions[i] = optionType.Substring(1, optionType.Length - 2).Split(',').Select(ss => ss.Trim()).ToArray();
                        }

                        if (optionSplitByColons.Length == 3)
                        {
                            string optionDefaultValue = optionSplitByColons[2];
                            if (optionDefaultValue.Equals("True", StringComparison.OrdinalIgnoreCase)) //even if it's not a Bool, I mean really, who cares.
                                Options[i] = 1;
                            else if (optionDefaultValue.Equals("False", StringComparison.OrdinalIgnoreCase))
                                Options[i] = 0;
                            else if (!int.TryParse(optionDefaultValue, out Options[i]))
                                Options[i] = 0;
                        }
                    }
                }
                else
                    Options = new int[0];
            }
            static internal readonly string[] KeysToReadFromIni = {"Name", "ImageFilename", "Library", "ClassName", "Options"};
            public int CompareTo(ExtendedWeapon other)
            {
                return Name.CompareTo(other.Name);
            }

            internal void AddOptionControls(int id, Panel panel, int y)
            {
                var label = new Label();
                label.Text = OptionNames[id];
                label.Top = y;
                label.AutoSize = true;
                panel.Controls.Add(label);
                Control control;
                switch (OptionTypes[id]) {
                    case oTypes.Bool: {
                        control = new CheckBox();
                        (control as CheckBox).Checked = Options[id] != 0;
                        control.AutoSize = true;
                        break; }
                    case oTypes.Dropdown: {
                        control = new ComboBox();
                        (control as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                        (control as ComboBox).DropDownWidth *= 2;
                        (control as ComboBox).Items.AddRange(OptionOptions[id]);
                        (control as ComboBox).SelectedIndex = Options[id];
                        break; }
                    default: {
                        control = new NumericUpDown();
                        (control as NumericUpDown).Value = Options[id];
                        break; }
                }
                control.Location = new Point(label.Right + 3, y);
                control.Width = panel.ClientSize.Width - control.Left;
                panel.Controls.Add(control);
            }
        }
        List<ExtendedWeapon> AllAvailableWeapons = new List<ExtendedWeapon>();

        internal void ShowForm(PlusPropertyList.Weapon[] s)
        {
            weaponsInProgress = (weaponsSource = s).Select(w => w.Clone()).ToArray();

            var backupDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Weapons");
            var allIniFiles = new DirectoryInfo(backupDirectory).GetFiles("*.ini", SearchOption.TopDirectoryOnly);
            foreach (var iniFilename in allIniFiles)
            {
                var iniFile = new IniFile(iniFilename.FullName);
                int weaponDefinedInIniID = 0;
                while (true)
                {
                    var sectionName = weaponDefinedInIniID++.ToString();
                    if (string.IsNullOrEmpty(iniFile.IniReadValue(sectionName, "Name"))) //run out of weapons in this ini
                        break;
                    AllAvailableWeapons.Add(new ExtendedWeapon(ExtendedWeapon.KeysToReadFromIni.Select(k => iniFile.IniReadValue(sectionName, k).Trim()).ToArray()));
                }
            }
            AllAvailableWeapons.Sort();
            var weaponNames = AllAvailableWeapons.Select(w => w.Name).ToArray();

            for (int weaponID = 0; weaponID < 9; ++weaponID)
            {
                var panel = new Panel();
                panel.BorderStyle = BorderStyle.Fixed3D;
                panel.VerticalScroll.Visible = true;
                panel.VerticalScroll.Enabled = false;
                tableLayoutPanel1.Controls.Add(panel);

                var number = new Label();
                number.Text = (weaponID + 1).ToString();
                number.Font = new Font(number.Font.FontFamily, 16);
                number.Width = 20;
                panel.Controls.Add(number);

                var dropdown = new ComboBox();
                dropdown.Items.AddRange(weaponNames);
                dropdown.Top = panel.Height - dropdown.Height - 3;
                dropdown.DropDownStyle = ComboBoxStyle.DropDownList;
                dropdown.DropDownWidth += dropdown.DropDownWidth / 2;
                int localWeaponID = weaponID;
                dropdown.SelectedIndexChanged += (ss, ee) => {
                    weaponsInProgress[localWeaponID] = AllAvailableWeapons.Find(w => w.Name == (ss as ComboBox).SelectedItem.ToString()).Clone();
                    UpdatePanel(localWeaponID);
                };
                panel.Controls.Add(dropdown);

                var image = new PictureBox();
                image.Size = new Size(32, 32);
                image.Left = panel.ClientSize.Width - image.Width - 3;
                panel.Controls.Add(image);

                UpdatePanel(weaponID);
            }

            ShowDialog();
        }

        void UpdatePanel(int weaponID)
        {
            Panel panel = tableLayoutPanel1.Controls[weaponID] as Panel;
            int index = AllAvailableWeapons.FindIndex(w => w.Name == weaponsInProgress[weaponID].Name);
            (panel.Controls[1] as ComboBox).SelectedIndex = index;
            var fullWeapon = AllAvailableWeapons[index];
            (panel.Controls[2] as PictureBox).Image = fullWeapon.Image;
            panel.VerticalScroll.Value = 0;
            while (panel.Controls.Count > 3)
                panel.Controls.RemoveAt(3);
            panel.AutoScroll = panel.VerticalScroll.Visible = panel.VerticalScroll.Enabled = fullWeapon.Options.Length > 0;
            (panel.Controls[1] as ComboBox).Width = panel.ClientSize.Width;
            for (int i = 0; i < fullWeapon.Options.Length; ++i)
            {
                fullWeapon.AddOptionControls(i, panel, panel.Controls[1].Bottom + 3 + i * 15);
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            weaponsSource = weaponsInProgress.Select(w => w.Clone()).ToArray();
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
