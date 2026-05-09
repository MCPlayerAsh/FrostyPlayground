using System;
using System.Collections.Generic;
using System.Text;
using NewEditor.Data;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>ARM9 TM shuffle + BW2 tutor shuffle + pickup overlay patches.</summary>
    internal static class FvxGen5MoveListStages
    {
        public static bool ApplyRandomizeTmMoveList(FvxTmHmTutorExtrasSettings extras, Random rnd, StringBuilder log)
        {
            if (!extras.RandomizeTmMoveList) return true;
            if (MainEditor.fileSystem?.arm9 == null || MainEditor.moveDataNarc?.moves == null)
            {
                log.AppendLine("[TM move list] Skipped (ARM9 or moves not loaded).");
                return true;
            }

            var names = MainEditor.textNarc?.textFiles[VersionConstants.MoveNameTextFileID]?.text;
            if (!FvxGen5TmHmMoves.TryReadTmHmMoveOrder(MainEditor.fileSystem.arm9, out var current, out var errRead))
            {
                log.AppendLine("[TM move list] " + errRead);
                return false;
            }

            var pool = new List<int>();
            for (int i = 1; i < MainEditor.moveDataNarc.moves.Count; i++)
            {
                if (!MoveSlot.IsEligibleForPools(i, names)) continue;
                if (extras.NoGameBreakingMovesInTms && FvxGen5MoveBanList.IsBannedFromRandomPools(i)) continue;
                pool.Add(i);
            }

            if (pool.Count == 0)
            {
                log.AppendLine("[TM move list] No eligible moves in pool.");
                return false;
            }

            var newTms = new short[FvxGen5Constants.TmCount];
            if (pool.Count >= FvxGen5Constants.TmCount)
            {
                pool.Shuffle(rnd);
                for (int i = 0; i < FvxGen5Constants.TmCount; i++)
                    newTms[i] = (short)pool[i];
            }
            else
            {
                for (int i = 0; i < FvxGen5Constants.TmCount; i++)
                    newTms[i] = (short)pool[rnd.Next(pool.Count)];
            }

            var full = new List<short>(FvxGen5Constants.TmCount + FvxGen5Constants.HmCount);
            full.AddRange(newTms);
            for (int h = 0; h < FvxGen5Constants.HmCount; h++)
                full.Add(current[FvxGen5Constants.TmCount + h]);

            if (!FvxGen5TmHmMoves.TryPatchTmHmMoveOrder(MainEditor.fileSystem.arm9, full, out var errPatch))
            {
                log.AppendLine("[TM move list] " + errPatch);
                return false;
            }

            log.AppendLine("[TM move list] Randomized " + FvxGen5Constants.TmCount + " TMs (HMs unchanged).");
            if (extras.SyncTmItemDescriptionsAndPalettes)
                FvxGen5TmItemMetadataSync.TrySyncAfterTmListChange(newTms, log);
            return true;
        }

        public static bool ApplyRandomizeTutorMoveList(FvxTmHmTutorExtrasSettings extras, Random rnd, StringBuilder log)
        {
            if (!extras.RandomizeTutorMoveList) return true;
            if (MainEditor.RomType != RomType.BW2)
            {
                log.AppendLine("[Tutor move list] Skipped (BW2 only).");
                return true;
            }

            if (!FvxGen5UsRomTables.TryGetBw2MoveTutorLayout(MainEditor.RomTypeId, out int overlayId, out int dataOffset)
                || MainEditor.moveDataNarc?.moves == null)
            {
                log.AppendLine("[Tutor move list] Unsupported ROM or missing data.");
                return false;
            }

            var names = MainEditor.textNarc?.textFiles[VersionConstants.MoveNameTextFileID]?.text;
            var pool = new List<int>();
            for (int i = 1; i < MainEditor.moveDataNarc.moves.Count; i++)
            {
                if (!MoveSlot.IsEligibleForPools(i, names)) continue;
                if (extras.NoGameBreakingMovesInTms && FvxGen5MoveBanList.IsBannedFromRandomPools(i)) continue;
                pool.Add(i);
            }

            if (pool.Count == 0)
            {
                log.AppendLine("[Tutor move list] No eligible moves.");
                return false;
            }

            string errMut = null;
            bool ok = FvxGen5OverlayIo.TryMutateOverlay(overlayId, buf =>
            {
                int need = dataOffset + FvxGen5Constants.Bw2MoveTutorCount * FvxGen5Constants.Bw2MoveTutorBytesPerEntry;
                if (buf.Length < need)
                {
                    errMut = "Tutor overlay buffer too small.";
                    return;
                }

                pool.Shuffle(rnd);
                for (int i = 0; i < FvxGen5Constants.Bw2MoveTutorCount; i++)
                {
                    int mid = pool.Count >= FvxGen5Constants.Bw2MoveTutorCount
                        ? pool[i]
                        : pool[rnd.Next(pool.Count)];
                    HelperFunctions.WriteShort(buf, dataOffset + i * FvxGen5Constants.Bw2MoveTutorBytesPerEntry, mid);
                }
            }, out var errOv);

            if (!string.IsNullOrEmpty(errMut))
            {
                log.AppendLine("[Tutor move list] " + errMut);
                return false;
            }

            if (!ok)
            {
                log.AppendLine("[Tutor move list] " + errOv);
                return false;
            }

            log.AppendLine("[Tutor move list] Randomized " + FvxGen5Constants.Bw2MoveTutorCount + " tutor slots.");
            return true;
        }

        public static bool ApplyPickupRandom(FvxItemsSettings items, Random rnd, StringBuilder log)
        {
            if (items.Pickup != FvxPickupMode.Random) return true;
            if (MainEditor.itemDataNarc?.items == null)
            {
                log.AppendLine("[Pickup] Item data not loaded.");
                return false;
            }

            if (!FvxGen5UsRomTables.TryGetPickupOverlay(MainEditor.RomTypeId, out int pickupOvl))
            {
                log.AppendLine("[Pickup] Unsupported ROM id for pickup overlay.");
                return false;
            }

            int maxItem = Math.Max(1, MainEditor.itemDataNarc.items.Count - 1);
            string errMut = null;
            bool ok = FvxGen5OverlayIo.TryMutateOverlay(pickupOvl, buf =>
            {
                int idx = IndexOfSequence(buf, FvxGen5Constants.PickupTableLocatorBytes);
                if (idx < 0)
                {
                    errMut = "Pickup table locator not found in overlay.";
                    return;
                }

                if (idx + FvxGen5Constants.NumberOfPickupItems * 2 > buf.Length)
                {
                    errMut = "Pickup table extends past overlay.";
                    return;
                }

                for (int i = 0; i < FvxGen5Constants.NumberOfPickupItems; i++)
                {
                    int itemId = rnd.Next(1, maxItem + 1);
                    HelperFunctions.WriteShort(buf, idx + i * 2, itemId);
                }
            }, out var errOv);

            if (!string.IsNullOrEmpty(errMut))
            {
                log.AppendLine("[Pickup] " + errMut);
                return false;
            }

            if (!ok)
            {
                log.AppendLine("[Pickup] " + errOv);
                return false;
            }

            log.AppendLine("[Pickup] Randomized " + FvxGen5Constants.NumberOfPickupItems + " pickup entries.");
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
    }
}
