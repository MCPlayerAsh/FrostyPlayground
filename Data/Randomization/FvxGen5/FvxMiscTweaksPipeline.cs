using System;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public static class FvxMiscTweaksPipeline
    {
        public static bool TryRun(FvxMiscTweaksOptions opt, Random rnd, out string error)
        {
            error = null;
            if (opt == null)
            {
                error = "Misc tweaks options are null.";
                return false;
            }

            opt.NationalDexAtStart = false;

            FvxGen5MiscRuntimeState.Reset();
            if (!opt.AnySelected)
                return true;

            return FvxGen5MiscTweaksRunner.TryApply(opt, rnd, out error);
        }
    }
}
