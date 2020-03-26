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
        
        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            var findParameterName = comboBox1.Text.Trim();
            var parameterLocationsPerEvent = new int[256];
            var foundValues = new SortedSet<int>();

            if (findParameterName.Length > 0)
            {
                for (int eventID = 1; eventID < 256; ++eventID)
                {
                    var ev = EventStrings[eventID];
                    for (int i = 5; i < ev.Length; ++i)
                        if (String.Equals(ev[i].Split(':')[0].Trim(), findParameterName, StringComparison.OrdinalIgnoreCase)) {
                            parameterLocationsPerEvent[eventID] = i - 4;
                            break;
                        }
                }
            }

            foreach (var eventBits in EventMap) {
                var eventID = eventBits & 0xFF;
                if (parameterLocationsPerEvent[eventID] != 0)
                    foundValues.Add(Mainframe.ExtractParameterValues(eventBits, EventStrings[eventID])[parameterLocationsPerEvent[eventID] - 1]);
            }

            listBox1.Items.AddRange(foundValues.Select(val => val.ToString()).ToArray());
            if (listBox1.Items.Count == 0)
                ResultPrintout.Text = "(found no matches)";
            else
            {
                int i;
                for (i = 0; foundValues.Contains(i); ++i) ;
                ResultPrintout.Text = "First free value: " + i.ToString();
            }
        }
    }
}
