using System.Windows.Forms;
using NewEditor.Data.Randomization.FvxGen5;
using NewEditor.Data.Randomization.GeneShuffle;

namespace NewEditor.Forms
{
    /// <summary>
    /// Shared UI for Gene Shuffle (Types + FVX learnsets + TM/HM + Tutors).
    /// Hosted by both <see cref="GeneShuffleForm"/> and the "Gene Shuffle" tab in
    /// <see cref="FvxRandomizerForm"/>. Does NOT include the Fairy checkbox; the host form owns that.
    /// </summary>
    public partial class GeneShuffleControl : UserControl
    {
        readonly TmHmTutorsControl tmHmTutorsControl;

        public GeneShuffleControl()
        {
            InitializeComponent();
            tmHmTutorsControl = new TmHmTutorsControl { Dock = DockStyle.Fill };
            rootTable.Controls.Add(tmHmTutorsControl, 0, 2);
            rootTable.SetRowSpan(tmHmTutorsControl, 2);
            tmHmGroup.Visible = false;
            tutorGroup.Visible = false;
        }

        public GeneShuffleTypeMode TypeMode => (GeneShuffleTypeMode)typeModeCombo.SelectedIndex;

        /// <summary>Disable the tutor controls when the loaded ROM is BW1 (no tutors).</summary>
        public void SetTutorEnabled(bool enabled, string disabledReason = null)
        {
            tmHmTutorsControl.SetTutorEnabled(enabled, disabledReason);
        }

        public FvxRandomizerOptions BuildOptions()
        {
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
            return opt;
        }

        public void ApplyLearnsetOptions(GeneShuffleTypeMode typeMode, FvxRandomizerOptions learn)
        {
            int tm = (int)typeMode;
            if (tm >= 0 && tm < typeModeCombo.Items.Count) typeModeCombo.SelectedIndex = tm;
            if (learn == null) return;
            int m = (int)learn.MovesetsMod;
            if (m >= 0 && m < movesetModCombo.Items.Count) movesetModCombo.SelectedIndex = m;
            blockBrokenMovesCheck.Checked = learn.BlockBrokenMovesetMoves;
            guaranteedStartCheck.Checked = learn.StartWithGuaranteedMoves;
            int g = learn.GuaranteedMoveCount;
            if (g < guaranteedCountNumeric.Minimum) g = (int)guaranteedCountNumeric.Minimum;
            if (g > guaranteedCountNumeric.Maximum) g = (int)guaranteedCountNumeric.Maximum;
            guaranteedCountNumeric.Value = g;
            reorderDamagingCheck.Checked = learn.ReorderDamagingMoves;
            forceGoodDamagingCheck.Checked = learn.MovesetsForceGoodDamaging;
            int p = learn.MovesetsGoodDamagingPercent;
            if (p < goodDamagingPercentNumeric.Minimum) p = (int)goodDamagingPercentNumeric.Minimum;
            if (p > goodDamagingPercentNumeric.Maximum) p = (int)goodDamagingPercentNumeric.Maximum;
            goodDamagingPercentNumeric.Value = p;
            evoMoveAllCheck.Checked = learn.EvolutionMovesForAll;
            randomizeEggCheck.Checked = learn.RandomizeEggMoves;
            tmHmTutorsControl.LoadFromOptions(learn);
        }

        /// <summary>True when the user actually picked something other than "Unchanged" anywhere.</summary>
        public bool AnyOptionsActive
        {
            get
            {
                if (movesetModCombo.SelectedIndex != 0) return true;
                if (tmHmTutorsControl.AnyOptionsActive) return true;
                return false;
            }
        }
    }
}
