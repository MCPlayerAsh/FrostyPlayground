namespace NewEditor.Data.Randomization.FvxGen5
{
    public enum FvxMovesetsMod
    {
        Unchanged,
        CompletelyRandom,
        RandomPreferSameType,
        MetronomeOnly
    }

    public enum FvxTmHmCompatMod
    {
        Unchanged,
        CompletelyRandom,
        RandomPreferType
    }

    public enum FvxTutorCompatMod
    {
        Unchanged,
        CompletelyRandom,
        RandomPreferType
    }

    /// <summary>UI + apply flags for FVX-style randomization (Gen 5).</summary>
    public sealed class FvxRandomizerOptions
    {
        public FvxMovesetsMod MovesetsMod { get; set; }
        public bool BlockBrokenMovesetMoves { get; set; }
        public bool StartWithGuaranteedMoves { get; set; }
        public int GuaranteedMoveCount { get; set; } = 4;
        public bool MovesetsForceGoodDamaging { get; set; }
        public int MovesetsGoodDamagingPercent { get; set; } = 50;
        public bool EvolutionMovesForAll { get; set; }

        public bool RandomizeEggMoves { get; set; }

        public FvxTmHmCompatMod TmHmCompatMod { get; set; }
        public bool TmsFollowEvolutions { get; set; }

        public FvxTutorCompatMod TutorCompatMod { get; set; }
        public bool TutorFollowEvolutions { get; set; }
    }
}
