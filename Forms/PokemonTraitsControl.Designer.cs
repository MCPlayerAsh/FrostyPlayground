namespace NewEditor.Forms
{
    partial class PokemonTraitsControl
    {
        System.ComponentModel.IContainer components = null;

        // Layout
        System.Windows.Forms.TableLayoutPanel rootTable;
        System.Windows.Forms.GroupBox baseStatsGroup;
        System.Windows.Forms.GroupBox abilitiesGroup;
        System.Windows.Forms.GroupBox evolutionsGroup;

        // Base Stats
        System.Windows.Forms.RadioButton baseStatsUnchanged;
        System.Windows.Forms.RadioButton baseStatsShuffle;
        System.Windows.Forms.RadioButton baseStatsRandom;
        System.Windows.Forms.CheckBox baseStatsFollowEvolutions;
        System.Windows.Forms.CheckBox baseStatsRandomizeAdded;
        System.Windows.Forms.CheckBox standardizeExpCheck;
        System.Windows.Forms.ComboBox standardizeExpTargetCombo;
        System.Windows.Forms.RadioButton standardizeExpScopeLegendarySlow;
        System.Windows.Forms.RadioButton standardizeExpScopeStrongLegendarySlow;
        System.Windows.Forms.RadioButton standardizeExpScopeAllPokemon;

        // Abilities
        System.Windows.Forms.RadioButton abilitiesUnchanged;
        System.Windows.Forms.RadioButton abilitiesRandom;
        System.Windows.Forms.CheckBox abilitiesAllowWonderGuard;
        System.Windows.Forms.CheckBox abilitiesCombineDuplicates;
        System.Windows.Forms.CheckBox abilitiesEnsureTwo;
        System.Windows.Forms.CheckBox abilitiesFollowEvolutions;
        System.Windows.Forms.Label abilitiesBanLabel;
        System.Windows.Forms.CheckBox abilitiesBanTrapping;
        System.Windows.Forms.CheckBox abilitiesBanNegative;
        System.Windows.Forms.CheckBox abilitiesBanBad;

        // Evolutions
        System.Windows.Forms.RadioButton evolutionsUnchanged;
        System.Windows.Forms.RadioButton evolutionsRandom;
        System.Windows.Forms.RadioButton evolutionsRandomEveryLevel;
        System.Windows.Forms.CheckBox evolutionsSimilarStrength;
        System.Windows.Forms.CheckBox evolutionsSameTyping;
        System.Windows.Forms.CheckBox evolutionsLimitToThreeStages;
        System.Windows.Forms.CheckBox evolutionsNoConvergence;
        System.Windows.Forms.CheckBox evolutionsForceChange;
        System.Windows.Forms.CheckBox evolutionsForceGrowth;
        System.Windows.Forms.CheckBox evolutionsChangeImpossible;
        System.Windows.Forms.CheckBox evolutionsMakeEasier;
        System.Windows.Forms.TrackBar evolutionsMakeEasierSlider;
        System.Windows.Forms.Label evolutionsMakeEasierValueLabel;
        System.Windows.Forms.CheckBox evolutionsUseEstimatedLevels;
        System.Windows.Forms.CheckBox evolutionsRemoveTimeBased;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        void InitializeComponent()
        {
            rootTable = new System.Windows.Forms.TableLayoutPanel();
            baseStatsGroup = new System.Windows.Forms.GroupBox();
            abilitiesGroup = new System.Windows.Forms.GroupBox();
            evolutionsGroup = new System.Windows.Forms.GroupBox();

            baseStatsUnchanged = new System.Windows.Forms.RadioButton();
            baseStatsShuffle = new System.Windows.Forms.RadioButton();
            baseStatsRandom = new System.Windows.Forms.RadioButton();
            baseStatsFollowEvolutions = new System.Windows.Forms.CheckBox();
            baseStatsRandomizeAdded = new System.Windows.Forms.CheckBox();
            standardizeExpCheck = new System.Windows.Forms.CheckBox();
            standardizeExpTargetCombo = new System.Windows.Forms.ComboBox();
            standardizeExpScopeLegendarySlow = new System.Windows.Forms.RadioButton();
            standardizeExpScopeStrongLegendarySlow = new System.Windows.Forms.RadioButton();
            standardizeExpScopeAllPokemon = new System.Windows.Forms.RadioButton();

            abilitiesUnchanged = new System.Windows.Forms.RadioButton();
            abilitiesRandom = new System.Windows.Forms.RadioButton();
            abilitiesAllowWonderGuard = new System.Windows.Forms.CheckBox();
            abilitiesCombineDuplicates = new System.Windows.Forms.CheckBox();
            abilitiesEnsureTwo = new System.Windows.Forms.CheckBox();
            abilitiesFollowEvolutions = new System.Windows.Forms.CheckBox();
            abilitiesBanLabel = new System.Windows.Forms.Label();
            abilitiesBanTrapping = new System.Windows.Forms.CheckBox();
            abilitiesBanNegative = new System.Windows.Forms.CheckBox();
            abilitiesBanBad = new System.Windows.Forms.CheckBox();

            evolutionsUnchanged = new System.Windows.Forms.RadioButton();
            evolutionsRandom = new System.Windows.Forms.RadioButton();
            evolutionsRandomEveryLevel = new System.Windows.Forms.RadioButton();
            evolutionsSimilarStrength = new System.Windows.Forms.CheckBox();
            evolutionsSameTyping = new System.Windows.Forms.CheckBox();
            evolutionsLimitToThreeStages = new System.Windows.Forms.CheckBox();
            evolutionsNoConvergence = new System.Windows.Forms.CheckBox();
            evolutionsForceChange = new System.Windows.Forms.CheckBox();
            evolutionsForceGrowth = new System.Windows.Forms.CheckBox();
            evolutionsChangeImpossible = new System.Windows.Forms.CheckBox();
            evolutionsMakeEasier = new System.Windows.Forms.CheckBox();
            evolutionsMakeEasierSlider = new System.Windows.Forms.TrackBar();
            evolutionsMakeEasierValueLabel = new System.Windows.Forms.Label();
            evolutionsUseEstimatedLevels = new System.Windows.Forms.CheckBox();
            evolutionsRemoveTimeBased = new System.Windows.Forms.CheckBox();

            rootTable.SuspendLayout();
            baseStatsGroup.SuspendLayout();
            abilitiesGroup.SuspendLayout();
            evolutionsGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(evolutionsMakeEasierSlider)).BeginInit();
            SuspendLayout();

            // Root layout: row 0 = base stats (full width), row 1 = abilities (full width), row 2 = evolutions (full width).
            rootTable.Dock = System.Windows.Forms.DockStyle.Fill;
            rootTable.ColumnCount = 1;
            rootTable.RowCount = 3;
            rootTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            // ---------------- Base Statistics ----------------
            baseStatsGroup.Text = "Pokemon Base Statistics";
            baseStatsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            baseStatsGroup.Padding = new System.Windows.Forms.Padding(6);

            baseStatsUnchanged.AutoSize = true;
            baseStatsUnchanged.Location = new System.Drawing.Point(10, 22);
            baseStatsUnchanged.Text = "Unchanged";
            baseStatsUnchanged.Checked = true;
            baseStatsUnchanged.CheckedChanged += BaseStatsModeChanged;

            baseStatsShuffle.AutoSize = true;
            baseStatsShuffle.Location = new System.Drawing.Point(10, 44);
            baseStatsShuffle.Text = "Shuffle";
            baseStatsShuffle.CheckedChanged += BaseStatsModeChanged;

            baseStatsRandom.AutoSize = true;
            baseStatsRandom.Location = new System.Drawing.Point(10, 66);
            baseStatsRandom.Text = "Random";
            baseStatsRandom.CheckedChanged += BaseStatsModeChanged;

            baseStatsFollowEvolutions.AutoSize = true;
            baseStatsFollowEvolutions.Location = new System.Drawing.Point(140, 22);
            baseStatsFollowEvolutions.Text = "Follow Evolutions";
            baseStatsFollowEvolutions.CheckedChanged += BaseStatsFollowChanged;

            baseStatsRandomizeAdded.AutoSize = true;
            baseStatsRandomizeAdded.Location = new System.Drawing.Point(140, 44);
            baseStatsRandomizeAdded.Text = "Randomize Added Stats on Evolution";

            standardizeExpCheck.AutoSize = true;
            standardizeExpCheck.Location = new System.Drawing.Point(420, 22);
            standardizeExpCheck.Text = "Standardize EXP Curves to:";
            standardizeExpCheck.CheckedChanged += StandardizeExpChanged;

            standardizeExpTargetCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            standardizeExpTargetCombo.Location = new System.Drawing.Point(420, 44);
            standardizeExpTargetCombo.Size = new System.Drawing.Size(140, 24);

            standardizeExpScopeLegendarySlow.AutoSize = true;
            standardizeExpScopeLegendarySlow.Location = new System.Drawing.Point(580, 22);
            standardizeExpScopeLegendarySlow.Text = "Legendaries: Slow";
            standardizeExpScopeLegendarySlow.Checked = true;

            standardizeExpScopeStrongLegendarySlow.AutoSize = true;
            standardizeExpScopeStrongLegendarySlow.Location = new System.Drawing.Point(580, 44);
            standardizeExpScopeStrongLegendarySlow.Text = "Strong Legendaries: Slow";

            standardizeExpScopeAllPokemon.AutoSize = true;
            standardizeExpScopeAllPokemon.Location = new System.Drawing.Point(580, 66);
            standardizeExpScopeAllPokemon.Text = "All Pokemon";

            baseStatsGroup.Controls.Add(baseStatsUnchanged);
            baseStatsGroup.Controls.Add(baseStatsShuffle);
            baseStatsGroup.Controls.Add(baseStatsRandom);
            baseStatsGroup.Controls.Add(baseStatsFollowEvolutions);
            baseStatsGroup.Controls.Add(baseStatsRandomizeAdded);
            baseStatsGroup.Controls.Add(standardizeExpCheck);
            baseStatsGroup.Controls.Add(standardizeExpTargetCombo);
            baseStatsGroup.Controls.Add(standardizeExpScopeLegendarySlow);
            baseStatsGroup.Controls.Add(standardizeExpScopeStrongLegendarySlow);
            baseStatsGroup.Controls.Add(standardizeExpScopeAllPokemon);

            // ---------------- Abilities ----------------
            abilitiesGroup.Text = "Pokemon Abilities";
            abilitiesGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            abilitiesGroup.Padding = new System.Windows.Forms.Padding(6);

            abilitiesUnchanged.AutoSize = true;
            abilitiesUnchanged.Location = new System.Drawing.Point(10, 22);
            abilitiesUnchanged.Text = "Unchanged";
            abilitiesUnchanged.Checked = true;
            abilitiesUnchanged.CheckedChanged += AbilitiesModeChanged;

            abilitiesRandom.AutoSize = true;
            abilitiesRandom.Location = new System.Drawing.Point(10, 44);
            abilitiesRandom.Text = "Random";
            abilitiesRandom.CheckedChanged += AbilitiesModeChanged;

            abilitiesAllowWonderGuard.AutoSize = true;
            abilitiesAllowWonderGuard.Location = new System.Drawing.Point(140, 22);
            abilitiesAllowWonderGuard.Text = "Allow Wonder Guard";

            abilitiesCombineDuplicates.AutoSize = true;
            abilitiesCombineDuplicates.Location = new System.Drawing.Point(290, 22);
            abilitiesCombineDuplicates.Text = "Combine Duplicate Abilities";

            abilitiesEnsureTwo.AutoSize = true;
            abilitiesEnsureTwo.Location = new System.Drawing.Point(490, 22);
            abilitiesEnsureTwo.Text = "Ensure Two Abilities";

            abilitiesFollowEvolutions.AutoSize = true;
            abilitiesFollowEvolutions.Location = new System.Drawing.Point(140, 44);
            abilitiesFollowEvolutions.Text = "Follow Evolutions";

            abilitiesBanLabel.AutoSize = true;
            abilitiesBanLabel.Location = new System.Drawing.Point(140, 70);
            abilitiesBanLabel.Text = "Ban:";

            abilitiesBanTrapping.AutoSize = true;
            abilitiesBanTrapping.Location = new System.Drawing.Point(180, 68);
            abilitiesBanTrapping.Text = "Trapping Abilities";

            abilitiesBanNegative.AutoSize = true;
            abilitiesBanNegative.Location = new System.Drawing.Point(330, 68);
            abilitiesBanNegative.Text = "Negative Abilities";

            abilitiesBanBad.AutoSize = true;
            abilitiesBanBad.Location = new System.Drawing.Point(480, 68);
            abilitiesBanBad.Text = "Bad Abilities";

            abilitiesGroup.Controls.Add(abilitiesUnchanged);
            abilitiesGroup.Controls.Add(abilitiesRandom);
            abilitiesGroup.Controls.Add(abilitiesAllowWonderGuard);
            abilitiesGroup.Controls.Add(abilitiesCombineDuplicates);
            abilitiesGroup.Controls.Add(abilitiesEnsureTwo);
            abilitiesGroup.Controls.Add(abilitiesFollowEvolutions);
            abilitiesGroup.Controls.Add(abilitiesBanLabel);
            abilitiesGroup.Controls.Add(abilitiesBanTrapping);
            abilitiesGroup.Controls.Add(abilitiesBanNegative);
            abilitiesGroup.Controls.Add(abilitiesBanBad);

            // ---------------- Evolutions ----------------
            evolutionsGroup.Text = "Pokemon Evolutions";
            evolutionsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            evolutionsGroup.Padding = new System.Windows.Forms.Padding(6);

            evolutionsUnchanged.AutoSize = true;
            evolutionsUnchanged.Location = new System.Drawing.Point(10, 22);
            evolutionsUnchanged.Text = "Unchanged";
            evolutionsUnchanged.Checked = true;
            evolutionsUnchanged.CheckedChanged += EvolutionsModeChanged;

            evolutionsRandom.AutoSize = true;
            evolutionsRandom.Location = new System.Drawing.Point(10, 44);
            evolutionsRandom.Text = "Random";
            evolutionsRandom.CheckedChanged += EvolutionsModeChanged;

            evolutionsRandomEveryLevel.AutoSize = true;
            evolutionsRandomEveryLevel.Location = new System.Drawing.Point(10, 66);
            evolutionsRandomEveryLevel.Text = "Random Every Level";
            evolutionsRandomEveryLevel.CheckedChanged += EvolutionsModeChanged;

            evolutionsSimilarStrength.AutoSize = true;
            evolutionsSimilarStrength.Location = new System.Drawing.Point(170, 22);
            evolutionsSimilarStrength.Text = "Similar Strength";

            evolutionsSameTyping.AutoSize = true;
            evolutionsSameTyping.Location = new System.Drawing.Point(170, 44);
            evolutionsSameTyping.Text = "Same Typing";

            evolutionsLimitToThreeStages.AutoSize = true;
            evolutionsLimitToThreeStages.Location = new System.Drawing.Point(170, 66);
            evolutionsLimitToThreeStages.Text = "Limit Evolutions to Three Stages";

            evolutionsNoConvergence.AutoSize = true;
            evolutionsNoConvergence.Location = new System.Drawing.Point(170, 88);
            evolutionsNoConvergence.Text = "No Convergence";

            evolutionsForceChange.AutoSize = true;
            evolutionsForceChange.Location = new System.Drawing.Point(170, 110);
            evolutionsForceChange.Text = "Force Change";
            evolutionsForceChange.Checked = true;

            evolutionsForceGrowth.AutoSize = true;
            evolutionsForceGrowth.Location = new System.Drawing.Point(170, 132);
            evolutionsForceGrowth.Text = "Force Growth";

            evolutionsChangeImpossible.AutoSize = true;
            evolutionsChangeImpossible.Location = new System.Drawing.Point(420, 22);
            evolutionsChangeImpossible.Text = "Change Impossible Evolutions";

            evolutionsMakeEasier.AutoSize = true;
            evolutionsMakeEasier.Location = new System.Drawing.Point(420, 44);
            evolutionsMakeEasier.Text = "Make Evolutions Easier";
            evolutionsMakeEasier.CheckedChanged += EvolutionsMakeEasierChanged;

            evolutionsMakeEasierSlider.Location = new System.Drawing.Point(420, 64);
            evolutionsMakeEasierSlider.Size = new System.Drawing.Size(220, 45);
            evolutionsMakeEasierSlider.Minimum = 30;
            evolutionsMakeEasierSlider.Maximum = 64;
            evolutionsMakeEasierSlider.TickFrequency = 5;
            evolutionsMakeEasierSlider.Value = 50;
            evolutionsMakeEasierSlider.Enabled = false;
            evolutionsMakeEasierSlider.ValueChanged += EvolutionsMakeEasierSliderChanged;

            evolutionsMakeEasierValueLabel.AutoSize = true;
            evolutionsMakeEasierValueLabel.Location = new System.Drawing.Point(645, 70);
            evolutionsMakeEasierValueLabel.Text = "50";

            evolutionsUseEstimatedLevels.AutoSize = true;
            evolutionsUseEstimatedLevels.Location = new System.Drawing.Point(420, 112);
            evolutionsUseEstimatedLevels.Text = "Use Estimated Evolution Levels";

            evolutionsRemoveTimeBased.AutoSize = true;
            evolutionsRemoveTimeBased.Location = new System.Drawing.Point(420, 134);
            evolutionsRemoveTimeBased.Text = "Remove Time-Based Evolutions";

            evolutionsGroup.Controls.Add(evolutionsUnchanged);
            evolutionsGroup.Controls.Add(evolutionsRandom);
            evolutionsGroup.Controls.Add(evolutionsRandomEveryLevel);
            evolutionsGroup.Controls.Add(evolutionsSimilarStrength);
            evolutionsGroup.Controls.Add(evolutionsSameTyping);
            evolutionsGroup.Controls.Add(evolutionsLimitToThreeStages);
            evolutionsGroup.Controls.Add(evolutionsNoConvergence);
            evolutionsGroup.Controls.Add(evolutionsForceChange);
            evolutionsGroup.Controls.Add(evolutionsForceGrowth);
            evolutionsGroup.Controls.Add(evolutionsChangeImpossible);
            evolutionsGroup.Controls.Add(evolutionsMakeEasier);
            evolutionsGroup.Controls.Add(evolutionsMakeEasierSlider);
            evolutionsGroup.Controls.Add(evolutionsMakeEasierValueLabel);
            evolutionsGroup.Controls.Add(evolutionsUseEstimatedLevels);
            evolutionsGroup.Controls.Add(evolutionsRemoveTimeBased);

            rootTable.Controls.Add(baseStatsGroup, 0, 0);
            rootTable.Controls.Add(abilitiesGroup, 0, 1);
            rootTable.Controls.Add(evolutionsGroup, 0, 2);

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(rootTable);
            Font = new System.Drawing.Font("Arial", 9.75F);

            ((System.ComponentModel.ISupportInitialize)(evolutionsMakeEasierSlider)).EndInit();
            baseStatsGroup.ResumeLayout(false);
            baseStatsGroup.PerformLayout();
            abilitiesGroup.ResumeLayout(false);
            abilitiesGroup.PerformLayout();
            evolutionsGroup.ResumeLayout(false);
            evolutionsGroup.PerformLayout();
            rootTable.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
