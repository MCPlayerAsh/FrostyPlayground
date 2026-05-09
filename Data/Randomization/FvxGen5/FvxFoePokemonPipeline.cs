using System;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public static class FvxFoePokemonPipeline
    {
        public static bool TryRun(FvxFoePokemonOptions opt, Random rnd, out string error)
        {
            error = null;
            if (opt == null)
            {
                error = "Foe Pokémon options are null.";
                return false;
            }
            if (MainEditor.fileSystem == null)
            {
                error = "No file system loaded.";
                return false;
            }
            if (MainEditor.trainerNarc?.trainers == null || MainEditor.trainerPokeNarc?.pokemonGroups == null)
            {
                error = "Trainer data is not loaded.";
                return false;
            }
            if (MainEditor.RomType != RomType.BW1 && MainEditor.RomType != RomType.BW2)
            {
                error = "Foe Pokémon randomizer supports Black/White and Black 2/White 2 only.";
                return false;
            }
            if (opt.IncludeFairyTypes && MainEditor.RomTypeId == "pokemon w")
            {
                error = "Fairy-type support uses a Vpatch that exists for Pokémon Black 1, Black 2, and White 2 only—not White 1. Uncheck \"Include Fairy-type\" on the Starters tab or use a supported ROM.";
                return false;
            }
            if (!opt.AnyRandomizationActive)
                return true;

            if (!FvxGen5TypeSupport.TryPrepareFairyPatch(opt.IncludeFairyTypes, out _, out var fairyErr))
            {
                error = fairyErr ?? "Fairy patch step failed.";
                return false;
            }

            int maxType = FvxGen5TypeSupport.MaxPrimaryTypeInclusive(opt.IncludeFairyTypes, FvxGen5TypeSupport.TypeChartListsFairy());
            return FvxFoePokemonRunner.TryRun(opt, rnd, maxType, out error);
        }
    }
}
