using System.Collections.Generic;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Settings tab "General Options" carried into randomization pipelines (Gen 5).</summary>
    public sealed class FvxRandomizerGlobalOptions
    {
        public bool LimitPokemon { get; set; }
        /// <summary>National species indices allowed when <see cref="LimitPokemon"/> is true.</summary>
        public HashSet<int> AllowedSpecies { get; set; }

        public bool BanIrregularAltFormes { get; set; }
        public bool BanPrematureEvos { get; set; }
        public bool RandomizeIntroMon { get; set; }
        public bool RaceMode { get; set; }

        public static FvxRandomizerGlobalOptions Disabled() =>
            new FvxRandomizerGlobalOptions { AllowedSpecies = new HashSet<int>() };
    }
}
