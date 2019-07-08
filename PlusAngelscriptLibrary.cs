using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;
using Ionic.Crc;
using System.Text.RegularExpressions;
using System.Linq;

namespace MLLE
{
    public partial struct PlusPropertyList
    {
        const uint CurrentMLLEData5Version = 0x105;
        const string MLLEData5MagicString = "MLLE";
        const string CurrentMLLEData5VersionStringForComparison = "0x105";
        const string CurrentMLLEData5VersionString = "1.5";

        const string AngelscriptLibrary =
@"//This is a standard library created by MLLE to read some JJ2+ properties from a level file whose script includes this library. DO NOT MANUALLY MODIFY THIS FILE.





#pragma require '{0}'
namespace MLLE {{
    jjPAL@ Palette;
    dictionary@ _layers;{1}

    bool Setup({2}) {{
        jjPAL palette = jjBackupPalette;
        @Palette = @palette;
        dictionary layers;
        @_layers = @layers;

        jjSTREAM crcCheck('{0}');
        string crcLine;
        if (crcCheck.isEmpty() || !crcCheck.getLine(crcLine)) {{
            jjDebug('MLLE::Setup: Include file has been renamed!');
            return false;
        }}
        array<string> regexResults;
        if (!jjRegexMatch(crcLine, '\\/\\/(\\d+)\\r?', regexResults)) {{
            jjDebug('MLLE::Setup: Include file is improperly formatted!');
            return false;
        }}
        if (parseUInt(regexResults[1]) != jjCRC32(crcCheck)) {{
            jjDebug('MLLE::Setup: Include file has been damaged!');
            return false;
        }}
        
        jjSTREAM level(jjLevelFileName);
        if (level.isEmpty()) {{
            jjDebug('MLLE::Setup: Error reading ""' + jjLevelFileName + '""!');
            return false;
        }}
        level.discard(230);
        array<uint> CompressedDataSizes(4, 0);
        for (uint i = 0; i < CompressedDataSizes.length; ++i) {{
            level.pop(CompressedDataSizes[i]);
            level.discard(4);
        }}
        for (uint i = 0; i < CompressedDataSizes.length; ++i)
            level.discard(CompressedDataSizes[i]);

        if (level.getSize() < 20) {{
            jjDebug('MLLE::Setup: Level file does not contain any additional data!');
            return false;
        }}
        string magic;
        level.get(magic, '" + MLLEData5MagicString + @"'.length);
        if (magic != '" + MLLEData5MagicString + @"') {{
            jjDebug('MLLE::Setup: Level was not saved by MLLE!');
            return false;
        }}
        uint levelDataVersion;
        level.pop(levelDataVersion);
        if (levelDataVersion != " + CurrentMLLEData5VersionStringForComparison + @") {{
            jjDebug('MLLE::Setup: Level\'s Data5 section was saved in a different version of MLLE than this script!');
            return false;
        }}

        uint csize, usize;
        level.pop(csize); level.pop(usize);
        jjSTREAM data5;
        if (!jjZlibUncompress(level, data5, usize)) {{
            jjDebug('MLLE::Setup: Error during ZLIB uncompression!');
            return false;
        }}

        bool pbool; uint8 pbyte; int8 pchar; float pfloat; int pint; uint puint, puint2;
        data5.pop(pbool); jjIsSnowing = pbool;
        data5.pop(pbool); jjIsSnowingOutdoorsOnly = pbool;
        data5.pop(pbyte); jjSnowingIntensity = pbyte;
        data5.pop(pbyte); jjSnowingType = SNOWING::Type(pbyte);

        if (jjIsSnowing) {{
            if (jjSnowingType == SNOWING::SNOW && jjAnimSets[ANIM::SNOW] == 0)
                jjAnimSets[ANIM::SNOW].load();
            else if (jjSnowingType == SNOWING::LEAF && jjAnimSets[ANIM::PLUS_SCENERY] == 0)
                jjAnimSets[ANIM::PLUS_SCENERY].load();
        }}

        data5.pop(pbool); jjWarpsTransmuteCoins = pbool;
        data5.pop(pbool); jjDelayGeneratedCrateOrigins = pbool;
        data5.pop(pint);  jjEcho = pint;
        data5.pop(puint); jjSetDarknessColor(_colorFromArgb(puint));
        data5.pop(pfloat);jjWaterChangeSpeed = pfloat;
        data5.pop(pbyte); jjWaterInteraction = WATERINTERACTION::WaterInteraction(pbyte);
        data5.pop(pint);  jjWaterLayer = pint;
        data5.pop(pbyte); jjWaterLighting = WATERLIGHT::wl(pbyte);
        data5.pop(pfloat); if (int(pfloat) < jjLayerHeight[4] * 32) jjSetWaterLevel(pfloat, true);
        data5.pop(puint); data5.pop(puint2); jjSetWaterGradient(_colorFromArgb(puint), _colorFromArgb(puint2));

        data5.pop(pbool); if (pbool) {{
            for (uint i = 0; i < 256; ++i) {{
                data5.pop(palette.color[i].red);
                data5.pop(palette.color[i].green);
                data5.pop(palette.color[i].blue);
            }}
            palette.apply();
        }}

        _recolorAnimationIf(data5, ANIM::PINBALL, 0, 4);
        _recolorAnimationIf(data5, ANIM::PINBALL, 2, 4);
        _recolorAnimationIf(data5, ANIM::CARROTPOLE, 0, 1);
        _recolorAnimationIf(data5, ANIM::DIAMPOLE, 0, 1);
        _recolorAnimationIf(data5, ANIM::PINBALL, 4, 8);
        _recolorAnimationIf(data5, ANIM::JUNGLEPOLE, 0, 1);
        _recolorAnimationIf(data5, ANIM::PLUS_SCENERY, 0, 17);
        _recolorAnimationIf(data5, ANIM::PSYCHPOLE, 0, 1);
        _recolorAnimationIf(data5, ANIM::SMALTREE, 0, 1);
        _recolorAnimationIf(data5, ANIM::SNOW, 0, 8);
        _recolorAnimationIf(data5, ANIM::COMMON, 2, 18);
        _recolorAnimationIf(data5, ANIM::BOLLPLAT, 0, 2);
        _recolorAnimationIf(data5, ANIM::FRUITPLAT, 0, 2);
        _recolorAnimationIf(data5, ANIM::GRASSPLAT, 0, 2);
        _recolorAnimationIf(data5, ANIM::PINKPLAT, 0, 2);
        _recolorAnimationIf(data5, ANIM::SONICPLAT, 0, 2);
        _recolorAnimationIf(data5, ANIM::SPIKEPLAT, 0, 2);
        _recolorAnimationIf(data5, ANIM::SPIKEBOLL, 0, 2);
        _recolorAnimationIf(data5, ANIM::SPIKEBOLL3D, 0, 2);
        _recolorAnimationIf(data5, ANIM::VINE, 1, 1);

        data5.pop(pbyte);
        for (uint i = 0; i < pbyte; ++i) {{
            string tilesetFilename = _read7BitEncodedStringFromStream(data5);
            uint16 tileStart, tileCount;
            data5.pop(tileStart); data5.pop(tileCount);
            array<uint8>@ colors = null;
            data5.pop(pbool); if (pbool) {{
                @colors = array<uint8>(256);
                for (uint j = 0; j < 256; ++j)
                    data5.pop(colors[j]);
            }}
            if (!jjTilesFromTileset(tilesetFilename, tileStart, tileCount, colors)) {{
                jjDebug('MLLE::Setup: Error reading ""' + tilesetFilename + '""!');
                return false;
            }}
        }}
        if (pbyte != 0) {{
            array<uint> layersIDsWithTileMaps;
            for (uint i = 1; i <= 8; ++i)
                if (jjLayers[i].hasTileMap)
                    layersIDsWithTileMaps.insertLast(i);
            if (jjLayersFromLevel(jjLevelFileName, layersIDsWithTileMaps).length == 0) {{
                jjDebug('MLLE::Setup: Error reading ""' + jjLevelFileName + '""!');
            }}
        }}

        array<jjLAYER@> newLayerOrder, nonDefaultLayers;
        data5.pop(puint);
        for (uint i = 8; i < puint; i += 8) {{
            array<uint> layerIDsToGrab;
            for (uint j = i; j < puint && j < i + 8; ++j) {{
                layerIDsToGrab.insertLast((j & 7) + 1);
            }}
            const string extraLayersFilename = jjLevelFileName.substr(0, jjLevelFileName.length() - 4) + '-MLLE-Data-' + (i/8) + '.j2l';
            array<jjLAYER@> extraLayers = jjLayersFromLevel(extraLayersFilename, layerIDsToGrab);
            if (extraLayers.length == 0) {{
                jjDebug('MLLE::Setup: Error reading ""' + extraLayersFilename + '""!');
                return false;
            }}
            for (uint j = 0; j < extraLayers.length(); ++j)
                nonDefaultLayers.insertLast(extraLayers[j]);
        }}
        uint nextNonDefaultLayerID = 0;
        for (uint i = 0; i < puint; ++i) {{
            data5.pop(pchar);
            jjLAYER@ layer;
            if (pchar >= 0)
                @layer = jjLayers[pchar + 1];
            else
                @layer = nonDefaultLayers[nextNonDefaultLayerID++];
            string layerName = _read7BitEncodedStringFromStream(data5);
            _layers.set(layerName, @layer);
            data5.pop(pbool);
            if (layer.hasTileMap)
                layer.hasTiles = !pbool;
            data5.pop(pbyte);
            layer.spriteMode = SPRITE::Mode(pbyte);
            data5.pop(pbyte);
            layer.spriteParam = pbyte;
            data5.pop(pint);
            layer.rotationAngle = pint;
            data5.pop(pint);
            layer.rotationRadiusMultiplier = pint;
            newLayerOrder.insertLast(layer);
        }}
        jjLayerOrderSet(newLayerOrder);

        uint16 numberOfImages; data5.pop(numberOfImages);
        for (uint16 i = 0; i < numberOfImages; ++i) {{
            uint16 tileID; data5.pop(tileID);
            jjPIXELMAP tile(32, 32);
            for (int y = 0; y < 32; ++y)
                for (int x = 0; x < 32; ++x)
                    data5.pop(tile[x,y]);
            tile.save(tileID, true);
        }}
        data5.pop(numberOfImages);
        for (uint16 i = 0; i < numberOfImages; ++i) {{
            uint16 tileID; data5.pop(tileID);
            jjMASKMAP tile;
            for (int y = 0; y < 32; ++y)
                for (int x = 0; x < 32; ++x)
                    data5.pop(tile[x,y]);
            tile.save(tileID, true);
        }}

        for (uint i = 1; i <= 9; ++i) {{
            jjWEAPON@ weapon = jjWeapons[i];
            data5.pop(pbool);
            data5.pop(weapon.maximum);
            data5.pop(pbyte); weapon.comesFromBirds = pbyte != 0; weapon.comesFromBirdsPowerup = pbyte == 2;
            data5.pop(weapon.comesFromGunCrates);
            data5.pop(pbyte); weapon.gemsLost = pbyte;
            data5.pop(pbyte); weapon.gemsLostPowerup = pbyte;
            uint8 ammoCrateEventID = 0;
            if (i >= 7) {{
                data5.pop(ammoCrateEventID);
                if (ammoCrateEventID > 32) {{
                    jjOBJ@ preset = jjObjectPresets[ammoCrateEventID];
                    preset.behavior = function(obj) {{ if (obj.state == STATE::DEACTIVATE) obj.eventID = obj.counterEnd; obj.behave(BEHAVIOR::AMMO15); }};
                    preset.playerHandling = HANDLING::SPECIAL;
                    preset.scriptedCollisions = false;
                    preset.direction = 1;
                    preset.energy = 1;
                    preset.curFrame = jjAnimations[preset.curAnim = (i == 7) ? (jjAnimSets[ANIM::PICKUPS] + 59) : (jjAnimSets[ANIM::PLUS_COMMON] + i - 8)] + (preset.frameID = 0);
                    preset.killAnim = jjAnimSets[ANIM::AMMO] + 71;
                    preset.eventID = OBJECT::ICEAMMO15;
                    preset.counterEnd = ammoCrateEventID;
                    preset.var[2] = 31 + i;
                    preset.var[3] = i - 1;
                    preset.points = 300;
                }}
            }}
            {3}if (i == 8) {{
                data5.pop(pbyte);
                if (pbyte == 0)
                    weapon.spread = SPREAD::GUN8;
                else if (pbyte == 1)
                    weapon.spread = SPREAD::PEPPERSPRAY;
                else if (pbyte >= 2)
                    weapon.spread = SPREAD::NORMAL;
                if (pbyte == 2)
                    weapon.gradualAim = false;
            }}
        }}

        if (!data5.isEmpty()) {{
            jjDebug('MLLE::Setup: Warning, Data5 longer than expected');
        }}
        
        return true;
    }}

