using System;
using System.Collections.Generic;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public static class FvxWildEncounterRandomizer
    {
        public static void Apply(FvxWildPokemonSettings wild, Random rnd, TypeSystemContext ctx)
        {
            if (wild.WildMode == FvxWildMode.Unchanged) return;
            if (MainEditor.encounterNarc?.mainEncounterPools == null || MainEditor.pokemonDataNarc?.pokemon == null || rnd == null)
                return;

            var pkData = MainEditor.pokemonDataNarc.pokemon;
            int maxSpecies = Math.Min(FvxGen5Constants.NationalDexCount, pkData.Count - 1);

            foreach (var pool in MainEditor.encounterNarc.mainEncounterPools)
                RandomizePoolEntry(pool, wild, rnd, maxSpecies, pkData);
            foreach (var pool in MainEditor.encounterNarc.subEncounterPools)
                RandomizePoolEntry(pool, wild, rnd, maxSpecies, pkData);
        }

        static void RandomizePoolEntry(EncounterEntry pool, FvxWildPokemonSettings wild, Random rnd, int maxSpecies, List<PokemonEntry> pkData)
        {
            if (pool == null) return;

            void DoSlots(EncounterSlot[] arr)
            {
                if (arr == null) return;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] == null) continue;
                    int s = PickSpecies(wild, rnd, maxSpecies, pkData);
                    arr[i].pokemonID = (short)s;
                    arr[i].pokemonForm = 0;
                    if (wild.UseLevelModifier && wild.LevelModifierPercent != 0)
                    {
                        int delta = wild.LevelModifierPercent;
                        int ml = arr[i].minLevel + arr[i].minLevel * delta / 100;
                        int xl = arr[i].maxLevel + arr[i].maxLevel * delta / 100;
                        ml = Math.Max(1, Math.Min(100, ml));
                        xl = Math.Max(ml, Math.Min(100, xl));
                        arr[i].minLevel = (byte)ml;
                        arr[i].maxLevel = (byte)xl;
                    }
                }
            }

            if (pool.landSlots != null)
                foreach (var arr in pool.landSlots) DoSlots(arr);
            if (pool.waterSlots != null)
                foreach (var arr in pool.waterSlots) DoSlots(arr);

            pool.EncounterSlotsToGroups();
            pool.ApplyData();
        }

        static int PickSpecies(FvxWildPokemonSettings wild, Random rnd, int maxSpecies, List<PokemonEntry> pkData)
        {
            for (int t = 0; t < 100; t++)
            {
                int s = rnd.Next(1, maxSpecies + 1);
                if (wild.DontUseLegendaries && s < pkData.Count && pkData[s].baseStatTotal >= 600)
                    continue;
                return s;
            }
            return rnd.Next(1, maxSpecies + 1);
        }
    }
}
