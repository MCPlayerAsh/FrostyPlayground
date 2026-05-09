using System;
using System.Collections.Generic;
using NewEditor.Data.NARCTypes;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Approximates UPR <c>Species.isLegalEvolutionAtLevel</c> for Gen5 NARC evolutions.</summary>
    internal static class FvxPrematureEvoLegality
    {
        public static bool IsLegalEvolutionAtLevel(int speciesIndex, int level, double evoLevelModifier,
            IReadOnlyList<EvolutionDataEntry> evolutions)
        {
            if (evolutions == null || speciesIndex <= 0 || speciesIndex >= evolutions.Count)
                return true;

            bool anyIncoming = false;
            for (int parent = 0; parent < evolutions.Count; parent++)
            {
                var methods = evolutions[parent]?.methods;
                if (methods == null) continue;
                foreach (var m in methods)
                {
                    if (m.newPokemonID != speciesIndex) continue;
                    if (FvxGen5EvolutionMethods.IsImpossibleWithoutTrade(m.method)) continue;
                    anyIncoming = true;
                    int est = EstimateMinLevelForEdge(m.method, m.condition);
                    if (level >= evoLevelModifier * est)
                        return true;
                }
            }

            return !anyIncoming;
        }

        static int EstimateMinLevelForEdge(int method, int condition)
        {
            if (FvxGen5EvolutionMethods.ConditionIsLevel(method))
                return Math.Max(1, condition);
            switch (method)
            {
                case FvxGen5EvolutionMethods.LevelUpHighFriendship:
                case FvxGen5EvolutionMethods.FriendshipDay:
                case FvxGen5EvolutionMethods.FriendshipNight:
                    return 25;
                case FvxGen5EvolutionMethods.Stone:
                case FvxGen5EvolutionMethods.StoneMale:
                case FvxGen5EvolutionMethods.StoneFemale:
                    return 1;
                default:
                    return 30;
            }
        }
    }
}
