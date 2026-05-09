using System;
using System.Linq;
using System.Windows.Forms;
using NewEditor.Data;
using NewEditor.Data.Randomization.FvxGen5;
using NewEditor.Data.Randomization.GeneShuffle;

namespace NewEditor.Forms
{
    public partial class FvxRandomizerForm : Form
    {
        bool _syncingStaticsLevel;
        readonly ToolTip _fvxTips = new ToolTip();

        public FvxRandomizerForm()
        {
            InitializeComponent();
            mainTabs.Alignment = TabAlignment.Bottom;
            checkTradeOt.Enabled = false;
            checkTradeOt.Checked = false;
            _fvxTips.SetToolTip(checkTradeOt, "Not implemented: OT lives in mixed script/text layers and unsafe to patch blindly on BW/BW2.");
            _fvxTips.SetToolTip(checkStarterGraphics,
                "Updates starter ball graphics, starter cries, starter-location story text, and the Pokédex-registration script (BW1/BW2 US-aligned offsets).");
            UpdateStarterCustomEnabled();
            UpdateSingleTypeEnabled();
            UpdateStaticsLevelControlsEnabled();
            UpdateBwGateUi();
            UpdateIncludeFairyUi();
            if (MainEditor.RomType == RomType.BW1)
                geneShuffleControl.SetTutorEnabled(false, "Move tutor compatibility (BW1 — not applicable)");
        }

        /// <summary>Call after ROM/text NARCs are loaded so custom starter combos populate.</summary>
        public void RefreshAfterRomLoaded()
        {
            if (IsDisposed) return;
            PopulateSpeciesCombos();
            PopulateSingleTypeCombo();
            UpdateBwGateUi();
            UpdateIncludeFairyUi();
        }

        void FvxRandomizerForm_Shown(object sender, EventArgs e)
        {
            PopulateSpeciesCombos();
            PopulateSingleTypeCombo();
            UpdateBwGateUi();
            UpdateIncludeFairyUi();
        }

        /// <summary>White 1 cannot use Fairy Vpatch — mirror Gene Shuffle.</summary>
        void UpdateIncludeFairyUi()
        {
            if (MainEditor.RomTypeId == "pokemon w")
            {
                checkIncludeFairy.Enabled = false;
                checkIncludeFairy.Checked = false;
                checkIncludeFairy.Text = "Include Fairy-type (not available on White 1)";
                _fvxTips.SetToolTip(checkIncludeFairy,
                    "Fairy Vpatch is not supported on Pokémon White 1.");
            }
            else
            {
                checkIncludeFairy.Enabled = true;
                checkIncludeFairy.Text = "Include Fairy-type (applies Fairy Vpatch when needed)";
                _fvxTips.SetToolTip(checkIncludeFairy,
                    "Match this to Gene Shuffle’s “Include Fairy-Types” when you used both tools on the same ROM, so type bounds and the battle chart (17 vs 18 types) stay aligned.");
            }
        }

        void UpdateBwGateUi()
        {
            bool ok = MainEditor.RomType == RomType.BW1 || MainEditor.RomType == RomType.BW2;
            applyButton.Enabled = ok && MainEditor.fileSystem != null;
            if (!ok)
                bwGateLabel.Text = "Starters / statics / trades randomizer targets Gen 5 (BW / BW2) only.";
            else if (MainEditor.fileSystem == null)
                bwGateLabel.Text = "Load a ROM first.";
            else
                bwGateLabel.Text = "";
        }

        void PopulateSpeciesCombos()
        {
            var names = MainEditor.textNarc?.textFiles?[VersionConstants.PokemonNameTextFileID]?.text;
            var combos = new[] { comboCustomStarter0, comboCustomStarter1, comboCustomStarter2 };
            if (names == null || names.Count == 0)
            {
                foreach (var c in combos)
                {
                    c.Items.Clear();
                    c.Enabled = false;
                }
                return;
            }
            object[] arr = names.Cast<object>().ToArray();
            foreach (var c in combos)
            {
                c.Items.Clear();
                c.Items.AddRange(arr);
            }
            int[] defaults = { 495, 498, 501 };
            for (int i = 0; i < 3; i++)
            {
                var c = combos[i];
                int want = defaults[i];
                if (want < c.Items.Count)
                    c.SelectedIndex = want;
                else if (c.Items.Count > 1)
                    c.SelectedIndex = 1;
            }
            UpdateStarterCustomEnabled();
        }

        void PopulateSingleTypeCombo()
        {
            comboSingleType.Items.Clear();
            comboSingleType.Items.Add("Random");
            var typeNames = MainEditor.textNarc?.textFiles?[VersionConstants.TypeNameTextFileID]?.text;
            if (typeNames != null)
            {
                foreach (var t in typeNames)
                    comboSingleType.Items.Add(t);
            }
            if (comboSingleType.Items.Count > 0)
                comboSingleType.SelectedIndex = 0;
        }

