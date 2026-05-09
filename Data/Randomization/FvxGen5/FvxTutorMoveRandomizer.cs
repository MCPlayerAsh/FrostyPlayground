using System;
using System.Collections.Generic;
using System.Linq;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    internal static class FvxTutorMoveRandomizer
    {
        public static IReadOnlyList<short> BuildRandomizedTutorMoves(FvxRandomizerOptions opt, Random rnd, IReadOnlyList<short> currentTutorIds, IReadOnlyList<short> tmHmIds, out string error)
        {
            error = null;
            if (MainEditor.RomType != RomType.BW2)
                return Array.Empty<short>();
            if (currentTutorIds == null || currentTutorIds.Count == 0)
                return Array.Empty<short>();
            if (opt.TutorMovesMod == FvxTutorMoveMod.Unchanged)
                return currentTutorIds.ToList();

            var allMoves = MainEditor.moveDataNarc?.moves;
            if (allMoves == null || allMoves.Count == 0)
            {
                error = "Move data is missing.";
                return null;
            }

            bool blockBroken = opt.TutorMovesMod == FvxTutorMoveMod.RandomNoGameBreaking;
            var tmHmSet = new HashSet<int>((tmHmIds ?? Array.Empty<short>()).Select(x => (int)x));
            var fieldMoves = new HashSet<int>(FvxGen5TmHmMoves.FieldMoveIds());
            var pool = BuildMovePool(allMoves, blockBroken, tmHmSet);
            var damagingPool = pool.Where(m => FvxGen5MoveScoring.IsGoodDamaging(m, 100)).ToList();
            if (pool.Count == 0)
            {
                error = "No valid tutor move candidates.";
                return null;
            }

            int total = currentTutorIds.Count;
            int forceCount = opt.TutorsForceGoodDamaging ? (int)Math.Round(total * (Math.Max(0, Math.Min(100, opt.TutorsGoodDamagingPercent)) / 100.0)) : 0;
            int forcedPlaced = 0;
            var used = new HashSet<int>();
            var outIds = currentTutorIds.ToList();
            for (int i = 0; i < outIds.Count; i++)
            {
                int oldMove = outIds[i];
                if (opt.KeepFieldMoveTutors && fieldMoves.Contains(oldMove))
                {
                    used.Add(oldMove);
                    continue;
                }
                bool forceDamaging = forcedPlaced < forceCount && damagingPool.Count > 0;
                int pick = PickUniqueMoveId(forceDamaging ? damagingPool : pool, used, rnd);
                if (pick < 0) pick = PickUniqueMoveId(pool, used, rnd);
                if (pick < 0) pick = oldMove;
                outIds[i] = (short)pick;
                used.Add(pick);
                if (forceDamaging) forcedPlaced++;
            }
            return outIds;
        }

        static List<MoveDataEntry> BuildMovePool(IReadOnlyList<MoveDataEntry> allMoves, bool blockBroken, HashSet<int> excludedMoveIds)
        {
            var ban = FvxGen5MoveBanList.AllBannedForPools(blockBroken, Array.Empty<int>(), Array.Empty<int>());
            var pool = new List<MoveDataEntry>();
            for (int i = 0; i < allMoves.Count; i++)
            {
                var mv = allMoves[i];
                if (mv == null) continue;
                int id = mv.nameID;
                if (id <= 0 || id >= allMoves.Count) continue;
                if (mv.category == 9) continue;
                if (ban.Contains(id) || FvxGen5MoveBanList.IsBannedFromRandomPools(id)) continue;
                if (excludedMoveIds.Contains(id)) continue;
                pool.Add(mv);
            }
            return pool;
        }

        static int PickUniqueMoveId(IReadOnlyList<MoveDataEntry> pool, HashSet<int> used, Random rnd)
        {
            if (pool == null || pool.Count == 0) return -1;
            var candidates = pool.Where(m => m != null && !used.Contains(m.nameID)).ToList();
            if (candidates.Count == 0) return -1;
            return candidates[rnd.Next(candidates.Count)].nameID;
        }
    }
}
