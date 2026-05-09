namespace NewEditor.Data.Randomization.FvxGen5
{
    public enum FvxStarterSelectionMode
    {
        Unchanged,
        Custom,
        RandomCompletely,
        RandomBasicThreeStageLine,
        RandomAnyBasic
    }

    public enum FvxStarterTypeRestriction
    {
        None,
        FireWaterGrass,
        AnyTypeTriangle,
        UniquePrimaryType,
        SingleType
    }

    public enum FvxStaticsRandomizationMode
    {
        Unchanged,
        SwapLegendariesAndStandards,
        RandomCompletely,
        RandomSimilarStrength
    }

    public enum FvxTradesRandomizationMode
    {
        Unchanged,
        RandomizeGivenOnly,
        RandomizeBoth
    }

    /// <summary>UI + apply flags for Starters / Statics / In-game trades (FVX-style, Gen 5).</summary>
    public sealed class FvxStartersStaticsTradesOptions
    {
        /// <summary>
        /// When true, type pools (e.g. Any Type Triangle, Single Type random, dual-type filters) must include
        /// Fairy where applicable: use <see cref="FvxGen5TypeSupport.MaxPrimaryTypeInclusive"/> after any Fairy Vpatch step.
        /// </summary>
        public bool IncludeFairyTypes { get; set; }

        public FvxStarterSelectionMode StarterSelectionMode { get; set; }
        /// <summary>National species indices for custom starters (internal dex order).</summary>
        public int CustomStarterSpeciesIndex0 { get; set; }
        public int CustomStarterSpeciesIndex1 { get; set; }
        public int CustomStarterSpeciesIndex2 { get; set; }

        public FvxStarterTypeRestriction StarterTypeRestriction { get; set; }
        /// <summary>When <see cref="StarterTypeRestriction"/> is <see cref="FvxStarterTypeRestriction.SingleType"/>: null = random type.</summary>
        public byte? SinglePrimaryTypeId { get; set; }

        public bool NoDualTypes { get; set; }
        public bool DontUseLegendaries { get; set; }

        public bool LimitBstMin { get; set; }
        public int BstMinimum { get; set; } = 307;
        public bool LimitBstMax { get; set; }
        public int BstMaximum { get; set; } = 320;

        public FvxStaticsRandomizationMode StaticsMode { get; set; }
        public bool StaticsRandomize600PlusBst { get; set; }
        public bool StaticsLimitMainGameLegendaries { get; set; }

        public bool StaticsUseLevelPercentModifier { get; set; }
        /// <summary>Percent added to static encounter levels (-100 to 150).</summary>
        public int StaticsLevelPercentModifier { get; set; }

        public FvxTradesRandomizationMode TradesMode { get; set; }
        public bool TradesRandomizeNicknames { get; set; }
        /// <summary>Not applied automatically — OT is stored in mixed script/text layers; UI disabled until a safe approach exists.</summary>
        public bool TradesRandomizeOts { get; set; }
        public bool TradesRandomizeIvs { get; set; }
        public bool TradesRandomizeItems { get; set; }

        /// <summary>Reserved for future starter sprite/cry NARC sync with chosen species (currently unused).</summary>
        public bool StarterUpdateGraphicsAndCries { get; set; }

        public FvxRandomizerGlobalOptions Global { get; set; }
    }
}
