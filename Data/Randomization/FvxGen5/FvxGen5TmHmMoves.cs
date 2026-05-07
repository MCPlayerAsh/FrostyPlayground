using System;
using System.Collections.Generic;
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
        public static bool TryReadTmHmMoveOrder(List<byte> arm9, out List<short> tmHmMoveIds, out string error)
        {
            tmHmMoveIds = null;
            error = null;
            byte[] raw = arm9.Count < 600000
                ? BLZDecoder.BLZ_DecodePub(arm9.ToArray())
                : arm9.ToArray();

            int idx = IndexOfSequence(raw, FvxGen5Constants.TmDataPrefixBytes);
            if (idx < 0)
            {
                error = "Could not locate TM/HM move table in ARM9.";
                return false;
            }

            int offset = idx + FvxGen5Constants.TmDataPrefixBytes.Length;
            var list = new List<short>(FvxGen5Constants.TmCount + FvxGen5Constants.HmCount);

            for (int i = 0; i < FvxGen5Constants.TmBlockOneCount; i++)
            {
                if (offset + i * 2 + 1 >= raw.Length) { error = "ARM9 TM table truncated."; return false; }
                list.Add((short)HelperFunctions.ReadShort(raw, offset + i * 2));
            }

            offset += (FvxGen5Constants.TmBlockOneCount + FvxGen5Constants.HmCount) * 2;
            for (int i = 0; i < (FvxGen5Constants.TmCount - FvxGen5Constants.TmBlockOneCount); i++)
            {
                if (offset + i * 2 + 1 >= raw.Length) { error = "ARM9 TM table (block 2) truncated."; return false; }
                list.Add((short)HelperFunctions.ReadShort(raw, offset + i * 2));
            }

            if (list.Count != FvxGen5Constants.TmCount)
            {
                error = "Unexpected TM count.";
                return false;
            }

            int hmOffset = idx + FvxGen5Constants.TmDataPrefixBytes.Length + FvxGen5Constants.TmBlockOneCount * 2;
            for (int i = 0; i < FvxGen5Constants.HmCount; i++)
            {
                if (hmOffset + i * 2 + 1 >= raw.Length) { error = "ARM9 HM table truncated."; return false; }
                list.Add((short)HelperFunctions.ReadShort(raw, hmOffset + i * 2));
            }

            tmHmMoveIds = list;
            return true;
        }

        /// <summary>Writes TM + HM move IDs back to ARM9 (same layout as UPR setTMMoves + HM block).</summary>
        public static bool TryPatchTmHmMoveOrder(List<byte> arm9, IReadOnlyList<short> tmHmMoveIds, out string error)
        {
            error = null;
            if (tmHmMoveIds == null || tmHmMoveIds.Count != FvxGen5Constants.TmCount + FvxGen5Constants.HmCount)
            {
                error = "TM/HM list must contain exactly " + (FvxGen5Constants.TmCount + FvxGen5Constants.HmCount) + " move IDs.";
                return false;
            }

            bool compressed = arm9 != null && arm9.Count < 600000;
            byte[] raw = compressed ? BLZDecoder.BLZ_DecodePub(arm9.ToArray()) : arm9.ToArray();

            int idx = IndexOfSequence(raw, FvxGen5Constants.TmDataPrefixBytes);
            if (idx < 0)
            {
                error = "Could not locate TM/HM move table in ARM9.";
                return false;
            }

            int baseOff = idx + FvxGen5Constants.TmDataPrefixBytes.Length;

            for (int i = 0; i < FvxGen5Constants.TmBlockOneCount; i++)
            {
                if (baseOff + i * 2 + 1 >= raw.Length) { error = "ARM9 TM write range invalid."; return false; }
                HelperFunctions.WriteShort(raw, baseOff + i * 2, tmHmMoveIds[i]);
            }

            int block2 = baseOff + (FvxGen5Constants.TmBlockOneCount + FvxGen5Constants.HmCount) * 2;
            for (int i = 0; i < (FvxGen5Constants.TmCount - FvxGen5Constants.TmBlockOneCount); i++)
            {
                if (block2 + i * 2 + 1 >= raw.Length) { error = "ARM9 TM block 2 write range invalid."; return false; }
                HelperFunctions.WriteShort(raw, block2 + i * 2, tmHmMoveIds[FvxGen5Constants.TmBlockOneCount + i]);
            }

            int hmOffset = idx + FvxGen5Constants.TmDataPrefixBytes.Length + FvxGen5Constants.TmBlockOneCount * 2;
            for (int i = 0; i < FvxGen5Constants.HmCount; i++)
            {
                if (hmOffset + i * 2 + 1 >= raw.Length) { error = "ARM9 HM write range invalid."; return false; }
                HelperFunctions.WriteShort(raw, hmOffset + i * 2, tmHmMoveIds[FvxGen5Constants.TmCount + i]);
            }

            arm9.Clear();
            var outBytes = compressed ? BLZDecoder.BLZ_EncodePub(raw, false) : raw;
            arm9.AddRange(outBytes);
            return true;
        }

        /// <summary>Tries to read BW2 tutor move IDs from overlay 36 (after any shuffle). Falls back unavailable if ROM/layout mismatch.</summary>
        public static bool TryReadBw2TutorMovesFromOverlay(out List<short> tutorMoves, out string error)
        {
            tutorMoves = null;
            error = null;
            if (MainEditor.RomType != RomType.BW2 || MainEditor.fileSystem?.overlays == null)
            {
                error = "Not BW2 or overlays missing.";
                return false;
            }
            if (!FvxGen5UsRomTables.TryGetBw2MoveTutorLayout(MainEditor.RomTypeId, out int overlayId, out int dataOffset))
            {
                error = "Move tutor layout unknown for this ROM id.";
                return false;
            }

            if (overlayId < 0 || overlayId >= MainEditor.fileSystem.overlays.Count)
            {
                error = "Move tutor overlay index out of range.";
                return false;
            }

            List<byte> rawOv = MainEditor.fileSystem.overlays[overlayId];
            bool comp = MainEditor.fileSystem.y9 != null && overlayId < MainEditor.fileSystem.y9.entries.Count
                && MainEditor.fileSystem.y9.entries[overlayId].compressed;
            byte[] buf = comp ? BLZDecoder.BLZ_DecodePub(rawOv.ToArray()) : rawOv.ToArray();

            int need = dataOffset + FvxGen5Constants.Bw2MoveTutorCount * FvxGen5Constants.Bw2MoveTutorBytesPerEntry;
            if (buf.Length < need)
            {
                error = "Decoded tutor overlay too small.";
                return false;
            }

            tutorMoves = new List<short>(FvxGen5Constants.Bw2MoveTutorCount);
            for (int i = 0; i < FvxGen5Constants.Bw2MoveTutorCount; i++)
            {
                int o = dataOffset + i * FvxGen5Constants.Bw2MoveTutorBytesPerEntry;
                tutorMoves.Add((short)HelperFunctions.ReadShort(buf, o));
            }

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
    }
}
