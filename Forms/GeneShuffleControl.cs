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
        public GeneShuffleControl()
        {
            InitializeComponent();
        }

        public GeneShuffleTypeMode TypeMode => (GeneShuffleTypeMode)typeModeCombo.SelectedIndex;

        /// <summary>Disable the tutor controls when the loaded ROM is BW1 (no tutors).</summary>
        public void SetTutorEnabled(bool enabled, string disabledReason = null)
        {
            tutorGroup.Enabled = enabled;
            if (!enabled && !string.IsNullOrEmpty(disabledReason))
                tutorGroup.Text = disabledReason;
        }

        public FvxRandomizerOptions BuildOptions()
        {
            return new FvxRandomizerOptions
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
        }

        /// <summary>True when the user actually picked something other than "Unchanged" anywhere.</summary>
        public bool AnyOptionsActive
        {
            get
            {
                if (movesetModCombo.SelectedIndex != 0) return true;
                if (tmHmModCombo.SelectedIndex != 0) return true;
                if (tutorGroup.Enabled && tutorModCombo.SelectedIndex != 0) return true;
                return false;
            }
        }
    }
}
