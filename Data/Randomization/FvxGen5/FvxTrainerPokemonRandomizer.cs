using System;
using System.Collections.Generic;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public static class FvxTrainerPokemonRandomizer
    {
        public static void Apply(FvxFoePokemonSettings foe, Random rnd, TypeSystemContext ctx)
        {
            if (foe.TrainerPokemon == FvxTrainerPokemonMode.Unchanged) return;
            if (MainEditor.trainerPokeNarc?.pokemonGroups == null || MainEditor.pokemonDataNarc?.pokemon == null || rnd == null)
                return;

            var groups = MainEditor.trainerPokeNarc.pokemonGroups;
            var pkData = MainEditor.pokemonDataNarc.pokemon;
            int maxSpecies = Math.Min(FvxGen5Constants.NationalDexCount, pkData.Count - 1);

            foreach (var grp in groups)
            {
                if (grp?.pokemon == null) continue;
                foreach (var tp in grp.pokemon)
                {
                    short next = PickSpecies(foe, rnd, tp.pokemonID, maxSpecies, pkData);
                    tp.pokemonID = next;
                    tp.form = 0;
                }
                grp.ApplyData();
            }
        }

        static short PickSpecies(FvxFoePokemonSettings foe, Random rnd, short currentId, int maxSpecies, List<PokemonEntry> pkData)
        {
            int cur = Math.Max(0, Math.Min((int)currentId, pkData.Count - 1));
            int curBst = pkData[cur].baseStatTotal;

            for (int attempt = 0; attempt < 200; attempt++)
            {
                int s = rnd.Next(1, maxSpecies + 1);
                if (foe.DontUseLegendaries && IsLegendaryish(pkData, s)) continue;
                if (foe.TrainerPokemon == FvxTrainerPokemonMode.RandomSimilarStrength)
                {
                    int bst = pkData[s].baseStatTotal;
                    int w = Math.Max(5, foe.SimilarStrengthWindowPercent);
                    if (Math.Abs(bst - curBst) > curBst * w / 100) continue;
                }
                return (short)s;
            }
            return (short)rnd.Next(1, maxSpecies + 1);
        }

        static bool IsLegendaryish(List<PokemonEntry> pkData, int id)
        {
            if (id < 0 || id >= pkData.Count) return false;
            return pkData[id].baseStatTotal >= 600;
        }
    }
}
