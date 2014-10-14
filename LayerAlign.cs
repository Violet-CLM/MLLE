using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


    public partial class LayerAlign : Form
    {
        public int xchange, ychange;
        public static int[] results = null;
        static LayerAlign newLayerAlign;
        public LayerAlign()
        {
            InitializeComponent();
        }

        public static int[] Show(byte layer, int x, int y)
        {
            newLayerAlign = new LayerAlign();
            newLayerAlign.xchange = x;
            newLayerAlign.ychange = y;
            newLayerAlign.TopBox.Value = 0;
            newLayerAlign.LeftBox.Value = 0;
            newLayerAlign.label1.Text = String.Format(newLayerAlign.label1.Text,layer+1, Math.Abs(y), (y >= 0) ? "tall" : "short",Math.Abs(x),(x >= 0) ? "wid" : "narrow");
            newLayerAlign.ShowDialog();
            return results;
        }

        private void TopBox_ValueChanged(object sender, EventArgs e) { BottomBox.Value = ychange + TopBox.Value; }
        private void BottomBox_ValueChanged(object sender, EventArgs e) { TopBox.Value = BottomBox.Value - ychange ; }
        private void LeftBox_ValueChanged(object sender, EventArgs e) { RightBox.Value = xchange + LeftBox.Value; }
        private void RightBox_ValueChanged(object sender, EventArgs e) { LeftBox.Value = RightBox.Value - xchange; }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            results = new int[4] { (int)newLayerAlign.TopBox.Value, (int)newLayerAlign.BottomBox.Value, (int)newLayerAlign.LeftBox.Value, (int)newLayerAlign.RightBox.Value };
            newLayerAlign.Dispose();

        }
        private void ButtonCancel_Click(object sender, EventArgs e) { results = null; newLayerAlign.Dispose(); }

        private void UpperLeft_Click(object sender, EventArgs e) { TopBox.Value = 0; LeftBox.Value = 0; }
        private void UpperCenter_Click(object sender, EventArgs e) { TopBox.Value = 0; LeftBox.Value = -xchange / 2; }
        private void UpperRight_Click(object sender, EventArgs e) { TopBox.Value = 0; RightBox.Value = 0; }
        private void LeftMiddle_Click(object sender, EventArgs e) { TopBox.Value = -ychange / 2; LeftBox.Value = 0; }
        private void MiddleCenter_Click(object sender, EventArgs e) { TopBox.Value = -ychange / 2; LeftBox.Value = -xchange / 2; }
        private void RightMiddle_Click(object sender, EventArgs e) { TopBox.Value = -ychange / 2; RightBox.Value = 0; }
        private void LowerLeft_Click(object sender, EventArgs e) { BottomBox.Value = 0; LeftBox.Value = 0; }
        private void LowerCenter_Click(object sender, EventArgs e) { BottomBox.Value = 0; LeftBox.Value = -xchange / 2; }
        private void LowerRight_Click(object sender, EventArgs e) { BottomBox.Value = 0; RightBox.Value = 0; }
    }

