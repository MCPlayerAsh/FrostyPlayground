using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using NewEditor.Forms;

namespace NewEditor.Data.DsNitro
{
    public static class TypeIconExporter
    {
        public const int Bw1TypeIconNarcIndex = 82;
        public const int Bw2TypeIconNarcIndex = 125;

        public static int GetTypeIconNarcIndex()
        {
            if (MainEditor.RomType == RomType.BW2) return Bw2TypeIconNarcIndex;
            return Bw1TypeIconNarcIndex;
        }

        static string GetMagic(byte[] d, int off = 0)
        {
            if (d == null || off + 4 > d.Length) return "";
            return Encoding.ASCII.GetString(d, off, 4);
        }

        static bool IsPaletteMagic(string m) => m == "NCLR" || m == "RLCN";
        static bool IsCharMagic(string m) => m == "NCGR" || m == "RGCN";

        public sealed class ExportResult
        {
            public int PngOk;
            public int PngFail;
            public int RawFiles;
            public List<string> Errors = new List<string>();
        }

        /// <summary>Export PNGs from type-icon NARC; pairs NCLR+NCGR when consecutive entries.</summary>
        public static ExportResult ExportPngs(string folder, bool useTypeNames)
        {
            var res = new ExportResult();
            if (MainEditor.fileSystem?.narcs == null)
            {
                res.Errors.Add("No ROM loaded.");
                return res;
            }

            int idx = GetTypeIconNarcIndex();
            if (idx < 0 || idx >= MainEditor.fileSystem.narcs.Count)
            {
                res.Errors.Add("Type icon NARC index out of range.");
                return res;
            }

            var narc = MainEditor.fileSystem.narcs[idx];
            int n = narc.numFileEntries;
            var names = GetTypeNames(useTypeNames, Math.Max(n, 32));
            Directory.CreateDirectory(folder);

            int slot = 0;
            for (int i = 0; i < n;)
            {
                var e0 = narc.GetFileEntry(i);
                if (e0 == null || e0.Count == 0) { i++; continue; }

                byte[] d0 = NitroLz.TryUnwrap(e0.ToArray());
                string m0 = GetMagic(d0);

                if (i + 1 < n && IsPaletteMagic(m0))
                {
                    var e1 = narc.GetFileEntry(i + 1);
                    if (e1 != null && e1.Count > 0)
                    {
                        byte[] d1 = NitroLz.TryUnwrap(e1.ToArray());
                        string m1 = GetMagic(d1);
                        if (IsCharMagic(m1) && NclrReader.TryReadPalette(d0, out var pal) && pal != null)
                        {
                            var bmp = NcgrReader.TryDecodeToBitmap(d1, pal);
                            if (bmp != null)
                            {
                                SavePng(folder, slot++, names, bmp, res);
                                bmp.Dispose();
                                i += 2;
                                continue;
                            }
                        }
                    }
                }

                if (IsCharMagic(m0))
                {
                    if (NclrReader.TryReadPalette(d0, out var palSelf) && palSelf != null)
                    {
                        var bmp = NcgrReader.TryDecodeToBitmap(d0, palSelf);
                        if (bmp != null)
                        {
                            SavePng(folder, slot++, names, bmp, res);
                            bmp.Dispose();
                            i++;
                            continue;
                        }
                    }
                    var bmpG = NcgrReader.TryDecodeToBitmap(d0, BuiltinGrayPalette());
                    if (bmpG != null)
                    {
                        SavePng(folder, slot++, names, bmpG, res);
                        bmpG.Dispose();
                        i++;
                        continue;
                    }
                }

                res.PngFail++;
                if (res.Errors.Count < 25)
                    res.Errors.Add("Entry " + i + " magic=" + m0 + " could not decode.");
                i++;
            }

            return res;
        }

        static Color[] BuiltinGrayPalette()
        {
            var p = new Color[16];
            for (int i = 0; i < 16; i++)
            {
                int g = i * 17;
                p[i] = Color.FromArgb(255, g, g, g);
            }
            p[0] = Color.FromArgb(0, 0, 0, 0);
            return p;
        }

        static void SavePng(string folder, int typeIndex, List<string> names, Bitmap bmp, ExportResult res)
        {
            try
            {
                string name = (names != null && typeIndex < names.Count && !string.IsNullOrEmpty(names[typeIndex]))
                    ? SanitizeFileName(names[typeIndex]) + ".png"
                    : "type_" + typeIndex.ToString("D2") + ".png";
                bmp.Save(Path.Combine(folder, name), ImageFormat.Png);
                res.PngOk++;
            }
            catch (Exception ex)
            {
                res.PngFail++;
                res.Errors.Add("Save failed: " + ex.Message);
            }
        }

        static string SanitizeFileName(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s.Trim();
        }

        static List<string> GetTypeNames(bool use, int max)
        {
            if (!use || MainEditor.textNarc == null) return null;
            try
            {
                var text = MainEditor.textNarc.textFiles[VersionConstants.TypeNameTextFileID].text;
                var list = new List<string>();
                for (int i = 0; i < max && i < text.Count; i++)
                    list.Add(text[i]);
                return list;
            }
            catch
            {
                return null;
            }
        }

        public static ExportResult ExportRaw(string folder, bool writeDecompressedSidecar)
        {
            var res = new ExportResult();
            if (MainEditor.fileSystem?.narcs == null)
            {
                res.Errors.Add("No ROM loaded.");
                return res;
            }

            int idx = GetTypeIconNarcIndex();
            if (idx < 0 || idx >= MainEditor.fileSystem.narcs.Count)
            {
                res.Errors.Add("Type icon NARC index out of range.");
                return res;
            }

            var narc = MainEditor.fileSystem.narcs[idx];
            Directory.CreateDirectory(folder);

            for (int i = 0; i < narc.numFileEntries; i++)
            {
                var entry = narc.GetFileEntry(i);
                if (entry == null || entry.Count == 0) continue;
                byte[] raw = entry.ToArray();

                string ext = ".bin";
                byte[] dec = NitroLz.TryUnwrap(raw);
                string magic = GetMagic(dec);
                if (magic == "NCGR" || magic == "RGCN") ext = ".rgcn";
                else if (magic == "NCLR" || magic == "RLCN") ext = ".rlcn";

                string baseName = "entry_" + i.ToString("D3");
                File.WriteAllBytes(Path.Combine(folder, baseName + "_raw.bin"), raw);
                res.RawFiles++;

                if (writeDecompressedSidecar && NitroLz.LooksLz(raw) && dec != null && dec.Length != raw.Length)
                {
                    File.WriteAllBytes(Path.Combine(folder, baseName + "_dec" + ext), dec);
                    res.RawFiles++;
                }
            }

            return res;
        }
    }
}
