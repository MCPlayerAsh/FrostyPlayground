using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NewEditor.Data.Randomization.FvxGen5;

namespace NewEditor.Forms
{
    public class FvxGen5RandomizerForm : Form
    {
        FvxGen5FullSettings _settings = new FvxGen5FullSettings();

        TabControl tabs;
        NumericUpDown seedNumeric;
        TextBox logText;
        Button applyButton;
        Button loadSettingsButton;
        Button saveSettingsButton;
        Button closeButton;

        ComboBox traitsStatsCombo, traitsTypesCombo, traitsAbilitiesCombo, traitsEvolutionsCombo;
        CheckBox traitsFollowEvoStats, traitsForceDual, traitsAllowWonderGuard, traitsFollowAbilities;
        ComboBox foeTrainerCombo;
        CheckBox foeNoLegend;
        NumericUpDown foeSimilarPct;
        ComboBox wildModeCombo;
        CheckBox wildNoLegend;
        CheckBox wildLevelMod;
        NumericUpDown wildLevelPct;
        ComboBox coreMovesetCombo, coreTmHmCombo, coreTutorCombo;
        CheckBox coreBlockBroken, coreGuaranteedStart, coreForceGood, coreEvoAll, coreEggMoves, coreTmFollow, coreTutorFollow;
        NumericUpDown coreGuaranteedCount, coreGoodDamagingPct;
        CheckBox mdPower, mdAcc, mdPp, mdType, mdCat, mdName, mdGenUp;
        NumericUpDown mdGen;
        CheckBox tmShuffleMoveList, tmShuffleTutorList, tmSyncItemMeta;
        ComboBox startersModeCombo, staticsModeCombo, tradesModeCombo;
        ComboBox fieldItemsCombo, shopCombo, pickupCombo;
        CheckBox miscFastText, miscNatDex, miscFastEgg, miscChallenge, miscForgetHm;

        public FvxGen5RandomizerForm()
        {
            Text = "Randomize (Gen 5 FVX-style)";
            MinimumSize = new Size(880, 620);
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = true;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(8)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));

            tabs = new TabControl { Dock = DockStyle.Fill };
            BuildTabs();
            root.Controls.Add(tabs, 0, 0);

