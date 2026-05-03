using System;
using System.Windows.Forms;
using NewEditor.Data.Randomization.FvxGen5;
using NewEditor.Data.Randomization.GeneShuffle;

namespace NewEditor.Forms
{
    public partial class GeneShuffleForm : Form
    {
        public GeneShuffleForm()
        {
            InitializeComponent();
            if (MainEditor.RomType == RomType.BW1)
            {
                tutorGroup.Enabled = false;
                tutorGroup.Text = "Move tutor compatibility (BW1 — not applicable)";
            }
            if (MainEditor.RomTypeId == "pokemon w")
            {
                includeFairyCheck.Enabled = false;
                includeFairyCheck.Checked = false;
                includeFairyCheck.Text = "Include Fairy-Types (not available on White 1)";
            }
        }

        void ApplyClick(object sender, EventArgs e)
        {
            if (MainEditor.RomType != RomType.BW1 && MainEditor.RomType != RomType.BW2)
            {
                MessageBox.Show("Gene Shuffle supports Black/White and Black 2/White 2 only.");
                return;
            }
            if (MainEditor.pokemonDataNarc == null || MainEditor.moveDataNarc == null || MainEditor.learnsetNarc == null
                || MainEditor.fileSystem == null || MainEditor.evolutionsNarc?.evolutions == null)
            {
                MessageBox.Show("Load a ROM first (including evolution data).");
                return;
            }

            if (includeFairyCheck.Checked && MainEditor.RomTypeId == "pokemon w")
            {
                MessageBox.Show("Fairy-type support is not available on Pokémon White 1. Uncheck \"Include Fairy-Types\" or use Black 1, Black 2, or White 2.");
                return;
            }

            if (!GeneShuffleFairyPatch.TryPrepare(includeFairyCheck.Checked, out var patchApplied, out var fairyErr))
            {
                MessageBox.Show(fairyErr ?? "Fairy patch step failed.");
                return;
            }

            int maxType = GeneShuffleFairyPatch.MaxTypeInclusive(includeFairyCheck.Checked, patchApplied);
            int seed = (int)seedNumeric.Value;
            var rnd = seed != 0 ? new Random(seed) : new Random();

            var typeMode = (GeneShuffleTypeMode)typeModeCombo.SelectedIndex;
            try
            {
                TypeGeneRandomizer.Apply(typeMode, rnd, maxType, MainEditor.pokemonDataNarc.pokemon, MainEditor.evolutionsNarc.evolutions);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Type randomization failed: " + ex.Message);
                return;
            }

            var opt = new FvxRandomizerOptions
            {
                MovesetsMod = (FvxMovesetsMod)movesetModCombo.SelectedIndex,
                BlockBrokenMovesetMoves = blockBrokenMovesCheck.Checked,
                StartWithGuaranteedMoves = guaranteedStartCheck.Checked,
                GuaranteedMoveCount = (int)guaranteedCountNumeric.Value,
                MovesetsForceGoodDamaging = forceGoodDamagingCheck.Checked,
                MovesetsGoodDamagingPercent = (int)goodDamagingPercentNumeric.Value,
                EvolutionMovesForAll = evoMoveAllCheck.Checked,
                RandomizeEggMoves = randomizeEggCheck.Checked,
                TmHmCompatMod = (FvxTmHmCompatMod)tmHmModCombo.SelectedIndex,
                TmsFollowEvolutions = tmFollowEvoCheck.Checked,
                TutorCompatMod = (FvxTutorCompatMod)tutorModCombo.SelectedIndex,
                TutorFollowEvolutions = tutorFollowEvoCheck.Checked
            };

            if (!FvxLearnsetPipeline.TryRun(opt, rnd, out var fvxErr))
            {
                MessageBox.Show("Types were updated, but FVX learnset step failed: " + (fvxErr ?? ""));
                return;
            }

            MessageBox.Show("Gene Shuffle finished (types + FVX learnsets). Save the ROM to keep changes.");
        }
    }
}
