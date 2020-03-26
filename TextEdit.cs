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
    bool poundColors, tildeColors;
    public TextEdit(string[] h, bool p, bool t)
    {
        poundColors = p;
        tildeColors = t;
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
        richTextBox1.SelectedText = "§";
        richTextBox1.Focus();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool LockWindowUpdate(IntPtr hWndLock);

    readonly static Color[] PossibleFontColors = { Color.SaddleBrown, Color.DarkGray, Color.DarkTurquoise, Color.DarkGreen, Color.Red, Color.Blue, Color.DarkOrange, Color.HotPink };
    private void richTextBox1_TextChanged(object sender, EventArgs e)
    {
        allAtSigns = richTextBox1.Text.Replace("\r", "").Replace("\n", "@");
        workTexts[listBox1.SelectedIndex] = (allAtSigns.Length > 512) ? allAtSigns.Substring(0, 512) : allAtSigns;
        label1.Text = String.Format("{0}/512 chars", allAtSigns.Length);
        label1.ForeColor = (allAtSigns.Length > 512) ? Color.Red : Color.Black;
        if (poundColors)
        {
            LockWindowUpdate(richTextBox1.Handle);
            int colorID = 0;
            int selectionStart = 0;
            int lastCharacter = richTextBox1.Text.Length;
            bool currentlyHashingColors = false;
            var selectionPos = richTextBox1.SelectionStart;
            for (int characterIndex = 0; characterIndex < lastCharacter; ++characterIndex)
            {
                switch (richTextBox1.Text[characterIndex]) {
                    case '#':
                        currentlyHashingColors = true;
                        break;
                    case '~':
                        if (tildeColors)
                        {
                            if (!currentlyHashingColors)
                            {
                                richTextBox1.SelectionStart = selectionStart;
                                richTextBox1.SelectionLength = characterIndex - selectionStart;
                                richTextBox1.SelectionColor = PossibleFontColors[colorID];
                                selectionStart = characterIndex;
                                colorID = 0;
                            }
                            else
                                currentlyHashingColors = false;
                        } 
                        else
                            colorID = (colorID + 1) & 7;
                        break;
                    case ' ':
                    case '\r':
                    case '\n':
                    case '@':
                        continue;
                    case '§':
                        characterIndex += 1;
                        continue;
                    default:
                        if (currentlyHashingColors)
                        {
                            richTextBox1.SelectionStart = selectionStart;
                            richTextBox1.SelectionLength = characterIndex - selectionStart;
                            richTextBox1.SelectionColor = PossibleFontColors[colorID];
                            selectionStart = characterIndex;
                            colorID = (colorID + 1) & 7;
                        }
                        break;
                }
            }
            richTextBox1.SelectionStart = selectionStart;
            richTextBox1.SelectionLength = lastCharacter - selectionStart;
            richTextBox1.SelectionColor = (selectionStart != 0) ? PossibleFontColors[colorID] : Color.Black;
            richTextBox1.SelectionStart = selectionPos;
            richTextBox1.SelectionLength = 0;
            LockWindowUpdate(IntPtr.Zero);
        }
    }

    private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        richTextBox1.Text = workTexts[listBox1.SelectedIndex].Replace("@", Environment.NewLine);
    }

    private void TextEdit_Load(object sender, EventArgs e)
    {
        listBox1.SelectedIndex = 0;
    }
}

