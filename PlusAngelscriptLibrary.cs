using System.IO;
using Ionic.Zlib;

namespace MLLE
{
    public partial struct PlusPropertyList
    {
        const uint CurrentMLLEData5Version = 0x100;
        const string MLLEData5MagicString = "MLLE";
        const string CurrentMLLEData5VersionStringForComparison = "0x100";
        const string CurrentMLLEData5VersionString = "1.0";
        const string AngelscriptLibraryFilename = "MLLE-Include-" + CurrentMLLEData5VersionString + ".asc";
        const string IncludeAngelscriptLibrary = "#pragma include " + AngelscriptLibraryFilename;

        const string AngelscriptLibrary = 
@"//This is a standard library created by MLLE to read some JJ2+ properties from a level file whose script includes this library. DO NOT MANUALLY MODIFY THIS FILE.





#pragma require '" + AngelscriptLibraryFilename + @"'
namespace MLLE {
    bool Setup() {
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
        data5.pop(pbool); jjWarpsTransmuteCoins = pbool;
        data5.pop(pbool); jjDelayGeneratedCrateOrigins = pbool;
        data5.pop(pint);  jjEcho = pint;
        data5.pop(puint); jjSetDarknessColor(_colorFromArgb(puint));
        data5.pop(pfloat);jjWaterChangeSpeed = pfloat;
        data5.pop(pbyte); jjWaterInteraction = WATERINTERACTION::WaterInteraction(pbyte);
        data5.pop(pint);  jjWaterLayer = pint;
        data5.pop(pbyte); jjWaterLighting = WATERLIGHT::wl(pbyte);
        data5.pop(pfloat);jjSetWaterLevel(pfloat, true);
        data5.pop(puint); data5.pop(puint2); jjSetWaterGradient(_colorFromArgb(puint), _colorFromArgb(puint2));

        if (!data5.isEmpty()) {
            jjDebug('MLLE::Setup: Warning, Data5 longer than expected');
        }
        
        return true;
    }

    jjPALCOLOR _colorFromArgb(uint Argb) {
        return jjPALCOLOR(Argb >> 16, Argb >> 8, Argb >> 0);
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
        }
    }
}
