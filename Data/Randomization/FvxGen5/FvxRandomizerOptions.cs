namespace NewEditor.Data.Randomization.FvxGen5
{
    public enum FvxMovesetsMod
    {
        Unchanged,
        CompletelyRandom,
        RandomPreferSameType,
        MetronomeOnlyMode
    }

    public enum FvxTmHmCompatMod
    {
        Unchanged,
        CompletelyRandom,
        RandomPreferType,
        FullCompatibility
    }

    public enum FvxTutorCompatMod
    {
        Unchanged,
        CompletelyRandom,
        RandomPreferType,
        FullCompatibility
    }

    public enum FvxTmMoveMod
    {
        Unchanged,
        Random,
        RandomNoGameBreaking
    }

    public enum FvxTutorMoveMod
    {
        Unchanged,
        Random,
        RandomNoGameBreaking
    }

    /// <summary>UI + apply flags for FVX-style randomization (Gen 5).</summary>
    public sealed class FvxRandomizerOptions
    {
        public FvxMovesetsMod MovesetsMod { get; set; }
        public bool BlockBrokenMovesetMoves { get; set; }
        public bool StartWithGuaranteedMoves { get; set; }
        public int GuaranteedMoveCount { get; set; } = 4;
        public bool ReorderDamagingMoves { get; set; }
        public bool MovesetsForceGoodDamaging { get; set; }
        public int MovesetsGoodDamagingPercent { get; set; } = 50;
        public bool EvolutionMovesForAll { get; set; }

        public bool RandomizeEggMoves { get; set; }

        public FvxTmMoveMod TmMovesMod { get; set; }
        public bool KeepFieldMoveTms { get; set; }
        public bool TmsForceGoodDamaging { get; set; }
        public int TmsGoodDamagingPercent { get; set; } = 0;

        public FvxTmHmCompatMod TmHmCompatMod { get; set; }
        public bool TmLevelupMoveSanity { get; set; }
        public bool TmsFollowEvolutions { get; set; }
        public bool FullHmCompatibility { get; set; }

        public FvxTutorMoveMod TutorMovesMod { get; set; }
        public bool KeepFieldMoveTutors { get; set; }
        public bool TutorsForceGoodDamaging { get; set; }
        public int TutorsGoodDamagingPercent { get; set; } = 0;
        public FvxTutorCompatMod TutorCompatMod { get; set; }
        public bool TutorLevelupMoveSanity { get; set; }
        public bool TutorFollowEvolutions { get; set; }
    }
}
