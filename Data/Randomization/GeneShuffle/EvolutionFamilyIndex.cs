using System.Collections.Generic;
using NewEditor.Data.NARCTypes;

namespace NewEditor.Data.Randomization.GeneShuffle
{
    /// <summary>Undirected evolution families (connected components over evolution edges).</summary>
    public static class EvolutionFamilyIndex
    {
        public static List<List<int>> BuildFamilies(int speciesCount, IReadOnlyList<EvolutionDataEntry> evolutions)
        {
            if (speciesCount <= 0 || evolutions == null) return new List<List<int>>();
            var parent = new int[speciesCount];
            for (int i = 0; i < speciesCount; i++) parent[i] = i;

            int Find(int x)
            {
                while (parent[x] != x)
                {
                    parent[x] = parent[parent[x]];
                    x = parent[x];
                }
                return x;
            }

            void Union(int a, int b)
            {
                if (a < 0 || a >= speciesCount || b < 0 || b >= speciesCount) return;
                a = Find(a);
                b = Find(b);
                if (a != b) parent[b] = a;
            }

            int n = System.Math.Min(speciesCount, evolutions.Count);
            for (int from = 0; from < n; from++)
            {
                var methods = evolutions[from]?.methods;
                if (methods == null) continue;
                foreach (var m in methods)
                {
                    if (m.method == 0) continue;
                    int to = m.newPokemonID;
                    if (to < 0 || to >= speciesCount) continue;
                    Union(from, to);
                }
            }

            var buckets = new Dictionary<int, List<int>>();
            for (int i = 0; i < speciesCount; i++)
            {
                int r = Find(i);
                if (!buckets.TryGetValue(r, out var list))
                {
                    list = new List<int>();
                    buckets[r] = list;
                }
                list.Add(i);
            }

            var outList = new List<List<int>>(buckets.Count);
            foreach (var kv in buckets) outList.Add(kv.Value);
            return outList;
        }
    }
}
