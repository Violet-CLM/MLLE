using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public partial class PasswordInputForm : Form
    {
        static string returnval;
        public PasswordInputForm()
        {
            InitializeComponent();
        }
        public static string ShowForm(OpeningResults condition)
        {
            PasswordInputForm newPIF = new PasswordInputForm();
            returnval = null;
            if (condition == OpeningResults.PasswordNeeded) newPIF.label1.Text = "A password is needed to open this level.";
            else if (condition == OpeningResults.WrongPassword) newPIF.label1.Text = "Sorry, wrong password. Please try again.";
            newPIF.ShowDialog();
            return returnval;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            returnval = textBox1.Text;
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e) { Dispose(); }
    }
