using System;
using System.Collections.Generic;
using NewEditor.Data;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Ports UPR-FVX <c>Gen5RomHandler.setIntroPokemon</c> overlay patches (species must be base forme).</summary>
    public static class FvxIntroMonRunner
    {
        static readonly byte[] IntroGraphicPrefix = ParseHex("5A0000010000001700000001000000");
        static readonly byte[] Bw1IntroCryPrefix = ParseHex("0021009101910291");
        static readonly byte[] Bw2IntroCryLocator = ParseHex("3D020000F8B51C1C");

        public static bool TryApply(int nationalSpeciesId, out string error)
        {
            error = null;
            if (MainEditor.fileSystem?.overlays == null)
            {
                error = "ROM overlays are not loaded.";
                return false;
            }
            if (nationalSpeciesId < 1 || nationalSpeciesId > 65535)
            {
                error = "Invalid intro species.";
                return false;
            }

            bool bw2 = MainEditor.RomType == RomType.BW2;
            int graphicOvl = bw2 ? FvxGen5RomOffsets.IntroGraphicOverlayBw2 : FvxGen5RomOffsets.IntroGraphicOverlayBw1;
            int cryOvl = bw2 ? FvxGen5RomOffsets.IntroCryOverlayBw2 : FvxGen5RomOffsets.IntroCryOverlayBw1;

            var overlays = MainEditor.fileSystem.overlays;
            if (graphicOvl < 0 || graphicOvl >= overlays.Count || cryOvl < 0 || cryOvl >= overlays.Count)
            {
                error = "Intro overlay index out of range for this ROM.";
                return false;
            }

            var introGraphic = overlays[graphicOvl];
            int gOff = FindBytes(introGraphic, IntroGraphicPrefix);
            if (gOff < 0)
            {
                error = "Could not locate intro graphic patch point.";
                return false;
            }

            int wordOff = gOff + IntroGraphicPrefix.Length;
            WriteLe16(introGraphic, wordOff, nationalSpeciesId);

            var introCry = overlays[cryOvl];
            if (bw2)
            {
                int cOff = FindBytes(introCry, Bw2IntroCryLocator);
                if (cOff < 0)
                {
                    error = "Could not locate BW2 intro cry patch point.";
                    return false;
                }
                WriteLe16(introCry, cOff, nationalSpeciesId);
            }
            else
            {
                int cOff = FindBytes(introCry, Bw1IntroCryPrefix);
                if (cOff < 0)
                {
                    error = "Could not locate BW1 intro cry patch point.";
                    return false;
                }
                cOff += Bw1IntroCryPrefix.Length;
                ApplyBw1IntroCryRewrite(introCry, cOff, nationalSpeciesId);
            }

            return true;
        }

        /// <summary>Matches UPR's block move + pc-relative species constant for BW1 intro cry overlay.</summary>
        static void ApplyBw1IntroCryRewrite(List<byte> introCry, int offset, int species)
        {
            for (int i = offset + 6; i < offset + 40 && i < introCry.Count; i++)
                introCry[i - 2] = introCry[i];

            if (offset + 10 < introCry.Count)
                introCry[offset + 10]++;

            if (offset + 38 + 4 <= introCry.Count)
                HelperFunctions.WriteInt(introCry, offset + 38, species);

            if (offset + 1 < introCry.Count)
            {
                introCry[offset] = 0x09;
                introCry[offset + 1] = 0x48;
            }
        }

        static void WriteLe16(List<byte> buf, int offset, int value)
        {
            if (offset + 1 >= buf.Count) return;
            buf[offset] = (byte)(value & 0xFF);
            buf[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        static int FindBytes(IReadOnlyList<byte> data, byte[] pat)
        {
            if (data == null || pat == null || pat.Length == 0) return -1;
            for (int i = 0; i <= data.Count - pat.Length; i++)
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

        static byte[] ParseHex(string hex)
        {
            if ((hex.Length & 1) != 0) throw new ArgumentException("hex");
            var b = new byte[hex.Length / 2];
            for (int i = 0; i < b.Length; i++)
                b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return b;
        }
    }
}
