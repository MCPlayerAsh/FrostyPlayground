namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Readable constants for the BW/BW2 evolution method IDs as stored in the evolutions NARC.
    /// IDs match <c>VersionConstants.BW2_EvolutionMethodNames</c> indices.
    /// </summary>
    internal static class FvxGen5EvolutionMethods
    {
        public const int None = 0;
        public const int LevelUpHighFriendship = 1;
        public const int FriendshipDay = 2;
        public const int FriendshipNight = 3;
        public const int LevelUp = 4;
        public const int Trade = 5;
        public const int TradeHeldItem = 6;
        public const int TradeKarrablastShelmet = 7;
        public const int Stone = 8;
        public const int LevelUpAttackHigher = 9;
        public const int LevelUpAttackEqualDefense = 10;
        public const int LevelUpAttackLower = 11;
        public const int LevelUpSilcoon = 12;
        public const int LevelUpCascoon = 13;
        public const int LevelUpNinjask = 14;
        public const int LevelUpShedinja = 15;
        public const int LevelUpHighBeauty = 16;
        public const int StoneMale = 17;
        public const int StoneFemale = 18;
        public const int LevelUpHeldItemDay = 19;
        public const int LevelUpHeldItemNight = 20;
        public const int LevelUpKnownMove = 21;
        public const int LevelUpKnownPokemon = 22;
        public const int LevelUpMale = 23;
        public const int LevelUpFemale = 24;
        public const int LevelUpElectricCave = 25;
        public const int LevelUpMossyRock = 26;
        public const int LevelUpIceRock = 27;

        /// <summary>True for any "level up" style method (we can swap in a level threshold).</summary>
        public static bool IsLevelMethod(int method)
        {
            switch (method)
            {
                case LevelUp:
                case LevelUpAttackHigher:
                case LevelUpAttackEqualDefense:
                case LevelUpAttackLower:
                case LevelUpSilcoon:
                case LevelUpCascoon:
                case LevelUpNinjask:
                case LevelUpShedinja:
                case LevelUpMale:
                case LevelUpFemale:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>True for methods that read the <c>condition</c> field as a level threshold.</summary>
        public static bool ConditionIsLevel(int method)
        {
            switch (method)
            {
                case LevelUp:
                case LevelUpAttackHigher:
                case LevelUpAttackEqualDefense:
                case LevelUpAttackLower:
                case LevelUpSilcoon:
                case LevelUpCascoon:
                case LevelUpNinjask:
                case LevelUpShedinja:
                case LevelUpMale:
                case LevelUpFemale:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Methods that ordinarily can't happen on a real cart without trading or a friend.</summary>
        public static bool IsImpossibleWithoutTrade(int method)
        {
            switch (method)
            {
                case Trade:
                case TradeHeldItem:
                case TradeKarrablastShelmet:
                case LevelUpKnownPokemon:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Time-of-day-dependent methods.</summary>
        public static bool IsTimeBased(int method)
        {
            switch (method)
            {
                case FriendshipDay:
                case FriendshipNight:
                case LevelUpHeldItemDay:
                case LevelUpHeldItemNight:
                    return true;
                default:
                    return false;
            }
        }
    }
}
