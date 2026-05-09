using System;
using System.Collections.Generic;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// In-game trades: locates FieldTradeCheck / TradeNPCQualify (0x1B5) and following GivePokemon-style commands by scanning
    /// script bytes (including subroutine regions), then applies species / IV / item / nickname options.
    /// </summary>
    internal static class FvxGen5TradeScriptPatcher
    {
        const short CmdTradeCheck = 0x1B5;
        const short CmdGivePokemonLike = 0x10C;
        const short CmdGivePokemon2Like = 0x10E;
        const short CmdGivePokemonNLike = 0x2EA;
        const short CmdWordSetPastTradePkmName = 0x22F;
        const short CmdWordSetPokeSpecies = 0x57;
        const short CmdVmJump = 0x1E;
        const short CmdVmJumpIf = 0x1F;
        const short CmdCallRoutine = 0x4;

        const int MaxBytesAfterTradeForGive = 4500;
        /// <summary>UPR-style cap for in-game trade nickname strings shown from text NARC.</summary>
        internal const int MaxTradeNicknameChars = 10;

        public static void ApplyToNarc(FvxStartersStaticsTradesOptions opt, Random rnd, IReadOnlyList<int> speciesPool, ScriptNARC narc, bool blackVersion = true)
        {
            if (narc?.scriptFiles == null || speciesPool == null || speciesPool.Count == 0) return;
            foreach (int fileId in FvxGen5RomLayout.TradeScriptFileIds(blackVersion))
            {
                if (fileId < 0 || fileId >= narc.scriptFiles.Count) continue;
                RefByte[] bytes = narc.scriptFiles[fileId].bytes;
                if (opt.TradesRandomizeNicknames)
                    bytes = ReplacePastTradeNameWithWordSetPokeSpecies(bytes, opt, rnd, speciesPool);
                ApplyToBuffer(opt, rnd, speciesPool, bytes);
                narc.scriptFiles[fileId].bytes = bytes;
            }
        }

        public static List<int> BuildTradeSpeciesPool(FvxStartersStaticsTradesOptions opt, IReadOnlyList<PokemonEntry> pokemon)
        {
            var pool = new List<int>();
            for (int i = 1; i < pokemon.Count && i <= FvxGen5Constants.NationalDexCount; i++)
            {
                if (opt.DontUseLegendaries && FvxGen5StartersStaticsTradesRunner.IsLegendaryNationalDex(i)) continue;
                pool.Add(i);
            }
            bool bw2 = MainEditor.RomType == RomType.BW2;
            return FvxGlobalSpeciesPoolFilter.FilterPool(pool, opt.Global, bw2,
                MainEditor.evolutionsNarc?.evolutions, 25);
        }

        /// <summary>
        /// <c>WordSetPastTradePkmName</c> is 6 bytes; <c>WordSetPokeSpecies</c> is 5 bytes. We rewrite to load a species/name line
        /// into the vanilla string buffer (second halfword of the original command), then remove one trailing byte and fix VM jumps.
        /// Custom nicknames append new lines to the Pokémon name text file and use those indices.
        /// </summary>
        static RefByte[] ReplacePastTradeNameWithWordSetPokeSpecies(RefByte[] bytes, FvxStartersStaticsTradesOptions opt, Random rnd, IReadOnlyList<int> speciesPool)
        {
            if (bytes == null || bytes.Length < 8) return bytes;

            FvxCustomNamesSet custom = null;
            try { custom = FvxCustomNamesSet.ReadOrCreate(FvxCustomNamesSet.DefaultFilePath()); } catch { /* use null */ }
            var nickPick = BuildTradeNicknamePickList(custom);

            for (int guard = 0; guard < 512; guard++)
            {
                int pos = FindLastCommandPosition(bytes, CmdWordSetPastTradePkmName);
                if (pos < 0) break;

                var cmd = new ScriptCommand(bytes, pos);
                if (cmd.commandID != CmdWordSetPastTradePkmName || pos + cmd.ByteLength > bytes.Length)
                    break;

                int strBufHalf = HelperFunctions.ReadShort(bytes, pos + 4);
                int strBufByte = Math.Max(0, Math.Min(255, strBufHalf));

                int nameLineIdx = PickTradeDisplayNameIndex(rnd, speciesPool, nickPick);

                WriteWordSetPokeSpecies(bytes, pos, (byte)strBufByte, nameLineIdx);

                int removeAt = pos + 5;
                if (removeAt >= bytes.Length)
                    break;
                bytes = RemoveByteAt(bytes, removeAt);
                AdjustRelativeJumpsAfterByteRemoved(bytes, removeAt);
            }

            return bytes;
        }

        static List<string> BuildTradeNicknamePickList(FvxCustomNamesSet custom)
        {
            var list = new List<string>();
            if (custom?.PokemonNicknames == null) return list;
            foreach (var s in custom.PokemonNicknames)
            {
                if (string.IsNullOrWhiteSpace(s)) continue;
                var t = s.Trim();
                if (t.Length > MaxTradeNicknameChars)
                    t = t.Substring(0, MaxTradeNicknameChars);
                if (t.Length > 0 && !list.Contains(t))
                    list.Add(t);
            }
            return list;
        }

        static int PickTradeDisplayNameIndex(Random rnd, IReadOnlyList<int> speciesPool, List<string> nickPick)
        {
            if (nickPick.Count > 0)
            {
                string pick = nickPick[rnd.Next(nickPick.Count)];
                return AppendPokemonNameLine(pick);
            }
            return speciesPool[rnd.Next(speciesPool.Count)];
        }

        static int AppendPokemonNameLine(string text)
        {
            var tf = MainEditor.textNarc?.textFiles?[VersionConstants.PokemonNameTextFileID];
            if (tf?.text == null)
                return 1;
            tf.text.Add(text);
            return tf.text.Count - 1;
        }

        static void WriteWordSetPokeSpecies(RefByte[] bytes, int pos, byte strBufIdx, int speciesOrNameIndex)
        {
            ushort sp = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, speciesOrNameIndex));
            HelperFunctions.WriteShort(bytes, pos, CmdWordSetPokeSpecies);
            HelperFunctions.WriteByte(bytes, pos + 2, strBufIdx);
            HelperFunctions.WriteShort(bytes, pos + 3, sp);
        }

        static int FindLastCommandPosition(RefByte[] bytes, short commandId)
        {
            int found = -1;
            for (int pos = 0; pos < bytes.Length - 2; pos += 2)
            {
                short id = (short)HelperFunctions.ReadShort(bytes, pos);
                if (id != commandId) continue;
                var cmd = new ScriptCommand(bytes, pos);
                if (cmd.commandID == commandId && pos + cmd.ByteLength <= bytes.Length)
                    found = pos;
            }
            return found;
        }

        static RefByte[] RemoveByteAt(RefByte[] src, int index)
        {
            if (src == null || index < 0 || index >= src.Length) return src;
            var dst = new RefByte[src.Length - 1];
            for (int i = 0; i < index; i++)
                dst[i] = src[i];
            for (int i = index + 1; i < src.Length; i++)
                dst[i - 1] = src[i];
            return dst;
        }

        /// <summary>Update VM relative branch targets after deleting one byte at <paramref name="removedOldIndex"/> in the pre-deletion buffer.</summary>
        static void AdjustRelativeJumpsAfterByteRemoved(RefByte[] bytes, int removedOldIndex)
        {
            if (bytes == null || bytes.Length < 6) return;

            for (int pos = 0; pos < bytes.Length - 2; pos += 2)
            {
                short id = (short)HelperFunctions.ReadShort(bytes, pos);
                if (id != CmdVmJump && id != CmdVmJumpIf && id != CmdCallRoutine) continue;

                var cmd = new ScriptCommand(bytes, pos);
                if (cmd.commandID != id || pos + cmd.ByteLength > bytes.Length)
                    continue;

                int oldPos = pos < removedOldIndex ? pos : pos + 1;

                if (id == CmdVmJump && cmd.parameters != null && cmd.parameters.Length >= 1)
                {
                    int oldRel = cmd.parameters[0];
                    int oldAbs = oldPos + cmd.ByteLength + oldRel;
                    int newAbs = oldAbs > removedOldIndex ? oldAbs - 1 : oldAbs;
                    int newRel = newAbs - pos - cmd.ByteLength;
                    HelperFunctions.WriteInt(bytes, pos + 2, newRel);
                }
                else if (id == CmdVmJumpIf && cmd.parameters != null && cmd.parameters.Length >= 2)
                {
                    int pOff = 2 + CommandReference.commandList[CmdVmJumpIf].parameterBytes[0];
                    int oldRel = cmd.parameters[1];
                    int oldAbs = oldPos + cmd.ByteLength + oldRel;
                    int newAbs = oldAbs > removedOldIndex ? oldAbs - 1 : oldAbs;
                    int newRel = newAbs - pos - cmd.ByteLength;
                    HelperFunctions.WriteInt(bytes, pos + pOff, newRel);
                }
                else if (id == CmdCallRoutine && cmd.parameters != null && cmd.parameters.Length >= 1)
                {
                    int oldRel = cmd.parameters[0];
                    int oldAbs = oldPos + cmd.ByteLength + oldRel;
                    int newAbs = oldAbs > removedOldIndex ? oldAbs - 1 : oldAbs;
                    int newRel = newAbs - pos - cmd.ByteLength;
                    HelperFunctions.WriteInt(bytes, pos + 2, newRel);
                }
            }
        }

        public static void ApplyToBuffer(FvxStartersStaticsTradesOptions opt, Random rnd, IReadOnlyList<int> speciesPool, RefByte[] bytes)
        {
            if (bytes == null || bytes.Length < 8 || speciesPool.Count == 0)
                return;

            var events = new List<ScriptEvent>();
            for (int pos = 0; pos < bytes.Length - 2; pos += 2)
            {
                short id = (short)HelperFunctions.ReadShort(bytes, pos);
                if (id != CmdTradeCheck && id != CmdGivePokemonLike && id != CmdGivePokemon2Like && id != CmdGivePokemonNLike)
                    continue;

                var cmd = new ScriptCommand(bytes, pos);
                if (cmd.ByteLength < 2 || pos + cmd.ByteLength > bytes.Length)
                    continue;
                if (cmd.commandID != id)
                    continue;

                int kind =
                    id == CmdTradeCheck ? 0 :
                    id == CmdGivePokemon2Like ? 1 :
                    id == CmdGivePokemonLike ? 2 :
                    id == CmdGivePokemonNLike ? 3 : -1;
                if (kind >= 0)
                    events.Add(new ScriptEvent(pos, kind, cmd));
            }

            events.Sort((a, b) => a.Pos.CompareTo(b.Pos));

            int? pendingGivenSpecies = null;
            int lastTradePos = -1;

            foreach (var ev in events)
            {
                if (ev.Kind == 0)
                {
                    PatchTradeCheck(opt, rnd, speciesPool, bytes, ev);
                    lastTradePos = ev.Pos;
                    pendingGivenSpecies = HelperFunctions.ReadShort(bytes, ev.Pos + 4);
                }
                else if (pendingGivenSpecies != null && lastTradePos >= 0 && ev.Pos > lastTradePos
                    && ev.Pos - lastTradePos <= MaxBytesAfterTradeForGive
                    && (ev.Kind == 1 || ev.Kind == 2 || ev.Kind == 3))
                {
                    int sp = pendingGivenSpecies.Value;
                    TryWriteHalfwordParam(bytes, ev.Pos, ev.Cmd, 0, sp);

                    if (ev.Kind == 1 && opt.TradesRandomizeIvs)
                    {
                        for (int pi = 3; pi <= 8; pi++)
                            TryWriteHalfwordParam(bytes, ev.Pos, ev.Cmd, pi, rnd.Next(0, 32));
                    }
                    if (ev.Kind == 1 && opt.TradesRandomizeItems)
                        TryWriteHalfwordParam(bytes, ev.Pos, ev.Cmd, 2, rnd.Next(1, 640));

                    if (ev.Kind == 2 && opt.TradesRandomizeItems)
                        TryWriteHalfwordParam(bytes, ev.Pos, ev.Cmd, 2, rnd.Next(1, 640));

                    pendingGivenSpecies = null;
                }
            }
        }

        struct ScriptEvent
        {
            public readonly int Pos;
            public readonly int Kind;
            public readonly ScriptCommand Cmd;
            public ScriptEvent(int pos, int kind, ScriptCommand cmd)
            {
                Pos = pos;
                Kind = kind;
                Cmd = cmd;
            }
        }

        static void PatchTradeCheck(FvxStartersStaticsTradesOptions opt, Random rnd, IReadOnlyList<int> speciesPool, RefByte[] bytes, ScriptEvent ev)
        {
            switch (opt.TradesMode)
            {
                case FvxTradesRandomizationMode.RandomizeGivenOnly:
                    TryWriteHalfwordParam(bytes, ev.Pos, ev.Cmd, 1, speciesPool[rnd.Next(speciesPool.Count)]);
                    break;
                case FvxTradesRandomizationMode.RandomizeBoth:
                    TryWriteHalfwordParam(bytes, ev.Pos, ev.Cmd, 0, speciesPool[rnd.Next(speciesPool.Count)]);
                    TryWriteHalfwordParam(bytes, ev.Pos, ev.Cmd, 1, speciesPool[rnd.Next(speciesPool.Count)]);
                    break;
            }
        }

        static bool TryWriteHalfwordParam(RefByte[] bytes, int cmdOffset, ScriptCommand cmd, int paramIndex, int value)
        {
            if (!CommandReference.commandList.ContainsKey(cmd.commandID))
                return false;
            var pb = CommandReference.commandList[cmd.commandID].parameterBytes;
            if (paramIndex < 0 || paramIndex >= pb.Count || pb[paramIndex] != 2)
                return false;
            int pos = cmdOffset + 2;
            for (int i = 0; i < paramIndex; i++)
                pos += pb[i];
            if (pos + 1 >= bytes.Length)
                return false;
            HelperFunctions.WriteShort(bytes, pos, value);
            return true;
        }
    }
}
