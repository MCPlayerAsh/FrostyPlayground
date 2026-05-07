using System;
using System.Text;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Stages partially ported; remaining options log as skipped.</summary>
    public static class FvxGen5PlaceholderStages
    {
        public static void StartersStaticsTrades(FvxGen5FullSettings s, Random rnd, StringBuilder log)
        {
            if (s.StartersStaticsTrades.StartersMode == FvxStartersMode.Unchanged
                && s.StartersStaticsTrades.StaticsMode == FvxTrainerPokemonMode.Unchanged
                && s.StartersStaticsTrades.TradesMode == FvxTradesMode.Unchanged)
                return;

            FvxGen5StartersRandomizer.Apply(s.StartersStaticsTrades, rnd, log);

            FvxGen5StaticsTradesRandomizer.ApplyStatics(s.StartersStaticsTrades, s.Foe, rnd, log);
            FvxGen5StaticsTradesRandomizer.ApplyTrades(s.StartersStaticsTrades.TradesMode, s.Foe, rnd, log);
        }

        public static void Items(FvxGen5FullSettings s, Random rnd, StringBuilder log)
        {
            if (s.Items.FieldItems == FvxFieldItemsMode.Unchanged
                && s.Items.ShopItems == FvxShopItemsMode.Unchanged
                && s.Items.Pickup == FvxPickupMode.Unchanged)
                return;

            FvxGen5ItemsRandomizer.Apply(s.Items, rnd, log);
        }

        public static void MiscTweaks(FvxGen5FullSettings s, StringBuilder log)
        {
            FvxGen5MiscTweaks.Apply(s.Misc, log);
        }

    }
}
