using System;
using System.Windows.Forms;
using NewEditor.Data.Randomization.FvxGen5;
using NewEditor.Data.Randomization.GeneShuffle;

namespace NewEditor.Forms
{
    public partial class GeneShuffleForm : Form
    {
        readonly ToolTip _geneShuffleTips = new ToolTip();

        public GeneShuffleForm()
        {
            InitializeComponent();
            if (MainEditor.RomType == RomType.BW1)
                geneShuffleControl.SetTutorEnabled(false, "Move tutor compatibility (BW1 — not applicable)");
            if (MainEditor.RomTypeId == "pokemon w")
            {
                includeFairyCheck.Enabled = false;
                includeFairyCheck.Checked = false;
                includeFairyCheck.Text = "Include Fairy-Types (not available on White 1)";
                _geneShuffleTips.SetToolTip(includeFairyCheck,
                    "Fairy Vpatch is not supported on Pokémon White 1.");
            }
            else
            {
                _geneShuffleTips.SetToolTip(includeFairyCheck,
                    "Match this to the Starters / Statics / Trades tab’s “Include Fairy-type” when you use both tools on the same ROM, so type bounds and the battle chart (17 vs 18 types) stay aligned.");
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

            try
            {
                TypeGeneRandomizer.Apply(geneShuffleControl.TypeMode, rnd, maxType, MainEditor.pokemonDataNarc.pokemon, MainEditor.evolutionsNarc.evolutions);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Type randomization failed: " + ex.Message);
                return;
            }

            // Pokemon Traits (Base Stats / Abilities / Evolutions / EXP curves) — runs after types so
            // "Same Typing" evolution filters see the freshly-rolled types.
            var traitOpt = traitsControl.BuildOptions();
            if (!FvxPokemonTraitsPipeline.TryRun(traitOpt, rnd, out var traitErr))
            {
                MessageBox.Show("Types were updated, but Pokemon Traits step failed: " + (traitErr ?? ""));
                return;
            }

            var opt = geneShuffleControl.BuildOptions();
            if (!FvxLearnsetPipeline.TryRun(opt, rnd, out var fvxErr))
            {
                MessageBox.Show("Types and traits were updated, but FVX learnset step failed: " + (fvxErr ?? ""));
                return;
            }

            MessageBox.Show("Gene Shuffle finished (types + traits + FVX learnsets). Save the ROM to keep changes.");
        }
    }
}
