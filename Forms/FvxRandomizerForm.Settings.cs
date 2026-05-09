using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NewEditor;
using NewEditor.Data;
using NewEditor.Data.Randomization.FvxGen5;
using NewEditor.Data.Randomization.GeneShuffle;

namespace NewEditor.Forms
{
    partial class FvxRandomizerForm
    {
        readonly HashSet<int> _limitSpeciesAllowlist = new HashSet<int>();

        Label _lblRomDisplay;
        Label _lblRomTypeId;
        Label _lblRomSupport;
        CheckBox _setLimitPokemon;
        Button _setLimitConfigure;
        CheckBox _setBanIrregular;
        CheckBox _setBanPremature;
        CheckBox _setRandomizeIntro;
        CheckBox _setRaceMode;
        Button _setCustomNames;
        Button _setLoadJson;
        Button _setSaveJson;
        CheckBox _batchEnabled;
        NumericUpDown _batchCount;
        NumericUpDown _batchStartIndex;
        TextBox _batchPrefix;
        TextBox _batchOutDir;
        Button _batchBrowseDir;
        CheckBox _batchLogs;
        CheckBox _batchAdvanceIndex;

        void InitializeSettingsTabUi()
        {
            tabPageSettings.Controls.Clear();
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(4)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            var left = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(4, 0, 8, 0)
            };
            var right = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(4, 0, 4, 0)
            };

            var romBox = new GroupBox { Text = "ROM Information", Width = 420, Height = 110, Padding = new Padding(8) };
            var romFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            _lblRomDisplay = new Label { AutoSize = true, Text = "—", Margin = new Padding(0, 4, 0, 2) };
            _lblRomTypeId = new Label { AutoSize = true, Text = "—", Margin = new Padding(0, 2, 0, 2) };
            _lblRomSupport = new Label { AutoSize = true, Text = "—", Margin = new Padding(0, 2, 0, 2) };
            romFlow.Controls.Add(_lblRomDisplay);
            romFlow.Controls.Add(_lblRomTypeId);
            romFlow.Controls.Add(_lblRomSupport);
            romBox.Controls.Add(romFlow);
            left.Controls.Add(romBox);

            var jsonFlow = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
            _setLoadJson = new Button { Text = "Load settings (JSON)", AutoSize = true, Margin = new Padding(0, 0, 8, 0) };
            _setSaveJson = new Button { Text = "Save settings (JSON)", AutoSize = true };
            _setLoadJson.Click += SettingsLoadJsonClick;
            _setSaveJson.Click += SettingsSaveJsonClick;
            jsonFlow.Controls.Add(_setLoadJson);
            jsonFlow.Controls.Add(_setSaveJson);
            left.Controls.Add(jsonFlow);

            // Fixed height + Fill (no AutoSize on inner flow): Dock.Fill + AutoSize together collapses GroupBox height and clips rows.
            var genBox = new GroupBox { Text = "General Options", Width = 420, Height = 220, Padding = new Padding(8), Margin = new Padding(0, 8, 0, 0) };
            var genFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            _setLimitPokemon = new CheckBox { AutoSize = true, Text = "Limit Pokémon", Margin = new Padding(0, 4, 0, 2) };
            _setLimitConfigure = new Button { Text = "Configure…", AutoSize = true, Margin = new Padding(18, 0, 0, 4) };
            _setLimitPokemon.CheckedChanged += (_, __) => _setLimitConfigure.Enabled = _setLimitPokemon.Checked;
            _setLimitConfigure.Click += SettingsLimitConfigureClick;
            _setBanIrregular = new CheckBox { AutoSize = true, Text = "No irregular alt formes", Margin = new Padding(0, 2, 0, 2) };
            _setBanPremature = new CheckBox { AutoSize = true, Text = "No premature evolutions", Margin = new Padding(0, 2, 0, 2) };
            _setRandomizeIntro = new CheckBox { AutoSize = true, Text = "Randomize intro Pokémon", Margin = new Padding(0, 2, 0, 2) };
            _setRaceMode = new CheckBox { AutoSize = true, Text = "Race mode (checksum popup)", Margin = new Padding(0, 2, 0, 2) };
            genFlow.Controls.Add(_setLimitPokemon);
            genFlow.Controls.Add(_setLimitConfigure);
            genFlow.Controls.Add(_setBanIrregular);
            genFlow.Controls.Add(_setBanPremature);
            genFlow.Controls.Add(_setRandomizeIntro);
            genFlow.Controls.Add(_setRaceMode);
            genBox.Controls.Add(genFlow);
            left.Controls.Add(genBox);

