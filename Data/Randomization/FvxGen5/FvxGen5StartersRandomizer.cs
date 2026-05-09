using System;
using System.Collections.Generic;
using System.Text;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    internal static class FvxGen5StartersRandomizer
    {
        public static void Apply(FvxStartersStaticsTradesSettings s, Random rnd, StringBuilder log)
        {
            if (s.StartersMode == FvxStartersMode.Unchanged) return;
            if (s.StartersMode == FvxStartersMode.Custom)
            {
                log.AppendLine("[Starters] Custom starters require species picker — skipped.");
                return;
            }

            if (!FvxGen5UsRomTables.IsSupportedUsRom(MainEditor.RomTypeId))
            {
                log.AppendLine("[Starters] Starter script offsets are defined for US Pokémon B/W/B2/W2 only — skipped.");
                return;
            }

            if (MainEditor.scriptNarc?.scriptFiles == null || MainEditor.pokemonDataNarc?.pokemon == null)
            {
                log.AppendLine("[Starters] Script or Pokémon data not loaded.");
                return;
            }

            if (!FvxGen5UsRomTables.TryGetStarterWriteSites(MainEditor.RomTypeId, out var sites))
            {
                log.AppendLine("[Starters] Could not resolve starter write sites.");
                return;
            }

            var pk = MainEditor.pokemonDataNarc.pokemon;
            int maxS = Math.Min(FvxGen5Constants.NationalDexCount, pk.Count - 1);

            var picks = PickThreeSpecies(s.StartersMode, rnd, pk, maxS);
            if (picks == null)
            {
                log.AppendLine("[Starters] Could not pick starter species.");
                return;
            }

            for (int st = 0; st < 3; st++)
            {
                int species = picks[st];
                foreach (var (narcIndex, offset) in sites[st])
                {
                    if (narcIndex < 0 || narcIndex >= MainEditor.scriptNarc.scriptFiles.Count) continue;
                    var sf = MainEditor.scriptNarc.scriptFiles[narcIndex];
                    if (sf?.bytes == null || offset < 0 || offset + 1 >= sf.bytes.Length) continue;
                    sf.bytes[offset] = (byte)(species & 0xFF);
                    sf.bytes[offset + 1] = (byte)((species >> 8) & 0xFF);
                }
            }

            log.AppendLine("[Starters] Wrote species " + picks[0] + " / " + picks[1] + " / " + picks[2] + " (" + s.StartersMode + ").");
        }

        static int[] PickThreeSpecies(FvxStartersMode mode, Random rnd, List<PokemonEntry> pk, int maxS)
        {
            bool Ok(int s)
            {
                if (s < 1 || s > maxS) return false;
                if (pk[s].baseStatTotal >= 600) return false;
                if (mode == FvxStartersMode.RandomCompletely) return true;
                int bst = pk[s].baseStatTotal;
                if (mode == FvxStartersMode.RandomBasicThreeStage && bst > 330) return false;
                if (mode == FvxStartersMode.RandomAnyBasic && bst > 420) return false;
                return true;
            }

            for (int attempt = 0; attempt < 600; attempt++)
            {
                int a = rnd.Next(1, maxS + 1);
                int b = rnd.Next(1, maxS + 1);
                int c = rnd.Next(1, maxS + 1);
                if (a == b || a == c || b == c) continue;
                if (!Ok(a) || !Ok(b) || !Ok(c)) continue;
                return new[] { a, b, c };
            }

            return null;
        }
    }
}
