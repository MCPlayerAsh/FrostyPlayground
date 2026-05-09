using System;
using System.Windows.Forms;
using NewEditor.Data.Randomization.FvxGen5;

namespace NewEditor.Forms
{
    /// <summary>
    /// Shared UI for FVX-style Pokemon Traits randomization. Hosted by both
    /// <see cref="GeneShuffleForm"/> and <see cref="FvxRandomizerForm"/> so the two stay in sync.
    /// </summary>
    public partial class PokemonTraitsControl : UserControl
    {
        public PokemonTraitsControl()
        {
            InitializeComponent();
            PopulateExpCurveCombo();
            UpdateEnableState();
        }

        void PopulateExpCurveCombo()
        {
            standardizeExpTargetCombo.Items.Clear();
            standardizeExpTargetCombo.Items.Add(new CurveOption(FvxExpCurve.MediumFast, "Medium Fast"));
            standardizeExpTargetCombo.Items.Add(new CurveOption(FvxExpCurve.Fast, "Fast"));
            standardizeExpTargetCombo.Items.Add(new CurveOption(FvxExpCurve.MediumSlow, "Medium Slow"));
            standardizeExpTargetCombo.Items.Add(new CurveOption(FvxExpCurve.Slow, "Slow"));
            standardizeExpTargetCombo.Items.Add(new CurveOption(FvxExpCurve.Erratic, "Erratic"));
            standardizeExpTargetCombo.Items.Add(new CurveOption(FvxExpCurve.Fluctuating, "Fluctuating"));
            standardizeExpTargetCombo.SelectedIndex = 0;
        }

        public FvxPokemonTraitsOptions BuildOptions()
        {
            var opt = new FvxPokemonTraitsOptions
            {
                BaseStatsMod = baseStatsRandom.Checked
                    ? FvxBaseStatsMod.Random
                    : baseStatsShuffle.Checked
                        ? FvxBaseStatsMod.Shuffle
                        : FvxBaseStatsMod.Unchanged,
                BaseStatsFollowEvolutions = baseStatsFollowEvolutions.Checked,
                BaseStatsRandomizeAddedOnEvolution = baseStatsRandomizeAdded.Checked,

                StandardizeExpScope = !standardizeExpCheck.Checked
                    ? FvxStandardizeExpScope.None
                    : standardizeExpScopeAllPokemon.Checked
                        ? FvxStandardizeExpScope.AllPokemon
                        : standardizeExpScopeStrongLegendarySlow.Checked
                            ? FvxStandardizeExpScope.StrongLegendariesSlow
                            : FvxStandardizeExpScope.LegendariesSlow,
                StandardizeExpTarget = ((CurveOption)standardizeExpTargetCombo.SelectedItem)?.Value ?? FvxExpCurve.MediumFast,

                AbilitiesMod = abilitiesRandom.Checked ? FvxAbilitiesMod.Random : FvxAbilitiesMod.Unchanged,
                AbilitiesAllowWonderGuard = abilitiesAllowWonderGuard.Checked,
                AbilitiesCombineDuplicates = abilitiesCombineDuplicates.Checked,
                AbilitiesEnsureTwo = abilitiesEnsureTwo.Checked,
                AbilitiesFollowEvolutions = abilitiesFollowEvolutions.Checked,
                AbilitiesBanTrapping = abilitiesBanTrapping.Checked,
                AbilitiesBanNegative = abilitiesBanNegative.Checked,
                AbilitiesBanBad = abilitiesBanBad.Checked,

                EvolutionsMod = evolutionsRandomEveryLevel.Checked
                    ? FvxEvolutionsMod.RandomEveryLevel
                    : evolutionsRandom.Checked
                        ? FvxEvolutionsMod.Random
                        : FvxEvolutionsMod.Unchanged,
                EvolutionsSimilarStrength = evolutionsSimilarStrength.Checked,
                EvolutionsSameTyping = evolutionsSameTyping.Checked,
                EvolutionsLimitToThreeStages = evolutionsLimitToThreeStages.Checked,
                EvolutionsNoConvergence = evolutionsNoConvergence.Checked,
                EvolutionsForceChange = evolutionsForceChange.Checked,
                EvolutionsForceGrowth = evolutionsForceGrowth.Checked,
                EvolutionsChangeImpossible = evolutionsChangeImpossible.Checked,
                EvolutionsMakeEasier = evolutionsMakeEasier.Checked,
                EvolutionsMakeEasierLevelCap = evolutionsMakeEasierSlider.Value,
                EvolutionsUseEstimatedLevels = evolutionsUseEstimatedLevels.Checked,
                EvolutionsRemoveTimeBased = evolutionsRemoveTimeBased.Checked
            };

            return opt;
        }

