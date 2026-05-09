using System;
using System.Collections.Generic;
using System.Linq;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Reads TM (95) + HM (6) move IDs from ARM9 using the same layout as FVX Gen5RomHandler.getTMMoves/getHMMoves.
    /// </summary>
    internal static class FvxGen5TmHmMoves
    {
        struct TmHmTableOffsets
        {
            public int PrefixIndex;
            public int TmBlockOneOffset;
            public int TmBlockTwoOffset;
            public int HmOffset;
        }

        public static bool TryReadTmHmMoveOrder(List<byte> arm9, out List<short> tmHmMoveIds, out string error)
        {
            tmHmMoveIds = null;
            error = null;
            byte[] raw = arm9.Count < 600000 ? BLZDecoder.BLZ_DecodePub(arm9.ToArray()) : arm9.ToArray();
            if (!TryLocateOffsets(raw, out var off, out error))
                return false;
            return TryReadTmHmMoveOrder(raw, off, out tmHmMoveIds, out error);
        }

        public static bool TryWriteTmHmMoveOrder(List<byte> arm9, IReadOnlyList<short> tmHmMoveIds, out string error)
        {
            error = null;
            if (tmHmMoveIds == null || tmHmMoveIds.Count < FvxGen5Constants.TmCount + FvxGen5Constants.HmCount)
            {
                error = "Invalid TM/HM move list.";
                return false;
            }
            bool wasCompressed = arm9.Count < 600000;
            byte[] work = wasCompressed ? BLZDecoder.BLZ_DecodePub(arm9.ToArray()) : arm9.ToArray();
            if (!TryLocateOffsets(work, out var off, out error))
                return false;

            for (int i = 0; i < FvxGen5Constants.TmBlockOneCount; i++)
                HelperFunctions.WriteShort(work, off.TmBlockOneOffset + i * 2, tmHmMoveIds[i]);
            for (int i = 0; i < (FvxGen5Constants.TmCount - FvxGen5Constants.TmBlockOneCount); i++)
                HelperFunctions.WriteShort(work, off.TmBlockTwoOffset + i * 2, tmHmMoveIds[FvxGen5Constants.TmBlockOneCount + i]);
            for (int i = 0; i < FvxGen5Constants.HmCount; i++)
                HelperFunctions.WriteShort(work, off.HmOffset + i * 2, tmHmMoveIds[FvxGen5Constants.TmCount + i]);

            arm9.Clear();
            arm9.AddRange(wasCompressed ? BLZDecoder.BLZ_EncodePub(work, false) : work);
            return true;
        }

        static int IndexOfSequence(byte[] data, byte[] pattern)
        {
            if (pattern.Length == 0 || data.Length < pattern.Length) return -1;
            for (int i = 0; i <= data.Length - pattern.Length; i++)
            {
                int j = 0;
                while (j < pattern.Length && data[i + j] == pattern[j]) j++;
                if (j == pattern.Length) return i;
            }
            return -1;
        }

        /// <summary>Move IDs for tutor slots in editor order: misc (7) + driftveil + lentimas + humilau + nacrene.</summary>
        public static List<short> ResolveBw2TutorMoveIds()
        {
            var names = VersionConstants.BW2_TutorMoves;
            var text = MainEditor.textNarc.textFiles[VersionConstants.MoveNameTextFileID].text;
            var ids = new List<short>(names.Count);
            foreach (string n in names)
            {
                int idx = text.FindIndex(s => string.Equals(s, n, StringComparison.Ordinal));
                if (idx < 0) idx = 0;
                ids.Add((short)idx);
            }
            return ids;
        }

        public static IReadOnlyList<int> EarlyRequiredHmMoveIds(bool bw2)
        {
            if (bw2) return Array.Empty<int>();
            return new[] { FvxGen5Constants.CutMoveId };
        }

        static bool TryLocateOffsets(byte[] raw, out TmHmTableOffsets offsets, out string error)
        {
            offsets = default;
            error = null;
            int idx = IndexOfSequence(raw, FvxGen5Constants.TmDataPrefixBytes);
            if (idx < 0)
            {
                error = "Could not locate TM/HM move table in ARM9.";
                return false;
            }
            int tmBlockOneOffset = idx + FvxGen5Constants.TmDataPrefixBytes.Length;
            int hmOffset = tmBlockOneOffset + FvxGen5Constants.TmBlockOneCount * 2;
            int tmBlockTwoOffset = tmBlockOneOffset + (FvxGen5Constants.TmBlockOneCount + FvxGen5Constants.HmCount) * 2;
            offsets = new TmHmTableOffsets
            {
                PrefixIndex = idx,
                TmBlockOneOffset = tmBlockOneOffset,
                TmBlockTwoOffset = tmBlockTwoOffset,
                HmOffset = hmOffset
            };
            return true;
        }

        static bool TryReadTmHmMoveOrder(byte[] raw, TmHmTableOffsets off, out List<short> tmHmMoveIds, out string error)
        {
            tmHmMoveIds = null;
            error = null;
            var list = new List<short>(FvxGen5Constants.TmCount + FvxGen5Constants.HmCount);
            for (int i = 0; i < FvxGen5Constants.TmBlockOneCount; i++)
            {
                if (off.TmBlockOneOffset + i * 2 + 1 >= raw.Length) { error = "ARM9 TM table truncated."; return false; }
                list.Add((short)HelperFunctions.ReadShort(raw, off.TmBlockOneOffset + i * 2));
            }
            for (int i = 0; i < (FvxGen5Constants.TmCount - FvxGen5Constants.TmBlockOneCount); i++)
            {
                if (off.TmBlockTwoOffset + i * 2 + 1 >= raw.Length) { error = "ARM9 TM table (block 2) truncated."; return false; }
                list.Add((short)HelperFunctions.ReadShort(raw, off.TmBlockTwoOffset + i * 2));
            }
            if (list.Count != FvxGen5Constants.TmCount)
            {
                error = "Unexpected TM count.";
                return false;
            }
            for (int i = 0; i < FvxGen5Constants.HmCount; i++)
            {
                if (off.HmOffset + i * 2 + 1 >= raw.Length) { error = "ARM9 HM table truncated."; return false; }
                list.Add((short)HelperFunctions.ReadShort(raw, off.HmOffset + i * 2));
            }
            tmHmMoveIds = list;
            return true;
        }

        public static IReadOnlyList<int> FieldMoveIds()
        {
            // Common Gen5 progression/overworld utility moves that FVX-style "keep field moves" should preserve.
            return new[] { 15, 19, 57, 70, 127, 148, 230, 249, 291, 431 };
        }

        public static IReadOnlyList<short> BuildRandomizedTmHmMoves(FvxRandomizerOptions opt, Random rnd, IReadOnlyList<short> currentTmHm, out string error)
        {
            error = null;
            if (currentTmHm == null || currentTmHm.Count < FvxGen5Constants.TmCount + FvxGen5Constants.HmCount)
            {
                error = "Current TM/HM list is invalid.";
                return null;
            }
            if (opt.TmMovesMod == FvxTmMoveMod.Unchanged)
                return currentTmHm.ToList();
            var allMoves = MainEditor.moveDataNarc?.moves;
            if (allMoves == null || allMoves.Count == 0)
            {
                error = "Move data is missing.";
                return null;
            }
            bool blockBroken = opt.TmMovesMod == FvxTmMoveMod.RandomNoGameBreaking;
            var fieldMoves = new HashSet<int>(FieldMoveIds());
            var chosen = new HashSet<int>();
            var pool = BuildMovePool(allMoves, blockBroken, includeHms: false);
            var damagingPool = pool.Where(m => FvxGen5MoveScoring.IsGoodDamaging(m, 100)).ToList();
            if (pool.Count == 0)
            {
                error = "No valid TM move candidates.";
                return null;
            }
            var output = currentTmHm.ToList();
            int tmCount = FvxGen5Constants.TmCount;
            int forceCount = opt.TmsForceGoodDamaging ? (int)Math.Round(tmCount * (Math.Max(0, Math.Min(100, opt.TmsGoodDamagingPercent)) / 100.0)) : 0;
            int forcedPlaced = 0;
            for (int i = 0; i < tmCount; i++)
            {
                int oldMove = output[i];
                if (opt.KeepFieldMoveTms && fieldMoves.Contains(oldMove))
                {
                    chosen.Add(oldMove);
                    continue;
                }
                bool forceDamaging = forcedPlaced < forceCount && damagingPool.Count > 0;
                int move = PickUniqueMoveId(forceDamaging ? damagingPool : pool, chosen, rnd);
                if (move < 0) move = PickUniqueMoveId(pool, chosen, rnd);
                if (move < 0) move = oldMove;
                output[i] = (short)move;
                chosen.Add(move);
                if (forceDamaging) forcedPlaced++;
            }
            // Preserve HM table entries as-is; HM compatibility still depends on them and HM table is separate in-game.
            return output;
        }

        static List<MoveDataEntry> BuildMovePool(IReadOnlyList<MoveDataEntry> allMoves, bool blockBroken, bool includeHms)
        {
            var hmIds = includeHms ? new List<int>() : currentHmMoveIds();
            var banned = FvxGen5MoveBanList.AllBannedForPools(blockBroken, hmIds, Array.Empty<int>());
            var pool = new List<MoveDataEntry>();
            for (int i = 0; i < allMoves.Count; i++)
            {
                var mv = allMoves[i];
                if (mv == null) continue;
                int id = mv.nameID;
                if (id <= 0 || id >= allMoves.Count) continue;
                if (mv.category == 9) continue;
                if (banned.Contains(id) || FvxGen5MoveBanList.IsBannedFromRandomPools(id)) continue;
                pool.Add(mv);
            }
            return pool;
        }

        static List<int> currentHmMoveIds()
        {
            if (MainEditor.fileSystem?.arm9 == null) return new List<int>();
            if (!TryReadTmHmMoveOrder(MainEditor.fileSystem.arm9, out var tmhm, out _))
                return new List<int>();
            return tmhm.Skip(FvxGen5Constants.TmCount).Take(FvxGen5Constants.HmCount).Select(x => (int)x).ToList();
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
