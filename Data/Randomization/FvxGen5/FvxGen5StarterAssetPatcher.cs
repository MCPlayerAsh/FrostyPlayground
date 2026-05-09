using System;
using System.Collections.Generic;
using System.Globalization;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>
    /// UPR-style starter graphics, cries, optional Pokédex-registration script splice, and starter-location story text.
    /// </summary>
    internal static class FvxGen5StarterAssetPatcher
    {
        static readonly byte[] Bw1NewStarterScript =
        {
            0x24, 0x00, 0xA7, 0x02, 0xE7, 0x00, 0x00, 0x00,
            0xDE, 0x00, 0x00, 0x00, 0xF8, 0x01, 0x05, 0x00,
        };

        static readonly byte[] Bw2NewStarterScript =
        {
            0x28, 0x00, 0xA1, 0x40, 0x04, 0x00, 0xDE, 0x00,
            0x00, 0x00, 0xFD, 0x01, 0x05, 0x00,
        };

        /// <summary>Apply starter presentation when requested and starters were randomized/customized.</summary>
        public static bool TryApply(bool requested, int[] nationalSpeciesIds, bool bw2, out string error)
        {
            error = null;
            if (!requested || nationalSpeciesIds == null || nationalSpeciesIds.Length != 3)
                return true;

            var fs = MainEditor.fileSystem;
            var sprites = MainEditor.pokemonSpritesNarc?.sprites;
            var script = MainEditor.scriptNarc;
            var story = MainEditor.storyTextNarc;
            var text = MainEditor.textNarc;
            var pd = MainEditor.pokemonDataNarc?.pokemon;

            if (fs?.narcs == null || sprites == null || script == null || story == null || text == null || pd == null)
            {
                error = "Starter presentation: required narcs or Pokémon data are not loaded.";
                return false;
            }

            int gfxIdx = FvxGen5RomLayout.StarterGraphicsNarcIndex;
            if (gfxIdx < 0 || gfxIdx >= fs.narcs.Count)
            {
                error = "Starter presentation: starter graphics NARC index is out of range.";
                return false;
            }

            var layout = FvxGen5RomLayout.StarterPresentationUs(bw2);
            var starterNarc = fs.narcs[gfxIdx];

            foreach (int sid in nationalSpeciesIds)
            {
                if (sid <= 0 || sid >= sprites.Count)
                {
                    error = $"Starter presentation: species index {sid} is out of sprite range.";
                    return false;
                }
            }

            if (!TryPatchStarterGraphics(starterNarc, sprites, nationalSpeciesIds, out var gfxErr))
            {
                error = gfxErr;
                return false;
            }

            if (!TryPatchPokedexScript(script, bw2, layout.PokedexGivenScriptFileId, out var dexErr))
            {
                error = dexErr;
                return false;
            }

            if (!TryPatchStarterCryOverlay(fs, layout, nationalSpeciesIds, out var cryErr))
            {
                error = cryErr;
                return false;
            }

            if (!TryPatchStoryStrings(story, text, pd, layout, nationalSpeciesIds, bw2, out var txtErr))
            {
                error = txtErr;
                return false;
            }

            script.WriteData();
            story.WriteData();
            return true;
        }

        static bool TryPatchStarterGraphics(NARC starterNarc, List<PokemonSpriteEntry> pokesprites, int[] speciesIds,
            out string error)
        {
            error = null;
            for (int starterIndex = 0; starterIndex < 3; starterIndex++)
            {
                int pokeNumber = speciesIds[starterIndex];
                var entry = pokesprites[pokeNumber];
                if (entry.files.Count < 19)
                {
                    error = $"Starter graphics: species {pokeNumber} has too few sprite subfiles.";
                    return false;
                }

                byte[] palette = entry.files[18];
                byte[] compressedPic = entry.files[0];
                byte[] uncompressedPic = DsDecmp.Decompress(compressedPic);
                if (uncompressedPic == null || uncompressedPic.Length == 0)
                {
                    error = $"Starter graphics: could not decompress front sprite for species {pokeNumber}.";
                    return false;
                }

                starterNarc.ReplaceFileEntry(starterIndex * 2, new List<byte>(palette));
                starterNarc.ReplaceFileEntry(12 + starterIndex, new List<byte>(uncompressedPic));
            }

            return true;
        }

        static bool TryPatchPokedexScript(ScriptNARC script, bool bw2, int fileId, out string error)
        {
            error = null;
            if (fileId < 0 || fileId >= script.scriptFiles.Count)
            {
                error = "Pokédex script patch: script file id is out of range.";
                return false;
            }

            byte[] oldFile = RefBytesToArray(script.scriptFiles[fileId].bytes);
            byte[] newBlob = bw2 ? Bw2NewStarterScript : Bw1NewStarterScript;
            string magicHex = bw2 ? "2800A1400400" : "2400A702";

            int hit = FindUniqueHex(oldFile, magicHex);
            if (hit == -1)
            {
                error = "Pokédex script patch: starter magic bytes were not found (unsupported or modified ROM).";
                return false;
            }

            if (hit == -2)
            {
                error = "Pokédex script patch: starter magic matched more than once.";
                return false;
            }

            var newFile = new byte[oldFile.Length + newBlob.Length];
            Buffer.BlockCopy(oldFile, 0, newFile, 0, oldFile.Length);
            Buffer.BlockCopy(newBlob, 0, newFile, oldFile.Length, newBlob.Length);

            int offset = hit;
            if (bw2)
            {
                newFile[offset++] = 0x1E;
                newFile[offset++] = 0x00;
            }
            else
            {
                newFile[offset++] = 0x04;
                newFile[offset++] = 0x00;
            }

            WriteRelativePointer(newFile, offset, oldFile.Length);
            script.scriptFiles[fileId] = new ScriptFile(newFile);
            return true;
        }

        static bool TryPatchStarterCryOverlay(NDSFileSystem fs, FvxGen5RomLayout.StarterPresentationRow layout,
            int[] speciesIds, out string error)
        {
            error = null;
            int ovlIdx = layout.StarterCryOvlNumber;
            if (ovlIdx < 0 || ovlIdx >= fs.overlays.Count)
            {
                error = "Starter cry patch: overlay index is out of range.";
                return false;
            }

            byte[] prefix = FvxGen5RomLayout.ParseHexPrefix(layout.StarterCryTablePrefixHex);
            if (prefix.Length == 0)
            {
                error = "Starter cry patch: invalid cry table prefix.";
                return false;
            }

            var raw = fs.overlays[ovlIdx].ToArray();
            bool wasCompressed = ovlIdx < fs.y9.entries.Count && fs.y9.entries[ovlIdx].compressed;
            byte[] work = wasCompressed ? BLZDecoder.BLZ_DecodePub(raw) : raw;
            if (work == null || work.Length == 0)
            {
                error = "Starter cry overlay could not be read or decompressed.";
                return false;
            }

            int p = IndexOf(work, prefix);
            if (p < 0)
            {
                error = "Starter cry patch: cry table prefix was not found in overlay.";
                return false;
            }

            int w = p + prefix.Length;
            if (w + 6 > work.Length)
            {
                error = "Starter cry patch: cry species table would overflow overlay.";
                return false;
            }

            for (int i = 0; i < 3; i++)
                HelperFunctions.WriteShort(work, w + i * 2, speciesIds[i]);

            if (wasCompressed)
            {
                byte[] enc = BLZDecoder.BLZ_EncodePub(work, true);
                if (enc == null || enc.Length == 0)
                {
                    error = "Starter cry patch: BLZ recompression failed.";
                    return false;
                }

                fs.overlays[ovlIdx] = new List<byte>(enc);
                var y9 = fs.y9.entries[ovlIdx];
                y9.compressed = true;
                y9.compressedSize = enc.Length;
                y9.Apply();
            }
            else
                fs.overlays[ovlIdx] = new List<byte>(work);

            return true;
        }

        static bool TryPatchStoryStrings(TextNARC story, TextNARC textNarc, IReadOnlyList<PokemonEntry> pokemon,
            FvxGen5RomLayout.StarterPresentationRow layout, int[] speciesIds, bool bw2, out string error)
        {
            error = null;
            int fileId = layout.StarterLocationStoryTextFileId;
            if (fileId < 0 || fileId >= story.textFiles.Count)
            {
                error = "Starter text: story text file id is out of range.";
                return false;
            }

            var tf = story.textFiles[fileId];
            if (tf.text == null)
                tf.text = new List<string>();

            int typeFile = VersionConstants.TypeNameTextFileID;
            int nameFile = VersionConstants.PokemonNameTextFileID;
            if (typeFile < 0 || typeFile >= textNarc.textFiles.Count || nameFile < 0 || nameFile >= textNarc.textFiles.Count)
            {
                error = "Starter text: type or Pokémon name text file is unavailable.";
                return false;
            }

            var typeNames = textNarc.textFiles[typeFile].text;
            var pokeNames = textNarc.textFiles[nameFile].text;

            void EnsureLine(int idx)
            {
                while (tf.text.Count <= idx) tf.text.Add(string.Empty);
            }

            if (!bw2)
            {
                int maxLine = layout.Bw1StarterTextMaxLine;
                for (int i = 0; i < 3; i++)
                {
                    int lineIdx = maxLine - i;
                    EnsureLine(lineIdx);
                    tf.text[lineIdx] = BuildStarterBallLabel(speciesIds[i], pokemon, typeNames, pokeNames);
                }

                EnsureLine(layout.Bw1CherenText1Line);
                tf.text[layout.Bw1CherenText1Line] =
                    "Cheren: Hey, how come you get to pick\\xfffeout my Pok\\x00e9mon?"
                    + "\\xf000\\xbe01\\x0000\\xfffEOh, never mind. I wanted this one\\xfffefrom the start, anyway."
                    + "\\xf000\\xbe01\\x0000";

                EnsureLine(layout.Bw1CherenText2Line);
                tf.text[layout.Bw1CherenText2Line] =
                    "It's decided. You'll be my opponent...\\xfffEin our first Pok\\x00e9mon battle!"
                    + "\\xf000\\xbe01\\x0000\\xfffELet's see what you can do, \\xfffEmy Pok\\x00e9mon!"
                    + "\\xf000\\xbe01\\x0000";
            }
            else
            {
                int maxLine = layout.Bw2StarterTextMaxLine;
                for (int i = 0; i < 3; i++)
                {
                    int lineIdx = maxLine - i;
                    EnsureLine(lineIdx);
                    tf.text[lineIdx] = BuildStarterBallLabel(speciesIds[i], pokemon, typeNames, pokeNames);
                }

                EnsureLine(layout.Bw2RivalTextLine);
                tf.text[layout.Bw2RivalTextLine] =
                    "\\xf000\\x0100\\x0001\\x0001: Let's see how good\\xfffEa Trainer you are!"
                    + "\\xf000\\xbe01\\x0000\\xfffEI'll use my Pok\\x00e9mon"
                    + "\\xfffethat I raised from an Egg!\\xf000\\xbe01\\x0000";
            }

            tf.CompressData();
            return true;
        }

        static string BuildStarterBallLabel(int speciesId, IReadOnlyList<PokemonEntry> pokemon,
            IList<string> typeNames, IList<string> pokeNames)
        {
            byte t1 = pokemon[speciesId].type1;
            string typeStr = (t1 >= 0 && t1 < typeNames.Count) ? typeNames[t1] : "?";
            string typeCamel = ToDisplayCase(typeStr);
            string name = (speciesId >= 0 && speciesId < pokeNames.Count) ? pokeNames[speciesId] : "?";
            return "\\xf000\\xbd02\\x0000The " + typeCamel + "-type Pok\\x00e9mon\\xfffe\\xf000\\xbd02\\x0000" + name;
        }

        static string ToDisplayCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length == 1) return s.ToUpperInvariant();
            return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
        }

        static byte[] RefBytesToArray(RefByte[] r)
        {
            var b = new byte[r.Length];
            for (int i = 0; i < r.Length; i++) b[i] = r[i];
            return b;
        }

        static void WriteRelativePointer(byte[] data, int offset, int absolutePointer)
        {
            int rel = absolutePointer - (offset + 4);
            HelperFunctions.WriteInt(data, offset, rel);
        }

        /// <summary>Returns start index, -1 if none, -2 if multiple.</summary>
        static int FindUniqueHex(byte[] data, string hex)
        {
            if (hex.Length % 2 != 0) return -1;
            var pat = new byte[hex.Length / 2];
            for (int i = 0; i < pat.Length; i++)
                pat[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            int hits = 0;
            int found = -1;
            for (int i = 0; i <= data.Length - pat.Length; i++)
            {
                bool ok = true;
                for (int j = 0; j < pat.Length; j++)
                {
                    if (data[i + j] != pat[j])
                    {
                        ok = false;
                        break;
                    }
                }

                if (!ok) continue;
                hits++;
                found = i;
                if (hits > 1) return -2;
            }

            return hits == 1 ? found : -1;
        }

        static int IndexOf(byte[] data, byte[] pat)
        {
            if (pat.Length == 0 || data.Length < pat.Length) return -1;
            for (int i = 0; i <= data.Length - pat.Length; i++)
            {
                bool ok = true;
                for (int j = 0; j < pat.Length; j++)
                {
                    if (data[i + j] != pat[j])
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok) return i;
            }

            return -1;
        }
    }
}
