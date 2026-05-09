using System;
using System.Collections.Generic;
using NewEditor.Forms;

namespace NewEditor.Data.Randomization.FvxGen5
{
    /// <summary>Decode/patch/recompress overlays using Y9 compression flags.</summary>
    internal static class FvxGen5OverlayIo
    {
        public static bool TryMutateOverlay(int overlayIndex, Action<byte[]> mutateDecoded, out string error)
        {
            error = null;
            var fs = MainEditor.fileSystem;
            if (fs?.overlays == null || overlayIndex < 0 || overlayIndex >= fs.overlays.Count)
            {
                error = "Overlay not loaded.";
                return false;
            }

            List<byte> raw = fs.overlays[overlayIndex];
            bool compressed = fs.y9 != null && overlayIndex < fs.y9.entries.Count && fs.y9.entries[overlayIndex].compressed;

            byte[] buf = compressed
                ? BLZDecoder.BLZ_DecodePub(raw.ToArray())
                : raw.ToArray();

            mutateDecoded(buf);

            fs.overlays[overlayIndex] = new List<byte>(compressed
                ? BLZDecoder.BLZ_EncodePub(buf, true)
                : buf);

            SyncY9EntryForOverlay(overlayIndex, fs.overlays[overlayIndex], compressed);
            return true;
        }

        /// <summary>
        /// Like <see cref="TryMutateOverlay(int, Action{byte[]}, out string)"/>, but if the callback returns a non-null message,
        /// the overlay is left unchanged (avoids a pointless BLZ re-encode round-trip on failure).
        /// </summary>
        public static bool TryMutateOverlay(int overlayIndex, Func<byte[], string> mutateDecoded, out string error)
        {
            error = null;
            var fs = MainEditor.fileSystem;
            if (fs?.overlays == null || overlayIndex < 0 || overlayIndex >= fs.overlays.Count)
            {
                error = "Overlay not loaded.";
                return false;
            }

            List<byte> raw = fs.overlays[overlayIndex];
            bool compressed = fs.y9 != null && overlayIndex < fs.y9.entries.Count && fs.y9.entries[overlayIndex].compressed;

            byte[] buf = compressed
                ? BLZDecoder.BLZ_DecodePub(raw.ToArray())
                : raw.ToArray();

            string stepErr = mutateDecoded(buf);
            if (!string.IsNullOrEmpty(stepErr))
            {
                error = stepErr;
                return false;
            }

            fs.overlays[overlayIndex] = new List<byte>(compressed
                ? BLZDecoder.BLZ_EncodePub(buf, true)
                : buf);

            SyncY9EntryForOverlay(overlayIndex, fs.overlays[overlayIndex], compressed);
            return true;
        }

        /// <summary>
        /// After BLZ recompression the file size often changes; if y9.compressedSize is stale the loader reads the wrong
        /// number of bytes and can hang on a white screen. Matches <see cref="FvxGen5StarterAssetPatcher"/> cry overlay handling.
        /// </summary>
        static void SyncY9EntryForOverlay(int overlayIndex, List<byte> newBytes, bool wasCompressed)
        {
            var fs = MainEditor.fileSystem;
            if (fs?.y9?.entries == null || overlayIndex < 0 || overlayIndex >= fs.y9.entries.Count || newBytes == null)
                return;
            var e = fs.y9.entries[overlayIndex];
            if (wasCompressed)
            {
                e.compressed = true;
                e.compressedSize = newBytes.Count;
            }
            else
                e.mountSize = newBytes.Count;
            e.Apply();
        }
    }
}
