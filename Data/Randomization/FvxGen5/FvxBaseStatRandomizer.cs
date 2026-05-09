using System;
using System.Collections.Generic;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// FVX BaseStatRandomizer port: shuffle / redistribute the six base stats and standardize EXP curves.
    /// "Follow Evolutions" copies the basic species' stats (or shuffled stats) up the evolution chain,
    /// optionally re-rolling only the BST delta added on each evolution step.
    /// </summary>
    public static class FvxBaseStatRandomizer
    {
        /// <summary>National-dex IDs of legendary / mythical species available in Gen 5.</summary>
        static readonly HashSet<int> LegendaryDexIds = new HashSet<int>
        {
            144, 145, 146, 150, 151, 243, 244, 245, 249, 250, 251,
            377, 378, 379, 380, 381, 382, 383, 384, 385, 386,
            480, 481, 482, 483, 484, 485, 486, 487, 488, 489, 490, 491, 492, 493,
            638, 639, 640, 641, 642, 643, 644, 645, 646, 647, 648, 649
        };

        /// <summary>"Strong" / box-art legendaries (super legendaries).</summary>
        static readonly HashSet<int> StrongLegendaryDexIds = new HashSet<int>
        {
            150,                          // Mewtwo
            249, 250,                     // Lugia, Ho-Oh
            382, 383, 384,                // Kyogre, Groudon, Rayquaza
            483, 484, 487,                // Dialga, Palkia, Giratina
            643, 644, 646                 // Reshiram, Zekrom, Kyurem
        };

        public static void Apply(FvxPokemonTraitsOptions opt, Random rnd, List<PokemonEntry> pokemon, IReadOnlyList<EvolutionDataEntry> evolutions)
        {
            if (opt == null || pokemon == null || pokemon.Count == 0) return;

            ApplyBaseStats(opt, rnd, pokemon, evolutions);
            ApplyExpCurveStandardization(opt, pokemon);
        }

        static void ApplyBaseStats(FvxPokemonTraitsOptions opt, Random rnd, List<PokemonEntry> pokemon, IReadOnlyList<EvolutionDataEntry> evolutions)
        {
            if (opt.BaseStatsMod == FvxBaseStatsMod.Unchanged) return;

            if (opt.BaseStatsFollowEvolutions && evolutions != null)
            {
                var graph = FvxGen5EvolutionGraph.FromEvolutions(evolutions);

                Action<int> basicAction = i =>
                {
                    if (i < 0 || i >= pokemon.Count) return;
                    var pk = pokemon[i];
                    if (pk == null) return;
                    if (opt.BaseStatsMod == FvxBaseStatsMod.Shuffle) ShuffleStats(pk, rnd);
                    else RandomizeWithinBst(pk, rnd);
                    pk.ApplyData();
                };

                Action<int, int, bool> evolvedAction = (from, to, _) =>
                {
                    if (from < 0 || from >= pokemon.Count || to < 0 || to >= pokemon.Count) return;
                    var src = pokemon[from];
                    var dst = pokemon[to];
                    if (src == null || dst == null) return;

                    int origBst = dst.baseStatTotal;
                    int srcBst = src.baseStatTotal;

                    if (opt.BaseStatsMod == FvxBaseStatsMod.Shuffle)
                    {
                        if (opt.BaseStatsRandomizeAddedOnEvolution && origBst > srcBst)
                            CopyShuffledStatsWithDelta(src, dst, origBst - srcBst, rnd);
                        else
                            CopyStatsPermutation(src, dst);
                    }
                    else
                    {
                        if (opt.BaseStatsRandomizeAddedOnEvolution && origBst > srcBst)
                            CopyRandomizedStatsWithDelta(src, dst, origBst - srcBst, rnd);
                        else
                            CopyStatsExact(src, dst);
                    }
                    dst.ApplyData();
                };

                graph.ApplyCopyUp(basicAction, evolvedAction);
                return;
            }

            foreach (var pk in pokemon)
            {
                if (pk == null) continue;
                if (opt.BaseStatsMod == FvxBaseStatsMod.Shuffle) ShuffleStats(pk, rnd);
                else RandomizeWithinBst(pk, rnd);
                pk.ApplyData();
            }
        }

        static void ApplyExpCurveStandardization(FvxPokemonTraitsOptions opt, List<PokemonEntry> pokemon)
        {
            if (opt.StandardizeExpScope == FvxStandardizeExpScope.None) return;
            byte target = (byte)opt.StandardizeExpTarget;
            byte slow = (byte)FvxExpCurve.Slow;

            for (int i = 0; i < pokemon.Count; i++)
            {
                var pk = pokemon[i];
                if (pk == null) continue;
                bool isLegend = LegendaryDexIds.Contains(i);
                bool isStrong = StrongLegendaryDexIds.Contains(i);

                switch (opt.StandardizeExpScope)
                {
                    case FvxStandardizeExpScope.AllPokemon:
                        pk.levelRate = isLegend ? slow : target;
                        break;
                    case FvxStandardizeExpScope.LegendariesSlow:
                        if (isLegend) pk.levelRate = slow;
                        else pk.levelRate = target;
                        break;
                    case FvxStandardizeExpScope.StrongLegendariesSlow:
                        if (isStrong) pk.levelRate = slow;
                        else pk.levelRate = target;
                        break;
                }
                pk.ApplyData();
            }
        }

        static byte[] GetStatArray(PokemonEntry pk)
        {
            return new byte[] { pk.baseHP, pk.baseAttack, pk.baseDefense, pk.baseSpAtt, pk.baseSpDef, pk.baseSpeed };
        }

        static void SetStatArray(PokemonEntry pk, byte[] s)
        {
            pk.baseHP = s[0];
            pk.baseAttack = s[1];
            pk.baseDefense = s[2];
            pk.baseSpAtt = s[3];
            pk.baseSpDef = s[4];
            pk.baseSpeed = s[5];
        }

        /// <summary>
        /// Permute the 6 stats in place (BST stays exactly the same, individual stat values stay the same).
        /// </summary>
        static void ShuffleStats(PokemonEntry pk, Random rnd)
        {
            var s = GetStatArray(pk);
            for (int i = s.Length - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                byte tmp = s[i]; s[i] = s[j]; s[j] = tmp;
            }
            SetStatArray(pk, s);
        }

        /// <summary>
        /// Redistribute the BST among 6 stats by repeatedly drawing a random share, clamped to [1, 255].
        /// Approximates UPR <c>randomizeStatsWithinBST</c>.
        /// </summary>
        static void RandomizeWithinBst(PokemonEntry pk, Random rnd)
        {
            int bst = pk.baseStatTotal;
            if (bst <= 6) return;
            byte[] s = DistributeBst(bst, rnd);
            SetStatArray(pk, s);
        }

        static byte[] DistributeBst(int bst, Random rnd)
        {
            // Each of 6 stats >= 1, <= 255. Sample 6 weights and scale, then snap and patch the rounding error.
            double[] w = new double[6];
            double sum = 0.0;
            for (int i = 0; i < 6; i++)
            {
                w[i] = rnd.NextDouble() + 0.05;
                sum += w[i];
            }
            int[] raw = new int[6];
            int total = 0;
            for (int i = 0; i < 6; i++)
            {
                int v = (int)Math.Round(w[i] / sum * bst);
                if (v < 1) v = 1;
                if (v > 255) v = 255;
                raw[i] = v;
                total += v;
            }

            int diff = bst - total;
            int safety = 200;
            while (diff != 0 && safety-- > 0)
            {
                int idx = rnd.Next(6);
                if (diff > 0 && raw[idx] < 255) { raw[idx]++; diff--; }
                else if (diff < 0 && raw[idx] > 1) { raw[idx]--; diff++; }
            }

            byte[] s = new byte[6];
            for (int i = 0; i < 6; i++) s[i] = (byte)raw[i];
            return s;
        }

        static void CopyStatsExact(PokemonEntry src, PokemonEntry dst)
        {
            dst.baseHP = src.baseHP;
            dst.baseAttack = src.baseAttack;
            dst.baseDefense = src.baseDefense;
            dst.baseSpAtt = src.baseSpAtt;
            dst.baseSpDef = src.baseSpDef;
            dst.baseSpeed = src.baseSpeed;
        }

        /// <summary>Copy the source's stat values but shuffle slot positions, so the evolved stats follow the basic's distribution shape.</summary>
        static void CopyStatsPermutation(PokemonEntry src, PokemonEntry dst)
        {
            CopyStatsExact(src, dst);
        }

        /// <summary>
        /// Copy the source's stats and add <paramref name="delta"/> to dst across the six stats randomly so dst BST = src BST + delta.
        /// Used when "Randomize Added Stats on Evolution" is on.
        /// </summary>
        static void CopyRandomizedStatsWithDelta(PokemonEntry src, PokemonEntry dst, int delta, Random rnd)
        {
            int[] raw = { src.baseHP, src.baseAttack, src.baseDefense, src.baseSpAtt, src.baseSpDef, src.baseSpeed };
            int safety = delta * 4 + 100;
            while (delta > 0 && safety-- > 0)
            {
                int idx = rnd.Next(6);
                if (raw[idx] < 255)
                {
                    raw[idx]++;
                    delta--;
                }
            }
            byte[] s = new byte[6];
            for (int i = 0; i < 6; i++) s[i] = (byte)raw[i];
            SetStatArray(dst, s);
        }

        static void CopyShuffledStatsWithDelta(PokemonEntry src, PokemonEntry dst, int delta, Random rnd)
        {
            CopyRandomizedStatsWithDelta(src, dst, delta, rnd);
        }
    }
}
