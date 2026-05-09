using System.Collections.Generic;

namespace NewEditor.Data.Randomization.FvxGen5
{
    internal static class FvxGen5MiscRuntimeState
    {
        static readonly HashSet<int> _bannedItems = new HashSet<int>();

        public static IReadOnlyCollection<int> BannedItems => _bannedItems;

        public static void Reset()
        {
            _bannedItems.Clear();
        }

        public static void BanItem(int itemId)
        {
            if (itemId > 0)
                _bannedItems.Add(itemId);
        }

        public static bool IsBannedItem(int itemId) => itemId > 0 && _bannedItems.Contains(itemId);
    }
}
