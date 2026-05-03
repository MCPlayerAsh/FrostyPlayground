using System;
using System.Collections.Generic;
using System.Linq;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// FVX SpeciesMovesetRandomizer behavior for Gen5 learnsets + egg moves (behavioral port).
    /// </summary>
    public static class FvxSpeciesMovesetRandomizer
    {
        const int PerfectAccuracy = 100;

        public static void RandomizeLevelUp(FvxRandomizerOptions opt, Random rnd, IReadOnlyList<short> tmHmMoveIds)
        {
            if (opt.MovesetsMod == FvxMovesetsMod.Unchanged) return;
            var moves = MainEditor.moveDataNarc.moves;
            var pokemon = MainEditor.pokemonDataNarc.pokemon;
            var learnsets = MainEditor.learnsetNarc.learnsets;
            bool typeThemed = opt.MovesetsMod == FvxMovesetsMod.RandomPreferSameType;
            bool noBroken = opt.BlockBrokenMovesetMoves;
            bool forceStarting = MainEditor.RomType != RomType.HGSS && opt.StartWithGuaranteedMoves;
            int forceCount = Math.Max(2, Math.Min(4, opt.GuaranteedMoveCount));
            double goodDamagingFrac = opt.MovesetsForceGoodDamaging ? opt.MovesetsGoodDamagingPercent / 100.0 : 0;
            bool evoMovesAll = opt.EvolutionMovesForAll;

            var hmIds = new HashSet<int>();
            if (tmHmMoveIds != null)
                foreach (var id in tmHmMoveIds) hmIds.Add(id);

            var extraBanned = new List<int>();
            CreateMovePools(rnd, moves, noBroken, hmIds, extraBanned, out var validMoves, out var validDamaging,
                out var validTypeMoves, out var validTypeDamaging);

            int n = Math.Min(pokemon.Count, learnsets.Count);
            for (int pkmnNum = 0; pkmnNum < n; pkmnNum++)
            {
                var pk = pokemon[pkmnNum];
                var ls = learnsets[pkmnNum];
                if (ls?.moves == null) continue;

                var movesList = new List<LevelUpMoveSlot>(ls.moves);

                if (IsCosmeticForm(pk, pokemon))
                {
                    int baseIdx = BaseFormIndex(pk, pokemon);
                    if (baseIdx >= 0 && baseIdx < learnsets.Count && learnsets[baseIdx].moves != null)
                    {
                        for (int i = 0; i < movesList.Count && i < learnsets[baseIdx].moves.Count; i++)
                            movesList[i] = new LevelUpMoveSlot(learnsets[baseIdx].moves[i].moveID, movesList[i].level);
                    }
                    ls.moves = movesList;
                    ls.ApplyData();
                    pk.levelUpMoves = ls;
                    continue;
                }

                double atkSpAtkRatio = AtkSpAtkRatio(pk);

                if (forceStarting)
                {
                    int lv1 = 0;
                    foreach (var ml in movesList) if (ml.level == 1) lv1++;
                    for (int i = 0; i < forceCount - lv1; i++)
                        movesList.Insert(0, new LevelUpMoveSlot(0, 1));
                }

                if (evoMovesAll && movesList.Count > 0 && movesList[0].level != 0)
                    movesList.Insert(0, new LevelUpMoveSlot(0, 0));

                int lv1index = movesList.Count > 0 && movesList[0].level == 1 ? 0 : 1;
                while (lv1index < movesList.Count && movesList[lv1index].level == 1) lv1index++;
                if (lv1index != 0) lv1index--;

                int goodDamagingLeft = (int)Math.Round(goodDamagingFrac * movesList.Count);
                var learnt = new List<int>();

                int lv1AttackingMove = 0;
                for (int i = 0; i < movesList.Count; i++)
                {
                    bool attemptDamaging = i == lv1index || goodDamagingLeft > 0;
                    byte? typeOfMove = PickTypeTheme(rnd, typeThemed, pk);

                    var pickList = PickMoveList(rnd, validMoves, validDamaging, validTypeMoves, validTypeDamaging,
                        typeOfMove, attemptDamaging, atkSpAtkRatio, learnt);

                    var mv = pickList[rnd.Next(pickList.Count)];
                    while (learnt.Contains(mv.nameID))
                        mv = pickList[rnd.Next(pickList.Count)];

                    if (i == lv1index) lv1AttackingMove = mv.nameID;
                    else goodDamagingLeft--;

                    learnt.Add(mv.nameID);
                }

                Shuffle(learnt, rnd);
                if (lv1index >= 0 && lv1index < learnt.Count && learnt[lv1index] != lv1AttackingMove)
                {
                    int swap = learnt.IndexOf(lv1AttackingMove);
                    if (swap >= 0)
                    {
                        int t = learnt[lv1index];
                        learnt[lv1index] = learnt[swap];
                        learnt[swap] = t;
                    }
                }

                for (int i = 0; i < movesList.Count && i < learnt.Count; i++)
                {
                    movesList[i] = new LevelUpMoveSlot((short)learnt[i], movesList[i].level);
                    if (i == lv1index) movesList[i] = new LevelUpMoveSlot((short)learnt[i], 1);
                }

                ls.moves = movesList;
                ls.ApplyData();
                pk.levelUpMoves = ls;
            }
        }

        public static void RandomizeEggMoves(FvxRandomizerOptions opt, Random rnd, IReadOnlyList<short> tmHmMoveIds)
        {
            if (!opt.RandomizeEggMoves || opt.MovesetsMod == FvxMovesetsMod.Unchanged) return;
            if (MainEditor.eggMoveNarc?.entries == null || MainEditor.moveDataNarc?.moves == null) return;

            var moves = MainEditor.moveDataNarc.moves;
            var pokemon = MainEditor.pokemonDataNarc.pokemon;
            var egg = MainEditor.eggMoveNarc.entries;
            bool typeThemed = opt.MovesetsMod == FvxMovesetsMod.RandomPreferSameType;
            bool noBroken = opt.BlockBrokenMovesetMoves;
            double goodDamagingFrac = opt.MovesetsForceGoodDamaging ? opt.MovesetsGoodDamagingPercent / 100.0 : 0;

            var hmIds = new HashSet<int>();
            if (tmHmMoveIds != null) foreach (var id in tmHmMoveIds) hmIds.Add(id);
            var extraBanned = new List<int>();
            CreateMovePools(rnd, moves, noBroken, hmIds, extraBanned, out var validMoves, out var validDamaging,
                out var validTypeMoves, out var validTypeDamaging);

            int n = Math.Min(pokemon.Count, egg.Count);
            for (int pkmnNum = 0; pkmnNum < n; pkmnNum++)
            {
                var pk = pokemon[pkmnNum];
                var entry = egg[pkmnNum];
                if (entry?.moves == null) continue;

                if (IsCosmeticForm(pk, pokemon))
                {
                    int baseIdx = BaseFormIndex(pk, pokemon);
                    if (baseIdx >= 0 && baseIdx < egg.Count && egg[baseIdx].moves != null)
                    {
                        entry.moves.Clear();
                        foreach (var m in egg[baseIdx].moves) entry.moves.Add(m);
                    }
                    entry.ApplyData();
                    continue;
                }

                double atkSpAtkRatio = AtkSpAtkRatio(pk);
                var movesList = new List<short>(entry.moves);
                int goodDamagingLeft = (int)Math.Round(goodDamagingFrac * Math.Max(1, movesList.Count));
                var learnt = new List<int>();

                for (int i = 0; i < movesList.Count; i++)
                {
                    bool attemptDamaging = goodDamagingLeft > 0;
                    byte? typeOfMove = PickTypeTheme(rnd, typeThemed, pk);
                    var pickList = PickMoveList(rnd, validMoves, validDamaging, validTypeMoves, validTypeDamaging,
                        typeOfMove, attemptDamaging, atkSpAtkRatio, learnt);
                    var mv = pickList[rnd.Next(pickList.Count)];
                    while (learnt.Contains(mv.nameID))
                        mv = pickList[rnd.Next(pickList.Count)];
                    learnt.Add(mv.nameID);
                    goodDamagingLeft--;
                }

                Shuffle(learnt, rnd);
                entry.moves.Clear();
                foreach (var id in learnt) entry.moves.Add((short)id);
                entry.ApplyData();
            }
        }

        static void CreateMovePools(Random rnd, List<MoveDataEntry> allMoves, bool noBroken, HashSet<int> hmIds, List<int> extraBanned,
            out List<MoveDataEntry> validMoves, out List<MoveDataEntry> validDamaging,
            out Dictionary<byte, List<MoveDataEntry>> validTypeMoves,
            out Dictionary<byte, List<MoveDataEntry>> validTypeDamaging)
        {
            var ban = FvxGen5MoveBanList.AllBannedForPools(noBroken, hmIds?.ToList() ?? new List<int>(), extraBanned);
            validMoves = new List<MoveDataEntry>();
            validDamaging = new List<MoveDataEntry>();
            validTypeMoves = new Dictionary<byte, List<MoveDataEntry>>();
            validTypeDamaging = new Dictionary<byte, List<MoveDataEntry>>();

            foreach (var mv in allMoves)
            {
                if (mv == null) continue;
                int id = mv.nameID;
                if (id < 0 || id >= allMoves.Count) continue;
                if (FvxGen5MoveBanList.IsBannedFromRandomPools(id) || ban.Contains(id)) continue;
                if (mv.category == 9) continue;
                validMoves.Add(mv);
                if (!validTypeMoves.ContainsKey(mv.element)) validTypeMoves[mv.element] = new List<MoveDataEntry>();
                validTypeMoves[mv.element].Add(mv);

                if (!FvxGen5MoveBanList.IsBannedFromDamagingPool(id) && FvxGen5MoveScoring.IsGoodDamaging(mv, PerfectAccuracy))
                {
                    validDamaging.Add(mv);
                    if (!validTypeDamaging.ContainsKey(mv.element)) validTypeDamaging[mv.element] = new List<MoveDataEntry>();
                    validTypeDamaging[mv.element].Add(mv);
                }
            }

            BalanceTypePowers(validMoves, validTypeMoves, validTypeDamaging, rnd);
        }

        static void BalanceTypePowers(List<MoveDataEntry> validMoves, Dictionary<byte, List<MoveDataEntry>> validTypeMoves,
            Dictionary<byte, List<MoveDataEntry>> validTypeDamaging, Random random)
        {
            if (validTypeMoves.Count == 0) return;
            var avgTypePowers = new Dictionary<byte, double>();
            double totalAvg = 0;
            foreach (var kv in validTypeMoves)
            {
                double sum = 0;
                foreach (var m in kv.Value)
                {
                    if (m.basePower > 0) sum += m.basePower * FvxGen5MoveScoring.GetHitCount(m);
                }
                double avg = kv.Value.Count > 0 ? sum / kv.Value.Count : 0;
                avgTypePowers[kv.Key] = avg;
                totalAvg += avg;
            }
            totalAvg /= validTypeMoves.Count;
            double minAvg = totalAvg * 0.75;
            double maxAvg = totalAvg * 1.25;

            foreach (var type in avgTypePowers.Keys.ToList())
            {
                var typeMoves = validTypeMoves[type];
                double avgPowerForType = avgTypePowers[type];
                var alreadyPicked = new HashSet<MoveDataEntry>();
                int iter = 0;
                while (avgPowerForType < minAvg && iter < 10000)
                {
                    var stronger = typeMoves.Where(mv => mv.basePower * FvxGen5MoveScoring.GetHitCount(mv) > avgPowerForType).ToList();
                    if (stronger.Count == 0) break;
                    stronger.RemoveAll(m => alreadyPicked.Contains(m));
                    if (stronger.Count == 0) { alreadyPicked.Clear(); continue; }
                    var extraMove = stronger[random.Next(stronger.Count)];
                    alreadyPicked.Add(extraMove);
                    avgPowerForType = (avgPowerForType * typeMoves.Count + extraMove.basePower * FvxGen5MoveScoring.GetHitCount(extraMove)) / (typeMoves.Count + 1);
                    typeMoves.Add(extraMove);
                    iter++;
                }
                iter = 0;
                alreadyPicked.Clear();
                while (avgPowerForType > maxAvg && iter < 10000)
                {
                    var weaker = typeMoves.Where(mv => mv.basePower * FvxGen5MoveScoring.GetHitCount(mv) < avgPowerForType).ToList();
                    if (weaker.Count == 0) break;
                    weaker.RemoveAll(m => alreadyPicked.Contains(m));
                    if (weaker.Count == 0) { alreadyPicked.Clear(); continue; }
                    var extraMove = weaker[random.Next(weaker.Count)];
                    alreadyPicked.Add(extraMove);
                    avgPowerForType = (avgPowerForType * typeMoves.Count + extraMove.basePower * FvxGen5MoveScoring.GetHitCount(extraMove)) / (typeMoves.Count + 1);
                    typeMoves.Add(extraMove);
                    iter++;
                }
            }
        }

        static List<MoveDataEntry> PickMoveList(Random rnd, List<MoveDataEntry> validMoves,
            List<MoveDataEntry> validDamaging, Dictionary<byte, List<MoveDataEntry>> validTypeMoves,
            Dictionary<byte, List<MoveDataEntry>> validTypeDamaging, byte? typeOfMove, bool attemptDamaging,
            double atkSpAtkRatio, List<int> learnt)
        {
            List<MoveDataEntry> pickList = validMoves;
            if (attemptDamaging)
            {
                if (typeOfMove.HasValue && validTypeDamaging.TryGetValue(typeOfMove.Value, out var tdl) && CheckUnused(tdl, learnt))
                    pickList = tdl;
                else if (CheckUnused(validDamaging, learnt))
                    pickList = validDamaging;

                bool phys = rnd.NextDouble() < atkSpAtkRatio;
                var filtered = pickList.Where(mv => FvxGen5MoveScoring.IsPhysicalCategory(mv) == phys).ToList();
                if (filtered.Count > 0 && CheckUnused(filtered, learnt))
                    pickList = filtered;
            }
            else if (typeOfMove.HasValue && validTypeMoves.TryGetValue(typeOfMove.Value, out var tl) && CheckUnused(tl, learnt))
                pickList = tl;

            if (pickList.Count == 0) pickList = validMoves;
            return pickList;
        }

        static bool CheckUnused(List<MoveDataEntry> potential, List<int> used)
        {
            foreach (var mv in potential)
                if (!used.Contains(mv.nameID)) return true;
            return false;
        }

        static byte? PickTypeTheme(Random rnd, bool typeThemed, PokemonEntry pkmn)
        {
            if (!typeThemed) return null;
            double picked = rnd.NextDouble();
            const byte normal = 0;
            bool monoType = pkmn.type2 == pkmn.type1 || pkmn.type2 == 255;
            if ((pkmn.type1 == normal && !monoType) || pkmn.type2 == normal)
            {
                byte other = pkmn.type1 == normal ? pkmn.type2 : pkmn.type1;
                if (picked < 0.1) return normal;
                if (picked < 0.4) return other;
                return null;
            }
            if (!monoType)
            {
                if (picked < 0.2) return pkmn.type1;
                if (picked < 0.4) return pkmn.type2;
                return null;
            }
            if (picked < 0.4) return pkmn.type1;
            return null;
        }

        static double AtkSpAtkRatio(PokemonEntry p)
        {
            int a = p.baseAttack;
            int sa = p.baseSpAtt;
            if (a + sa <= 0) return 0.5;
            return (double)a / (a + sa);
        }

        static bool IsCosmeticForm(PokemonEntry pk, List<PokemonEntry> all) => pk.formID > 0;

        static int BaseFormIndex(PokemonEntry pk, List<PokemonEntry> all)
        {
            for (int j = 0; j < all.Count; j++)
            {
                if (all[j].nameID == pk.nameID && all[j].formID == 0) return j;
            }
            return 0;
        }

        static void Shuffle<T>(IList<T> list, Random rnd)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
