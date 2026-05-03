using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NewEditor.Data;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.GeneShuffle
{
    /// <summary>Applies bundled Fairy Vpatches for Gene Shuffle when requested.</summary>
    public static class GeneShuffleFairyPatch
    {
        /// <summary>True if type name text already lists Fairy (ROM already extended).</summary>
        public static bool TypeChartListsFairy()
        {
            try
            {
                var text = MainEditor.textNarc?.textFiles[VersionConstants.TypeNameTextFileID]?.text;
                return text != null && text.Contains("Fairy");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// If <paramref name="includeFairy"/> is false, returns success without patching.
        /// White 1 + Fairy is rejected. Otherwise applies matching patch when Fairy is not already in type chart.
        /// </summary>
        public static bool TryPrepare(bool includeFairy, out bool patchApplied, out string error)
        {
            patchApplied = false;
            error = null;
            if (!includeFairy) return true;

            string rt = MainEditor.RomTypeId ?? "";
            if (rt == "pokemon w")
            {
                error = "Fairy-type support in Gene Shuffle uses a Vpatch that exists for Pokémon Black 1, Black 2, and White 2 only—not White 1. Uncheck \"Include Fairy-Types\" or use a supported ROM.";
                return false;
            }

            if (TypeChartListsFairy()) return true;

            string fileName;
            if (rt == "pokemon b") fileName = "Black1Fairy.Vpatch";
            else if (rt == "pokemon b2") fileName = "Black2Fairy.Vpatch";
            else if (rt == "pokemon w2") fileName = "White2Fairy.Vpatch";
            else
            {
                error = "Gene Shuffle Fairy patch is only for Gen 5 Black/White/Black 2/White 2.";
                return false;
            }

            string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Patches", fileName);
            if (!File.Exists(path))
            {
                error = "Fairy patch file not found: " + path;
                return false;
            }

            Dictionary<string, IEnumerable<byte>> data = FileFunctions.ReadAllSections(path, true);
            if (!data.ContainsKey("version"))
            {
                error = "Invalid fairy patch (missing version section).";
                return false;
            }

            string version = Encoding.ASCII.GetString(data["version"].ToArray());
            if (version != rt)
            {
                error = "Fairy patch version does not match loaded ROM.";
                return false;
            }

            if (MainEditor.fileSystem == null)
            {
                error = "No ROM loaded.";
                return false;
            }

            PatchingSystem.ApplyPatch(MainEditor.fileSystem, data);
            patchApplied = true;
            return true;
        }

        /// <summary>Max type index inclusive: 16 vanilla, 17 when Fairy pool is active.</summary>
        public static int MaxTypeInclusive(bool includeFairyRequested, bool fairyPatchJustApplied)
        {
            if (!includeFairyRequested) return 16;
            if (fairyPatchJustApplied || TypeChartListsFairy()) return 17;
            return 16;
        }
    }
}
