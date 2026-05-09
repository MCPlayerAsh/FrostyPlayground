using System;
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

            // Overlays are often BLZ-compressed in ROM; search/patch must run on decoded bytes (see FvxGen5OverlayIo).
            if (!FvxGen5OverlayIo.TryMutateOverlay(graphicOvl, buf =>
            {
                int gOff = FindBytes(buf, IntroGraphicPrefix);
                if (gOff < 0) return "Could not locate intro graphic patch point.";
                WriteLe16(buf, gOff + IntroGraphicPrefix.Length, nationalSpeciesId);
                return null;
            }, out error))
                return false;

            if (!FvxGen5OverlayIo.TryMutateOverlay(cryOvl, buf =>
            {
                if (bw2)
                {
                    int cOff = FindBytes(buf, Bw2IntroCryLocator);
                    if (cOff < 0) return "Could not locate BW2 intro cry patch point.";
                    WriteLe16(buf, cOff, nationalSpeciesId);
                }
                else
                {
                    int cOff = FindBytes(buf, Bw1IntroCryPrefix);
                    if (cOff < 0) return "Could not locate BW1 intro cry patch point.";
                    ApplyBw1IntroCryRewrite(buf, cOff + Bw1IntroCryPrefix.Length, nationalSpeciesId);
                }
                return null;
            }, out error))
                return false;

            return true;
        }

        /// <summary>Matches UPR's block move + pc-relative species constant for BW1 intro cry overlay.</summary>
        static void ApplyBw1IntroCryRewrite(byte[] introCry, int offset, int species)
        {
            for (int i = offset + 6; i < offset + 40 && i < introCry.Length; i++)
                introCry[i - 2] = introCry[i];

            if (offset + 10 < introCry.Length)
                introCry[offset + 10]++;

            if (offset + 38 + 4 <= introCry.Length)
                HelperFunctions.WriteInt(introCry, offset + 38, species);

            if (offset + 1 < introCry.Length)
            {
                introCry[offset] = 0x09;
                introCry[offset + 1] = 0x48;
            }
        }

        static void WriteLe16(byte[] buf, int offset, int value)
        {
            if (offset < 0 || offset + 1 >= buf.Length) return;
            buf[offset] = (byte)(value & 0xFF);
            buf[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        static int FindBytes(byte[] data, byte[] pat)
        {
            if (data == null || pat == null || pat.Length == 0) return -1;
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
