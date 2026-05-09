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
