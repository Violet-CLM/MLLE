using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


    public partial class TextEdit : Form
    {
        public DialogResult result = DialogResult.Cancel;
        public string[] workTexts;
        string allAtSigns;
        public TextEdit(string[] h)
        {
            h.CopyTo(workTexts = new string[16], 0);
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            result = DialogResult.OK;
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e) { Close(); }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Paste("§");
            textBox1.Focus();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            allAtSigns = textBox1.Text.Replace(Environment.NewLine, "@");
            workTexts[listBox1.SelectedIndex] = (allAtSigns.Length > 512) ? allAtSigns.Substring(0, 512) : allAtSigns;
            label1.Text = String.Format("{0}/512 chars", allAtSigns.Length);
            label1.ForeColor = (allAtSigns.Length > 512) ? Color.Red : Color.Black;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = workTexts[listBox1.SelectedIndex].Replace("@", Environment.NewLine);
        }

        private void TextEdit_Load(object sender, EventArgs e)
        {
            listBox1.SelectedIndex = 0;
        }
    }

