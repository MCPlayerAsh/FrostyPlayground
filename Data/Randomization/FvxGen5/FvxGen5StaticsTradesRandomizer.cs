using System;
using System.Text;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Static encounter script patches (BW2 US) and in-game trade script species (BW1 US, UPR TradeScript[]).</summary>
    internal static class FvxGen5StaticsTradesRandomizer
    {
        sealed class TradeScriptSpec
        {
            public readonly int File;
            public readonly int[] RequestedOffsets;
            public readonly int[] GivenOffsets;

            public TradeScriptSpec(int file, int[] req, int[] giv)
            {
                File = file;
                RequestedOffsets = req;
                GivenOffsets = giv;
            }
        }

        /// <summary>UPR [Black (U)] TradeScript[] entries (file : requested : given).</summary>
        static readonly TradeScriptSpec[] Bw1UsTradeScripts =
        {
            new TradeScriptSpec(46, new[] { 0x81, 0x97 }, new[] { 0x86, 0x9C }),
            new TradeScriptSpec(202, new[] { 0x224 }, new[] { 0x21F }),
            new TradeScriptSpec(686, new[] { 0x76 }, new[] { 0x71 }),
            new TradeScriptSpec(830, new[] { 0xB3, 0xEA, 0x114 }, new[] { 0xAE, 0xE5, 0x10F }),
            new TradeScriptSpec(764, new[] { 0x43 }, new[] { 0x3E }),
        };

        public static void ApplyStatics(FvxStartersStaticsTradesSettings s, FvxFoePokemonSettings foeRules, Random rnd, StringBuilder log)
        {
            if (s.StaticsMode == FvxTrainerPokemonMode.Unchanged) return;
            if (MainEditor.RomType != RomType.BW2 || !FvxGen5UsRomTables.IsSupportedUsRom(MainEditor.RomTypeId))
            {
                log.AppendLine("[Statics] Implemented for US Black 2 / White 2 only — skipped.");
                return;
            }

            if (MainEditor.scriptNarc?.scriptFiles == null || MainEditor.pokemonDataNarc?.pokemon == null)
            {
                log.AppendLine("[Statics] Script or Pokémon data not loaded.");
                return;
            }

            var pk = MainEditor.pokemonDataNarc.pokemon;
            int maxS = Math.Min(FvxGen5Constants.NationalDexCount, pk.Count - 1);
            int patched = 0;

            foreach (var (file, offset) in FvxGen5Bw2UsStaticSpeciesSites.Sites)
            {
                if (file < 0 || file >= MainEditor.scriptNarc.scriptFiles.Count) continue;
                var sf = MainEditor.scriptNarc.scriptFiles[file];
                if (sf?.bytes == null || offset < 0 || offset + 1 >= sf.bytes.Length) continue;

                short cur = (short)HelperFunctions.ReadShort(sf.bytes, offset);
                short next = FvxTrainerPokemonRandomizer.PickReplacementSpecies(
                    s.StaticsMode, foeRules.DontUseLegendaries, foeRules.SimilarStrengthWindowPercent,
                    rnd, cur, maxS, pk);
                sf.bytes[offset] = (byte)(next & 0xFF);
                sf.bytes[offset + 1] = (byte)((next >> 8) & 0xFF);
                patched++;
            }

            log.AppendLine("[Statics] Patched " + patched + " species halfwords (" + s.StaticsMode + ").");
        }

        public static void ApplyTrades(FvxTradesMode mode, FvxFoePokemonSettings foeRules, Random rnd, StringBuilder log)
        {
            if (mode == FvxTradesMode.Unchanged) return;
            if (MainEditor.RomType != RomType.BW1 || !FvxGen5UsRomTables.IsSupportedUsRom(MainEditor.RomTypeId))
            {
                log.AppendLine("[Trades] Trade script offsets are embedded for US Black/White only — skipped.");
                return;
            }

            if (MainEditor.scriptNarc?.scriptFiles == null || MainEditor.pokemonDataNarc?.pokemon == null)
            {
                log.AppendLine("[Trades] Script or Pokémon data not loaded.");
                return;
            }

            var pk = MainEditor.pokemonDataNarc.pokemon;
            int maxS = Math.Min(FvxGen5Constants.NationalDexCount, pk.Count - 1);
            int patched = 0;

            foreach (var spec in Bw1UsTradeScripts)
            {
                if (spec.File < 0 || spec.File >= MainEditor.scriptNarc.scriptFiles.Count) continue;
                var sf = MainEditor.scriptNarc.scriptFiles[spec.File];
                if (sf?.bytes == null) continue;

                for (int i = 0; i < spec.RequestedOffsets.Length; i++)
                {
                    int oReq = spec.RequestedOffsets[i];
                    int oGiv = spec.GivenOffsets[i];
                    if (oReq < 0 || oReq + 1 >= sf.bytes.Length || oGiv < 0 || oGiv + 1 >= sf.bytes.Length) continue;

                    short req = (short)HelperFunctions.ReadShort(sf.bytes, oReq);
                    short giv = (short)HelperFunctions.ReadShort(sf.bytes, oGiv);

                    short newReq = req;
                    short newGiv = giv;

                    if (mode == FvxTradesMode.RandomizeBoth)
                    {
                        newReq = FvxTrainerPokemonRandomizer.PickReplacementSpecies(
                            FvxTrainerPokemonMode.RandomCompletely, foeRules.DontUseLegendaries,
                            foeRules.SimilarStrengthWindowPercent, rnd, req, maxS, pk);
                        newGiv = FvxTrainerPokemonRandomizer.PickReplacementSpecies(
                            FvxTrainerPokemonMode.RandomCompletely, foeRules.DontUseLegendaries,
                            foeRules.SimilarStrengthWindowPercent, rnd, giv, maxS, pk);
                    }
                    else if (mode == FvxTradesMode.RandomizeGivenOnly)
                    {
                        newGiv = FvxTrainerPokemonRandomizer.PickReplacementSpecies(
                            FvxTrainerPokemonMode.RandomCompletely, foeRules.DontUseLegendaries,
                            foeRules.SimilarStrengthWindowPercent, rnd, giv, maxS, pk);
                    }

                    WriteHalf(sf.bytes, oReq, newReq);
                    WriteHalf(sf.bytes, oGiv, newGiv);
                    patched++;
                }
            }

            log.AppendLine("[Trades] Updated " + patched + " script species pair(s) (" + mode + "). In-game trade NARC text/stats are unchanged in this editor.");
        }

        static void WriteHalf(RefByte[] bytes, int offset, short value)
        {
            bytes[offset] = (byte)(value & 0xFF);
            bytes[offset + 1] = (byte)((value >> 8) & 0xFF);
        }
    }
}
