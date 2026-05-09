namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Battle style randomization mode for trainer battles.</summary>
    public enum FvxFoeBattleStyleMode
    {
        Unchanged,
        Random,
        SingleStyle,
    }

    /// <summary>Settings for the FVX-style "Foe Pokemon" tab (Gen 5 BW/BW2).</summary>
    public sealed class FvxFoePokemonOptions
    {
        /// <summary>Match Starters tab / Gene Shuffle Fairy toggle for type upper bounds.</summary>
        public bool IncludeFairyTypes { get; set; }

        /// <summary>Primary trainer-Pokémon mode (UPR-ZX <c>TrainersMod</c>).</summary>
        public FvxFoeTrainerPokemonMode TrainerPokemonMode { get; set; }

        /// <summary>How trainer tiers (Boss / Important / Regular) are resolved.</summary>
        public FvxFoeTierDetectionMode TierDetectionMode { get; set; } = FvxFoeTierDetectionMode.MatchingVanillaUpr;

        public bool AdditionalPokemonBoss { get; set; }
        public bool AdditionalPokemonImportant { get; set; }
        public int AdditionalPokemonImportantCount { get; set; } = 1;
        public bool AdditionalPokemonRegular { get; set; }
        public int AdditionalPokemonRegularCount { get; set; } = 1;

        public bool HeldItemsBoss { get; set; }
        public bool HeldItemsImportant { get; set; }
        public bool HeldItemsRegular { get; set; }
        public bool HeldConsumableOnly { get; set; }
        public bool HeldSensibleItems { get; set; }
        public bool HeldHighestLevelOnly { get; set; }

        public bool DiverseTypesBoss { get; set; }
        public bool DiverseTypesImportant { get; set; }
        public bool DiverseTypesRegular { get; set; }

        public FvxFoeBattleStyleMode BattleStyleMode { get; set; }
        /// <summary>0–3: Single, Double, Triple, Rotation (matches Trainer Editor combo order).</summary>
        public int SingleStyleBattleType { get; set; }

        public bool RivalCarriesStarter { get; set; }
        public bool SimilarStrength { get; set; }
        public bool AvoidDuplicates { get; set; }
        public bool WeightTypesByCount { get; set; }
        public bool UseLocalPokemon { get; set; }
        public bool DontUseLegendaries { get; set; }
        public bool NoEarlyWonderGuard { get; set; }
        public bool AllowAlternateFormes { get; set; }

        /// <summary>
        /// BW2 only: include extended personal-table species (index 652+, e.g. Pokéstar Studios) in trainer pools.
        /// When off, pools stop before those entries.
        /// </summary>
        public bool AllowNonStandardPokemon { get; set; }

        public bool LeagueUniquePokemon { get; set; }
        public int LeagueUniqueCount { get; set; } = 1;

        public bool RandomizeTrainerNames { get; set; }
        public bool RandomizeTrainerClassNames { get; set; }

        public bool TrainersEvolvePokemon { get; set; }
        /// <summary>Percent chance / strength (-100 to 150) for evolution upgrades.</summary>
        public int TrainersEvolvePercent { get; set; }

        public bool LevelPercentModifierEnabled { get; set; }
        /// <summary>Percent level adjustment (-100 to 150).</summary>
        public int LevelPercentModifier { get; set; }

        /// <summary>
        /// BW1 only: use script starter species primary types for Striaton / trio gym themes (GYM1, GYM9, GYM10),
        /// matching UPR-FVX when the starter type triangle differs from vanilla.
        /// </summary>
        public bool Bw1TrioGymsMatchStarterTriangle { get; set; }

        public bool AnyRandomizationActive =>
            TrainerPokemonMode != FvxFoeTrainerPokemonMode.Unchanged
            || RandomizeTrainerNames
            || RandomizeTrainerClassNames
            || BattleStyleMode != FvxFoeBattleStyleMode.Unchanged
            || LevelPercentModifierEnabled
            || TrainersEvolvePokemon
            || AdditionalPokemonBoss || AdditionalPokemonImportant || AdditionalPokemonRegular
            || HeldItemsBoss || HeldItemsImportant || HeldItemsRegular
            || DiverseTypesBoss || DiverseTypesImportant || DiverseTypesRegular;
    }
}