            logText = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font(FontFamily.GenericMonospace, 8.5f)
            };
            root.Controls.Add(logText, 0, 1);

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            bottom.Controls.Add(new Label { Text = "Seed (0 = random):", AutoSize = true });
            seedNumeric = new NumericUpDown { Minimum = int.MinValue, Maximum = int.MaxValue, Width = 120 };
            bottom.Controls.Add(seedNumeric);
            applyButton = new Button { Text = "Apply randomization", AutoSize = true };
            applyButton.Click += ApplyClick;
            loadSettingsButton = new Button { Text = "Load settings…", AutoSize = true };
            loadSettingsButton.Click += LoadSettingsClick;
            saveSettingsButton = new Button { Text = "Save settings…", AutoSize = true };
            saveSettingsButton.Click += SaveSettingsClick;
            closeButton = new Button { Text = "Close", AutoSize = true };
            closeButton.Click += (_, __) => Close();
            bottom.Controls.Add(applyButton);
            bottom.Controls.Add(loadSettingsButton);
            bottom.Controls.Add(saveSettingsButton);
            bottom.Controls.Add(closeButton);
            root.Controls.Add(bottom, 0, 2);

            Controls.Add(root);

            try { _settings = FvxGen5SettingsPersistence.Load(FvxGen5SettingsPersistence.DefaultPath); }
            catch { /* keep default */ }
            PushUiFromSettings();
        }

        TabPage WrapTab(string title, Action<Panel> buildContent)
        {
            var page = new TabPage(title);
            var host = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(4) };
            page.Controls.Add(host);
            buildContent(host);
            return page;
        }

        void BuildTabs()
        {
            tabs.TabPages.Add(WrapTab("Pokemon Traits", BuildTraitsContent));
            tabs.TabPages.Add(WrapTab("Starters, Statics & Trades", BuildStartersContent));
            tabs.TabPages.Add(WrapTab("Moves & Movesets", BuildMovesContent));
            tabs.TabPages.Add(WrapTab("Foe Pokemon", BuildFoeContent));
            tabs.TabPages.Add(WrapTab("Wild Pokemon", BuildWildContent));
            tabs.TabPages.Add(WrapTab("TM/HMs & Tutors", BuildTmHmContent));
            tabs.TabPages.Add(WrapTab("Items", BuildItemsContent));
            tabs.TabPages.Add(WrapTab("Misc. Tweaks", BuildMiscContent));
        }

        void BuildTraitsContent(Panel p)
        {
            int y = 8;
            var gb = new GroupBox { Text = "Base stats", Location = new Point(8, y), Size = new Size(820, 100) };
            traitsStatsCombo = AddLabeledCombo(gb, "Mode:", 10, 22, new[] { "Unchanged", "Shuffle", "Random" }, 100);
            traitsFollowEvoStats = AddCheck(gb, "Follow evolutions", 360, 24);
            p.Controls.Add(gb);
            y += 108;

            gb = new GroupBox { Text = "Types", Location = new Point(8, y), Size = new Size(820, 70) };
            traitsTypesCombo = AddLabeledCombo(gb, "Mode:", 10, 22, new[] { "Unchanged", "Random (follow evolutions)", "Random (completely)" }, 260);
            traitsForceDual = AddCheck(gb, "Force dual types", 560, 24);
            p.Controls.Add(gb);
            y += 78;

            gb = new GroupBox { Text = "Abilities", Location = new Point(8, y), Size = new Size(820, 70) };
            traitsAbilitiesCombo = AddLabeledCombo(gb, "Mode:", 10, 22, new[] { "Unchanged", "Random" }, 110);
            traitsAllowWonderGuard = AddCheck(gb, "Allow Wonder Guard", 260, 24);
            traitsFollowAbilities = AddCheck(gb, "Follow evolutions", 420, 24);
            p.Controls.Add(gb);
            y += 78;

            gb = new GroupBox { Text = "Evolutions (stub — not applied yet)", Location = new Point(8, y), Size = new Size(820, 60) };
            traitsEvolutionsCombo = AddLabeledCombo(gb, "Mode:", 10, 22, new[] { "Unchanged", "Random", "Random every level" }, 200);
            p.Controls.Add(gb);
        }

        void BuildStartersContent(Panel p)
        {
            var gb = new GroupBox { Text = "Starters / statics / trades", Location = new Point(8, 8), Size = new Size(820, 200) };
            startersModeCombo = AddLabeledCombo(gb, "Starters:", 10, 24, new[] { "Unchanged", "Custom", "Random (completely)", "Random (basic 3-stage)", "Random (any basic)" }, 220);
            staticsModeCombo = AddLabeledCombo(gb, "Statics:", 10, 56, new[] { "Unchanged", "Random completely", "Random similar strength" }, 220);
            tradesModeCombo = AddLabeledCombo(gb, "Trades:", 10, 88, new[] { "Unchanged", "Randomize given only", "Randomize both" }, 240);
            gb.Controls.Add(new Label
            {
                Text = "Statics: US B2/W2 script patches. Trades: US B/W trade script species only (trade NARC not edited).",
                Location = new Point(10, 130),
                Size = new Size(780, 36),
                ForeColor = Color.DimGray
            });
            p.Controls.Add(gb);
        }

        void BuildMovesContent(Panel p)
        {
            var gb = new GroupBox { Text = "Pokemon movesets (Core)", Location = new Point(8, 8), Size = new Size(820, 190) };
            coreMovesetCombo = AddLabeledCombo(gb, "Movesets:", 10, 22, new[] { "Unchanged", "Completely random", "Random (prefer same type)", "Metronome only" }, 280);
            coreBlockBroken = AddCheck(gb, "No game-breaking moves", 10, 52);
            coreGuaranteedStart = AddCheck(gb, "Guaranteed level-1 moves", 220, 52);
            coreGuaranteedCount = new NumericUpDown { Location = new Point(420, 48), Size = new Size(45, 22), Minimum = 2, Maximum = 4, Value = 4 };
            gb.Controls.Add(coreGuaranteedCount);
            coreForceGood = AddCheck(gb, "Force % good damaging", 500, 52);
            coreGoodDamagingPct = new NumericUpDown { Location = new Point(660, 48), Size = new Size(50, 22), Minimum = 0, Maximum = 100, Value = 50 };
            gb.Controls.Add(coreGoodDamagingPct);
            coreEvoAll = AddCheck(gb, "Evolution moves for all", 10, 80);
            coreEggMoves = AddCheck(gb, "Randomize egg moves", 220, 80);
            p.Controls.Add(gb);

            gb = new GroupBox { Text = "Move data", Location = new Point(8, 206), Size = new Size(820, 110) };
            mdPower = AddCheck(gb, "Power", 10, 22);
            mdAcc = AddCheck(gb, "Accuracy", 90, 22);
            mdPp = AddCheck(gb, "PP", 180, 22);
            mdType = AddCheck(gb, "Types", 250, 22);
            mdCat = AddCheck(gb, "Category", 330, 22);
            mdName = AddCheck(gb, "Names", 420, 22);
            mdGenUp = AddCheck(gb, "Update to gen", 500, 22);
            mdGen = new NumericUpDown { Location = new Point(620, 18), Minimum = 1, Maximum = 9, Value = 6, Width = 50 };
            gb.Controls.Add(mdGen);
            p.Controls.Add(gb);
        }

        void BuildFoeContent(Panel p)
        {
            var gb = new GroupBox { Text = "Trainer Pokémon", Location = new Point(8, 8), Size = new Size(820, 110) };
            foeTrainerCombo = AddLabeledCombo(gb, "Mode:", 10, 22, new[] { "Unchanged", "Random completely", "Random (similar strength)" }, 240);
            foeNoLegend = AddCheck(gb, "Don't use legendaries (BST ≥ 600)", 10, 52);
            gb.Controls.Add(new Label { Text = "Similar BST ±%", Location = new Point(280, 54), AutoSize = true });
            foeSimilarPct = new NumericUpDown { Location = new Point(380, 50), Minimum = 5, Maximum = 100, Value = 20, Width = 50 };
            gb.Controls.Add(foeSimilarPct);
            p.Controls.Add(gb);
        }

        void BuildWildContent(Panel p)
        {
            var gb = new GroupBox { Text = "Wild encounters", Location = new Point(8, 8), Size = new Size(820, 134) };
            wildModeCombo = AddLabeledCombo(gb, "Mode:", 10, 22,
                new[]
                {
                    "Unchanged",
                    "Random completely",
                    "Random completely (type zone themes not implemented — same as random)"
                }, 520);
            wildNoLegend = AddCheck(gb, "Don't use legendaries", 10, 52);
            wildLevelMod = AddCheck(gb, "Level modifier %", 220, 52);
            wildLevelPct = new NumericUpDown { Location = new Point(380, 48), Minimum = -100, Maximum = 150, Value = 0, Width = 55 };
            gb.Controls.Add(wildLevelPct);
            gb.Controls.Add(new Label
            {
                Text = "This dialog edits encounters in memory only — save the ROM afterward. The full Randomizer window’s Wild Pokémon tab is separate; use one flow per session.",
                Location = new Point(10, 84),
                Size = new Size(800, 36),
                ForeColor = Color.DimGray
            });
            p.Controls.Add(gb);
        }

        void BuildTmHmContent(Panel p)
        {
            var gb = new GroupBox { Text = "Compatibility (Core)", Location = new Point(8, 8), Size = new Size(820, 120) };
            coreTmHmCombo = AddLabeledCombo(gb, "TM/HM compat:", 10, 22, new[] { "Unchanged", "Random completely", "Random (prefer same type)" }, 260);
            coreTmFollow = AddCheck(gb, "Follow evolutions (TM)", 500, 24);
            coreTutorCombo = AddLabeledCombo(gb, "Tutor compat:", 10, 54, new[] { "Unchanged", "Random completely", "Random (prefer same type)" }, 260);
            coreTutorFollow = AddCheck(gb, "Follow evolutions (tutor)", 500, 56);
            if (MainEditor.RomType == RomType.BW1)
            {
                gb.Enabled = false;
                gb.Text = "Tutor compatibility (BW1 — N/A)";
            }
            p.Controls.Add(gb);

            gb = new GroupBox { Text = "Move lists (ARM9 / overlay)", Location = new Point(8, 134), Size = new Size(820, 96) };
            tmShuffleMoveList = AddCheck(gb, "Randomize TM move list (ARM9; HMs unchanged)", 10, 22);
            tmShuffleTutorList = AddCheck(gb, "Randomize tutor move list (BW2 overlay 36)", 380, 22);
            tmSyncItemMeta = AddCheck(gb, "After TM shuffle: sync item descriptions + TM palette colors (recommended)", 10, 50);
            p.Controls.Add(gb);
        }

        void BuildItemsContent(Panel p)
        {
            var gb = new GroupBox { Text = "Items", Location = new Point(8, 8), Size = new Size(820, 120) };
            fieldItemsCombo = AddLabeledCombo(gb, "Field items:", 10, 22, new[] { "Unchanged", "Shuffle", "Random", "Random (even distribution)" }, 220);
            shopCombo = AddLabeledCombo(gb, "Shop items:", 10, 54, new[] { "Unchanged", "Shuffle", "Random" }, 220);
            pickupCombo = AddLabeledCombo(gb, "Pickup:", 10, 86, new[] { "Unchanged", "Random" }, 220);
            p.Controls.Add(gb);
        }

        void BuildMiscContent(Panel p)
        {
            var gb = new GroupBox { Text = "Misc (ARM9 / script / optional IPS under Patches\\Gen5\\)", Location = new Point(8, 8), Size = new Size(820, 120) };
            miscFastText = AddCheck(gb, "Fastest text", 10, 22);
            miscNatDex = AddCheck(gb, "National Dex at start", 160, 22);
            miscFastEgg = AddCheck(gb, "Fast egg hatch", 340, 22);
            miscChallenge = AddCheck(gb, "Force Challenge Mode", 480, 22);
            miscForgetHm = AddCheck(gb, "Forgettable HMs", 10, 48);
            miscNatDex.Enabled = false;
            miscNatDex.Checked = false;
            p.Controls.Add(gb);
        }

        static ComboBox AddLabeledCombo(GroupBox gb, string label, int x, int y, string[] items, int width)
        {
            gb.Controls.Add(new Label { Text = label, Location = new Point(x, y + 3), AutoSize = true });
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(x + 108, y), Size = new Size(width, 24) };
            c.Items.AddRange(items);
            c.SelectedIndex = 0;
            gb.Controls.Add(c);
            return c;
        }

        static CheckBox AddCheck(GroupBox gb, string text, int x, int y)
        {
            var c = new CheckBox { Text = text, Location = new Point(x, y), AutoSize = true };
            gb.Controls.Add(c);
            return c;
        }

        void PushUiFromSettings()
        {
            ClampSeedToNumeric();
            var c = _settings.Core;
            coreMovesetCombo.SelectedIndex = Math.Min(coreMovesetCombo.Items.Count - 1, (int)c.MovesetsMod);
            coreBlockBroken.Checked = c.BlockBrokenMovesetMoves;
            coreGuaranteedStart.Checked = c.StartWithGuaranteedMoves;
            coreGuaranteedCount.Value = c.GuaranteedMoveCount;
            coreForceGood.Checked = c.MovesetsForceGoodDamaging;
            coreGoodDamagingPct.Value = c.MovesetsGoodDamagingPercent;
            coreEvoAll.Checked = c.EvolutionMovesForAll;
            coreEggMoves.Checked = c.RandomizeEggMoves;
            coreTmHmCombo.SelectedIndex = Math.Min(coreTmHmCombo.Items.Count - 1, (int)c.TmHmCompatMod);
            coreTmFollow.Checked = c.TmsFollowEvolutions;
            coreTutorCombo.SelectedIndex = Math.Min(coreTutorCombo.Items.Count - 1, (int)c.TutorCompatMod);
            coreTutorFollow.Checked = c.TutorFollowEvolutions;

            traitsStatsCombo.SelectedIndex = Math.Min(traitsStatsCombo.Items.Count - 1, (int)_settings.Traits.BaseStats);
            traitsFollowEvoStats.Checked = _settings.Traits.FollowEvolutionsStats;
            traitsTypesCombo.SelectedIndex = Math.Min(traitsTypesCombo.Items.Count - 1, (int)_settings.Traits.Types);
            traitsForceDual.Checked = _settings.Traits.ForceDualTypes;
            traitsAbilitiesCombo.SelectedIndex = Math.Min(traitsAbilitiesCombo.Items.Count - 1, (int)_settings.Traits.Abilities);
            traitsAllowWonderGuard.Checked = _settings.Traits.AllowWonderGuard;
            traitsFollowAbilities.Checked = _settings.Traits.FollowEvolutionsAbilities;
            traitsEvolutionsCombo.SelectedIndex = Math.Min(traitsEvolutionsCombo.Items.Count - 1, (int)_settings.Traits.Evolutions);

            foeTrainerCombo.SelectedIndex = Math.Min(foeTrainerCombo.Items.Count - 1, (int)_settings.Foe.TrainerPokemon);
            foeNoLegend.Checked = _settings.Foe.DontUseLegendaries;
            foeSimilarPct.Value = _settings.Foe.SimilarStrengthWindowPercent;

            wildModeCombo.SelectedIndex = Math.Min(wildModeCombo.Items.Count - 1, (int)_settings.Wild.WildMode);
            wildNoLegend.Checked = _settings.Wild.DontUseLegendaries;
            wildLevelMod.Checked = _settings.Wild.UseLevelModifier;
            wildLevelPct.Value = _settings.Wild.LevelModifierPercent;

            var md = _settings.MoveData;
            mdPower.Checked = md.RandomizeMovePower;
            mdAcc.Checked = md.RandomizeMoveAccuracy;
            mdPp.Checked = md.RandomizeMovePP;
            mdType.Checked = md.RandomizeMoveTypes;
            mdCat.Checked = md.RandomizeMoveCategory;
            mdName.Checked = md.RandomizeMoveNames;
            mdGenUp.Checked = md.UpdateMovesToGeneration;
            mdGen.Value = md.UpdateMovesGeneration;

            tmShuffleMoveList.Checked = _settings.TmHmTutorExtras.RandomizeTmMoveList;
            tmShuffleTutorList.Checked = _settings.TmHmTutorExtras.RandomizeTutorMoveList;
            tmSyncItemMeta.Checked = _settings.TmHmTutorExtras.SyncTmItemDescriptionsAndPalettes;

            startersModeCombo.SelectedIndex = Math.Min(startersModeCombo.Items.Count - 1, (int)_settings.StartersStaticsTrades.StartersMode);
            staticsModeCombo.SelectedIndex = Math.Min(staticsModeCombo.Items.Count - 1, (int)_settings.StartersStaticsTrades.StaticsMode);
            tradesModeCombo.SelectedIndex = Math.Min(tradesModeCombo.Items.Count - 1, (int)_settings.StartersStaticsTrades.TradesMode);

            fieldItemsCombo.SelectedIndex = Math.Min(fieldItemsCombo.Items.Count - 1, (int)_settings.Items.FieldItems);
            shopCombo.SelectedIndex = Math.Min(shopCombo.Items.Count - 1, (int)_settings.Items.ShopItems);
            pickupCombo.SelectedIndex = Math.Min(pickupCombo.Items.Count - 1, (int)_settings.Items.Pickup);

            miscFastText.Checked = _settings.Misc.FastestText;
            miscNatDex.Checked = false;
            miscFastEgg.Checked = _settings.Misc.FastEggHatching;
            miscChallenge.Checked = _settings.Misc.ForceChallengeMode;
            miscForgetHm.Checked = _settings.Misc.ForgettableHms;
            _settings.Misc.GiveNationalDexAtStart = false;
        }

        void ClampSeedToNumeric()
        {
            long s = _settings.Seed;
            decimal lo = seedNumeric.Minimum, hi = seedNumeric.Maximum;
            if (s < (long)lo) seedNumeric.Value = lo;
            else if (s > (long)hi) seedNumeric.Value = hi;
            else seedNumeric.Value = s;
        }

        void PullSettingsFromUi()
        {
            _settings.Seed = (int)seedNumeric.Value;
            var c = _settings.Core;
            c.MovesetsMod = (FvxMovesetsMod)coreMovesetCombo.SelectedIndex;
            c.BlockBrokenMovesetMoves = coreBlockBroken.Checked;
            c.StartWithGuaranteedMoves = coreGuaranteedStart.Checked;
            c.GuaranteedMoveCount = (int)coreGuaranteedCount.Value;
            c.MovesetsForceGoodDamaging = coreForceGood.Checked;
            c.MovesetsGoodDamagingPercent = (int)coreGoodDamagingPct.Value;
            c.EvolutionMovesForAll = coreEvoAll.Checked;
            c.RandomizeEggMoves = coreEggMoves.Checked;
            c.TmHmCompatMod = (FvxTmHmCompatMod)coreTmHmCombo.SelectedIndex;
            c.TmsFollowEvolutions = coreTmFollow.Checked;
            c.TutorCompatMod = (FvxTutorCompatMod)coreTutorCombo.SelectedIndex;
            c.TutorFollowEvolutions = coreTutorFollow.Checked;

            _settings.Traits.BaseStats = (FvxBaseStatsMode)traitsStatsCombo.SelectedIndex;
            _settings.Traits.FollowEvolutionsStats = traitsFollowEvoStats.Checked;
            _settings.Traits.Types = (FvxTraitsTypeMode)traitsTypesCombo.SelectedIndex;
            _settings.Traits.ForceDualTypes = traitsForceDual.Checked;
            _settings.Traits.Abilities = (FvxAbilitiesMode)traitsAbilitiesCombo.SelectedIndex;
            _settings.Traits.AllowWonderGuard = traitsAllowWonderGuard.Checked;
            _settings.Traits.FollowEvolutionsAbilities = traitsFollowAbilities.Checked;
            _settings.Traits.Evolutions = (FvxEvolutionsMode)traitsEvolutionsCombo.SelectedIndex;

            _settings.Foe.TrainerPokemon = (FvxTrainerPokemonMode)foeTrainerCombo.SelectedIndex;
            _settings.Foe.DontUseLegendaries = foeNoLegend.Checked;
            _settings.Foe.SimilarStrengthWindowPercent = (int)foeSimilarPct.Value;

            _settings.Wild.WildMode = (FvxWildMode)wildModeCombo.SelectedIndex;
            _settings.Wild.DontUseLegendaries = wildNoLegend.Checked;
            _settings.Wild.UseLevelModifier = wildLevelMod.Checked;
            _settings.Wild.LevelModifierPercent = (int)wildLevelPct.Value;

            var md = _settings.MoveData;
            md.RandomizeMovePower = mdPower.Checked;
            md.RandomizeMoveAccuracy = mdAcc.Checked;
            md.RandomizeMovePP = mdPp.Checked;
            md.RandomizeMoveTypes = mdType.Checked;
            md.RandomizeMoveCategory = mdCat.Checked;
            md.RandomizeMoveNames = mdName.Checked;
            md.UpdateMovesToGeneration = mdGenUp.Checked;
            md.UpdateMovesGeneration = (int)mdGen.Value;

            _settings.TmHmTutorExtras.RandomizeTmMoveList = tmShuffleMoveList.Checked;
            _settings.TmHmTutorExtras.RandomizeTutorMoveList = tmShuffleTutorList.Checked;
            _settings.TmHmTutorExtras.SyncTmItemDescriptionsAndPalettes = tmSyncItemMeta.Checked;

            _settings.StartersStaticsTrades.StartersMode = (FvxStartersMode)startersModeCombo.SelectedIndex;
            _settings.StartersStaticsTrades.StaticsMode = (FvxTrainerPokemonMode)staticsModeCombo.SelectedIndex;
            _settings.StartersStaticsTrades.TradesMode = (FvxTradesMode)tradesModeCombo.SelectedIndex;

            _settings.Items.FieldItems = (FvxFieldItemsMode)fieldItemsCombo.SelectedIndex;
            _settings.Items.ShopItems = (FvxShopItemsMode)shopCombo.SelectedIndex;
            _settings.Items.Pickup = (FvxPickupMode)pickupCombo.SelectedIndex;

            _settings.Misc.FastestText = miscFastText.Checked;
            _settings.Misc.GiveNationalDexAtStart = false;
            _settings.Misc.FastEggHatching = miscFastEgg.Checked;
            _settings.Misc.ForceChallengeMode = miscChallenge.Checked;
            _settings.Misc.ForgettableHms = miscForgetHm.Checked;
        }

        void ApplyClick(object sender, EventArgs e)
        {
            PullSettingsFromUi();
            if (!FvxGen5RandomizerOrchestrator.TryRun(_settings, _settings.Seed, out var report))
            {
                logText.Text = report;
                MessageBox.Show(report, "Randomizer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            logText.Text = report;
            MessageBox.Show("Randomization stages completed. Save the ROM to keep changes.", "Randomizer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            try { FvxGen5SettingsPersistence.Save(_settings, FvxGen5SettingsPersistence.DefaultPath); }
            catch { /* ignore */ }
        }

        void LoadSettingsClick(object sender, EventArgs e)
        {
            using (var d = new OpenFileDialog { Filter = "JSON settings|*.json|All files|*.*", Title = "Load randomizer settings" })
            {
                if (d.ShowDialog() != DialogResult.OK) return;
                _settings = FvxGen5SettingsPersistence.Load(d.FileName);
                PushUiFromSettings();
            }
        }

        void SaveSettingsClick(object sender, EventArgs e)
        {
            PullSettingsFromUi();
            using (var d = new SaveFileDialog { Filter = "JSON settings|*.json", DefaultExt = "json", FileName = FvxGen5SettingsPersistence.DefaultFileName })
            {
                if (d.ShowDialog() != DialogResult.OK) return;
                FvxGen5SettingsPersistence.Save(_settings, d.FileName);
            }
        }
    }
}
