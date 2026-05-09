using System;
using System.Security.Cryptography;

namespace NewEditor.Data.Randomization.FvxGen5
{
    public static class FvxRandomizerRaceChecksum
    {
        /// <summary>Deterministic 32-bit value from ROM bytes (UPR race mode popup analogue).</summary>
        public static int ComputeFromRomBytes(byte[] romBytes)
        {
            if (romBytes == null || romBytes.Length == 0) return 0;
            using (var sha = SHA256.Create())
            {
                var h = sha.ComputeHash(romBytes);
                return BitConverter.ToInt32(h, 0);
            }
        }
    }
}