        void ApplyClick(object sender, EventArgs e)
        {
            if (MainEditor.RomType != RomType.BW1 && MainEditor.RomType != RomType.BW2)
            {
                MessageBox.Show("This randomizer supports Black/White and Black 2/White 2 only.");
                return;
            }
            if (MainEditor.fileSystem == null)
            {
                MessageBox.Show("Load a ROM first.");
                return;
            }

            int seed = (int)seedNumeric.Value;
            var rnd = seed != 0 ? new Random(seed) : new Random();
            var opt = BuildOptionsFromUi();

            // Order matters:
            //   1) Fairy + Types (Gene Shuffle tab) so any "Same Typing" / type-aware downstream filters see new types.
            //   2) Pokemon Traits — base stats / abilities / evolutions / EXP curves.
            //   3) FVX learnsets / TMs / tutors (Gene Shuffle tab) — uses fresh types.
            //   4) Starters / Statics / Trades — sees fresh BSTs and types.
            if (!RunGeneShuffleStep(opt.IncludeFairyTypes, rnd, out var geneErr))
            {
                MessageBox.Show("Gene Shuffle step failed: " + (geneErr ?? ""));
                return;
            }

            var traitOpt = traitsControl.BuildOptions();
            if (!FvxPokemonTraitsPipeline.TryRun(traitOpt, rnd, out var traitErr))
            {
                MessageBox.Show("Pokemon Traits step failed: " + (traitErr ?? ""));
                return;
            }

            if (!RunGeneShuffleLearnsetStep(rnd, out var learnErr))
            {
                MessageBox.Show("Gene Shuffle learnset step failed: " + (learnErr ?? ""));
                return;
            }

            if (!FvxStartersStaticsTradesPipeline.TryRun(opt, rnd, out var err))
                MessageBox.Show(string.IsNullOrEmpty(err) ? "Randomization did not run." : err);
            else
                MessageBox.Show("FVX-style settings applied. Save the ROM to keep changes.");
        }