            _setCustomNames = new Button { Text = "Custom Names Editor…", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
            _setCustomNames.Click += (_, __) =>
            {
                using (var d = new FvxCustomNamesEditorForm()) d.ShowDialog(this);
            };
            left.Controls.Add(_setCustomNames);

            var batchBox = new GroupBox { Text = "Batch Randomization", Width = 420, Height = 340, Padding = new Padding(8), Margin = new Padding(0, 8, 0, 0) };
            var batchFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            _batchEnabled = new CheckBox { AutoSize = true, Text = "Enable batch output", Margin = new Padding(0, 4, 0, 4) };
            _batchCount = new NumericUpDown { Minimum = 1, Maximum = 500, Value = 3, Width = 60, Margin = new Padding(0, 0, 0, 4) };
            _batchStartIndex = new NumericUpDown { Minimum = 1, Maximum = 99999, Value = 1, Width = 60, Margin = new Padding(0, 0, 0, 4) };
            _batchPrefix = new TextBox { Width = 200, Text = "random_", Margin = new Padding(0, 0, 0, 4) };
            _batchOutDir = new TextBox { Width = 360, Margin = new Padding(0, 0, 0, 4) };
            _batchBrowseDir = new Button { Text = "Output folder…", AutoSize = true, Margin = new Padding(0, 0, 0, 4) };
            _batchBrowseDir.Click += (_, __) =>
            {
                using (var d = new FolderBrowserDialog())
                {
                    if (d.ShowDialog(this) == DialogResult.OK) _batchOutDir.Text = d.SelectedPath;
                }
            };
            _batchLogs = new CheckBox { AutoSize = true, Text = "Generate log files (seed, checksum, preset JSON)", Margin = new Padding(0, 4, 0, 2) };
            _batchAdvanceIndex = new CheckBox { AutoSize = true, Text = "Auto-advance starting index", Margin = new Padding(0, 2, 0, 2) };
            batchFlow.Controls.Add(_batchEnabled);
            batchFlow.Controls.Add(new Label { AutoSize = true, Text = "Number of ROMs:", Margin = new Padding(0, 4, 8, 0) });
            batchFlow.Controls.Add(_batchCount);
            batchFlow.Controls.Add(new Label { AutoSize = true, Text = "Starting index:", Margin = new Padding(0, 4, 8, 0) });
            batchFlow.Controls.Add(_batchStartIndex);
            batchFlow.Controls.Add(new Label { AutoSize = true, Text = "Filename prefix:", Margin = new Padding(0, 8, 0, 0) });
            batchFlow.Controls.Add(_batchPrefix);
            batchFlow.Controls.Add(new Label { AutoSize = true, Text = "Output directory:", Margin = new Padding(0, 8, 0, 0) });
            var dirRow = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
            dirRow.Controls.Add(_batchOutDir);
            dirRow.Controls.Add(_batchBrowseDir);
            batchFlow.Controls.Add(dirRow);
            batchFlow.Controls.Add(_batchLogs);
            batchFlow.Controls.Add(_batchAdvanceIndex);
            batchBox.Controls.Add(batchFlow);
            right.Controls.Add(batchBox);

            root.Controls.Add(left, 0, 0);
            root.Controls.Add(right, 1, 0);
            tabPageSettings.Controls.Add(root);

            try
            {
                _batchStartIndex.Value = FvxBatchIndexPrefs.Read();
            }
            catch { /* ignore */ }

            RefreshSettingsRomLabels();

            _fvxTips.SetToolTip(_batchLogs,
                "When batch output is enabled, writes a .log next to each ROM: iteration seed, race checksum (if race mode is on), rom type, and the full settings preset as JSON.");
            _fvxTips.SetToolTip(_setRaceMode,
                "After a successful Apply, shows a checksum of the built ROM for racing. Batch mode can log the same checksum per output file when logs are enabled.");
        }

        void RefreshSettingsRomLabels()
        {
            if (_lblRomDisplay == null) return;
            string path = MainEditor.Instance?.loadedRomPath ?? "";
            string name = path.Length > 0 && File.Exists(path)
                ? Path.GetFileName(path)
                : "(no file)";
            _lblRomDisplay.Text = "ROM: " + name;
            _lblRomTypeId.Text = "Internal id: " + (string.IsNullOrEmpty(MainEditor.RomTypeId) ? "—" : MainEditor.RomTypeId);
            bool ok = MainEditor.RomType == RomType.BW1 || MainEditor.RomType == RomType.BW2;
            _lblRomSupport.Text = ok ? "Support: Complete (Gen 5)" : "Support: Unsupported (use BW / BW2)";
        }

