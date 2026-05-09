namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>UPR-FVX <c>Gen5Constants.bw1IrregularFormes</c> / <c>bw2IrregularFormes</c> (national indices).</summary>
    internal static class FvxGen5IrregularFormes
    {
        /// <summary>When "No irregular alt formes" is on, exclude these species from pools (BW2 adds Kyurem formes).</summary>
        public static bool IsBannedWhenOptionOn(int nationalSpeciesIndex, bool bw2Rom)
        {
            switch (nationalSpeciesIndex)
            {
                case 662:
                case 663:
                case 664:
                case 666:
                case 667:
                    return true;
                case 668:
                case 669:
                    return bw2Rom;
                default:
                    return false;
            }
        }
    }
}