        /// <summary>
        /// Apply the Gene Shuffle Type-randomization step, sharing the Fairy patch state with the
        /// Starters/Statics/Trades pipeline that the same Apply click runs.
        /// </summary>
        bool RunGeneShuffleStep(bool includeFairy, Random rnd, out string error)
        {
            error = null;
            if (MainEditor.pokemonDataNarc == null || MainEditor.evolutionsNarc?.evolutions == null)
                return true; // Gene Shuffle requires evolution data; if missing, skip silently (Starters tab will gate).

            if (!GeneShuffleFairyPatch.TryPrepare(includeFairy, out var patchApplied, out var fairyErr))
            {
                error = fairyErr ?? "Fairy patch step failed.";
                return false;
            }
            int maxType = GeneShuffleFairyPatch.MaxTypeInclusive(includeFairy, patchApplied);
            try
            {
                TypeGeneRandomizer.Apply(geneShuffleControl.TypeMode, rnd, maxType,
                    MainEditor.pokemonDataNarc.pokemon, MainEditor.evolutionsNarc.evolutions);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        bool RunGeneShuffleLearnsetStep(Random rnd, out string error)
        {
            error = null;
            if (!geneShuffleControl.AnyOptionsActive) return true;
            var opt = geneShuffleControl.BuildOptions();
            return FvxLearnsetPipeline.TryRun(opt, rnd, out error);
        }

        FvxStartersStaticsTradesOptions BuildOptionsFromUi()
        {
            byte? singleType = null;
            if (typeRestrictSingle.Checked && comboSingleType.SelectedIndex > 0)
                singleType = (byte)(comboSingleType.SelectedIndex - 1);

            return new FvxStartersStaticsTradesOptions
            {
                IncludeFairyTypes = checkIncludeFairy.Checked,
                StarterSelectionMode = GetStarterMode(),
                CustomStarterSpeciesIndex0 = comboCustomStarter0.SelectedIndex >= 0 ? comboCustomStarter0.SelectedIndex : 0,
                CustomStarterSpeciesIndex1 = comboCustomStarter1.SelectedIndex >= 0 ? comboCustomStarter1.SelectedIndex : 0,
                CustomStarterSpeciesIndex2 = comboCustomStarter2.SelectedIndex >= 0 ? comboCustomStarter2.SelectedIndex : 0,
                StarterTypeRestriction = GetTypeRestriction(),
                SinglePrimaryTypeId = singleType,
                NoDualTypes = checkNoDualTypes.Checked,
                DontUseLegendaries = checkNoLegendaries.Checked,
                LimitBstMin = checkBstMin.Checked,
                BstMinimum = (int)numericBstMin.Value,
                LimitBstMax = checkBstMax.Checked,
                BstMaximum = (int)numericBstMax.Value,
                StaticsMode = GetStaticsMode(),
                StaticsRandomize600PlusBst = checkStatics600.Checked,
                StaticsLimitMainGameLegendaries = checkStaticsLegendLimit.Checked,
                StaticsUseLevelPercentModifier = checkStaticsLevelPct.Checked,
                StaticsLevelPercentModifier = trackStaticsLevelPct.Value,
                TradesMode = GetTradesMode(),
                TradesRandomizeNicknames = checkTradeNick.Checked,
                TradesRandomizeOts = false,
                TradesRandomizeIvs = checkTradeIv.Checked,
                TradesRandomizeItems = checkTradeItem.Checked,
                StarterUpdateGraphicsAndCries = checkStarterGraphics.Checked
            };
        }

        FvxStarterSelectionMode GetStarterMode()
        {
            if (starterModeCustom.Checked) return FvxStarterSelectionMode.Custom;
            if (starterModeRandomFull.Checked) return FvxStarterSelectionMode.RandomCompletely;
            if (starterModeRandomBasic3.Checked) return FvxStarterSelectionMode.RandomBasicThreeStageLine;
            if (starterModeRandomAnyBasic.Checked) return FvxStarterSelectionMode.RandomAnyBasic;
            return FvxStarterSelectionMode.Unchanged;
        }

        FvxStarterTypeRestriction GetTypeRestriction()
        {
            if (typeRestrictFwg.Checked) return FvxStarterTypeRestriction.FireWaterGrass;
            if (typeRestrictTriangle.Checked) return FvxStarterTypeRestriction.AnyTypeTriangle;
            if (typeRestrictUnique.Checked) return FvxStarterTypeRestriction.UniquePrimaryType;
            if (typeRestrictSingle.Checked) return FvxStarterTypeRestriction.SingleType;
            return FvxStarterTypeRestriction.None;
        }

        FvxStaticsRandomizationMode GetStaticsMode()
        {
            if (statModeSwap.Checked) return FvxStaticsRandomizationMode.SwapLegendariesAndStandards;
            if (statModeRandomFull.Checked) return FvxStaticsRandomizationMode.RandomCompletely;
            if (statModeSimilar.Checked) return FvxStaticsRandomizationMode.RandomSimilarStrength;
            return FvxStaticsRandomizationMode.Unchanged;
        }

        FvxTradesRandomizationMode GetTradesMode()
        {
            if (tradeModeGiven.Checked) return FvxTradesRandomizationMode.RandomizeGivenOnly;
            if (tradeModeBoth.Checked) return FvxTradesRandomizationMode.RandomizeBoth;
            return FvxTradesRandomizationMode.Unchanged;
        }

        void StarterModeChanged(object sender, EventArgs e) => UpdateStarterCustomEnabled();

        void UpdateStarterCustomEnabled()
        {
            bool custom = starterModeCustom.Checked;
            comboCustomStarter0.Enabled = custom;
            comboCustomStarter1.Enabled = custom;
            comboCustomStarter2.Enabled = custom;
        }

        void TypeRestrictionChanged(object sender, EventArgs e) => UpdateSingleTypeEnabled();

        void UpdateSingleTypeEnabled()
        {
            comboSingleType.Enabled = typeRestrictSingle.Checked;
        }

        void StaticsLevelCheckChanged(object sender, EventArgs e) => UpdateStaticsLevelControlsEnabled();

        void UpdateStaticsLevelControlsEnabled()
        {
            bool on = checkStaticsLevelPct.Checked;
            trackStaticsLevelPct.Enabled = on;
            numericStaticsLevelPct.Enabled = on;
        }

        void TrackStaticsLevelValueChanged(object sender, EventArgs e)
        {
            if (_syncingStaticsLevel) return;
            _syncingStaticsLevel = true;
            try
            {
                numericStaticsLevelPct.Value = trackStaticsLevelPct.Value;
            }
            finally
            {
                _syncingStaticsLevel = false;
            }
        }

        void NumericStaticsLevelValueChanged(object sender, EventArgs e)
        {
            if (_syncingStaticsLevel) return;
            _syncingStaticsLevel = true;
            try
            {
                int v = (int)numericStaticsLevelPct.Value;
                if (v < trackStaticsLevelPct.Minimum) v = trackStaticsLevelPct.Minimum;
                if (v > trackStaticsLevelPct.Maximum) v = trackStaticsLevelPct.Maximum;
                if (trackStaticsLevelPct.Value != v)
                    trackStaticsLevelPct.Value = v;
            }
            finally
            {
                _syncingStaticsLevel = false;
            }
        }
    }
}
