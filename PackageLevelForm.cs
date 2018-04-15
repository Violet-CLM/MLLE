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
    public partial class PackageLevelForm : Form
    {
        string initialLevelFilepath, finalZipFilepath, levelExtension, tilesetExtension;
        bool checkScripts;
        public PackageLevelForm()
        {
            InitializeComponent();
        }

        internal void ShowForm(string ilf, string fzp, string le, string te, bool cs)
        {
            initialLevelFilepath = ilf;
            finalZipFilepath = fzp;
            levelExtension = le;
            tilesetExtension = te; //String.Empty if LEV
            checkScripts = cs;

            textBox1.AppendText("Level filename: " + ilf + " ->");
            textBox1.AppendText(Environment.NewLine + "ZIP filename: " + fzp);
            textBox1.AppendText(Environment.NewLine + "Ready to start packaging!");

            ShowDialog();
        }
    }
}
