using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    internal static class FvxGen5StartersStaticsTradesRunner
    {
        internal static bool IsLegendaryNationalDex(int nationalSpeciesIndex)
            => LegendaryOrMythicalNationalDex.Contains(nationalSpeciesIndex);

        static readonly HashSet<int> LegendaryOrMythicalNationalDex = new HashSet<int>
        {
            144, 145, 146, 150, 151, 243, 244, 245, 249, 250, 251,
            377, 378, 379, 380, 381, 382, 383, 384, 385, 386,
            480, 481, 482, 483, 484, 485, 486, 487, 488, 489, 490, 491, 492, 493,
            638, 639, 640, 641, 642, 643, 644, 645, 646, 647, 648, 649
        };

        public static bool TryRun(FvxStartersStaticsTradesOptions opt, Random rnd, bool bw2, int maxTypeInclusive, out string error)
        {
            error = null;
            var pd = MainEditor.pokemonDataNarc?.pokemon;
            var sn = MainEditor.scriptNarc;
            var evo = MainEditor.evolutionsNarc?.evolutions;
            if (pd == null || sn == null || evo == null)
            {
                error = "Pokémon data, scripts, or evolutions are not loaded.";
                return false;
            }

            var graph = FvxGen5EvolutionGraph.FromEvolutions(evo);
            var incoming = graph.ComputeIncoming();

            byte[] chart = null;
            if (StartersNeedRomTypeChart(opt))
            {
                if (!FvxGen5TypeChart.TryReadChart(bw2, maxTypeInclusive, out chart, out var chartErr))
                {
                    error = chartErr ?? "Could not read type chart.";
                    return false;
                }
            }

            if (!TryApplyStarters(opt, rnd, bw2, sn, pd, evo, graph, incoming, chart, maxTypeInclusive,
                    out var startersApplied, out var starterErr))
            {
                error = starterErr;
                return false;
            }

            if (opt.StarterUpdateGraphicsAndCries && startersApplied != null)
            {
                if (!FvxGen5StarterAssetPatcher.TryApply(true, startersApplied, bw2, out var assetErr))
                {
                    error = assetErr ?? "Starter graphics/text patch failed.";
                    return false;
                }
            }

            if (!TryApplyStatics(opt, rnd, bw2, sn, pd, maxTypeInclusive, out var staticErr))
            {
                error = staticErr;
                return false;
            }

            if (!TryApplyTrades(opt, rnd, bw2, sn, pd, out var tradeErr))
            {
                error = tradeErr;
                return false;
            }

            return true;
        }

        static bool StartersNeedRomTypeChart(FvxStartersStaticsTradesOptions opt)
        {
            if (opt.StarterSelectionMode == FvxStarterSelectionMode.Unchanged ||
                opt.StarterSelectionMode == FvxStarterSelectionMode.Custom)
                return false;
            return opt.StarterTypeRestriction == FvxStarterTypeRestriction.AnyTypeTriangle;
        }

        static bool IsMono(PokemonEntry pk) => pk.type2 == 255 || pk.type2 == pk.type1;

        static bool TypesInChart(PokemonEntry pk, int maxType)
        {
            if (pk.type1 > maxType) return false;
            byte t2 = pk.type2;
            if (t2 != 255 && t2 != pk.type1 && t2 > maxType) return false;
            return true;
        }

        static bool BstOk(PokemonEntry pk, FvxStartersStaticsTradesOptions opt)
        {
            int bst = pk.baseStatTotal;
            if (opt.LimitBstMin && bst < opt.BstMinimum) return false;
            if (opt.LimitBstMax && bst > opt.BstMaximum) return false;
            return true;
        }

        static bool LegendaryOk(int speciesIndex, FvxStartersStaticsTradesOptions opt)
        {
            if (!opt.DontUseLegendaries) return true;
            return !LegendaryOrMythicalNationalDex.Contains(speciesIndex);
        }

        static bool DualTypeOk(PokemonEntry pk, FvxStartersStaticsTradesOptions opt)
        {
            if (!opt.NoDualTypes) return true;
            return IsMono(pk);
        }

        static bool StarterPoolMember(int i, PokemonEntry pk, FvxStartersStaticsTradesOptions opt, int maxType,
            IReadOnlyList<EvolutionDataEntry> evo, bool bw2)
        {
            if (i <= 0 || i > FvxGen5Constants.NationalDexCount) return false;
            if (!TypesInChart(pk, maxType)) return false;
            if (!BstOk(pk, opt)) return false;
            if (!LegendaryOk(i, opt)) return false;
            if (!DualTypeOk(pk, opt)) return false;
            var g = opt.Global;
            if (g != null)
            {
                if (g.LimitPokemon && g.AllowedSpecies != null && g.AllowedSpecies.Count > 0
                    && !g.AllowedSpecies.Contains(i))
                    return false;
                if (g.BanIrregularAltFormes && FvxGen5IrregularFormes.IsBannedWhenOptionOn(i, bw2))
                    return false;
                if (g.BanPrematureEvos && evo != null
                    && !FvxPrematureEvoLegality.IsLegalEvolutionAtLevel(i, 5, 1.0, evo))
                    return false;
            }
            return true;
        }

        static List<int> BuildStarterIndices(IReadOnlyList<PokemonEntry> pokemon, FvxStartersStaticsTradesOptions opt,
            int maxType, IReadOnlyList<EvolutionDataEntry> evo, bool bw2)
        {
            var list = new List<int>();
            for (int i = 1; i < pokemon.Count; i++)
            {
                if (StarterPoolMember(i, pokemon[i], opt, maxType, evo, bw2))
                    list.Add(i);
            }
            return list;
        }

        static bool TryApplyStarters(
            FvxStartersStaticsTradesOptions opt,
            Random rnd,
            bool bw2,
            ScriptNARC narc,
            IReadOnlyList<PokemonEntry> pokemon,
            IReadOnlyList<EvolutionDataEntry> evo,
            FvxGen5EvolutionGraph graph,
            bool[] incoming,
            byte[] chart,
            int maxType,
            out int[] startersApplied,
            out string error)
        {
            startersApplied = null;
            error = null;
            var groups = FvxGen5RomLayout.StarterOffsets(bw2);
            if (groups == null || groups.Length != 3)
            {
                error = "Starter layout error.";
                return false;
            }

            switch (opt.StarterSelectionMode)
            {
                case FvxStarterSelectionMode.Unchanged:
                    return true;
                case FvxStarterSelectionMode.Custom:
                    if (!ValidateCustomStarters(opt, pokemon, evo, bw2, maxType, out error))
                        return false;
                    WriteStarterGroup(narc, groups[0], opt.CustomStarterSpeciesIndex0);
                    WriteStarterGroup(narc, groups[1], opt.CustomStarterSpeciesIndex1);
                    WriteStarterGroup(narc, groups[2], opt.CustomStarterSpeciesIndex2);
                    startersApplied = new[]
                    {
                        opt.CustomStarterSpeciesIndex0,
                        opt.CustomStarterSpeciesIndex1,
                        opt.CustomStarterSpeciesIndex2,
                    };
                    return true;
            }

            var pool = BuildStarterIndices(pokemon, opt, maxType, evo, bw2);
            if (pool.Count < 3)
            {
                error = "Not enough Pokémon match starter filters (BST / legendaries / types).";
                return false;
            }

            bool basicOnly = opt.StarterSelectionMode == FvxStarterSelectionMode.RandomAnyBasic;
            bool threeStage = opt.StarterSelectionMode == FvxStarterSelectionMode.RandomBasicThreeStageLine;

            List<int> filtered;
            if (basicOnly || threeStage)
            {
                filtered = pool.Where(i => graph.IsBasic(i, incoming)).ToList();
                if (threeStage)
                    filtered = filtered.Where(i => MaxDepthFromBasic(graph, pokemon.Count, i) >= 2).ToList();
                if (filtered.Count < 3)
                {
                    error = "Not enough basic Pokémon match the selected starter mode.";
                    return false;
                }
            }
            else
                filtered = pool.ToList();

            if (filtered.Count < 3)
            {
                error = "Not enough Pokémon match starter filters.";
                return false;
            }

            int[] picks = new int[3];
            switch (opt.StarterTypeRestriction)
            {
                case FvxStarterTypeRestriction.FireWaterGrass:
                    if (!TryPickFwg(rnd, filtered, pokemon, picks, out error))
                        return false;
                    break;
                case FvxStarterTypeRestriction.AnyTypeTriangle:
                    if (chart == null)
                    {
                        error = "Type chart is required for Any Type Triangle starters.";
                        return false;
                    }
                    if (!TryPickTriangle(rnd, filtered, pokemon, chart, maxType, picks, out error))
                        return false;
                    break;
                case FvxStarterTypeRestriction.UniquePrimaryType:
                    if (!TryPickUniquePrimary(rnd, filtered, pokemon, picks, out error))
                        return false;
                    break;
                case FvxStarterTypeRestriction.SingleType:
                    if (!TryPickSingleType(rnd, filtered, pokemon, opt, maxType, picks, out error))
                        return false;
                    break;
                default:
                    if (!TryPickThreeRandom(rnd, filtered, picks, out error))
                        return false;
                    break;
            }

            WriteStarterGroup(narc, groups[0], picks[0]);
            WriteStarterGroup(narc, groups[1], picks[1]);
            WriteStarterGroup(narc, groups[2], picks[2]);
            startersApplied = (int[])picks.Clone();
            return true;
        }

        static int MaxDepthFromBasic(FvxGen5EvolutionGraph graph, int count, int basic)
        {
            var depth = new int[count];
            for (int i = 0; i < count; i++) depth[i] = -1;
            var q = new Queue<int>();
            depth[basic] = 0;
            q.Enqueue(basic);
            int maxD = 0;
            while (q.Count > 0)
            {
                int u = q.Dequeue();
                foreach (int v in graph.Outgoing(u))
                {
                    if (v < 0 || v >= count || depth[v] >= 0) continue;
                    depth[v] = depth[u] + 1;
                    maxD = Math.Max(maxD, depth[v]);
                    q.Enqueue(v);
                }
            }
            return maxD;
        }

        static bool TryPickThreeRandom(Random rnd, List<int> filtered, int[] picks, out string error)
        {
            error = null;
            var bag = new List<int>(filtered);
            for (int s = 0; s < 3; s++)
            {
                if (bag.Count == 0)
                {
                    error = "Ran out of starter candidates.";
                    return false;
                }
                int j = rnd.Next(bag.Count);
                picks[s] = bag[j];
                bag.RemoveAt(j);
            }
            return true;
        }

        static bool TryPickUniquePrimary(Random rnd, List<int> filtered, IReadOnlyList<PokemonEntry> pokemon, int[] picks, out string error)
        {
            error = null;
            for (int attempt = 0; attempt < 4000; attempt++)
            {
                var bag = new List<int>(filtered);
                var used = new HashSet<byte>();
                bool ok = true;
                for (int s = 0; s < 3; s++)
                {
                    var candidates = bag.Where(i => !used.Contains(pokemon[i].type1)).ToList();
                    if (candidates.Count == 0)
                    {
                        ok = false;
                        break;
                    }
                    int idx = candidates[rnd.Next(candidates.Count)];
                    picks[s] = idx;
                    used.Add(pokemon[idx].type1);
                    bag.Remove(idx);
                }
                if (ok) return true;
            }
            error = "Could not pick three starters with distinct primary types.";
            return false;
        }

        static bool TryPickSingleType(Random rnd, List<int> filtered, IReadOnlyList<PokemonEntry> pokemon, FvxStartersStaticsTradesOptions opt, int maxTypeInclusive, int[] picks, out string error)
        {
            error = null;
            byte type = opt.SinglePrimaryTypeId ?? (byte)rnd.Next(0, maxTypeInclusive + 1);
            var monoOfType = filtered.Where(i =>
            {
                var pk = pokemon[i];
                return IsMono(pk) && pk.type1 == type;
            }).ToList();
            if (monoOfType.Count < 3)
            {
                error = "Not enough mono-type Pokémon for the chosen single-type starters.";
                return false;
            }
            return TryPickThreeRandom(rnd, monoOfType, picks, out error);
        }

        static bool ValidateCustomStarters(FvxStartersStaticsTradesOptions opt, IReadOnlyList<PokemonEntry> pokemon,
            IReadOnlyList<EvolutionDataEntry> evo, bool bw2, int maxType, out string error)
        {
            error = null;
            int[] idx = { opt.CustomStarterSpeciesIndex0, opt.CustomStarterSpeciesIndex1, opt.CustomStarterSpeciesIndex2 };
            for (int k = 0; k < idx.Length; k++)
            {
                int i = idx[k];
                if (i <= 0 || i >= pokemon.Count)
                {
                    error = $"Custom starter #{k + 1} has an invalid species index ({i}).";
                    return false;
                }
                if (i > FvxGen5Constants.NationalDexCount)
                {
                    error = $"Custom starter #{k + 1} is beyond the supported national dex range ({FvxGen5Constants.NationalDexCount}).";
                    return false;
                }
            }

            bool enforceFilters = opt.DontUseLegendaries || opt.NoDualTypes || opt.LimitBstMin || opt.LimitBstMax
                || (opt.Global != null && (opt.Global.LimitPokemon || opt.Global.BanIrregularAltFormes
                    || opt.Global.BanPrematureEvos));
            for (int k = 0; k < idx.Length; k++)
            {
                int i = idx[k];
                var pk = pokemon[i];
                if (!TypesInChart(pk, maxType))
                {
                    error = $"Custom starter #{k + 1} uses a type outside the current chart (enable Include Fairy or apply the type patch).";
                    return false;
                }
                if (enforceFilters && !StarterPoolMember(i, pk, opt, maxType, evo, bw2))
                {
                    error = $"Custom starter #{k + 1} does not satisfy enabled filters (BST / legendaries / types / Settings limits).";
                    return false;
                }
            }
            return true;
        }

        static bool TryPickFwg(Random rnd, List<int> filtered, IReadOnlyList<PokemonEntry> pokemon, int[] picks, out string error)
        {
            error = null;
            byte[] perm = { FvxGen5TypeChart.Fire, FvxGen5TypeChart.Water, FvxGen5TypeChart.Grass };
            Shuffle(perm, rnd);
            for (int s = 0; s < 3; s++)
            {
                byte want = perm[s];
                var cand = filtered.Where(i => pokemon[i].type1 == want).ToList();
                if (cand.Count == 0)
                {
                    error = "Could not find a starter for Fire/Water/Grass (pool filter may be too strict).";
                    return false;
                }
                picks[s] = cand[rnd.Next(cand.Count)];
                filtered.RemoveAll(x => x == picks[s]);
            }
            return true;
        }

        static bool TryPickTriangle(Random rnd, List<int> filtered, IReadOnlyList<PokemonEntry> pokemon, byte[] chart, int maxType, int[] picks, out string error)
        {
            error = null;
            if (!TryRandomDirectedTriangle(rnd, chart, maxType, out byte t0, out byte t1, out byte t2))
            {
                var trips = FvxGen5TypeChart.EnumerateTypeTriangles(chart, maxType);
                if (trips.Count == 0)
                {
                    error = FvxGen5TypeChart.ExplainNoTypeTrianglesMessage(chart, maxType);
                    return false;
                }
                var trip = trips[rnd.Next(trips.Count)];
                t0 = trip.a; t1 = trip.b; t2 = trip.c;
            }

            // Map script slots 0→1→2→0 to the directed triangle (t0→t1→t2→t0). Random full permutations
            // broke left/mid/right RPS half the time; only cyclic rotations preserve slot order.
            byte[] tri = { t0, t1, t2 };
            for (int attempt = 0; attempt < 500; attempt++)
            {
                var bag = new List<int>(filtered);
                bool ok = true;
                int rot = rnd.Next(3);
                for (int s = 0; s < 3; s++)
                {
                    byte wantType = tri[(rot + s) % 3];
                    var cand = bag.Where(i => pokemon[i].type1 == wantType).ToList();
                    if (cand.Count == 0)
                    {
                        ok = false;
                        break;
                    }
                    picks[s] = cand[rnd.Next(cand.Count)];
                    bag.Remove(picks[s]);
                }
                if (ok)
                {
                    byte a = pokemon[picks[0]].type1;
                    byte b = pokemon[picks[1]].type1;
                    byte c = pokemon[picks[2]].type1;
#if DEBUG
                    Trace.WriteLine(
                        "[FvxGen5] TryPickTriangle: tri=(" + t0 + "," + t1 + "," + t2 + ") rot=" + rot
                        + " species=(" + picks[0] + "," + picks[1] + "," + picks[2] + ") type1=(" + a + "," + b + "," + c + ")");
#endif
                    Debug.Assert(FvxGen5TypeChart.FormsDirectedTriangle(chart, maxType, a, b, c),
                        "Starter slots should follow type1 RPS: slot0 beats slot1 beats slot2 beats slot0 on the ROM chart.");
                    return true;
                }
            }
            error = "Could not assign starters for the chosen type triangle (try relaxing filters).";
            return false;
        }

        static bool TryRandomDirectedTriangle(Random rnd, byte[] chart, int maxType, out byte t0, out byte t1, out byte t2)
        {
            int n = maxType + 1;
            t0 = t1 = t2 = 0;
            var triCtx = FvxGen5TypeChart.TypeTriangleEvalContext.Create(chart, maxType);
            for (int i = 0; i < 12000; i++)
            {
                t0 = (byte)rnd.Next(n);
                t1 = (byte)rnd.Next(n);
                t2 = (byte)rnd.Next(n);
                if (t0 != t1 && t1 != t2 && t0 != t2
                    && FvxGen5TypeChart.FormsDirectedTriangle(chart, maxType, t0, t1, t2, triCtx))
                    return true;
            }
            return false;
        }

        static void WriteStarterGroup(ScriptNARC narc, FvxGen5RomLayout.FileOffset[] sites, int speciesId)
        {
            if (sites == null) return;
            foreach (var s in sites)
            {
                if (s.File < 0 || s.File >= narc.scriptFiles.Count) continue;
                var bytes = narc.scriptFiles[s.File].bytes;
                if (s.Offset >= 0 && s.Offset + 1 < bytes.Length)
                    HelperFunctions.WriteShort(bytes, s.Offset, speciesId);
            }
        }

        static void Shuffle(byte[] arr, Random rnd)
        {
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                byte tmp = arr[i];
                arr[i] = arr[j];
                arr[j] = tmp;
            }
        }

        static bool TryApplyStatics(
            FvxStartersStaticsTradesOptions opt,
            Random rnd,
            bool bw2,
            ScriptNARC narc,
            IReadOnlyList<PokemonEntry> pokemon,
            int maxType,
            out string error)
        {
            error = null;
            if (opt.StaticsMode == FvxStaticsRandomizationMode.Unchanged)
                return true;

            var patches = FvxGen5RomLayout.StaticEncounters(bw2, MainEditor.BlackVersion);
            var mainLeg = new HashSet<int>(FvxGen5RomLayout.MainGameLegendaries(bw2));

            var staticPool = new List<int>();
            for (int i = 1; i < pokemon.Count && i <= FvxGen5Constants.NationalDexCount; i++)
            {
                if (!TypesInChart(pokemon[i], maxType)) continue;
                if (opt.DontUseLegendaries && LegendaryOrMythicalNationalDex.Contains(i)) continue;
                staticPool.Add(i);
            }
            staticPool = FvxGlobalSpeciesPoolFilter.FilterPool(staticPool, opt.Global, bw2,
                MainEditor.evolutionsNarc?.evolutions, 45);

            var legendaryPool = staticPool.Where(i => LegendaryOrMythicalNationalDex.Contains(i)).ToList();
            var nonLegendPool = staticPool.Where(i => !LegendaryOrMythicalNationalDex.Contains(i)).ToList();

            foreach (var patch in patches)
            {
                foreach (var site in patch.SpeciesSites)
                {
                    if (site.File < 0 || site.File >= narc.scriptFiles.Count) continue;
                    var bytes = narc.scriptFiles[site.File].bytes;
                    if (site.Offset < 0 || site.Offset + 1 >= bytes.Length) continue;

                    int oldSp = HelperFunctions.ReadShort(bytes, site.Offset);
                    if (oldSp <= 0 || oldSp >= pokemon.Count) continue;

                    var oldPk = pokemon[oldSp];
                    if (!opt.StaticsRandomize600PlusBst && oldPk.baseStatTotal >= 600)
                        continue;

                    if (opt.StaticsLimitMainGameLegendaries && mainLeg.Contains(oldSp) && opt.StaticsMode != FvxStaticsRandomizationMode.RandomCompletely)
                        continue;

                    int replacement = oldSp;
                    switch (opt.StaticsMode)
                    {
                        case FvxStaticsRandomizationMode.SwapLegendariesAndStandards:
                            replacement = LegendaryOrMythicalNationalDex.Contains(oldSp)
                                ? PickSimilar(rnd, nonLegendPool, pokemon, oldPk.baseStatTotal)
                                : PickSimilar(rnd, legendaryPool.Count > 0 ? legendaryPool : staticPool, pokemon, oldPk.baseStatTotal);
                            break;
                        case FvxStaticsRandomizationMode.RandomCompletely:
                            replacement = staticPool[rnd.Next(staticPool.Count)];
                            break;
                        case FvxStaticsRandomizationMode.RandomSimilarStrength:
                            replacement = PickSimilar(rnd, staticPool, pokemon, oldPk.baseStatTotal);
                            break;
                    }

                    if (replacement <= 0 && staticPool.Count > 0)
                        replacement = staticPool[rnd.Next(staticPool.Count)];

                    if (replacement > 0)
                        HelperFunctions.WriteShort(bytes, site.Offset, replacement);
                }

                if (!opt.StaticsUseLevelPercentModifier || patch.LevelSites == null || patch.LevelSites.Length == 0)
                    continue;

                int pct = opt.StaticsLevelPercentModifier;
                foreach (var lvSite in patch.LevelSites)
                {
                    if (lvSite.File < 0 || lvSite.File >= narc.scriptFiles.Count) continue;
                    var bytes = narc.scriptFiles[lvSite.File].bytes;
                    if (patch.LevelSitesUseHalfword)
                    {
                        if (lvSite.Offset < 0 || lvSite.Offset + 1 >= bytes.Length) continue;
                        int oldLv = HelperFunctions.ReadShort(bytes, lvSite.Offset);
                        int nl = (int)Math.Round(oldLv * (100 + pct) / 100.0);
                        nl = HelperFunctions.Clamp(nl, 1, 100);
                        HelperFunctions.WriteShort(bytes, lvSite.Offset, nl);
                    }
                    else
                    {
                        if (lvSite.Offset < 0 || lvSite.Offset >= bytes.Length) continue;
                        int oldLv = bytes[lvSite.Offset];
                        int nl = (int)Math.Round(oldLv * (100 + pct) / 100.0);
                        nl = HelperFunctions.Clamp(nl, 1, 255);
                        HelperFunctions.WriteByte(bytes, lvSite.Offset, (byte)nl);
                    }
                }
            }

            return true;
        }

        static int PickSimilar(Random rnd, List<int> pool, IReadOnlyList<PokemonEntry> pokemon, int targetBst)
        {
            if (pool.Count == 0) return -1;
            const int window = 45;
            var close = pool.Where(i => Math.Abs(pokemon[i].baseStatTotal - targetBst) <= window).ToList();
            var use = close.Count > 0 ? close : pool;
            return use[rnd.Next(use.Count)];
        }

        static bool TryApplyTrades(FvxStartersStaticsTradesOptions opt, Random rnd, bool bw2, ScriptNARC narc, IReadOnlyList<PokemonEntry> pokemon, out string error)
        {
            error = null;
            if (opt.TradesMode == FvxTradesRandomizationMode.Unchanged)
                return true;

            var pool = FvxGen5TradeScriptPatcher.BuildTradeSpeciesPool(opt, pokemon);
            if (pool.Count == 0)
            {
                error = "Trade randomization: empty species pool.";
                return false;
            }

            FvxGen5TradeScriptPatcher.ApplyToNarc(opt, rnd, pool, narc, MainEditor.BlackVersion);
            if (opt.TradesRandomizeNicknames)
            {
                int nameFile = VersionConstants.PokemonNameTextFileID;
                var tf = MainEditor.textNarc?.textFiles;
                if (tf != null && nameFile >= 0 && nameFile < tf.Count)
                {
                    try { tf[nameFile].CompressData(); } catch { /* best-effort */ }
                }
            }
            FvxGen5TradeStoryText.Apply(opt, bw2, MainEditor.BlackVersion);
            return true;
        }
    }
}
