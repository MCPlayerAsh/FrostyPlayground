using System;
using System.Text;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Runs FVX-style stages in a safe order on the loaded in-memory ROM.</summary>
    public static class FvxGen5RandomizerOrchestrator
    {
        public static bool TryRun(FvxGen5FullSettings settings, int seed, out string report)
        {
            var log = new StringBuilder();
            if (MainEditor.fileSystem == null || MainEditor.pokemonDataNarc == null)
            {
                report = "Load a ROM first.";
                return false;
            }
            if (MainEditor.RomType != RomType.BW1 && MainEditor.RomType != RomType.BW2)
            {
                report = "FVX-style randomizer supports Black/White and Black 2/White 2 only.";
                return false;
            }

            var rnd = seed != 0 ? new Random(seed) : new Random();
            var ctx = TypeSystemContext.FromLoadedRom();
            log.AppendLine("Type system: max type index = " + ctx.MaxTypeIndexInclusive + ", Fairy = " + ctx.HasFairy);

            try
            {
                FvxPokemonTraitsRandomizer.Apply(settings.Traits, rnd, ctx);

                FvxMoveDataRandomizer.Apply(settings.MoveData, rnd, ctx);

                if (!FvxGen5MoveListStages.ApplyRandomizeTmMoveList(settings.TmHmTutorExtras, rnd, log))
                {
                    report = log.ToString();
                    return false;
                }
                if (!FvxGen5MoveListStages.ApplyRandomizeTutorMoveList(settings.TmHmTutorExtras, rnd, log))
                {
                    report = log.ToString();
                    return false;
                }

                if (!FvxLearnsetPipeline.TryRun(settings.Core, rnd, out var learnErr))
                {
                    log.AppendLine("Learnset/TM pipeline: " + learnErr);
                    report = log.ToString();
                    return false;
                }
                log.AppendLine("Learnsets / TM-HM compat / tutors: OK.");

                FvxGen5PlaceholderStages.StartersStaticsTrades(settings, rnd, log);

                FvxTrainerPokemonRandomizer.Apply(settings.Foe, rnd, ctx);
                log.AppendLine("Trainer Pokémon: " + settings.Foe.TrainerPokemon + ".");

                FvxWildEncounterRandomizer.Apply(settings.Wild, rnd, ctx);
                log.AppendLine("Wild encounters: " + settings.Wild.WildMode + ".");

                FvxGen5PlaceholderStages.Items(settings, rnd, log);
                FvxGen5PlaceholderStages.MiscTweaks(settings, log);

                report = log.ToString();
                return true;
            }
            catch (Exception ex)
            {
                report = log + Environment.NewLine + ex.Message;
                return false;
            }
        }
    }
}
