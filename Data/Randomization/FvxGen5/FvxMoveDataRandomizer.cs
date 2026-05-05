using System;
using System.Collections.Generic;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public static class FvxMoveDataRandomizer
    {
        public static void Apply(FvxMoveDataSettings s, Random rnd, TypeSystemContext ctx)
        {
            if (MainEditor.moveDataNarc?.moves == null || rnd == null || ctx == null) return;
            var moves = MainEditor.moveDataNarc.moves;
            var names = MainEditor.textNarc?.textFiles[VersionConstants.MoveNameTextFileID]?.text;

            bool any = s.RandomizeMovePower || s.RandomizeMoveAccuracy || s.RandomizeMovePP
                || s.RandomizeMoveTypes || s.RandomizeMoveCategory || s.RandomizeMoveNames
                || s.UpdateMovesToGeneration;
            if (!any) return;

            if (s.UpdateMovesToGeneration)
            {
                // Full generation-updates require external move tables; skipped until ported from FVX.
            }

            for (int i = 1; i < moves.Count; i++)
            {
                if (!MoveSlot.IsEligibleForPools(i, names)) continue;
                var m = moves[i];

                if (s.RandomizeMovePower)
                    m.basePower = (byte)rnd.Next(0, 256);
                if (s.RandomizeMoveAccuracy)
                    m.accuracy = (byte)rnd.Next(0, 102);
                if (s.RandomizeMovePP)
                    m.powerPoints = (byte)rnd.Next(5, 41);
                if (s.RandomizeMoveTypes)
                    m.element = ctx.RandomType(rnd);
                if (s.RandomizeMoveCategory)
                    m.damageType = (byte)rnd.Next(0, 3);

                m.ApplyData();
            }

            if (s.RandomizeMoveNames && names != null && MainEditor.textNarc != null)
            {
                var swapIdx = new List<int>();
                for (int i = 1; i < moves.Count && i < names.Count; i++)
                    if (MoveSlot.IsEligibleForPools(i, names)) swapIdx.Add(i);
                for (int a = 0; a < swapIdx.Count; a++)
                {
                    int b = rnd.Next(swapIdx.Count);
                    int i = swapIdx[a], j = swapIdx[b];
                    string tmp = names[i];
                    names[i] = names[j];
                    names[j] = tmp;
                }
                try { MainEditor.textNarc.textFiles[VersionConstants.MoveNameTextFileID].CompressData(); } catch { /* ignore */ }
            }
        }
    }

    /// <summary>Shared eligibility checks with trainer/move randomizers.</summary>
    public static class MoveSlot
    {
        public static bool IsEligibleForPools(int moveId, IReadOnlyList<string> moveNames)
        {
            if (moveId <= 0) return false;
            if (moveId >= 560 && moveId <= 679) return false;
            if (moveId >= 680 && moveNames != null && moveId < moveNames.Count)
            {
                string n = moveNames[moveId];
                if (string.Equals(n, "Empty", StringComparison.OrdinalIgnoreCase)) return false;
                if (string.Equals(n, "Pound", StringComparison.OrdinalIgnoreCase)) return false;
            }
            return true;
        }
    }
}
