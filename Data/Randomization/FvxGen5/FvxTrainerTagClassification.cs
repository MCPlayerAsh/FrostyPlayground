using System;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Mirrors UPR-ZX <see cref="Trainer.java"/> boss/important helpers.</summary>
    internal static class FvxTrainerTagClassification
    {
        public static bool IsBossTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            return tag.StartsWith("ELITE", StringComparison.Ordinal)
                   || tag.StartsWith("CHAMPION", StringComparison.Ordinal)
                   || tag.StartsWith("UBER", StringComparison.Ordinal)
                   || tag.EndsWith("LEADER", StringComparison.Ordinal);
        }

        public static bool IsImportantTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            return tag.StartsWith("RIVAL", StringComparison.Ordinal)
                   || tag.StartsWith("FRIEND", StringComparison.Ordinal)
                   || tag.EndsWith("STRONG", StringComparison.Ordinal);
        }

        /// <summary>UPR <c>Trainer.skipImportant()</c> — skip extra Pokémon on early rival slots.</summary>
        public static bool SkipImportant(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            return tag.StartsWith("RIVAL1-", StringComparison.Ordinal)
                   || tag.StartsWith("FRIEND1-", StringComparison.Ordinal)
                   || tag.EndsWith("NOTSTRONG", StringComparison.Ordinal);
        }

        /// <summary>UPR <c>Trainer.isFirstRivalOrFriend()</c>.</summary>
        public static bool IsFirstRivalOrFriendTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            return tag.StartsWith("RIVAL1-", StringComparison.Ordinal)
                   || tag.StartsWith("FRIEND1-", StringComparison.Ordinal);
        }

        /// <summary>UPR <c>Trainer.shouldNotGetBuffs()</c> — held items / easy-mode trainers.</summary>
        public static bool ShouldNotGetBuffs(string tag)
            => IsFirstRivalOrFriendTag(tag)
               || (!string.IsNullOrEmpty(tag) && tag.EndsWith("NOTSTRONG", StringComparison.Ordinal));

        /// <summary>League trainers for unique-species rule (Elite Four + Champion).</summary>
        public static bool IsLeagueTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            return tag.StartsWith("ELITE", StringComparison.Ordinal)
                   || tag.StartsWith("CHAMPION", StringComparison.Ordinal);
        }

        public static FvxFoeTrainerTier ClassifyTier(string tag, bool useTags)
        {
            if (!useTags || string.IsNullOrEmpty(tag))
                return FvxFoeTrainerTier.Regular;
            if (IsBossTag(tag)) return FvxFoeTrainerTier.Boss;
            if (IsImportantTag(tag)) return FvxFoeTrainerTier.Important;
            return FvxFoeTrainerTier.Regular;
        }

        /// <summary>Type-themed groups share one type (UPR randomizer).</summary>
        public static bool IsTypeThemedGroupKey(string groupKey)
        {
            if (string.IsNullOrEmpty(groupKey)) return false;
            if (groupKey.StartsWith("GYM", StringComparison.Ordinal)) return true;
            if (groupKey.StartsWith("ELITE", StringComparison.Ordinal)) return true;
            if (groupKey.StartsWith("CHAMPION", StringComparison.Ordinal)) return true;
            if (groupKey.StartsWith("THEMED", StringComparison.Ordinal)) return true;
            return false;
        }

        /// <summary>Stable group id for shared type (gym block, elite member, or full THEMED:… tag).</summary>
        public static string GroupKeyFromTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return "";
            if (tag.StartsWith("THEMED:", StringComparison.Ordinal))
                return tag;
            int dash = tag.IndexOf('-');
            return dash < 0 ? tag : tag.Substring(0, dash);
        }
    }
}
