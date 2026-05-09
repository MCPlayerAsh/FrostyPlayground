using System;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public static class FvxWildPokemonPipeline
    {
        public static bool TryRun(FvxWildPokemonOptions opt, Random rnd, out string error)
        {
            error = null;
            if (opt == null)
            {
                error = "Wild Pokémon options are null.";
                return false;
            }
            if (MainEditor.fileSystem == null)
            {
                error = "No file system loaded.";
                return false;
            }
            if (MainEditor.encounterNarc?.mainEncounterPools == null
                || MainEditor.encounterNarc.mainEncounterPools.Count == 0)
            {
                error = "Encounter data is not loaded.";
                return false;
            }
            if (MainEditor.pokemonDataNarc?.pokemon == null)
            {
                error = "Pokémon personal data is not loaded.";
                return false;
            }
            if (MainEditor.RomType != RomType.BW1 && MainEditor.RomType != RomType.BW2)
            {
                error = "Wild Pokémon randomizer supports Black/White and Black 2/White 2 only.";
                return false;
            }
            if (!opt.AnyRandomizationActive)
                return true;
            return FvxWildPokemonRunner.TryRun(opt, rnd, out error);
        }
    }
}
