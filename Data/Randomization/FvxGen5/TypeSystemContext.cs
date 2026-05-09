using System;
using System.Linq;
using NewEditor.Data;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// Runtime type bounds for Gen 5: reads live text + ROM state (Fairy patch, Gene Shuffle, etc.).
    /// </summary>
    public sealed class TypeSystemContext
    {
        /// <summary>Maximum valid type index (inclusive), from type-name table.</summary>
        public int MaxTypeIndexInclusive { get; }

        /// <summary>Type count = MaxTypeIndexInclusive + 1.</summary>
        public int TypeCount => MaxTypeIndexInclusive + 1;

        public bool HasFairy { get; }

        TypeSystemContext(int maxInclusive, bool hasFairy)
        {
            MaxTypeIndexInclusive = Math.Max(0, Math.Min(255, maxInclusive));
            HasFairy = hasFairy;
        }

        public static TypeSystemContext FromLoadedRom()
        {
            try
            {
                var text = MainEditor.textNarc?.textFiles[VersionConstants.TypeNameTextFileID]?.text;
                int count = text?.Count ?? 17;
                int maxIdx = Math.Max(0, count - 1);
                bool fairy = text != null && text.Any(n => string.Equals(n, "Fairy", StringComparison.OrdinalIgnoreCase));
                return new TypeSystemContext(maxIdx, fairy);
            }
            catch
            {
                return new TypeSystemContext(16, false);
            }
        }

        public bool IsStab(byte moveElement, byte pokeType1, byte pokeType2)
        {
            return moveElement == pokeType1 || (pokeType2 != 255 && pokeType2 != pokeType1 && moveElement == pokeType2);
        }

        public byte RandomType(Random rnd) => (byte)rnd.Next(0, TypeCount);
    }
}
