using System;
using System.Collections.Generic;
using System.Text;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    internal static class FvxGen5ItemsRandomizer
    {
        public static void Apply(FvxItemsSettings s, Random rnd, StringBuilder log)
        {
            if (s.FieldItems != FvxFieldItemsMode.Unchanged)
            {
                if (!FvxGen5FieldItemsRandomizer.TryApply(s.FieldItems, rnd, log))
                { /* errors logged */ }
            }

            if (s.ShopItems != FvxShopItemsMode.Unchanged)
            {
                if (MainEditor.pokemartNarc?.shops == null)
                    log.AppendLine("[Shop items] Poké Mart NARC not loaded (BW2 only) — skipped.");
                else
                    ApplyShops(s.ShopItems, rnd, log);
            }

            if (!FvxGen5MoveListStages.ApplyPickupRandom(s, rnd, log))
            {
                // Error already logged by ApplyPickupRandom when failing critically
            }
        }

        static void ApplyShops(FvxShopItemsMode mode, Random rnd, StringBuilder log)
        {
            var shops = MainEditor.pokemartNarc.shops;
            int maxItem = MainEditor.itemDataNarc?.items != null
                ? Math.Max(1, MainEditor.itemDataNarc.items.Count - 1)
                : 639;

            foreach (var shop in shops)
            {
                if (shop?.items == null || shop.items.Count == 0) continue;

                switch (mode)
                {
                    case FvxShopItemsMode.Shuffle:
                        shop.items.Shuffle(rnd);
                        break;
                    case FvxShopItemsMode.Random:
                        int n = shop.items.Count;
                        shop.items.Clear();
                        for (int i = 0; i < n; i++)
                            shop.items.Add(rnd.Next(1, maxItem + 1));
                        break;
                }

                shop.Apply();
            }

            log.AppendLine("[Shop items] Applied mode: " + mode + ".");
        }
    }
}
