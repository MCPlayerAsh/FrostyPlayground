using System;
using System.Collections.Generic;
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
        bool _syncingFoeEvolve;
        bool _syncingFoeLevel;
        bool _syncingWildLevel;
        readonly ToolTip _fvxTips = new ToolTip();
        CheckBox wildRandomizeCheck;
        GroupBox wildReplacePerSpeciesGroup;
        RadioButton wildReplaceWholeGame;
        RadioButton wildReplacePerArea;
        RadioButton wildReplacePerMap;
        RadioButton wildReplacePerEncounterSet;
        RadioButton wildReplaceMaximum;
        CheckBox wildSplitByEncounterType;
        GroupBox wildTypeRestrictionsGroup;
        RadioButton wildTypeNone;
        RadioButton wildTypeRandomThemes;
        RadioButton wildTypeKeepPrimary;
        CheckBox wildTypeKeepThemes;
        GroupBox wildEvolutionRestrictionsGroup;
        RadioButton wildEvoNone;
        RadioButton wildEvoBasicOnly;
        RadioButton wildEvoSameStage;
        CheckBox wildEvoKeepRelations;
        CheckBox wildUseTimeBased;
        CheckBox wildNoLegendaries;
        CheckBox wildSetMinimumCatchRate;
        TrackBar wildMinCatchRateLevel;
        CheckBox wildRandomizeHeldItems;
        CheckBox wildBanBadItems;
        CheckBox wildCatchEmAll;
        CheckBox wildSimilarStrength;
        CheckBox wildBalanceLowLevel;
        CheckBox wildAllowAltFormes;
        CheckBox wildLevelModifierEnabled;
        TrackBar wildLevelModifierTrack;
        NumericUpDown wildLevelModifierValue;

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
            _fvxTips.SetToolTip(foeMidAllowNonStandard,
                "Black 2 / White 2: when off, trainer randomization excludes extended personal-table slots after the National Dex (index 652+, e.g. Pokéstar Studios). When on, those entries are included in the same pool as everything else.");
            _fvxTips.SetToolTip(grpFoeBattleTier,
                "Uses Boss / Important / Regular tier detection (same as Tier detection above). Checked tiers always get the selected battle type; other tiers use the Battle style radios.");
            UpdateFoeBattleTierComboEnabled();
            UpdateFoeMiddleColumnEnabled();
            UpdateFoeBattleStyleControls();
            UpdateFoeEvolveControlsEnabled();
            UpdateFoeLevelControlsEnabled();
            UpdateFoeLatestEvoLabel();
            InitializeWildTabUi();
            UpdateWildControlsEnabled();
        }

        /// <summary>Call after ROM/text NARCs are loaded so custom starter combos populate.</summary>
        public void RefreshAfterRomLoaded()
        {
            if (IsDisposed) return;
            PopulateSpeciesCombos();
            PopulateSingleTypeCombo();
            UpdateBwGateUi();
            UpdateIncludeFairyUi();
            UpdateFoeMiddleColumnEnabled();
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
            //   4) Starters / Statics / Trades — sees fresh BSTs/types; must run before foe trainers when BW1 trio gyms follow scripts.
            //   5) Wild Pokémon — updates encounter/catch/item baselines before trainer randomization.
            //   6) Foe Pokémon (trainers) — uses fresh learnsets/types for pools; reads script starters for Striaton themes.
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

            if (!FvxStartersStaticsTradesPipeline.TryRun(opt, rnd, out var startersErr))
            {
                MessageBox.Show(string.IsNullOrEmpty(startersErr) ? "Starters / statics / trades step did not run." : startersErr);
                return;
            }

            var wildOpt = BuildWildPokemonOptionsFromUi();
            if (!FvxWildPokemonPipeline.TryRun(wildOpt, rnd, out var wildErr))
            {
                MessageBox.Show("Wild Pokémon step failed: " + (wildErr ?? ""));
                return;
            }

            var foeOpt = BuildFoePokemonOptionsFromUi();
            if (!FvxFoePokemonPipeline.TryRun(foeOpt, rnd, out var foeErr))
            {
                MessageBox.Show("Foe Pokémon step failed: " + (foeErr ?? ""));
                return;
            }

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

        FvxFoePokemonOptions BuildFoePokemonOptionsFromUi()
        {
            var mode = FvxFoeTrainerPokemonMode.Unchanged;
            switch (comboFoeTrainerPokemonMode.SelectedIndex)
            {
                case 1: mode = FvxFoeTrainerPokemonMode.Random; break;
                case 2: mode = FvxFoeTrainerPokemonMode.Distributed; break;
                case 3: mode = FvxFoeTrainerPokemonMode.MainPlaythrough; break;
                case 4: mode = FvxFoeTrainerPokemonMode.TypeThemed; break;
                case 5: mode = FvxFoeTrainerPokemonMode.TypeThemedElite4Gyms; break;
                case 6: mode = FvxFoeTrainerPokemonMode.KeepThemed; break;
                case 7: mode = FvxFoeTrainerPokemonMode.KeepThemeOrPrimary; break;
            }

            var tierDet = FvxFoeTierDetectionMode.Heuristic;
            if (comboFoeTierDetection.SelectedIndex == 1)
                tierDet = FvxFoeTierDetectionMode.MatchingVanillaUpr;

            var battle = FvxFoeBattleStyleMode.Unchanged;
            if (foeBattleRandomEach.Checked) battle = FvxFoeBattleStyleMode.Random;
            else if (foeBattleRandomGlobal.Checked) battle = FvxFoeBattleStyleMode.RandomGlobal;
            else if (foeBattleSingle.Checked) battle = FvxFoeBattleStyleMode.SingleStyle;
            bool pokemonModeActive = mode != FvxFoeTrainerPokemonMode.Unchanged;

            return new FvxFoePokemonOptions
            {
                IncludeFairyTypes = checkIncludeFairy.Checked,
                TrainerPokemonMode = mode,
                TierDetectionMode = tierDet,
                AdditionalPokemonBoss = foeAddBoss.Checked,
                AdditionalPokemonImportant = foeAddImportant.Checked,
                AdditionalPokemonImportantCount = (int)numFoeAddImportant.Value,
                AdditionalPokemonRegular = foeAddRegular.Checked,
                AdditionalPokemonRegularCount = (int)numFoeAddRegular.Value,
                HeldItemsBoss = foeHeldBoss.Checked,
                HeldItemsImportant = foeHeldImportant.Checked,
                HeldItemsRegular = foeHeldRegular.Checked,
                HeldConsumableOnly = foeHeldConsumable.Checked,
                HeldSensibleItems = foeHeldSensible.Checked,
                HeldHighestLevelOnly = foeHeldHighestLv.Checked,
                DiverseTypesBoss = foeDivBoss.Checked,
                DiverseTypesImportant = foeDivImportant.Checked,
                DiverseTypesRegular = foeDivRegular.Checked,
                BattleStyleMode = battle,
                SingleStyleBattleType = comboFoeSingleBattleType.SelectedIndex >= 0 ? comboFoeSingleBattleType.SelectedIndex : 0,
                UniqueBattleStyleBoss = foeBattleTierBoss.Checked,
                UniqueBattleStyleBossBattleType = comboFoeBattleTierBoss.SelectedIndex >= 0 ? comboFoeBattleTierBoss.SelectedIndex : 0,
                UniqueBattleStyleImportant = foeBattleTierImportant.Checked,
                UniqueBattleStyleImportantBattleType = comboFoeBattleTierImportant.SelectedIndex >= 0 ? comboFoeBattleTierImportant.SelectedIndex : 0,
                UniqueBattleStyleRegular = foeBattleTierRegular.Checked,
                UniqueBattleStyleRegularBattleType = comboFoeBattleTierRegular.SelectedIndex >= 0 ? comboFoeBattleTierRegular.SelectedIndex : 0,
                RivalCarriesStarter = pokemonModeActive && foeMidRivalStarter.Checked,
                SimilarStrength = pokemonModeActive && foeMidSimilar.Checked,
                AvoidDuplicates = pokemonModeActive && foeMidNoDupes.Checked,
                WeightTypesByCount = pokemonModeActive && foeMidWeightTypes.Checked,
                UseLocalPokemon = pokemonModeActive && foeMidLocal.Checked,
                DontUseLegendaries = pokemonModeActive && foeMidNoLegend.Checked,
                NoEarlyWonderGuard = pokemonModeActive && foeMidNoWG.Checked,
                AllowAlternateFormes = pokemonModeActive && foeMidAltForms.Checked,
                AllowNonStandardPokemon = pokemonModeActive && MainEditor.RomType == RomType.BW2 && foeMidAllowNonStandard.Checked,
                LeagueUniquePokemon = pokemonModeActive && foeMidLeagueUnique.Checked,
                LeagueUniqueCount = (int)numFoeLeagueUnique.Value,
                RandomizeTrainerNames = foeRightRandNames.Checked,
                RandomizeTrainerClassNames = foeRightRandClass.Checked,
                TrainersEvolvePokemon = chkFoeTrainersEvolve.Checked,
                TrainersEvolvePercent = trackFoeEvolvePct.Value,
                LevelPercentModifierEnabled = chkFoeLevelPct.Checked,
                LevelPercentModifier = trackFoeLevelPct.Value,
                Bw1TrioGymsMatchStarterTriangle = foeBw1TrioGymsStarters.Checked
            };
        }

        FvxWildPokemonOptions BuildWildPokemonOptionsFromUi()
        {
            var zoneMode = FvxWildReplacementMode.WholeGame;
            if (wildReplacePerArea.Checked) zoneMode = FvxWildReplacementMode.NamedLocation;
            else if (wildReplacePerMap.Checked) zoneMode = FvxWildReplacementMode.PerMap;
            else if (wildReplacePerEncounterSet.Checked) zoneMode = FvxWildReplacementMode.PerEncounterSet;
            else if (wildReplaceMaximum.Checked) zoneMode = FvxWildReplacementMode.MaximumPossible;

            var typeMode = FvxWildTypeRestrictionMode.None;
            if (wildTypeRandomThemes.Checked) typeMode = FvxWildTypeRestrictionMode.RandomZoneThemes;
            else if (wildTypeKeepPrimary.Checked) typeMode = FvxWildTypeRestrictionMode.KeepPrimaryType;

            var evoMode = FvxWildEvolutionRestrictionMode.None;
            if (wildEvoBasicOnly.Checked) evoMode = FvxWildEvolutionRestrictionMode.BasicOnly;
            else if (wildEvoSameStage.Checked) evoMode = FvxWildEvolutionRestrictionMode.SameEvolutionStage;

            return new FvxWildPokemonOptions
            {
                RandomizeWildPokemon = wildRandomizeCheck.Checked,
                ReplacementMode = zoneMode,
                SplitByEncounterType = wildSplitByEncounterType.Checked,
                TypeRestrictionMode = typeMode,
                KeepZoneTypeThemes = wildTypeKeepThemes.Checked,
                EvolutionRestrictionMode = evoMode,
                KeepEvolutionRelations = wildEvoKeepRelations.Checked,
                UseTimeBasedEncounters = wildUseTimeBased.Checked,
                DontUseLegendaries = wildNoLegendaries.Checked,
                SetMinimumCatchRate = wildSetMinimumCatchRate.Checked,
                MinimumCatchRateLevel = wildMinCatchRateLevel.Value,
                RandomizeHeldItems = wildRandomizeHeldItems.Checked,
                BanBadItems = wildBanBadItems.Checked,
                CatchEmAllMode = wildCatchEmAll.Checked,
                SimilarStrength = wildSimilarStrength.Checked,
                BalanceLowLevelEncounters = wildBalanceLowLevel.Checked,
                AllowAlternateFormes = wildAllowAltFormes.Checked,
                LevelModifierEnabled = wildLevelModifierEnabled.Checked,
                LevelModifierPercent = wildLevelModifierTrack.Value
            };
        }

        void FoeTrainerPokemonModeChanged(object sender, EventArgs e) => UpdateFoeMiddleColumnEnabled();

        void FoeMidLeagueUniqueChanged(object sender, EventArgs e) => UpdateFoeMiddleColumnEnabled();

        void UpdateFoeMiddleColumnEnabled()
        {
            foeBw1TrioGymsStarters.Enabled = MainEditor.RomType == RomType.BW1;
            bool on = comboFoeTrainerPokemonMode.SelectedIndex > 0;
            foeMidRivalStarter.Enabled = on;
            foeMidSimilar.Enabled = on;
            foeMidNoDupes.Enabled = on;
            foeMidWeightTypes.Enabled = on;
            foeMidLocal.Enabled = on;
            foeMidNoLegend.Enabled = on;
            foeMidNoWG.Enabled = on;
            foeMidAltForms.Enabled = on;
            foeMidAllowNonStandard.Enabled = on && MainEditor.RomType == RomType.BW2;
            foeMidLeagueUnique.Enabled = on;
            numFoeLeagueUnique.Enabled = on && foeMidLeagueUnique.Checked;
        }

        void FoeBattleStyleChanged(object sender, EventArgs e) => UpdateFoeBattleStyleControls();

        void FoeBattleTierUniqueChanged(object sender, EventArgs e) => UpdateFoeBattleTierComboEnabled();

        void UpdateFoeBattleTierComboEnabled()
        {
            comboFoeBattleTierBoss.Enabled = foeBattleTierBoss.Checked;
            comboFoeBattleTierImportant.Enabled = foeBattleTierImportant.Checked;
            comboFoeBattleTierRegular.Enabled = foeBattleTierRegular.Checked;
        }

        void UpdateFoeBattleStyleControls()
            => comboFoeSingleBattleType.Enabled = foeBattleSingle.Checked;

        void InitializeWildTabUi()
        {
            mainTabs.TabPages.Remove(tabPageMovesMovesets);
            tabPageWildPokemon.Controls.Clear();
            tabPageWildPokemon.Padding = new Padding(8);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(4, 6, 4, 4)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var left = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Margin = new Padding(4, 0, 8, 0) };
            var middle = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Margin = new Padding(4, 0, 8, 0) };
            var right = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Margin = new Padding(4, 0, 4, 0) };

            wildRandomizeCheck = new CheckBox { AutoSize = true, Text = "Randomize Wild Pokemon", Margin = new Padding(0, 4, 0, 6) };
            wildRandomizeCheck.CheckedChanged += WildRandomizeCheckedChanged;
            left.Controls.Add(wildRandomizeCheck);

            wildReplacePerSpeciesGroup = new GroupBox { Text = "Replacements Per Species", Width = 286, Height = 202, Padding = new Padding(8), Margin = new Padding(0) };
            var replaceInner = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            wildReplaceWholeGame = new RadioButton { AutoSize = true, Text = "1 In Whole Game", Checked = true, Margin = new Padding(0, 2, 0, 2) };
            wildReplacePerArea = new RadioButton { AutoSize = true, Text = "1 Per Named Location", Margin = new Padding(0, 2, 0, 2) };
            wildReplacePerMap = new RadioButton { AutoSize = true, Text = "1 Per Map", Margin = new Padding(0, 2, 0, 2) };
            wildReplacePerEncounterSet = new RadioButton { AutoSize = true, Text = "1 Per Encounter Set", Margin = new Padding(0, 2, 0, 2) };
            wildSplitByEncounterType = new CheckBox { AutoSize = true, Text = "Split by encounter types", Margin = new Padding(0, 2, 0, 2) };
            wildReplaceMaximum = new RadioButton { AutoSize = true, Text = "Maximum Possible", Margin = new Padding(0, 2, 0, 2) };
            replaceInner.Controls.Add(wildReplaceWholeGame);
            replaceInner.Controls.Add(wildReplacePerArea);
            replaceInner.Controls.Add(wildReplacePerMap);
            replaceInner.Controls.Add(wildReplacePerEncounterSet);
            replaceInner.Controls.Add(wildSplitByEncounterType);
            replaceInner.Controls.Add(wildReplaceMaximum);
            wildReplacePerSpeciesGroup.Controls.Add(replaceInner);
            left.Controls.Add(wildReplacePerSpeciesGroup);

            wildTypeRestrictionsGroup = new GroupBox { Text = "Type Restrictions", Width = 286, Height = 152, Padding = new Padding(8), Margin = new Padding(0, 4, 0, 4) };
            var typeInner = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            wildTypeNone = new RadioButton { AutoSize = true, Text = "None", Checked = true, Margin = new Padding(0, 2, 0, 2) };
            wildTypeKeepThemes = new CheckBox { AutoSize = true, Text = "Keep Set/Zone Themes", Margin = new Padding(0, 2, 0, 2) };
            wildTypeRandomThemes = new RadioButton { AutoSize = true, Text = "Random Zone Themes", Margin = new Padding(0, 2, 0, 2) };
            wildTypeKeepPrimary = new RadioButton { AutoSize = true, Text = "Keep Primary Type", Margin = new Padding(0, 2, 0, 2) };
            typeInner.Controls.Add(wildTypeNone);
            typeInner.Controls.Add(wildTypeKeepThemes);
            typeInner.Controls.Add(wildTypeRandomThemes);
            typeInner.Controls.Add(wildTypeKeepPrimary);
            wildTypeRestrictionsGroup.Controls.Add(typeInner);
            middle.Controls.Add(wildTypeRestrictionsGroup);

            wildEvolutionRestrictionsGroup = new GroupBox { Text = "Evolution Restrictions", Width = 286, Height = 152, Padding = new Padding(8), Margin = new Padding(0, 6, 0, 0) };
            var evoInner = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            wildEvoNone = new RadioButton { AutoSize = true, Text = "None", Checked = true, Margin = new Padding(0, 2, 0, 2) };
            wildEvoKeepRelations = new CheckBox { AutoSize = true, Text = "Keep Relations", Margin = new Padding(0, 2, 0, 2) };
            wildEvoBasicOnly = new RadioButton { AutoSize = true, Text = "Only Basic Pokemon", Margin = new Padding(0, 2, 0, 2) };
            wildEvoSameStage = new RadioButton { AutoSize = true, Text = "Same Evolution Stage", Margin = new Padding(0, 2, 0, 2) };
            evoInner.Controls.Add(wildEvoNone);
            evoInner.Controls.Add(wildEvoKeepRelations);
            evoInner.Controls.Add(wildEvoBasicOnly);
            evoInner.Controls.Add(wildEvoSameStage);
            wildEvolutionRestrictionsGroup.Controls.Add(evoInner);
            middle.Controls.Add(wildEvolutionRestrictionsGroup);

            wildUseTimeBased = new CheckBox { AutoSize = true, Text = "Use Time Based Encounters", Checked = true, Margin = new Padding(0, 4, 0, 2) };
            wildNoLegendaries = new CheckBox { AutoSize = true, Text = "Don't Use Legendaries", Margin = new Padding(0, 2, 0, 2) };
            wildSetMinimumCatchRate = new CheckBox { AutoSize = true, Text = "Set Minimum Catch Rate:", Margin = new Padding(0, 2, 0, 2) };
            wildSetMinimumCatchRate.CheckedChanged += WildMinCatchRateCheckedChanged;
            wildMinCatchRateLevel = new TrackBar { Minimum = 1, Maximum = 5, TickStyle = TickStyle.BottomRight, TickFrequency = 1, Value = 1, Width = 260, Margin = new Padding(0, 0, 0, 10) };
            wildRandomizeHeldItems = new CheckBox { AutoSize = true, Text = "Randomize Held Items", Margin = new Padding(0, 2, 0, 2) };
            wildRandomizeHeldItems.CheckedChanged += WildHeldItemsCheckedChanged;
            wildBanBadItems = new CheckBox { AutoSize = true, Text = "Ban Bad Items", Margin = new Padding(18, 0, 0, 2) };
            wildCatchEmAll = new CheckBox { AutoSize = true, Text = "Catch Em All Mode", Margin = new Padding(0, 2, 0, 2) };
            wildSimilarStrength = new CheckBox { AutoSize = true, Text = "Similar Strength", Margin = new Padding(0, 2, 0, 2) };
            wildSimilarStrength.CheckedChanged += WildSimilarStrengthCheckedChanged;
            wildBalanceLowLevel = new CheckBox { AutoSize = true, Text = "Balance Low Level Encounters", Margin = new Padding(18, 0, 0, 2) };
            wildAllowAltFormes = new CheckBox { AutoSize = true, Text = "Allow Alternate Formes", Margin = new Padding(0, 2, 0, 2) };
            wildLevelModifierEnabled = new CheckBox { AutoSize = true, Text = "Percentage Level Modifier:", Margin = new Padding(0, 6, 0, 2) };
            wildLevelModifierEnabled.CheckedChanged += WildLevelModifierCheckedChanged;
            wildLevelModifierTrack = new TrackBar { Minimum = -100, Maximum = 150, TickFrequency = 25, Value = 0, Width = 232 };
            wildLevelModifierTrack.ValueChanged += WildLevelTrackValueChanged;
            wildLevelModifierValue = new NumericUpDown { Minimum = -100, Maximum = 150, Value = 0, Width = 60 };
            wildLevelModifierValue.ValueChanged += WildLevelValueChanged;

            var levelFlow = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Margin = new Padding(0, 0, 0, 0) };
            levelFlow.Controls.Add(wildLevelModifierTrack);
            levelFlow.Controls.Add(wildLevelModifierValue);

            right.Controls.Add(wildUseTimeBased);
            right.Controls.Add(wildNoLegendaries);
            right.Controls.Add(wildSetMinimumCatchRate);
            right.Controls.Add(wildMinCatchRateLevel);
            right.Controls.Add(wildRandomizeHeldItems);
            right.Controls.Add(wildBanBadItems);
            right.Controls.Add(wildCatchEmAll);
            right.Controls.Add(wildSimilarStrength);
            right.Controls.Add(wildBalanceLowLevel);
            right.Controls.Add(wildAllowAltFormes);
            right.Controls.Add(wildLevelModifierEnabled);
            right.Controls.Add(levelFlow);

            root.Controls.Add(left, 0, 0);
            root.Controls.Add(middle, 1, 0);
            root.Controls.Add(right, 2, 0);
            tabPageWildPokemon.Controls.Add(root);
        }

        void WildRandomizeCheckedChanged(object sender, EventArgs e) => UpdateWildControlsEnabled();

        void WildSimilarStrengthCheckedChanged(object sender, EventArgs e) => UpdateWildControlsEnabled();

        void WildHeldItemsCheckedChanged(object sender, EventArgs e) => UpdateWildControlsEnabled();

        void WildMinCatchRateCheckedChanged(object sender, EventArgs e) => UpdateWildControlsEnabled();

        void WildLevelModifierCheckedChanged(object sender, EventArgs e) => UpdateWildControlsEnabled();

        void UpdateWildControlsEnabled()
        {
            bool randomizeOn = wildRandomizeCheck.Checked;
            wildReplacePerSpeciesGroup.Enabled = randomizeOn;
            wildTypeRestrictionsGroup.Enabled = randomizeOn;
            wildEvolutionRestrictionsGroup.Enabled = randomizeOn;
            wildSplitByEncounterType.Enabled = randomizeOn;
            wildCatchEmAll.Enabled = randomizeOn;
            wildSimilarStrength.Enabled = randomizeOn;
            wildAllowAltFormes.Enabled = randomizeOn;
            wildNoLegendaries.Enabled = randomizeOn;

            wildBalanceLowLevel.Enabled = randomizeOn && wildSimilarStrength.Checked;
            if (!wildBalanceLowLevel.Enabled) wildBalanceLowLevel.Checked = false;

            bool heldOn = randomizeOn && wildRandomizeHeldItems.Checked;
            wildBanBadItems.Enabled = heldOn;
            if (!heldOn) wildBanBadItems.Checked = false;

            wildMinCatchRateLevel.Enabled = wildSetMinimumCatchRate.Checked;
            wildLevelModifierTrack.Enabled = wildLevelModifierEnabled.Checked;
            wildLevelModifierValue.Enabled = wildLevelModifierEnabled.Checked;
        }

        void WildLevelTrackValueChanged(object sender, EventArgs e)
        {
            if (_syncingWildLevel) return;
            _syncingWildLevel = true;
            try { wildLevelModifierValue.Value = wildLevelModifierTrack.Value; }
            finally { _syncingWildLevel = false; }
        }

        void WildLevelValueChanged(object sender, EventArgs e)
        {
            if (_syncingWildLevel) return;
            _syncingWildLevel = true;
            try
            {
                int v = (int)wildLevelModifierValue.Value;
                if (v < wildLevelModifierTrack.Minimum) v = wildLevelModifierTrack.Minimum;
                if (v > wildLevelModifierTrack.Maximum) v = wildLevelModifierTrack.Maximum;
                if (wildLevelModifierTrack.Value != v) wildLevelModifierTrack.Value = v;
            }
            finally { _syncingWildLevel = false; }
        }

        void FoeEvolveCheckChanged(object sender, EventArgs e)
        {
            UpdateFoeEvolveControlsEnabled();
            UpdateFoeLatestEvoLabel();
        }

        void UpdateFoeEvolveControlsEnabled()
        {
            bool on = chkFoeTrainersEvolve.Checked;
            trackFoeEvolvePct.Enabled = on;
            numFoeEvolvePct.Enabled = on;
        }

        void TrackFoeEvolveValueChanged(object sender, EventArgs e)
        {
            if (_syncingFoeEvolve) return;
            _syncingFoeEvolve = true;
            try
            {
                numFoeEvolvePct.Value = trackFoeEvolvePct.Value;
            }
            finally { _syncingFoeEvolve = false; }
            UpdateFoeLatestEvoLabel();
        }

        void NumFoeEvolveValueChanged(object sender, EventArgs e)
        {
            if (_syncingFoeEvolve) return;
            _syncingFoeEvolve = true;
            try
            {
                int v = (int)numFoeEvolvePct.Value;
                if (v < trackFoeEvolvePct.Minimum) v = trackFoeEvolvePct.Minimum;
                if (v > trackFoeEvolvePct.Maximum) v = trackFoeEvolvePct.Maximum;
                if (trackFoeEvolvePct.Value != v) trackFoeEvolvePct.Value = v;
            }
            finally { _syncingFoeEvolve = false; }
            UpdateFoeLatestEvoLabel();
        }

        void UpdateFoeLatestEvoLabel()
        {
            if (!chkFoeTrainersEvolve.Checked || trackFoeEvolvePct.Value == 0)
                lblFoeLatestEvo.Text = "Latest fully evolved level: —";
            else
                lblFoeLatestEvo.Text = "Latest fully evolved level: (varies by species / level)";
        }

        void FoeLevelPctCheckChanged(object sender, EventArgs e) => UpdateFoeLevelControlsEnabled();

        void UpdateFoeLevelControlsEnabled()
        {
            bool on = chkFoeLevelPct.Checked;
            trackFoeLevelPct.Enabled = on;
            numFoeLevelPct.Enabled = on;
        }

        void TrackFoeLevelValueChanged(object sender, EventArgs e)
        {
            if (_syncingFoeLevel) return;
            _syncingFoeLevel = true;
            try { numFoeLevelPct.Value = trackFoeLevelPct.Value; }
            finally { _syncingFoeLevel = false; }
        }

        void NumFoeLevelValueChanged(object sender, EventArgs e)
        {
            if (_syncingFoeLevel) return;
            _syncingFoeLevel = true;
            try
            {
                int v = (int)numFoeLevelPct.Value;
                if (v < trackFoeLevelPct.Minimum) v = trackFoeLevelPct.Minimum;
                if (v > trackFoeLevelPct.Maximum) v = trackFoeLevelPct.Maximum;
                if (trackFoeLevelPct.Value != v) trackFoeLevelPct.Value = v;
            }
            finally { _syncingFoeLevel = false; }
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