        public void ApplyFromOptions(FvxPokemonTraitsOptions o)
        {
            if (o == null) return;
            switch (o.BaseStatsMod)
            {
                case FvxBaseStatsMod.Shuffle: baseStatsShuffle.Checked = true; break;
                case FvxBaseStatsMod.Random: baseStatsRandom.Checked = true; break;
                default: baseStatsUnchanged.Checked = true; break;
            }
            baseStatsFollowEvolutions.Checked = o.BaseStatsFollowEvolutions;
            baseStatsRandomizeAdded.Checked = o.BaseStatsRandomizeAddedOnEvolution;

            standardizeExpCheck.Checked = o.StandardizeExpScope != FvxStandardizeExpScope.None;
            standardizeExpScopeAllPokemon.Checked = o.StandardizeExpScope == FvxStandardizeExpScope.AllPokemon;
            standardizeExpScopeStrongLegendarySlow.Checked = o.StandardizeExpScope == FvxStandardizeExpScope.StrongLegendariesSlow;
            standardizeExpScopeLegendarySlow.Checked = o.StandardizeExpScope == FvxStandardizeExpScope.LegendariesSlow;
            for (int i = 0; i < standardizeExpTargetCombo.Items.Count; i++)
            {
                if (((CurveOption)standardizeExpTargetCombo.Items[i]).Value == o.StandardizeExpTarget)
                {
                    standardizeExpTargetCombo.SelectedIndex = i;
                    break;
                }
            }

            if (o.AbilitiesMod == FvxAbilitiesMod.Random) abilitiesRandom.Checked = true;
            else abilitiesUnchanged.Checked = true;
            abilitiesAllowWonderGuard.Checked = o.AbilitiesAllowWonderGuard;
            abilitiesCombineDuplicates.Checked = o.AbilitiesCombineDuplicates;
            abilitiesEnsureTwo.Checked = o.AbilitiesEnsureTwo;
            abilitiesFollowEvolutions.Checked = o.AbilitiesFollowEvolutions;
            abilitiesBanTrapping.Checked = o.AbilitiesBanTrapping;
            abilitiesBanNegative.Checked = o.AbilitiesBanNegative;
            abilitiesBanBad.Checked = o.AbilitiesBanBad;

            switch (o.EvolutionsMod)
            {
                case FvxEvolutionsMod.RandomEveryLevel: evolutionsRandomEveryLevel.Checked = true; break;
                case FvxEvolutionsMod.Random: evolutionsRandom.Checked = true; break;
                default: evolutionsUnchanged.Checked = true; break;
            }
            evolutionsSimilarStrength.Checked = o.EvolutionsSimilarStrength;
            evolutionsSameTyping.Checked = o.EvolutionsSameTyping;
            evolutionsLimitToThreeStages.Checked = o.EvolutionsLimitToThreeStages;
            evolutionsNoConvergence.Checked = o.EvolutionsNoConvergence;
            evolutionsForceChange.Checked = o.EvolutionsForceChange;
            evolutionsForceGrowth.Checked = o.EvolutionsForceGrowth;
            evolutionsChangeImpossible.Checked = o.EvolutionsChangeImpossible;
            evolutionsMakeEasier.Checked = o.EvolutionsMakeEasier;
            int cap = o.EvolutionsMakeEasierLevelCap;
            if (cap < evolutionsMakeEasierSlider.Minimum) cap = evolutionsMakeEasierSlider.Minimum;
            if (cap > evolutionsMakeEasierSlider.Maximum) cap = evolutionsMakeEasierSlider.Maximum;
            evolutionsMakeEasierSlider.Value = cap;
            evolutionsUseEstimatedLevels.Checked = o.EvolutionsUseEstimatedLevels;
            evolutionsRemoveTimeBased.Checked = o.EvolutionsRemoveTimeBased;
            UpdateEnableState();
        }

        void BaseStatsModeChanged(object sender, EventArgs e) => UpdateEnableState();
        void BaseStatsFollowChanged(object sender, EventArgs e) => UpdateEnableState();
        void StandardizeExpChanged(object sender, EventArgs e) => UpdateEnableState();
        void AbilitiesModeChanged(object sender, EventArgs e) => UpdateEnableState();
        void EvolutionsModeChanged(object sender, EventArgs e) => UpdateEnableState();
        void EvolutionsMakeEasierChanged(object sender, EventArgs e) => UpdateEnableState();

        void EvolutionsMakeEasierSliderChanged(object sender, EventArgs e)
        {
            evolutionsMakeEasierValueLabel.Text = evolutionsMakeEasierSlider.Value.ToString();
        }

        void UpdateEnableState()
        {
            bool baseStatsActive = !baseStatsUnchanged.Checked;
            baseStatsFollowEvolutions.Enabled = baseStatsActive;
            baseStatsRandomizeAdded.Enabled = baseStatsActive && baseStatsFollowEvolutions.Checked;

            bool exp = standardizeExpCheck.Checked;
            standardizeExpTargetCombo.Enabled = exp;
            standardizeExpScopeLegendarySlow.Enabled = exp;
            standardizeExpScopeStrongLegendarySlow.Enabled = exp;
            standardizeExpScopeAllPokemon.Enabled = exp;

            bool abilitiesActive = abilitiesRandom.Checked;
            abilitiesAllowWonderGuard.Enabled = abilitiesActive;
            abilitiesCombineDuplicates.Enabled = abilitiesActive;
            abilitiesEnsureTwo.Enabled = abilitiesActive;
            abilitiesFollowEvolutions.Enabled = abilitiesActive;
            abilitiesBanTrapping.Enabled = abilitiesActive;
            abilitiesBanNegative.Enabled = abilitiesActive;
            abilitiesBanBad.Enabled = abilitiesActive;

            bool evosActive = !evolutionsUnchanged.Checked;
            evolutionsSimilarStrength.Enabled = evosActive;
            evolutionsSameTyping.Enabled = evosActive;
            evolutionsLimitToThreeStages.Enabled = evosActive;
            evolutionsNoConvergence.Enabled = evosActive;
            evolutionsForceChange.Enabled = evosActive;
            evolutionsForceGrowth.Enabled = evosActive;

            evolutionsMakeEasierSlider.Enabled = evolutionsMakeEasier.Checked;
        }

        sealed class CurveOption
        {
            public FvxExpCurve Value { get; }
            public string Label { get; }
            public CurveOption(FvxExpCurve v, string label)
            {
                Value = v;
                Label = label;
            }
            public override string ToString() => Label;
        }
    }
}
