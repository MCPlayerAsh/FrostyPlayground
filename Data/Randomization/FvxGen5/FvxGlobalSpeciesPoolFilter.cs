using System.Collections.Generic;
using System.Linq;
using NewEditor.Data.NARCTypes;

namespace NewEditor.Data.Randomization.FvxGen5
{
    internal static class FvxGlobalSpeciesPoolFilter
    {
        public static List<int> FilterPool(
            IEnumerable<int> pool,
            FvxRandomizerGlobalOptions global,
            bool bw2Rom,
            IReadOnlyList<EvolutionDataEntry> evolutions,
            int? minLevelForPrematureBan)
        {
            var list = pool.ToList();
            if (global == null) return list;

            if (global.LimitPokemon && global.AllowedSpecies != null && global.AllowedSpecies.Count > 0)
                list = list.Where(global.AllowedSpecies.Contains).ToList();

            if (global.BanIrregularAltFormes)
                list = list.Where(i => !FvxGen5IrregularFormes.IsBannedWhenOptionOn(i, bw2Rom)).ToList();

            if (global.BanPrematureEvos && minLevelForPrematureBan.HasValue && evolutions != null)
            {
                int lv = minLevelForPrematureBan.Value;
                list = list.Where(i => FvxPrematureEvoLegality.IsLegalEvolutionAtLevel(i, lv, 1.0, evolutions))
                    .ToList();
            }

            return list;
        }
    }
}
