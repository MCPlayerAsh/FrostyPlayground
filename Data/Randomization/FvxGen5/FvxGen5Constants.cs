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

        /// <summary>UPR Gen5Constants.pickupTableLocator hex "19005C00DD00".</summary>
        public static readonly byte[] PickupTableLocatorBytes = { 0x19, 0x00, 0x5C, 0x00, 0xDD, 0x00 };

        public const int Bw2MoveTutorCount = 60;
        public const int Bw2MoveTutorBytesPerEntry = 12;
        public const int NumberOfPickupItems = 29;

        public const int MoveTutorBw2OverlayId = 36;
    }
}
