namespace NewEditor.Data.Randomization.FvxGen5
{
    public sealed class FvxMiscTweaksOptions
    {
        public bool FastestText { get; set; }
        public bool NationalDexAtStart { get; set; }
        public bool FastEggHatching { get; set; }
        public bool ForceChallengeMode { get; set; }
        public bool BanLuckyEgg { get; set; }
        public bool NoFreeLuckyEgg { get; set; }
        public bool BanBigMoneyManiacItems { get; set; }
        public bool RunWithoutRunningShoes { get; set; }
        public bool DisableLowHpMusic { get; set; }
        public bool ForgettableHms { get; set; }
        public bool BalanceStaticLevels { get; set; }

        public bool AnySelected =>
            FastestText
            || NationalDexAtStart
            || FastEggHatching
            || ForceChallengeMode
            || BanLuckyEgg
            || NoFreeLuckyEgg
            || BanBigMoneyManiacItems
            || RunWithoutRunningShoes
            || DisableLowHpMusic
            || ForgettableHms
            || BalanceStaticLevels;
    }
}
