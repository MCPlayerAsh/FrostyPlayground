using System;
using System.IO;
using System.Text.Json;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public static class FvxGen5SettingsPersistence
    {
        public static string DefaultFileName => "FvxGen5RandomizerSettings.json";

        public static string DefaultPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultFileName);

        static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = false
        };

        public static void Save(FvxGen5FullSettings settings, string path)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            string json = JsonSerializer.Serialize(settings, JsonOpts);
            File.WriteAllText(path, json);
        }

        public static FvxGen5FullSettings Load(string path)
        {
            if (!File.Exists(path)) return new FvxGen5FullSettings();
            string json = File.ReadAllText(path);
            try
            {
                return JsonSerializer.Deserialize<FvxGen5FullSettings>(json, JsonOpts) ?? new FvxGen5FullSettings();
            }
            catch
            {
                return new FvxGen5FullSettings();
            }
        }
    }
}
