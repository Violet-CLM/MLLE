using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;
using System.Text.RegularExpressions;

namespace MLLE
{
    public partial struct PlusPropertyList
    {
        const uint CurrentMLLEData5Version = 0x103;
        const string MLLEData5MagicString = "MLLE";
        const string CurrentMLLEData5VersionStringForComparison = "0x103";
        const string CurrentMLLEData5VersionString = "1.3";
        const string AngelscriptLibraryFilename = "MLLE-Include-" + CurrentMLLEData5VersionString + ".asc";

        const string AngelscriptLibraryCallStockLine = "const bool MLLESetupSuccessful = MLLE::Setup();\r\n";

        const string AngelscriptLibrary = 
@"//This is a standard library created by MLLE to read some JJ2+ properties from a level file whose script includes this library. DO NOT MANUALLY MODIFY THIS FILE.





#pragma require '" + AngelscriptLibraryFilename + @"'
namespace MLLE {
    jjPAL@ Palette;
    dictionary@ _layers;

    bool Setup() {
        jjPAL palette = jjBackupPalette;
        @Palette = @palette;
        dictionary layers;
        @_layers = @layers;

        jjSTREAM crcCheck('" + AngelscriptLibraryFilename + @"');
        string crcLine;
        if (crcCheck.isEmpty() || !crcCheck.getLine(crcLine)) {
            jjDebug('MLLE::Setup: Include file has been renamed!');
            return false;
        }
        array<string> regexResults;
        if (!jjRegexMatch(crcLine, '\\/\\/(\\d+)\\r?', regexResults)) {
            jjDebug('MLLE::Setup: Include file is improperly formatted!');
            return false;
        }
        if (parseUInt(regexResults[1]) != jjCRC32(crcCheck)) {
            jjDebug('MLLE::Setup: Include file has been damaged!');
            return false;
        }
        
        jjSTREAM level(jjLevelFileName);
        level.discard(230);
        array<uint> CompressedDataSizes(4, 0);
        for (uint i = 0; i < CompressedDataSizes.length; ++i) {
            level.pop(CompressedDataSizes[i]);
            level.discard(4);
        }
        for (uint i = 0; i < CompressedDataSizes.length; ++i)
            level.discard(CompressedDataSizes[i]);

        if (level.getSize() < 20) {
            jjDebug('MLLE::Setup: Level file does not contain any additional data!');
            return false;
        }
        string magic;
        level.get(magic, '" + MLLEData5MagicString + @"'.length);
        if (magic != '" + MLLEData5MagicString + @"') {
            jjDebug('MLLE::Setup: Level was not saved by MLLE!');
            return false;
        }
        uint levelDataVersion;
        level.pop(levelDataVersion);
        if (levelDataVersion != " + CurrentMLLEData5VersionStringForComparison + @") {
            jjDebug('MLLE::Setup: Level\'s Data5 section was saved in a different version of MLLE than this script!');
            return false;
        }

        uint csize, usize;
        level.pop(csize); level.pop(usize);
        jjSTREAM data5;
        if (!jjZlibUncompress(level, data5, usize)) {
            jjDebug('MLLE::Setup: Error during ZLIB uncompression!');
            return false;
        }

        bool pbool; uint8 pbyte; int8 pchar; float pfloat; int pint; uint puint, puint2;
        data5.pop(pbool); jjIsSnowing = pbool;
        data5.pop(pbool); jjIsSnowingOutdoorsOnly = pbool;
        data5.pop(pbyte); jjSnowingIntensity = pbyte;
        data5.pop(pbyte); jjSnowingType = SNOWING::Type(pbyte);

        if (jjIsSnowing) {
            if (jjSnowingType == SNOWING::SNOW && jjAnimSets[ANIM::SNOW] == 0)
                jjAnimSets[ANIM::SNOW].load();
            else if (jjSnowingType == SNOWING::LEAF && jjAnimSets[ANIM::PLUS_SCENERY] == 0)
                jjAnimSets[ANIM::PLUS_SCENERY].load();
        }

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

        data5.pop(pbool); if (pbool) {
            for (uint i = 0; i < 256; ++i) {
                data5.pop(palette.color[i].red);
                data5.pop(palette.color[i].green);
                data5.pop(palette.color[i].blue);
            }
            palette.apply();
        }

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

        data5.pop(pbyte);
        for (uint i = 0; i < pbyte; ++i) {
            string tilesetFilename = _read7BitEncodedStringFromStream(data5);
            uint16 tileStart, tileCount;
            data5.pop(tileStart); data5.pop(tileCount);
            array<uint8>@ colors = null;
            data5.pop(pbool); if (pbool) {
                @colors = array<uint8>(256);
                for (uint j = 0; j < 256; ++j)
                    data5.pop(colors[j]);
            }
            jjTilesFromTileset(tilesetFilename, tileStart, tileCount, colors);
        }
        if (pbyte != 0) {
            array<uint> layersIDsWithTileMaps;
            for (uint i = 1; i <= 8; ++i)
                if (jjLayers[i].hasTileMap)
                    layersIDsWithTileMaps.insertLast(i);
            jjLayersFromLevel(jjLevelFileName, layersIDsWithTileMaps);
        }

        array<jjLAYER@> newLayerOrder, nonDefaultLayers;
        data5.pop(puint);
        for (uint i = 8; i < puint; i += 8) {
            array<uint> layerIDsToGrab;
            for (uint j = i; j < puint && j < i + 8; ++j) {
                layerIDsToGrab.insertLast((j & 7) + 1);
            }
            array<jjLAYER@> extraLayers = jjLayersFromLevel(jjLevelFileName.substr(0, jjLevelFileName.length() - 4) + '-MLLE-Data-' + (i/8) + '.j2l', layerIDsToGrab);
            for (uint j = 0; j < extraLayers.length(); ++j)
                nonDefaultLayers.insertLast(extraLayers[j]);
        }
        uint nextNonDefaultLayerID = 0;
        for (uint i = 0; i < puint; ++i) {
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
        }
        jjLayerOrderSet(newLayerOrder);

        uint16 numberOfImages; data5.pop(numberOfImages);
        for (uint16 i = 0; i < numberOfImages; ++i) {
            uint16 tileID; data5.pop(tileID);
            jjPIXELMAP tile(32, 32);
            for (int y = 0; y < 32; ++y)
                for (int x = 0; x < 32; ++x)
                    data5.pop(tile[x,y]);
            tile.save(tileID);
        }
        data5.pop(numberOfImages);
        for (uint16 i = 0; i < numberOfImages; ++i) {
            uint16 tileID; data5.pop(tileID);
            jjMASKMAP tile;
            for (int y = 0; y < 32; ++y)
                for (int x = 0; x < 32; ++x)
                    data5.pop(tile[x,y]);
            tile.save(tileID);
        }

        if (!data5.isEmpty()) {
            jjDebug('MLLE::Setup: Warning, Data5 longer than expected');
        }
        
        return true;
    }

