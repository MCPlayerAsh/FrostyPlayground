using System;
using System.Text;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Stages not yet ported from UPR FVX (scripts, marts, ARM9 tweaks).</summary>
    public static class FvxGen5PlaceholderStages
    {
        public static void StartersStaticsTrades(FvxGen5FullSettings s, Random rnd, StringBuilder log)
        {
            if (s.StartersStaticsTrades.StartersMode == FvxStartersMode.Unchanged
                && s.StartersStaticsTrades.StaticsMode == FvxTrainerPokemonMode.Unchanged
                && s.StartersStaticsTrades.TradesMode == FvxTradesMode.Unchanged)
                return;
            log.AppendLine("[Starters / Statics / Trades] Not fully implemented — requires script/NARC mapping (FVX port). Options were not applied.");
        }

        public static void Items(FvxGen5FullSettings s, Random rnd, StringBuilder log)
        {
            if (s.Items.FieldItems == FvxFieldItemsMode.Unchanged
                && s.Items.ShopItems == FvxShopItemsMode.Unchanged
                && s.Items.Pickup == FvxPickupMode.Unchanged)
                return;
            log.AppendLine("[Items] Field / shop / pickup randomization not yet implemented (FVX port). Options were not applied.");
        }

        public static void MiscTweaks(FvxGen5FullSettings s, StringBuilder log)
        {
            if (!s.Misc.FastestText && !s.Misc.GiveNationalDexAtStart && !s.Misc.FastEggHatching
                && !s.Misc.ForceChallengeMode && !s.Misc.ForgettableHms)
                return;
            log.AppendLine("[Misc Tweaks] Binary patch modules not yet wired — options were not applied.");
        }

        public static void TmHmMoveListShuffle(FvxGen5FullSettings s, StringBuilder log)
        {
            if (!s.TmHmTutorExtras.RandomizeTmMoveList && !s.TmHmTutorExtras.RandomizeTutorMoveList) return;
            log.AppendLine("[TM/HM move lists] Randomizing TM/HM move table in ARM9 is not implemented yet. Compatibility flags in Core still apply.");
        }
    }
}
