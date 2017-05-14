using System.IO;
using Ionic.Zlib;
using System.Text.RegularExpressions;

namespace MLLE
{
    public partial struct PlusPropertyList
    {
        const uint CurrentMLLEData5Version = 0x100;
        const string MLLEData5MagicString = "MLLE";
        const string CurrentMLLEData5VersionStringForComparison = "0x100";
        const string CurrentMLLEData5VersionString = "1.0";
        const string AngelscriptLibraryFilename = "MLLE-Include-" + CurrentMLLEData5VersionString + ".asc";

        const string AngelscriptLibraryCallStockLine = "const bool MLLESetupSuccessful = MLLE::Setup();\r\n";

        const string AngelscriptLibrary = 
@"//This is a standard library created by MLLE to read some JJ2+ properties from a level file whose script includes this library. DO NOT MANUALLY MODIFY THIS FILE.





#pragma require '" + AngelscriptLibraryFilename + @"'
namespace MLLE {
    jjPAL@ Palette;

    bool Setup() {
        jjPAL palette;
        @Palette = @palette;

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
        if (levelDataVersion > " + CurrentMLLEData5VersionStringForComparison + @") {
            jjDebug('MLLE::Setup: Level\'s Data5 section was saved in a more recent version of MLLE than this script understands!');
            return false;
        }

        uint csize, usize;
        level.pop(csize); level.pop(usize);
        jjSTREAM data5;
        if (!jjZlibUncompress(level, data5, usize)) {
            jjDebug('MLLE::Setup: Error during ZLIB uncompression!');
            return false;
        }

        bool pbool; uint8 pbyte; float pfloat; int pint; uint puint, puint2;
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

        data5.pop(pbyte);
        for (uint i = 0; i < pbyte; ++i) {
            string tilesetFilename = _read7BitEncodedStringFromStream(data5);
            uint16 tileStart, tileCount;
            data5.pop(tileStart); data5.pop(tileCount);
            jjTilesFromTileset(tilesetFilename, tileStart, tileCount);
        }

        if (!data5.isEmpty()) {
            jjDebug('MLLE::Setup: Warning, Data5 longer than expected');
        }
        
        return true;
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
}";

        public void SaveLibrary(string filepath)
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
