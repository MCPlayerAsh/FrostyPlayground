using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NewEditor.Data.Randomization.FvxGen5;

namespace NewEditor.Forms
{
    public partial class LearnsetRandomizerFvxForm : Form
    {
        readonly TmHmTutorsControl tmHmTutorsControl;

        public LearnsetRandomizerFvxForm()
        {
            InitializeComponent();
            tmHmTutorsControl = new TmHmTutorsControl();
            tmHmTutorsControl.Location = tmHmGroup.Location;
            tmHmTutorsControl.Size = new System.Drawing.Size(420, 186);
            Controls.Add(tmHmTutorsControl);
            tmHmGroup.Visible = false;
            tutorGroup.Visible = false;
            if (MainEditor.RomType == RomType.BW1)
            {
                tmHmTutorsControl.SetTutorEnabled(false, "Move tutor compatibility (BW1 — not applicable)");
            }
        }

        void ApplyClick(object sender, EventArgs e)
        {
            if (MainEditor.RomType != RomType.BW1 && MainEditor.RomType != RomType.BW2)
            {
                MessageBox.Show("This tool supports Black/White and Black 2/White 2 only.");
                return;
            }
            if (MainEditor.pokemonDataNarc == null || MainEditor.moveDataNarc == null || MainEditor.learnsetNarc == null
                || MainEditor.fileSystem == null)
            {
                MessageBox.Show("Load a ROM first.");
                return;
            }

            int seed = (int)seedNumeric.Value;
            var rnd = seed != 0 ? new Random(seed) : new Random();

            var opt = new FvxRandomizerOptions
            {
                MovesetsMod = (FvxMovesetsMod)movesetModCombo.SelectedIndex,
                BlockBrokenMovesetMoves = blockBrokenMovesCheck.Checked,
                StartWithGuaranteedMoves = guaranteedStartCheck.Checked,
                GuaranteedMoveCount = (int)guaranteedCountNumeric.Value,
                ReorderDamagingMoves = reorderDamagingCheck.Checked,
                MovesetsForceGoodDamaging = forceGoodDamagingCheck.Checked,
                MovesetsGoodDamagingPercent = (int)goodDamagingPercentNumeric.Value,
                EvolutionMovesForAll = evoMoveAllCheck.Checked,
                RandomizeEggMoves = randomizeEggCheck.Checked
            };
            tmHmTutorsControl.ApplyToOptions(opt);

            if (!FvxLearnsetPipeline.TryRun(opt, rnd, out var err))
                MessageBox.Show(string.IsNullOrEmpty(err) ? "FVX randomization failed." : err);
            else
                MessageBox.Show("FVX-style randomization applied. Save the ROM to keep changes.");
        }
    }
}
