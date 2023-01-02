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
    public partial class Arguments : Form
    {
        static readonly string[] possibleArguments = {
            "-spaz", "-lori",
            "-easy", "-hard", "-turbo",
            "-coop", "-battle", "-capture", "-race", "-treasure",
            "-dctf", "-dom", "-fr", "-head", "-jb", "-lrs", "-pest", "-rt", "-tb", "-tlrs", "-xlrs",
            "-player 2", "-player 3", "-player 4",
            "-server", "-list"
        };
        readonly Tuple<ComboBox, int, int>[] SimpleBoxes;

        TextBox Source;
        public Arguments(TextBox source)
        {
            Source = source;
            InitializeComponent();

            SimpleBoxes = new Tuple<ComboBox, int, int>[]{
                new Tuple<ComboBox, int, int>(characters, 0, 1),
                new Tuple<ComboBox, int, int>(difficulties, 2, 4),
                new Tuple<ComboBox, int, int>(gamemodes, 5, 20),
                new Tuple<ComboBox, int, int>(playernumber, 21, 23)
            };
            foreach (var simpleBox in SimpleBoxes)
            {
                simpleBox.Item1.SelectedIndex = 0;
                int check = simpleBox.Item2;
                int newIndex = 1;
                do
                {
                    if (Source.Text.IndexOf(possibleArguments[check], StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        simpleBox.Item1.SelectedIndex = newIndex;
                        break;
                    }
                    ++newIndex;
                } while (++check <= simpleBox.Item3);
            }

            if (Source.Text.IndexOf("-server", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (Source.Text.IndexOf("-list", StringComparison.OrdinalIgnoreCase) >= 0)
                    online.SelectedIndex = 2;
                else
                    online.SelectedIndex = 1;
            }
            else
                online.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            characters.Enabled = online.SelectedIndex == 0;
        }

        private void gamemodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (online.SelectedIndex == 0 && gamemodes.SelectedIndex >= 6)
                online.SelectedIndex = 1;
        }
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            string output = Source.Text.ToLower();
            foreach (string old in possibleArguments)
                output = output.Replace(old, "");

            foreach (var simpleBox in SimpleBoxes)
                if (simpleBox.Item1.SelectedIndex != 0)
                    output = possibleArguments[simpleBox.Item1.SelectedIndex + simpleBox.Item2 - 1] + " " + output;

            if (online.SelectedIndex == 2)
                output = "-list " + output;
            if (online.SelectedIndex >= 1)
                output = "-server " + output;

            Source.Text = System.Text.RegularExpressions.Regex.Replace(output, @"\s+", " ");

            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
