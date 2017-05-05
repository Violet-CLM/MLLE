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
        PlusPropertyList? result = null;
        public PlusProperties()
        {
            InitializeComponent();
        }

        public PlusPropertyList? ShowForm(ref PlusPropertyList current)
        {
            propertyGrid1.SelectedObject = new PlusPropertyList(current);
            ShowDialog();
            return result;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            //do I need to do any validation here?
            result = (PlusPropertyList)propertyGrid1.SelectedObject;
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
