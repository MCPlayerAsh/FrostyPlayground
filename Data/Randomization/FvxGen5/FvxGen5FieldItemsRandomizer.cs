using System;
using System.Collections.Generic;
using System.Text;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Field item balls + hidden items via ScriptNARC (UPR getFieldItemIds / setFieldItemIds).</summary>
    internal static class FvxGen5FieldItemsRandomizer
    {
        const int ScriptListTerminator = 0xFD13;
        const int NormalItemSetVarCommand = 0x28;
        const int HiddenItemSetVarCommand = 0x2A;
        const int NormalItemVarSet = 0x800C;
        const int HiddenItemVarSet = 0x8000;

        public static bool TryApply(FvxFieldItemsMode mode, Random rnd, StringBuilder log)
        {
            if (mode == FvxFieldItemsMode.Unchanged) return true;
            if (!FvxGen5UsRomTables.TryGetFieldItemScriptIndices(MainEditor.RomTypeId, out int narcNormal, out int narcHidden))
            {
                log.AppendLine("[Field items] Unsupported ROM id (US B/W/B2/W2 only).");
                return true;
            }

            if (MainEditor.scriptNarc?.scriptFiles == null)
            {
                log.AppendLine("[Field items] Script NARC not loaded.");
                return false;
            }

            int maxItem = MainEditor.itemDataNarc?.items != null
                ? Math.Max(1, MainEditor.itemDataNarc.items.Count - 1)
                : 639;

            var ids = new List<int>();
            if (!CollectItemIds(narcNormal, narcHidden, ids, out string errCollect))
            {
                log.AppendLine("[Field items] " + errCollect);
                return false;
            }

            if (ids.Count == 0)
            {
                log.AppendLine("[Field items] No item slots found in script tables.");
                return true;
            }

            var next = new List<int>(ids.Count);
            switch (mode)
            {
                case FvxFieldItemsMode.Shuffle:
                    next.AddRange(ids);
                    next.Shuffle(rnd);
                    break;
                case FvxFieldItemsMode.Random:
                case FvxFieldItemsMode.RandomEvenDistribution:
                    for (int i = 0; i < ids.Count; i++)
                        next.Add(rnd.Next(1, maxItem + 1));
                    if (mode == FvxFieldItemsMode.RandomEvenDistribution)
                        log.AppendLine("[Field items] Note: \"even distribution\" uses independent random draws (same as Random).");
                    break;
                default:
                    return true;
            }

            if (!WriteItemIds(narcNormal, narcHidden, next, out string errWrite))
            {
                log.AppendLine("[Field items] " + errWrite);
                return false;
            }

            log.AppendLine("[Field items] Updated " + ids.Count + " ball/hidden item slots (" + mode + ").");
            return true;
        }

        static bool CollectItemIds(int narcNormal, int narcHidden, List<int> ids, out string error)
        {
            error = null;
            if (narcNormal < 0 || narcNormal >= MainEditor.scriptNarc.scriptFiles.Count
                || narcHidden < 0 || narcHidden >= MainEditor.scriptNarc.scriptFiles.Count)
            {
                error = "Item ball / hidden item script index out of range.";
                return false;
            }

            CollectFromFile(MainEditor.scriptNarc.scriptFiles[narcNormal], true, ids);
            CollectFromFile(MainEditor.scriptNarc.scriptFiles[narcHidden], false, ids);
            return true;
        }

        static void CollectFromFile(ScriptFile sf, bool normal, List<int> ids)
        {
            if (sf?.bytes == null) return;
            var itemScripts = ToByteArray(sf.bytes);
            int[] skip = normal ? FvxGen5UsRomTables.GetItemBallsSkip(MainEditor.RomTypeId)
                : FvxGen5UsRomTables.GetHiddenItemsSkip(MainEditor.RomTypeId);
            int setCmd = normal ? NormalItemSetVarCommand : HiddenItemSetVarCommand;
            int varSet = normal ? NormalItemVarSet : HiddenItemVarSet;

            int offset = 0;
            int skipTableOffset = 0;
            while (true)
            {
                int part1 = HelperFunctions.ReadShort(itemScripts, offset);
                if (part1 == ScriptListTerminator) break;

                int offsetInFile = ReadRelativePointer(itemScripts, offset);
                offset += 4;
                if (offsetInFile > itemScripts.Length) break;

                if (skipTableOffset < skip.Length && skip[skipTableOffset] == (offset / 4) - 1)
                {
                    skipTableOffset++;
                    continue;
                }

                int command = HelperFunctions.ReadShort(itemScripts, offsetInFile + 2);
                int variable = HelperFunctions.ReadShort(itemScripts, offsetInFile + 4);
                if (command == setCmd && variable == varSet)
                    ids.Add(HelperFunctions.ReadShort(itemScripts, offsetInFile + 6));
            }
        }

        static bool WriteItemIds(int narcNormal, int narcHidden, List<int> newIds, out string error)
        {
            error = null;
            int idx = 0;
            if (!WriteFile(MainEditor.scriptNarc.scriptFiles[narcNormal], true, newIds, ref idx, out error))
                return false;
            if (!WriteFile(MainEditor.scriptNarc.scriptFiles[narcHidden], false, newIds, ref idx, out error))
                return false;
            if (idx != newIds.Count)
            {
                error = "Internal error: item slot count mismatch on write.";
                return false;
            }
            return true;
        }

        static bool WriteFile(ScriptFile sf, bool normal, List<int> newIds, ref int idx, out string error)
        {
            error = null;
            if (sf?.bytes == null) return true;
            var itemScripts = ToByteArray(sf.bytes);
            int[] skip = normal ? FvxGen5UsRomTables.GetItemBallsSkip(MainEditor.RomTypeId)
                : FvxGen5UsRomTables.GetHiddenItemsSkip(MainEditor.RomTypeId);
            int setCmd = normal ? NormalItemSetVarCommand : HiddenItemSetVarCommand;
            int varSet = normal ? NormalItemVarSet : HiddenItemVarSet;

            int offset = 0;
            int skipTableOffset = 0;
            while (true)
            {
                int part1 = HelperFunctions.ReadShort(itemScripts, offset);
                if (part1 == ScriptListTerminator) break;

                int offsetInFile = ReadRelativePointer(itemScripts, offset);
                offset += 4;
                if (offsetInFile > itemScripts.Length) break;

                if (skipTableOffset < skip.Length && skip[skipTableOffset] == (offset / 4) - 1)
                {
                    skipTableOffset++;
                    continue;
                }

                int command = HelperFunctions.ReadShort(itemScripts, offsetInFile + 2);
                int variable = HelperFunctions.ReadShort(itemScripts, offsetInFile + 4);
                if (command == setCmd && variable == varSet)
                {
                    if (idx >= newIds.Count)
                    {
                        error = "Ran out of item values while writing scripts.";
                        return false;
                    }
                    HelperFunctions.WriteShort(itemScripts, offsetInFile + 6, newIds[idx++]);
                }
            }

            CopyToScriptFile(sf, itemScripts);
            return true;
        }

        static int ReadRelativePointer(byte[] data, int offset) =>
            HelperFunctions.ReadInt(data, offset) + offset + 4;

        static byte[] ToByteArray(RefByte[] rb)
        {
            var b = new byte[rb.Length];
            for (int i = 0; i < rb.Length; i++) b[i] = rb[i];
            return b;
        }

        static void CopyToScriptFile(ScriptFile sf, byte[] data)
        {
            if (sf.bytes.Length != data.Length)
            {
                sf.bytes = new RefByte[data.Length];
                for (int i = 0; i < data.Length; i++) sf.bytes[i] = data[i];
            }
            else
            {
                for (int i = 0; i < data.Length; i++) sf.bytes[i] = data[i];
            }
        }
    }
}
