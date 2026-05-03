using System.Collections.Generic;
using System.Linq;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Move ban / filter tables (behavioral port of FVX GlobalConstants bannedRandomMoves / bannedForDamagingMove
    /// plus AbstractRomHandler.getGameBreakingMoves defaults).
    /// </summary>
    internal static class FvxGen5MoveBanList
    {
        static readonly HashSet<int> BannedRandom = BuildBannedRandom();
        static readonly HashSet<int> BannedDamaging = BuildBannedDamaging();
        static readonly HashSet<int> GameBreaking = new HashSet<int> { 49, 82 }; // SonicBoom, Dragon Rage (abstract ROM handler default)

        static HashSet<int> BuildBannedRandom()
        {
            var s = new HashSet<int> { FvxGen5Constants.StruggleMoveId };
            return s;
        }

        static HashSet<int> BuildBannedDamaging()
        {
            // FVX GlobalConstants.bannedForDamagingMove (MoveIDs); omit entries >= 559 for Gen 5 ROMs.
            int[] ids =
            {
                12, 32, 49, 82, 90, 99, 120, 132, 138, 153, 173, 205, 206, 248, 252, 255, 264, 301, 329, 353, 364, 387, 389, 485, 492
            };
            return new HashSet<int>(ids);
        }

        public static bool IsBannedFromRandomPools(int moveId) =>
            moveId < 0 || BannedRandom.Contains(moveId) || GameBreaking.Contains(moveId);

        public static bool IsBannedFromDamagingPool(int moveId) =>
            BannedDamaging.Contains(moveId);

        public static HashSet<int> AllBannedForPools(bool blockBroken, IReadOnlyList<int> hmMoveIds, IReadOnlyList<int> extraBanned)
        {
            var set = new HashSet<int>(BannedRandom);
            set.UnionWith(hmMoveIds);
            if (blockBroken) set.UnionWith(GameBreaking);
            if (extraBanned != null) set.UnionWith(extraBanned);
            return set;
        }
    }
}
