using System;
using System.Collections.Generic;
using NewEditor.Data.NARCTypes;
using NewEditor.Data.Randomization.GeneShuffle;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public static class FvxPokemonTraitsRandomizer
    {
        public static void Apply(FvxTraitsSettings traits, Random rnd, TypeSystemContext ctx)
        {
            if (MainEditor.pokemonDataNarc?.pokemon == null || MainEditor.evolutionsNarc?.evolutions == null || rnd == null || ctx == null)
                return;

            var pokemon = MainEditor.pokemonDataNarc.pokemon;
            var evo = MainEditor.evolutionsNarc.evolutions;

            if (traits.BaseStats != FvxBaseStatsMode.Unchanged)
                ApplyBaseStats(traits, rnd, pokemon, evo);

            if (traits.Types != FvxTraitsTypeMode.Unchanged)
            {
                GeneShuffleTypeMode mode = traits.Types == FvxTraitsTypeMode.RandomFollowEvolutions
                    ? GeneShuffleTypeMode.FollowingEvolution
                    : GeneShuffleTypeMode.FullRandom;
                TypeGeneRandomizer.Apply(mode, rnd, ctx.MaxTypeIndexInclusive, pokemon, evo);

                if (traits.ForceDualTypes)
                {
                    foreach (var pk in pokemon)
                    {
                        if (pk == null) continue;
                        if (pk.type2 == pk.type1 || pk.type2 == 255)
                        {
                            byte t2;
                            do { t2 = ctx.RandomType(rnd); } while (t2 == pk.type1);
                            pk.type2 = t2;
                        }
                        pk.ApplyData();
                    }
                }
            }

            if (traits.Abilities == FvxAbilitiesMode.Random)
                ApplyAbilities(traits, rnd, pokemon, evo);
        }

        static void ApplyBaseStats(FvxTraitsSettings traits, Random rnd, List<PokemonEntry> pokemon, IReadOnlyList<EvolutionDataEntry> evo)
        {
            bool follow = traits.FollowEvolutionsStats;
            var families = EvolutionFamilyIndex.BuildFamilies(pokemon.Count, evo);

            if (!follow)
            {
                foreach (var pk in pokemon)
                {
                    if (pk == null) continue;
                    RandomizeOneStats(pk, traits.BaseStats, rnd);
                    pk.ApplyData();
                }
                return;
            }

            foreach (var fam in families)
            {
                if (fam.Count == 0) continue;
                var src = pokemon[fam[0]];
                byte hp = src.baseHP, atk = src.baseAttack, def = src.baseDefense;
                byte spd = src.baseSpeed, spa = src.baseSpAtt, sdef = src.baseSpDef;
                var vals = new[] { hp, atk, def, spd, spa, sdef };

                if (traits.BaseStats == FvxBaseStatsMode.Shuffle)
                    Shuffle(vals, rnd);
                else
                    for (int i = 0; i < vals.Length; i++) vals[i] = (byte)rnd.Next(25, 151);

                foreach (int idx in fam)
                {
                    if (idx < 0 || idx >= pokemon.Count) continue;
                    var pk = pokemon[idx];
                    pk.baseHP = vals[0];
                    pk.baseAttack = vals[1];
                    pk.baseDefense = vals[2];
                    pk.baseSpeed = vals[3];
                    pk.baseSpAtt = vals[4];
                    pk.baseSpDef = vals[5];
                    pk.ApplyData();
                }
            }
        }

        static void RandomizeOneStats(PokemonEntry pk, FvxBaseStatsMode mode, Random rnd)
        {
            var vals = new[] { pk.baseHP, pk.baseAttack, pk.baseDefense, pk.baseSpeed, pk.baseSpAtt, pk.baseSpDef };
            if (mode == FvxBaseStatsMode.Shuffle)
                Shuffle(vals, rnd);
            else
                for (int i = 0; i < vals.Length; i++) vals[i] = (byte)rnd.Next(25, 151);
            pk.baseHP = vals[0];
            pk.baseAttack = vals[1];
            pk.baseDefense = vals[2];
            pk.baseSpeed = vals[3];
            pk.baseSpAtt = vals[4];
            pk.baseSpDef = vals[5];
        }

        static void Shuffle(byte[] arr, Random rnd)
        {
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                byte t = arr[i];
                arr[i] = arr[j];
                arr[j] = t;
            }
        }

        static void ApplyAbilities(FvxTraitsSettings traits, Random rnd, List<PokemonEntry> pokemon, IReadOnlyList<EvolutionDataEntry> evo)
        {
            const int maxAbility = 164;
            if (!traits.FollowEvolutionsAbilities)
            {
                foreach (var pk in pokemon)
                {
                    if (pk == null) continue;
                    if (!traits.AllowWonderGuard)
                    {
                        pk.ability1 = RandomAbility(rnd, maxAbility, excludeWonderGuard: true);
                        pk.ability2 = RandomAbility(rnd, maxAbility, excludeWonderGuard: true);
                        pk.ability3 = RandomAbility(rnd, maxAbility, excludeWonderGuard: true);
                    }
                    else
                    {
                        pk.ability1 = (byte)rnd.Next(0, maxAbility + 1);
                        pk.ability2 = (byte)rnd.Next(0, maxAbility + 1);
                        pk.ability3 = (byte)rnd.Next(0, maxAbility + 1);
                    }
                    pk.ApplyData();
                }
                return;
            }

            var families = EvolutionFamilyIndex.BuildFamilies(pokemon.Count, evo);
            foreach (var fam in families)
            {
                byte a1 = RandomAbility(rnd, maxAbility, !traits.AllowWonderGuard);
                byte a2 = RandomAbility(rnd, maxAbility, !traits.AllowWonderGuard);
                byte a3 = RandomAbility(rnd, maxAbility, !traits.AllowWonderGuard);
                foreach (int idx in fam)
                {
                    if (idx < 0 || idx >= pokemon.Count) continue;
                    var pk = pokemon[idx];
                    pk.ability1 = a1;
                    pk.ability2 = a2;
                    pk.ability3 = a3;
                    pk.ApplyData();
                }
            }
        }

        static byte RandomAbility(Random rnd, int maxAbility, bool excludeWonderGuard)
        {
            for (int t = 0; t < 50; t++)
            {
                byte v = (byte)rnd.Next(0, maxAbility + 1);
                if (excludeWonderGuard && v == 25) continue;
                return v;
            }
            return (byte)rnd.Next(0, maxAbility + 1);
        }
    }
}
