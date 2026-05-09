using System;
using System.Collections.Generic;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;

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

        const int MaxBytesAfterTradeForGive = 4500;

        public static void ApplyToNarc(FvxStartersStaticsTradesOptions opt, Random rnd, IReadOnlyList<int> speciesPool, ScriptNARC narc, bool blackVersion = true)
        {
            if (narc?.scriptFiles == null || speciesPool == null || speciesPool.Count == 0) return;
            foreach (int fileId in FvxGen5RomLayout.TradeScriptFileIds(blackVersion))
            {
                if (fileId < 0 || fileId >= narc.scriptFiles.Count) continue;
                ApplyToBuffer(opt, rnd, speciesPool, narc.scriptFiles[fileId].bytes);
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
            return pool;
        }

        public static void ApplyToBuffer(FvxStartersStaticsTradesOptions opt, Random rnd, IReadOnlyList<int> speciesPool, RefByte[] bytes)
        {
            if (bytes == null || bytes.Length < 8 || speciesPool.Count == 0)
                return;

            var events = new List<ScriptEvent>();
            for (int pos = 0; pos < bytes.Length - 2; pos += 2)
            {
                short id = (short)HelperFunctions.ReadShort(bytes, pos);
                if (id != CmdTradeCheck && id != CmdGivePokemonLike && id != CmdGivePokemon2Like && id != CmdGivePokemonNLike
                    && id != CmdWordSetPastTradePkmName)
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
                    id == CmdGivePokemonNLike ? 3 :
                    id == CmdWordSetPastTradePkmName ? 4 : -1;
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
                else if (ev.Kind == 4 && opt.TradesRandomizeNicknames)
                {
                    int labelIdx = speciesPool[rnd.Next(speciesPool.Count)];
                    TryWriteHalfwordParam(bytes, ev.Pos, ev.Cmd, 1, labelIdx);
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