    jjLAYER@ GetLayer(const string &in name) {
        jjLAYER@ handle = null;
        _layers.get(name, @handle);
        return handle;
    }

    jjPALCOLOR _colorFromArgb(uint Argb) {
        return jjPALCOLOR(Argb >> 16, Argb >> 8, Argb >> 0);
    }

    uint _read7BitEncodedUintFromStream(jjSTREAM@ stream) {
        uint result = 0;
        while (true) {
            uint8 byteRead; stream.pop(byteRead);
            result |= (byteRead & 0x7F);
            if (byteRead >= 0x80)
                result <<= 7;
            else
                break;
        }
        return result;
    }
    string _read7BitEncodedStringFromStream(jjSTREAM@ stream) {
        string result;
        stream.get(result, _read7BitEncodedUintFromStream(stream));
        return result;
    }

    void _recolorAnimationIf(jjSTREAM@ stream, ANIM::Set set, uint animID, uint frameCount) {
        bool pbool; stream.pop(pbool); if (!pbool) return;

        if (jjAnimSets[set] == 0)
            jjAnimSets[set].load();
        const uint firstFrameID = jjAnimations[jjAnimSets[set] + animID];
        array<uint8> colors(256);
        for (uint i = 0; i < 256; ++i)
            stream.pop(colors[i]);
        for (uint i = 0; i < frameCount; ++i) {
            jjANIMFRAME@ frame = jjAnimFrames[firstFrameID + i];
            jjPIXELMAP image(frame);
            for (uint x = 0; x < image.width; ++x)
                for (uint y = 0; y < image.height; ++y)
                    image[x,y] = colors[image[x,y]];
            image.save(frame);
        }
    }
}";

