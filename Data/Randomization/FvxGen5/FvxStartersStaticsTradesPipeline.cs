using System;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public static class FvxStartersStaticsTradesPipeline
    {
        /// <summary>Applies Fairy Vpatch when requested, then starters / statics / ingame trades according to <paramref name="opt"/>.</summary>
        public static bool TryRun(FvxStartersStaticsTradesOptions opt, Random rnd, out string error)
        {
            error = null;
            if (opt == null)
            {
                error = "Options are null.";
                return false;
            }
            if (MainEditor.fileSystem == null)
            {
                error = "No file system loaded.";
                return false;
            }

            if (opt.IncludeFairyTypes && MainEditor.RomTypeId == "pokemon w")
            {
                error = "Fairy-type support uses a Vpatch that exists for Pokémon Black 1, Black 2, and White 2 only—not White 1. Uncheck \"Include Fairy-type\" or use Black 1, Black 2, or White 2.";
                return false;
            }

            if (!FvxGen5TypeSupport.TryPrepareFairyPatch(opt.IncludeFairyTypes, out var fairyApplied, out var fairyErr))
            {
                error = fairyErr ?? "Fairy patch step failed.";
                return false;
            }

            int maxType = FvxGen5TypeSupport.MaxPrimaryTypeInclusive(opt.IncludeFairyTypes, fairyApplied);
            bool bw2 = MainEditor.RomType == RomType.BW2;

            return FvxGen5StartersStaticsTradesRunner.TryRun(opt, rnd, bw2, maxType, out error);
        }
    }
}
