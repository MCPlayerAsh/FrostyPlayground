namespace NewEditor.Data.Randomization.FvxGen5
{
    public enum FvxBaseStatsMode { Unchanged, Shuffle, Random }

    public enum FvxTraitsTypeMode { Unchanged, RandomFollowEvolutions, RandomCompletely }

    public enum FvxAbilitiesMode { Unchanged, Random }

    public enum FvxEvolutionsMode { Unchanged, Random, RandomEveryLevel }

    public enum FvxTrainerPokemonMode { Unchanged, RandomCompletely, RandomSimilarStrength }

    public enum FvxWildMode { Unchanged, RandomCompletely, RandomWithThemes }

    public enum FvxStartersMode { Unchanged, Custom, RandomCompletely, RandomBasicThreeStage, RandomAnyBasic }

    public enum FvxFieldItemsMode { Unchanged, Shuffle, Random, RandomEvenDistribution }

    public enum FvxShopItemsMode { Unchanged, Shuffle, Random }

    public enum FvxPickupMode { Unchanged, Random }

    /// <summary>All FVX-style Gen 5 randomizer options (phased: some tabs may be no-ops until implemented).</summary>
    public sealed class FvxGen5FullSettings
    {
        public int Seed { get; set; }

        /// <summary>Single source for learnsets, TM/HM compat, move tutors (shared by Moves and TM/HM tabs).</summary>
        public FvxRandomizerOptions Core { get; set; } = new FvxRandomizerOptions();

        public FvxMoveDataSettings MoveData { get; set; } = new FvxMoveDataSettings();

        public FvxTraitsSettings Traits { get; set; } = new FvxTraitsSettings();
        public FvxStartersStaticsTradesSettings StartersStaticsTrades { get; set; } = new FvxStartersStaticsTradesSettings();
        public FvxFoePokemonSettings Foe { get; set; } = new FvxFoePokemonSettings();
        public FvxWildPokemonSettings Wild { get; set; } = new FvxWildPokemonSettings();
        public FvxTmHmTutorExtrasSettings TmHmTutorExtras { get; set; } = new FvxTmHmTutorExtrasSettings();
        public FvxItemsSettings Items { get; set; } = new FvxItemsSettings();
        public FvxMiscTweaksSettings Misc { get; set; } = new FvxMiscTweaksSettings();
    }

    public sealed class FvxMoveDataSettings
    {
        public bool RandomizeMovePower { get; set; }
        public bool RandomizeMoveAccuracy { get; set; }
        public bool RandomizeMovePP { get; set; }
        public bool RandomizeMoveTypes { get; set; }
        public bool RandomizeMoveCategory { get; set; }
        public bool RandomizeMoveNames { get; set; }
        public bool UpdateMovesToGeneration { get; set; }
        public int UpdateMovesGeneration { get; set; } = 6;
    }

    public sealed class FvxTraitsSettings
    {
        public FvxBaseStatsMode BaseStats { get; set; } = FvxBaseStatsMode.Unchanged;
        public bool FollowEvolutionsStats { get; set; }
        public bool RandomizeAddedStatsOnEvolution { get; set; }
        public bool UpdateBaseStatsToGeneration { get; set; }
        public int UpdateBaseStatsGeneration { get; set; } = 6;

        public bool StandardizeExpCurves { get; set; }
        public int ExpCurveTargetIndex { get; set; }

        public FvxTraitsTypeMode Types { get; set; } = FvxTraitsTypeMode.Unchanged;
        public bool ForceDualTypes { get; set; }

        public FvxAbilitiesMode Abilities { get; set; } = FvxAbilitiesMode.Unchanged;
        public bool AllowWonderGuard { get; set; }
        public bool CombineDuplicateAbilities { get; set; }
        public bool EnsureTwoAbilities { get; set; }
        public bool FollowEvolutionsAbilities { get; set; }

        public FvxEvolutionsMode Evolutions { get; set; } = FvxEvolutionsMode.Unchanged;
    }

    public sealed class FvxStartersStaticsTradesSettings
    {
        public FvxStartersMode StartersMode { get; set; } = FvxStartersMode.Unchanged;
        public FvxTrainerPokemonMode StaticsMode { get; set; } = FvxTrainerPokemonMode.Unchanged;
        public FvxTradesMode TradesMode { get; set; } = FvxTradesMode.Unchanged;
    }

    public enum FvxTradesMode { Unchanged, RandomizeGivenOnly, RandomizeBoth }

    public sealed class FvxFoePokemonSettings
    {
        public FvxTrainerPokemonMode TrainerPokemon { get; set; } = FvxTrainerPokemonMode.Unchanged;
        public bool DontUseLegendaries { get; set; }
        public int SimilarStrengthWindowPercent { get; set; } = 20;
    }

    public sealed class FvxWildPokemonSettings
    {
        public FvxWildMode WildMode { get; set; } = FvxWildMode.Unchanged;
        public bool DontUseLegendaries { get; set; }
        public int LevelModifierPercent { get; set; }
        public bool UseLevelModifier { get; set; }
    }

    public sealed class FvxTmHmTutorExtrasSettings
    {
        public bool RandomizeTmMoveList { get; set; }
        public bool RandomizeTutorMoveList { get; set; }
        public bool NoGameBreakingMovesInTms { get; set; } = true;
    }

    public sealed class FvxItemsSettings
    {
        public FvxFieldItemsMode FieldItems { get; set; } = FvxFieldItemsMode.Unchanged;
        public FvxShopItemsMode ShopItems { get; set; } = FvxShopItemsMode.Unchanged;
        public FvxPickupMode Pickup { get; set; } = FvxPickupMode.Unchanged;
    }

    public sealed class FvxMiscTweaksSettings
    {
        public bool FastestText { get; set; }
        public bool GiveNationalDexAtStart { get; set; }
        public bool FastEggHatching { get; set; }
        public bool ForceChallengeMode { get; set; }
        public bool ForgettableHms { get; set; }
    }
}