    jjLAYER@ GetLayer(const string &in name) {{
        jjLAYER@ handle = null;
        _layers.get(name, @handle);
        return handle;
    }}

    jjPALCOLOR _colorFromArgb(uint Argb) {{
        return jjPALCOLOR(Argb >> 16, Argb >> 8, Argb >> 0);
    }}

    uint _read7BitEncodedUintFromStream(jjSTREAM@ stream) {{
        uint result = 0;
        while (true) {{
            uint8 byteRead; stream.pop(byteRead);
            result |= (byteRead & 0x7F);
            if (byteRead >= 0x80)
                result <<= 7;
            else
                break;
        }}
        return result;
    }}
    string _read7BitEncodedStringFromStream(jjSTREAM@ stream) {{
        string result;
        stream.get(result, _read7BitEncodedUintFromStream(stream));
        return result;
    }}

    void _recolorAnimationIf(jjSTREAM@ stream, ANIM::Set set, uint animID, uint frameCount) {{
        bool pbool; stream.pop(pbool); if (!pbool) return;

        if (jjAnimSets[set] == 0)
            jjAnimSets[set].load();
        const uint firstFrameID = jjAnimations[jjAnimSets[set] + animID];
        array<uint8> colors(256);
        for (uint i = 0; i < 256; ++i)
            stream.pop(colors[i]);
        for (uint i = 0; i < frameCount; ++i) {{
            jjANIMFRAME@ frame = jjAnimFrames[firstFrameID + i];
            jjPIXELMAP image(frame);
            for (uint x = 0; x < image.width; ++x)
                for (uint y = 0; y < image.height; ++y)
                    image[x,y] = colors[image[x,y]];
            image.save(frame);
        }}
    }}
}}{4}";
        const string AngelscriptLibraryWeaponsPortion1 = @"
    se::WeaponHook@ WeaponHook;";
        const string AngelscriptLibraryWeaponsPortion2 = "array<MLLEWeaponApply@> w";
        const string AngelscriptLibraryWeaponsPortion3 = @"if (pbool) {
                if (WeaponHook is null)
                    @WeaponHook = se::DefaultWeaponHook();
                _read7BitEncodedStringFromStream(data5);
                jjSTREAM param;
                data5.pop(param);
                w[i-1].Apply(i, WeaponHook, param, ammoCrateEventID);
            } else ";
        const string AngelscriptLibraryWeaponsPortion4 = @"

#include 'SEweapon.asc'
#pragma require 'SEweapon.asc'
shared interface MLLEWeaponApply { bool Apply(uint, se::WeaponHook@ = null, jjSTREAM@ = null, uint8 = 0); }";

