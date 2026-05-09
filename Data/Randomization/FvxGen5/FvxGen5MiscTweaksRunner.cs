using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Misc tweaks runner; Fastest Text / National Dex IPS files match UPR-FVX <c>Patches/...</c> layout.</summary>
    internal static class FvxGen5MiscTweaksRunner
    {
        const string RunningShoesPrefixHex = "01D0012008BD002008BD63";
        const string ForceChallengeModeLocatorHex = "816A406B0B1C07490022434090000858834201D1";
        const string LowHealthMusicLocatorHex = "00D10127";
        const string ForgettableHmsBeforeHex = "084A002359005118B8310988884201D101207047591C09060B0E062BF2D300207047C046";
        const string ForgettableHmsAfterHex = "000000000000000000000000000000000000000000000000000000000000002070470000000000";
        const int ScriptListTerminator = 0xFD13;
        const int HiddenItemSetVarCommand = 0x2A;
        const int HiddenItemVarSet = 0x8000;

        sealed class MiscAction
        {
            public int Priority { get; set; }
            public Func<string> Apply { get; set; }
        }

        public static bool TryApply(FvxMiscTweaksOptions opt, Random rnd, out string error)
        {
            _ = rnd;
            error = null;
            if (MainEditor.fileSystem == null)
            {
                error = "No file system loaded.";
                return false;
            }

            NormalizeForCapabilities(opt);
            var actions = BuildActions(opt);
            foreach (var action in actions.OrderByDescending(a => a.Priority))
            {
                var stepError = action.Apply();
                if (!string.IsNullOrEmpty(stepError))
                {
                    error = stepError;
                    return false;
                }
            }

            return true;
        }

        static void NormalizeForCapabilities(FvxMiscTweaksOptions opt)
        {
            bool bw2 = MainEditor.RomType == RomType.BW2;
            bool bw1 = MainEditor.RomType == RomType.BW1;
            if (!bw1 && !bw2)
            {
                opt.ForceChallengeMode = false;
                opt.BalanceStaticLevels = false;
            }
            else if (bw1)
            {
                opt.ForceChallengeMode = false;
            }
            else
            {
                opt.BalanceStaticLevels = false;
            }

            if (!CanApplyFastestTextPatch())
                opt.FastestText = false;
            if (!CanApplyNationalDexPatch())
                opt.NationalDexAtStart = false;
            opt.RandomizeCatchingTutorial = false;
            opt.RandomizePcPotion = false;
        }

        static List<MiscAction> BuildActions(FvxMiscTweaksOptions opt)
        {
            var list = new List<MiscAction>();
            if (opt.BanLuckyEgg)
                list.Add(new MiscAction { Priority = 1, Apply = ApplyBanLuckyEgg });
            if (opt.BanBigMoneyManiacItems)
                list.Add(new MiscAction { Priority = 1, Apply = ApplyBanBigMoneyManiacItems });
            if (opt.NoFreeLuckyEgg)
                list.Add(new MiscAction { Priority = 0, Apply = ApplyNoFreeLuckyEgg });
            if (opt.FastEggHatching)
                list.Add(new MiscAction { Priority = 0, Apply = ApplyFastEggHatching });
            if (opt.RunWithoutRunningShoes)
                list.Add(new MiscAction { Priority = 0, Apply = ApplyRunWithoutRunningShoes });
            if (opt.DisableLowHpMusic)
                list.Add(new MiscAction { Priority = 0, Apply = ApplyDisableLowHpMusic });
            if (opt.ForgettableHms)
                list.Add(new MiscAction { Priority = 0, Apply = ApplyForgettableHms });
            if (opt.ForceChallengeMode)
                list.Add(new MiscAction { Priority = 0, Apply = ApplyForceChallengeMode });
            if (opt.BalanceStaticLevels)
                list.Add(new MiscAction { Priority = 0, Apply = ApplyBalanceStaticLevels });
            if (opt.FastestText)
                list.Add(new MiscAction { Priority = 0, Apply = ApplyFastestText });
            if (opt.NationalDexAtStart)
                list.Add(new MiscAction { Priority = 0, Apply = ApplyNationalDexAtStart });
            return list;
        }

        public static bool CanApplyFastestTextPatch()
            => !string.IsNullOrEmpty(ResolvePatchPath(GetFastestTextPatchName()));

        public static bool CanApplyNationalDexPatch()
            => !string.IsNullOrEmpty(ResolvePatchPath(GetNationalDexPatchName()));

        static string ApplyBanLuckyEgg()
        {
            int luckyEgg = FindItemByNameContains("lucky egg");
            if (luckyEgg <= 0)
                return "Could not find Lucky Egg item.";
            FvxGen5MiscRuntimeState.BanItem(luckyEgg);
            return null;
        }

        static string ApplyBanBigMoneyManiacItems()
        {
            string[] tokens =
            {
                "balm mushroom",
                "big nugget",
                "pearl string",
                "comet shard",
                "relic vase",
                "relic band",
                "relic statue",
                "relic crown",
                "lansat berry",
                "starf berry",
                "enigma berry",
                "micle berry",
                "custap berry",
                "jaboca berry",
                "rowap berry"
            };
            foreach (var token in tokens)
            {
                int id = FindItemByNameContains(token);
                if (id > 0)
                    FvxGen5MiscRuntimeState.BanItem(id);
            }
            return null;
        }

        static string ApplyNoFreeLuckyEgg()
        {
            if (MainEditor.scriptNarc?.scriptFiles == null)
                return "Script data is not loaded.";

            int scriptFile = MainEditor.RomType == RomType.BW2
                ? FvxGen5RomOffsets.LuckyEggScriptBw2
                : MainEditor.RomType == RomType.BW1
                    ? FvxGen5RomOffsets.LuckyEggScriptBw1
                    : -1;
            if (scriptFile < 0 || scriptFile >= MainEditor.scriptNarc.scriptFiles.Count)
                return "Lucky Egg gift script offset is out of range for this ROM.";

            int luckyEgg = FindItemByNameContains("lucky egg");
            int gooeyMulch = FindItemByNameContains("gooey mulch");
            if (luckyEgg <= 0 || gooeyMulch <= 0)
                return "Could not find Lucky Egg or Gooey Mulch item IDs.";

            var sf = MainEditor.scriptNarc.scriptFiles[scriptFile];
            var bytes = sf.bytes;
            int hitsWanted = MainEditor.RomType == RomType.BW2 ? 2 : 1;
            int changed = 0;
            int pos = 0;
            while (pos < bytes.Length - 4 && changed < hitsWanted && HelperFunctions.ReadShort(bytes, pos) != ScriptListTerminator)
            {
                int offsetInFile = HelperFunctions.ReadInt(bytes, pos) + pos + 4;
                pos += 4;
                if (offsetInFile > bytes.Length)
                    break;

                while (true)
                {
                    offsetInFile++;
                    if (offsetInFile >= bytes.Length)
                        break;
                    int b = (byte)bytes[offsetInFile];
                    if (b == HiddenItemSetVarCommand && offsetInFile + 5 < bytes.Length)
                    {
                        int command = HelperFunctions.ReadShort(bytes, offsetInFile);
                        int variable = HelperFunctions.ReadShort(bytes, offsetInFile + 2);
                        int item = HelperFunctions.ReadShort(bytes, offsetInFile + 4);
                        if (command == HiddenItemSetVarCommand && variable == HiddenItemVarSet && item == luckyEgg)
                        {
                            HelperFunctions.WriteShort(bytes, offsetInFile + 4, gooeyMulch);
                            changed++;
                        }
                    }
                    if (b == 0x2E)
                        break;
                }
            }

            sf.ApplyData();
            if (changed < hitsWanted)
                Debug.WriteLine(
                    $"No Free Lucky Egg: replaced {changed} of {hitsWanted} gift(s); ROM layout may differ from vanilla.");
            return null;
        }

        static string ApplyFastEggHatching()
        {
            var pokemon = MainEditor.pokemonDataNarc?.pokemon;
            if (pokemon == null)
                return "Pokemon personal data is not loaded.";

            foreach (var p in pokemon)
            {
                if (p == null) continue;
                p.hatchCounter = 1;
                p.ApplyData();
            }
            return null;
        }

        static string ApplyRunWithoutRunningShoes()
        {
            int ovl = MainEditor.RomType == RomType.BW2
                ? FvxGen5RomOffsets.FieldMovementOverlayBw2
                : MainEditor.RomType == RomType.BW1
                    ? FvxGen5RomOffsets.FieldMovementOverlayBw1
                    : -1;
            if (ovl < 0)
                return "Run without running shoes is only available on BW/BW2.";
            return TryPatchOverlayByLocator(ovl, RunningShoesPrefixHex, 0, new byte[] { 0x00, 0x00 });
        }

        static string ApplyDisableLowHpMusic()
        {
            int ovl = MainEditor.RomType == RomType.BW2
                ? FvxGen5RomOffsets.LowHpMusicOverlayBw2
                : MainEditor.RomType == RomType.BW1
                    ? FvxGen5RomOffsets.LowHpMusicOverlayBw1
                    : -1;
            if (ovl < 0)
                return "Disable low HP music is only available on BW/BW2.";
            return TryPatchOverlayByLocator(ovl, LowHealthMusicLocatorHex, 1, new byte[] { 0xE0 });
        }

        static string ApplyForgettableHms()
        {
            var arm9 = MainEditor.fileSystem?.arm9;
            if (arm9 == null)
                return "ARM9 data is not loaded.";

            bool wasCompressed = arm9.Count < 600000;
            byte[] work = wasCompressed ? BLZDecoder.BLZ_DecodePub(arm9.ToArray()) : arm9.ToArray();
            if (work == null || work.Length == 0)
                return "ARM9 could not be read.";

            byte[] before = HexToBytes(ForgettableHmsBeforeHex);
            byte[] after = HexToBytes(ForgettableHmsAfterHex);
            int idx = IndexOf(work, before);
            if (idx < 0)
                return "Forgettable HMs patch location was not found.";
            if (idx + after.Length > work.Length)
                return "Forgettable HMs patch exceeds ARM9 size.";
            Buffer.BlockCopy(after, 0, work, idx, after.Length);
            WriteArm9(arm9, work, wasCompressed);
            return null;
        }

        static string ApplyForceChallengeMode()
        {
            if (MainEditor.RomType != RomType.BW2)
                return null;

            var arm9 = MainEditor.fileSystem?.arm9;
            if (arm9 == null)
                return "ARM9 data is not loaded.";
            bool wasCompressed = arm9.Count < 600000;
            byte[] work = wasCompressed ? BLZDecoder.BLZ_DecodePub(arm9.ToArray()) : arm9.ToArray();
            if (work == null || work.Length == 0)
                return "ARM9 could not be read.";

            int idx = IndexOf(work, HexToBytes(ForceChallengeModeLocatorHex));
            if (idx < 0 || idx + 4 > work.Length)
                return "Force Challenge Mode locator was not found.";
            work[idx] = 0x02;
            work[idx + 1] = 0x20;
            work[idx + 2] = 0x70;
            work[idx + 3] = 0x47;
            WriteArm9(arm9, work, wasCompressed);
            return null;
        }

        static string ApplyBalanceStaticLevels()
        {
            if (MainEditor.RomType != RomType.BW1)
                return null;
            if (MainEditor.scriptNarc?.scriptFiles == null
                || MainEditor.scriptNarc.scriptFiles.Count <= FvxGen5RomOffsets.FossilPokemonStaticScriptBw1)
                return "Static script data is not loaded.";
            var fossil = MainEditor.scriptNarc.scriptFiles[FvxGen5RomOffsets.FossilPokemonStaticScriptBw1];
            if (fossil.bytes == null || fossil.bytes.Length < 0x3F9)
                return "Fossil static script layout is not available.";
            HelperFunctions.WriteShort(fossil.bytes, FvxGen5RomOffsets.FossilPokemonLevelWriteOffset, 20);
            fossil.ApplyData();
            return null;
        }

        static string ApplyFastestText()
        {
            string patchPath = ResolvePatchPath(GetFastestTextPatchName());
            if (string.IsNullOrEmpty(patchPath))
                return "Fastest Text patch is unavailable.";

            var arm9 = MainEditor.fileSystem?.arm9;
            if (arm9 == null)
                return "ARM9 data is not loaded.";
            bool wasCompressed = arm9.Count < 600000;
            byte[] work = wasCompressed ? BLZDecoder.BLZ_DecodePub(arm9.ToArray()) : arm9.ToArray();
            if (work == null || work.Length == 0)
                return "ARM9 could not be read.";
            if (!TryApplyIpsPatch(work, patchPath, out var patchErr))
                return "Fastest Text patch failed: " + patchErr;
            WriteArm9(arm9, work, wasCompressed);
            return null;
        }

        static string ApplyNationalDexAtStart()
        {
            string patchPath = ResolvePatchPath(GetNationalDexPatchName());
            if (string.IsNullOrEmpty(patchPath))
                return "National Dex patch is unavailable.";
            if (MainEditor.scriptNarc?.scriptFiles == null)
                return "Script data is not loaded.";

            int scriptId = MainEditor.RomType == RomType.BW2
                ? FvxGen5RomOffsets.NationalDexScriptBw2
                : MainEditor.RomType == RomType.BW1
                    ? FvxGen5RomOffsets.NationalDexScriptBw1
                    : -1;
            if (scriptId < 0 || scriptId >= MainEditor.scriptNarc.scriptFiles.Count)
                return "National Dex script offset is out of range for this ROM.";

            var script = MainEditor.scriptNarc.scriptFiles[scriptId];
            var expanded = new byte[(script.bytes?.Length ?? 0) + 4];
            if (script.bytes != null && script.bytes.Length > 0)
                Buffer.BlockCopy(script.bytes, 0, expanded, 0, script.bytes.Length);
            if (!TryApplyIpsPatch(expanded, patchPath, out var patchErr))
                return "National Dex patch failed: " + patchErr;
            script.bytes = expanded.Select(b => (RefByte)b).ToArray();
            script.ApplyData();
            return null;
        }

        static string TryPatchOverlayByLocator(int overlayIndex, string locatorHex, int patchOffset, byte[] replacement)
        {
            var fs = MainEditor.fileSystem;
            if (fs?.overlays == null || overlayIndex < 0 || overlayIndex >= fs.overlays.Count)
                return "Required overlay is unavailable for this ROM.";

            var y9 = fs.y9?.entries;
            bool wasCompressed = y9 != null && overlayIndex < y9.Count && y9[overlayIndex].compressed;
            byte[] raw = fs.overlays[overlayIndex].ToArray();
            byte[] work = wasCompressed ? BLZDecoder.BLZ_DecodePub(raw) : raw;
            if (work == null || work.Length == 0)
                return "Overlay could not be read.";

            int idx = IndexOf(work, HexToBytes(locatorHex));
            int at = idx + patchOffset;
            if (idx < 0 || at < 0 || at + replacement.Length > work.Length)
                return "Overlay patch locator was not found.";

            for (int i = 0; i < replacement.Length; i++)
                work[at + i] = replacement[i];

            if (wasCompressed)
            {
                byte[] enc = BLZDecoder.BLZ_EncodePub(work, true);
                if (enc == null || enc.Length == 0)
                    return "Overlay recompression failed.";
                fs.overlays[overlayIndex] = new List<byte>(enc);
                if (y9 != null && overlayIndex < y9.Count)
                {
                    y9[overlayIndex].compressed = true;
                    y9[overlayIndex].compressedSize = enc.Length;
                    y9[overlayIndex].Apply();
                }
            }
            else
            {
                fs.overlays[overlayIndex] = new List<byte>(work);
            }

            return null;
        }

        static void WriteArm9(List<byte> arm9, byte[] work, bool wasCompressed)
        {
            arm9.Clear();
            arm9.AddRange(wasCompressed ? BLZDecoder.BLZ_EncodePub(work, false) : work);
        }

        static int FindItemByNameContains(string token)
        {
            var names = MainEditor.textNarc?.textFiles?[VersionConstants.ItemNameTextFileID]?.text;
            if (names == null) return -1;
            string low = token.ToLowerInvariant();
            for (int i = 1; i < names.Count; i++)
            {
                string nm = (names[i] ?? "").ToLowerInvariant();
                if (nm.Contains(low))
                    return i;
            }
            return -1;
        }

        static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return Array.Empty<byte>();
            string cleaned = new string(hex.Where(c => Uri.IsHexDigit(c)).ToArray());
            if ((cleaned.Length & 1) != 0)
                return Array.Empty<byte>();
            var bytes = new byte[cleaned.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(cleaned.Substring(i * 2, 2), 16);
            return bytes;
        }

        static int IndexOf(byte[] haystack, byte[] needle)
        {
            if (haystack == null || needle == null || needle.Length == 0 || haystack.Length < needle.Length)
                return -1;
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                bool ok = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok) return i;
            }
            return -1;
        }

        static string GetFastestTextPatchName()
        {
            bool bw2 = MainEditor.RomType == RomType.BW2;
            bool black = MainEditor.BlackVersion;
            if (bw2) return black ? "instant_text/b2_instant_text" : "instant_text/w2_instant_text";
            return black ? "instant_text/b1_instant_text" : "instant_text/w1_instant_text";
        }

        static string GetNationalDexPatchName()
            => MainEditor.RomType == RomType.BW2 ? "national_dex/bw2_national_dex" : "national_dex/bw1_national_dex";

        static string ResolvePatchPath(string patchName)
        {
            if (string.IsNullOrWhiteSpace(patchName))
                return null;
            string suffix = patchName + ".ips";
            var roots = new List<string> { AppDomain.CurrentDomain.BaseDirectory };
            string asmDir = Path.GetDirectoryName(typeof(FvxGen5MiscTweaksRunner).Assembly.Location);
            if (!string.IsNullOrEmpty(asmDir))
                roots.Add(asmDir);

            foreach (string baseDir in roots.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                foreach (string rel in new[] { Path.Combine("Patches", suffix), Path.Combine("patches", suffix) })
                {
                    string c = Path.Combine(baseDir, rel);
                    if (File.Exists(c))
                        return c;
                }
            }
            return null;
        }

        static bool TryApplyIpsPatch(byte[] target, string ipsPath, out string error)
        {
            error = null;
            byte[] patch;
            try { patch = File.ReadAllBytes(ipsPath); }
            catch (Exception ex) { error = ex.Message; return false; }
            if (patch.Length < 8) { error = "Patch file is too small."; return false; }
            if (patch[0] != 'P' || patch[1] != 'A' || patch[2] != 'T' || patch[3] != 'C' || patch[4] != 'H')
            {
                error = "Patch header is invalid.";
                return false;
            }

            int pos = 5;
            while (pos + 3 <= patch.Length)
            {
                if (pos + 3 <= patch.Length && patch[pos] == 'E' && patch[pos + 1] == 'O' && patch[pos + 2] == 'F')
                    return true;
                if (pos + 5 > patch.Length) { error = "Patch truncated."; return false; }
                int offset = (patch[pos] << 16) | (patch[pos + 1] << 8) | patch[pos + 2];
                int size = (patch[pos + 3] << 8) | patch[pos + 4];
                pos += 5;
                if (size == 0)
                {
                    if (pos + 3 > patch.Length) { error = "Patch RLE truncated."; return false; }
                    int rleSize = (patch[pos] << 8) | patch[pos + 1];
                    byte value = patch[pos + 2];
                    pos += 3;
                    if (offset < 0 || offset + rleSize > target.Length) { error = "Patch write exceeds target size."; return false; }
                    for (int i = 0; i < rleSize; i++) target[offset + i] = value;
                }
                else
                {
                    if (pos + size > patch.Length) { error = "Patch payload truncated."; return false; }
                    if (offset < 0 || offset + size > target.Length) { error = "Patch write exceeds target size."; return false; }
                    Buffer.BlockCopy(patch, pos, target, offset, size);
                    pos += size;
                }
            }

            error = "Patch EOF marker not found.";
            return false;
        }
    }
}
