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
    public partial class FindParameterValues : Form
    {
        public FindParameterValues()
        {
            InitializeComponent();
        }
        uint[,] EventMap;
        string[][] EventStrings;
        internal void ShowForm(ref uint[,] eventMap, string[][] eventStrings)
        {
            EventMap = eventMap;
            EventStrings = eventStrings;

            ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
