using NewEditor.Data.Randomization.GeneShuffle;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Fairy-aware type bounds for all FVX Gen 5 randomization (starters, statics, trades, and future tabs).
    /// When implementing type rolls (e.g. any-type triangle, single-type filters, compatibility checks),
    /// use <see cref="MaxPrimaryTypeInclusive"/> with the user’s <see cref="FvxStartersStaticsTradesOptions.IncludeFairyTypes"/>
    /// and the patch-applied flag—do not hardcode 16 or 17 types.
    /// </summary>
    public static class FvxGen5TypeSupport
    {
        /// <inheritdoc cref="GeneShuffleFairyPatch.MaxTypeInclusive"/>
        public static int MaxPrimaryTypeInclusive(bool includeFairyRequested, bool fairyPatchJustApplied)
            => GeneShuffleFairyPatch.MaxTypeInclusive(includeFairyRequested, fairyPatchJustApplied);

        /// <inheritdoc cref="GeneShuffleFairyPatch.TypeChartListsFairy"/>
        public static bool TypeChartListsFairy()
            => GeneShuffleFairyPatch.TypeChartListsFairy();

        /// <summary>Same bundled Fairy Vpatch flow as Gene Shuffle (Black 1 / B2 / W2; not White 1).</summary>
        public static bool TryPrepareFairyPatch(bool includeFairy, out bool patchApplied, out string error)
            => GeneShuffleFairyPatch.TryPrepare(includeFairy, out patchApplied, out error);
    }
}
