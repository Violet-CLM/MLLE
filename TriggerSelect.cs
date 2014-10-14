using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public partial class TriggerSelect : Form
    {
    static uint result;
        public TriggerSelect()
        {
            InitializeComponent();
        }
        public static uint ShowForm(bool isTeam, uint oldZone)
        {
            TriggerSelect newTS = new TriggerSelect();
            result = 0;
            newTS.checkBox1.Visible = isTeam;
            if ((oldZone & 255) == 246) { oldZone >>= 12; newTS.numericUpDown1.Value = (oldZone & 31); if (isTeam) newTS.checkBox1.Checked = (oldZone > 31); }
            newTS.ShowDialog();
            return result;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e) {  }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            result = 246 + ((uint)numericUpDown1.Value << 12) + (uint)((checkBox1.Checked) ? 1 << 17 : 0);
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e) { Dispose(); }

    }
