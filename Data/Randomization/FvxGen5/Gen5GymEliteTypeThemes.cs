using System.Collections.Generic;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Mirrors UPR-FVX Gen5Constants gym/elite theme maps and Gen5RomHandler.getGymAndEliteTypeThemes.
    /// Gen5 internal type IDs (0–16, no Fairy).
    /// Used for <c>KEEP_THEMED</c> / group assignment; hacks with custom tags fall back to shared team type.
    /// </summary>
    internal static class Gen5GymEliteTypeThemes
    {
        /// <summary>Black/White — vanilla layout.</summary>
        static readonly Dictionary<string, byte> Bw1 = new Dictionary<string, byte>(System.StringComparer.Ordinal)
        {
            ["ELITE1"] = 7,  // Ghost
            ["ELITE2"] = 16, // Dark
            ["ELITE3"] = 13, // Psychic
            ["ELITE4"] = 1,  // Fighting
            ["GYM1"] = 11,   // Grass
            ["GYM2"] = 0,    // Normal
            ["GYM3"] = 6,    // Bug
            ["GYM4"] = 12,   // Electric
            ["GYM5"] = 4,    // Ground
            ["GYM6"] = 2,    // Flying
            ["GYM7"] = 14,   // Ice
            ["GYM8"] = 15,   // Dragon
            ["GYM9"] = 9,    // Fire
            ["GYM10"] = 10,  // Water
            ["GYM11"] = 0,   // Normal (trio gym trainers)
        };

        /// <summary>Black 2 / White 2 — vanilla layout.</summary>
        static readonly Dictionary<string, byte> Bw2 = new Dictionary<string, byte>(System.StringComparer.Ordinal)
        {
            ["CHAMPION"] = 15, // Dragon
            ["ELITE1"] = 7,
            ["ELITE2"] = 16,
            ["ELITE3"] = 13,
            ["ELITE4"] = 1,
            ["GYM1"] = 0,
            ["GYM2"] = 3,    // Poison
            ["GYM3"] = 6,
            ["GYM4"] = 12,
            ["GYM5"] = 4,
            ["GYM6"] = 2,
            ["GYM7"] = 15,
            ["GYM8"] = 10,
        };

        /// <summary>
        /// When <paramref name="useBw1StarterTriangle"/> is true for BW1, overrides GYM1/GYM9/GYM10 with the three
        /// starters' primary types (slot order matches scripts). Mirrors UPR-FVX when <c>isTypeTriangleChanged()</c>.
        /// </summary>
        public static bool TryGetTheme(string groupKey, bool isBw2, byte[] bw1StarterPrimaryTypes,
            bool useBw1StarterTriangle, out byte type)
        {
            type = 0;
            if (string.IsNullOrEmpty(groupKey))
                return false;

            if (!isBw2 && useBw1StarterTriangle && bw1StarterPrimaryTypes != null
                && bw1StarterPrimaryTypes.Length == 3)
            {
                switch (groupKey)
                {
                    case "GYM1":
                        type = bw1StarterPrimaryTypes[0];
                        return true;
                    case "GYM9":
                        type = bw1StarterPrimaryTypes[1];
                        return true;
                    case "GYM10":
                        type = bw1StarterPrimaryTypes[2];
                        return true;
                }
            }

            return (isBw2 ? Bw2 : Bw1).TryGetValue(groupKey, out type);
        }

        public static bool TryGetTheme(string groupKey, bool isBw2, out byte type)
            => TryGetTheme(groupKey, isBw2, null, false, out type);
    }
}
