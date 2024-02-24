﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;

namespace MLLE
{
    public partial class PackageLevelForm : Form
    {
        string initialLevelFilepath, finalZipFilepath, levelExtension, tilesetExtension;
        bool checkScripts;

        static readonly string[] DefaultFilenames = {
            "Battle1.j2l", "Battle2.j2l", "Battle3.j2l", "Capture1.j2l", "Capture2.j2l", "Capture3.j2l", "Capture4.j2l", "Race1.j2l", "Race2.j2l", "Race3.j2l", "Treasur1.j2l", "Treasur2.j2l", "Treasur3.j2l",
            "Abattle1.j2l", "Arace1.j2l", "Arace2.j2l", "Battlea.j2l",
            "Carrot1.j2l", "Carrot1n.j2l", "Castle1.j2l", "Castle1n.j2l", "Labrat1.j2l", "Labrat2.j2l", "Labrat3.j2l", "Trainer.j2l",
            "Beach.j2l", "Beach2.j2l", "Colon1.j2l", "Colon2.j2l", "Psych1.j2l", "Psych2.j2l", "Psych3.j2l",
            "Diam1.j2l", "Diam3.j2l", "Diamsecr.j2l", "Garglair.j2l", "Medivo1.j2l", "Medivo2.j2l", "Tube1.j2l", "Tube2.j2l", "Tube3.j2l",
            "Damn.j2l", "Damn2.j2l", "Hell.j2l", "Hell2.j2l", "Jung1.j2l", "Jung2.j2l",
            "Easter1.j2l", "Easter2.j2l", "Easter3.j2l", "Haunted1.j2l", "Haunted2.j2l", "Haunted3.j2l", "Town1.j2l", "Town2.j2l", "Town3.j2l",
            "Share1.j2l", "Share2.j2l", "Share3.j2l", "Sharect2.j2l", "Sharectf.j2l", "Sharetrs.j2l",
            "Beach.j2t", "Beach2.j2t", "Carrot1.j2t", "Carrot1N.j2t", "Castle1.j2t", "Castle1N.j2t", "Colon1.j2t", "Colon2.j2t", "Damn1.j2t", "Damn2.j2t", "Diam1.j2t", "Diam2.j2t", "Easter.j2t", "Easter99.j2t", "HauntedH1.j2t", "Inferno1.j2t", "InfernoN.j2t", "Jungle1.j2t", "Jungle2.j2t", "Labrat1.j2t", "Labrat1N.j2t", "Labrat3.j2t", "Medivo.j2t", "Medivo2.j2t", "Newhaunt.j2t", "Psych1.j2t", "Psych2.j2t", "Town1.j2t", "Town2.j2t", "Tube.j2t", "TubeNite.j2t", "Xmas1.j2t", "Xmas2.j2t", "Xmas3.j2t",
            "Beach.j2b", "Bonus2.j2b", "Bonus3.j2b", "Boss1.j2b", "Boss2.j2b", "Carrotus.j2b", "Castle.j2b", "Colony.j2b", "Dang.j2b", "Diamond.j2b", "Ending.j2b", "Fastrack.j2b", "Freeze.j2b", "Funkyg.j2b", "Hell.j2b", "Intro.j2b", "Jungle.j2b", "Labrat.j2b", "Medivo.j2b", "Medivo2.j2b", "Menu.j2b", "Order.j2b", "Tubelec.j2b", "Water.j2b", "Whare.j2b", "City1g.it", "Easter.it", "Haunted.it",
            "Binnen.j2t", "Binnen.xm", "Boss.j2t", "Boss.xm", "Raintile.j2t", "Rain.xm",
            //todo AGA files I guess
        };

        class TFilenameOnlyComparer : StringComparer {
            public override int Compare(string x, string y) {
                return StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetFileName(x), Path.GetFileName(y));
            }
            public override bool Equals(string x, string y) {
                return StringComparer.InvariantCultureIgnoreCase.Equals(Path.GetFileName(x), Path.GetFileName(y));
            }
            public override int GetHashCode(string obj) {
                return StringComparer.InvariantCultureIgnoreCase.GetHashCode(Path.GetFileName(obj));
            }
        }
        static TFilenameOnlyComparer FilenameOnlyComparer = new TFilenameOnlyComparer();

