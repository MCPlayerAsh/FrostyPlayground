using System;
using System.Collections.Generic;
using NewEditor.Data.NARCTypes;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// FVX EvolutionRandomizer port. Rewrites the evolutions NARC in place:
    /// <list type="bullet">
    /// <item>Mode <see cref="FvxEvolutionsMod.Random"/> picks a different target species per existing evo slot.</item>
    /// <item>Mode <see cref="FvxEvolutionsMod.RandomEveryLevel"/> additionally collapses all methods to "level up" so every species gets one new evolution.</item>
    /// <item>Filter toggles (Similar Strength, Same Typing, Limit to 3 Stages, No Convergence, Force Change, Force Growth) constrain the candidate pool for each roll.</item>
    /// <item>Tweaks (Change Impossible, Make Easier, Estimated Levels, Remove Time-Based) rewrite the existing methods/conditions even when mode is Unchanged.</item>
    /// </list>
    /// </summary>
    public static class FvxEvolutionRandomizer
    {
        const int SimilarStrengthBstWindow = 60;
        const int ForceGrowthMinDelta = 30;
        const int DefaultEvolutionLevel = 36;
        const int DefaultEvolveEveryLevelStep = 5;

        public static void Apply(FvxPokemonTraitsOptions opt, Random rnd, List<PokemonEntry> pokemon, List<EvolutionDataEntry> evolutions)
        {
            if (opt == null || pokemon == null || evolutions == null) return;

            ApplyTweaks(opt, pokemon, evolutions);

            if (opt.EvolutionsMod == FvxEvolutionsMod.Unchanged)
            {
                WriteAll(evolutions);
                return;
            }

            ApplyRandom(opt, rnd, pokemon, evolutions);
            WriteAll(evolutions);
        }

        static void WriteAll(List<EvolutionDataEntry> evolutions)
        {
            for (int i = 0; i < evolutions.Count; i++)
            {
                var e = evolutions[i];
                if (e?.methods == null || e.bytes == null) continue;
                int methodCount = e.bytes.Length / 6;
                if (e.methods.Length != methodCount) continue;
                e.ApplyData();
            }
        }

        // -----------------------------------------------------------------
        // Tweaks (apply regardless of mode)
        // -----------------------------------------------------------------

        static void ApplyTweaks(FvxPokemonTraitsOptions opt, List<PokemonEntry> pokemon, List<EvolutionDataEntry> evolutions)
        {
            if (!opt.EvolutionsChangeImpossible
                && !opt.EvolutionsMakeEasier
                && !opt.EvolutionsRemoveTimeBased
                && !opt.EvolutionsUseEstimatedLevels)
                return;

            int cap = HelperFunctions.Clamp(opt.EvolutionsMakeEasierLevelCap, 30, 64);

            for (int from = 0; from < evolutions.Count; from++)
            {
                var entry = evolutions[from];
                if (entry?.methods == null) continue;

                for (int i = 0; i < entry.methods.Length; i++)
                {
                    var m = entry.methods[i];
                    if (m.method == FvxGen5EvolutionMethods.None) continue;

                    int method = m.method;
                    int condition = m.condition;
                    int target = m.newPokemonID;

                    if (opt.EvolutionsChangeImpossible && FvxGen5EvolutionMethods.IsImpossibleWithoutTrade(method))
                    {
                        int level = opt.EvolutionsUseEstimatedLevels
                            ? EstimateEvolutionLevel(from, target, pokemon)
                            : DefaultEvolutionLevel;
                        method = FvxGen5EvolutionMethods.LevelUp;
                        condition = level;
                    }

                    if (opt.EvolutionsRemoveTimeBased && FvxGen5EvolutionMethods.IsTimeBased(method))
                    {
                        // Day/Night friendship -> high friendship; held-item day/night -> level-up.
                        if (method == FvxGen5EvolutionMethods.FriendshipDay || method == FvxGen5EvolutionMethods.FriendshipNight)
                            method = FvxGen5EvolutionMethods.LevelUpHighFriendship;
                        else
                        {
                            int level = opt.EvolutionsUseEstimatedLevels
                                ? EstimateEvolutionLevel(from, target, pokemon)
                                : DefaultEvolutionLevel;
                            method = FvxGen5EvolutionMethods.LevelUp;
                            condition = level;
                        }
                    }

                    if (opt.EvolutionsMakeEasier && FvxGen5EvolutionMethods.ConditionIsLevel(method))
                    {
                        if (condition > cap) condition = cap;
                        if (condition < 2) condition = 2;
                    }

                    entry.methods[i] = new EvolutionMethod((short)method, (short)condition, (short)target);
                }
            }
        }

        static int EstimateEvolutionLevel(int fromIndex, int toIndex, List<PokemonEntry> pokemon)
        {
            if (fromIndex < 0 || fromIndex >= pokemon.Count || toIndex < 0 || toIndex >= pokemon.Count)
                return DefaultEvolutionLevel;
            var src = pokemon[fromIndex];
            var dst = pokemon[toIndex];
            if (src == null || dst == null) return DefaultEvolutionLevel;

            int delta = dst.baseStatTotal - src.baseStatTotal;
            // Heuristic: ~22 + delta/12 → small jumps evolve earlier, big jumps evolve later.
            int est = 22 + (delta / 12);
            if (est < 16) est = 16;
            if (est > 50) est = 50;
            return est;
        }

        // -----------------------------------------------------------------
        // Randomization (mode = Random / RandomEveryLevel)
        // -----------------------------------------------------------------

        static void ApplyRandom(FvxPokemonTraitsOptions opt, Random rnd, List<PokemonEntry> pokemon, List<EvolutionDataEntry> evolutions)
        {
            int n = Math.Min(pokemon.Count, evolutions.Count);

            // Build "stage" map (pre-randomization) so Limit-To-Three-Stages can keep pre-existing depth context.
            int[] preStage = ComputePreEvolutionStages(evolutions, n);
            // Track how many incoming edges each species ends up with after randomization (for No Convergence).
            int[] usedIncoming = new int[pokemon.Count];

            // Track newly-created chain depth so we don't exceed three stages even with brand-new chains.
            // pokemon.Count can exceed evolutions.Count (alt forms aren't always in the evolutions NARC),
            // so only seed the [0..n) range from preStage; entries past n stay at the default 0.
            int[] postStage = new int[pokemon.Count];
            for (int i = 0; i < n; i++) postStage[i] = preStage[i];

            for (int from = 0; from < n; from++)
            {
                var entry = evolutions[from];
                if (entry?.methods == null) continue;

                for (int slot = 0; slot < entry.methods.Length; slot++)
                {
                    var m = entry.methods[slot];
                    if (m.method == FvxGen5EvolutionMethods.None) continue;

                    int newTarget = PickRandomEvolutionTarget(opt, rnd, pokemon, n, from, m.newPokemonID, postStage, usedIncoming);
                    if (newTarget < 0) continue; // no valid candidate; leave the original alone.

                    int method = m.method;
                    int condition = m.condition;
                    if (opt.EvolutionsMod == FvxEvolutionsMod.RandomEveryLevel)
                    {
                        method = FvxGen5EvolutionMethods.LevelUp;
                        // Spread out the levels a little so the player doesn't double-evo on the same level.
                        condition = Math.Min(98, 5 + slot * DefaultEvolveEveryLevelStep);
                    }
                    else if (FvxGen5EvolutionMethods.ConditionIsLevel(method) && condition < 2)
                    {
                        condition = DefaultEvolutionLevel;
                    }

                    entry.methods[slot] = new EvolutionMethod((short)method, (short)condition, (short)newTarget);
                    usedIncoming[newTarget]++;
                    postStage[newTarget] = Math.Max(postStage[newTarget], postStage[from] + 1);
                }
            }
        }

        static int PickRandomEvolutionTarget(
            FvxPokemonTraitsOptions opt,
            Random rnd,
            List<PokemonEntry> pokemon,
            int speciesCount,
            int from,
            int originalTarget,
            int[] postStage,
            int[] usedIncoming)
        {
            var src = pokemon[from];
            if (src == null) return -1;
            int srcBst = src.baseStatTotal;

            var candidates = new List<int>(speciesCount);
            for (int i = 1; i < speciesCount; i++)
            {
                if (i == from) continue;
                var pk = pokemon[i];
                if (pk == null) continue;

                if (opt.EvolutionsForceChange && i == originalTarget) continue;

                if (opt.EvolutionsLimitToThreeStages && postStage[from] + 1 > 2) continue;

                if (opt.EvolutionsNoConvergence && usedIncoming[i] > 0) continue;

                if (opt.EvolutionsForceGrowth && pk.baseStatTotal < srcBst + ForceGrowthMinDelta) continue;

                if (opt.EvolutionsSameTyping)
                {
                    bool typesOverlap = pk.type1 == src.type1
                                     || (pk.type2 != 255 && (pk.type2 == src.type1 || pk.type2 == src.type2))
                                     || (src.type2 != 255 && pk.type1 == src.type2);
                    if (!typesOverlap) continue;
                }

                if (opt.EvolutionsSimilarStrength)
                {
                    if (Math.Abs(pk.baseStatTotal - srcBst) > SimilarStrengthBstWindow) continue;
                }

                candidates.Add(i);
            }

            if (candidates.Count == 0)
            {
                // Relax: drop No-Convergence + Same-Typing + Similar-Strength as a fallback.
                for (int i = 1; i < speciesCount; i++)
                {
                    if (i == from) continue;
                    var pk = pokemon[i];
                    if (pk == null) continue;
                    if (opt.EvolutionsForceChange && i == originalTarget) continue;
                    if (opt.EvolutionsLimitToThreeStages && postStage[from] + 1 > 2) continue;
                    if (opt.EvolutionsForceGrowth && pk.baseStatTotal < srcBst + ForceGrowthMinDelta) continue;
                    candidates.Add(i);
                }
            }

            if (candidates.Count == 0) return -1;
            return candidates[rnd.Next(candidates.Count)];
        }

        /// <summary>
        /// Stage 0 = basic (no incoming evos). Stage k = max stage of any species that evolves into us, +1.
        /// Computed over the *current* (pre-randomization) graph so existing chains are respected.
        /// </summary>
        static int[] ComputePreEvolutionStages(IReadOnlyList<EvolutionDataEntry> evolutions, int n)
        {
            var incomingFrom = new List<int>[n];
            for (int i = 0; i < n; i++) incomingFrom[i] = new List<int>();

            for (int from = 0; from < n; from++)
            {
                var e = evolutions[from];
                if (e?.methods == null) continue;
                foreach (var m in e.methods)
                {
                    if (m.method == FvxGen5EvolutionMethods.None) continue;
                    int to = m.newPokemonID;
                    if (to >= 0 && to < n && to != from) incomingFrom[to].Add(from);
                }
            }

            var stage = new int[n];
            for (int i = 0; i < n; i++) stage[i] = -1;

            int Resolve(int idx, HashSet<int> visiting)
            {
                if (stage[idx] >= 0) return stage[idx];
                if (incomingFrom[idx].Count == 0) { stage[idx] = 0; return 0; }
                if (!visiting.Add(idx)) { stage[idx] = 0; return 0; } // cycle guard
                int best = 0;
                foreach (int parent in incomingFrom[idx])
                    best = Math.Max(best, Resolve(parent, visiting) + 1);
                visiting.Remove(idx);
                stage[idx] = best;
                return best;
            }

            for (int i = 0; i < n; i++)
                if (stage[i] < 0) Resolve(i, new HashSet<int>());

            return stage;
        }
    }
}
