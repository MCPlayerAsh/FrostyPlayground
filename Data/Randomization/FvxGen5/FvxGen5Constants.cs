namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Gen 5 layout values aligned with UPR FVX Gen5Constants / Gen5RomHandler TM table scan.
    /// </summary>
    internal static class FvxGen5Constants
    {
        /// <summary>ARM9 search prefix for TM/HM move table (hex string "87038803" in FVX).</summary>
        public static readonly byte[] TmDataPrefixBytes = { 0x87, 0x03, 0x88, 0x03 };

        public const int TmCount = 95;
        public const int HmCount = 6;
        public const int TmBlockOneCount = 92;
        public const int NationalDexCount = 649;

        /// <summary>FVX GlobalConstants.MIN_DAMAGING_MOVE_POWER</summary>
        public const int MinDamagingMovePower = 50;

        public const int CutMoveId = 15;
        public const int StruggleMoveId = 165;

        /// <summary>Gen 5 Metronome move ID.</summary>
        public const int MetronomeMoveId = 118;
    }
}
