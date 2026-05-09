using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Reads starter species embedded in scripts (<see cref="FvxGen5RomLayout.StarterOffsets"/>),
    /// mirroring UPR-FVX <c>RomHandler.getStarters()</c> for Gen 5 US ROMs.
    /// </summary>
    internal static class FvxRivalStarterResolver
    {
        /// <summary>
        /// Script-read trio (national dex indices). Null if scripts unavailable or unreadable.
        /// </summary>
        public static bool TryReadStarterTrio(bool bw2, ScriptNARC narc, out int[] trioNationalDex)
        {
            trioNationalDex = null;
            if (narc?.scriptFiles == null || narc.scriptFiles.Count == 0)
                return false;

            var layout = FvxGen5RomLayout.StarterOffsets(bw2);
            if (layout == null || layout.Length != 3)
                return false;

            var species = new int[3];
            for (int slot = 0; slot < 3; slot++)
            {
                var sites = layout[slot];
                int found = 0;
                if (sites != null)
                {
                    foreach (var s in sites)
                    {
                        if (s.File < 0 || s.File >= narc.scriptFiles.Count) continue;
                        var bytes = narc.scriptFiles[s.File].bytes;
                        if (bytes == null || s.Offset < 0 || s.Offset + 1 >= bytes.Length) continue;
                        found = HelperFunctions.ReadShort(bytes, s.Offset);
                        if (found > 0) break;
                    }
                }
                if (found <= 0) return false;
                species[slot] = found;
            }

            trioNationalDex = species;
            return true;
        }
    }
}
