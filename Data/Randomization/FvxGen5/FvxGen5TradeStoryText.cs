using NewEditor.Data;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Hooks for rewriting trade-related story strings when <see cref="FvxGen5RomLayout.TradeStoryTextPatches"/> is populated.
    /// </summary>
    internal static class FvxGen5TradeStoryText
    {
        public static void Apply(FvxStartersStaticsTradesOptions opt, bool bw2, bool blackVersion)
        {
            if (opt.TradesMode == FvxTradesRandomizationMode.Unchanged)
                return;

            var patches = FvxGen5RomLayout.TradeStoryTextPatches(bw2, blackVersion);
            if (patches == null || patches.Length == 0)
                return;

            var story = MainEditor.storyTextNarc;
            if (story?.textFiles == null)
                return;

            foreach (var p in patches)
            {
                if (p.TextFileId < 0 || p.TextFileId >= story.textFiles.Count)
                    continue;
                var tf = story.textFiles[p.TextFileId];
                if (tf?.text == null || p.LineIndex < 0 || p.LineIndex >= tf.text.Count)
                    continue;
                // When patches are filled in, set tf.text[p.LineIndex] from Pokémon name lines or trade mapping.
            }
        }
    }
}
