using System;
using System.Collections.Generic;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Shared FVX learnset + egg + TM/HM + tutor randomization entry point.</summary>
    public static class FvxLearnsetPipeline
    {
        /// <summary>Runs TM/HM read and all FVX randomizers. Returns false on TM table failure or exception.</summary>
        public static bool TryRun(FvxRandomizerOptions opt, Random rnd, out string error)
        {
            error = null;
            if (MainEditor.fileSystem == null)
            {
                error = "No file system loaded.";
                return false;
            }
            if (!FvxGen5TmHmMoves.TryReadTmHmMoveOrder(MainEditor.fileSystem.arm9, out var tmHm, out var tmErr))
            {
                error = "TM/HM table: " + tmErr;
                return false;
            }
            try
            {
                FvxSpeciesMovesetRandomizer.RandomizeLevelUp(opt, rnd, tmHm);
                FvxSpeciesMovesetRandomizer.RandomizeEggMoves(opt, rnd, tmHm);
                FvxTmTutorCompatibilityRandomizer.RandomizeTmHm(opt, rnd, tmHm);
                IReadOnlyList<short> tutorIds;
                if (MainEditor.RomType == RomType.BW2)
                    tutorIds = FvxGen5TmHmMoves.ResolveBw2TutorMoveIds();
                else
                    tutorIds = Array.Empty<short>();
                FvxTmTutorCompatibilityRandomizer.RandomizeTutors(opt, rnd, tutorIds);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
