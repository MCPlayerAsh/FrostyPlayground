using System.Collections.Generic;
using NewEditor.Data.NARCTypes;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Remove evolution edges that leave a user-defined species allowlist (UPR "limit Pokémon" concept).</summary>
    internal static class FvxEvolutionAllowlistPruner
    {
        public static void Apply(IList<EvolutionDataEntry> evolutions, HashSet<int> allowed)
        {
            if (evolutions == null || allowed == null || allowed.Count == 0) return;

            for (int from = 0; from < evolutions.Count; from++)
            {
                var entry = evolutions[from];
                if (entry?.methods == null) continue;
                if (!allowed.Contains(from))
                {
                    for (int i = 0; i < entry.methods.Length; i++)
                        entry.methods[i].method = 0;
                    entry.ApplyData();
                    continue;
                }

                for (int i = 0; i < entry.methods.Length; i++)
                {
                    var m = entry.methods[i];
                    if (m.method == 0) continue;
                    int to = m.newPokemonID;
                    if (to < 0 || !allowed.Contains(to))
                        entry.methods[i].method = 0;
                }
                entry.ApplyData();
            }
        }
    }
}
