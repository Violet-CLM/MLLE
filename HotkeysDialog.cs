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
    public class HotkeyInfo
    {
        public string Category { get; set; }
        public string Action { get; set; }
        public string Hotkey { get; set; }
        public string Context { get; set; }
        public bool IsCustomizable { get; set; }
    }

    public partial class HotkeysDialog : Form
    {
        public HotkeysDialog()
        {
            InitializeComponent();
            PopulateHotkeyData();
        }

        private void PopulateHotkeyData()
        {
            var hotkeyData = new List<HotkeyInfo>
            {
                // File Operations
                new HotkeyInfo { Category = "File Operations", Action = "New Level", Hotkey = "Ctrl+N", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "File Operations", Action = "Open Level", Hotkey = "Ctrl+O", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "File Operations", Action = "Save Level", Hotkey = "Ctrl+S", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "File Operations", Action = "Save & Run", Hotkey = "Ctrl+R", Context = "Global", IsCustomizable = false },

                // Edit Operations
                new HotkeyInfo { Category = "Edit Operations", Action = "Undo", Hotkey = "Ctrl+Z", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Edit Operations", Action = "Redo", Hotkey = "Ctrl+Y", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Edit Operations", Action = "Copy Selection", Hotkey = "Ctrl+C", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Edit Operations", Action = "Cut Selection", Hotkey = "Ctrl+X", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Edit Operations", Action = "Deselect All", Hotkey = "Ctrl+D", Context = "Global", IsCustomizable = false },

                // Layer Navigation
                new HotkeyInfo { Category = "Layer Navigation", Action = "View Layer 1", Hotkey = "1", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "View Layer 2", Hotkey = "2", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "View Layer 3", Hotkey = "3", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "View Layer 4", Hotkey = "4", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "View Layer 5", Hotkey = "5", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "View Layer 6", Hotkey = "6", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "View Layer 7", Hotkey = "7", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "View Layer 8", Hotkey = "8", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "Edit Layer Properties 1", Hotkey = "Ctrl+1", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "Edit Layer Properties 2", Hotkey = "Ctrl+2", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "Edit Layer Properties 3", Hotkey = "Ctrl+3", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "Edit Layer Properties 4", Hotkey = "Ctrl+4", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "Edit Layer Properties 5", Hotkey = "Ctrl+5", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "Edit Layer Properties 6", Hotkey = "Ctrl+6", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "Edit Layer Properties 7", Hotkey = "Ctrl+7", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "Edit Layer Properties 8", Hotkey = "Ctrl+8", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "Previous Layer", Hotkey = "Ctrl+L", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Layer Navigation", Action = "Next Layer", Hotkey = "Shift+L", Context = "Global", IsCustomizable = false },

                // Display Controls
                new HotkeyInfo { Category = "Display Controls", Action = "Zoom In", Hotkey = "Ctrl+Plus", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Display Controls", Action = "Zoom Out", Hotkey = "Ctrl+Minus", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Display Controls", Action = "Toggle Mask Mode", Hotkey = "Ctrl+M", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Display Controls", Action = "View Partial Mask (hold)", Hotkey = "M", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Display Controls", Action = "Toggle Parallax Mode", Hotkey = "Ctrl+P", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Display Controls", Action = "View Partial Parallax (hold)", Hotkey = "P", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Display Controls", Action = "Toggle Event View", Hotkey = "Ctrl+V", Context = "Global", IsCustomizable = false },

                // Tile Operations
                new HotkeyInfo { Category = "Tile Operations", Action = "Flip Tiles Horizontally", Hotkey = "F", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Operations", Action = "Smart Flip Horizontally", Hotkey = "Ctrl+F", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Operations", Action = "Flip Tiles Vertically (JJ2+ only)", Hotkey = "I", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Operations", Action = "Smart Flip Vertically (JJ2+ only)", Hotkey = "Ctrl+I", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Operations", Action = "Copy Current Tile", Hotkey = "Comma (,)", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Operations", Action = "Copy Tile + Event", Hotkey = "Shift+Comma", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Operations", Action = "Use Blank Tile", Hotkey = "Backspace", Context = "Global", IsCustomizable = false },

                // Tile Types (JJ2+ Tileset Editing)
                new HotkeyInfo { Category = "Tile Types", Action = "Assign Transparent Tiletype", Hotkey = "Shift+T", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Types", Action = "Assign Tiletype 0", Hotkey = "Shift+0", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Types", Action = "Assign Tiletype 1", Hotkey = "Shift+1", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Types", Action = "Assign Tiletype 2", Hotkey = "Shift+2", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Types", Action = "Assign Tiletype 3", Hotkey = "Shift+3", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Types", Action = "Assign Tiletype 4", Hotkey = "Shift+4", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Types", Action = "Assign Tiletype 5", Hotkey = "Shift+5", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Types", Action = "Assign Tiletype 6", Hotkey = "Shift+6", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Types", Action = "Assign Tiletype 7", Hotkey = "Shift+7", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Types", Action = "Assign Tiletype 8", Hotkey = "Shift+8", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Types", Action = "Assign Tiletype 9", Hotkey = "Shift+9", Context = "Global", IsCustomizable = false },

                // Selection Tools
                new HotkeyInfo { Category = "Selection Tools", Action = "Begin/End Selection", Hotkey = "B", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Selection Tools", Action = "Add to Selection", Hotkey = "Shift+B", Context = "Global", IsCustomizable = true },
                new HotkeyInfo { Category = "Selection Tools", Action = "Subtract from Selection", Hotkey = "Ctrl+B", Context = "Global", IsCustomizable = true },
                new HotkeyInfo { Category = "Selection Tools", Action = "Clear Layer/Selection", Hotkey = "Delete", Context = "Global", IsCustomizable = false },

                // Event Operations
                new HotkeyInfo { Category = "Event Operations", Action = "Select Event at Mouse", Hotkey = "E", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Event Operations", Action = "Copy Event at Mouse", Hotkey = "Ctrl+E", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Event Operations", Action = "Paste Event at Mouse", Hotkey = "Shift+E", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Event Operations", Action = "Copy Event", Hotkey = "Ctrl+Left-click", Context = "Level Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Event Operations", Action = "Copy Event", Hotkey = "Ctrl+Right-click", Context = "Level Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Event Operations", Action = "Select Event", Hotkey = "Middle-click", Context = "Level Editor", IsCustomizable = false },

                // Navigation
                new HotkeyInfo { Category = "Navigation", Action = "Scroll Left", Hotkey = "Left Arrow", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Navigation", Action = "Scroll Right", Hotkey = "Right Arrow", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Navigation", Action = "Pan View", Hotkey = "Middle-click", Context = "Level Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Navigation", Action = "Pan View", Hotkey = "Tab+Drag", Context = "Level Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Navigation", Action = "Scroll", Hotkey = "Mouse Wheel", Context = "Level/Tileset Editors", IsCustomizable = false },

                // Special Functions
                new HotkeyInfo { Category = "Special Functions", Action = "Save & Run from Mouse Position", Hotkey = "Ctrl+Shift+R", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Special Functions", Action = "Special X Mode (hold)", Hotkey = "X", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Special Functions", Action = "Special X Mode with Shift (hold)", Hotkey = "Shift+X", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Special Functions", Action = "Special Y Mode (hold)", Hotkey = "Y", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Special Functions", Action = "Special Y Mode with Shift (hold)", Hotkey = "Shift+Y", Context = "Global", IsCustomizable = false },
                new HotkeyInfo { Category = "Special Functions", Action = "Tab Mode (hold)", Hotkey = "Tab", Context = "Global", IsCustomizable = false },

                // Palette Form
                new HotkeyInfo { Category = "Palette Form", Action = "Select None", Hotkey = "Ctrl+D", Context = "Palette Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Palette Form", Action = "Select 1 Color", Hotkey = "Ctrl+1", Context = "Palette Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Palette Form", Action = "Select 8 Colors", Hotkey = "Ctrl+8", Context = "Palette Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Palette Form", Action = "Select 16 Colors", Hotkey = "Ctrl+6", Context = "Palette Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Palette Form", Action = "Gradient", Hotkey = "Ctrl+G", Context = "Palette Editor", IsCustomizable = false },

                // Sprite Recolor Form
                new HotkeyInfo { Category = "Sprite Recolor Form", Action = "Select None", Hotkey = "Ctrl+D", Context = "Sprite Recolor Dialog", IsCustomizable = false },

                // Tile Image Editor Form
                new HotkeyInfo { Category = "Tile Image Editor", Action = "Flip", Hotkey = "F", Context = "Tile Image Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Image Editor", Action = "Invert", Hotkey = "I", Context = "Tile Image Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Image Editor", Action = "Rotate", Hotkey = "R", Context = "Tile Image Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Tile Image Editor", Action = "Eyedropper Tool", Hotkey = "Ctrl+Left-click", Context = "Tile Image Editor", IsCustomizable = false },

                // Animation Editing
                new HotkeyInfo { Category = "Animation Editing", Action = "Delete Animation Frame", Hotkey = "Delete", Context = "Animation Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Animation Editing", Action = "Flip Animation Frame", Hotkey = "F", Context = "Animation Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Animation Editing", Action = "Invert Animation Frame (JJ2+ only)", Hotkey = "I", Context = "Animation Editor", IsCustomizable = false },
                new HotkeyInfo { Category = "Animation Editing", Action = "Insert Animation Frame", Hotkey = "Ctrl+Click", Context = "Animation Editor", IsCustomizable = false }
            };

            // Bind to DataGridView
            dataGridViewHotkeys.DataSource = hotkeyData;

            // Configure columns
            dataGridViewHotkeys.Columns["Category"].Width = 120;
            dataGridViewHotkeys.Columns["Action"].Width = 200;
            dataGridViewHotkeys.Columns["Hotkey"].Width = 120;
            dataGridViewHotkeys.Columns["Context"].Width = 100;
            dataGridViewHotkeys.Columns["IsCustomizable"].Visible = false; // Hide for MVP

            // Make read-only
            dataGridViewHotkeys.ReadOnly = true;
            dataGridViewHotkeys.AllowUserToAddRows = false;
            dataGridViewHotkeys.AllowUserToDeleteRows = false;
            dataGridViewHotkeys.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}