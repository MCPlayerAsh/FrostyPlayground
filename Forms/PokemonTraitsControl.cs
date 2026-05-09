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
