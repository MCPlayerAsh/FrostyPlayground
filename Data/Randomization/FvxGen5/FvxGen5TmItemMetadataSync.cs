using System;
using System.Collections.Generic;
using System.Text;
using NewEditor.Data;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>After TM move IDs change, sync item description strings and TM case palette words in ARM9 (UPR setTMMoves follow-up).</summary>
    internal static class FvxGen5TmItemMetadataSync
    {
        public const int TmItem01 = 328;
        public const int TmItem93 = 420;

        /// <summary>Maps move type (0–17) to TM disc palette index; mirrors UPR Gen5 type colors.</summary>
        public static int TmPaletteForMoveType(byte t)
        {
            if (t > 17) return 0;
            // Gen 5 TM/HM case palette indices track type order; unknown types stay 0.
            return t;
        }

        public static bool TrySyncAfterTmListChange(IReadOnlyList<short> tmMoveIds, StringBuilder log)
        {
            if (tmMoveIds == null || tmMoveIds.Count < FvxGen5Constants.TmCount) return true;

            if (MainEditor.textNarc?.textFiles == null || MainEditor.moveDataNarc?.moves == null
                || MainEditor.fileSystem?.arm9 == null)
            {
                log?.AppendLine("[TM item metadata] Skipped (text, moves, or ARM9 missing).");
                return true;
            }

            int moveDescFile = VersionConstants.MoveDescriptionTextFileID;
            int itemDescFile = FvxGen5UsRomTables.GetItemDescriptionTextFileIndex(MainEditor.RomType);
            if (itemDescFile < 0 || moveDescFile < 0
                || itemDescFile >= MainEditor.textNarc.textFiles.Count
                || moveDescFile >= MainEditor.textNarc.textFiles.Count)
            {
                log?.AppendLine("[TM item metadata] Skipped (item/move description text file index unavailable for this ROM).");
                return true;
            }

            var itemTf = MainEditor.textNarc.textFiles[itemDescFile];
            var moveTf = MainEditor.textNarc.textFiles[moveDescFile];
            if (itemTf?.text == null || moveTf?.text == null)
            {
                log?.AppendLine("[TM item metadata] Skipped (missing text lines).");
                return true;
            }

            int maxItemLine = TmItem93 + (FvxGen5Constants.TmCount - FvxGen5Constants.TmBlockOneCount - 1);
            while (itemTf.text.Count <= maxItemLine) itemTf.text.Add("");

            for (int i = 0; i < FvxGen5Constants.TmBlockOneCount; i++)
            {
                int mid = tmMoveIds[i];
                if (mid < 0 || mid >= moveTf.text.Count) continue;
                int itemLine = TmItem01 + i;
                if (itemLine < itemTf.text.Count)
                    itemTf.text[itemLine] = moveTf.text[mid] ?? "";
            }

            for (int i = 0; i < FvxGen5Constants.TmCount - FvxGen5Constants.TmBlockOneCount; i++)
            {
                int mid = tmMoveIds[FvxGen5Constants.TmBlockOneCount + i];
                if (mid < 0 || mid >= moveTf.text.Count) continue;
                int itemLine = TmItem93 + i;
                if (itemLine < itemTf.text.Count)
                    itemTf.text[itemLine] = moveTf.text[mid] ?? "";
            }

            try { itemTf.CompressData(); } catch { /* keep going */ }

            if (!TryPatchTmPalettes(tmMoveIds, log))
                log?.AppendLine("[TM item metadata] Warning: could not patch TM palette words (descriptions still updated).");

            log?.AppendLine("[TM item metadata] Synced TM item descriptions (text file " + itemDescFile + ").");
            return true;
        }

        static bool TryPatchTmPalettes(IReadOnlyList<short> tmMoveIds, StringBuilder log)
        {
            bool bw2 = MainEditor.RomType == RomType.BW2;
            byte[] prefixHex = HexToBytes(bw2 ? "FD03FE03020003000400050006000700" : "E903EA03020003000400050006000700");
            bool compressed = MainEditor.fileSystem.arm9.Count < 600000;
            byte[] raw = compressed ? BLZDecoder.BLZ_DecodePub(MainEditor.fileSystem.arm9.ToArray()) : MainEditor.fileSystem.arm9.ToArray();

            int idx = IndexOf(raw, prefixHex);
            if (idx < 0)
            {
                log?.AppendLine("[TM item metadata] TM palette block not found in ARM9.");
                return false;
            }


            int offsPals = idx + prefixHex.Length;

            for (int i = 0; i < FvxGen5Constants.TmBlockOneCount; i++)
            {
                int mid = tmMoveIds[i];
                if (mid < 1 || mid >= MainEditor.moveDataNarc.moves.Count) continue;
                byte el = MainEditor.moveDataNarc.moves[mid].element;
                int pal = TmPaletteForMoveType(el);
                int itmNum = TmItem01 + i;
                int o = offsPals + itmNum * 4 + 2;
                if (o + 1 >= raw.Length) return false;
                HelperFunctions.WriteShort(raw, o, pal);
            }

            for (int i = 0; i < FvxGen5Constants.TmCount - FvxGen5Constants.TmBlockOneCount; i++)
            {
                int mid = tmMoveIds[FvxGen5Constants.TmBlockOneCount + i];
                if (mid < 1 || mid >= MainEditor.moveDataNarc.moves.Count) continue;
                byte el = MainEditor.moveDataNarc.moves[mid].element;
                int pal = TmPaletteForMoveType(el);
                int itmNum = TmItem93 + i;
                int o = offsPals + itmNum * 4 + 2;
                if (o + 1 >= raw.Length) return false;
                HelperFunctions.WriteShort(raw, o, pal);
            }

            MainEditor.fileSystem.arm9.Clear();
            MainEditor.fileSystem.arm9.AddRange(compressed ? BLZDecoder.BLZ_EncodePub(raw, false) : raw);
            return true;
        }

        static byte[] HexToBytes(string hex)
        {
            var b = new byte[hex.Length / 2];
            for (int i = 0; i < b.Length; i++)
                b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return b;
        }

        static int IndexOf(byte[] data, byte[] pattern)
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
    }
}
