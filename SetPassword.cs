using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

    public partial class SetPassword : Form
    {
        static string returnval;
        public SetPassword()
        {
            InitializeComponent();
        }
        public static string ShowForm()
        {
            returnval = null;
            SetPassword SP = new SetPassword();
            SP.ShowDialog();
            return returnval;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == textBox2.Text) { returnval = textBox1.Text; Dispose(); }
            else MessageBox.Show("The password you entered does not match the \"confirmed\" password. Please try again.", "Password mismatch", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            returnval = null;
            Dispose();
        }
    }