        HashSet<string> fileNamesThatHaveBeenProcessedAsLevelsOrScripts = new HashSet<string>(FilenameOnlyComparer); //what it says
        HashSet<string> filePathsToInclude = new HashSet<string>(FilenameOnlyComparer); //full paths, for when creating the final ZIP
        DialogResult addFilepath (ref string newFilepath) { //this doesn't care about file extension at all
            string quotedFilename = '"' + Path.GetFileName(newFilepath) + '"';
            string filenameSeenBefore;
            if (filePathsToInclude.TryGetValue(newFilepath, out filenameSeenBefore)) //test only the filename, not the directory
            {
                newFilepath = filenameSeenBefore;
                textBox1.AppendText(Environment.NewLine + "Duplicate filename " + quotedFilename);
                return DialogResult.No;
            }
            else if (checkboxExcludeDefault.Checked && DefaultFilenames.Contains(newFilepath, FilenameOnlyComparer))
            {
                textBox1.AppendText(Environment.NewLine + "Official filename " + quotedFilename);
                return DialogResult.No;
            }
            else
            {
                if (!File.Exists(newFilepath))
                {
                    if (checkboxMissing.Checked)
                    {
                        switch (MessageBox.Show("Warning: could not locate file " + quotedFilename + " in the expected folder. Do you wish to add this file manually?", "File Not Found", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning))
                        {
                            case DialogResult.Yes:
                                while (true)
                                {
                                    var alternateFileOpenDialog = new OpenFileDialog();
                                    alternateFileOpenDialog.DefaultExt = Path.GetExtension(newFilepath);
                                    alternateFileOpenDialog.FileName = newFilepath;
                                    if (alternateFileOpenDialog.ShowDialog() == DialogResult.OK)
                                    {
                                        if (FilenameOnlyComparer.Compare(alternateFileOpenDialog.FileName, newFilepath) != 0) //different filename than the original
                                        {
                                            switch (MessageBox.Show("Warning: a file with the filename " + quotedFilename + " was expected, but you found a file with the filename \"" + Path.GetFileName(alternateFileOpenDialog.FileName) + "\". Is this okay (Yes), or would you like to try again (No)?", "Different Filename", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning))
                                            {
                                                case DialogResult.Yes:
                                                    break;
                                                case DialogResult.No:
                                                    continue;
                                                case DialogResult.Cancel:
                                                    textBox1.AppendText(Environment.NewLine + "Could not find " + quotedFilename);
                                                    textBox1.AppendText(Environment.NewLine + "Packaging cancelled by user.");
                                                    return DialogResult.Cancel;
                                            }
                                        }
                                        newFilepath = alternateFileOpenDialog.FileName;
                                        textBox1.AppendText(Environment.NewLine + "Asked user for help...");
                                        break;
                                    }
                                    else
                                        goto lblCouldNotFind; //it's not my fault C# doesn't allow falling through for switch statements
                                }
                                break;
                            case DialogResult.No:
                                lblCouldNotFind:
                                filePathsToInclude.Add(newFilepath); //so this one doesn't get asked about again
                                textBox1.AppendText(Environment.NewLine + "Could not find " + quotedFilename + ", skipping");
                                return DialogResult.No;
                            case DialogResult.Cancel:
                                textBox1.AppendText(Environment.NewLine + "Packaging cancelled by user.");
                                return DialogResult.Cancel;
                        }
                    }
                    else
                    {
                        textBox1.AppendText(Environment.NewLine + "Could not find " + quotedFilename + ", skipping");
                        return DialogResult.No;
                    }
                }
                filePathsToInclude.Add(newFilepath);
                textBox1.AppendText(Environment.NewLine + "Found " + quotedFilename);

                if (checkBoxMLLESet.Checked && Path.GetExtension(newFilepath).Equals('.' + tilesetExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    var mllesetFilepath = Path.ChangeExtension(newFilepath, "MLLESet");
                    if (File.Exists(mllesetFilepath))
                        addFilepath(ref mllesetFilepath);
                }
                return DialogResult.Yes;
            }
        }
        private DialogResult extendedAddFilepath(ref string newFilepath) //for scripts or levels
        {
            DialogResult result = addFilepath(ref newFilepath);
            if (result == DialogResult.No && File.Exists(newFilepath) && !fileNamesThatHaveBeenProcessedAsLevelsOrScripts.Contains(newFilepath)) //hasn't been processed yet
                result = DialogResult.Yes;
            if (result == DialogResult.Yes)
                fileNamesThatHaveBeenProcessedAsLevelsOrScripts.Add(newFilepath);
            return result;
        }
        private void OKButton_Click(object sender, EventArgs e)
        {
            if (OKButton.Text == "OK")
            {
                Close();
            }
            else
            {
                textBox1.AppendText(Environment.NewLine + "Start!");
                checkboxAngelscriptFunctions.Enabled = checkboxExcludeDefault.Enabled = checkboxIncludeMusic.Enabled = checkboxMissing.Enabled = checkboxMultipleLevels.Enabled = OKButton.Enabled = false;
                Directory.SetCurrentDirectory(Path.GetDirectoryName(initialLevelFilepath));

                string angelscriptPragmaPattern = "(#pragma\\s+require|#pragma\\s+offer";
                if (checkboxAngelscriptFunctions.Checked)
                    angelscriptPragmaPattern += "|jjNxt\\s*\\(|jjMusicLoad\\s*\\(|jjLayersFromLevel\\s*\\(|jjTilesFromTileset\\s*\\(|jjSampleLoad\\s*\\(\\s*SOUND\\s*::\\s*[a-z0-9_]+\\s*,";
                angelscriptPragmaPattern += ")\\s*(['\"])(.+?)\\2";

                var levelFilepaths = new List<string>();
                levelFilepaths.Add(initialLevelFilepath);

                for (int levelID = 0; levelID < levelFilepaths.Count; ++levelID)
                {
                    string levelFilepath = levelFilepaths[levelID];
                    textBox1.AppendText(Environment.NewLine + "Inspecting level \"" + levelFilepath + '"');
                    switch (extendedAddFilepath(ref levelFilepath))
                    {
                        case DialogResult.Yes:
                            break;
                        case DialogResult.No: //e.g. file not found
                            continue;
                        case DialogResult.Cancel:
                            return;
                    }
                    J2LFile j2l = new J2LFile();
                    var Data5 = new byte[0];
                    var openResults = j2l.OpenLevel(levelFilepath, ref Data5, onlyInterestedInData1:true);
                    if (!(openResults == OpeningResults.Success || openResults == OpeningResults.SuccessfulButAmbiguous))
                    {
                        textBox1.AppendText(Environment.NewLine + "Unexpected error while opening level." + Environment.NewLine + "Packaging cancelled.");
                        return;
                    }
                    if (tilesetExtension != String.Empty) //not GorH
                    {
                        var tilesetFilename = Path.ChangeExtension(j2l.MainTilesetFilename, tilesetExtension);
                        if (addFilepath(ref tilesetFilename) == DialogResult.Cancel)
                            return;
                    }
                    if (checkboxIncludeMusic.Checked)
                    {
                        var musicFilename = j2l.Music;
                        if (musicFilename.Trim() != String.Empty)
                        {
                            if (!Path.HasExtension(musicFilename))
                                musicFilename = Path.ChangeExtension(musicFilename, "j2b");
                            if (addFilepath(ref musicFilename) == DialogResult.Cancel)
                                return;
                        }
                    }
                    if (checkboxMultipleLevels.Checked)
                        foreach (var additionalLevelFilename in new string[] { j2l.NextLevel, j2l.SecretLevel })
                            if (additionalLevelFilename != String.Empty && !(additionalLevelFilename.Length >= 3 && StringComparer.InvariantCultureIgnoreCase.Compare(additionalLevelFilename.Substring(0,3), "end") == 0)) //any level filename starting with END is invalid (to JJ2, at least)
                                levelFilepaths.Add(Path.ChangeExtension(additionalLevelFilename, levelExtension)); //come back to this level later in the loop
                    if (checkScripts)
                    {
                        string mainScriptFilename = Path.ChangeExtension(levelFilepath, "j2as");
                        if (File.Exists(mainScriptFilename)) //not all levels have them, and we don't want to go down this path otherwise
                        {
                            var scriptFilepaths = new List<string>();
                            scriptFilepaths.Add(mainScriptFilename);

                            for (int scriptID = 0; scriptID < scriptFilepaths.Count; ++scriptID)
                            {
                                string scriptFilepath = scriptFilepaths[scriptID];
                                switch (extendedAddFilepath(ref scriptFilepath))
                                {
                                    case DialogResult.Yes:
                                        break;
                                    case DialogResult.No: //e.g. file not found
                                        continue;
                                    case DialogResult.Cancel:
                                        return;
                                }
                                textBox1.AppendText(Environment.NewLine + "Reading script \"" + scriptFilepath + '"');
                                var fileContents = System.IO.File.ReadAllText(scriptFilepath, J2LFile.FileEncoding);
                                foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(fileContents, "#include\\s+(['\"])(.+?)\\1", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) { //actually I don't even know whether #include is case-insensitive   jjSampleLoad(SOUND::ORANGE_BOEMR, "expmine.wav");
                                    scriptFilepaths.Add(match.Groups[2].Value); //come back to this script later in the loop
                                }
                                foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(fileContents, angelscriptPragmaPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                                {
                                    string foundFilepath = match.Groups[3].Value;
                                    if (match.Groups[1].Value.Contains("jjN")) //don't need to worry about case here, because it's a function.
                                        levelFilepaths.Add(foundFilepath);
                                    else if (addFilepath(ref foundFilepath) == DialogResult.Cancel)
                                        return;
                                }
                            }
                        }
                    }
                }

                textBox1.AppendText(Environment.NewLine + "Beginning ZIP...");
                if (File.Exists(finalZipFilepath))
                    File.Delete(finalZipFilepath);
                using (var zip = ZipFile.Open(finalZipFilepath, ZipArchiveMode.Create))
                {
                    foreach (string filepath in filePathsToInclude)
                    {
                        if (File.Exists(filepath))
                        {
                            textBox1.AppendText(Environment.NewLine + "Compressing " + filepath);
                            zip.CreateEntryFromFile(filepath, Path.GetFileName(filepath));
                        }
                        else
                        {
                            textBox1.AppendText(Environment.NewLine + "Couldn't find " + filepath);
                        }
                    }
                }

                textBox1.AppendText(Environment.NewLine + "Done! Packaged " + filePathsToInclude.Count.ToString() + " files (" + (new System.IO.FileInfo(finalZipFilepath).Length / 1024).ToString() + " kb)");
                textBox1.AppendText(Environment.NewLine + "Click OK to finish.");
                OKButton.Enabled = true;
                CancelButton.Enabled = false;
                OKButton.Text = "OK";
            }
        }

        public PackageLevelForm()
        {
            InitializeComponent();
        }

        internal void ShowForm(string ilf, string fzp, string le, string te, bool cs)
        {
            initialLevelFilepath = ilf;
            finalZipFilepath = fzp;
            levelExtension = le;
            tilesetExtension = te;
            checkScripts = cs;

            if (tilesetExtension == String.Empty) //GorH
            {
                checkboxMultipleLevels.Enabled = false;
                checkboxMultipleLevels.Checked = false;
                checkboxExcludeDefault.Enabled = false;
                checkboxExcludeDefault.Checked = false;
            }
            else if (tilesetExtension == "til") //AGA
            {
                checkboxExcludeDefault.Enabled = false;
                checkboxExcludeDefault.Checked = false;
            }
            checkboxAngelscriptFunctions.Enabled = checkScripts;

            textBox1.AppendText("Level filename: " + ilf + " ->");
            textBox1.AppendText(Environment.NewLine + "ZIP filename: " + fzp);
            textBox1.AppendText(Environment.NewLine + "Ready to start packaging!");

            ShowDialog();
        }
    }
}
