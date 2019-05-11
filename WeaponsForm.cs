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
            public ExtendedWeapon(string[] s)
            {
                Name = s[0];

                Image = (string.IsNullOrEmpty(s[1])) ? new Bitmap(1, 1): new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Weapons", s[1]));

                string[] optionSpecs = s[4].Split('|').Select(ss => ss.Trim()).ToArray();
                Options = new int[optionSpecs.Length];
            }
            static internal readonly string[] KeysToReadFromIni = {"Name", "ImageFilename", "Library", "ClassName", "Options"};
            public int CompareTo(ExtendedWeapon other)
            {
                return Name.CompareTo(other.Name);
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
                tableLayoutPanel1.Controls.Add(panel);

                var number = new Label();
                number.Text = (weaponID + 1).ToString();
                number.Font = new Font(number.Font.FontFamily, 16);
                number.Width = 20;
                panel.Controls.Add(number);

                var dropdown = new ComboBox();
                dropdown.Items.AddRange(weaponNames);
                dropdown.Top = panel.Height - dropdown.Height - 3;
                dropdown.Width = panel.Width;
                dropdown.DropDownStyle = ComboBoxStyle.DropDownList;
                int localWeaponID = weaponID;
                dropdown.SelectedIndexChanged += (ss, ee) => {
                    weaponsInProgress[localWeaponID] = AllAvailableWeapons.Find(w => w.Name == (ss as ComboBox).SelectedItem.ToString()).Clone();
                    UpdatePanel(localWeaponID);
                };
                panel.Controls.Add(dropdown);

                var image = new PictureBox();
                image.Size = new Size(64, 64);
                image.Left = panel.Width - image.Width - 3;
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
            (panel.Controls[2] as PictureBox).Image = AllAvailableWeapons[index].Image;
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