        static string GetPragmaRequire(string filename)
        {
            return "#pragma require \"" + filename + "\"\r\n";
        }
        public static string GetExtraDataLevelFilepath(string filepath, int index)
        {
            return Path.Combine(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath) + "-MLLE-Data-" + (index + 1) + ".j2l");
        }

        internal void SaveLibrary(string filepath, List<J2TFile> Tilesets, int numberOfExtraDataLevels)
        {
            var encoding = J2LFile.FileEncoding;
            using (BinaryWriter binwriter = new BinaryWriter(File.Open(Path.Combine(Path.GetDirectoryName(filepath), AngelscriptLibraryFilename), FileMode.Create, FileAccess.Write), encoding)) {
                binwriter.Write(encoding.GetBytes("//"));
                var libraryFileAsBytes = encoding.GetBytes(AngelscriptLibrary);
                CRC32 CRCCalculator = new CRC32();
                CRCCalculator.SlurpBlock(libraryFileAsBytes, 0, libraryFileAsBytes.Length);
                binwriter.Write(encoding.GetBytes(((uint)CRCCalculator.Crc32Result).ToString() + "\r\n"));
                binwriter.Write(libraryFileAsBytes);
            }

            string scriptFilepath = Path.ChangeExtension(filepath, ".j2as");
            string fileContents = "";
            if (File.Exists(scriptFilepath))
                fileContents = System.IO.File.ReadAllText(scriptFilepath, encoding);
            for (int i = 1; i < Tilesets.Count; ++i)
            {
                string pragma = GetPragmaRequire(Tilesets[i].FilenameOnly);
                if (!fileContents.Contains(pragma))
                    fileContents = pragma + fileContents;
            }
            int extraDataLevelID = 0;
            for (extraDataLevelID = 0; extraDataLevelID < numberOfExtraDataLevels; ++extraDataLevelID)
            {
                string pragma = GetPragmaRequire(Path.GetFileName(GetExtraDataLevelFilepath(filepath, extraDataLevelID)));
                if (!fileContents.Contains(pragma))
                    fileContents = pragma + fileContents;
            }
            while (true) //remove extra such pragmas/files if the number of layers has decreased since the last time this level was saved
            {
                string extraFilepath = GetExtraDataLevelFilepath(filepath, extraDataLevelID++);
                File.Delete(extraFilepath);
                string pragma = GetPragmaRequire(Path.GetFileName(extraFilepath));
                if (fileContents.Contains(pragma))
                    fileContents = fileContents.Replace(pragma, "");
                else
                    break;
            }
            if (!fileContents.Contains("MLLE::Setup()"))
                fileContents = AngelscriptLibraryCallStockLine + fileContents;
            System.IO.File.WriteAllText(scriptFilepath, "#include \"" + AngelscriptLibraryFilename + "\"\r\n" + fileContents, encoding);
        }

        public static void RemovePriorReferencesToMLLELibrary(string filepath)
        {
            string scriptFilepath = Path.ChangeExtension(filepath, ".j2as");
            if (File.Exists(scriptFilepath))
            {
                var encoding = J2LFile.FileEncoding;
                string fileContents = System.IO.File.ReadAllText(scriptFilepath, encoding);
                fileContents = fileContents.Replace(AngelscriptLibraryCallStockLine, ""); //get rid of the simpler old uses of MLLE::Setup(), though not all can be so painlessly removed
                fileContents = Regex.Replace(fileContents, "\\s*#include\\s+['\"]MLLE-Include-\\d+\\.\\d+\\.asc['\"]\\s*\\r?\\n?", ""); //get rid of existing #include calls to MLLE-Include, especially if they referenced older/newer versions of the file
                if (fileContents.Length > 0)
                    System.IO.File.WriteAllText(scriptFilepath, fileContents, encoding);
                else
                    File.Delete(scriptFilepath);
            }
        }
    }
}
