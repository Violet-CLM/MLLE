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
        const uint CurrentMLLEData5Version = 0x106;
        const string MLLEData5MagicString = "MLLE";
        const string CurrentMLLEData5VersionStringForComparison = "0x106";
        const string CurrentMLLEData5VersionString = "1.6";

        const string AngelscriptLibrary =
@"//This is a standard library created by MLLE to read some JJ2+ properties from a level file whose script includes this library. DO NOT MANUALLY MODIFY THIS FILE.





#pragma require '{0}'
namespace MLLE {{
    jjPAL@ Palette;
    dictionary@ _layers, _palettes;{1}
    array<_offgridObject>@ _offGridObjects;

    bool Setup({2}) {{
        jjPAL palette = jjBackupPalette;
        @Palette = @palette;
        dictionary layers;
        @_layers = @layers;
        dictionary palettes;
        @_palettes = @palettes;

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

        bool pbool; uint8 pbyte; int8 pchar; int16 pshort; float pfloat; int pint; uint puint, puint2;
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
            _readPalette(data5, palette);
            palette.apply();
            data5.pop(pbool);
        }}

        data5.pop(pbyte);
        while (pbyte-- != 0) {{
            jjPAL extra;
            string paletteName = _read7BitEncodedStringFromStream(data5);
            _readPalette(data5, extra);
            int index = jjSpriteModeFirstFreeMapping();
            if (index < 0) {{
                jjDebug('MLLE::Setup: Not enough room for additional palette ' + paletteName);
            }} else {{
                _palettes.set(paletteName, uint8(index));
                array<uint8> indexMapping(256);
                for (uint i = 0; i < 256; ++i)
                    indexMapping[i] = jjPalette.findNearestColor(extra.color[i]);
                jjSpriteModeSetMapping(index, indexMapping, extra);
            }}
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

        uint16 numberOfObjects; data5.pop(numberOfObjects);
        while (numberOfObjects-- != 0) {{
            uint16 tileID; data5.pop(tileID);
            jjPIXELMAP tile(32, 32);
            for (int y = 0; y < 32; ++y)
                for (int x = 0; x < 32; ++x)
                    data5.pop(tile[x,y]);
            tile.save(tileID, true);
        }}
        data5.pop(numberOfObjects);
        while (numberOfObjects-- != 0) {{
            uint16 tileID; data5.pop(tileID);
            jjMASKMAP tile;
            for (int y = 0; y < 32; ++y)
                for (int x = 0; x < 32; ++x)
                    data5.pop(tile[x,y]);
            tile.save(tileID, true);
        }}

        data5.pop(pshort);
        for (uint i = 1; i <= 9; ++i) {{
            jjWEAPON@ weapon = jjWeapons[i];
            data5.pop(pbool);
            data5.pop(pint); weapon.maximum = pint;
            data5.pop(pbyte); weapon.comesFromBirds = pbyte != 0; weapon.comesFromBirdsPowerup = pbyte == 2;
            data5.pop(pbyte); weapon.comesFromGunCrates = pbyte != 0;
            data5.pop(pbyte); weapon.gemsLost = pbyte;
            data5.pop(pbyte); weapon.gemsLostPowerup = pbyte;
            data5.pop(pbyte); weapon.infinite = pbyte & 1 == 1; weapon.replenishes = pbyte & 2 == 2;
            uint8 ammoCrateEventID = 0;
            if (i >= 7) {{
                data5.pop(ammoCrateEventID);
                if (ammoCrateEventID > 32) {{
                    jjOBJ@ preset = jjObjectPresets[ammoCrateEventID];
                    preset.behavior = AmmoCrate(ammoCrateEventID);
                    preset.playerHandling = HANDLING::SPECIAL;
                    preset.scriptedCollisions = false;
                    preset.direction = 1;
                    preset.energy = 1;
                    preset.curFrame = jjAnimations[preset.curAnim = (i == 7) ? (jjAnimSets[ANIM::PICKUPS] + 59) : (jjAnimSets[ANIM::PLUS_COMMON] + i - 8)] + (preset.frameID = 0);
                    preset.killAnim = jjAnimSets[ANIM::AMMO] + 71;
                    preset.eventID = OBJECT::ICEAMMO15;
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

        data5.pop(numberOfObjects);
        if (numberOfObjects != 0) {{
            if (jjGameConnection != GAME::LOCAL)
                jjObjectPresets[254].behavior = _replaceMe;
            else
                jjObjectPresets[254].behavior = BEHAVIOR::INACTIVE;
            @_offGridObjects = array<_offgridObject>();
            array<uint8> animSetsToLoad(256, 0);
            animSetsToLoad[OBJECT::BONUSPOST] = ANIM::BONUS;
            animSetsToLoad[OBJECT::SWINGINGVINE] = ANIM::VINE;
            animSetsToLoad[OBJECT::TUFTURT] = ANIM::TUFTUR;
            animSetsToLoad[OBJECT::LABRAT] = ANIM::LABRAT;
            animSetsToLoad[OBJECT::LIZARD] = ANIM::LIZARD;
            animSetsToLoad[OBJECT::FLOATLIZARD] = ANIM::LIZARD;
            animSetsToLoad[OBJECT::SUCKER] = ANIM::SUCKER;
            animSetsToLoad[OBJECT::CATERPILLAR] = ANIM::CATERPIL;
            animSetsToLoad[OBJECT::SMOKERING] = ANIM::CATERPIL;
            animSetsToLoad[OBJECT::CHESHIRE1] = ANIM::CAT;
            animSetsToLoad[OBJECT::CHESHIRE2] = ANIM::CAT2;
            animSetsToLoad[OBJECT::HATTER] = ANIM::HATTER;
            animSetsToLoad[OBJECT::SKELETON] = ANIM::SKELETON;
            animSetsToLoad[OBJECT::DOGGYDOGG] = ANIM::DOG;
            animSetsToLoad[OBJECT::NORMTURTLE] = ANIM::TURTLE;
            animSetsToLoad[OBJECT::TURTLESHELL] = ANIM::TURTLE;
            animSetsToLoad[OBJECT::DEMON] = ANIM::DEMON;
            animSetsToLoad[OBJECT::STEAM] = ANIM::STEAM;
            animSetsToLoad[OBJECT::ROTATINGROCK] = ANIM::ROCK;
            animSetsToLoad[OBJECT::HELMUT] = ANIM::HELMUT;
            animSetsToLoad[OBJECT::BILSY] = ANIM::BILSBOSS;
            animSetsToLoad[OBJECT::BAT] = ANIM::BAT;
            animSetsToLoad[OBJECT::BEE] = ANIM::BUMBEE;
            animSetsToLoad[OBJECT::DRAGONFLY] = ANIM::DRAGFLY;
            animSetsToLoad[OBJECT::FATCHICK] = ANIM::FATCHK;
            animSetsToLoad[OBJECT::FENCER] = ANIM::FENCER;
            animSetsToLoad[OBJECT::FISH] = ANIM::FISH;
            animSetsToLoad[OBJECT::MOTH] = ANIM::MOTH;
            animSetsToLoad[OBJECT::RAPIER] = ANIM::RAPIER;
            animSetsToLoad[OBJECT::SPARK] = ANIM::SPARK;
            animSetsToLoad[OBJECT::LEFTPADDLE] = ANIM::PINBALL;
            animSetsToLoad[OBJECT::RIGHTPADDLE] = ANIM::PINBALL;
            animSetsToLoad[OBJECT::FIVEHUNDREDBUMP] = ANIM::PINBALL;
            animSetsToLoad[OBJECT::CARROTBUMP] = ANIM::PINBALL;
            animSetsToLoad[OBJECT::QUEEN] = ANIM::QUEEN;
            animSetsToLoad[OBJECT::FLOATSUCKER] = ANIM::SUCKER;
            animSetsToLoad[OBJECT::BRIDGE] = ANIM::BRIDGE;
            animSetsToLoad[OBJECT::MONKEY] = ANIM::MONKEY;
            animSetsToLoad[OBJECT::STANDMONKEY] = ANIM::MONKEY;
            animSetsToLoad[OBJECT::RAVEN] = ANIM::RAVEN;
            animSetsToLoad[OBJECT::TUBETURTLE] = ANIM::TUBETURT;
            animSetsToLoad[OBJECT::SMALLTREE] = ANIM::SMALTREE;
            animSetsToLoad[OBJECT::DIAMONDUSPOLE] = ANIM::DIAMPOLE;
            animSetsToLoad[OBJECT::PSYCHPOLE] = ANIM::PSYCHPOLE;
            animSetsToLoad[OBJECT::CARROTUSPOLE] = ANIM::CARROTPOLE;
            animSetsToLoad[OBJECT::JUNGLEPOLE] = ANIM::JUNGLEPOLE;
            animSetsToLoad[OBJECT::UTERUS] = ANIM::UTERUS;
            animSetsToLoad[OBJECT::UTERUSSPIKEBALL] = ANIM::UTERUS;
            animSetsToLoad[OBJECT::CRAB] = ANIM::UTERUS;
            animSetsToLoad[OBJECT::ROBOT] = ANIM::ROBOT;
            animSetsToLoad[OBJECT::DEVANROBOT] = ANIM::DEVAN;
            animSetsToLoad[OBJECT::FRUITPLATFORM] = ANIM::FRUITPLAT;
            animSetsToLoad[OBJECT::BOLLPLATFORM] = ANIM::BOLLPLAT;
            animSetsToLoad[OBJECT::GRASSPLATFORM] = ANIM::GRASSPLAT;
            animSetsToLoad[OBJECT::PINKPLATFORM] = ANIM::PINKPLAT;
            animSetsToLoad[OBJECT::SONICPLATFORM] = ANIM::SONICPLAT;
            animSetsToLoad[OBJECT::SPIKEPLATFORM] = ANIM::SPIKEPLAT;
            animSetsToLoad[OBJECT::SPIKEBOLL] = ANIM::SPIKEBOLL;
            animSetsToLoad[OBJECT::SPIKEBOLL3D] = ANIM::SPIKEBOLL3D;
            animSetsToLoad[OBJECT::EVA] = ANIM::EVA;
            animSetsToLoad[OBJECT::WITCH] = ANIM::WITCH;
            animSetsToLoad[OBJECT::ROCKETTURTLE] = ANIM::ROCKTURT;
            animSetsToLoad[OBJECT::BUBBA] = ANIM::BUBBA;
            animSetsToLoad[OBJECT::DEVILDEVAN] = ANIM::DEVILDEVAN;
            animSetsToLoad[OBJECT::TUFBOSS] = ANIM::TUFBOSS;
            animSetsToLoad[OBJECT::BIGROCK] = ANIM::BIGROCK;
            animSetsToLoad[OBJECT::BIGBOX] = ANIM::BIGBOX;
            animSetsToLoad[OBJECT::BOLLY] = ANIM::SONCSHIP;
            animSetsToLoad[OBJECT::BUTTERFLY] = ANIM::BUTTERFLY;
            animSetsToLoad[OBJECT::BEEBOY] = ANIM::BEEBOY;
            animSetsToLoad[OBJECT::XMASNORMTURTLE] = ANIM::XTURTLE;
            animSetsToLoad[OBJECT::XMASLIZARD] = ANIM::XLIZARD;
            animSetsToLoad[OBJECT::XMASFLOATLIZARD] = ANIM::XLIZARD;
            animSetsToLoad[OBJECT::XMASBILSY] = ANIM::XBILSY;
            animSetsToLoad[OBJECT::CAT] = ANIM::ZDOG;
            animSetsToLoad[OBJECT::PACMANGHOST] = ANIM::ZSPARK;
            do {{
                uint16 xPos; data5.pop(xPos);
                uint16 yPos; data5.pop(yPos);
                int32 params; data5.pop(params);
                _offGridObjects.insertLast(_offgridObject(xPos, yPos, params));
                uint8 eventID = params;
                if (eventID == OBJECT::GENERATOR) eventID = params >> 12;
                if (animSetsToLoad[eventID] != 0) {{
                    jjOBJ@ preset = jjObjectPresets[eventID];
                    if (preset.curAnim < 100) {{
                        preset.curFrame = jjAnimations[preset.determineCurAnim(animSetsToLoad[eventID], preset.curAnim)] + preset.frameID;
                        if ((eventID >= OBJECT::FRUITPLATFORM && eventID <= OBJECT::SPIKEBOLL3D) || eventID == OBJECT::WITCH)
                            preset.killAnim += jjAnimSets[animSetsToLoad[eventID]];
                        else if (eventID == OBJECT::CATERPILLAR && jjObjectPresets[OBJECT::SMOKERING].curAnim < 100)
                            jjObjectPresets[OBJECT::SMOKERING].determineCurAnim(ANIM::CATERPIL, jjObjectPresets[OBJECT::SMOKERING].curAnim);
                    }}
                    animSetsToLoad[eventID] = 0;
                }}
            }} while (--numberOfObjects != 0);
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
    uint8 GetPaletteMappingID(const string &in name) {{
        uint8 mappingID;
        _palettes.get(name, mappingID);
        return mappingID;
    }}
    jjPAL@ GetPalette(const string &in name) {{
        if (name == 'Level Palette')
            return Palette;
        return jjSpriteModeGetColorMapping(GetPaletteMappingID(name));
    }}

    void ReapplyPalette() {{
        Palette.apply();
    }}

    class AmmoCrate : jjBEHAVIORINTERFACE {{
        uint8 realEventID;
        AmmoCrate(uint8 r) {{ realEventID = r; }}
        bool onIsSolid(jjOBJ@) {{ return true; }}
        void onBehave(jjOBJ@ obj) {{
            if (obj.state == STATE::DEACTIVATE)
                obj.eventID = realEventID;
            obj.behave(BEHAVIOR::AMMO15);
        }}
    }}

    void SpawnOffgrids() {{
        if (jjGameConnection == GAME::LOCAL) {{
            SpawnOffgridsLocal();
            for (int y = 0; y < jjLayerHeight[4]; ++y)
            for (int x = 0; x < jjLayerWidth[4]; ++x) {{
                const int ev = jjParameterGet(x,y, -12,32);
                if (ev == 0)
                    return;
                else if (ev == int(0xFFFFF3FE))
                    jjParameterSet(x,y, -12,32, 0);
            }}
        }}
    }}
    void SpawnOffgridsLocal() {{
        for (uint i = 0; i < _offGridObjects.length; ++i)
            _spawnOffgrid(i);
    }}
    class _offgridObject {{
        float xPos, yPos;
        int32 params;
        _offgridObject() {{}}
        _offgridObject(uint16 x, uint16 y, int32 p) {{ xPos = x; yPos = y; params = p; }}
    }}
    void _spawnOffgrid(uint i) {{
        const _offgridObject@ og = _offGridObjects[i];
        const int difficulty = og.params & 0x300;
        if (difficulty != 0 && (jjGameMode == GAME::SP || jjGameMode == GAME::COOP || jjGameConnection == GAME::LOCAL)) {{
            if (difficulty == 0x100) {{
                if (jjDifficulty > 0)
                    return;
            }} else if (difficulty == 0x200) {{
                if (jjDifficulty < 2)
                    return;
            }} else {{
                if (jjGameConnection == GAME::LOCAL && jjLocalPlayerCount == 1)
                    return;
            }}
        }}
        const uint xTile = uint(og.xPos) >> 5, yTile = uint(og.yPos) >> 5;
        const int realEvent = jjParameterGet(xTile,yTile, -12,32);
        jjParameterSet(xTile,yTile, -12,32, og.params);
        jjOBJ@ obj = jjObjects[jjAddObject(og.params, og.xPos, og.yPos, 0, CREATOR::LEVEL)];
        jjParameterSet(xTile,yTile, -12,32, realEvent);
        if (jjGameConnection == GAME::LOCAL) {{
            obj.deactivates = false;
            obj.creatorID = 1;
        }}
    }}
    uint _replaceMeIndex = 0;
    void _replaceMe(jjOBJ@ obj) {{
        jjParameterSet(uint(obj.xOrg)>>5, uint(obj.yOrg)>>5, -12,32, 0);
        obj.delete();
        _spawnOffgrid(_replaceMeIndex++);
    }}

    jjPALCOLOR _colorFromArgb(uint Argb) {{
        return jjPALCOLOR(Argb >> 16, Argb >> 8, Argb >> 0);
    }}

    void _readPalette(jjSTREAM& stream, jjPAL& palette) {{
        for (uint i = 0; i < 256; ++i) {{
            stream.pop(palette.color[i].red);
            stream.pop(palette.color[i].green);
            stream.pop(palette.color[i].blue);
        }}
    }}

    uint _read7BitEncodedUintFromStream(jjSTREAM& stream) {{
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
    string _read7BitEncodedStringFromStream(jjSTREAM& stream) {{
        string result;
        stream.get(result, _read7BitEncodedUintFromStream(stream));
        return result;
    }}

    void _recolorAnimationIf(jjSTREAM& stream, ANIM::Set set, uint animID, uint frameCount) {{
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
                    @WeaponHook = pshort < 0 ? se::DefaultWeaponHook() :  se::DefaultWeaponHook(pshort);
                _read7BitEncodedStringFromStream(data5);
                jjSTREAM param;
                data5.pop(param);
                w[i-1].Apply(i, WeaponHook, param, ammoCrateEventID);
            } else ";
        const string AngelscriptLibraryWeaponsPortion4 = @"

#include 'SEweapon.asc'
#pragma require 'SEweapon.asc'
shared interface MLLEWeaponApply { bool Apply(uint, se::WeaponHook@ = null, jjSTREAM@ = null, uint8 = 0); }";

        static readonly internal string TagForProgrammaticallyAddedLines = " ///@MLLE-Generated\r\n";
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

        internal void SaveLibrary(ref string fileContents, string filepath, List<J2TFile> Tilesets, int numberOfExtraDataLevels, WeaponsForm.ExtendedWeapon[] customWeapons)
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

            RequiredFilenames.Add(Path.ChangeExtension(Path.GetFileName(filepath), ".j2l"));
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

            bool[] hooksNeeded = IncludeHookSpecs.Select(ss => weaponLibrary && ss.WeaponhookMethod && customWeapons.Any(cw => cw != null && ss.WeaponIniHookIdentifier.Match(cw.Hooks).Success)).ToArray();
            hooksNeeded[3] = weaponLibrary; //onDrawAmmo is used by ALL custom weapons
            hooksNeeded[5] = (Palette != null) && ReapplyPalette;
            hooksNeeded[6] = hooksNeeded[7] = OffGridObjects.Count > 0;

            for (int specID = 0; specID < IncludeHookSpecs.Length; ++specID)
                IncludeHookSpecs[specID].Process(ref fileContents, hooksNeeded[specID]);
        }

        internal class Spec
        {
            readonly internal bool WeaponhookMethod;
            readonly bool LastCallFromThisHook;

            internal readonly Regex WeaponIniHookIdentifier;

            readonly string IncludeCall; //e.g. "MLLE::WeaponHook::processMain();"
            readonly string AddEntireHook; //e.g. "void onMain() { WeaponHook::processMain(); }" (but with newlines instead of some spaces)
            readonly Regex IncludeCallFind; //e.g. a pattern to match "MLLE::WeaponHook::processMain();"
            readonly Regex EnclosingFunctionStart; //e.g. a pattern to match "void onMain() {"
            readonly Regex EnclosingFunctionEmpty; //e.g. a pattern to match "void onMain() {}"

            internal Spec(string[] components, bool w = true, bool l = true)
            {
                WeaponhookMethod = w;
                LastCallFromThisHook = l;
                if (WeaponhookMethod)
                    WeaponIniHookIdentifier = new Regex("\\b" + components[1] + "\\b", RegexOptions.IgnoreCase);

                string returnType = components[0];
                string hookName = components[1];
                string includeName = components[2];
                string[] parameters = components.Skip(3).ToArray();

                IncludeCallFind = new Regex(@"(return\s+)?MLLE\s*::\s*" + (WeaponhookMethod ? @"WeaponHook\s*\.\s*" : "") + includeName + @"\s*\([^;]*\)(\s*;)?");

                IncludeCall = "\r\n\t" + (returnType == "void" ? "" : "return ") + "MLLE::" + (WeaponhookMethod ? "WeaponHook." : "") + includeName + "(";
                for (int paramStringID = 0; paramStringID < parameters.Length; paramStringID += 2)
                {
                    IncludeCall += "$" + (paramStringID / 2 + 2).ToString();
                    if (paramStringID + 2 < parameters.Length)
                        IncludeCall += ", ";
                }
                IncludeCall += ");";

                string includeCallWithDefaultParameterNames = IncludeCall;
                AddEntireHook = "\r\n" + returnType + " " + hookName + "(";
                for (int paramStringID = 0; paramStringID < parameters.Length; paramStringID += 2)
                {
                    AddEntireHook += parameters[paramStringID].Replace(" ", "") + " " + parameters[paramStringID + 1];
                    includeCallWithDefaultParameterNames = includeCallWithDefaultParameterNames.Replace("$" + (paramStringID / 2 + 2).ToString(), parameters[paramStringID + 1]);
                    if (paramStringID + 2 < parameters.Length)
                        AddEntireHook += ", ";
                }
                AddEntireHook += ") {" + includeCallWithDefaultParameterNames + "\r\n}\r\n";

                string functionPattern = "(" + returnType + @"\s+" + hookName + @"\s*\(\s*";
                for (int paramStringID = 0; paramStringID < parameters.Length; paramStringID += 2)
                {
                    functionPattern += parameters[paramStringID].Replace(" ", @"\s*") + @"\s*(\S+)\s*";
                    if (paramStringID + 2 < parameters.Length)
                        functionPattern += @",\s*";
                }
                functionPattern += @"\)[^{]*{)";
                EnclosingFunctionStart = new Regex(functionPattern);
                EnclosingFunctionEmpty = new Regex(functionPattern + @"[\s;]*}\r?\n?");
            }
            internal void Process(ref string fileContents, bool needed)
            {
                if (needed) {
                    if (!IncludeCallFind.Match(fileContents).Success) { //include library call not being made
                        if (EnclosingFunctionStart.Match(fileContents).Success) //hook function already exists
                            fileContents = EnclosingFunctionStart.Replace(fileContents, "$1" + IncludeCall);
                        else //add everything from scratch
                            fileContents += AddEntireHook;
                    } //else it's already being made, the script is fine already, don't change anything
                } else {
                    fileContents = IncludeCallFind.Replace(fileContents, ""); //remove the method call
                    if (LastCallFromThisHook)
                        fileContents = EnclosingFunctionEmpty.Replace(fileContents, ""); //then remove the hook function it was in if that hook is now totally empty
                }
            }
        }
        private static readonly Spec[] IncludeHookSpecs = {
            new Spec(new string[]{"void", "onMain", "processMain"}),
            new Spec(new string[]{"void", "onPlayer", "processPlayer", "jjPLAYER @", "player"}),
            new Spec(new string[]{"void", "onPlayerInput", "processPlayerInput", "jjPLAYER @", "player"}),
            new Spec(new string[]{"bool", "onDrawAmmo", "drawAmmo", "jjPLAYER @", "player", "jjCANVAS @", "canvas"}),
            new Spec(new string[]{"void", "onReceive", "processPacket", "jjSTREAM & in", "packet", "int", "fromClientID" }),
            new Spec(new string[]{"void", "onLevelReload", "ReapplyPalette"}, false, false),
            new Spec(new string[]{"void", "onLevelBegin", "SpawnOffgrids"}, false),
            new Spec(new string[]{"void", "onLevelReload", "SpawnOffgridsLocal"}, false)
        };

        public static void RemovePriorReferencesToMLLELibrary(ref string fileContents)
        {
            fileContents = new Regex(
                "^[^\\n]*(" +
                    "///@MLLE-Generated" + //get rid of all lines that end in the "///@MLLE-Generated" tag
                    "|" +
                    "///@SaveAndRunArgs" + //older versions before 2.18
                    "|" +
                    "#include\\s+['\"]MLLE-Include-\\d+\\.\\d+\\.asc['\"]" + //get rid of existing #include calls to MLLE-Include, especially if they referenced older/newer versions of the file
                ")[^\\n]*\\r?\\n?", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(fileContents, "");
        }
    }
}
