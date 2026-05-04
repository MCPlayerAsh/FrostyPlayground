using NewEditor.Data;

namespace NewEditor.Data.DsNitro
{
    /// <summary>Unwraps LZ10/LZ11 Nitro-compressed blobs to raw resource bytes.</summary>
    public static class NitroLz
    {
        public static byte[] TryUnwrap(byte[] raw)
        {
            if (raw == null || raw.Length == 0) return raw;
            byte tag = (byte)(raw[0] & 0xFF);
            if (tag == 0x10 || tag == 0x11)
            {
                var dec = DsDecmp.Decompress(raw);
                return dec ?? raw;
            }
            return raw;
        }

        public static bool LooksLz(byte[] raw) =>
            raw != null && raw.Length > 0 && ((raw[0] & 0xFF) == 0x10 || (raw[0] & 0xFF) == 0x11);
    }
}
