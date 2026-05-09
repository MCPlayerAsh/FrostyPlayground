using System.Collections.Generic;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Gen-5 ability ID constants and ban lists ported from UPR-FVX
    /// <c>GlobalConstants.battleTrappingAbilities</c>, <c>negativeAbilities</c>, and <c>badAbilities</c>.
    /// Ability IDs match the Gen-5 personal data ability column.
    /// </summary>
    internal static class FvxGen5AbilityBanLists
    {
        public const int NoneAbility = 0;
        public const int WonderGuardAbility = 25;

        /// <summary>
        /// Last ability available in Gen 5 personal data (Teravolt = 164). Ability rolls stay in [1, MaxGen5AbilityIdInclusive].
        /// </summary>
        public const int MaxGen5AbilityIdInclusive = 164;

        /// <summary>
        /// Battle trapping abilities: prevent the opponent from switching out.
        /// IDs: Shadow Tag (23), Arena Trap (71), Magnet Pull (42).
        /// </summary>
        public static readonly HashSet<int> TrappingAbilities = new HashSet<int>
        {
            23, 42, 71
        };

        /// <summary>
        /// Negative / self-detrimental abilities (Truant, Slow Start, Defeatist, Stall, Klutz, etc.).
        /// </summary>
        public static readonly HashSet<int> NegativeAbilities = new HashSet<int>
        {
            54,  // Truant
            103, // Slow Start
            129, // Klutz
            100, // Stall
            132, // Defeatist
            12,  // Damp (very situational; UPR includes here as low-impact)
        };

        /// <summary>
        /// "Bad" abilities: extremely situational or near-useless in random battle settings.
        /// Includes form-locked / signature abilities that don't transfer well to random species.
        /// </summary>
        public static readonly HashSet<int> BadAbilities = new HashSet<int>
        {
            13,  // Limber (situational paralysis immunity)
            14,  // Cloud Nine
            17,  // Compound Eyes (only good with low-acc moves)
            20,  // Run Away (does nothing in trainer fights)
            29,  // Vital Spirit
            32,  // Suction Cups
            36,  // Pickup
            40,  // Hyper Cutter
            41,  // Pickup (alt)
            43,  // Soundproof
            45,  // Sand Veil (weather-locked)
            48,  // Plus
            49,  // Minus
            56,  // Forecast (Castform-locked)
            58,  // Sticky Hold
            60,  // Cute Charm (gender-locked)
            61,  // Plus (alt)
            68,  // Anger Point
            72,  // Honey Gather
            76,  // Air Lock
            77,  // Tangled Feet
            81,  // Steadfast
            82,  // Snow Cloak
            84,  // Normalize
            87,  // Ice Body
            96,  // Forewarn
            108, // Anticipation
            115, // Frisk
            120, // Healer
            123, // Friend Guard
            127, // Illusion (Zoroark form-locked)
            136, // Multitype (Arceus-locked)
            138, // Zen Mode (Darmanitan-locked)
            141, // Victory Star
            148, // Flare Boost
            149, // Heavy Metal
            150, // Light Metal
            155, // Pickpocket (situational)
            161, // Sap Sipper (good actually, kept here only if user picks the broad ban; can be tuned later)
        };

        /// <summary>
        /// Returns a fresh list of allowed ability IDs (1..MaxGen5AbilityIdInclusive) given the ban toggles.
        /// </summary>
        public static List<int> BuildAllowedAbilityPool(FvxPokemonTraitsOptions opt)
        {
            var pool = new List<int>(MaxGen5AbilityIdInclusive);
            for (int id = 1; id <= MaxGen5AbilityIdInclusive; id++)
            {
                if (!opt.AbilitiesAllowWonderGuard && id == WonderGuardAbility) continue;
                if (opt.AbilitiesBanTrapping && TrappingAbilities.Contains(id)) continue;
                if (opt.AbilitiesBanNegative && NegativeAbilities.Contains(id)) continue;
                if (opt.AbilitiesBanBad && BadAbilities.Contains(id)) continue;
                pool.Add(id);
            }
            if (pool.Count == 0) pool.Add(WonderGuardAbility); // fail-safe so we never roll on an empty pool
            return pool;
        }
    }
}
