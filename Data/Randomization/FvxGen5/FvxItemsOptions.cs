namespace NewEditor.Data.Randomization.FvxGen5
{
    public enum FvxFieldItemsMod
    {
        Unchanged = 0,
        Shuffle = 1,
        Random = 2,
        RandomEven = 3
    }

    public enum FvxShopItemsMod
    {
        Unchanged = 0,
        Shuffle = 1,
        Random = 2
    }

    public enum FvxPickupItemsMod
    {
        Unchanged = 0,
        Random = 1
    }

    public sealed class FvxItemsOptions
    {
        public FvxFieldItemsMod FieldItemsMod { get; set; } = FvxFieldItemsMod.Unchanged;
        public bool BanBadRandomFieldItems { get; set; }

        public FvxShopItemsMod ShopItemsMod { get; set; } = FvxShopItemsMod.Unchanged;
        public bool BanBadRandomShopItems { get; set; }
        public bool BanRegularShopItems { get; set; }
        public bool BanOverpoweredShopItems { get; set; }
        public bool GuaranteeEvolutionItems { get; set; }
        public bool GuaranteeXItems { get; set; }
        public bool BalanceShopPrices { get; set; }
        public bool AddCheapRareCandiesToShops { get; set; }

        public FvxPickupItemsMod PickupItemsMod { get; set; } = FvxPickupItemsMod.Unchanged;
        public bool BanBadRandomPickupItems { get; set; }

        public bool AnyRandomizationActive =>
            FieldItemsMod != FvxFieldItemsMod.Unchanged
            || ShopItemsMod != FvxShopItemsMod.Unchanged
            || PickupItemsMod != FvxPickupItemsMod.Unchanged
            || BalanceShopPrices
            || AddCheapRareCandiesToShops;
    }
}
