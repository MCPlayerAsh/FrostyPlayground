using System;
using System.IO;
using System.Text;
using NewEditor.Data;
using NewEditor.Data.NARCTypes;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Gen 5 misc tweaks: HM forgettable + challenge mode + fast eggs (native); fastest text + national dex (optional IPS under Patches/).</summary>
    internal static class FvxGen5MiscTweaks
    {
        static readonly byte[] HmsForgettableBefore = HexToBytes("084A002359005118B8310988884201D101207047591C09060B0E062BF2D300207047C046");
        static readonly byte[] HmsForgettableAfter = HexToBytes("0000000000000000000000000000000000000000000000000000000000000000207047000000000000");

        static readonly byte[] ForceChallengeModeLocator = HexToBytes("816A406B0B1C07490022434090000858834201D1");

        public static void Apply(FvxMiscTweaksSettings misc, StringBuilder log)
        {
            if (misc == null) return;
            misc.GiveNationalDexAtStart = false;
            if (!misc.FastestText && !misc.GiveNationalDexAtStart && !misc.FastEggHatching
                && !misc.ForceChallengeMode && !misc.ForgettableHms)
                return;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            if (misc.ForgettableHms)
            {
                if (TryApplyForgettableHms(log)) { }
                else log.AppendLine("[Misc] Forgettable HMs: could not apply (offset or expected bytes mismatch).");
            }

            if (misc.ForceChallengeMode)
            {
                if (MainEditor.RomType != RomType.BW2)
                    log.AppendLine("[Misc] Force Challenge Mode: BW2 only — skipped.");
                else if (!TryApplyForceChallengeMode(log))
                    log.AppendLine("[Misc] Force Challenge Mode: locator not found in ARM9.");
            }

            if (misc.FastEggHatching)
                ApplyFastEggHatching(log);

            if (misc.FastestText)
            {
                if (FvxGen5UsRomTables.TryGetMiscIpsRelativePaths(MainEditor.RomTypeId, out string ftRel, out _))
                {
                    if (!TryApplyArm9Ips(Path.Combine(baseDir, "Patches", ftRel), log))
                        log.AppendLine("[Misc] Fastest text: IPS missing or invalid — expected Patches\\" + ftRel);
                }
                else log.AppendLine("[Misc] Fastest text: unsupported ROM id.");
            }

            if (misc.GiveNationalDexAtStart)
            {
                if (FvxGen5UsRomTables.TryGetMiscIpsRelativePaths(MainEditor.RomTypeId, out _, out string ndRel))
                {
                    if (!TryApplyNationalDexAtStart(Path.Combine(baseDir, "Patches", ndRel), log))
                        log.AppendLine("[Misc] National Dex at start: IPS missing/invalid or script index out of range — expected Patches\\" + ndRel);
                }
                else log.AppendLine("[Misc] National Dex at start: unsupported ROM id.");
            }
        }

        static bool TryApplyForgettableHms(StringBuilder log)
        {
            if (MainEditor.fileSystem?.arm9 == null) return false;
            if (!FvxGen5UsRomTables.TryGetHmForgettableArm9Offset(MainEditor.RomTypeId, out int off))
                return false;

            bool compressed = MainEditor.fileSystem.arm9.Count < 600000;
            byte[] raw = compressed ? BLZDecoder.BLZ_DecodePub(MainEditor.fileSystem.arm9.ToArray()) : MainEditor.fileSystem.arm9.ToArray();
            if (off < 0 || off + HmsForgettableBefore.Length > raw.Length) return false;

            for (int i = 0; i < HmsForgettableBefore.Length; i++)
            {
                if (raw[off + i] != HmsForgettableBefore[i])
                    return false;
            }

            if (off + HmsForgettableAfter.Length > raw.Length) return false;
            for (int i = 0; i < HmsForgettableAfter.Length; i++)
                raw[off + i] = HmsForgettableAfter[i];

            MainEditor.fileSystem.arm9.Clear();
            MainEditor.fileSystem.arm9.AddRange(compressed ? BLZDecoder.BLZ_EncodePub(raw, false) : raw);
            log?.AppendLine("[Misc] Forgettable HMs: patched ARM9.");
            return true;
        }

        static bool TryApplyForceChallengeMode(StringBuilder log)
        {
            if (MainEditor.fileSystem?.arm9 == null) return false;
            bool compressed = MainEditor.fileSystem.arm9.Count < 600000;
            byte[] raw = compressed ? BLZDecoder.BLZ_DecodePub(MainEditor.fileSystem.arm9.ToArray()) : MainEditor.fileSystem.arm9.ToArray();
            int idx = IndexOf(raw, ForceChallengeModeLocator);
            if (idx < 0) return false;

            raw[idx] = 0x02;
            raw[idx + 1] = 0x20;
            raw[idx + 2] = 0x70;
            raw[idx + 3] = 0x47;

            MainEditor.fileSystem.arm9.Clear();
            MainEditor.fileSystem.arm9.AddRange(compressed ? BLZDecoder.BLZ_EncodePub(raw, false) : raw);
            log?.AppendLine("[Misc] Force Challenge Mode: patched ARM9.");
            return true;
        }

        static void ApplyFastEggHatching(StringBuilder log)
        {
            if (MainEditor.pokemonDataNarc?.pokemon == null) return;
            int n = 0;
            for (int i = 1; i < MainEditor.pokemonDataNarc.pokemon.Count; i++)
            {
                var p = MainEditor.pokemonDataNarc.pokemon[i];
                if (p == null) continue;
                p.hatchCounter = 1;
                p.ApplyData();
                n++;
            }
            log?.AppendLine("[Misc] Fast egg hatching: set hatch counter to 1 for " + n + " species.");
        }

        static bool TryApplyArm9Ips(string path, StringBuilder log)
        {
            if (!File.Exists(path) || MainEditor.fileSystem?.arm9 == null) return false;
            byte[] ips = File.ReadAllBytes(path);
            bool compressed = MainEditor.fileSystem.arm9.Count < 600000;
            byte[] raw = compressed ? BLZDecoder.BLZ_DecodePub(MainEditor.fileSystem.arm9.ToArray()) : MainEditor.fileSystem.arm9.ToArray();
            var scratch = new StringBuilder();
            if (!FvxGen5Ips.TryApply(raw, ips, scratch))
            {
                log?.AppendLine("[Misc] IPS apply failed: " + scratch);
                return false;
            }
            MainEditor.fileSystem.arm9.Clear();
            MainEditor.fileSystem.arm9.AddRange(compressed ? BLZDecoder.BLZ_EncodePub(raw, false) : raw);
            log?.AppendLine("[Misc] Applied ARM9 IPS: " + Path.GetFileName(path));
            return true;
        }

        static bool TryApplyNationalDexAtStart(string ipsPath, StringBuilder log)
        {
            if (!File.Exists(ipsPath)) return false;
            if (MainEditor.scriptNarc?.scriptFiles == null) return false;
            if (!FvxGen5UsRomTables.TryGetNationalDexScriptNarcIndex(MainEditor.RomTypeId, out int narcIdx)) return false;
            if (narcIdx < 0 || narcIdx >= MainEditor.scriptNarc.scriptFiles.Count) return false;

            var sf = MainEditor.scriptNarc.scriptFiles[narcIdx];
            if (sf?.bytes == null) return false;

            var orig = new byte[sf.bytes.Length];
            for (int i = 0; i < orig.Length; i++) orig[i] = sf.bytes[i];

            var expanded = new byte[orig.Length + 4];
            Buffer.BlockCopy(orig, 0, expanded, 0, orig.Length);

            byte[] ips = File.ReadAllBytes(ipsPath);
            var scratch = new StringBuilder();
            if (!FvxGen5Ips.TryApply(expanded, ips, scratch))
            {
                log?.AppendLine("[Misc] National Dex IPS failed: " + scratch);
                return false;
            }

            sf.bytes = new RefByte[expanded.Length];
            for (int i = 0; i < expanded.Length; i++) sf.bytes[i] = expanded[i];
            log?.AppendLine("[Misc] National Dex at start: applied IPS to script file " + narcIdx + ".");
            return true;
        }

        static byte[] HexToBytes(string hex)
        {
            hex = hex.Replace(" ", "");
            var b = new byte[hex.Length / 2];
            for (int i = 0; i < b.Length; i++)
                b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return b;
        }

        static int IndexOf(byte[] data, byte[] pattern)
        {
            if (pattern.Length == 0 || data.Length < pattern.Length) return -1;
            for (int i = 0; i <= data.Length - pattern.Length; i++)
            {
                int j = 0;
                while (j < pattern.Length && data[i + j] == pattern[j]) j++;
                if (j == pattern.Length) return i;
            }
            return -1;
        }
    }
}
