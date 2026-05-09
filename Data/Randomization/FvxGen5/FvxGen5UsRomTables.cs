using System;
using System.IO;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>US ROM offsets from UPR gen5_offsets.ini (Black/White/B2/W2 U).</summary>
    internal static class FvxGen5UsRomTables
    {
        /// <summary>Text NARC file index for item descriptions.</summary>
        public static int GetItemDescriptionTextFileIndex(RomType rt)
        {
            if (rt == RomType.BW2) return 63;
            if (rt == RomType.BW1) return 53;
            return -1;
        }

        /// <summary>Script NARC indices for consolidated item ball / hidden item tables.</summary>
        public static bool TryGetFieldItemScriptIndices(string romTypeId, out int itemBallsNarcIndex, out int hiddenItemsNarcIndex)
        {
            switch (romTypeId?.ToLowerInvariant())
            {
                case "pokemon b":
                case "pokemon w":
                    itemBallsNarcIndex = 864;
                    hiddenItemsNarcIndex = 865;
                    return true;
                case "pokemon b2":
                case "pokemon w2":
                    itemBallsNarcIndex = 1240;
                    hiddenItemsNarcIndex = 1241;
                    return true;
                default:
                    itemBallsNarcIndex = hiddenItemsNarcIndex = 0;
                    return false;
            }
        }

        public static int[] GetItemBallsSkip(string romTypeId) => Array.Empty<int>();

        public static int[] GetHiddenItemsSkip(string romTypeId) => Array.Empty<int>();

        /// <summary>ARM9 offset (hex, decompressed) for HM forgettable patch (UPR HMMovesForgettableFunctionOffset).</summary>
        public static bool TryGetHmForgettableArm9Offset(string romTypeId, out int offsetHex)
        {
            switch (romTypeId?.ToLowerInvariant())
            {
                case "pokemon b":
                    offsetHex = 0x1D2E4;
                    return true;
                case "pokemon w":
                    offsetHex = 0x1D300;
                    return true;
                case "pokemon b2":
                    offsetHex = 0x22B18;
                    return true;
                case "pokemon w2":
                    offsetHex = 0x22B44;
                    return true;
                default:
                    offsetHex = 0;
                    return false;
            }
        }

        public static bool TryGetNationalDexScriptNarcIndex(string romTypeId, out int narcIndex)
        {
            switch (romTypeId?.ToLowerInvariant())
            {
                case "pokemon b":
                case "pokemon w":
                    narcIndex = 792;
                    return true;
                case "pokemon b2":
                case "pokemon w2":
                    narcIndex = 854;
                    return true;
                default:
                    narcIndex = 0;
                    return false;
            }
        }

        public static bool TryGetMiscIpsRelativePaths(string romTypeId, out string fastestTextRel, out string nationalDexRel)
        {
            switch (romTypeId?.ToLowerInvariant())
            {
                case "pokemon b":
                    fastestTextRel = Path.Combine("Gen5", "instant_text", "b1_instant_text.ips");
                    nationalDexRel = Path.Combine("Gen5", "national_dex", "bw1_national_dex.ips");
                    return true;
                case "pokemon w":
                    fastestTextRel = Path.Combine("Gen5", "instant_text", "w1_instant_text.ips");
                    nationalDexRel = Path.Combine("Gen5", "national_dex", "bw1_national_dex.ips");
                    return true;
                case "pokemon b2":
                    fastestTextRel = Path.Combine("Gen5", "instant_text", "b2_instant_text.ips");
                    nationalDexRel = Path.Combine("Gen5", "national_dex", "bw2_national_dex.ips");
                    return true;
                case "pokemon w2":
                    fastestTextRel = Path.Combine("Gen5", "instant_text", "w2_instant_text.ips");
                    nationalDexRel = Path.Combine("Gen5", "national_dex", "bw2_national_dex.ips");
                    return true;
                default:
                    fastestTextRel = nationalDexRel = null;
                    return false;
            }
        }

        /// <summary>Overlay 36 + data offset for BW2 move tutor table.</summary>
        public static bool TryGetBw2MoveTutorLayout(string romTypeId, out int overlayId, out int dataOffsetHex)
        {
            overlayId = FvxGen5Constants.MoveTutorBw2OverlayId;
            switch (romTypeId?.ToLowerInvariant())
            {
                case "pokemon b2":
                    dataOffsetHex = 0x51538;
                    return true;
                case "pokemon w2":
                    dataOffsetHex = 0x5152C;
                    return true;
                default:
                    overlayId = 0;
                    dataOffsetHex = 0;
                    return false;
            }
        }

        public static bool TryGetPickupOverlay(string romTypeId, out int overlayId)
        {
            switch (romTypeId?.ToLowerInvariant())
            {
                case "pokemon b":
                case "pokemon w":
                    overlayId = 92;
                    return true;
                case "pokemon b2":
                case "pokemon w2":
                    overlayId = 166;
                    return true;
                default:
                    overlayId = 0;
                    return false;
            }
        }

        /// <summary>Starter species halfword patches: script NARC index + byte offset.</summary>
        public static bool TryGetStarterWriteSites(string romTypeId, out (int narcIndex, int offset)[][] sites)
        {
            sites = null;
            switch (romTypeId?.ToLowerInvariant())
            {
                case "pokemon b":
                case "pokemon w":
                    sites = Bw1StarterSites;
                    return true;
                case "pokemon b2":
                case "pokemon w2":
                    sites = Bw2StarterSites;
                    return true;
                default:
                    return false;
            }
        }

        static readonly (int narcIndex, int offset)[][] Bw1StarterSites =
        {
            new[]
            {
                (782, 639), (782, 644), (782, 0x361), (782, 0x5FD), (304, 0xF9), (304, 0x19C)
            },
            new[]
            {
                (782, 687), (782, 692), (782, 0x356), (782, 0x5F2), (304, 0x11C), (304, 0x1C4)
            },
            new[]
            {
                (782, 716), (782, 721), (782, 0x338), (782, 0x5D4), (304, 0x12C), (304, 0x1D9)
            }
        };

        static readonly (int narcIndex, int offset)[][] Bw2StarterSites =
        {
            new[] { (854, 0x58B), (854, 0x590), (854, 0x595) },
            new[] { (854, 0x5C0), (854, 0x5C5), (854, 0x5CA) },
            new[] { (854, 0x5E2), (854, 0x5E7), (854, 0x5EC) }
        };

        public static bool IsSupportedUsRom(string romTypeId)
        {
            switch (romTypeId?.ToLowerInvariant())
            {
                case "pokemon b":
                case "pokemon w":
                case "pokemon b2":
                case "pokemon w2":
                    return true;
                default:
                    return false;
            }
        }
    }
}
