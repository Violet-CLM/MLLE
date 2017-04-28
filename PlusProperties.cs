using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace MLLE
{
    public partial class PlusProperties : Form
    {
        public PlusProperties()
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = new PlusPropertyList();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
