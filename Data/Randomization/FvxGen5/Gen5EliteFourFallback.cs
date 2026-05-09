using System.Collections.Generic;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Vanilla US 1-based elite indices from UPR-FVX <c>gen5_offsets.ini</c> when trainer tags are unusable.
    /// Modded ROMs: prefer tag-based league detection; this set is only a fallback after filtering to ROM size.
    /// </summary>
    internal static class Gen5EliteFourFallback
    {
        /// <summary>1-based indices (BW2 normal mode; challenge mode not wired).</summary>
        public static void AddVanillaEliteIndicesOneBased(bool bw2, ICollection<int> sink)
        {
            if (bw2)
            {
                foreach (int i in Bw2Normal) sink.Add(i);
            }
            else
            {
                foreach (int i in Bw1BlackWhite) sink.Add(i);
            }
        }

        static readonly int[] Bw1BlackWhite = { 228, 229, 230, 231, 232, 586, 587 };

        static readonly int[] Bw2Normal = { 38, 39, 40, 41, 341 };

        /// <summary>0-based trainer indices for processing-order priority (FVX elite-first sort).</summary>
        public static HashSet<int> ResolveLeagueTrainerIndicesZeroBased(string[] tags, int trainerCount, bool bw2)
        {
            var set = new HashSet<int>();
            if (tags != null)
            {
                for (int i = 0; i < tags.Length && i < trainerCount; i++)
                {
                    if (FvxTrainerTagClassification.IsLeagueTag(tags[i]))
                        set.Add(i);
                }
            }

            if (set.Count > 0)
                return set;

            var oneBased = new List<int>();
            AddVanillaEliteIndicesOneBased(bw2, oneBased);
            foreach (int ob in oneBased)
            {
                int z = ob - 1;
                if (z >= 0 && z < trainerCount)
                    set.Add(z);
            }

            return set;
        }
    }
}