        FvxRandomizerGlobalOptions BuildGlobalOptionsFromSettings()
        {
            var g = FvxRandomizerGlobalOptions.Disabled();
            g.LimitPokemon = _setLimitPokemon.Checked;
            g.AllowedSpecies = new HashSet<int>(_limitSpeciesAllowlist);
            g.BanIrregularAltFormes = _setBanIrregular.Checked;
            g.BanPrematureEvos = _setBanPremature.Checked;
            g.RandomizeIntroMon = _setRandomizeIntro.Checked;
            g.RaceMode = _setRaceMode.Checked;
            return g;
        }

        void SettingsLimitConfigureClick(object sender, EventArgs e)
        {
            using (var d = new FvxLimitPokemonForm(_limitSpeciesAllowlist))
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                _limitSpeciesAllowlist.Clear();
                foreach (var x in d.SelectedSpeciesIndices) _limitSpeciesAllowlist.Add(x);
            }
        }

        void SettingsLoadJsonClick(object sender, EventArgs e)
        {
            using (var d = new OpenFileDialog { Filter = "JSON|*.json" })
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var json = File.ReadAllText(d.FileName);
                    var cfg = FvxRandomizerConfigIO.FromJson(json);
                    if (cfg == null || cfg.SchemaVersion != 1)
                    {
                        MessageBox.Show("Invalid config file or unsupported schema version.");
                        return;
                    }
                    ApplyConfigToUi(cfg);
                    MessageBox.Show("Settings loaded.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not load JSON: " + ex.Message);
                }
            }
        }

        void SettingsSaveJsonClick(object sender, EventArgs e)
        {
            using (var d = new SaveFileDialog { Filter = "JSON|*.json" })
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    File.WriteAllText(d.FileName, FvxRandomizerConfigIO.ToJson(CollectConfigFromUi()));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Save failed: " + ex.Message);
                }
            }
        }

        FvxRandomizerConfigFile CollectConfigFromUi()
        {
            var g = new FvxRandomizerSettingsGeneral
            {
                LimitPokemon = _setLimitPokemon.Checked,
                AllowedSpecies = _limitSpeciesAllowlist.OrderBy(x => x).ToArray(),
                BanIrregularAltFormes = _setBanIrregular.Checked,
                BanPrematureEvos = _setBanPremature.Checked,
                RandomizeIntroMon = _setRandomizeIntro.Checked,
                RaceMode = _setRaceMode.Checked
            };
            var b = new FvxRandomizerBatchSettings
            {
                Enabled = _batchEnabled.Checked,
                Count = (int)_batchCount.Value,
                StartingIndex = (int)_batchStartIndex.Value,
                FileNamePrefix = _batchPrefix.Text ?? "",
                OutputDirectory = _batchOutDir.Text ?? "",
                GenerateLogs = _batchLogs.Checked,
                AutoAdvanceStartingIndex = _batchAdvanceIndex.Checked
            };

            var foe = BuildFoePokemonOptionsFromUi();
            var wild = BuildWildPokemonOptionsFromUi();
            var sst = BuildOptionsFromUi();
            foe.Global = null;
            wild.Global = null;
            sst.Global = null;

            return new FvxRandomizerConfigFile
            {
                SchemaVersion = 1,
                Seed = (int)seedNumeric.Value,
                IncludeFairyTypes = checkIncludeFairy.Checked,
                Traits = traitsControl.BuildOptions(),
                GeneShuffleTypeMode = (int)geneShuffleControl.TypeMode,
                GeneLearn = geneShuffleControl.BuildOptions(),
                Starters = sst,
                Wild = wild,
                Foe = foe,
                Items = BuildItemsOptionsFromUi(),
                Misc = BuildMiscTweaksOptionsFromUi(),
                General = g,
                Batch = b
            };
        }

        void ApplyConfigToUi(FvxRandomizerConfigFile c)
        {
            _applyingJsonPreset = true;
            try
            {
                seedNumeric.Value = c.Seed;
                checkIncludeFairy.Checked = c.IncludeFairyTypes;
                traitsControl.ApplyFromOptions(c.Traits);
                int gtm = c.GeneShuffleTypeMode;
                if (gtm < 0 || gtm > 2) gtm = 0;
                geneShuffleControl.ApplyLearnsetOptions((GeneShuffleTypeMode)gtm, c.GeneLearn);
                ApplyStartersOptionsToUi(c.Starters);
                ApplyWildOptionsToUi(c.Wild);
                ApplyFoeOptionsToUi(c.Foe);
                ApplyItemsOptionsToUi(c.Items);
                ApplyMiscOptionsToUi(c.Misc);

                _setLimitPokemon.Checked = c.General.LimitPokemon;
                _limitSpeciesAllowlist.Clear();
                if (c.General.AllowedSpecies != null)
                    foreach (var x in c.General.AllowedSpecies) _limitSpeciesAllowlist.Add(x);
                _setBanIrregular.Checked = c.General.BanIrregularAltFormes;
                _setBanPremature.Checked = c.General.BanPrematureEvos;
                _setRandomizeIntro.Checked = c.General.RandomizeIntroMon;
                _setRaceMode.Checked = c.General.RaceMode;

                _batchEnabled.Checked = c.Batch.Enabled;
                _batchCount.Value = Math.Max(_batchCount.Minimum, Math.Min(_batchCount.Maximum, c.Batch.Count));
                _batchStartIndex.Value = Math.Max(_batchStartIndex.Minimum, Math.Min(_batchStartIndex.Maximum, c.Batch.StartingIndex));
                _batchPrefix.Text = c.Batch.FileNamePrefix ?? "";
                _batchOutDir.Text = c.Batch.OutputDirectory ?? "";
                _batchLogs.Checked = c.Batch.GenerateLogs;
                _batchAdvanceIndex.Checked = c.Batch.AutoAdvanceStartingIndex;
            }
            finally
            {
                _applyingJsonPreset = false;
            }

            RefreshUiAfterSettingsPresetApplied();
        }

        void ApplyStartersOptionsToUi(FvxStartersStaticsTradesOptions o)
        {
            switch (o.StarterSelectionMode)
            {
                case FvxStarterSelectionMode.Custom: starterModeCustom.Checked = true; break;
                case FvxStarterSelectionMode.RandomCompletely: starterModeRandomFull.Checked = true; break;
                case FvxStarterSelectionMode.RandomBasicThreeStageLine: starterModeRandomBasic3.Checked = true; break;
                case FvxStarterSelectionMode.RandomAnyBasic: starterModeRandomAnyBasic.Checked = true; break;
                default: starterModeUnchanged.Checked = true; break;
            }
            if (o.CustomStarterSpeciesIndex0 < comboCustomStarter0.Items.Count) comboCustomStarter0.SelectedIndex = o.CustomStarterSpeciesIndex0;
            if (o.CustomStarterSpeciesIndex1 < comboCustomStarter1.Items.Count) comboCustomStarter1.SelectedIndex = o.CustomStarterSpeciesIndex1;
            if (o.CustomStarterSpeciesIndex2 < comboCustomStarter2.Items.Count) comboCustomStarter2.SelectedIndex = o.CustomStarterSpeciesIndex2;

            switch (o.StarterTypeRestriction)
            {
                case FvxStarterTypeRestriction.FireWaterGrass: typeRestrictFwg.Checked = true; break;
                case FvxStarterTypeRestriction.AnyTypeTriangle: typeRestrictTriangle.Checked = true; break;
                case FvxStarterTypeRestriction.UniquePrimaryType: typeRestrictUnique.Checked = true; break;
                case FvxStarterTypeRestriction.SingleType: typeRestrictSingle.Checked = true; break;
                default: typeRestrictNone.Checked = true; break;
            }
            if (o.SinglePrimaryTypeId.HasValue)
            {
                int want = o.SinglePrimaryTypeId.Value + 1;
                if (want >= 0 && want < comboSingleType.Items.Count) comboSingleType.SelectedIndex = want;
            }
            checkNoDualTypes.Checked = o.NoDualTypes;
            checkNoLegendaries.Checked = o.DontUseLegendaries;
            checkBstMin.Checked = o.LimitBstMin;
            numericBstMin.Value = o.BstMinimum;
            checkBstMax.Checked = o.LimitBstMax;
            numericBstMax.Value = o.BstMaximum;
            checkStarterGraphics.Checked = o.StarterUpdateGraphicsAndCries;

            switch (o.StaticsMode)
            {
                case FvxStaticsRandomizationMode.SwapLegendariesAndStandards: statModeSwap.Checked = true; break;
                case FvxStaticsRandomizationMode.RandomCompletely: statModeRandomFull.Checked = true; break;
                case FvxStaticsRandomizationMode.RandomSimilarStrength: statModeSimilar.Checked = true; break;
                default: statModeUnchanged.Checked = true; break;
            }
            checkStatics600.Checked = o.StaticsRandomize600PlusBst;
            checkStaticsLegendLimit.Checked = o.StaticsLimitMainGameLegendaries;
            checkStaticsLevelPct.Checked = o.StaticsUseLevelPercentModifier;
            trackStaticsLevelPct.Value = Math.Max(trackStaticsLevelPct.Minimum, Math.Min(trackStaticsLevelPct.Maximum, o.StaticsLevelPercentModifier));

            switch (o.TradesMode)
            {
                case FvxTradesRandomizationMode.RandomizeGivenOnly: tradeModeGiven.Checked = true; break;
                case FvxTradesRandomizationMode.RandomizeBoth: tradeModeBoth.Checked = true; break;
                default: tradeModeUnchanged.Checked = true; break;
            }
            checkTradeNick.Checked = o.TradesRandomizeNicknames;
            checkTradeIv.Checked = o.TradesRandomizeIvs;
            checkTradeItem.Checked = o.TradesRandomizeItems;
        }

        void ApplyWildOptionsToUi(FvxWildPokemonOptions o)
        {
            if (o == null) o = new FvxWildPokemonOptions();

            var replaceMode = Enum.IsDefined(typeof(FvxWildReplacementMode), o.ReplacementMode)
                ? o.ReplacementMode
                : FvxWildReplacementMode.WholeGame;
            var typeMode = Enum.IsDefined(typeof(FvxWildTypeRestrictionMode), o.TypeRestrictionMode)
                ? o.TypeRestrictionMode
                : FvxWildTypeRestrictionMode.None;
            var evoMode = Enum.IsDefined(typeof(FvxWildEvolutionRestrictionMode), o.EvolutionRestrictionMode)
                ? o.EvolutionRestrictionMode
                : FvxWildEvolutionRestrictionMode.None;

            wildReplaceWholeGame.Checked = replaceMode == FvxWildReplacementMode.WholeGame;
            wildReplacePerArea.Checked = replaceMode == FvxWildReplacementMode.NamedLocation;
            wildReplacePerMap.Checked = replaceMode == FvxWildReplacementMode.PerMap;
            wildReplacePerEncounterSet.Checked = replaceMode == FvxWildReplacementMode.PerEncounterSet;
            wildReplaceMaximum.Checked = replaceMode == FvxWildReplacementMode.MaximumPossible;
            wildSplitByEncounterType.Checked = o.SplitByEncounterType;
            wildTypeNone.Checked = typeMode == FvxWildTypeRestrictionMode.None;
            wildTypeRandomThemes.Checked = typeMode == FvxWildTypeRestrictionMode.RandomZoneThemes;
            wildTypeKeepPrimary.Checked = typeMode == FvxWildTypeRestrictionMode.KeepPrimaryType;
            wildTypeKeepThemes.Checked = o.KeepZoneTypeThemes;
            wildEvoNone.Checked = evoMode == FvxWildEvolutionRestrictionMode.None;
            wildEvoBasicOnly.Checked = evoMode == FvxWildEvolutionRestrictionMode.BasicOnly;
            wildEvoSameStage.Checked = evoMode == FvxWildEvolutionRestrictionMode.SameEvolutionStage;
            wildEvoKeepRelations.Checked = o.KeepEvolutionRelations;
            wildUseTimeBased.Checked = o.UseTimeBasedEncounters;
            wildNoLegendaries.Checked = o.DontUseLegendaries;
            wildSetMinimumCatchRate.Checked = o.SetMinimumCatchRate;
            wildMinCatchRateLevel.Value = Math.Max(wildMinCatchRateLevel.Minimum, Math.Min(wildMinCatchRateLevel.Maximum, o.MinimumCatchRateLevel));
            wildRandomizeHeldItems.Checked = o.RandomizeHeldItems;
            wildBanBadItems.Checked = o.BanBadItems;
            wildCatchEmAll.Checked = o.CatchEmAllMode;
            wildSimilarStrength.Checked = o.SimilarStrength;
            wildBalanceLowLevel.Checked = o.BalanceLowLevelEncounters;
            wildAllowAltFormes.Checked = o.AllowAlternateFormes;
            wildLevelModifierEnabled.Checked = o.LevelModifierEnabled;
            wildLevelModifierTrack.Value = Math.Max(wildLevelModifierTrack.Minimum, Math.Min(wildLevelModifierTrack.Maximum, o.LevelModifierPercent));

            // Apply last: toggling this runs UpdateWildControlsEnabled, which must see final values for dependent controls (held items, similar strength, etc.).
            wildRandomizeCheck.Checked = o.RandomizeWildPokemon;
        }

        void ApplyFoeOptionsToUi(FvxFoePokemonOptions o)
        {
            comboFoeTrainerPokemonMode.SelectedIndex = (int)o.TrainerPokemonMode;
            comboFoeTierDetection.SelectedIndex = o.TierDetectionMode == FvxFoeTierDetectionMode.MatchingVanillaUpr ? 1 : 0;
            foeBw1TrioGymsStarters.Checked = o.Bw1TrioGymsMatchStarterTriangle;
            foeAddBoss.Checked = o.AdditionalPokemonBoss;
            foeAddImportant.Checked = o.AdditionalPokemonImportant;
            numFoeAddImportant.Value = o.AdditionalPokemonImportantCount;
            foeAddRegular.Checked = o.AdditionalPokemonRegular;
            numFoeAddRegular.Value = o.AdditionalPokemonRegularCount;
            foeHeldBoss.Checked = o.HeldItemsBoss;
            foeHeldImportant.Checked = o.HeldItemsImportant;
            foeHeldRegular.Checked = o.HeldItemsRegular;
            foeHeldConsumable.Checked = o.HeldConsumableOnly;
            foeHeldSensible.Checked = o.HeldSensibleItems;
            foeHeldHighestLv.Checked = o.HeldHighestLevelOnly;
            foeDivBoss.Checked = o.DiverseTypesBoss;
            foeDivImportant.Checked = o.DiverseTypesImportant;
            foeDivRegular.Checked = o.DiverseTypesRegular;
            foeBattleUnchanged.Checked = o.BattleStyleMode == FvxFoeBattleStyleMode.Unchanged;
            foeBattleRandomEach.Checked = o.BattleStyleMode == FvxFoeBattleStyleMode.Random;
            foeBattleRandomGlobal.Checked = o.BattleStyleMode == FvxFoeBattleStyleMode.RandomGlobal;
            foeBattleSingle.Checked = o.BattleStyleMode == FvxFoeBattleStyleMode.SingleStyle;
            if (o.SingleStyleBattleType >= 0 && o.SingleStyleBattleType < comboFoeSingleBattleType.Items.Count)
                comboFoeSingleBattleType.SelectedIndex = o.SingleStyleBattleType;
            foeBattleTierBoss.Checked = o.UniqueBattleStyleBoss;
            if (o.UniqueBattleStyleBossBattleType >= 0 && o.UniqueBattleStyleBossBattleType < comboFoeBattleTierBoss.Items.Count)
                comboFoeBattleTierBoss.SelectedIndex = o.UniqueBattleStyleBossBattleType;
            foeBattleTierImportant.Checked = o.UniqueBattleStyleImportant;
            if (o.UniqueBattleStyleImportantBattleType >= 0 && o.UniqueBattleStyleImportantBattleType < comboFoeBattleTierImportant.Items.Count)
                comboFoeBattleTierImportant.SelectedIndex = o.UniqueBattleStyleImportantBattleType;
            foeBattleTierRegular.Checked = o.UniqueBattleStyleRegular;
            if (o.UniqueBattleStyleRegularBattleType >= 0 && o.UniqueBattleStyleRegularBattleType < comboFoeBattleTierRegular.Items.Count)
                comboFoeBattleTierRegular.SelectedIndex = o.UniqueBattleStyleRegularBattleType;
            foeMidRivalStarter.Checked = o.RivalCarriesStarter;
            foeMidSimilar.Checked = o.SimilarStrength;
            foeMidNoDupes.Checked = o.AvoidDuplicates;
            foeMidWeightTypes.Checked = o.WeightTypesByCount;
            foeMidLocal.Checked = o.UseLocalPokemon;
            foeMidNoLegend.Checked = o.DontUseLegendaries;
            foeMidNoWG.Checked = o.NoEarlyWonderGuard;
            foeMidAltForms.Checked = o.AllowAlternateFormes;
            foeMidAllowNonStandard.Checked = o.AllowNonStandardPokemon;
            foeMidLeagueUnique.Checked = o.LeagueUniquePokemon;
            numFoeLeagueUnique.Value = o.LeagueUniqueCount;
            foeRightRandNames.Checked = o.RandomizeTrainerNames;
            foeRightRandClass.Checked = o.RandomizeTrainerClassNames;
            chkFoeTrainersEvolve.Checked = o.TrainersEvolvePokemon;
            trackFoeEvolvePct.Value = Math.Max(trackFoeEvolvePct.Minimum, Math.Min(trackFoeEvolvePct.Maximum, o.TrainersEvolvePercent));
            chkFoeLevelPct.Checked = o.LevelPercentModifierEnabled;
            trackFoeLevelPct.Value = Math.Max(trackFoeLevelPct.Minimum, Math.Min(trackFoeLevelPct.Maximum, o.LevelPercentModifier));
        }

        void ApplyItemsOptionsToUi(FvxItemsOptions o)
        {
            itemsFieldUnchanged.Checked = o.FieldItemsMod == FvxFieldItemsMod.Unchanged;
            itemsFieldShuffle.Checked = o.FieldItemsMod == FvxFieldItemsMod.Shuffle;
            itemsFieldRandom.Checked = o.FieldItemsMod == FvxFieldItemsMod.Random;
            itemsFieldRandomEven.Checked = o.FieldItemsMod == FvxFieldItemsMod.RandomEven;
            itemsFieldBanBad.Checked = o.BanBadRandomFieldItems;
            itemsShopUnchanged.Checked = o.ShopItemsMod == FvxShopItemsMod.Unchanged;
            itemsShopShuffle.Checked = o.ShopItemsMod == FvxShopItemsMod.Shuffle;
            itemsShopRandom.Checked = o.ShopItemsMod == FvxShopItemsMod.Random;
            itemsShopBanBad.Checked = o.BanBadRandomShopItems;
            itemsShopBanRegular.Checked = o.BanRegularShopItems;
            itemsShopBanOverpowered.Checked = o.BanOverpoweredShopItems;
            itemsShopGuaranteeEvolution.Checked = o.GuaranteeEvolutionItems;
            itemsShopGuaranteeXItems.Checked = o.GuaranteeXItems;
            itemsShopBalancePrices.Checked = o.BalanceShopPrices;
            itemsShopAddCheapRareCandy.Checked = o.AddCheapRareCandiesToShops;
            itemsPickupUnchanged.Checked = o.PickupItemsMod == FvxPickupItemsMod.Unchanged;
            itemsPickupRandom.Checked = o.PickupItemsMod == FvxPickupItemsMod.Random;
            itemsPickupBanBad.Checked = o.BanBadRandomPickupItems;
        }

        void ApplyMiscOptionsToUi(FvxMiscTweaksOptions o)
        {
            if (miscFastestText == null) return;
            miscFastestText.Checked = o.FastestText;
            miscNationalDexAtStart.Checked = false;
            miscFastEggHatching.Checked = o.FastEggHatching;
            miscForceChallengeMode.Checked = o.ForceChallengeMode;
            miscBanLuckyEgg.Checked = o.BanLuckyEgg;
            miscNoFreeLuckyEgg.Checked = o.NoFreeLuckyEgg;
            miscBanBigMoneyManiacItems.Checked = o.BanBigMoneyManiacItems;
            miscRunWithoutRunningShoes.Checked = o.RunWithoutRunningShoes;
            miscDisableLowHpMusic.Checked = o.DisableLowHpMusic;
            miscForgettableHms.Checked = o.ForgettableHms;
            miscBalanceStaticLevels.Checked = o.BalanceStaticLevels;
        }

        bool TryRunRandomizationPipeline(Random rnd, FvxRandomizerGlobalOptions global, out int? raceChecksum, out string error)
        {
            raceChecksum = null;
            error = null;
            var opt = BuildOptionsFromUi();
            opt.Global = global;
            var wildOpt = BuildWildPokemonOptionsFromUi();
            wildOpt.Global = global;
            var foeOpt = BuildFoePokemonOptionsFromUi();
            foeOpt.Global = global;

            if (!RunGeneShuffleStep(opt.IncludeFairyTypes, rnd, out var geneErr))
            {
                error = geneErr;
                return false;
            }

            var traitOpt = traitsControl.BuildOptions();
            if (!FvxPokemonTraitsPipeline.TryRun(traitOpt, rnd, out var traitErr))
            {
                error = traitErr;
                return false;
            }

            if (global != null && global.LimitPokemon && global.AllowedSpecies != null && global.AllowedSpecies.Count > 0
                && MainEditor.evolutionsNarc?.evolutions != null)
                FvxEvolutionAllowlistPruner.Apply(MainEditor.evolutionsNarc.evolutions, global.AllowedSpecies);

            if (!RunGeneShuffleLearnsetStep(rnd, out var learnErr))
            {
                error = learnErr;
                return false;
            }

            var miscOpt = BuildMiscTweaksOptionsFromUi();
            if (!FvxMiscTweaksPipeline.TryRun(miscOpt, rnd, out var miscErr))
            {
                error = miscErr;
                return false;
            }

            if (!FvxStartersStaticsTradesPipeline.TryRun(opt, rnd, out var startersErr))
            {
                error = string.IsNullOrEmpty(startersErr) ? "Starters / statics / trades step did not run." : startersErr;
                return false;
            }

            if (!FvxWildPokemonPipeline.TryRun(wildOpt, rnd, out var wildErr))
            {
                error = wildErr;
                return false;
            }

            var itemsOpt = BuildItemsOptionsFromUi();
            if (!FvxItemsPipeline.TryRun(itemsOpt, rnd, out var itemsErr))
            {
                error = itemsErr;
                return false;
            }

            if (!FvxFoePokemonPipeline.TryRun(foeOpt, rnd, out var foeErr))
            {
                error = foeErr;
                return false;
            }

            if (global != null && global.RandomizeIntroMon)
            {
                if (!TryPickAndApplyIntroMon(rnd, global, out var introErr))
                {
                    error = introErr;
                    return false;
                }
            }

            if (global != null && global.RaceMode && MainEditor.fileSystem != null)
            {
                byte[] rom = MainEditor.fileSystem.BuildRom();
                raceChecksum = FvxRandomizerRaceChecksum.ComputeFromRomBytes(rom);
            }

            return true;
        }

        bool TryPickAndApplyIntroMon(Random rnd, FvxRandomizerGlobalOptions global, out string error)
        {
            error = null;
            var pd = MainEditor.pokemonDataNarc?.pokemon;
            if (pd == null || pd.Count < 2)
            {
                error = "Intro: Pokémon data not loaded.";
                return false;
            }
            bool bw2 = MainEditor.RomType == RomType.BW2;
            var pool = new List<int>();
            for (int i = 1; i < pd.Count; i++)
                pool.Add(i);
            var evo = MainEditor.evolutionsNarc?.evolutions;
            pool = FvxGlobalSpeciesPoolFilter.FilterPool(pool, global, bw2, evo, 100);
            if (pool.Count == 0)
            {
                error = "Intro: no species left after global filters.";
                return false;
            }
            if (evo != null && evo.Count > 0)
            {
                var graph = FvxGen5EvolutionGraph.FromEvolutions(evo);
                var incoming = graph.ComputeIncoming();
                pool = pool.Where(i => i > 0 && i < incoming.Length && graph.IsBasic(i, incoming)).ToList();
                if (pool.Count == 0)
                {
                    error = "Intro: no unevolved (basic) species left for the intro cutscene; turn off Randomize intro Pokémon or widen filters.";
                    return false;
                }
            }
            int species = pool[rnd.Next(pool.Count)];
            return FvxIntroMonRunner.TryApply(species, out error);
        }

        static string BuildBatchLogText(int iterationSeed, int? raceChecksum, FvxRandomizerConfigFile preset)
        {
            var sb = new StringBuilder();
            sb.AppendLine("iterationSeed=" + iterationSeed);
            if (raceChecksum.HasValue) sb.AppendLine("raceChecksum=" + raceChecksum.Value);
            sb.AppendLine("romType=" + MainEditor.RomTypeId);
            sb.AppendLine("uiSeedField=" + (preset?.Seed ?? 0));
            sb.AppendLine();
            sb.AppendLine("--- preset (JSON) ---");
            if (preset != null)
                sb.AppendLine(FvxRandomizerConfigIO.ToJson(preset));
            return sb.ToString();
        }

        void RunBatchRandomization()
        {
            string dir = _batchOutDir.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                MessageBox.Show("Choose a valid output directory for batch ROMs.");
                return;
            }
            if (string.IsNullOrEmpty(MainEditor.Instance?.loadedRomPath) || !File.Exists(MainEditor.Instance.loadedRomPath))
            {
                MessageBox.Show("Load a ROM from a file path first (batch reloads from disk).");
                return;
            }

            int count = (int)_batchCount.Value;
            int start = (int)_batchStartIndex.Value;
            string prefix = _batchPrefix.Text ?? "random_";
            int baseSeed = (int)seedNumeric.Value;
            var global = BuildGlobalOptionsFromSettings();

            for (int i = 0; i < count; i++)
            {
                MainEditor.ReloadRomFileSystemFromLoadedPath();
                int iterSeed = baseSeed != 0 ? unchecked(baseSeed + i) : new Random().Next(int.MinValue, int.MaxValue);
                var rnd = new Random(iterSeed);
                if (!TryRunRandomizationPipeline(rnd, global, out int? race, out var err))
                {
                    MessageBox.Show("Batch stopped at file " + i + ": " + (err ?? ""));
                    MainEditor.ReloadRomFileSystemFromLoadedPath();
                    return;
                }

                byte[] rom = MainEditor.fileSystem.BuildRom();
                string outPath = Path.Combine(dir, prefix + (start + i) + ".nds");
                File.WriteAllBytes(outPath, rom);

                if (_batchLogs.Checked)
                {
                    var preset = CollectConfigFromUi();
                    File.WriteAllText(outPath + ".log", BuildBatchLogText(iterSeed, race, preset), Encoding.UTF8);
                }
            }

            if (_batchAdvanceIndex.Checked)
                FvxBatchIndexPrefs.Write(start + count);

            MainEditor.ReloadRomFileSystemFromLoadedPath();
            MessageBox.Show("Batch complete. Wrote " + count + " ROM(s) to:\n" + dir);
        }
    }

    static class FvxBatchIndexPrefs
    {
        const string Section = "FvxBatchStartIndex";

        public static int Read()
        {
            var b = FileFunctions.ReadFileSection("Preferences.txt", Section);
            if (b == null || b.Count == 0) return 1;
            if (int.TryParse(Encoding.UTF8.GetString(b.ToArray()), out int v)) return Math.Max(1, v);
            return 1;
        }

        public static void Write(int v) =>
            FileFunctions.WriteFileSection("Preferences.txt", Section, Encoding.UTF8.GetBytes(Math.Max(1, v).ToString()));
    }
}
