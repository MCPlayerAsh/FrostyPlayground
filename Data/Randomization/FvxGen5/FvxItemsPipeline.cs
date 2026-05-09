using System;
using System.Collections.Generic;
using System.Linq;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public static class FvxItemsPipeline
    {
        const int ScriptListTerminator = 0xFD13;
        const int NormalItemSetVarCommand = 0x28;
        const int HiddenItemSetVarCommand = 0x2A;
        const int NormalItemVarSet = 0x800C;
        const int HiddenItemVarSet = 0x8000;
        const string PickupTableLocatorHex = "19005C00DD00";
        const int PickupItemCount = 29;

        sealed class FieldItemSlot
        {
            public ScriptFile Script { get; set; }
            public int ItemOffset { get; set; }
            public int ItemId { get; set; }
            public bool IsTm { get; set; }
        }

        public static bool TryRun(FvxItemsOptions opt, Random rnd, out string error)
        {
            error = null;
            if (opt == null)
            {
                error = "Items options are null.";
                return false;
            }
            if (!opt.AnyRandomizationActive)
                return true;

            if (MainEditor.fileSystem == null)
            {
                error = "No file system loaded.";
                return false;
            }

            NormalizeOptionsForRomCapabilities(opt);

            if (!TryRandomizeFieldItems(opt, rnd, out error))
                return false;

            if (!TryRandomizePickupItems(opt, rnd, out error))
                return false;

            if (!TryRandomizeShops(opt, rnd, out error))
                return false;

            if (opt.BalanceShopPrices && !TryBalanceShopPrices(out error))
                return false;

            if (opt.AddCheapRareCandiesToShops && !TryAddCheapRareCandies(out error))
                return false;

            return true;
        }

        static void NormalizeOptionsForRomCapabilities(FvxItemsOptions opt)
        {
            // Mirrors UPR Settings.tweakForRom() behavior for item options:
            // - no shop support => shop randomization/pricing options disabled
            // - no shop size support => add-cheap-rare-candies disabled
            bool hasShopSupport = MainEditor.pokemartNarc?.shops != null && MainEditor.itemDataNarc?.items != null;
            if (!hasShopSupport)
            {
                opt.ShopItemsMod = FvxShopItemsMod.Unchanged;
                opt.BalanceShopPrices = false;
            }

            bool canChangeShopSizes = MainEditor.pokemartItemCountNarc?.itemCounts != null && hasShopSupport;
            if (!canChangeShopSizes)
                opt.AddCheapRareCandiesToShops = false;
        }

        static bool TryRandomizeFieldItems(FvxItemsOptions opt, Random rnd, out string error)
        {
            error = null;
            if (opt.FieldItemsMod == FvxFieldItemsMod.Unchanged)
                return true;

            if (MainEditor.scriptNarc?.scriptFiles == null || MainEditor.itemDataNarc?.items == null)
            {
                error = "Script or item data is not loaded.";
                return false;
            }

            if (!GetFieldScriptOffsets(out int normalOffset, out int hiddenOffset))
            {
                error = "Field item randomization supports Black/White and Black 2/White 2 only.";
                return false;
            }

            var itemNames = MainEditor.textNarc?.textFiles?[VersionConstants.ItemNameTextFileID]?.text;
            var slots = new List<FieldItemSlot>();
            if (!CollectFieldItemSlots(normalOffset, NormalItemSetVarCommand, NormalItemVarSet, itemNames, slots, out error))
                return false;
            if (!CollectFieldItemSlots(hiddenOffset, HiddenItemSetVarCommand, HiddenItemVarSet, itemNames, slots, out error))
                return false;
            if (slots.Count == 0)
                return true;

            var tmPool = slots.Where(s => s.IsTm).Select(s => s.ItemId).ToList();
            var nonTmPool = slots.Where(s => !s.IsTm).Select(s => s.ItemId).ToList();

            switch (opt.FieldItemsMod)
            {
                case FvxFieldItemsMod.Shuffle:
                    Shuffle(tmPool, rnd);
                    Shuffle(nonTmPool, rnd);
                    break;
                case FvxFieldItemsMod.Random:
                case FvxFieldItemsMod.RandomEven:
                    var requiredTmIds = slots.Where(s => s.IsTm).Select(s => s.ItemId).Distinct().ToList();
                    if (!BuildRandomFieldPools(opt, rnd, itemNames, tmPool.Count, nonTmPool.Count, requiredTmIds, out tmPool, out nonTmPool, out error))
                        return false;
                    break;
            }

            int tmAt = 0;
            int nonTmAt = 0;
            var touched = new HashSet<ScriptFile>();
            foreach (var slot in slots)
            {
                int next = slot.IsTm ? tmPool[tmAt++] : nonTmPool[nonTmAt++];
                HelperFunctions.WriteShort(slot.Script.bytes, slot.ItemOffset, next);
                touched.Add(slot.Script);
            }
            foreach (var sf in touched)
                sf.ApplyData();
            return true;
        }

        static bool TryRandomizePickupItems(FvxItemsOptions opt, Random rnd, out string error)
        {
            error = null;
            if (opt.PickupItemsMod == FvxPickupItemsMod.Unchanged)
                return true;

            int overlayIndex = MainEditor.RomType == RomType.BW2 ? 166 : MainEditor.RomType == RomType.BW1 ? 92 : -1;
            if (overlayIndex < 0 || MainEditor.fileSystem?.overlays == null || overlayIndex >= MainEditor.fileSystem.overlays.Count)
            {
                error = "Pickup overlay is unavailable for this ROM.";
                return false;
            }
            var y9 = MainEditor.fileSystem.y9?.entries;
            bool wasCompressed = y9 != null && overlayIndex < y9.Count && y9[overlayIndex].compressed;
            byte[] raw = MainEditor.fileSystem.overlays[overlayIndex].ToArray();
            byte[] work = wasCompressed ? BLZDecoder.BLZ_DecodePub(raw) : raw;
            if (work == null || work.Length == 0)
            {
                error = "Pickup overlay could not be read or decompressed.";
                return false;
            }

            byte[] locator = HexToBytes(PickupTableLocatorHex);
            int tableOffset = IndexOf(work, locator);
            if (tableOffset < 0 || tableOffset + PickupItemCount * 2 > work.Length)
            {
                // UPR behavior: if a game/ROM variant cannot expose pickup table, effectively no-op.
                return true;
            }

            var itemNames = MainEditor.textNarc?.textFiles?[VersionConstants.ItemNameTextFileID]?.text;
            var pool = BuildAllowedItemPool(itemNames, includeTms: false, banBad: opt.BanBadRandomPickupItems);
            if (pool.Count == 0)
            {
                error = "No candidate items available for pickup randomization.";
                return false;
            }

            for (int i = 0; i < PickupItemCount; i++)
            {
                int next = pool[rnd.Next(pool.Count)];
                HelperFunctions.WriteShort(work, tableOffset + i * 2, next);
            }

            if (wasCompressed)
            {
                byte[] enc = BLZDecoder.BLZ_EncodePub(work, true);
                if (enc == null || enc.Length == 0)
                {
                    error = "Pickup overlay recompression failed.";
                    return false;
                }
                MainEditor.fileSystem.overlays[overlayIndex] = new List<byte>(enc);
                if (y9 != null && overlayIndex < y9.Count)
                {
                    y9[overlayIndex].compressed = true;
                    y9[overlayIndex].compressedSize = enc.Length;
                    y9[overlayIndex].Apply();
                }
            }
            else
            {
                MainEditor.fileSystem.overlays[overlayIndex] = new List<byte>(work);
            }
            return true;
        }

        static bool GetFieldScriptOffsets(out int normalOffset, out int hiddenOffset)
        {
            if (MainEditor.RomType == RomType.BW2)
            {
                normalOffset = 1240;
                hiddenOffset = 1241;
                return true;
            }
            if (MainEditor.RomType == RomType.BW1)
            {
                normalOffset = 864;
                hiddenOffset = 865;
                return true;
            }
            normalOffset = -1;
            hiddenOffset = -1;
            return false;
        }

        static bool CollectFieldItemSlots(int scriptFileIndex, int commandId, int varId, IList<string> itemNames, List<FieldItemSlot> outSlots, out string error)
        {
            error = null;
            if (scriptFileIndex < 0 || scriptFileIndex >= MainEditor.scriptNarc.scriptFiles.Count)
            {
                error = "Field item script offset is out of range for this ROM.";
                return false;
            }
            var sf = MainEditor.scriptNarc.scriptFiles[scriptFileIndex];
            var bytes = sf.bytes;
            if (bytes == null || bytes.Length < 8)
                return true;

            int pos = 0;
            while (pos < bytes.Length - 4 && HelperFunctions.ReadShort(bytes, pos) != ScriptListTerminator)
            {
                int ptr = HelperFunctions.ReadInt(bytes, pos);
                int target = pos + ptr + 4;
                if (target >= 0 && target + 8 <= bytes.Length)
                {
                    int cmd = HelperFunctions.ReadShort(bytes, target + 2);
                    int set = HelperFunctions.ReadShort(bytes, target + 4);
                    if (cmd == commandId && set == varId)
                    {
                        int itemId = HelperFunctions.ReadShort(bytes, target + 6);
                        outSlots.Add(new FieldItemSlot
                        {
                            Script = sf,
                            ItemOffset = target + 6,
                            ItemId = itemId,
                            IsTm = IsTmItemId(itemId, itemNames)
                        });
                    }
                }
                pos += 4;
            }
            return true;
        }

        static bool BuildRandomFieldPools(FvxItemsOptions opt, Random rnd, IList<string> itemNames, int tmCount, int nonTmCount, IList<int> requiredTmIds,
            out List<int> tmPool, out List<int> nonTmPool, out string error)
        {
            error = null;
            tmPool = new List<int>();
            nonTmPool = new List<int>();

            var tmCandidates = BuildAllowedItemPool(itemNames, includeTms: true, banBad: false).Where(i => IsTmItemId(i, itemNames)).Distinct().ToList();
            tmPool.AddRange((requiredTmIds ?? Array.Empty<int>()).Distinct().Where(i => tmCandidates.Contains(i)));
            if (tmPool.Count > tmCount)
            {
                error = "Could not randomize field TM items: required TM set exceeds TM slots.";
                return false;
            }

            if (tmCandidates.Count < tmCount)
            {
                error = "Could not randomize field TM items: not enough TM candidates.";
                return false;
            }
            Shuffle(tmCandidates, rnd);
            foreach (int id in tmCandidates)
            {
                if (tmPool.Count >= tmCount) break;
                if (!tmPool.Contains(id)) tmPool.Add(id);
            }

            var nonTmCandidates = BuildAllowedItemPool(itemNames, includeTms: false, banBad: opt.BanBadRandomFieldItems);
            if (nonTmCandidates.Count == 0 && nonTmCount > 0)
            {
                error = "Could not randomize field items: non-TM pool is empty.";
                return false;
            }

            if (opt.FieldItemsMod == FvxFieldItemsMod.RandomEven)
            {
                var deck = new List<int>(nonTmCandidates);
                Shuffle(deck, rnd);
                for (int i = 0; i < nonTmCount; i++)
                {
                    if (deck.Count == 0)
                    {
                        deck.AddRange(nonTmCandidates);
                        Shuffle(deck, rnd);
                    }
                    nonTmPool.Add(deck[deck.Count - 1]);
                    deck.RemoveAt(deck.Count - 1);
                }
            }
            else
            {
                for (int i = 0; i < nonTmCount; i++)
                    nonTmPool.Add(nonTmCandidates[rnd.Next(nonTmCandidates.Count)]);
            }
            return true;
        }

        static bool TryRandomizeShops(FvxItemsOptions opt, Random rnd, out string error)
        {
            error = null;
            if (opt.ShopItemsMod == FvxShopItemsMod.Unchanged)
                return true;

            if (MainEditor.pokemartNarc?.shops == null || MainEditor.itemDataNarc?.items == null)
            {
                error = "Shop or item data is not loaded.";
                return false;
            }

            var shops = MainEditor.pokemartNarc.shops;
            var specialShops = GetSpecialShops(shops);
            if (specialShops.Count == 0)
            {
                error = "No special shops found.";
                return false;
            }

            var itemNames = MainEditor.textNarc?.textFiles?[VersionConstants.ItemNameTextFileID]?.text;
            var allItemIds = Enumerable.Range(1, MainEditor.itemDataNarc.items.Count - 1).ToList();

            if (opt.ShopItemsMod == FvxShopItemsMod.Shuffle)
            {
                var allSpecialItems = new List<int>();
                foreach (var sh in specialShops)
                    allSpecialItems.AddRange(sh.items);
                Shuffle(allSpecialItems, rnd);
                int cursor = 0;
                foreach (var sh in specialShops)
                {
                    for (int i = 0; i < sh.items.Count; i++)
                        sh.items[i] = allSpecialItems[cursor++];
                    sh.Apply();
                }
                UpdateShopCounts();
                return true;
            }

            var candidatePool = BuildShopCandidatePool(allItemIds, itemNames, opt);
            if (candidatePool.Count == 0)
            {
                error = "No candidate shop items remain after filters.";
                return false;
            }

            var guaranteed = BuildGuaranteedPool(itemNames, opt);
            guaranteed.RemoveWhere(i => !candidatePool.Contains(i));

            var nonMainGame = specialShops.Where(s => !IsMainGameShop(s)).ToList();
            var mainGame = specialShops.Where(IsMainGameShop).ToList();

            var allSlots = specialShops.Sum(s => s.items.Count);
            if (guaranteed.Count > allSlots)
            {
                error = "Too many guaranteed items for available shop slots.";
                return false;
            }

            var newItems = new List<int>(guaranteed);
            var poolForFill = new List<int>(candidatePool.Where(i => !guaranteed.Contains(i)));
            if (poolForFill.Count == 0 && newItems.Count < allSlots)
            {
                error = "No non-guaranteed shop items available for remaining slots.";
                return false;
            }
            while (newItems.Count < allSlots)
                newItems.Add(poolForFill[rnd.Next(poolForFill.Count)]);

            Shuffle(newItems, rnd);
            PlaceIntoShops(nonMainGame, newItems, guaranteed, skipGuaranteed: true);
            PlaceIntoShops(mainGame, newItems, guaranteed, skipGuaranteed: false);
            if (newItems.Count != 0)
            {
                error = "Internal shop item placement mismatch.";
                return false;
            }

            foreach (var sh in specialShops)
                sh.Apply();
            UpdateShopCounts();
            return true;
        }

        static void PlaceIntoShops(List<PokemartEntry> shops, List<int> newItems, HashSet<int> guaranteed, bool skipGuaranteed)
        {
            foreach (var sh in shops)
            {
                for (int i = 0; i < sh.items.Count; i++)
                {
                    int pickIdx = 0;
                    if (skipGuaranteed)
                    {
                        while (pickIdx < newItems.Count && guaranteed.Contains(newItems[pickIdx]))
                            pickIdx++;
                        if (pickIdx >= newItems.Count)
                            pickIdx = 0;
                    }
                    sh.items[i] = newItems[pickIdx];
                    newItems.RemoveAt(pickIdx);
                }
            }
        }

        static bool TryBalanceShopPrices(out string error)
        {
            error = null;
            var items = MainEditor.itemDataNarc?.items;
            if (items == null)
            {
                error = "Item data is not loaded.";
                return false;
            }

            for (int i = 1; i < items.Count; i++)
            {
                int current = items[i].BuyPrice;
                if (current <= 0) continue;
                int balanced = current / 2;
                balanced = Math.Max(200, balanced);
                balanced = (balanced / 10) * 10;
                items[i].BuyPrice = balanced;
            }
            return true;
        }

        static bool TryAddCheapRareCandies(out string error)
        {
            error = null;
            if (MainEditor.pokemartNarc?.shops == null || MainEditor.itemDataNarc?.items == null)
            {
                error = "Shop or item data is not loaded.";
                return false;
            }

            int rareCandy = FindByNameContains("rare candy");
            if (rareCandy <= 0)
            {
                error = "Could not find Rare Candy item.";
                return false;
            }

            foreach (var shop in MainEditor.pokemartNarc.shops)
            {
                if (!shop.items.Contains(rareCandy))
                    shop.items.Add(rareCandy);
                shop.Apply();
            }
            UpdateShopCounts();
            MainEditor.itemDataNarc.items[rareCandy].BuyPrice = 100;
            return true;
        }

        static List<PokemartEntry> GetSpecialShops(List<PokemartEntry> shops)
            => shops.Where((_, idx) => idx >= 6).ToList();

        static bool IsMainGameShop(PokemartEntry shop)
        {
            string n = shop?.name?.ToLowerInvariant() ?? "";
            if (n.Contains("black city") || n.Contains("mall") || n.Contains("sm9"))
                return false;
            return true;
        }

        static List<int> BuildShopCandidatePool(List<int> all, IList<string> names, FvxItemsOptions opt)
        {
            var pool = new HashSet<int>(all);
            pool.RemoveWhere(FvxGen5MiscRuntimeState.IsBannedItem);
            if (opt.BanBadRandomShopItems)
                pool.RemoveWhere(i => IsBadItemName(NameAt(names, i)));
            if (opt.BanRegularShopItems)
                pool.RemoveWhere(i => IsRegularShopItemName(NameAt(names, i)));
            if (opt.BanOverpoweredShopItems)
                pool.RemoveWhere(i => IsOverpoweredShopItemName(NameAt(names, i)));
            return pool.ToList();
        }

        static HashSet<int> BuildGuaranteedPool(IList<string> names, FvxItemsOptions opt)
        {
            var result = new HashSet<int>();
            if (opt.GuaranteeEvolutionItems)
            {
                for (int i = 1; i < names.Count; i++)
                    if (IsEvolutionItemName(NameAt(names, i)))
                        result.Add(i);
            }
            if (opt.GuaranteeXItems)
            {
                for (int i = 1; i < names.Count; i++)
                    if (IsXItemName(NameAt(names, i)))
                        result.Add(i);
            }
            result.RemoveWhere(FvxGen5MiscRuntimeState.IsBannedItem);
            return result;
        }

        static void UpdateShopCounts()
        {
            if (MainEditor.pokemartItemCountNarc?.itemCounts == null || MainEditor.pokemartNarc?.shops == null)
                return;

            int n = Math.Min(MainEditor.pokemartItemCountNarc.itemCounts.Count, MainEditor.pokemartNarc.shops.Count);
            for (int i = 0; i < n; i++)
                MainEditor.pokemartItemCountNarc.itemCounts[i] = (byte)Math.Min(255, MainEditor.pokemartNarc.shops[i].items.Count);
        }

        static int FindByNameContains(string token)
        {
            var names = MainEditor.textNarc?.textFiles?[VersionConstants.ItemNameTextFileID]?.text;
            if (names == null) return -1;
            string low = token.ToLowerInvariant();
            for (int i = 1; i < names.Count; i++)
            {
                var nm = NameAt(names, i);
                if (nm.Contains(low))
                    return i;
            }
            return -1;
        }

        static string NameAt(IList<string> names, int idx)
        {
            if (names == null || idx < 0 || idx >= names.Count || names[idx] == null)
                return "";
            return names[idx].ToLowerInvariant();
        }

        static List<int> BuildAllowedItemPool(IList<string> names, bool includeTms, bool banBad)
        {
            int count = MainEditor.itemDataNarc?.items?.Count ?? 0;
            var outIds = new List<int>();
            for (int i = 1; i < count; i++)
            {
                if (FvxGen5MiscRuntimeState.IsBannedItem(i))
                    continue;
                string nm = NameAt(names, i);
                if (string.IsNullOrWhiteSpace(nm))
                    continue;
                if (!includeTms && IsTmName(nm))
                    continue;
                if (IsLikelyKeyOrInvalidItemName(nm))
                    continue;
                if (banBad && IsBadItemName(nm))
                    continue;
                outIds.Add(i);
            }
            return outIds;
        }

        static bool IsTmItemId(int itemId, IList<string> names)
        {
            string n = NameAt(names, itemId);
            return IsTmName(n);
        }

        static bool IsTmName(string name)
            => name.StartsWith("tm") || name.StartsWith("hm");

        static bool IsLikelyKeyOrInvalidItemName(string name)
            => name == "none"
               || name == "cancel"
               || name.Contains("key item")
               || name.Contains("liberty pass")
               || name.Contains("town map")
               || name.Contains("xtransceiver")
               || name.Contains("medal box")
               || name.Contains("dna splicers")
               || name.Contains("bike")
               || name.Contains("rod");

        static bool IsBadItemName(string name)
            => name.Contains("mail")
               || name.Contains("mulch")
               || name.Contains("flute")
               || name.Contains("shard")
               || name.Contains("relic")
               || name.Contains("pretty")
               || name.Contains("tinymushroom")
               || name.Contains("balmmushroom")
               || name.Contains("nugget")
               || name.Contains("big pearl")
               || name.Contains("stardust");

        static bool IsRegularShopItemName(string name)
            => name.Contains("potion")
               || name.Contains("super potion")
               || name.Contains("hyper potion")
               || name.Contains("max potion")
               || name.Contains("revive")
               || name.Contains("antidote")
               || name.Contains("awakening")
               || name.Contains("paralyze heal")
               || name.Contains("full heal")
               || name.Contains("repel")
               || name.Contains("pokeball")
               || name.Contains("great ball")
               || name.Contains("ultra ball");

        static bool IsOverpoweredShopItemName(string name)
            => name.Contains("master ball")
               || name.Contains("max revive")
               || name.Contains("rare candy")
               || name.Contains("pp max")
               || name.Contains("full restore")
               || name.Contains("choice")
               || name.Contains("life orb")
               || name.Contains("focus sash");

        static bool IsEvolutionItemName(string name)
            => name.Contains("stone")
               || name.Contains("protector")
               || name.Contains("electirizer")
               || name.Contains("magmarizer")
               || name.Contains("reaper cloth")
               || name.Contains("razor fang")
               || name.Contains("razor claw")
               || name.Contains("dubious disc")
               || name.Contains("upgrade")
               || name.Contains("oval stone");

        static bool IsXItemName(string name)
            => name.StartsWith("x ") || name.StartsWith("x-");

        static void Shuffle<T>(IList<T> list, Random rnd)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                T t = list[i];
                list[i] = list[j];
                list[j] = t;
            }
        }

        static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex) || (hex.Length % 2) != 0)
                return Array.Empty<byte>();
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }

        static int IndexOf(byte[] haystack, byte[] needle)
        {
            if (haystack == null || needle == null || needle.Length == 0 || haystack.Length < needle.Length)
                return -1;
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                bool ok = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok) return i;
            }
            return -1;
        }
    }
}
