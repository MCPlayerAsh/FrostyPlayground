using System;
using System.Drawing;
using NewEditor.Data;

namespace NewEditor.Data.DsNitro
{
    /// <summary>Parses NCGR / RGCN character graphics (4bpp tiles).</summary>
    public static class NcgrReader
    {
        static bool IsNcgrMagic(byte[] d, int off)
        {
            if (d == null || off + 4 > d.Length) return false;
            string m = System.Text.Encoding.ASCII.GetString(d, off, 4);
            return m == "NCGR" || m == "RGCN" || m == "NCGC";
        }

        /// <summary>Decode tile bitmap; uses index 0 as transparent in output.</summary>
        public static Bitmap TryDecodeToBitmap(byte[] ncgrData, Color[] palette)
        {
            if (ncgrData == null || palette == null || palette.Length < 16) return null;
            if (!IsNcgrMagic(ncgrData, 0)) return null;

            var chunks = NitroChunkReader.ReadChunks(ncgrData);
            var ch = default(NitroChunkReader.Chunk);
            foreach (var c in chunks)
            {
                if (c.Id == "CHAR" || c.Id == "RAHC")
                {
                    ch = c;
                    break;
                }
            }

            if (ch.DataLength <= 0) return null;

            int ds = ch.DataStart;
            int dl = ch.DataLength;
            if (ds + dl > ncgrData.Length) return null;

            if (!TryReadDimensionsHeader(ncgrData, out int widthPx, out int heightPx))
            {
                widthPx = heightPx = 0;
            }

            const int bpp = 4;
            const int bytesPerTile = 32;

            foreach (int skip in new[] { 8, 16, 24, 32 })
            {
                if (skip >= dl) continue;
                int tileDataLen = dl - skip;
                if (tileDataLen <= 0 || tileDataLen % bytesPerTile != 0) continue;

                byte[] tileBytes = new byte[tileDataLen];
                Array.Copy(ncgrData, ds + skip, tileBytes, 0, tileDataLen);

                int numTiles = tileDataLen / bytesPerTile;
                int wp = widthPx, hp = heightPx;
                if (wp <= 0 || hp <= 0)
                    InferSize(numTiles, out wp, out hp);

                var palArgb = new Color[palette.Length];
                Array.Copy(palette, palArgb, palette.Length);
                palArgb[0] = Color.FromArgb(0, palette[0].R, palette[0].G, palette[0].B);

                var bmp = DsDecmp.DrawTiledImage(tileBytes, palArgb, 0, wp, hp, 8, 8, bpp);
                if (bmp != null) return bmp;
            }

            return null;
        }

        static void InferSize(int numTiles, out int widthPx, out int heightPx)
        {
            widthPx = heightPx = 8;
            int side = (int)Math.Sqrt(numTiles);
            if (side * side == numTiles)
            {
                widthPx = heightPx = side * 8;
                return;
            }
            for (int tw = 1; tw <= numTiles; tw++)
            {
                if (numTiles % tw != 0) continue;
                int th = numTiles / tw;
                int wp = tw * 8;
                int hp = th * 8;
                if (wp <= 128 && hp <= 128)
                {
                    widthPx = wp;
                    heightPx = hp;
                    return;
                }
            }
            widthPx = heightPx = Math.Max(8, side * 8);
        }

        static bool TryReadDimensionsHeader(byte[] data, out int widthPx, out int heightPx)
        {
            widthPx = heightPx = 0;
            if (data.Length < 0x24) return false;
            // Observed on some NCGR: pixel size at 0x14..0x17
            ushort w = (ushort)((data[0x14] & 0xFF) | ((data[0x15] & 0xFF) << 8));
            ushort h = (ushort)((data[0x16] & 0xFF) | ((data[0x17] & 0xFF) << 8));
            if (w > 0 && w <= 512 && h > 0 && h <= 512 && (w % 8) == 0 && (h % 8) == 0)
            {
                widthPx = w;
                heightPx = h;
                return true;
            }
            return false;
        }

    }
}
