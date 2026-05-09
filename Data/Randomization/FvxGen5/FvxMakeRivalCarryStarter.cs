using System;
using System.Collections.Generic;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Mirrors UPR-FVX <c>TrainerPokemonRandomizer.makeRivalCarryStarter</c> /
    /// <c>rivalCarriesStarterUpdate</c> (non-Yellow): run <b>before</b> general trainer randomization.
    /// </summary>
    internal static class FvxMakeRivalCarryStarter
    {
        public static void Apply(string[] tags, List<TrainerEntry> trainers, int[] starterTrio,
            FvxGen5EvolutionGraph graph, IReadOnlyList<EvolutionDataEntry> evolutions, bool[] incoming, Random rnd,
            int trainersEvoLevelPercent)
        {
            if (tags == null || trainers == null || starterTrio == null || starterTrio.Length != 3
                || graph == null || evolutions == null || incoming == null)
                return;

            RivalUpdate(tags, trainers, starterTrio, graph, evolutions, incoming, rnd, "RIVAL", 1, trainersEvoLevelPercent);
            RivalUpdate(tags, trainers, starterTrio, graph, evolutions, incoming, rnd, "FRIEND", 2, trainersEvoLevelPercent);
        }

        static void RivalUpdate(string[] tags, List<TrainerEntry> trainers, int[] starterTrio,
            FvxGen5EvolutionGraph graph, IReadOnlyList<EvolutionDataEntry> evolutions, bool[] incoming, Random rnd,
            string prefix, int starterOffset, int trainersEvoLevelPercent)
        {
            int highest = HighestPrefixNumber(tags, prefix);
            if (highest == 0) return;

            for (int variant = 0; variant < 3; variant++)
            {
                int root = starterTrio[(variant + starterOffset) % 3];
                if (root <= 0 || root >= evolutions.Count) continue;

                for (int encounter = 0; encounter <= highest; encounter++)
                {
                    string wantTag = prefix + encounter + "-" + variant;
                    for (int ti = 0; ti < trainers.Count && ti < tags.Length; ti++)
                    {
                        if (tags[ti] != wantTag) continue;
                        var tr = trainers[ti];
                        if (tr.pokemon?.pokemon == null || tr.pokemon.pokemon.Count == 0) continue;
                        int bestI = BestPokemonIndex(tr);
                        var tp = tr.pokemon.pokemon[bestI];
                        int sp = SpeciesForLevel(root, tp.level, graph, evolutions, incoming, rnd, trainersEvoLevelPercent);
                        if (sp > 0) tp.pokemonID = (short)sp;
                    }
                }
            }
        }

        static int HighestPrefixNumber(string[] tags, string prefix)
        {
            int max = 0;
            if (tags == null) return 0;
            foreach (string t in tags)
            {
                if (string.IsNullOrEmpty(t) || !t.StartsWith(prefix, StringComparison.Ordinal)) continue;
                int dash = t.IndexOf('-');
                if (dash <= prefix.Length) continue;
                if (!int.TryParse(t.Substring(prefix.Length, dash - prefix.Length), out int n)) continue;
                if (n > max) max = n;
            }
            return max;
        }

        /// <summary>Same priority as UPR <c>changeStarterWithTag</c> without forced slot.</summary>
        public static int BestPokemonIndex(TrainerEntry tr)
        {
            var list = tr.pokemon.pokemon;
            int cnt = list.Count;
            if (cnt <= 1) return 0;
            int bestI = 0;
            for (int i = 1; i < cnt; i++)
            {
                int bonus = (i == cnt - 1) ? 2 : 0;
                if (list[i].level + bonus > list[bestI].level)
                    bestI = i;
            }
            return bestI;
        }

        static int SpeciesForLevel(int rootSpecies, byte level, FvxGen5EvolutionGraph graph,
            IReadOnlyList<EvolutionDataEntry> evolutions, bool[] incoming, Random rnd, int trainersEvoLevelPercent)
        {
            double evoMod = 1 + trainersEvoLevelPercent / 100.0;
            int species = rootSpecies;
            var visited = new HashSet<int> { species };
            bool progressed = true;
            while (progressed)
            {
                progressed = false;
                if (species < 0 || species >= evolutions.Count) break;
                var outs = graph.Outgoing(species);
                var candidates = new List<int>();
                foreach (int to in outs)
                {
                    if (to < 0 || to >= evolutions.Count || visited.Contains(to)) continue;
                    if (!CanEvolveAtLevel(evolutions[species], to, level, evoMod)) continue;
                    candidates.Add(to);
                }
                if (candidates.Count == 0) break;
                int pick = candidates[rnd.Next(candidates.Count)];
                visited.Add(pick);
                species = pick;
                progressed = true;
            }
            return species;
        }

        static bool CanEvolveAtLevel(EvolutionDataEntry fromEntry, int toSpecies, byte level, double evoLvlModifier)
        {
            if (fromEntry?.methods == null) return false;
            foreach (var m in fromEntry.methods)
            {
                if (m.newPokemonID != toSpecies) continue;
                if (FvxGen5EvolutionMethods.IsImpossibleWithoutTrade(m.method)) return false;
                if (!FvxGen5EvolutionMethods.ConditionIsLevel(m.method)) continue;
                return level >= m.condition * evoLvlModifier;
            }
            return false;
        }
    }
}