        static readonly string TagForProgrammaticallyAddedLines = " ///@MLLE-Generated\r\n";
        static string GetPragmaInclude(string filename)
        {
            return "#include \"" + filename + "\"";
        }
        static string GetPragmaRequire(string filename)
        {
            return "#pragma require \"" + filename + "\"";
        }
        public static string GetExtraDataLevelFilepath(string filepath, int index)
        {
            return Path.Combine(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath) + "-MLLE-Data-" + (index + 1) + ".j2l");
        }

        internal void SaveLibrary(string filepath, List<J2TFile> Tilesets, int numberOfExtraDataLevels, WeaponsForm.ExtendedWeapon[] customWeapons)
        {
            var encoding = J2LFile.FileEncoding;
            List<string> RequiredFilenames = new List<string>();
            bool weaponLibrary = false;
            foreach (var cw in customWeapons)
                if (cw != null) {
                    RequiredFilenames.Add(cw.LibraryFilename);
                    weaponLibrary = true;
                }

            string AngelscriptLibraryFilename = "MLLE-Include-" + CurrentMLLEData5VersionString + (!weaponLibrary ? "" : "w") + ".asc";
            using (BinaryWriter binwriter = new BinaryWriter(File.Open(Path.Combine(Path.GetDirectoryName(filepath), AngelscriptLibraryFilename), FileMode.Create, FileAccess.Write), encoding)) {
                binwriter.Write(encoding.GetBytes("//"));
                var libraryFileAsBytes = encoding.GetBytes(string.Format(
                    AngelscriptLibrary,
                    AngelscriptLibraryFilename,
                    !weaponLibrary ? "" : AngelscriptLibraryWeaponsPortion1,
                    !weaponLibrary ? "" : AngelscriptLibraryWeaponsPortion2,
                    !weaponLibrary ? "" : AngelscriptLibraryWeaponsPortion3,
                    !weaponLibrary ? "" : AngelscriptLibraryWeaponsPortion4
                ));
                CRC32 CRCCalculator = new CRC32();
                CRCCalculator.SlurpBlock(libraryFileAsBytes, 0, libraryFileAsBytes.Length);
                binwriter.Write(encoding.GetBytes(((uint)CRCCalculator.Crc32Result).ToString() + "\r\n"));
                binwriter.Write(libraryFileAsBytes);
            }

            string scriptFilepath = Path.ChangeExtension(filepath, ".j2as");
            string fileContents = "";
            if (File.Exists(scriptFilepath))
                fileContents = System.IO.File.ReadAllText(scriptFilepath, encoding);
            RequiredFilenames.Add(Path.GetFileName(filepath));
            for (int i = 1; i < Tilesets.Count; ++i)
                RequiredFilenames.Add(Tilesets[i].FilenameOnly);
            int extraDataLevelID = 0;
            for (extraDataLevelID = 0; extraDataLevelID < numberOfExtraDataLevels; ++extraDataLevelID)
                RequiredFilenames.Add(Path.GetFileName(GetExtraDataLevelFilepath(filepath, extraDataLevelID)));
            foreach (string fn in RequiredFilenames)
            {
                string pragma = GetPragmaRequire(fn);
                if (!fileContents.Contains(pragma))
                    fileContents = pragma + TagForProgrammaticallyAddedLines + fileContents;
                if (fn.EndsWith("asc", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    pragma = GetPragmaInclude(fn);
                    if (!fileContents.Contains(pragma))
                        fileContents = pragma + TagForProgrammaticallyAddedLines + fileContents;
                }
            }
            while (true) //remove extra such pragmas/files if the number of layers has decreased since the last time this level was saved
            {
                string extraFilepath = GetExtraDataLevelFilepath(filepath, extraDataLevelID++);
                File.Delete(extraFilepath);
                string pragma = GetPragmaRequire(Path.GetFileName(extraFilepath));
                if (fileContents.Contains(pragma))
                    fileContents = new Regex("^[^\\n]*" + pragma + "\\s*?\\r?\\n?", RegexOptions.Multiline).Replace(fileContents, "");
                else
                    break;
            }
            fileContents = "#include \"" + AngelscriptLibraryFilename + "\"" + TagForProgrammaticallyAddedLines + fileContents;
            Regex setupPattern = new Regex(@"MLLE\s*::\s*Setup\s*\([^;]*\)\s*;");
            string desiredSetupCall = "MLLE::Setup(";
            if (weaponLibrary)
            {
                desiredSetupCall += "array<MLLEWeaponApply@> = {";
                for (int i = 0; i < 9; ++i)
                {
                    WeaponsForm.ExtendedWeapon cw = customWeapons[i];
                    if (cw != null)
                        desiredSetupCall += cw.Initialization;
                    else
                        desiredSetupCall += "null";
                    if (i < 8)
                        desiredSetupCall += ", ";
                }
                desiredSetupCall += "}";
            }
            desiredSetupCall += ");";
            Match match = setupPattern.Match(fileContents);
            if (!match.Success)
                fileContents = "const bool MLLESetupSuccessful = " + desiredSetupCall + TagForProgrammaticallyAddedLines + fileContents;
            else if (match.Value != desiredSetupCall)
                fileContents = setupPattern.Replace(fileContents, desiredSetupCall);

            bool[] hooksNeeded = WeaponHookSpecs.Select(ss => weaponLibrary && customWeapons.Any(cw => cw != null && new Regex("\\b" + ss[1] + "\\b", RegexOptions.IgnoreCase).Match(cw.Hooks).Success)).ToArray();
            hooksNeeded[3] = weaponLibrary; //onDrawAmmo is used by ALL custom weapons
            
            for (int specID = 0; specID < WeaponHookSpecs.Length; ++specID)
            {
                string[] spec = WeaponHookSpecs[specID];

                Regex weaponHookCallFindRegex = new Regex(@"(return\s+)?MLLE\s*::\s*WeaponHook\s*\.\s*" + spec[2] + @"\s*\([^;]*\)(\s*;)?");

                string functionPattern = "(" + spec[0] + @"\s+" + spec[1] + @"\s*\(\s*";
                for (int paramStringID = 3; paramStringID < spec.Length; paramStringID += 2)
                {
                    functionPattern += spec[paramStringID].Replace(" ", @"\s*") + @"\s*(\S+)\s*";
                    if (paramStringID + 2 < spec.Length)
                        functionPattern += @",\s*";
                }
                functionPattern += @"\)[^{]*{)"; //e.g. a pattern to match "void onMain() {"

                if (hooksNeeded[specID])
                {
                    if (!weaponHookCallFindRegex.Match(fileContents).Success) //weaponhook call not being made
                    {
                        string weaponhookMethodCall = "\r\n\t" + (spec[0] == "void" ? "" : "return ") + "MLLE::WeaponHook." + spec[2] + "(";
                        for (int paramStringID = 3; paramStringID < spec.Length; paramStringID += 2)
                        {
                            weaponhookMethodCall += "$" + ((paramStringID + 1) / 2).ToString();
                            if (paramStringID + 2 < spec.Length)
                                weaponhookMethodCall += ", ";
                        }
                        weaponhookMethodCall += ");"; //e.g. "WeaponHook::processMain();"

                        Regex functionRegex = new Regex(functionPattern);
                        match = functionRegex.Match(fileContents);
                        if (match.Success) //hook function already exists
                        {
                            fileContents = functionRegex.Replace(fileContents, "$1" + weaponhookMethodCall);
                        }
                        else
                        { //add everything from scratch
                            string functionToAdd = "\r\n" + spec[0] + " " + spec[1] + "(";
                            for (int paramStringID = 3; paramStringID < spec.Length; paramStringID += 2)
                            {
                                functionToAdd += spec[paramStringID].Replace(" ", "") + " " + spec[paramStringID + 1];
                                weaponhookMethodCall = weaponhookMethodCall.Replace("$" + ((paramStringID + 1) / 2).ToString(), spec[paramStringID + 1]);
                                if (paramStringID + 2 < spec.Length)
                                    functionToAdd += ", ";
                            }
                            functionToAdd += ") {" + weaponhookMethodCall + "\r\n}\r\n";

                            fileContents += functionToAdd;
                        }
                    }
                }
                else //hook NOT needed
                {
                    fileContents = new Regex(functionPattern + @"[\s;]*}\r?\n?").Replace(weaponHookCallFindRegex.Replace(fileContents, ""), ""); //remove the method call, then remove the hook function it was in if that hook is now totally empty
                }
            }
            System.IO.File.WriteAllText(scriptFilepath, fileContents, encoding);
        }
        static readonly string[][] WeaponHookSpecs = {
            new string[]{"void", "onMain", "processMain"},
            new string[]{"void", "onPlayer", "processPlayer", "jjPLAYER @", "player"},
            new string[]{"void", "onPlayerInput", "processPlayerInput", "jjPLAYER @", "player"},
            new string[]{"bool", "onDrawAmmo", "drawAmmo", "jjPLAYER @", "player", "jjCANVAS @", "canvas"},
            new string[]{"void", "onReceive", "processPacket", "jjSTREAM & in", "packet", "int", "fromClientID" }
        };

        public static void RemovePriorReferencesToMLLELibrary(string filepath)
        {
            string scriptFilepath = Path.ChangeExtension(filepath, ".j2as");
            if (File.Exists(scriptFilepath))
            {
                var encoding = J2LFile.FileEncoding;
                string fileContents = System.IO.File.ReadAllText(scriptFilepath, encoding);
                fileContents = new Regex(
                    "^[^\\n]*(" +
                        "///@MLLE-Generated" + //get rid of all lines that end in the "///@MLLE-Generated" tag
                        "|" +
                        "#include\\s+['\"]MLLE-Include-\\d+\\.\\d+\\.asc['\"]" + //get rid of existing #include calls to MLLE-Include, especially if they referenced older/newer versions of the file
                    ")[^\\n]*\\r?\\n?", RegexOptions.Multiline).Replace(fileContents, "");
                if (fileContents.Length > 0)
                    System.IO.File.WriteAllText(scriptFilepath, fileContents, encoding);
                else
                    File.Delete(scriptFilepath);
            }
        }
    }
}
