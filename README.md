Multi-layered level editor
==========================

A (largely stable) alternative level editor to JCS. MLLE offers various options that JCS does not, such as JJ2+ support (particularly infinite layers and tilesets per level, palette editing, super-easy custom weapons, and redrawable tiles/masks), fill and rectangle drawing tools, longer text strings, an integrated layer edit/parallax view screen, zooming above 100%, typing event names, more useful parameter fields for many events, autocompleting tiles, more complex tile selection options, a redo buffer, better layer resizing, automatic ZIP creation, and support (both editing levels and creating tilesets) for every single related game (1.23, 1.24, Battery Check, 1.10o, 1.00g/h, and Animaniacs: A Gigantic Adventure) (for the latter two, this is the only program of any kind that exists for editing them).

Installation instructions:

* The “Weapons” folder itself should be in the same folder as MLLE.exe.
* The contents of the “ExtractToJJ2+Folder” folder should be extracted into the same folder as Jazz2.exe.
* Extract the contents of Standard Weapon Interface into your JJ2+ folder prior to using MLLE’s Weapons window.
* If you already have a previous release of MLLE installed, you should not re-extract any of the .ini files, for risk of losing your personal settings.

There’s no help file, because help files are hard, but here are some hotkeys:
1-8: View that layer.
Ctrl + 1-8: Edit layer properties for that layer.
Ctrl + plus/minus: Zoom in/out.
Ctrl + M: Toggle mask mode.
M: View partial mask.
Ctrl + P: Toggle parallax mode.
P: View partial parallax.
Ctrl + V: Toggle event view.
Ctrl + Shift + R: Save & Run, with the start position temporarily moved to wherever your mouse is.
F, Backspace, E, Ctrl + E, Shift + E: You know how these things work.
I: Flip tiles vertically (available only for JJ2+ levels)
Comma: Copy current tile.
Shift + Comma: Copy current tile and the event on it.
B: Begin or end a selection of tiles to grab.
Delete: Clear layer/selection.
Ctrl + C: Copy current selection.
Ctrl + X: Copy and delete current selection.
Ctrl + D: Deselect all.
Ctrl + Z, Ctrl + Y: Undo, Redo.
For editing tileset:
Shift + T: Assign tile transparent tiletype.
Shift + 0-9: Assign tile that tiletype, if possible. (e.g. 1 for Transparent and 4 for Caption in regular JJ2 levels.)
Additionally, holding down the Control key while using either of the first two drawing tools (Paintbrush and Fill) triggers an alternate mode wherein they work somewhat differently. The Control key also serves as an eyedropper tool when you are redrawing tile images in JJ2+ levels.

Building
--------

1) Open Renderer1.csproj using Visual Studio
2) Add references to all dll's except bass.dll under the 'Other files to distribute' folder
   * Right click 'Renderer1' in the Solution Explorer view -> Add -> Reference -> browse
3) Build (Right click Renderer1 in solution explorer -> Build solution)
4) Copy all files from 'Other files to distribute' to bin/Debug
5) Copy MLLE.ini from your JJ2 installation here
6) Run
