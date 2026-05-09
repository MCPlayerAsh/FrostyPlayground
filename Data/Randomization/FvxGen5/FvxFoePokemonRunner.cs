using System;
using System.Collections.Generic;
using System.Linq;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    internal static class FvxFoePokemonRunner
    {
        const int WonderGuardAbilityIndex = 25;
        /// <summary>UPR-ZX Gen5 foe picker gates Wonder Guard roughly at level 20.</summary>
        const int EarlyWonderGuardLevelCap = 20;

        /// <summary>BW1 script starters → primary types for GYM1/GYM9/GYM10 theme overrides (FVX starter triangle).</summary>
        static byte[] BuildBw1StarterTrianglePrimaryTypes(bool bw2, List<PokemonEntry> pokemon, FvxFoePokemonOptions opt)
        {
            if (bw2 || !opt.Bw1TrioGymsMatchStarterTriangle || pokemon == null)
                return null;
            if (!FvxRivalStarterResolver.TryReadStarterTrio(false, MainEditor.scriptNarc, out int[] trio)
                || trio == null || trio.Length != 3)
                return null;
            var types = new byte[3];
            for (int i = 0; i < 3; i++)
            {
                int sid = trio[i];
                if (sid < 0 || sid >= pokemon.Count)
                    return null;
                types[i] = pokemon[sid].type1;
            }

            return types;
        }

        public static bool TryRun(FvxFoePokemonOptions opt, Random rnd, int maxTypeInclusive, out string error)
        {
            error = null;
            var pokemon = MainEditor.pokemonDataNarc?.pokemon;
            var evolutions = MainEditor.evolutionsNarc?.evolutions;
            var learnsets = MainEditor.learnsetNarc?.learnsets;
            if (pokemon == null || pokemon.Count < 2)
            {
                error = "Pokémon personal data is not loaded.";
                return false;
            }

            FvxGen5EvolutionGraph graph = null;
            bool[] incoming = null;
            if (evolutions != null)
            {
                graph = FvxGen5EvolutionGraph.FromEvolutions(evolutions);
                incoming = graph.ComputeIncoming();
            }

            var localSpecies = opt.UseLocalPokemon ? BuildLocalSpeciesSet() : null;
            if (opt.UseLocalPokemon && (localSpecies == null || localSpecies.Count == 0))
            {
                error = "\"Use Local Pokémon\" requires habitat list data (load a ROM with habitat NARC available).";
                return false;
            }

            var leagueUsed = new HashSet<int>();
            var trainers = MainEditor.trainerNarc.trainers;
            bool bw2 = MainEditor.RomType == RomType.BW2;
            var tags = Gen5UpTrainerTags.BuildTagsForRom(trainers.Count);
            var placementCounts = new Dictionary<int, int>();
            var groupPrimaryType = new Dictionary<string, byte>(StringComparer.Ordinal);
            byte[] bw1StarterTrianglePrimaryTypes = BuildBw1StarterTrianglePrimaryTypes(bw2, pokemon, opt);
            int[] starterTrio = null;
            FvxRivalStarterResolver.TryReadStarterTrio(bw2, MainEditor.scriptNarc, out starterTrio);
            if (opt.RivalCarriesStarter && starterTrio != null && graph != null && evolutions != null && incoming != null)
                FvxMakeRivalCarryStarter.Apply(tags, trainers, starterTrio, graph, evolutions, incoming, rnd,
                    opt.TrainersEvolvePercent);

            if (opt.RandomizeTrainerNames)
                RandomizeTrainerNames(trainers, rnd);
            if (opt.RandomizeTrainerClassNames)
                RandomizeTrainerClasses(trainers, rnd);

            var processOrder = BuildTrainerProcessingOrder(trainers.Count, tags, opt, bw2, rnd);
            byte globalBattleType = 0;
            if (opt.BattleStyleMode == FvxFoeBattleStyleMode.RandomGlobal)
                globalBattleType = (byte)rnd.Next(0, 4);

            foreach (int ti in processOrder)
            {
                var tr = trainers[ti];
                if (tr.numPokemon == 0 || tr.isHealer) continue;
                if (tr.pokemon?.pokemon == null) continue;

                string tag = tags != null && ti < tags.Length ? tags[ti] : "";
                var tier = ClassifyTier(tr, tag, opt);
                bool inMainPlaythrough = Gen5TrainerLists.IsMainPlaythroughTrainer(bw2, ti + 1);
                bool affectsPokemon = opt.TrainerPokemonMode != FvxFoeTrainerPokemonMode.Unchanged;

                if (!TryApplyTierUniqueBattleStyle(tr, tier, opt)
                    && opt.BattleStyleMode != FvxFoeBattleStyleMode.Unchanged)
                    ApplyBattleStyle(tr, opt, rnd, globalBattleType);

                if (opt.LevelPercentModifierEnabled)
                    ApplyLevelModifier(tr, opt.LevelPercentModifier, tag);

                int extraSlots = GetAdditionalSlots(opt, tier, tag);
                if (extraSlots > 0)
                    ExpandPartyAdditionalFVX(tr, extraSlots, rnd);

                if (affectsPokemon)
                {
                    bool usePlacementHistory =
                        opt.TrainerPokemonMode == FvxFoeTrainerPokemonMode.Distributed
                        || (opt.TrainerPokemonMode == FvxFoeTrainerPokemonMode.MainPlaythrough && inMainPlaythrough);
                    RandomizeTrainerParty(tr, ti, tag, opt, tier, pokemon, evolutions, graph, incoming, learnsets,
                        maxTypeInclusive, localSpecies, leagueUsed, placementCounts, groupPrimaryType,
                        usePlacementHistory, bw2, bw1StarterTrianglePrimaryTypes, rnd);
                    foreach (var tp in tr.pokemon.pokemon)
                        SanitizeTrainerPokemonSpeciesAndForm(tp, pokemon);
                }

                if (opt.TrainersEvolvePokemon && graph != null && evolutions != null)
                    ApplyTrainerEvolveChance(tr, opt.TrainersEvolvePercent, graph, evolutions, incoming, rnd);

                if (tr.pokemon?.pokemon != null)
                {
                    foreach (var tp in tr.pokemon.pokemon)
                        SanitizeTrainerPokemonSpeciesAndForm(tp, pokemon);
                }

                ApplyHeldItemsForTrainer(tr, opt, tier, tag, rnd);

                tr.ApplyData();
                if (tr.pokemon != null)
                    tr.pokemon.ApplyData();
            }

            return true;
        }

        static HashSet<int> BuildLocalSpeciesSet()
        {
            var h = new HashSet<int>();
            var lists = MainEditor.habitatListNarc?.lists;
            if (lists == null) return h;
            foreach (var list in lists)
            {
                if (list?.pokemon == null) continue;
                foreach (var p in list.pokemon)
                {
                    if (p.species > 0) h.Add(p.species);
                }
            }
            return h;
        }

        static FvxFoeTrainerTier ClassifyTier(TrainerEntry tr, string tag, FvxFoePokemonOptions opt)
        {
            bool vanilla = opt.TierDetectionMode == FvxFoeTierDetectionMode.MatchingVanillaUpr;
            if (vanilla)
                return FvxTrainerTagClassification.ClassifyTier(tag, true);

            string cn = TrainerClassName(tr.trainerClass);
            if (string.IsNullOrEmpty(cn)) return FvxFoeTrainerTier.Regular;
            if (IsBossClassName(cn)) return FvxFoeTrainerTier.Boss;
            if (IsImportantClassName(cn)) return FvxFoeTrainerTier.Important;
            return FvxFoeTrainerTier.Regular;
        }

        static string TrainerClassName(byte classId)
        {
            try
            {
                var text = MainEditor.textNarc?.textFiles?[VersionConstants.TrainerClassTextFileID]?.text;
                if (text == null || classId >= text.Count) return "";
                return text[classId] ?? "";
            }
            catch { return ""; }
        }

        static bool IsBossClassName(string cn)
        {
            if (cn.IndexOf("Gym Leader", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (cn.IndexOf("Elite Four", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (cn.IndexOf("Champion", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (cn.IndexOf("Subway Boss", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (cn.IndexOf("Shadow Triad", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        static bool IsImportantClassName(string cn)
        {
            if (cn.IndexOf("Rival", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (cn.IndexOf("Pokémon Trainer", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (cn.IndexOf("Pokemon Trainer", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (cn.IndexOf("Team Plasma", StringComparison.OrdinalIgnoreCase) >= 0
                && cn.IndexOf("Grunt", StringComparison.OrdinalIgnoreCase) < 0) return true;
            if (cn.IndexOf("Team Plasma Grunt", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            return false;
        }

        static bool IsLeagueTrainer(TrainerEntry tr, string tag, FvxFoePokemonOptions opt)
        {
            if (opt.TierDetectionMode == FvxFoeTierDetectionMode.MatchingVanillaUpr)
                return FvxTrainerTagClassification.IsLeagueTag(tag);
            string cn = TrainerClassName(tr.trainerClass);
            return cn.IndexOf("Elite Four", StringComparison.OrdinalIgnoreCase) >= 0
                   || cn.IndexOf("Champion", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool TierApplies(bool boss, bool important, bool regular, FvxFoeTrainerTier tier)
        {
            switch (tier)
            {
                case FvxFoeTrainerTier.Boss: return boss;
                case FvxFoeTrainerTier.Important: return important;
                default: return regular;
            }
        }

        static int GetAdditionalSlots(FvxFoePokemonOptions opt, FvxFoeTrainerTier tier, string tag)
        {
            if (opt.TierDetectionMode == FvxFoeTierDetectionMode.MatchingVanillaUpr
                && FvxTrainerTagClassification.SkipImportant(tag))
                return 0;

            switch (tier)
            {
                case FvxFoeTrainerTier.Boss:
                    return opt.AdditionalPokemonBoss ? 1 : 0;
                case FvxFoeTrainerTier.Important:
                    return opt.AdditionalPokemonImportant ? Math.Max(0, opt.AdditionalPokemonImportantCount) : 0;
                default:
                    return opt.AdditionalPokemonRegular ? Math.Max(0, opt.AdditionalPokemonRegularCount) : 0;
            }
        }

        static void RandomizeTrainerNames(List<TrainerEntry> trainers, Random rnd)
        {
            var names = MainEditor.textNarc?.textFiles?[VersionConstants.TrainerNameTextFileID]?.text;
            if (names == null || names.Count < 2) return;
            FvxCustomNamesSet custom = null;
            try { custom = FvxCustomNamesSet.ReadOrCreate(FvxCustomNamesSet.DefaultFilePath()); } catch { custom = null; }

            foreach (var tr in trainers)
            {
                if (tr.nameID < 0 || tr.nameID >= names.Count) continue;
                if (custom != null && (custom.TrainerNames.Count > 0 || custom.DoublesTrainerNames.Count > 0))
                {
                    bool dbl = tr.battleType == 1
                        && custom.DoublesTrainerNames.Count > 0;
                    var pickFrom = dbl ? custom.DoublesTrainerNames : custom.TrainerNames;
                    if (pickFrom.Count == 0) pickFrom = custom.TrainerNames.Count > 0 ? custom.TrainerNames : custom.DoublesTrainerNames;
                    if (pickFrom.Count > 0)
                        names[tr.nameID] = pickFrom[rnd.Next(pickFrom.Count)];
                }
                else
                    names[tr.nameID] = names[rnd.Next(names.Count)];
            }
            try { MainEditor.textNarc.textFiles[VersionConstants.TrainerNameTextFileID].CompressData(); } catch { }
        }

        static void RandomizeTrainerClasses(List<TrainerEntry> trainers, Random rnd)
        {
            var classes = MainEditor.textNarc?.textFiles?[VersionConstants.TrainerClassTextFileID]?.text;
            if (classes == null || classes.Count < 1) return;
            FvxCustomNamesSet custom = null;
            try { custom = FvxCustomNamesSet.ReadOrCreate(FvxCustomNamesSet.DefaultFilePath()); } catch { custom = null; }

            if (custom != null && (custom.TrainerClasses.Count > 0 || custom.DoublesTrainerClasses.Count > 0))
            {
                for (int i = 0; i < classes.Count; i++)
                {
                    var pickFrom = custom.TrainerClasses.Count > 0 ? custom.TrainerClasses : custom.DoublesTrainerClasses;
                    if (pickFrom.Count > 0)
                        classes[i] = pickFrom[rnd.Next(pickFrom.Count)];
                }
                try { MainEditor.textNarc.textFiles[VersionConstants.TrainerClassTextFileID].CompressData(); } catch { }
            }

            byte max = (byte)(classes.Count - 1);
            foreach (var tr in trainers)
                tr.trainerClass = (byte)rnd.Next(0, max + 1);
        }

        /// <returns>True if a tier-specific fixed battle type was applied (main battle style options are skipped).</returns>
        static bool TryApplyTierUniqueBattleStyle(TrainerEntry tr, FvxFoeTrainerTier tier, FvxFoePokemonOptions opt)
        {
            bool use = false;
            int idx = 0;
            switch (tier)
            {
                case FvxFoeTrainerTier.Boss:
                    use = opt.UniqueBattleStyleBoss;
                    idx = opt.UniqueBattleStyleBossBattleType;
                    break;
                case FvxFoeTrainerTier.Important:
                    use = opt.UniqueBattleStyleImportant;
                    idx = opt.UniqueBattleStyleImportantBattleType;
                    break;
                case FvxFoeTrainerTier.Regular:
                    use = opt.UniqueBattleStyleRegular;
                    idx = opt.UniqueBattleStyleRegularBattleType;
                    break;
            }
            if (!use) return false;
            if (idx < 0) idx = 0;
            if (idx > 3) idx = 3;
            tr.battleType = (byte)idx;
            EnsurePartySizeForBattleType(tr);
            return true;
        }

        static void ApplyBattleStyle(TrainerEntry tr, FvxFoePokemonOptions opt, Random rnd, byte globalBattleType)
        {
            switch (opt.BattleStyleMode)
            {
                case FvxFoeBattleStyleMode.Random:
                    tr.battleType = (byte)rnd.Next(0, 4);
                    break;
                case FvxFoeBattleStyleMode.RandomGlobal:
                    tr.battleType = globalBattleType;
                    break;
                case FvxFoeBattleStyleMode.SingleStyle:
                    int v = opt.SingleStyleBattleType;
                    if (v < 0) v = 0;
                    if (v > 3) v = 3;
                    tr.battleType = (byte)v;
                    break;
            }
            EnsurePartySizeForBattleType(tr);
        }

        /// <summary>
        /// Matches UPR-FVX <c>BattleStyle.getRequiredPokemonCount</c>: double = 2, triple and rotation = 3.
        /// </summary>
        static int RequiredPartyCountForBattleType(byte battleType)
        {
            if (battleType == 1) return 2;
            if (battleType == 2 || battleType == 3) return 3;
            return 1;
        }

        static void EnsurePartySizeForBattleType(TrainerEntry tr)
        {
            int need = RequiredPartyCountForBattleType(tr.battleType);
            if (tr.numPokemon < need)
                ExpandParty(tr, need - tr.numPokemon);
        }

        static void ExpandParty(TrainerEntry tr, int add)
        {
            if (add <= 0 || tr.pokemon?.pokemon == null) return;
            int newCount = Math.Min(6, tr.numPokemon + add);
            int delta = newCount - tr.numPokemon;
            if (delta <= 0) return;
            var last = tr.pokemon.pokemon.Count > 0
                ? tr.pokemon.pokemon[tr.pokemon.pokemon.Count - 1].Clone()
                : new TrainerPokemon { pokemonID = 1, level = 5, moves = new short[4] };
            for (int i = 0; i < delta; i++)
                tr.pokemon.pokemon.Add(last.Clone());
            tr.numPokemon = (byte)newCount;
        }

        /// <summary>UPR-FVX <c>addTrainerPokemon</c>: insert before last slot, template from random original member.</summary>
        static void ExpandPartyAdditionalFVX(TrainerEntry tr, int additional, Random rnd)
        {
            if (additional <= 0 || tr.pokemon?.pokemon == null) return;
            var list = tr.pokemon.pokemon;
            int originalSize = list.Count;
            if (originalSize == 0) return;

            int lowest = 100;
            int highest = 0;
            bool duplicateHighest = false;
            foreach (var tpk in list)
            {
                int curLevel = tpk.level;
                if (curLevel == highest)
                    duplicateHighest = true;
                if (curLevel < lowest)
                    lowest = curLevel;
                if (curLevel > highest)
                {
                    highest = curLevel;
                    duplicateHighest = false;
                }
            }

            int upperLevelBound = duplicateHighest ? highest : highest - 1;

            var originalTemplates = new List<TrainerPokemon>(originalSize);
            for (int i = 0; i < originalSize; i++)
                originalTemplates.Add(list[i]);

            for (int k = 0; k < additional; k++)
            {
                const int maxPokemon = 6;
                if (list.Count >= maxPokemon)
                    break;

                int secondToLastIndex = list.Count - 1;
                var template = originalTemplates[rnd.Next(originalSize)].Clone();
                template.heldItem = 0;
                int hi = System.Math.Max(upperLevelBound, lowest);
                byte newLv = (byte)(lowest + rnd.Next(hi - lowest + 1));
                template.level = newLv;
                list.Insert(secondToLastIndex, template);
                tr.numPokemon = (byte)list.Count;
            }
        }

        static List<int> BuildTrainerProcessingOrder(int trainerCount, string[] tags, FvxFoePokemonOptions opt,
            bool bw2, Random rnd)
        {
            var order = new List<int>(trainerCount);
            for (int i = 0; i < trainerCount; i++)
                order.Add(i);

            if (opt.TrainerPokemonMode != FvxFoeTrainerPokemonMode.Unchanged)
            {
                for (int i = order.Count - 1; i > 0; i--)
                {
                    int j = rnd.Next(i + 1);
                    int tmp = order[i];
                    order[i] = order[j];
                    order[j] = tmp;
                }
            }

            if (opt.LeagueUniquePokemon)
            {
                var league = Gen5EliteFourFallback.ResolveLeagueTrainerIndicesZeroBased(tags, trainerCount, bw2);
                var first = new List<int>();
                var second = new List<int>();
                foreach (int idx in order)
                {
                    if (league.Contains(idx))
                        first.Add(idx);
                    else
                        second.Add(idx);
                }

                order.Clear();
                order.AddRange(first);
                order.AddRange(second);
            }

            return order;
        }

        static void ApplyLevelModifier(TrainerEntry tr, int pct, string tag)
        {
            if (pct > 0 && FvxTrainerTagClassification.IsFirstRivalOrFriendTag(tag))
                return;
            if (tr.pokemon?.pokemon == null) return;
            foreach (var p in tr.pokemon.pokemon)
            {
                double f = 1.0 + pct / 100.0;
                int nl = (int)Math.Round(p.level * f);
                if (nl < 1) nl = 1;
                if (nl > 100) nl = 100;
                p.level = (byte)nl;
            }
        }

        static void ApplyTrainerEvolveChance(TrainerEntry tr, int pct, FvxGen5EvolutionGraph graph,
            IReadOnlyList<EvolutionDataEntry> evolutions, bool[] incoming, Random rnd)
        {
            if (tr.pokemon?.pokemon == null) return;
            double chance = Math.Max(0, Math.Min(1.0, pct / 100.0));
            if (pct <= 0) chance = 0;
            foreach (var p in tr.pokemon.pokemon)
            {
                if (rnd.NextDouble() > chance) continue;
                int species = p.pokemonID;
                if (species < 0 || species >= evolutions.Count) continue;
                int evolved = HighestEvolutionAtLevel(graph, evolutions, incoming, species, p.level, rnd);
                if (evolved > 0 && evolved != species)
                {
                    p.pokemonID = (short)evolved;
                    p.form = 0;
                }
            }
        }

        /// <summary>Walk evolution graph preferring level-up edges whose condition &lt;= level.</summary>
        static int HighestEvolutionAtLevel(FvxGen5EvolutionGraph graph, IReadOnlyList<EvolutionDataEntry> evolutions,
            bool[] incoming, int startSpecies, byte level, Random rnd)
        {
            int current = startSpecies;
            var visited = new HashSet<int> { current };
            bool progressed = true;
            while (progressed)
            {
                progressed = false;
                if (current < 0 || current >= evolutions.Count) break;
                var outs = graph.Outgoing(current);
                var candidates = new List<int>();
                foreach (int to in outs)
                {
                    if (to < 0 || to >= evolutions.Count || visited.Contains(to)) continue;
                    if (!CanEvolveViaLevel(evolutions[current], to, level)) continue;
                    candidates.Add(to);
                }
                if (candidates.Count == 0) break;
                int pick = candidates[rnd.Next(candidates.Count)];
                visited.Add(pick);
                current = pick;
                progressed = true;
            }
            return current;
        }

        static bool CanEvolveViaLevel(EvolutionDataEntry fromEntry, int toSpecies, byte level)
        {
            if (fromEntry?.methods == null) return false;
            foreach (var m in fromEntry.methods)
            {
                if (m.newPokemonID != toSpecies) continue;
                if (FvxGen5EvolutionMethods.IsImpossibleWithoutTrade(m.method)) return false;
                if (!FvxGen5EvolutionMethods.ConditionIsLevel(m.method)) continue;
                return level >= m.condition;
            }
            return false;
        }

        static void RandomizeTrainerParty(TrainerEntry tr, int trainerIndex, string tag, FvxFoePokemonOptions opt,
            FvxFoeTrainerTier tier,
            List<PokemonEntry> pokemon, IReadOnlyList<EvolutionDataEntry> evolutions,
            FvxGen5EvolutionGraph graph, bool[] incoming, List<LevelUpMoveset> learnsets,
            int maxTypeInclusive, HashSet<int> localSpecies, HashSet<int> leagueUsed,
            Dictionary<int, int> placementCounts, Dictionary<string, byte> groupPrimaryType,
            bool usePlacementHistory,
            bool bw2,
            byte[] bw1StarterTrianglePrimaryTypes,
            Random rnd)
        {
            if (tr.pokemon?.pokemon == null || tr.numPokemon == 0) return;

            var pool = BuildSpeciesPool(pokemon, opt, maxTypeInclusive, localSpecies, bw2);
            if (pool.Count == 0) return;

            int[] typeWeights = opt.WeightTypesByCount ? ComputeTypeWeights(pool, pokemon) : null;

            bool diverse = TierApplies(opt.DiverseTypesBoss, opt.DiverseTypesImportant, opt.DiverseTypesRegular, tier);
            bool isLeague = opt.LeagueUniquePokemon && IsLeagueTrainer(tr, tag, opt);
            int leagueSlots = isLeague ? Math.Max(0, Math.Min(6, opt.LeagueUniqueCount)) : 0;

            var usedTypes = new HashSet<byte>();
            var usedSpecies = new HashSet<int>();

            bool useGroupType = opt.TrainerPokemonMode == FvxFoeTrainerPokemonMode.TypeThemedElite4Gyms
                                && FvxTrainerTagClassification.IsTypeThemedGroupKey(
                                    FvxTrainerTagClassification.GroupKeyFromTag(tag));

            string groupKey = FvxTrainerTagClassification.GroupKeyFromTag(tag);
            byte? romGroupTheme = null;
            if (!string.IsNullOrEmpty(groupKey)
                && Gen5GymEliteTypeThemes.TryGetTheme(groupKey, bw2, bw1StarterTrianglePrimaryTypes,
                    opt.Bw1TrioGymsMatchStarterTriangle, out byte rt))
                romGroupTheme = rt;
            byte? sharedTeamType = TrySharedPartyType(tr.pokemon.pokemon, pokemon);

            for (int i = 0; i < tr.pokemon.pokemon.Count; i++)
            {
                var tp = tr.pokemon.pokemon[i];
                int origSpecies = tp.pokemonID;
                if (origSpecies < 0 || origSpecies >= pokemon.Count) origSpecies = 1;
                int origBst = pokemon[origSpecies].baseStatTotal;
                byte origType = pokemon[origSpecies].type1;

                if (opt.RivalCarriesStarter && IsRivalOrFriendTag(tag)
                    && i == FvxMakeRivalCarryStarter.BestPokemonIndex(tr))
                {
                    int kept = tp.pokemonID;
                    if (usePlacementHistory)
                        IncrementPlacement(placementCounts, kept);
                    if (diverse && kept >= 0 && kept < pokemon.Count)
                        usedTypes.Add(pokemon[kept].type1);
                    if (opt.AvoidDuplicates)
                        usedSpecies.Add(kept);
                    if (isLeague && i < leagueSlots)
                        leagueUsed.Add(kept);
                    continue;
                }

                List<int> filtered;
                if (opt.TrainerPokemonMode == FvxFoeTrainerPokemonMode.KeepThemed
                    || opt.TrainerPokemonMode == FvxFoeTrainerPokemonMode.KeepThemeOrPrimary)
                {
                    byte? slotTheme = romGroupTheme ?? sharedTeamType;
                    if (slotTheme == null
                        && opt.TrainerPokemonMode == FvxFoeTrainerPokemonMode.KeepThemeOrPrimary)
                        slotTheme = origType;
                    if (slotTheme.HasValue)
                    {
                        byte st = slotTheme.Value;
                        filtered = pool.Where(x => TypeMatchesPrimaryOrSecondary(pokemon[x], st)).ToList();
                    }
                    else
                        filtered = new List<int>(pool);
                }
                else
                {
                    filtered = FilterPool(pool, pokemon, opt, tag, groupPrimaryType, origSpecies, origType,
                        useGroupType);
                }

                if (filtered.Count == 0) filtered = new List<int>(pool);

                if (opt.Global != null && opt.Global.BanPrematureEvos && evolutions != null)
                {
                    var pl = filtered.Where(s =>
                        FvxPrematureEvoLegality.IsLegalEvolutionAtLevel(s, tp.level, 1.0, evolutions)).ToList();
                    if (pl.Count > 0) filtered = pl;
                }

                List<int> pickPool = filtered;
                if (isLeague && i < leagueSlots && opt.UseLocalPokemon && localSpecies != null)
                {
                    var nonLocal = filtered.Where(s => !localSpecies.Contains(s)).ToList();
                    if (nonLocal.Count > 0)
                        pickPool = nonLocal;
                }

                int picked = usePlacementHistory
                    ? PickSpeciesDistributedFVX(pickPool, pokemon, typeWeights, placementCounts, rnd)
                    : PickSpecies(pickPool, pokemon, typeWeights, rnd);

                if (opt.SimilarStrength)
                {
                    int low = Math.Max(1, (int)(origBst * 0.85));
                    int high = Math.Min(800, (int)(origBst * 1.15));
                    var sim = pickPool.Where(s => pokemon[s].baseStatTotal >= low && pokemon[s].baseStatTotal <= high).ToList();
                    if (sim.Count > 0)
                        picked = usePlacementHistory
                            ? PickSpeciesDistributedFVX(sim, pokemon, typeWeights, placementCounts, rnd)
                            : PickSpecies(sim, pokemon, typeWeights, rnd);
                }

                if (diverse)
                {
                    for (int attempt = 0; attempt < 24; attempt++)
                    {
                        byte pt = pokemon[picked].type1;
                        if (!usedTypes.Contains(pt)) break;
                        picked = usePlacementHistory
                            ? PickSpeciesDistributedFVX(pickPool, pokemon, typeWeights, placementCounts, rnd)
                            : PickSpecies(pickPool, pokemon, typeWeights, rnd);
                    }
                    usedTypes.Add(pokemon[picked].type1);
                }

                if (opt.AvoidDuplicates)
                {
                    for (int attempt = 0; attempt < 40 && usedSpecies.Contains(picked); attempt++)
                        picked = usePlacementHistory
                            ? PickSpeciesDistributedFVX(pickPool, pokemon, typeWeights, placementCounts, rnd)
                            : PickSpecies(pickPool, pokemon, typeWeights, rnd);
                    usedSpecies.Add(picked);
                }

                if (isLeague && i < leagueSlots)
                {
                    for (int attempt = 0; attempt < 60 && leagueUsed.Contains(picked); attempt++)
                    {
                        picked = usePlacementHistory
                            ? PickSpeciesDistributedFVX(pickPool, pokemon, typeWeights, placementCounts, rnd)
                            : PickSpecies(pickPool, pokemon, typeWeights, rnd);
                    }
                    leagueUsed.Add(picked);
                }

                if (opt.NoEarlyWonderGuard && tp.level < EarlyWonderGuardLevelCap
                    && HasWonderGuard(pokemon, picked))
                {
                    var noWg = pickPool.Where(s => !HasWonderGuard(pokemon, s)).ToList();
                    if (noWg.Count > 0)
                        picked = usePlacementHistory
                            ? PickSpeciesDistributedFVX(noWg, pokemon, typeWeights, placementCounts, rnd)
                            : PickSpecies(noWg, pokemon, typeWeights, rnd);
                }

                if (usePlacementHistory)
                    IncrementPlacement(placementCounts, picked);

                tp.pokemonID = (short)picked;
                if (!opt.AllowAlternateFormes)
                    tp.form = 0;
                else
                {
                    byte nf = pokemon[picked].numberOfForms;
                    if (nf <= 1)
                        tp.form = 0;
                    else if (opt.Global != null && opt.Global.BanIrregularAltFormes
                             && FvxGen5IrregularFormes.IsBannedWhenOptionOn(picked, bw2))
                        tp.form = 0;
                    else if (rnd.Next(3) == 0)
                    {
                        int formCount = Math.Min(nf, byte.MaxValue);
                        tp.form = (short)rnd.Next(formCount);
                    }
                    else
                        tp.form = 0;
                }
            }
        }

        /// <summary>
        /// Clamp species to loaded personal entries and form to [0, numberOfForms-1]. Fixes invalid IDs after
        /// evolution + alt-form rolls and corrupted personal <see cref="PokemonEntry.numberOfForms"/>.
        /// </summary>
        static void SanitizeTrainerPokemonSpeciesAndForm(TrainerPokemon tp, List<PokemonEntry> pokemon)
        {
            if (tp == null || pokemon == null || pokemon.Count <= 1)
                return;

            int sid = tp.pokemonID;
            if (sid < 1)
                sid = 1;
            else if (sid >= pokemon.Count)
                sid = pokemon.Count - 1;

            tp.pokemonID = (short)sid;

            byte nf = pokemon[sid].numberOfForms;
            if (nf == 0)
                nf = 1;
            int maxForm = nf - 1;
            if (tp.form < 0 || tp.form > maxForm)
                tp.form = 0;
        }

        static bool IsRivalTrainer(TrainerEntry tr, string tag, FvxFoePokemonOptions opt)
        {
            if (opt.TierDetectionMode == FvxFoeTierDetectionMode.MatchingVanillaUpr)
            {
                if (string.IsNullOrEmpty(tag)) return false;
                return tag.StartsWith("RIVAL", StringComparison.Ordinal)
                       || tag.StartsWith("FRIEND", StringComparison.Ordinal);
            }

            string cn = TrainerClassName(tr.trainerClass);
            if (cn.IndexOf("Rival", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            try
            {
                var names = MainEditor.textNarc?.textFiles?[VersionConstants.TrainerNameTextFileID]?.text;
                if (names == null || tr.nameID < 0 || tr.nameID >= names.Count) return false;
                string n = names[tr.nameID] ?? "";
                if (n.IndexOf("Hugh", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (n.IndexOf("Cheren", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (n.IndexOf("Bianca", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (n.IndexOf("N", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            catch { }
            return false;
        }

        static bool HasWonderGuard(List<PokemonEntry> pokemon, int speciesIndex)
        {
            if (speciesIndex < 0 || speciesIndex >= pokemon.Count) return false;
            var p = pokemon[speciesIndex];
            return p.ability1 == WonderGuardAbilityIndex
                   || p.ability2 == WonderGuardAbilityIndex
                   || p.ability3 == WonderGuardAbilityIndex;
        }

        static List<int> BuildSpeciesPool(List<PokemonEntry> pokemon, FvxFoePokemonOptions opt,
            int maxTypeInclusive, HashSet<int> localSpecies, bool bw2)
        {
            var pool = new List<int>();
            for (int i = 1; i < pokemon.Count; i++)
            {
                var pk = pokemon[i];
                if (pk.type1 > maxTypeInclusive) continue;
                if (pk.type2 != 255 && pk.type2 != pk.type1 && pk.type2 > maxTypeInclusive) continue;
                if (opt.DontUseLegendaries && FvxGen5StartersStaticsTradesRunner.IsLegendaryNationalDex(i)) continue;
                bool extendedBw2 = bw2 && FvxGen5NonStandardSpecies.IsBw2ExtendedSpecies(i);
                if (!opt.AllowNonStandardPokemon && extendedBw2)
                    continue;
                if (localSpecies != null && !localSpecies.Contains(i))
                {
                    if (!(opt.AllowNonStandardPokemon && extendedBw2))
                        continue;
                }
                pool.Add(i);
            }
            return FvxGlobalSpeciesPoolFilter.FilterPool(pool, opt.Global, bw2,
                MainEditor.evolutionsNarc?.evolutions, null);
        }

        /// <summary>UPR-FVX <c>getSharedType</c> style: type common to all members' primary/secondary typing.</summary>
        static byte? TrySharedPartyType(IReadOnlyList<TrainerPokemon> party, List<PokemonEntry> dex)
        {
            if (party == null || party.Count == 0 || dex == null) return null;
            HashSet<byte> intersection = null;
            foreach (var tp in party)
            {
                int sid = tp.pokemonID;
                if (sid < 0 || sid >= dex.Count) continue;
                var pk = dex[sid];
                var types = new HashSet<byte> { pk.type1 };
                if (pk.type2 != 255 && pk.type2 != pk.type1)
                    types.Add(pk.type2);
                if (intersection == null)
                    intersection = new HashSet<byte>(types);
                else
                    intersection.IntersectWith(types);
                if (intersection.Count == 0)
                    return null;
            }

            if (intersection == null || intersection.Count == 0)
                return null;
            return intersection.Min();
        }

        static List<int> FilterPool(List<int> pool, List<PokemonEntry> pokemon, FvxFoePokemonOptions opt,
            string tag, Dictionary<string, byte> groupPrimaryType,
            int origSpecies, byte origType,
            bool useGroupType)
        {
            IEnumerable<int> q = pool;
            string groupKey = FvxTrainerTagClassification.GroupKeyFromTag(tag);

            switch (opt.TrainerPokemonMode)
            {
                case FvxFoeTrainerPokemonMode.TypeThemed:
                    q = q.Where(i => TypeMatchesPrimaryOrSecondary(pokemon[i], origType));
                    break;
                case FvxFoeTrainerPokemonMode.TypeThemedElite4Gyms:
                    if (useGroupType && !string.IsNullOrEmpty(groupKey))
                    {
                        if (!groupPrimaryType.TryGetValue(groupKey, out byte gt))
                        {
                            gt = pokemon[origSpecies].type1;
                            groupPrimaryType[groupKey] = gt;
                        }
                        q = q.Where(i => TypeMatchesPrimaryOrSecondary(pokemon[i], gt));
                    }
                    break;
            }

            return q.ToList();
        }

        static bool TypeMatchesPrimaryOrSecondary(PokemonEntry pk, byte t)
            => pk.type1 == t || (pk.type2 != 255 && pk.type2 != pk.type1 && pk.type2 == t);

        /// <summary>UPR-FVX <c>pickTrainerPokeReplacement</c> distributed branch: prefer species with placement count &lt; 2× average.</summary>
        static int PickSpeciesDistributedFVX(List<int> filtered, List<PokemonEntry> pokemon, int[] typeWeights,
            Dictionary<int, int> placementCounts, Random rnd)
        {
            if (filtered.Count == 0) return 1;
            double avg = 0;
            if (placementCounts != null && placementCounts.Count > 0)
            {
                double s = 0;
                foreach (var v in placementCounts.Values) s += v;
                avg = s / placementCounts.Count;
            }

            List<int> pool = filtered;
            if (avg > 0)
            {
                var biased = filtered.Where(sid =>
                {
                    int h = placementCounts.TryGetValue(sid, out var c) ? c : 0;
                    return h < avg * 2.0;
                }).ToList();
                if (biased.Count > 0)
                    pool = biased;
            }

            return PickSpecies(pool, pokemon, typeWeights, rnd);
        }

        static void IncrementPlacement(Dictionary<int, int> placementCounts, int speciesId)
        {
            if (!placementCounts.ContainsKey(speciesId)) placementCounts[speciesId] = 0;
            placementCounts[speciesId]++;
        }

        static bool IsRivalOrFriendTag(string tag)
            => !string.IsNullOrEmpty(tag)
               && (tag.StartsWith("RIVAL", StringComparison.Ordinal)
                   || tag.StartsWith("FRIEND", StringComparison.Ordinal));

        static int[] ComputeTypeWeights(List<int> pool, List<PokemonEntry> pokemon)
        {
            var w = new int[19];
            foreach (int i in pool)
            {
                byte t = pokemon[i].type1;
                if (t < w.Length) w[t]++;
            }
            return w;
        }

        static int PickSpecies(List<int> filtered, List<PokemonEntry> pokemon, int[] typeWeights, Random rnd)
        {
            if (filtered.Count == 0) return 1;
            if (typeWeights == null)
                return filtered[rnd.Next(filtered.Count)];

            double sum = 0;
            var weights = new double[filtered.Count];
            for (int i = 0; i < filtered.Count; i++)
            {
                byte t = pokemon[filtered[i]].type1;
                int tw = t < typeWeights.Length ? Math.Max(1, typeWeights[t]) : 1;
                weights[i] = tw;
                sum += tw;
            }
            double r = rnd.NextDouble() * sum;
            for (int i = 0; i < filtered.Count; i++)
            {
                r -= weights[i];
                if (r <= 0) return filtered[i];
            }
            return filtered[filtered.Count - 1];
        }

        static void ApplyHeldItemsForTrainer(TrainerEntry tr, FvxFoePokemonOptions opt, FvxFoeTrainerTier tier,
            string tag, Random rnd)
        {
            if (FvxTrainerTagClassification.ShouldNotGetBuffs(tag))
                return;

            bool wantHeld = TierApplies(opt.HeldItemsBoss, opt.HeldItemsImportant, opt.HeldItemsRegular, tier);
            if (!wantHeld) return;

            if (!tr.heldItems)
            {
                tr.heldItems = true;
                foreach (var p in tr.pokemon.pokemon)
                    p.heldItem = 0;
            }

            var itemPool = BuildHeldItemPool(opt);
            if (itemPool.Count == 0) return;

            int highIdx = 0;
            if (opt.HeldHighestLevelOnly && tr.pokemon.pokemon.Count > 0)
            {
                byte maxLv = 0;
                for (int i = 0; i < tr.pokemon.pokemon.Count; i++)
                {
                    if (tr.pokemon.pokemon[i].level >= maxLv)
                    {
                        maxLv = tr.pokemon.pokemon[i].level;
                        highIdx = i;
                    }
                }
            }

            for (int i = 0; i < tr.pokemon.pokemon.Count; i++)
            {
                if (opt.HeldHighestLevelOnly && i != highIdx)
                {
                    tr.pokemon.pokemon[i].heldItem = 0;
                    continue;
                }
                tr.pokemon.pokemon[i].heldItem = (short)itemPool[rnd.Next(itemPool.Count)];
            }
        }

        static List<int> BuildHeldItemPool(FvxFoePokemonOptions opt)
        {
            var names = MainEditor.textNarc?.textFiles?[VersionConstants.ItemNameTextFileID]?.text;
            var items = MainEditor.itemDataNarc?.items;
            int n = Math.Min(names?.Count ?? 0, items?.Count ?? names?.Count ?? 0);
            var pool = FvxGen5HeldItemPools.BuildPool(opt, names, n);
            if (pool.Count == 0 && n > 1)
            {
                for (int i = 1; i < Math.Min(300, n); i++) pool.Add(i);
            }
            return pool;
        }
    }
}
