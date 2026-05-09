using System.Collections.Generic;
using System.Linq;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Held-item pools aligned with UPR-ZX Gen 5 <c>getAllHeldItems</c> / consumable / sensible branching.
    /// Consumable IDs approximate UPR-ZX merged Gen4+Gen5 consumable tables (berries, herbs, gems, balloons).
    /// </summary>
    internal static class FvxGen5HeldItemPools
    {
        static readonly HashSet<int> ConsumableSet = BuildConsumableSet();

        static HashSet<int> BuildConsumableSet()
        {
            var h = new HashSet<int>();
            for (int i = 149; i <= 286; i++)
                h.Add(i);
            for (int i = 537; i <= 575; i++)
                h.Add(i);
            h.Add(43);
            return h;
        }

        static bool IsNonsensibleHeldName(string nm)
        {
            if (string.IsNullOrEmpty(nm)) return true;
            string s = nm.ToLowerInvariant();
            return s.Contains("master ball") || s.Contains("rare candy") || s.Contains("sacred ash")
                   || s.Contains("adamant orb") || s.Contains("lustrous orb") || s.Contains("griseous orb");
        }

        /// <summary>Build pool matching UPR held-item mode flags.</summary>
        public static List<int> BuildPool(FvxFoePokemonOptions opt, IReadOnlyList<string> names, int itemCount)
        {
            var pool = new List<int>();
            int n = names == null ? 0 : System.Math.Min(names.Count, itemCount);
            if (n <= 1) return pool;

            if (opt.HeldConsumableOnly)
            {
                for (int i = 1; i < n; i++)
                {
                    if (ConsumableSet.Contains(i)) pool.Add(i);
                }
                if (pool.Count == 0)
                {
                    for (int i = 1; i < n; i++)
                    {
                        string nm = names[i] ?? "";
                        if (IsConsumableHeldNameFallback(nm)) pool.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 1; i < n; i++)
                    pool.Add(i);
            }

            if (opt.HeldSensibleItems)
            {
                pool = pool.Where(i =>
                {
                    if (i < 0 || i >= names.Count) return false;
                    return !IsNonsensibleHeldName(names[i] ?? "");
                }).ToList();
            }

            return pool;
        }

        static bool IsConsumableHeldNameFallback(string nm)
        {
            string s = nm.ToLowerInvariant();
            return s.Contains("berry") || s.Contains("potion") || s.Contains("elixir") || s.Contains("ether")
                   || s.Contains("revive") || s.Contains("moomoo") || s.Contains("energy root") || s.Contains("fresh water")
                   || (s.Contains("gem") && !s.Contains("rare"));
        }
    }
}
