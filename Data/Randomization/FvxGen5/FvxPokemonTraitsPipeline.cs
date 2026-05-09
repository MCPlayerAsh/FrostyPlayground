using System;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Single entry point for all Pokemon Traits randomization (Base Stats, Abilities, Evolutions, EXP curves).
    /// Designed to be called from both Gene Shuffle and the FVX Randomizer form.
    /// </summary>
    public static class FvxPokemonTraitsPipeline
    {
        public static bool TryRun(FvxPokemonTraitsOptions opt, Random rnd, out string error)
        {
            error = null;
            if (opt == null)
            {
                error = "Pokemon Traits options are null.";
                return false;
            }
            var pd = MainEditor.pokemonDataNarc?.pokemon;
            if (pd == null)
            {
                error = "Pokemon data is not loaded.";
                return false;
            }

            var evolutions = MainEditor.evolutionsNarc?.evolutions;
            if (evolutions == null && (opt.BaseStatsFollowEvolutions
                                       || opt.AbilitiesFollowEvolutions
                                       || opt.EvolutionsMod != FvxEvolutionsMod.Unchanged
                                       || opt.EvolutionsChangeImpossible
                                       || opt.EvolutionsMakeEasier
                                       || opt.EvolutionsRemoveTimeBased
                                       || opt.EvolutionsUseEstimatedLevels))
            {
                error = "Evolution data is not loaded; cannot apply trait options that depend on it.";
                return false;
            }

            try
            {
                FvxBaseStatRandomizer.Apply(opt, rnd, pd, evolutions);
                FvxAbilityRandomizer.Apply(opt, rnd, pd, evolutions);
                if (evolutions != null)
                    FvxEvolutionRandomizer.Apply(opt, rnd, pd, evolutions);
                foreach (var pk in pd) pk?.ApplyData();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
