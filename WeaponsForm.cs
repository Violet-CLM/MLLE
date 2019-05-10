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
        internal void ShowForm(PlusPropertyList.Weapon[] s)
        {
            weaponsInProgress = (weaponsSource = s).Select(w => w.Clone()).ToArray();
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
