using System;
using System.Collections.Generic;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// FVX TMHMTutorCompatibilityRandomizer (compatibility flags only).
    /// </summary>
    public static class FvxTmTutorCompatibilityRandomizer
    {
        public static void RandomizeTmHm(FvxRandomizerOptions opt, Random rnd, IReadOnlyList<short> tmHmMoveIds)
        {
            if (tmHmMoveIds == null || tmHmMoveIds.Count < FvxGen5Constants.TmCount + FvxGen5Constants.HmCount) return;

            var moves = MainEditor.moveDataNarc.moves;
            var pokemon = MainEditor.pokemonDataNarc.pokemon;
            bool preferType = opt.TmHmCompatMod == FvxTmHmCompatMod.RandomPreferType;
            var early = new HashSet<int>(FvxGen5TmHmMoves.EarlyRequiredHmMoveIds(MainEditor.RomType == RomType.BW2));

            void RandomizeOnePokemon(PokemonEntry pk)
            {
                for (int i = 0; i < tmHmMoveIds.Count && i < pk.TMs.Length; i++)
                {
                    int moveId = tmHmMoveIds[i];
                    var mv = moves[HelperFunctions.Clamp(moveId, 0, moves.Count - 1)];
                    double p = GetMoveCompatibilityProbability(pk, mv, early.Contains(moveId), preferType);
                    pk.TMs[i] = rnd.NextDouble() < p;
                }
            }

            void RandomizeBasicIndex(int speciesIndex)
            {
                if (speciesIndex < 0 || speciesIndex >= pokemon.Count) return;
                RandomizeOnePokemon(pokemon[speciesIndex]);
            }

            void CopyUp(int evFrom, int evTo, bool _)
            {
                var from = pokemon[evFrom];
                var to = pokemon[evTo];
                for (int i = 0; i < tmHmMoveIds.Count && i < to.TMs.Length; i++)
                {
                    if (!from.TMs[i])
                    {
                        int moveId = tmHmMoveIds[i];
                        var mv = moves[HelperFunctions.Clamp(moveId, 0, moves.Count - 1)];
                        double p = 0.25;
                        if (preferType)
                        {
                            p = 0.1;
                            if (TypeMatchesNewOnEvo(from, to, mv.element)) p = 0.9;
                        }
                        to.TMs[i] = rnd.NextDouble() < p;
                    }
                    else to.TMs[i] = from.TMs[i];
                }
            }

            bool randomizedMode = opt.TmHmCompatMod == FvxTmHmCompatMod.CompletelyRandom || opt.TmHmCompatMod == FvxTmHmCompatMod.RandomPreferType;
            if (randomizedMode && opt.TmsFollowEvolutions && MainEditor.evolutionsNarc?.evolutions != null)
            {
                var g = FvxGen5EvolutionGraph.FromEvolutions(MainEditor.evolutionsNarc.evolutions);
                g.ApplyCopyUp(RandomizeBasicIndex, CopyUp);
            }
            else if (randomizedMode)
            {
                foreach (var pk in pokemon) RandomizeOnePokemon(pk);
            }
            else if (opt.TmHmCompatMod == FvxTmHmCompatMod.FullCompatibility)
            {
                foreach (var pk in pokemon)
                    for (int i = 0; i < tmHmMoveIds.Count && i < pk.TMs.Length; i++)
                        pk.TMs[i] = true;
            }

            if (opt.TmLevelupMoveSanity)
            {
                EnsureTmLevelupSanity(tmHmMoveIds);
                if (opt.TmsFollowEvolutions)
                    EnsureTmEvolutionSanity(tmHmMoveIds);
            }
            if (opt.FullHmCompatibility)
                EnsureFullHmCompatibility(tmHmMoveIds);

            foreach (var pk in pokemon) pk.ApplyData();
        }

        static bool TypeMatchesNewOnEvo(PokemonEntry evFrom, PokemonEntry evTo, byte mvType)
        {
            bool prim = mvType == evTo.type1 && mvType != evFrom.type1 && mvType != evFrom.type2;
            bool sec = evTo.type2 != evTo.type1 && evTo.type2 != 255 && mvType == evTo.type2
                && mvType != evFrom.type2 && mvType != evFrom.type1;
            return prim || sec;
        }

        public static void RandomizeTutors(FvxRandomizerOptions opt, Random rnd, IReadOnlyList<short> tutorMoveIds)
        {
            if (MainEditor.RomType != RomType.BW2) return;
            if (tutorMoveIds == null || tutorMoveIds.Count == 0) return;

            var moves = MainEditor.moveDataNarc.moves;
            var pokemon = MainEditor.pokemonDataNarc.pokemon;
            bool preferType = opt.TutorCompatMod == FvxTutorCompatMod.RandomPreferType;
            var early = new HashSet<int>();

            void RandomizeOnePokemon(PokemonEntry pk)
            {
                for (int ti = 0; ti < tutorMoveIds.Count; ti++)
                {
                    int mid = tutorMoveIds[ti];
                    var mv = moves[HelperFunctions.Clamp(mid, 0, moves.Count - 1)];
                    double p = GetMoveCompatibilityProbability(pk, mv, early.Contains(mid), preferType);
                    bool v = rnd.NextDouble() < p;
                    SetTutorFlag(pk, ti, v);
                }
            }

            void RandomizeBasicIndex(int speciesIndex)
            {
                if (speciesIndex < 0 || speciesIndex >= pokemon.Count) return;
                RandomizeOnePokemon(pokemon[speciesIndex]);
            }

            void CopyUp(int evFrom, int evTo, bool _)
            {
                var from = pokemon[evFrom];
                var to = pokemon[evTo];
                for (int ti = 0; ti < tutorMoveIds.Count; ti++)
                {
                    bool prev = GetTutorFlag(from, ti);
                    if (!prev)
                    {
                        int mid = tutorMoveIds[ti];
                        var mv = moves[HelperFunctions.Clamp(mid, 0, moves.Count - 1)];
                        double p = 0.25;
                        if (preferType)
                        {
                            p = 0.1;
                            if (TypeMatchesNewOnEvo(from, to, mv.element)) p = 0.9;
                        }
                        SetTutorFlag(to, ti, rnd.NextDouble() < p);
                    }
                    else SetTutorFlag(to, ti, prev);
                }
            }

            bool randomizedMode = opt.TutorCompatMod == FvxTutorCompatMod.CompletelyRandom || opt.TutorCompatMod == FvxTutorCompatMod.RandomPreferType;
            if (randomizedMode && opt.TutorFollowEvolutions && MainEditor.evolutionsNarc?.evolutions != null)
            {
                var g = FvxGen5EvolutionGraph.FromEvolutions(MainEditor.evolutionsNarc.evolutions);
                g.ApplyCopyUp(RandomizeBasicIndex, CopyUp);
            }
            else if (randomizedMode)
            {
                foreach (var pk in pokemon) RandomizeOnePokemon(pk);
            }
            else if (opt.TutorCompatMod == FvxTutorCompatMod.FullCompatibility)
            {
                foreach (var pk in pokemon)
                    for (int ti = 0; ti < tutorMoveIds.Count; ti++)
                        SetTutorFlag(pk, ti, true);
            }

            if (opt.TutorLevelupMoveSanity)
            {
                EnsureTutorLevelupSanity(tutorMoveIds);
                if (opt.TutorFollowEvolutions)
                    EnsureTutorEvolutionSanity(tutorMoveIds);
            }

            foreach (var pk in pokemon) pk.ApplyData();
        }

        static void EnsureTmLevelupSanity(IReadOnlyList<short> tmHmMoveIds)
        {
            if (MainEditor.learnsetNarc?.learnsets == null) return;
            var tmLookup = new Dictionary<int, int>();
            for (int i = 0; i < tmHmMoveIds.Count; i++)
                if (!tmLookup.ContainsKey(tmHmMoveIds[i])) tmLookup[tmHmMoveIds[i]] = i;

            var pokemon = MainEditor.pokemonDataNarc.pokemon;
            int n = Math.Min(pokemon.Count, MainEditor.learnsetNarc.learnsets.Count);
            for (int pi = 0; pi < n; pi++)
            {
                var ls = MainEditor.learnsetNarc.learnsets[pi];
                if (ls?.moves == null) continue;
                var pk = pokemon[pi];
                foreach (var slot in ls.moves)
                {
                    int mid = slot.moveID;
                    if (tmLookup.TryGetValue(mid, out var tmIndex) && tmIndex < pk.TMs.Length)
                        pk.TMs[tmIndex] = true;
                }
            }
        }

        static void EnsureTutorLevelupSanity(IReadOnlyList<short> tutorMoveIds)
        {
            if (MainEditor.learnsetNarc?.learnsets == null) return;
            var tutorLookup = new Dictionary<int, int>();
            for (int i = 0; i < tutorMoveIds.Count; i++)
                if (!tutorLookup.ContainsKey(tutorMoveIds[i])) tutorLookup[tutorMoveIds[i]] = i;

            var pokemon = MainEditor.pokemonDataNarc.pokemon;
            int n = Math.Min(pokemon.Count, MainEditor.learnsetNarc.learnsets.Count);
            for (int pi = 0; pi < n; pi++)
            {
                var ls = MainEditor.learnsetNarc.learnsets[pi];
                if (ls?.moves == null) continue;
                var pk = pokemon[pi];
                foreach (var slot in ls.moves)
                {
                    int mid = slot.moveID;
                    if (tutorLookup.TryGetValue(mid, out var tutorIndex))
                        SetTutorFlag(pk, tutorIndex, true);
                }
            }
        }

        static void EnsureTmEvolutionSanity(IReadOnlyList<short> tmHmMoveIds)
        {
            if (MainEditor.evolutionsNarc?.evolutions == null) return;
            var pokemon = MainEditor.pokemonDataNarc.pokemon;
            var g = FvxGen5EvolutionGraph.FromEvolutions(MainEditor.evolutionsNarc.evolutions);
            g.ApplyCopyUp(_ => { }, (evFrom, evTo, _) =>
            {
                var from = pokemon[evFrom];
                var to = pokemon[evTo];
                for (int i = 0; i < tmHmMoveIds.Count && i < from.TMs.Length && i < to.TMs.Length; i++)
                    if (from.TMs[i]) to.TMs[i] = true;
            });
        }

        static void EnsureTutorEvolutionSanity(IReadOnlyList<short> tutorMoveIds)
        {
            if (MainEditor.evolutionsNarc?.evolutions == null) return;
            var pokemon = MainEditor.pokemonDataNarc.pokemon;
            var g = FvxGen5EvolutionGraph.FromEvolutions(MainEditor.evolutionsNarc.evolutions);
            g.ApplyCopyUp(_ => { }, (evFrom, evTo, _) =>
            {
                var from = pokemon[evFrom];
                var to = pokemon[evTo];
                for (int ti = 0; ti < tutorMoveIds.Count; ti++)
                    if (GetTutorFlag(from, ti)) SetTutorFlag(to, ti, true);
            });
        }

        static void EnsureFullHmCompatibility(IReadOnlyList<short> tmHmMoveIds)
        {
            int hmStart = FvxGen5Constants.TmCount;
            int hmEnd = Math.Min(tmHmMoveIds.Count, hmStart + FvxGen5Constants.HmCount);
            foreach (var pk in MainEditor.pokemonDataNarc.pokemon)
            {
                for (int i = hmStart; i < hmEnd && i < pk.TMs.Length; i++)
                    pk.TMs[i] = true;
            }
        }

        static bool GetTutorFlag(PokemonEntry p, int ti)
        {
            if (ti < 7) return p.miscTutors[ti];
            ti -= 7;
            if (ti < 15) return p.driftveilTutors[ti];
            ti -= 15;
            if (ti < 17) return p.lentimasTutors[ti];
            ti -= 17;
            if (ti < 13) return p.humilauTutors[ti];
            ti -= 13;
            return p.nacreneTutors[ti];
        }

        static void SetTutorFlag(PokemonEntry p, int ti, bool v)
        {
            if (ti < 7) p.miscTutors[ti] = v;
            else if (ti < 7 + 15) p.driftveilTutors[ti - 7] = v;
            else if (ti < 7 + 15 + 17) p.lentimasTutors[ti - 7 - 15] = v;
            else if (ti < 7 + 15 + 17 + 13) p.humilauTutors[ti - 7 - 15 - 17] = v;
            else p.nacreneTutors[ti - 7 - 15 - 17 - 13] = v;
        }

        static double GetMoveCompatibilityProbability(PokemonEntry pkmn, MoveDataEntry mv, bool requiredEarlyOn, bool preferSameType)
        {
            double probability = 0.5;
            if (preferSameType)
            {
                if (mv.element == pkmn.type1 || (pkmn.type2 != pkmn.type1 && pkmn.type2 != 255 && mv.element == pkmn.type2))
                    probability = 0.9;
                else if (mv.element == 0) probability = 0.5;
                else probability = 0.25;
            }
            if (requiredEarlyOn) probability = Math.Min(1.0, probability * 1.8);
            return probability;
        }
    }
}
