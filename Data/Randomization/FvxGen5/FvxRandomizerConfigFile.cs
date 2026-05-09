using System.Text.Json;
using System.Text.Json.Serialization;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public sealed class FvxRandomizerSettingsGeneral
    {
        public bool LimitPokemon { get; set; }
        public int[] AllowedSpecies { get; set; }
        public bool BanIrregularAltFormes { get; set; }
        public bool BanPrematureEvos { get; set; }
        public bool RandomizeIntroMon { get; set; }
        public bool RaceMode { get; set; }
    }

    public sealed class FvxRandomizerBatchSettings
    {
        public bool Enabled { get; set; }
        public int Count { get; set; } = 1;
        public int StartingIndex { get; set; } = 1;
        public string FileNamePrefix { get; set; } = "random_";
        public string OutputDirectory { get; set; } = "";
        public bool GenerateLogs { get; set; }
        public bool AutoAdvanceStartingIndex { get; set; }
    }

    /// <summary>Frosty JSON preset for the FVX randomizer dialog (schema version 1).</summary>
    public sealed class FvxRandomizerConfigFile
    {
        public int SchemaVersion { get; set; } = 1;
        public int Seed { get; set; }
        public bool IncludeFairyTypes { get; set; }
        public FvxPokemonTraitsOptions Traits { get; set; }
        public int GeneShuffleTypeMode { get; set; }
        public FvxRandomizerOptions GeneLearn { get; set; }
        public FvxStartersStaticsTradesOptions Starters { get; set; }
        public FvxWildPokemonOptions Wild { get; set; }
        public FvxFoePokemonOptions Foe { get; set; }
        public FvxItemsOptions Items { get; set; }
        public FvxMiscTweaksOptions Misc { get; set; }
        public FvxRandomizerSettingsGeneral General { get; set; }
        public FvxRandomizerBatchSettings Batch { get; set; }
    }

    public static class FvxRandomizerConfigIO
    {
        static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string ToJson(FvxRandomizerConfigFile file) =>
            JsonSerializer.Serialize(file, Options);

        public static FvxRandomizerConfigFile FromJson(string json) =>
            JsonSerializer.Deserialize<FvxRandomizerConfigFile>(json, Options);
    }
}
