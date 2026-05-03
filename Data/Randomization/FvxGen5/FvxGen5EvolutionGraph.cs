using System.Collections.Generic;
using NewEditor.Data.NARCTypes;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Evolution edges for Gen5 personal species index (0..N-1), FVX CopyUpEvolutionsHelper order.
    /// </summary>
    internal sealed class FvxGen5EvolutionGraph
    {
        readonly List<List<int>> edgesFrom = new List<List<int>>();
        readonly int count;

        public FvxGen5EvolutionGraph(int speciesCount)
        {
            count = speciesCount;
            for (int i = 0; i < speciesCount; i++) edgesFrom.Add(new List<int>());
        }

        public static FvxGen5EvolutionGraph FromEvolutions(IReadOnlyList<EvolutionDataEntry> evolutions)
        {
            var g = new FvxGen5EvolutionGraph(evolutions.Count);
            for (int from = 0; from < evolutions.Count; from++)
            {
                var m = evolutions[from].methods;
                if (m == null) continue;
                foreach (var method in m)
                {
                    if (method.method == 0) continue;
                    int to = method.newPokemonID;
                    if (to < 0 || to >= evolutions.Count) continue;
                    if (to == from) continue;
                    if (!g.edgesFrom[from].Contains(to))
                        g.edgesFrom[from].Add(to);
                }
            }
            return g;
        }

        public bool[] ComputeIncoming()
        {
            var inc = new bool[count];
            foreach (var list in edgesFrom)
                foreach (int t in list)
                    if (t >= 0 && t < count) inc[t] = true;
            return inc;
        }

        public bool IsBasic(int speciesIndex, bool[] incoming) => !incoming[speciesIndex];

        /// <summary>BFS from species with no incoming evolutions (basics), then along edgesFrom.</summary>
        public void ApplyCopyUp(System.Action<int> basicAction, System.Action<int, int, bool> evolvedAction)
        {
            var incoming = ComputeIncoming();
            var processed = new HashSet<int>();
            var isFinal = new bool[count];
            for (int i = 0; i < count; i++)
                isFinal[i] = incoming[i] && edgesFrom[i].Count == 0;

            var q = new Queue<int>();
            for (int i = 0; i < count; i++)
            {
                if (!IsBasic(i, incoming)) continue;
                basicAction(i);
                processed.Add(i);
                q.Enqueue(i);
            }

            while (q.Count > 0)
            {
                int from = q.Dequeue();
                foreach (int to in edgesFrom[from])
                {
                    if (to < 0 || to >= count || processed.Contains(to)) continue;
                    evolvedAction(from, to, isFinal[to]);
                    processed.Add(to);
                    q.Enqueue(to);
                }
            }
        }
    }
}
