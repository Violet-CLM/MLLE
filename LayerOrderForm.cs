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
    public partial class LayerOrderForm : Form
    {
        Mainframe SourceForm;
        List<Layer> SourceList;
        bool ChangesMade = false, ExistingLayerEdited = false;
        public LayerOrderForm()
        {
            InitializeComponent();
        }
        internal bool ShowForm(Mainframe source, List<Layer> layers)
        {
            SourceForm = source;
            SourceList = layers;
            listBox1.Items.AddRange(SourceList.ToArray());

            ShowDialog();

            return ChangesMade || ExistingLayerEdited;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (ChangesMade)
            {
                SourceList.Clear();
                Layer[] copyLayers = new Layer[listBox1.Items.Count];
                listBox1.Items.CopyTo(copyLayers, 0);
                SourceList.AddRange(copyLayers);
            }
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            ChangesMade = false;
            Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ButtonDelete.Enabled = (ButtonEdit.Enabled = ButtonClone.Enabled = listBox1.SelectedItem != null) && !(listBox1.SelectedItem as Layer).isDefault;
            ButtonUp.Enabled = listBox1.SelectedIndex > 0;
            ButtonDown.Enabled = listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < listBox1.Items.Count - 1;
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            int index = Math.Max(listBox1.SelectedIndex, 0);
            listBox1.Items.Insert(index, new Layer());
            listBox1.SelectedIndex = index;
            ChangesMade = true;
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            Layer layerToDelete = listBox1.SelectedItem as Layer;
            if (layerToDelete != null && !layerToDelete.isDefault)
            {
                listBox1.Items.Remove(layerToDelete);
                ChangesMade = true;
            }
        }

        private void ButtonEdit_Click(object sender, EventArgs e)
        {
            Layer layer = listBox1.SelectedItem as Layer;
            if (layer != null)
            {
                new LayerPropertiesForm(SourceForm, layer, false).ShowDialog();
                listBox1.Items[listBox1.SelectedIndex] = layer; //refresh ToString() in case the layer's name changed
                ExistingLayerEdited = true; //possibly
            }
        }

        private void ButtonUp_Click(object sender, EventArgs e)
        {
            Layer layer = listBox1.SelectedItem as Layer;
            if (layer == null) return;
            int newIndex = listBox1.SelectedIndex - 1;
            if (newIndex < 0)
                return;
            listBox1.Items.Remove(layer);
            listBox1.Items.Insert(newIndex, layer);
            listBox1.SelectedIndex = newIndex;
            ChangesMade = true;

        }

        private void ButtonClone_Click(object sender, EventArgs e)
        {
            Layer layer = listBox1.SelectedItem as Layer;
            if (layer != null)
            {
                listBox1.Items.Insert(listBox1.SelectedIndex + 1, new Layer(layer));
                ChangesMade = true;
            }
        }

        private void ButtonDown_Click(object sender, EventArgs e)
        {
            Layer layer = listBox1.SelectedItem as Layer;
            if (layer == null) return;
            int newIndex = listBox1.SelectedIndex + 1;
            if (newIndex > listBox1.Items.Count - 1)
                return;
            listBox1.Items.Remove(layer);
            listBox1.Items.Insert(newIndex, layer);
            listBox1.SelectedIndex = newIndex;
            ChangesMade = true;
        }
    }
}
