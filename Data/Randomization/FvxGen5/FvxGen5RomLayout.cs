using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// US ROM script/file offsets aligned with UPR-FVX <c>gen5_offsets.ini</c>.
    /// EU/JP editions may differ — extend with per-title-ID tables if needed.
    /// </summary>
    internal static class FvxGen5RomLayout
    {
        public readonly struct FileOffset
        {
            public readonly int File;
            public readonly int Offset;
            public FileOffset(int file, int offset)
            {
                File = file;
                Offset = offset;
            }
        }

        public readonly struct TradeScriptPatch
        {
            public readonly int File;
            public readonly int RequestedOffset;
            public readonly int GivenOffset;
            public TradeScriptPatch(int file, int req, int given)
            {
                File = file;
                RequestedOffset = req;
                GivenOffset = given;
            }
        }

        /// <summary>
        /// Bundles species halfword sites and optional level sites for one static encounter script cluster.
        /// Set <see cref="LevelSitesUseHalfword"/> to false only after verifying in the script editor that those offsets store a single-byte level.
        /// </summary>
        public sealed class StaticEncounterPatch
        {
            public readonly FileOffset[] SpeciesSites;
            public readonly FileOffset[] LevelSites;
            /// <summary>When true (default), level sites are halfwords; when false, single-byte level (clamp 1–255).</summary>
            public readonly bool LevelSitesUseHalfword;
            public StaticEncounterPatch(FileOffset[] speciesSites, FileOffset[] levelSites, bool levelSitesUseHalfword = true)
            {
                SpeciesSites = speciesSites;
                LevelSites = levelSites ?? System.Array.Empty<FileOffset>();
                LevelSitesUseHalfword = levelSitesUseHalfword;
            }
        }

        /// <summary>Optional story-text lines to rewrite when trade species change (US ROMs; extend per-language as needed).</summary>
        public readonly struct TradeStoryTextPatch
        {
            public readonly int TextFileId;
            public readonly int LineIndex;
            public TradeStoryTextPatch(int textFileId, int lineIndex)
            {
                TextFileId = textFileId;
                LineIndex = lineIndex;
            }
        }

        static FileOffset[] Sp(params int[] fileOffsetPairs)
        {
            var a = new FileOffset[fileOffsetPairs.Length / 2];
            for (int i = 0; i < a.Length; i++)
                a[i] = new FileOffset(fileOffsetPairs[i * 2], fileOffsetPairs[i * 2 + 1]);
            return a;
        }

        public static int BattleOverlayIndex(bool bw2) => bw2 ? 167 : 93;

        /// <summary>
        /// Byte offset of the type chart matrix in the battle overlay. Sourced from UPR <c>gen5_offsets.ini</c> (US B/W1 and B2/W2).
        /// The same table is used for European and Japanese retail builds in UPR for these games, but if a hack or build moves the chart,
        /// set the address in the Type Chart editor or fix the table here.
        /// </summary>
        public static int TypeChartOffsetInBattleOvl(bool bw2) => bw2 ? 0x3DC40 : 0x3A37C;

        public static int[] MainGameLegendaries(bool bw2) =>
            bw2 ? new[] { 638, 639, 640 } : new[] { 643, 644 };

        /// <summary>
        /// NARC list index for <c>a/2/0/2</c> (UPR <c>StarterGraphics</c>), same for BW1/BW2 US layouts.
        /// </summary>
        public const int StarterGraphicsNarcIndex = 202;

        /// <summary>
        /// Story-text line indices aligned with UPR Gen5Constants (US English).
        /// </summary>
        public readonly struct StarterPresentationRow
        {
            public readonly int StarterCryOvlNumber;
            public readonly string StarterCryTablePrefixHex;
            public readonly int StarterLocationStoryTextFileId;
            public readonly int PokedexGivenScriptFileId;
            public readonly int Bw1StarterTextMaxLine;
            public readonly int Bw1CherenText1Line;
            public readonly int Bw1CherenText2Line;
            public readonly int Bw2StarterTextMaxLine;
            public readonly int Bw2RivalTextLine;

            public StarterPresentationRow(
                int starterCryOvl,
                string cryPrefixHex,
                int storyFileId,
                int pokedexScriptFile,
                int bw1StarterMax,
                int bw1Cheren1,
                int bw1Cheren2,
                int bw2StarterMax,
                int bw2Rival)
            {
                StarterCryOvlNumber = starterCryOvl;
                StarterCryTablePrefixHex = cryPrefixHex;
                StarterLocationStoryTextFileId = storyFileId;
                PokedexGivenScriptFileId = pokedexScriptFile;
                Bw1StarterTextMaxLine = bw1StarterMax;
                Bw1CherenText1Line = bw1Cheren1;
                Bw1CherenText2Line = bw1Cheren2;
                Bw2StarterTextMaxLine = bw2StarterMax;
                Bw2RivalTextLine = bw2Rival;
            }
        }

        /// <summary>US Black/White 1 and 2 (all regional builds copy the same values from the primary US entry in UPR's ini).</summary>
        public static StarterPresentationRow StarterPresentationUs(bool bw2) =>
            bw2
                ? new StarterPresentationRow(316, "080A070000080000", 169, 854, 18, 26, 53, 37, 60)
                : new StarterPresentationRow(223, "080A0700080000", 430, 792, 18, 26, 53, 37, 60);

        public static byte[] ParseHexPrefix(string hex)
        {
            if (hex == null || hex.Length % 2 != 0) return Array.Empty<byte>();
            var b = new byte[hex.Length / 2];
            for (int i = 0; i < b.Length; i++)
                b[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return b;
        }

        public static int[] SpecialMusicStaticsNationalDex(bool bw2, bool blackVersion)
        {
            if (bw2)
            {
                return blackVersion
                    ? new[] { 377, 378, 379, 381, 480, 481, 482, 485, 486, 488, 494, 571, 612, 637, 638, 639, 640, 644, 646, 669 }
                    : new[] { 377, 378, 379, 380, 480, 481, 482, 485, 486, 488, 494, 571, 612, 637, 638, 639, 640, 643, 646, 668 };
            }
            return blackVersion
                ? new[] { 494, 571, 637, 638, 639, 640, 641, 643, 645, 646 }
                : new[] { 494, 571, 637, 638, 639, 640, 642, 644, 645, 646 };
        }

        public static FileOffset[][] StarterOffsets(bool bw2)
        {
            if (bw2)
            {
                return new[]
                {
                    Sp(854, 0x58B, 854, 0x590, 854, 0x595),
                    Sp(854, 0x5C0, 854, 0x5C5, 854, 0x5CA),
                    Sp(854, 0x5E2, 854, 0x5E7, 854, 0x5EC),
                };
            }
            return new[]
            {
                Sp(782, 639, 782, 644, 782, 0x361, 782, 0x5FD, 304, 0xF9, 304, 0x19C),
                Sp(782, 687, 782, 692, 782, 0x356, 782, 0x5F2, 304, 0x11C, 304, 0x1C4),
                Sp(782, 716, 782, 721, 782, 0x338, 782, 0x5D4, 304, 0x12C, 304, 0x1D9),
            };
        }

        public static TradeScriptPatch[] TradeScripts() => new[]
        {
            new TradeScriptPatch(46, 0x81, 0x86),
            new TradeScriptPatch(46, 0x97, 0x9C),
            new TradeScriptPatch(202, 0x224, 0x21F),
            new TradeScriptPatch(686, 0x76, 0x71),
            new TradeScriptPatch(830, 0xB3, 0xAE),
            new TradeScriptPatch(830, 0xEA, 0xE5),
            new TradeScriptPatch(830, 0x114, 0x10F),
            new TradeScriptPatch(764, 0x43, 0x3E),
        };

        /// <summary>Unique script file IDs for in-game trades. <paramref name="blackVersion"/> reserved when Black/White script IDs diverge (US defaults unchanged).</summary>
        public static int[] TradeScriptFileIds(bool blackVersion = true)
        {
            _ = blackVersion;
            return TradeScripts().Select(t => t.File).Distinct().OrderBy(x => x).ToArray();
        }

        /// <summary>Optional story lines to rewrite for trade dialogue (extend per game/language); empty until populated.</summary>
        public static TradeStoryTextPatch[] TradeStoryTextPatches(bool bw2, bool blackVersion)
        {
            _ = bw2;
            _ = blackVersion;
            return System.Array.Empty<TradeStoryTextPatch>();
        }

        /// <summary>Static encounter script patches (US). Use <paramref name="blackVersion"/> for Black-specific exclusives when offsets differ.</summary>
        public static List<StaticEncounterPatch> StaticEncounters(bool bw2, bool blackVersion = true)
        {
            var list = new List<StaticEncounterPatch>();
            if (bw2)
            {
                list.Add(new StaticEncounterPatch(Sp(662, 0x1DE, 662, 0x240, 740, 0xCD, 740, 0xFC, 740, 0x12C, 740, 0x14C), Sp(740, 0x12E, 740, 0x14E)));
                list.Add(new StaticEncounterPatch(Sp(730, 0x13A, 730, 0x15F, 730, 0x19B, 730, 0x1BB), Sp(730, 0x19D, 730, 0x1BD)));
                list.Add(new StaticEncounterPatch(Sp(948, 0x45D, 948, 0x48D, 948, 0x4AD), Sp(948, 0x48F, 948, 0x4AF)));
                list.Add(new StaticEncounterPatch(Sp(426, 0x38A, 426, 0x39B, 556, 0x367, 556, 0x568, 556, 0x5E6, 556, 0x6E1, 1208, 0x3A4, 1208, 0xA6A, 1208, 0x717), Sp(426, 0x39D)));
                list.Add(new StaticEncounterPatch(Sp(426, 0x36B, 426, 0x37C, 556, 0x350, 556, 0x551, 556, 0x5C7, 556, 0x6C3, 1208, 0x38D, 1208, 0xA53, 1208, 0x706), Sp(426, 0x37E)));
                list.Add(new StaticEncounterPatch(Sp(1112, 0x133, 1122, 0x2BA, 1122, 0x311, 1128, 0x37A, 1128, 0x3D1, 1208, 0x1B7, 1208, 0x1F8, 1208, 0x723, 1208, 0xF3D, 1208, 0xF4E), Sp(1208, 0xF50)));
                list.Add(new StaticEncounterPatch(Sp(1208, 0xD8B, 1208, 0xD97), Sp(1208, 0xD99)));
                list.Add(new StaticEncounterPatch(Sp(1208, 0xDB6, 1208, 0xDC2), Sp(1208, 0xDC4)));
                list.Add(new StaticEncounterPatch(Sp(304, 0xCC, 304, 0x14B, 304, 0x1BC, 304, 0x237, 304, 0x327, 304, 0x3E6, 304, 0x4A1, 304, 0x54A, 304, 0x5BD, 304, 0x5CE), Sp(304, 0x5D0)));
                list.Add(new StaticEncounterPatch(Sp(304, 0xB5, 304, 0x134, 304, 0x1A5, 304, 0x220, 304, 0x310, 304, 0x3CF, 304, 0x48A, 304, 0x533, 304, 0x59E, 304, 0x5AF), Sp(304, 0x5B1)));
                list.Add(new StaticEncounterPatch(Sp(32, 0x247, 32, 0x2B0, 32, 0x2C1, 1034, 0x12A), Sp(32, 0x2C3)));
                list.Add(new StaticEncounterPatch(Sp(684, 0x136, 684, 0x1C2, 684, 0x1D3, 1034, 0x169), Sp(684, 0x1D5)));
                list.Add(new StaticEncounterPatch(Sp(950, 0xA1, 950, 0x10A, 950, 0x11B, 1034, 0x1BE), Sp(950, 0x11D)));
                list.Add(new StaticEncounterPatch(Sp(1222, 0x134, 1222, 0x145, 1018, 0x32), Sp(1222, 0x147)));
                list.Add(new StaticEncounterPatch(Sp(1224, 0x134, 1224, 0x145, 1018, 0x2C), Sp(1224, 0x147)));
                list.Add(new StaticEncounterPatch(Sp(1226, 0x134, 1226, 0x145, 1018, 0x38), Sp(1226, 0x147)));
                list.Add(new StaticEncounterPatch(Sp(1018, 0x97, 1018, 0xA8), Sp(1018, 0xAA)));
                list.Add(new StaticEncounterPatch(Sp(526, 0x48D, 526, 0x512, 526, 0x523), Sp(526, 0x525)));
                list.Add(new StaticEncounterPatch(Sp(1068, 0x193, 1068, 0x1D6, 1068, 0x1E7, 1080, 0x193, 1080, 0x1D6, 1080, 0x1E7), Sp(1068, 0x1E9, 1080, 0x1E9)));
                list.Add(new StaticEncounterPatch(Sp(652, 0x5C6, 652, 0x5E9), Sp(652, 0x5EB)));
                list.Add(new StaticEncounterPatch(Sp(1102, 0x592, 1102, 0x5B5), Sp(1102, 0x5B7)));
                list.Add(new StaticEncounterPatch(Sp(364, 0xE, 364, 0x32, 364, 0x40), Sp(364, 0x34, 364, 0x42)));
                list.Add(new StaticEncounterPatch(Sp(1030, 0x290, 1030, 0x2A1), Sp(1030, 0x2A3)));
                list.Add(new StaticEncounterPatch(Sp(480, 0xE1, 480, 0x10A, 480, 0x131, 480, 0x15A), Sp(480, 0x10C, 480, 0x15C)));
                list.Add(new StaticEncounterPatch(Sp(1168, 0x2C, 1168, 0x4F), Sp(1168, 0x51)));
                list.Add(new StaticEncounterPatch(Sp(988, 0x382), Sp(988, 0x386)));
                list.Add(new StaticEncounterPatch(Sp(664, 0x3B5, 664, 0x3E2, 664, 0x40F, 664, 0x43C), Sp(664, 0x3B9, 664, 0x3E6, 664, 0x413, 664, 0x440)));
                list.Add(new StaticEncounterPatch(Sp(880, 0xAB4, 880, 0xAC7), Sp(880, 0xAB8)));
                list.Add(new StaticEncounterPatch(Sp(880, 0xAD3, 880, 0xAE6), Sp(880, 0xAD7)));
                list.Add(new StaticEncounterPatch(Sp(54, 0xDD), System.Array.Empty<FileOffset>()));
                list.Add(new StaticEncounterPatch(Sp(526, 0x27E), Sp(526, 0x282)));
                list.Add(new StaticEncounterPatch(Sp(1253, 0x5E0), Sp(1253, 0x3D6)));
                list.Add(new StaticEncounterPatch(Sp(1253, 0x5FF), Sp(1253, 0x3D6)));
                list.Add(new StaticEncounterPatch(Sp(1253, 0x61E), Sp(1253, 0x3D6)));
                list.Add(new StaticEncounterPatch(Sp(1253, 0x63D), Sp(1253, 0x3D6)));
                list.Add(new StaticEncounterPatch(Sp(1253, 0x65C), Sp(1253, 0x3D6)));
                list.Add(new StaticEncounterPatch(Sp(1253, 0x67B), Sp(1253, 0x3D6)));
                list.Add(new StaticEncounterPatch(Sp(1253, 0x69A), Sp(1253, 0x3D6)));
                list.Add(new StaticEncounterPatch(Sp(1253, 0x6B9), Sp(1253, 0x3D6)));
                list.Add(new StaticEncounterPatch(Sp(1253, 0x6D8), Sp(1253, 0x3D6)));
                list.Add(new StaticEncounterPatch(Sp(208, 0x5A6), Sp(208, 0x5A8)));
                list.Add(new StaticEncounterPatch(Sp(1273, 0x45), Sp(500, 0x46E, 500, 0x492, 500, 0x4B6, 506, 0x42A, 506, 0x44E)));
                list.Add(new StaticEncounterPatch(Sp(1273, 0xC7), Sp(534, 0x2F2, 534, 0x316, 562, 0x3FE, 562, 0x422, 563, 0x742, 563, 0x766, 563, 0x78A)));
                if (!blackVersion)
                {
                    // White 2–only static scripts (add offsets here when they diverge from Black 2 US).
                }
            }
            else
            {
                list.Add(new StaticEncounterPatch(Sp(304, 0x121, 304, 0x1C9, 304, 0x299), Sp(304, 0x29D)));
                list.Add(new StaticEncounterPatch(Sp(304, 0x131, 304, 0x1DE, 304, 0x2B7), Sp(304, 0x2BB)));
                list.Add(new StaticEncounterPatch(Sp(304, 0xFE, 304, 0x1A1, 304, 0x268), Sp(304, 0x26C)));
                list.Add(new StaticEncounterPatch(Sp(526, 0x758), Sp(526, 0x75C)));
                list.Add(new StaticEncounterPatch(Sp(94, 0x810, 94, 0x64, 94, 0xB4, 94, 0x44B, 94, 0x7AB, 94, 0x7D0, 94, 0x7DC), Sp(94, 0x44F)));
                list.Add(new StaticEncounterPatch(Sp(776, 0x85, 776, 0xB2), System.Array.Empty<FileOffset>()));
                list.Add(new StaticEncounterPatch(Sp(316, 0x369), Sp(316, 0x36B)));
                list.Add(new StaticEncounterPatch(Sp(316, 0x437), Sp(316, 0x439)));
                list.Add(new StaticEncounterPatch(Sp(316, 0x505), Sp(316, 0x507)));
                list.Add(new StaticEncounterPatch(Sp(316, 0x5D3), Sp(316, 0x5D5)));
                list.Add(new StaticEncounterPatch(Sp(316, 0x6A1), Sp(316, 0x6A3)));
                list.Add(new StaticEncounterPatch(Sp(306, 0x65, 306, 0x8F), Sp(306, 0x91)));
                list.Add(new StaticEncounterPatch(Sp(770, 0x2F8, 770, 0x353), Sp(770, 0x355)));
                list.Add(new StaticEncounterPatch(Sp(364, 0xE, 364, 0x1F), Sp(364, 0x21)));
                list.Add(new StaticEncounterPatch(Sp(474, 0x1CE, 474, 0x20A), Sp(474, 0x20C)));
                list.Add(new StaticEncounterPatch(Sp(426, 0x133, 426, 0x15B, 556, 0x1841, 556, 0xCFC, 556, 0x1878, 556, 0x18EA), Sp(426, 0x15D, 556, 0xCFE)));
                list.Add(new StaticEncounterPatch(Sp(426, 0x127, 426, 0x174, 556, 0x184D, 556, 0x186C, 556, 0xD15, 556, 0x18DE), Sp(426, 0x176, 556, 0xD17)));
                list.Add(new StaticEncounterPatch(Sp(670, 0x415, 670, 0x426, 692, 0x1E2), Sp(670, 0x428)));
                list.Add(new StaticEncounterPatch(Sp(458, 0x10, 458, 0x21, 692, 0x203), Sp(458, 0x23)));
                list.Add(new StaticEncounterPatch(Sp(312, 0x10, 312, 0x21, 692, 0x224), Sp(312, 0x23)));
                list.Add(new StaticEncounterPatch(Sp(752, 0x66D, 752, 0x6CC, 752, 0x6DD), Sp(752, 0x6DF)));
                list.Add(new StaticEncounterPatch(Sp(464, 0x10, 468, 0x4F, 468, 0x60), Sp(468, 0x62)));
                list.Add(new StaticEncounterPatch(Sp(877, 0x601), Sp(877, 0x3F7)));
                list.Add(new StaticEncounterPatch(Sp(877, 0x620), Sp(877, 0x3F7)));
                list.Add(new StaticEncounterPatch(Sp(877, 0x63F), Sp(877, 0x3F7)));
                list.Add(new StaticEncounterPatch(Sp(877, 0x65E), Sp(877, 0x3F7)));
                list.Add(new StaticEncounterPatch(Sp(877, 0x67D), Sp(877, 0x3F7)));
                list.Add(new StaticEncounterPatch(Sp(877, 0x69C), Sp(877, 0x3F7)));
                list.Add(new StaticEncounterPatch(Sp(877, 0x6BB), Sp(877, 0x3F7)));
                list.Add(new StaticEncounterPatch(Sp(877, 0x6DA), Sp(877, 0x3F7)));
                list.Add(new StaticEncounterPatch(Sp(877, 0x6F9), Sp(877, 0x3F7)));
                list.Add(new StaticEncounterPatch(Sp(897, 0x45), Sp(331, 0x2AA, 331, 0x2CE, 355, 0x196, 355, 0x2DA)));
                list.Add(new StaticEncounterPatch(Sp(897, 0xC1), Sp(355, 0x24A, 355, 0x26E)));
                if (!blackVersion)
                {
                    // White 1–only static scripts (add offsets here when they diverge from Black US).
                }
            }
            return list;
        }
    }
}
