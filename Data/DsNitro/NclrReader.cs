using System;
using System.Drawing;
using NewEditor.Data;

namespace NewEditor.Data.DsNitro
{
    /// <summary>Parses NCLR (palette) Nitro files after optional LZ decompress.</summary>
    public static class NclrReader
    {
        static bool IsNclrMagic(byte[] d, int off)
        {
            if (d == null || off + 4 > d.Length) return false;
            string m = System.Text.Encoding.ASCII.GetString(d, off, 4);
            return m == "NCLR" || m == "RLCN";
        }

        public static bool TryReadPalette(byte[] data, out Color[] palette)
        {
            palette = null;
            if (data == null || data.Length < 0x30) return false;
            if (!IsNclrMagic(data, 0)) return false;

            var chunks = NitroChunkReader.ReadChunks(data);
            foreach (var ch in chunks)
            {
                if (ch.Id != "TTLP" && ch.Id != "PLTT" && ch.Id != "PCSN") continue;
                var colors = ParsePlttPayload(data, ch.DataStart, ch.DataLength);
                if (colors != null && colors.Length > 0)
                {
                    palette = colors;
                    return true;
                }
            }

            // Fallback: scan for PLTT magic inside file (some packed layouts)
            int pltt = IndexOfAscii(data, "PLTT");
            if (pltt >= 0 && pltt + 12 < data.Length)
            {
                int len = ReadInt32(data, pltt + 4);
                var colors = ParsePlttPayload(data, pltt + 8, len);
                if (colors != null && colors.Length > 0)
                {
                    palette = colors;
                    return true;
                }
            }
            return false;
        }

        static int IndexOfAscii(byte[] d, string s)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(s);
            for (int i = 0; i <= d.Length - bytes.Length; i++)
            {
                int j = 0;
                while (j < bytes.Length && d[i + j] == bytes[j]) j++;
                if (j == bytes.Length) return i;
            }
            return -1;
        }

        static Color[] ParsePlttPayload(byte[] data, int start, int length)
        {
            if (start + 4 > data.Length) return null;
            for (int skip = 0; skip <= 4; skip += 2)
            {
                if (start + skip + 4 > data.Length) break;
                int colorCount = ReadUInt16(data, start + skip);
                if (colorCount <= 0 || colorCount > 256) continue;
                int colorBytes = colorCount * 2;
                int off = start + skip + 2;
                if (off + colorBytes > data.Length || colorBytes > length - skip) continue;

                var pal = new Color[Math.Max(16, colorCount)];
                for (int i = 0; i < colorCount; i++)
                {
                    int v = ReadUInt16(data, off + i * 2);
                    pal[i] = DsDecmp.Read16BitColor(v);
                }
                for (int i = colorCount; i < pal.Length; i++) pal[i] = pal[0];
                return pal;
            }
            if (length >= 32 && start + 32 <= data.Length)
            {
                int n = Math.Min(16, length / 2);
                var pal = new Color[16];
                for (int i = 0; i < n; i++)
                    pal[i] = DsDecmp.Read16BitColor(ReadUInt16(data, start + i * 2));
                for (int i = n; i < 16; i++) pal[i] = pal[0];
                return pal;
            }
            return null;
        }

        static int ReadUInt16(byte[] d, int o) => (d[o] & 0xFF) | ((d[o + 1] & 0xFF) << 8);
        static int ReadInt32(byte[] d, int o) =>
            (d[o] & 0xFF) | ((d[o + 1] & 0xFF) << 8) | ((d[o + 2] & 0xFF) << 16) | ((d[o + 3] & 0xFF) << 24);
    }
}
