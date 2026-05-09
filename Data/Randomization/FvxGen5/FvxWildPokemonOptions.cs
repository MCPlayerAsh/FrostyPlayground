namespace NewEditor.Data.Randomization.FvxGen5
{
    public enum FvxWildReplacementMode
    {
        WholeGame,
        NamedLocation,
        PerMap,
        PerEncounterSet,
        MaximumPossible
    }

    public enum FvxWildTypeRestrictionMode
    {
        None,
        RandomZoneThemes,
        KeepPrimaryType
    }

    public enum FvxWildEvolutionRestrictionMode
    {
        None,
        BasicOnly,
        SameEvolutionStage
    }

    public sealed class FvxWildPokemonOptions
    {
        public bool RandomizeWildPokemon { get; set; }
        public FvxWildReplacementMode ReplacementMode { get; set; } = FvxWildReplacementMode.WholeGame;
        public bool SplitByEncounterType { get; set; }

        public FvxWildTypeRestrictionMode TypeRestrictionMode { get; set; } = FvxWildTypeRestrictionMode.None;
        public bool KeepZoneTypeThemes { get; set; }

        public FvxWildEvolutionRestrictionMode EvolutionRestrictionMode { get; set; } = FvxWildEvolutionRestrictionMode.None;
        public bool KeepEvolutionRelations { get; set; }

        public bool UseTimeBasedEncounters { get; set; }
        public bool DontUseLegendaries { get; set; }
        public bool SetMinimumCatchRate { get; set; }
        public int MinimumCatchRateLevel { get; set; } = 1;

        public bool RandomizeHeldItems { get; set; }
        public bool BanBadItems { get; set; }

        public bool CatchEmAllMode { get; set; }
        public bool SimilarStrength { get; set; }
        public bool BalanceLowLevelEncounters { get; set; }
        public bool AllowAlternateFormes { get; set; }

        public bool LevelModifierEnabled { get; set; }
        public int LevelModifierPercent { get; set; }

        public FvxRandomizerGlobalOptions Global { get; set; }

        public bool AnyRandomizationActive =>
            RandomizeWildPokemon
            || SetMinimumCatchRate
            || RandomizeHeldItems
            || LevelModifierEnabled;
    }
}
