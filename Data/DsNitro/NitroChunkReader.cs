using System;
using System.Collections.Generic;
using System.Text;

namespace NewEditor.Data.DsNitro
{
    internal static class NitroChunkReader
    {
        internal readonly struct Chunk
        {
            public readonly string Id;
            public readonly int Start;
            public readonly int DataStart;
            public readonly int DataLength;

            public Chunk(string id, int start, int dataStart, int dataLength)
            {
                Id = id;
                Start = start;
                DataStart = dataStart;
                DataLength = dataLength;
            }
        }

        /// <summary>Enumerate Nitro chunks starting after the 16-byte file header.</summary>
        public static List<Chunk> ReadChunks(byte[] data)
        {
            var list = new List<Chunk>();
            if (data == null || data.Length < 28) return list;

            int pos = 0x10;
            while (pos + 8 <= data.Length)
            {
                string id = Encoding.ASCII.GetString(data, pos, 4);
                int sectionSize = ReadInt32(data, pos + 4);
                if (sectionSize < 8 || pos + sectionSize > data.Length) break;
                int dataStart = pos + 8;
                int dataLen = sectionSize - 8;
                list.Add(new Chunk(id, pos, dataStart, dataLen));
                pos += Pad4(sectionSize);
            }
            return list;
        }

        static int Pad4(int n) => (n + 3) & ~3;

        static int ReadInt32(byte[] d, int o) =>
            (d[o] & 0xFF) | ((d[o + 1] & 0xFF) << 8) | ((d[o + 2] & 0xFF) << 16) | ((d[o + 3] & 0xFF) << 24);

        public static Chunk FindChunk(byte[] data, string fourCc)
        {
            foreach (var c in ReadChunks(data))
                if (c.Id == fourCc) return c;
            return default;
        }
    }
}
