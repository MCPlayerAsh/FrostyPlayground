using System;
using System.Collections.Generic;
using System.Linq;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    internal static class FvxWildPokemonRunner
    {
        public static bool TryRun(FvxWildPokemonOptions opt, Random rnd, out string error)
        {
            error = null;
            try
            {
                var pokemon = MainEditor.pokemonDataNarc.pokemon;
                if (opt.SetMinimumCatchRate)
                    ApplyMinimumCatchRate(opt, pokemon);
                if (opt.RandomizeHeldItems)
                    RandomizeWildHeldItems(opt, pokemon, rnd);

                if (opt.RandomizeWildPokemon || opt.LevelModifierEnabled)
                    RandomizeEncounters(opt, pokemon, MainEditor.encounterNarc.encounterPools, rnd);

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        static void ApplyMinimumCatchRate(FvxWildPokemonOptions opt, List<PokemonEntry> pokemon)
        {
            var floors = MinCatchRateByLevel(opt.MinimumCatchRateLevel);
            for (int i = 1; i < pokemon.Count; i++)
            {
                var p = pokemon[i];
                int floor = FvxGen5StartersStaticsTradesRunner.IsLegendaryNationalDex(i) ? floors.legendary : floors.normal;
                if (p.catchRate < floor)
                {
                    p.catchRate = (byte)Math.Min(255, floor);
                    p.ApplyData();
                }
            }
        }

        static (int normal, int legendary) MinCatchRateByLevel(int level)
        {
            switch (level)
            {
                case 5: return (255, 255);
                case 4: return (200, 100);
                case 3: return (150, 75);
                case 2: return (100, 50);
                default: return (50, 25);
            }
        }

        static void RandomizeWildHeldItems(FvxWildPokemonOptions opt, List<PokemonEntry> pokemon, Random rnd)
        {
            var names = MainEditor.textNarc?.textFiles?[VersionConstants.ItemNameTextFileID]?.text;
            int count = Math.Min(names?.Count ?? 0, MainEditor.itemDataNarc?.items?.Count ?? 0);
            var pool = BuildWildHeldItemPool(opt, names, count);
            if (pool.Count == 0) return;

            for (int i = 1; i < pokemon.Count; i++)
            {
                var p = pokemon[i];
                p.heldItem1 = 0;
                p.heldItem2 = 0;
                p.heldItem3 = 0;

                double roll = rnd.NextDouble();
                if (roll < 0.5)
                {
                    p.ApplyData();
                    continue;
                }

                p.heldItem2 = (short)pool[rnd.Next(pool.Count)];
                if (roll >= 0.65)
                    p.heldItem3 = (short)pool[rnd.Next(pool.Count)];
                if (roll >= 0.9)
                    p.heldItem1 = (short)pool[rnd.Next(pool.Count)];
                p.ApplyData();
            }
        }

        static List<int> BuildWildHeldItemPool(FvxWildPokemonOptions opt, IReadOnlyList<string> names, int n)
        {
            var pool = new List<int>();
            if (names == null || n <= 1) return pool;
            for (int i = 1; i < n; i++)
            {
                string nm = names[i] ?? "";
                if (opt.BanBadItems && IsBadWildHeldItemName(nm))
                    continue;
                pool.Add(i);
            }
            return pool;
        }

        static bool IsBadWildHeldItemName(string name)
        {
            string s = name.ToLowerInvariant();
            return s.Contains("mail")
                   || s.Contains("balm")
                   || s.Contains("mulch")
                   || s.Contains("flute")
                   || s.Contains("repel")
                   || (MainEditor.RomType == RomType.BW1 && s.Contains("shard"))
                   || s.Contains("master ball")
                   || s.Contains("sacred ash")
                   || s.Contains("fossil");
        }

        static void RandomizeEncounters(FvxWildPokemonOptions opt, List<PokemonEntry> pokemon, List<EncounterEntry> allEntries, Random rnd)
        {
            var entries = allEntries
                .Where(e => opt.UseTimeBasedEncounters || e.season <= 0)
                .ToList();
            if (entries.Count == 0) return;

            var basePool = BuildSpeciesPool(pokemon, opt);
            if (basePool.Count == 0) return;

            var encounterTypes = new List<(EncounterEntry entry, bool land, int slotType)>();
            foreach (var entry in entries)
            {
                for (int i = 0; i < entry.landSlots.Count; i++) encounterTypes.Add((entry, true, i));
                for (int i = 0; i < entry.waterSlots.Count; i++) encounterTypes.Add((entry, false, i));
            }

            var zoneThemeByKey = new Dictionary<string, byte>();
            var mapByKey = new Dictionary<string, Dictionary<int, int>>();
            var catchEmAllQueue = BuildCatchEmAllQueue(opt, basePool, rnd);
            foreach (var t in encounterTypes)
            {
                var slots = t.land ? t.entry.landSlots[t.slotType] : t.entry.waterSlots[t.slotType];
                for (int si = 0; si < slots.Length; si++)
                {
                    var slot = slots[si];
                    int current = slot.pokemonID;
                    if (current <= 0 || current >= pokemon.Count) continue;

                    if (opt.LevelModifierEnabled)
                        ApplyLevelModifier(slot, opt.LevelModifierPercent);

                    if (!opt.RandomizeWildPokemon) continue;

                    var key = BuildMappingKey(opt, t.entry, t.slotType);
                    if (!mapByKey.TryGetValue(key, out var map))
                    {
                        map = new Dictionary<int, int>();
                        mapByKey[key] = map;
                    }

                    int replacement;
                    if (opt.ReplacementMode != FvxWildReplacementMode.MaximumPossible && map.TryGetValue(current, out replacement))
                    {
                        // Keep existing mapping in this scope.
                    }
                    else
                    {
                        var pool = FilterSpeciesPool(basePool, pokemon, opt, t.entry, slot, current, zoneThemeByKey, rnd);
                        if (pool.Count == 0) pool = basePool;
                        replacement = PickReplacement(pool, pokemon, opt, current, slot.minLevel, catchEmAllQueue, rnd);
                        if (opt.ReplacementMode != FvxWildReplacementMode.MaximumPossible)
                            map[current] = replacement;
                    }

                    slot.pokemonID = (short)replacement;
                    slot.pokemonForm = PickFormForSpecies(pokemon[replacement], opt.AllowAlternateFormes, rnd);
                }
                t.entry.ApplyData();
            }
        }

        static Queue<int> BuildCatchEmAllQueue(FvxWildPokemonOptions opt, List<int> basePool, Random rnd)
        {
            if (!opt.CatchEmAllMode) return null;
            var shuffled = new List<int>(basePool);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                int tmp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = tmp;
            }
            return new Queue<int>(shuffled);
        }

        static string BuildMappingKey(FvxWildPokemonOptions opt, EncounterEntry entry, int slotType)
        {
            switch (opt.ReplacementMode)
            {
                case FvxWildReplacementMode.NamedLocation:
                    return "loc:" + entry.nameID;
                case FvxWildReplacementMode.PerMap:
                    return "map:" + entry.nameID + ":" + (opt.UseTimeBasedEncounters ? entry.season : 0);
                case FvxWildReplacementMode.PerEncounterSet:
                    return "set:" + entry.nameID + ":" + (opt.UseTimeBasedEncounters ? entry.season : 0) + ":" + (opt.SplitByEncounterType ? slotType : -1);
                case FvxWildReplacementMode.WholeGame:
                    return "game:" + (opt.SplitByEncounterType ? slotType : -1);
                default:
                    return "max";
            }
        }

        static List<int> BuildSpeciesPool(List<PokemonEntry> pokemon, FvxWildPokemonOptions opt)
        {
            var pool = new List<int>();
            for (int i = 1; i < pokemon.Count; i++)
            {
                if (opt.DontUseLegendaries && FvxGen5StartersStaticsTradesRunner.IsLegendaryNationalDex(i))
                    continue;
                pool.Add(i);
            }
            return pool;
        }

        static List<int> FilterSpeciesPool(List<int> basePool, List<PokemonEntry> pokemon, FvxWildPokemonOptions opt,
            EncounterEntry entry, EncounterSlot slot, int originalSpecies, Dictionary<string, byte> zoneThemeByKey, Random rnd)
        {
            IEnumerable<int> q = basePool;
            var src = pokemon[originalSpecies];

            if (opt.EvolutionRestrictionMode == FvxWildEvolutionRestrictionMode.BasicOnly)
                q = q.Where(i => pokemon[i].evolutionStage <= 0);
            else if (opt.EvolutionRestrictionMode == FvxWildEvolutionRestrictionMode.SameEvolutionStage)
                q = q.Where(i => pokemon[i].evolutionStage == src.evolutionStage);

            if (opt.TypeRestrictionMode == FvxWildTypeRestrictionMode.KeepPrimaryType)
            {
                q = q.Where(i => TypeMatches(pokemon[i], src.type1));
            }
            else if (opt.TypeRestrictionMode == FvxWildTypeRestrictionMode.RandomZoneThemes || opt.KeepZoneTypeThemes)
            {
                string zoneKey = entry.nameID + ":" + (opt.UseTimeBasedEncounters ? entry.season : 0);
                if (!zoneThemeByKey.TryGetValue(zoneKey, out byte theme))
                {
                    if (opt.KeepZoneTypeThemes)
                        theme = src.type1;
                    else
                        theme = (byte)rnd.Next(0, 18);
                    zoneThemeByKey[zoneKey] = theme;
                }
                q = q.Where(i => TypeMatches(pokemon[i], theme));
            }

            return q.ToList();
        }

        static bool TypeMatches(PokemonEntry p, byte t)
        {
            return p.type1 == t || (p.type2 != 255 && p.type2 == t);
        }

        static int PickReplacement(List<int> pool, List<PokemonEntry> pokemon, FvxWildPokemonOptions opt,
            int originalSpecies, byte minLevel, Queue<int> catchEmAllQueue, Random rnd)
        {
            if (catchEmAllQueue != null)
            {
                int loops = catchEmAllQueue.Count;
                while (loops-- > 0)
                {
                    int candidate = catchEmAllQueue.Dequeue();
                    catchEmAllQueue.Enqueue(candidate);
                    if (pool.Contains(candidate))
                        return candidate;
                }
            }

            if (opt.SimilarStrength)
            {
                int target = pokemon[originalSpecies].baseStatTotal;
                if (opt.BalanceLowLevelEncounters)
                    target = Math.Min(target, minLevel * 10 + 250);
                int low = Math.Max(1, target - 60);
                int high = target + 60;
                var similar = pool.Where(i => pokemon[i].baseStatTotal >= low && pokemon[i].baseStatTotal <= high).ToList();
                if (similar.Count > 0)
                    return similar[rnd.Next(similar.Count)];
            }
            return pool[rnd.Next(pool.Count)];
        }

        static int PickFormForSpecies(PokemonEntry p, bool allowAltFormes, Random rnd)
        {
            if (!allowAltFormes) return 0;
            if (p.numberOfForms <= 1) return 0;
            if (rnd.Next(3) != 0) return 0;
            return rnd.Next(Math.Min(255, (int)p.numberOfForms));
        }

        static void ApplyLevelModifier(EncounterSlot slot, int percent)
        {
            double f = 1.0 + percent / 100.0;
            int min = (int)Math.Round(slot.minLevel * f);
            int max = (int)Math.Round(slot.maxLevel * f);
            if (min < 1) min = 1;
            if (max < 1) max = 1;
            if (min > 100) min = 100;
            if (max > 100) max = 100;
            if (max < min) max = min;
            slot.minLevel = (byte)min;
            slot.maxLevel = (byte)max;
        }
    }
}
