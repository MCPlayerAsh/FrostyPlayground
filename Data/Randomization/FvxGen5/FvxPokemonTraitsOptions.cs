namespace NewEditor.Data.Randomization.FvxGen5
{
    public enum FvxBaseStatsMod
    {
        Unchanged,
        Shuffle,
        Random
    }

    public enum FvxAbilitiesMod
    {
        Unchanged,
        Random
    }

    public enum FvxEvolutionsMod
    {
        Unchanged,
        Random,
        RandomEveryLevel
    }

    public enum FvxStandardizeExpScope
    {
        None,
        LegendariesSlow,
        StrongLegendariesSlow,
        AllPokemon
    }

    /// <summary>
    /// Mirrors the BW2 levelRate hex IDs (see PokemonEditor.levelRates).
    /// </summary>
    public enum FvxExpCurve
    {
        Erratic = 1,
        Fast = 4,
        MediumFast = 0,
        MediumSlow = 3,
        Slow = 5,
        Fluctuating = 2
    }

    /// <summary>
    /// UI + apply flags for FVX-style Pokemon Traits randomization (Base Stats / Abilities / Evolutions).
    /// </summary>
    public sealed class FvxPokemonTraitsOptions
    {
        public FvxBaseStatsMod BaseStatsMod { get; set; } = FvxBaseStatsMod.Unchanged;
        public bool BaseStatsFollowEvolutions { get; set; }
        public bool BaseStatsRandomizeAddedOnEvolution { get; set; }

        public FvxStandardizeExpScope StandardizeExpScope { get; set; } = FvxStandardizeExpScope.None;
        public FvxExpCurve StandardizeExpTarget { get; set; } = FvxExpCurve.MediumFast;

        public FvxAbilitiesMod AbilitiesMod { get; set; } = FvxAbilitiesMod.Unchanged;
        public bool AbilitiesAllowWonderGuard { get; set; }
        public bool AbilitiesCombineDuplicates { get; set; }
        public bool AbilitiesEnsureTwo { get; set; }
        public bool AbilitiesFollowEvolutions { get; set; }
        public bool AbilitiesBanTrapping { get; set; }
        public bool AbilitiesBanNegative { get; set; }
        public bool AbilitiesBanBad { get; set; }

        public FvxEvolutionsMod EvolutionsMod { get; set; } = FvxEvolutionsMod.Unchanged;
        public bool EvolutionsSimilarStrength { get; set; }
        public bool EvolutionsSameTyping { get; set; }
        public bool EvolutionsLimitToThreeStages { get; set; }
        public bool EvolutionsNoConvergence { get; set; }
        public bool EvolutionsForceChange { get; set; } = true;
        public bool EvolutionsForceGrowth { get; set; }

        public bool EvolutionsChangeImpossible { get; set; }
        public bool EvolutionsMakeEasier { get; set; }
        /// <summary>30..64; only used when <see cref="EvolutionsMakeEasier"/> is true.</summary>
        public int EvolutionsMakeEasierLevelCap { get; set; } = 50;
        public bool EvolutionsUseEstimatedLevels { get; set; }
        public bool EvolutionsRemoveTimeBased { get; set; }
    }
}
