using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        /*class ExtendedWeapon : PlusPropertyList.Weapon
        {
            ExtendedWeapon(string n, int[] o, string[] on,
        }*/
        internal void ShowForm(PlusPropertyList.Weapon[] s)
        {
            weaponsInProgress = (weaponsSource = s).Select(w => w.Clone()).ToArray();

            var weaponNames = new string[] { "blas", "bou", "icey", "seek", "fast", "hot", "boom", "lol", "walls" }; //TEMP

            for (int weaponID = 0; weaponID < 9; ++weaponID)
            {
                var panel = new Panel();
                panel.BorderStyle = BorderStyle.Fixed3D;
                tableLayoutPanel1.Controls.Add(panel);

                var number = new Label();
                number.Text = (weaponID + 1).ToString();
                number.Font = new Font(number.Font.FontFamily, 16);
                panel.Controls.Add(number);

                var dropdown = new ComboBox();
                dropdown.Items.AddRange(weaponNames);
                dropdown.Top = panel.Height - dropdown.Height - 3;
                dropdown.Width = panel.Width;
                dropdown.DropDownStyle = ComboBoxStyle.DropDownList;
                dropdown.SelectedItem = dropdown.Items[weaponID];
                panel.Controls.Add(dropdown);

            }

            ShowDialog();
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
