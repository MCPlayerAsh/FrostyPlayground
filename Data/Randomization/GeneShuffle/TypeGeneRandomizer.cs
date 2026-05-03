using System;
using System.Collections.Generic;
using System.Linq;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.GeneShuffle
{
    public static class TypeGeneRandomizer
    {
        public static void Apply(GeneShuffleTypeMode mode, Random rnd, int maxTypeInclusive, List<PokemonEntry> pokemon, IReadOnlyList<EvolutionDataEntry> evolutions)
        {
            if (pokemon == null || pokemon.Count == 0) return;
            int maxT = System.Math.Max(0, maxTypeInclusive);
            int speciesCount = pokemon.Count;

            byte PickType(Random r) => (byte)r.Next(0, maxT + 1);

            switch (mode)
            {
                case GeneShuffleTypeMode.FullRandom:
                    foreach (var pk in pokemon)
                    {
                        if (pk == null) continue;
                        pk.type1 = PickType(rnd);
                        pk.type2 = PickType(rnd);
                        pk.ApplyData();
                    }
                    break;

                case GeneShuffleTypeMode.FollowingEvolution:
                    {
                        var families = EvolutionFamilyIndex.BuildFamilies(speciesCount, evolutions);
                        foreach (var fam in families)
                        {
                            byte t1 = PickType(rnd);
                            byte t2 = PickType(rnd);
                            foreach (int idx in fam)
                            {
                                if (idx < 0 || idx >= pokemon.Count) continue;
                                var pk = pokemon[idx];
                                if (pk == null) continue;
                                pk.type1 = t1;
                                pk.type2 = t2;
                                pk.ApplyData();
                            }
                        }
                    }
                    break;

                case GeneShuffleTypeMode.VanillaTypeLogic:
                    {
                        var families = EvolutionFamilyIndex.BuildFamilies(speciesCount, evolutions);
                        foreach (var fam in families)
                        {
                            var distinct = new HashSet<byte>();
                            foreach (int idx in fam)
                            {
                                if (idx < 0 || idx >= pokemon.Count) continue;
                                var pk = pokemon[idx];
                                if (pk == null) continue;
                                distinct.Add(pk.type1);
                                if (pk.type2 != pk.type1 && pk.type2 != 255)
                                    distinct.Add(pk.type2);
                            }

                            var dList = distinct.ToList();
                            Shuffle(dList, rnd);

                            var targets = DrawDistinctTypes(dList.Count, maxT, rnd);
                            Shuffle(targets, rnd);

                            var map = new Dictionary<byte, byte>();
                            for (int i = 0; i < dList.Count && i < targets.Count; i++)
                                map[dList[i]] = targets[i];

                            foreach (int idx in fam)
                            {
                                if (idx < 0 || idx >= pokemon.Count) continue;
                                var pk = pokemon[idx];
                                if (pk == null) continue;
                                byte o1 = pk.type1;
                                byte o2 = pk.type2;
                                pk.type1 = map.TryGetValue(o1, out var n1) ? n1 : PickType(rnd);
                                if (o2 == 255 || o2 == o1)
                                    pk.type2 = pk.type1;
                                else
                                    pk.type2 = map.TryGetValue(o2, out var n2) ? n2 : PickType(rnd);
                                pk.ApplyData();
                            }
                        }
                    }
                    break;
            }
        }

        static List<byte> DrawDistinctTypes(int count, int maxTypeInclusive, Random rnd)
        {
            var pool = new List<byte>();
            for (int t = 0; t <= maxTypeInclusive; t++) pool.Add((byte)t);
            Shuffle(pool, rnd);
            if (count > pool.Count) count = pool.Count;
            return pool.GetRange(0, count);
        }

        static void Shuffle<T>(IList<T> list, Random rnd)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }
}
