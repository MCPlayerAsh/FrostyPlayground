namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Mirror of Universal Pokémon Randomizer ZX <c>Settings.TrainersMod</c> for foe Pokémon.</summary>
    public enum FvxFoeTrainerPokemonMode
    {
        Unchanged,
        Random,
        Distributed,
        MainPlaythrough,
        TypeThemed,
        TypeThemedElite4Gyms,
        /// <summary>UPR-FVX <c>KEEP_THEMED</c> — ROM gym/elite themes, else shared type across original team.</summary>
        KeepThemed,
        /// <summary>UPR-FVX <c>KEEP_THEME_OR_PRIMARY</c> — theme or shared team type, else each slot's original primary type.</summary>
        KeepThemeOrPrimary,
    }

    /// <summary>
    /// Boss / Important / Regular tier detection — heuristic text matching vs UPR-ZX Gen5 trainer tags.
    /// </summary>
    public enum FvxFoeTierDetectionMode
    {
        Heuristic,
        MatchingVanillaUpr,
    }

    internal enum FvxFoeTrainerTier
    {
        Regular,
        Important,
        Boss,
    }
}
