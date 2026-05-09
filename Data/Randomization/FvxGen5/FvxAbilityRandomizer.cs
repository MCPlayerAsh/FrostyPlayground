using System;
using System.Collections.Generic;
using NewEditor.Data.NARCTypes;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// FVX SpeciesAbilityRandomizer port: re-rolls ability1 / ability2 (slot 3 / hidden ability untouched).
    /// </summary>
    public static class FvxAbilityRandomizer
    {
        public static void Apply(FvxPokemonTraitsOptions opt, Random rnd, List<PokemonEntry> pokemon, IReadOnlyList<EvolutionDataEntry> evolutions)
        {
            if (opt == null || pokemon == null || pokemon.Count == 0) return;
            if (opt.AbilitiesMod == FvxAbilitiesMod.Unchanged) return;

            var pool = FvxGen5AbilityBanLists.BuildAllowedAbilityPool(opt);

            void RollOne(PokemonEntry pk)
            {
                if (pk == null) return;
                int a1 = PickAbility(pool, rnd);
                int a2 = PickAbility(pool, rnd);
                ApplyRolledPair(pk, a1, a2, opt, pool, rnd);
            }

            if (opt.AbilitiesFollowEvolutions && evolutions != null)
            {
                var graph = FvxGen5EvolutionGraph.FromEvolutions(evolutions);

                Action<int> basicAction = i =>
                {
                    if (i < 0 || i >= pokemon.Count) return;
                    RollOne(pokemon[i]);
                };

                Action<int, int, bool> evolvedAction = (from, to, _) =>
                {
                    if (from < 0 || from >= pokemon.Count || to < 0 || to >= pokemon.Count) return;
                    var src = pokemon[from];
                    var dst = pokemon[to];
                    if (src == null || dst == null) return;
                    dst.ability1 = src.ability1;
                    dst.ability2 = src.ability2;
                    dst.ApplyData();
                };

                graph.ApplyCopyUp(basicAction, evolvedAction);
            }
            else
            {
                foreach (var pk in pokemon) RollOne(pk);
            }

            foreach (var pk in pokemon) pk?.ApplyData();
        }

        static int PickAbility(List<int> pool, Random rnd)
        {
            return pool[rnd.Next(pool.Count)];
        }

        static void ApplyRolledPair(PokemonEntry pk, int a1, int a2, FvxPokemonTraitsOptions opt, List<int> pool, Random rnd)
        {
            bool hadTwoOriginally = pk.ability1 != FvxGen5AbilityBanLists.NoneAbility
                                  && pk.ability2 != FvxGen5AbilityBanLists.NoneAbility
                                  && pk.ability1 != pk.ability2;

            if (opt.AbilitiesCombineDuplicates && a1 == a2)
            {
                pk.ability1 = (byte)a1;
                pk.ability2 = (byte)a1;
                return;
            }

            if (opt.AbilitiesEnsureTwo)
            {
                int safety = 64;
                while (a2 == a1 && safety-- > 0 && pool.Count > 1)
                    a2 = PickAbility(pool, rnd);
                pk.ability1 = (byte)a1;
                pk.ability2 = (byte)a2;
                return;
            }

            // Default behavior: respect whether the original species had 1 or 2 abilities.
            pk.ability1 = (byte)a1;
            if (hadTwoOriginally) pk.ability2 = (byte)a2;
            else pk.ability2 = (byte)(opt.AbilitiesCombineDuplicates ? a1 : a1);
        }
    }
}
