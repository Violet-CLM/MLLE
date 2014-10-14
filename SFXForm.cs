using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;

    public partial class SFXForm : Form
    {
        public DialogResult result = DialogResult.Cancel;
        public string[][] Paths = new string[48][];
        private string[] resFilenames;
        private string DefaultDirectory;
        private Dictionary<string, VOres> VOFiles = new Dictionary<string, VOres>();

        public SFXForm(string[][] p, string d)
        {
            Paths = p;
            DefaultDirectory = d;
            InitializeComponent();
        }
        private void SFXForm_Load(object sender, EventArgs e)
        {
            resFilenames = Directory.GetFiles(DefaultDirectory, "VO*.res");
            for (int i = 0; i < resFilenames.Length; i++) Files.Items.Add(Path.GetFileNameWithoutExtension(resFilenames[i]));
        }

        private void ButtonOK_Click(object sender, EventArgs e)  { result = DialogResult.OK; Close(); }

        private void ButtonCancel_Click(object sender, EventArgs e) {  Close();  }

        private void InLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Paths[InLevel.SelectedIndex] == null)
            {
                Files.SelectedIndex = 0;
            }
            else
            {
                Files.SelectedIndex = Files.FindStringExact(Paths[InLevel.SelectedIndex][0]);
                Sounds.SelectedIndex = Sounds.FindStringExact(Paths[InLevel.SelectedIndex][1]);
            }
        }

        private void Files_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sounds.Items.Clear();
            if (Files.SelectedIndex == 0)
            {
                Paths[InLevel.SelectedIndex] = null;
            }
            else
            {
                string fileName = resFilenames[Files.SelectedIndex - 1];
                if (!VOFiles.ContainsKey(fileName)) VOFiles[fileName] = new VOres(fileName);
                Sounds.Items.AddRange(VOFiles[fileName].SFXnames);
            }
        }

        private void Sounds_SelectedIndexChanged(object sender, EventArgs e)
        {
            Paths[InLevel.SelectedIndex][0] = (string)Files.SelectedItem;
            Paths[InLevel.SelectedIndex][1] = (string)Sounds.SelectedItem;
        }

    }
