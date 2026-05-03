using NewEditor.Data.Randomization.FvxGen5;

namespace NewEditor.Data.Randomization.GeneShuffle
{
    public enum GeneShuffleTypeMode
    {
        FullRandom,
        FollowingEvolution,
        VanillaTypeLogic
    }

    /// <summary>Options for Gene Shuffle (types + nested FVX learnset step).</summary>
    public sealed class GeneShuffleOptions
    {
        public bool IncludeFairyTypes { get; set; }
        public GeneShuffleTypeMode TypeMode { get; set; }
        public FvxRandomizerOptions Fvx { get; set; } = new FvxRandomizerOptions();
    }
}
