namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// BW2 appends Pokéstar Studios and related species after the National Dex in the personal data table
    /// (internal indices starting at 652; not present on BW1).
    /// </summary>
    internal static class FvxGen5NonStandardSpecies
    {
        /// <summary>First species index in BW2 personal data that is outside the 1–649 National Dex.</summary>
        public const int Bw2FirstExtendedSpeciesIndex = 652;

        public static bool IsBw2ExtendedSpecies(int speciesIndex)
            => speciesIndex >= Bw2FirstExtendedSpeciesIndex;
    }
}
