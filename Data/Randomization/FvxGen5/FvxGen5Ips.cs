using System;
using System.Text;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Standard IPS (International Patching System) apply — compatible with UPR-bundled tweaks.</summary>
    internal static class FvxGen5Ips
    {
        public static bool TryApply(byte[] target, byte[] ips, StringBuilder log)
        {
            if (target == null || ips == null || ips.Length < 8)
            {
                log?.AppendLine("[IPS] Invalid patch or target buffer.");
                return false;
            }
            if (ips[0] != (byte)'P' || ips[1] != (byte)'A' || ips[2] != (byte)'T' || ips[3] != (byte)'C' || ips[4] != (byte)'H')
            {
                log?.AppendLine("[IPS] Not a PATCH IPS file.");
                return false;
            }

            int p = 5;
            while (p < ips.Length)
            {
                if (p + 3 <= ips.Length && ips[p] == (byte)'E' && ips[p + 1] == (byte)'O' && ips[p + 2] == (byte)'F')
                    return true;
                if (p + 5 > ips.Length)
                {
                    log?.AppendLine("[IPS] Truncated record.");
                    return false;
                }

                int off = (ips[p] << 16) | (ips[p + 1] << 8) | ips[p + 2];
                p += 3;
                int len = (ips[p] << 8) | ips[p + 1];
                p += 2;

                if (len == 0)
                {
                    if (p + 3 > ips.Length)
                    {
                        log?.AppendLine("[IPS] Truncated RLE record.");
                        return false;
                    }
                    int runlen = (ips[p] << 8) | ips[p + 1];
                    p += 2;
                    byte fill = ips[p++];
                    for (int i = 0; i < runlen; i++)
                    {
                        int d = off + i;
                        if ((uint)d < (uint)target.Length) target[d] = fill;
                    }
                }
                else
                {
                    for (int i = 0; i < len; i++)
                    {
                        if (p >= ips.Length)
                        {
                            log?.AppendLine("[IPS] Truncated data record.");
                            return false;
                        }
                        int d = off + i;
                        if ((uint)d < (uint)target.Length) target[d] = ips[p];
                        p++;
                    }
                }
            }

            return true;
        }
    }
}
